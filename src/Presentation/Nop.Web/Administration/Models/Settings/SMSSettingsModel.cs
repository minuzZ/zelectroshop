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

        [NopResourceDisplayName("Admin.Configuration.Settings.SMS.NumberLength")]
        public int NumberLength { get; set; }
        public bool NumberLength_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.SMS.MessageTemplate")]
        public string MessageTemplate { get; set; }
        public bool MessageTemplate_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.SMS.ServiceId")]
        public string ServiceId { get; set; }
        public bool ServiceId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.SMS.ServiceKey")]
        public string ServiceKey { get; set; }
        public bool ServiceKey_OverrideForStore { get; set; }
    }
}