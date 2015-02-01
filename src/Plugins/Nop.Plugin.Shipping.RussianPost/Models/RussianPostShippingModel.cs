using Nop.Web.Framework;

namespace Nop.Plugin.Shipping.RussianPost.Models
{
    public class RussianPostShippingModel
    {
        [NopResourceDisplayName("Plugins.Shipping.RussianPost.Fields.GatewayUrl")]
        public string GatewayUrl { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.RussianPost.Fields.Site")]
        public string Site { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.RussianPost.Fields.Email")]
        public string Email { get; set; }
    }
}