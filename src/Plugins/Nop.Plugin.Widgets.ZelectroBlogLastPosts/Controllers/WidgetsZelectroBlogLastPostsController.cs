using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Plugin.Widgets.ZelectroBlogLastPosts.Infrastructure.Cache;
using Nop.Plugin.Widgets.ZelectroBlogLastPosts.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Widgets.ZelectroBlogLastPosts.Controllers
{
    public class WidgetsZelectroBlogLastPostsController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly ICacheManager _cacheManager;
        private readonly ILocalizationService _localizationService;

        public WidgetsZelectroBlogLastPostsController(IWorkContext workContext,
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

        protected string GetPictureUrl(int pictureId)
        {
            string cacheKey = string.Format(ModelCacheEventConsumer.PICTURE_URL_MODEL_KEY, pictureId);
            return _cacheManager.Get(cacheKey, () =>
            {
                var url = _pictureService.GetPictureUrl(pictureId, showDefaultPicture: false);
                //little hack here. nulls aren't cacheable so set it to ""
                if (url == null)
                    url = "";

                return url;
            });
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var zelectroBlogLastPostsSettings = _settingService.LoadSetting<ZelectroBlogLastPostsSettings>(storeScope);
            var model = new ConfigurationModel();
            model.Picture1Id = zelectroBlogLastPostsSettings.Picture1Id;
            model.Text1 = zelectroBlogLastPostsSettings.Text1;
            model.Link1 = zelectroBlogLastPostsSettings.Link1;
            model.Header1 = zelectroBlogLastPostsSettings.Header1;
            model.Picture2Id = zelectroBlogLastPostsSettings.Picture2Id;
            model.Text2 = zelectroBlogLastPostsSettings.Text2;
            model.Link2 = zelectroBlogLastPostsSettings.Link2;
            model.Header2 = zelectroBlogLastPostsSettings.Header2;
            model.Picture3Id = zelectroBlogLastPostsSettings.Picture3Id;
            model.Text3 = zelectroBlogLastPostsSettings.Text3;
            model.Link3 = zelectroBlogLastPostsSettings.Link3;
            model.Header3 = zelectroBlogLastPostsSettings.Header3;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.Picture1Id_OverrideForStore = _settingService.SettingExists(zelectroBlogLastPostsSettings, x => x.Picture1Id, storeScope);
                model.Text1_OverrideForStore = _settingService.SettingExists(zelectroBlogLastPostsSettings, x => x.Text1, storeScope);
                model.Link1_OverrideForStore = _settingService.SettingExists(zelectroBlogLastPostsSettings, x => x.Link1, storeScope);
                model.Header1_OverrideForStore = _settingService.SettingExists(zelectroBlogLastPostsSettings, x => x.Header1, storeScope);
                model.Picture2Id_OverrideForStore = _settingService.SettingExists(zelectroBlogLastPostsSettings, x => x.Picture2Id, storeScope);
                model.Text2_OverrideForStore = _settingService.SettingExists(zelectroBlogLastPostsSettings, x => x.Text2, storeScope);
                model.Link2_OverrideForStore = _settingService.SettingExists(zelectroBlogLastPostsSettings, x => x.Link2, storeScope);
                model.Header2_OverrideForStore = _settingService.SettingExists(zelectroBlogLastPostsSettings, x => x.Header2, storeScope);
                model.Picture3Id_OverrideForStore = _settingService.SettingExists(zelectroBlogLastPostsSettings, x => x.Picture3Id, storeScope);
                model.Text3_OverrideForStore = _settingService.SettingExists(zelectroBlogLastPostsSettings, x => x.Text3, storeScope);
                model.Link3_OverrideForStore = _settingService.SettingExists(zelectroBlogLastPostsSettings, x => x.Link3, storeScope);
                model.Header3_OverrideForStore = _settingService.SettingExists(zelectroBlogLastPostsSettings, x => x.Header3, storeScope);
            }

            return View("~/Plugins/Widgets.ZelectroBlogLastPosts/Views/WidgetsZelectroBlogLastPosts/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var zelectroBlogLastPostsSettings = _settingService.LoadSetting<ZelectroBlogLastPostsSettings>(storeScope);
            zelectroBlogLastPostsSettings.Picture1Id = model.Picture1Id;
            zelectroBlogLastPostsSettings.Text1 = model.Text1;
            zelectroBlogLastPostsSettings.Link1 = model.Link1;
            zelectroBlogLastPostsSettings.Header1 = model.Header1;
            zelectroBlogLastPostsSettings.Picture2Id = model.Picture2Id;
            zelectroBlogLastPostsSettings.Text2 = model.Text2;
            zelectroBlogLastPostsSettings.Link2 = model.Link2;
            zelectroBlogLastPostsSettings.Header2 = model.Header2;
            zelectroBlogLastPostsSettings.Picture3Id = model.Picture3Id;
            zelectroBlogLastPostsSettings.Text3 = model.Text3;
            zelectroBlogLastPostsSettings.Link3 = model.Link3;
            zelectroBlogLastPostsSettings.Header3 = model.Header3;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.Picture1Id_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(zelectroBlogLastPostsSettings, x => x.Picture1Id, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(zelectroBlogLastPostsSettings, x => x.Picture1Id, storeScope);
            
            if (model.Text1_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(zelectroBlogLastPostsSettings, x => x.Text1, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(zelectroBlogLastPostsSettings, x => x.Text1, storeScope);
            
            if (model.Link1_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(zelectroBlogLastPostsSettings, x => x.Link1, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(zelectroBlogLastPostsSettings, x => x.Link1, storeScope);

            if (model.Header1_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(zelectroBlogLastPostsSettings, x => x.Header1, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(zelectroBlogLastPostsSettings, x => x.Header1, storeScope);
            
            if (model.Picture2Id_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(zelectroBlogLastPostsSettings, x => x.Picture2Id, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(zelectroBlogLastPostsSettings, x => x.Picture2Id, storeScope);
            
            if (model.Text2_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(zelectroBlogLastPostsSettings, x => x.Text2, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(zelectroBlogLastPostsSettings, x => x.Text2, storeScope);
            
            if (model.Link2_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(zelectroBlogLastPostsSettings, x => x.Link2, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(zelectroBlogLastPostsSettings, x => x.Link2, storeScope);

            if (model.Header2_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(zelectroBlogLastPostsSettings, x => x.Header2, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(zelectroBlogLastPostsSettings, x => x.Header2, storeScope);
            
            if (model.Picture3Id_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(zelectroBlogLastPostsSettings, x => x.Picture3Id, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(zelectroBlogLastPostsSettings, x => x.Picture3Id, storeScope);
            
            if (model.Text3_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(zelectroBlogLastPostsSettings, x => x.Text3, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(zelectroBlogLastPostsSettings, x => x.Text3, storeScope);
            
            if (model.Link3_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(zelectroBlogLastPostsSettings, x => x.Link3, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(zelectroBlogLastPostsSettings, x => x.Link3, storeScope);

            if (model.Header3_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(zelectroBlogLastPostsSettings, x => x.Header3, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(zelectroBlogLastPostsSettings, x => x.Header3, storeScope);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PublicInfo(string widgetZone, object additionalData = null)
        {
            var zelectroBlogLastPostsSettings = _settingService.LoadSetting<ZelectroBlogLastPostsSettings>(_storeContext.CurrentStore.Id);

            var model = new PublicInfoModel();
            model.Picture1Url = GetPictureUrl(zelectroBlogLastPostsSettings.Picture1Id);
            model.Text1 = zelectroBlogLastPostsSettings.Text1;
            model.Link1 = zelectroBlogLastPostsSettings.Link1;
            model.Header1 = zelectroBlogLastPostsSettings.Header1;

            model.Picture2Url = GetPictureUrl(zelectroBlogLastPostsSettings.Picture2Id);
            model.Text2 = zelectroBlogLastPostsSettings.Text2;
            model.Link2 = zelectroBlogLastPostsSettings.Link2;
            model.Header2 = zelectroBlogLastPostsSettings.Header2;

            model.Picture3Url = GetPictureUrl(zelectroBlogLastPostsSettings.Picture3Id);
            model.Text3 = zelectroBlogLastPostsSettings.Text3;
            model.Link3 = zelectroBlogLastPostsSettings.Link3;
            model.Header3 = zelectroBlogLastPostsSettings.Header3;

            if (string.IsNullOrEmpty(model.Picture1Url) && string.IsNullOrEmpty(model.Picture2Url) &&
                string.IsNullOrEmpty(model.Picture3Url))
                //no pictures uploaded
                return Content("");


            return View("~/Plugins/Widgets.ZelectroBlogLastPosts/Views/WidgetsZelectroBlogLastPosts/PublicInfo.cshtml", model);
        }
    }
}