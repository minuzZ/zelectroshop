using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Widgets.ZelectroBlogLastPosts.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }


        [NopResourceDisplayName("Plugins.Widgets.ZelectroBlogLastPosts.Picture")]
        [UIHint("Picture")]
        public int Picture1Id { get; set; }
        public bool Picture1Id_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.ZelectroBlogLastPosts.Text")]
        [AllowHtml]
        public string Text1 { get; set; }
        public bool Text1_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.ZelectroBlogLastPosts.Link")]
        [AllowHtml]
        public string Link1 { get; set; }
        public bool Link1_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.ZelectroBlogLastPosts.Header")]
        [AllowHtml]
        public string Header1 { get; set; }
        public bool Header1_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture2Id { get; set; }
        public bool Picture2Id_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Text")]
        [AllowHtml]
        public string Text2 { get; set; }
        public bool Text2_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Link")]
        [AllowHtml]
        public string Link2 { get; set; }
        public bool Link2_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.ZelectroBlogLastPosts.Header")]
        [AllowHtml]
        public string Header2 { get; set; }
        public bool Header2_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Picture")]
        [UIHint("Picture")]
        public int Picture3Id { get; set; }
        public bool Picture3Id_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Text")]
        [AllowHtml]
        public string Text3 { get; set; }
        public bool Text3_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.NivoSlider.Link")]
        [AllowHtml]
        public string Link3 { get; set; }
        public bool Link3_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.ZelectroBlogLastPosts.Header")]
        [AllowHtml]
        public string Header3 { get; set; }
        public bool Header3_OverrideForStore { get; set; }
    }
}