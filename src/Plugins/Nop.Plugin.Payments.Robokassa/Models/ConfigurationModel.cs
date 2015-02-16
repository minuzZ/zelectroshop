using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.Web.Mvc;
using System.Web;

namespace Nop.Plugin.Payments.Robokassa.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payment.Robokassa.Login")]
        public string Login { get; set; }
        public bool Login_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payment.Robokassa.Password1")]
        public string Password1 { get; set; }
        public bool Password1_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payment.Robokassa.Password2")]
        public string Password2 { get; set; }
        public bool Password2_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payment.Robokassa.PaymentDescription")]
        public string PaymentDescription { get; set; }
        public bool PaymentDescription_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payment.Robokassa.ApiURL")]
        public string ApiURL { get; set; }
        public bool ApiURL_OverrideForStore { get; set; }

        public HttpContextBase HttpContext { get; set; }
    }
}
