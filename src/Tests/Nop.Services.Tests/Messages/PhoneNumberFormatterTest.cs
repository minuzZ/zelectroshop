using Nop.Core.Domain.SMS;
using Nop.Services.Messages;
using Nop.Tests;
using NUnit.Framework;

namespace Nop.Services.Tests.Messages
{
    [TestFixture]
    public class PhoneNumberFormatterTest
    {
        private SMSSettings settings;
        private IPhoneNumberFormatter formatter;

        [SetUp]
        public void SetUp()
        {
            settings = new SMSSettings();
            settings.CountryCode = "+7";
            settings.NumberLength = 10;
            formatter = new PhoneNumberFormatter(settings);
        }
        /// <summary>
        /// Verifies that formatter will get last N numbers and add country code
        /// </summary>
        [Test]
        public void FormatNumberTest()
        {
            formatter.FormatNumber("9788111703").ShouldEqual<string>("+79788111703");
            formatter.FormatNumber("+7(978) 8111 703").ShouldEqual<string>("+79788111703");
            formatter.FormatNumber("+79788111703").ShouldEqual<string>("+79788111703");
        }
    }
}
