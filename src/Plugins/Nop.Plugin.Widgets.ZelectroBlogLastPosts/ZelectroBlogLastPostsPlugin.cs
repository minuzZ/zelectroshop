using System.Collections.Generic;
using System.IO;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;

namespace Nop.Plugin.Widgets.ZelectroBlogLastPosts
{
    /// <summary>
    /// PLugin
    /// </summary>
    public class ZelectroBlogLastPostsPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        public ZelectroBlogLastPostsPlugin(IPictureService pictureService,
            ISettingService settingService, IWebHelper webHelper)
        {
            this._pictureService = pictureService;
            this._settingService = settingService;
            this._webHelper = webHelper;
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return new List<string>() { "home_page_top" };
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "WidgetsZelectroBlogLastPosts";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Widgets.ZelectroBlogLastPosts.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for displaying widget
        /// </summary>
        /// <param name="widgetZone">Widget zone where it's displayed</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PublicInfo";
            controllerName = "WidgetsZelectroBlogLastPosts";
            routeValues = new RouteValueDictionary()
            {
                {"Namespaces", "Nop.Plugin.Widgets.ZelectroBlogLastPosts.Controllers"},
                {"area", null},
                {"widgetZone", widgetZone}
            };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //pictures
            var sampleImagesPath = _webHelper.MapPath("~/Plugins/Widgets.ZelectroBlogLastPosts/Content/sample-images/");


            //settings
            var settings = new ZelectroBlogLastPostsSettings()
            {
                Picture1Id = _pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "img1.jpg"), "image/pjpeg", "img1", true).Id,
                Text1 = "Если быть точным, то \"водометик\" является не модернизацией, а надстройкой поверх дистанционно управляемого танка. Управляющая программа, способы дистанционного управления (bluetooth или APC220)...",
                Link1 = "http://zelectro.cc/Z-Mini_Motor_Sensor_Shield_Prototype",
                Header1 = "Водометик - модернизация дистанционно управляемого танка на Arduino",
                Picture2Id = _pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "img2.jpg"), "image/pjpeg", "img2", true).Id,
                Text2 = "Если быть точным, то \"водометик\" является не модернизацией, а надстройкой поверх дистанционно управляемого танка. Управляющая программа, способы дистанционного управления (bluetooth или APC220)...",
                Link2 = "http://zelectro.cc/Z-Mini_Motor_Sensor_Shield_Prototype",
                Header2 = "Водометик - модернизация дистанционно управляемого танка на Arduino",
                Picture3Id = _pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "img3.jpg"), "image/pjpeg", "img3", true).Id,
                Text3 = "Если быть точным, то \"водометик\" является не модернизацией, а надстройкой поверх дистанционно управляемого танка. Управляющая программа, способы дистанционного управления (bluetooth или APC220)...",
                Link3 = "http://zelectro.cc/Z-Mini_Motor_Sensor_Shield_Prototype",
                Header3 = "Водометик - модернизация дистанционно управляемого танка на Arduino",
            };
            _settingService.SaveSetting(settings);

            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.ZelectroBlogLastPosts.Picture", "Картинка");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.ZelectroBlogLastPosts.Picture.Hint", "Загрузить картинку.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.ZelectroBlogLastPosts.Text", "Краткое описание");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.ZelectroBlogLastPosts.Text.Hint", "Введите краткое описание поста");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.ZelectroBlogLastPosts.Link", "Ссылка на пост");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.ZelectroBlogLastPosts.Link.Hint", "Введите ссылку на пост");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.ZelectroBlogLastPosts.Header", "Заголовок поста");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.ZelectroBlogLastPosts.Header.Hint", "Введите заголовок поста");

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<ZelectroBlogLastPostsSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Widgets.ZelectroBlogLastPosts.Picture");
            this.DeletePluginLocaleResource("Plugins.Widgets.ZelectroBlogLastPosts.Picture.Hint");
            this.DeletePluginLocaleResource("Plugins.Widgets.ZelectroBlogLastPosts.Text");
            this.DeletePluginLocaleResource("Plugins.Widgets.ZelectroBlogLastPosts.Text.Hint");
            this.DeletePluginLocaleResource("Plugins.Widgets.ZelectroBlogLastPosts.Link");
            this.DeletePluginLocaleResource("Plugins.Widgets.ZelectroBlogLastPosts.Link.Hint");
            this.DeletePluginLocaleResource("Plugins.Widgets.ZelectroBlogLastPosts.Header");
            this.DeletePluginLocaleResource("Plugins.Widgets.ZelectroBlogLastPosts.Header.Hint");

            base.Uninstall();
        }
    }
}
