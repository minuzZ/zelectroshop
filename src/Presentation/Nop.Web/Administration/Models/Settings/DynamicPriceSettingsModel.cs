using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Settings
{
    public partial class DynamicPriceSettingsModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.DynamicPrice.EuroRate")]
        public decimal EuroRate { get; set; }
        public bool EuroRate_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.DynamicPrice.DollarRate")]
        public decimal DollarRate { get; set; }
        public bool DollarRate_OverrideForStore { get; set; }
    }
}