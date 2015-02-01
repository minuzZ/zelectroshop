using System.Web.Mvc;
using Nop.Plugin.Shipping.RussianPost.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Shipping.RussianPost.Controllers
{
    [AdminAuthorize]
    public class ShippingRussianPostController : BasePluginController
    {
        private readonly RussianPostSettings _RussianPostSettings;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

        public ShippingRussianPostController(RussianPostSettings RussianPostSettings,
            ISettingService settingService,
            ILocalizationService localizationService)
        {
            this._RussianPostSettings = RussianPostSettings;
            this._settingService = settingService;
            this._localizationService = localizationService;
        }

        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new RussianPostShippingModel();
            model.GatewayUrl = _RussianPostSettings.GatewayUrl;
            model.Site = _RussianPostSettings.Site;
            model.Email = _RussianPostSettings.Email;

            return View("~/Plugins/Shipping.RussianPost/Views/ShippingRussianPost/Configure.cshtml", model);
        }

        [HttpPost]
        [ChildActionOnly]
        public ActionResult Configure(RussianPostShippingModel model)
        {
            if (!ModelState.IsValid)
            {
                return Configure();
            }
            
            //save settings
            _RussianPostSettings.GatewayUrl = model.GatewayUrl;
            _RussianPostSettings.Site = model.Site;
            _RussianPostSettings.Email = model.Email;
            _settingService.SaveSetting(_RussianPostSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

    }
}
