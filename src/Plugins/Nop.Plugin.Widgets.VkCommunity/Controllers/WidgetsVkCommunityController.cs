using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Plugin.Widgets.VkCommunity.Infrastructure.Cache;
using Nop.Plugin.Widgets.VkCommunity.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Widgets.VkCommunity.Controllers
{
    public class WidgetsVkCommunityController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly ICacheManager _cacheManager;
        private readonly ILocalizationService _localizationService;

        public WidgetsVkCommunityController(IWorkContext workContext,
            IStoreContext storeContext,
            IStoreService storeService, 
            IPictureService pictureService,
            ISettingService settingService,
            ICacheManager cacheManager,
            ILocalizationService localizationService)
        {
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._pictureService = pictureService;
            this._settingService = settingService;
            this._cacheManager = cacheManager;
            this._localizationService = localizationService;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var vkCommunitySettings = _settingService.LoadSetting<VkCommunitySettings>(storeScope);
            var model = new ConfigurationModel();
            model.VkCode = vkCommunitySettings.VkCode;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.VkCode_OverrideForStore = _settingService.SettingExists(vkCommunitySettings, x => x.VkCode, storeScope);
            }

            return View("~/Plugins/Widgets.VkCommunity/Views/WidgetsVkCommunity/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var vkCommunitySettings = _settingService.LoadSetting<VkCommunitySettings>(storeScope);
            vkCommunitySettings.VkCode = model.VkCode;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.VkCode_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(vkCommunitySettings, x => x.VkCode, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(vkCommunitySettings, x => x.VkCode, storeScope);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PublicInfo(string widgetZone, object additionalData = null)
        {
            var vkCommunitySettings = _settingService.LoadSetting<VkCommunitySettings>(_storeContext.CurrentStore.Id);

            var model = new PublicInfoModel();
            model.VkCode = vkCommunitySettings.VkCode;

            return View("~/Plugins/Widgets.VkCommunity/Views/WidgetsVkCommunity/PublicInfo.cshtml", model);
        }
    }
}