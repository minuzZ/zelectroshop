using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Widgets.VkCommunity.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }


        [NopResourceDisplayName("Plugins.Widgets.VkCommunity.Code")]
        [AllowHtml]
        public string VkCode { get; set; }
        public bool VkCode_OverrideForStore { get; set; }
    }
}