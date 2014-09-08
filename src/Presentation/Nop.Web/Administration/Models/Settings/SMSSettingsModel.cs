using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Settings
{
    public partial class SMSSettingsModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.SMS.Sender")]
        public string From { get; set; }
        public bool From_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.SMS.CountryCode")]
        public string CountryCode { get; set; }
        public bool CountryCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.SMS.MessageTemplate")]
        public string MessageTemplate { get; set; }
        public bool MessageTemplate_OverrideForStore { get; set; }
    }
}