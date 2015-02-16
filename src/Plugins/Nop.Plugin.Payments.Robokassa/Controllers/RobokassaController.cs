using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.Robokassa.Models;
using Nop.Services.Payments;
using System.Collections;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Logging;
using System.Web;
using Nop.Services.Configuration;

namespace Nop.Plugin.Payments.Robokassa.Controllers
{
    public class RobokassaController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        HttpContextBase _httpContext;
        IOrderService _orders;
        ILogger _log;

        public RobokassaController(IWorkContext workContext,
            IOrderService orders,
            IStoreService storeService,
            ILogger log, 
            IStoreContext storeContext,
            ISettingService settingService,
            HttpContextBase httpContext)
        {
            this._orders = orders;
            this._log = log;
            this._storeContext = storeContext;
            this._settingService = settingService;
            this._httpContext = httpContext;
            this._workContext = workContext;
            this._storeService = storeService;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var robokassaSettings = _settingService.LoadSetting<RobokassaSettings>(storeScope);

            var model = new ConfigurationModel();
            model.HttpContext = _httpContext;
            model.ApiURL = robokassaSettings.ApiURL;
            model.Login = robokassaSettings.Login;
            model.Password1 = robokassaSettings.Password1;
            model.Password2 = robokassaSettings.Password2;
            model.PaymentDescription = robokassaSettings.PaymentDescription;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.ApiURL_OverrideForStore = _settingService.SettingExists(robokassaSettings, x => x.ApiURL, storeScope);
                model.Login_OverrideForStore = _settingService.SettingExists(robokassaSettings, x => x.Login, storeScope);
                model.Password1_OverrideForStore = _settingService.SettingExists(robokassaSettings, x => x.Password1, storeScope);
                model.Password2_OverrideForStore = _settingService.SettingExists(robokassaSettings, x => x.Password2, storeScope);
                model.PaymentDescription_OverrideForStore = _settingService.SettingExists(robokassaSettings, x => x.PaymentDescription, storeScope);
            }

            return View("~/Plugins/Payments.Robokassa/Views/PaymentRobokassa/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            model.HttpContext = _httpContext;
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var robokassaSettings = _settingService.LoadSetting<RobokassaSettings>(storeScope);

            robokassaSettings.ApiURL = model.ApiURL;
            robokassaSettings.Login = model.Login;
            robokassaSettings.Password1 = model.Password1;
            robokassaSettings.Password2 = model.Password2;
            robokassaSettings.PaymentDescription = model.PaymentDescription;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.ApiURL_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(robokassaSettings, x => x.ApiURL, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(robokassaSettings, x => x.ApiURL, storeScope);

            if (model.Login_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(robokassaSettings, x => x.Login, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(robokassaSettings, x => x.Login, storeScope);

            if (model.Password1_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(robokassaSettings, x => x.Password1, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(robokassaSettings, x => x.Password1, storeScope);

            if (model.Password2_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(robokassaSettings, x => x.Password2, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(robokassaSettings, x => x.Password2, storeScope);

            if (model.PaymentDescription_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(robokassaSettings, x => x.PaymentDescription, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(robokassaSettings, x => x.PaymentDescription, storeScope);

            //now clear settings cache
            _settingService.ClearCache();

            return Configure();
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }
        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("~/Plugins/Payments.Robokassa/Views/PaymentRobokassa/PaymentInfo.cshtml");
        }

        [HttpGet]
        public ActionResult Result(string OutSum, string InvId, string SignatureValue)
        {
            if (!String.IsNullOrEmpty(OutSum)&&!String.IsNullOrEmpty(InvId)&&!String.IsNullOrEmpty(SignatureValue)){

                //load settings for a chosen store scope
                var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
                var robokassaSettings = _settingService.LoadSetting<RobokassaSettings>(storeScope);
                string merchantpass2 = robokassaSettings.Password2;

                //check md5:
                string computedmd5 = MD5Helper.GetMD5Hash(String.Format("{0}:{1}:{2}",OutSum,InvId,merchantpass2));
                if (computedmd5.ToUpper() != SignatureValue.ToUpper()) return Content("Error");

                int orderid = Convert.ToInt32(InvId);
                var order = _orders.GetOrderById(orderid);

                if (order.OrderGuid != Guid.Empty){
                    decimal sum = Convert.ToDecimal(OutSum.Replace(".",","));
                    if (order.OrderTotal <= sum){ //если оплачен полностью (или больше), то отметить как оплаченный
                        order.PaymentStatus = Core.Domain.Payments.PaymentStatus.Paid;
                        _orders.UpdateOrder(order);
                    }
                }
                return Content(string.Format("OK{0}",InvId));
            }
            return Content("Error");
        }

        [HttpGet]
        public ActionResult Fail(string OutSum, string InvId)
        {
            var model = new Fail();
            if (!String.IsNullOrEmpty(OutSum) && !String.IsNullOrEmpty(InvId))
            {
                model.orderid = InvId;
                model.ordersum = OutSum;
            }
            else
            {
                model.orderid = "0";
                model.ordersum = "0";
            }
            return View("~/Plugins/Payments.Robokassa/Views/PaymentRobokassa/Fail.cshtml", model);
        }

        [HttpGet]
        public ActionResult Success(string OutSum, string InvId, string SignatureValue, string Culture)
        {
            if (!String.IsNullOrEmpty(OutSum) && !String.IsNullOrEmpty(InvId) && !String.IsNullOrEmpty(SignatureValue))
            {
                //load settings for a chosen store scope
                var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
                var robokassaSettings = _settingService.LoadSetting<RobokassaSettings>(storeScope);
                string merchantpass = robokassaSettings.Password1;

                //check md5:
                string computedmd5 = MD5Helper.GetMD5Hash(String.Format("{0}:{1}:{2}", OutSum, InvId, merchantpass));
                if (computedmd5.ToUpper() != SignatureValue.ToUpper())
                {
                    _log.InsertLog(logLevel: Core.Domain.Logging.LogLevel.Error, shortMessage: "Неправильный переход на success", fullMessage: String.Format("OutSum:{0}, InvId:{1}, SignatureValue:{2}", OutSum, InvId, SignatureValue));
                    return Content("Ошибка: От Робокассы получены неверные параметры.\nОшибка сохранена в лог сервера.");
                }

                return RedirectToRoute("CheckoutCompleted", new { orderId = InvId });
            }
            return Content("Ошибка: не указаны параметры при переходе.");
        }

    }
}
