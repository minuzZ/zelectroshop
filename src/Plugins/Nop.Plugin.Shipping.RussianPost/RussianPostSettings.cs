
using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.RussianPost
{
    public class RussianPostSettings : ISettings
    {
        public string GatewayUrl { get; set; }

        public string Site { get; set; }

        public string Email { get; set; }
    }
}