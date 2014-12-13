using Nop.Core.Configuration;

namespace Nop.Core.Domain.DynamicPrice
{
    /// <summary>
    /// Dynamic Prices settings
    /// </summary>
    public class DynamicPriceSettings : ISettings
    {
        /// <summary>
        /// Gets or sets euro exchange rate
        /// </summary>
        public decimal EuroRate { get; set; }

        /// <summary>
        /// Gets or sets dollar exchange rate
        /// </summary>
        public decimal DollarRate { get; set; }
    }
}
