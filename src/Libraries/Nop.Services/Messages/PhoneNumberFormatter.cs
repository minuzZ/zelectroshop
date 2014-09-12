using System;
using Nop.Core.Domain.SMS;

namespace Nop.Services.Messages
{
    public class PhoneNumberFormatter : IPhoneNumberFormatter
    {
        SMSSettings _smsSettings;

        public PhoneNumberFormatter(SMSSettings smsSettings)
        {
            this._smsSettings = smsSettings;
        }

        /// <summary>
        /// Gets formatted number according to settings
        /// Admin.Configuration.Settings.SMS.CountryCode with 
        /// last Admin.Configuration.Settings.SMS.NumberLength numbers from input number
        /// </summary>
        /// <param name="number">Not formatted number</param>
        /// <returns></returns>
        public string FormatNumber(string number)
        {
            string phoneNum = String.Empty;
            int numLength = _smsSettings.NumberLength;

            if (number.Length >= numLength)
            {
                for (int i = number.Length - 1; i >= 0 && numLength > 0; --i)
                {
                    if (Char.IsDigit(number[i]))
                    {
                        phoneNum += number[i];
                        numLength--;
                    }
                }
            }

            if (numLength == 0)
            {
                char[] charArray = phoneNum.ToCharArray();
                Array.Reverse(charArray);
                phoneNum = _smsSettings.CountryCode + new String(charArray);
            }

            return phoneNum;
        }
    }
}
