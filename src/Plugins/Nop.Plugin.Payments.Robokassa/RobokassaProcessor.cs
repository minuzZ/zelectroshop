using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Plugins;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using System.Web;
using System.Web.Routing;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Robokassa.Controllers;
using Nop.Core.Domain.Payments;
using System.Security.Cryptography;

namespace Nop.Plugin.Payments.Robokassa
{

    public class Robokassa : BasePlugin, IPaymentMethod
    {
        private readonly ISettingService _settingService;
        private readonly HttpContextBase _httpContext;
        private readonly RobokassaSettings _robokassaSettings;

        public Robokassa(ISettingService settingService, 
            HttpContextBase httpContext,
            RobokassaSettings robokassaSettings
            )
        {
            this._settingService = settingService;
            this._httpContext = httpContext;
            this._robokassaSettings = robokassaSettings;
        }


        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "Robokassa";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Robokassa.Controllers" }, { "area", null } };
        }
        public override void Install()
        {
            var settings = new RobokassaSettings() { };
            _settingService.SaveSetting(settings);

            this.AddOrUpdatePluginLocaleResource("Plugins.Payment.Robokassa.Login", "Login");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payment.Robokassa.Login.Hint", "Enter login");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payment.Robokassa.Password1", "Password1");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payment.Robokassa.Password1.Hint", "Enter password 1");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payment.Robokassa.Password2", "Password2");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payment.Robokassa.Password2.Hint", "Enter password 2");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payment.Robokassa.PaymentDescription", "Payment Description");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payment.Robokassa.PaymentDescription.Hint", "Enter payment description");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payment.Robokassa.ApiURL", "Api URL");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payment.Robokassa.ApiURL.Hint", "Enter Api URL");
            base.Install();
        }
        public override void Uninstall()
        {
            _settingService.DeleteSetting<RobokassaSettings>();
            
            this.DeletePluginLocaleResource("Plugins.Payment.Robokassa.Login");
            this.DeletePluginLocaleResource("Plugins.Payment.Robokassa.Login.Hint");
            this.DeletePluginLocaleResource("Plugins.Payment.Robokassa.Password1");
            this.DeletePluginLocaleResource("Plugins.Payment.Robokassa.Password1.Hint");
            this.DeletePluginLocaleResource("Plugins.Payment.Robokassa.Password2");
            this.DeletePluginLocaleResource("Plugins.Payment.Robokassa.Password2.Hint");
            this.DeletePluginLocaleResource("Plugins.Payment.Robokassa.PaymentDescription");
            this.DeletePluginLocaleResource("Plugins.Payment.Robokassa.PaymentDescription.Hint");
            this.DeletePluginLocaleResource("Plugins.Payment.Robokassa.ApiURL");
            this.DeletePluginLocaleResource("Plugins.Payment.Robokassa.ApiURL.Hint");

            base.Uninstall();
        }

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Ежемесячная оплата не поддерживается.");
            return result;
        }
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //удостоверимся, что прошло не меньше минуты после перехода к оплате
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1)
                return false;

            return true;
        }
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Разрешение оплаты не поддерживается.");
            return result;
        }
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return 0; //без дополнительной оплаты
        }
        public Type GetControllerType()
        {
            return typeof(RobokassaController);
        }
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "Robokassa";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Robokassa.Controllers" }, { "area", null } };
        }

        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var builder = new StringBuilder();
            string merchant = _robokassaSettings.Login;
            string merchantpass = _robokassaSettings.Password1;
            string desc = _robokassaSettings.PaymentDescription;
            string signature = String.Format("{0}:{1}:{2}:{3}", merchant, postProcessPaymentRequest.Order.OrderTotal.ToString().Replace(",","."), postProcessPaymentRequest.Order.Id, merchantpass);
            string currency = "rub";
            string culture = "ru";

            builder.Append(_robokassaSettings.ApiURL);
            builder.AppendFormat("?MrchLogin={0}", merchant);
            builder.AppendFormat("&OutSum={0}", postProcessPaymentRequest.Order.OrderTotal.ToString().Replace(",", "."));
            builder.AppendFormat("&InvId={0}", postProcessPaymentRequest.Order.Id);
            builder.AppendFormat("&Desc={0}", HttpUtility.UrlEncode(desc));
            builder.AppendFormat("&SignatureValue={0}", MD5Helper.GetMD5Hash(signature));
            builder.AppendFormat("&sIncCurrLabel={0}", currency);
            builder.AppendFormat("&Email={0}", postProcessPaymentRequest.Order.BillingAddress.Email);
            builder.AppendFormat("&sCulture={0}", culture);

            _httpContext.Response.Redirect(builder.ToString());
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Ежемесячная оплата не поддерживается.");
            return result;
        }

        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Возврат денег не поддерживается.");
            return result;
        }

        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Метод недействительности не поддерживается.");
            return result;
        }

        public bool SkipPaymentInfo
        {
            get
            {
                return false;
            }
        }

    }

    public class MD5Helper
    {
        public static string GetMD5Hash(string input)
        {
            MD5 md5Hash = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }

}
