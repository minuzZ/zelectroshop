﻿using Nop.Core.Configuration;

namespace Nop.Core.Domain.SMS
{
    /// <summary>
    /// SMS settings
    /// </summary>
    public class SMSSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the SMS sender
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Gets or sets receivers country code
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets a message text template
        /// </summary>
        public string MessageTemplate { get; set; }
    }
}
