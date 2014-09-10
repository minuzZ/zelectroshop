using System;
using System.Net;
using Nop.Core.Domain.SMS;

namespace Nop.Services.Messages
{
    //TODO:
    //Temporary ISMSSender realization. Later hardcoded values should be moved to settings.
    public class BytehandSMSSender : ISMSSender
    {
        private readonly SMSSettings _smsSettings;
        private readonly IPhoneNumberFormatter _numberFormatter;

        public BytehandSMSSender(SMSSettings smsSettings, IPhoneNumberFormatter numberFormatter)
        {
            this._smsSettings = smsSettings;
            this._numberFormatter = numberFormatter;
        }

        public void SendSMS(string from, string number, string text)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest
                .Create(makeBytehandGetRequestURL(_smsSettings.ServiceId, _smsSettings.ServiceKey,
                    _numberFormatter.FormatNumber(number), from, text));

            WebResponse response = null;
            try
            {
                response = request.GetResponse();
            }
            finally
            {
                if (response != null)
                    response.Dispose();
            }
        }

        //TODO:
        //Remove hardcoded values
        private string makeBytehandGetRequestURL(string bytehandId, string bytehandKey, 
            string number, string from, string text)
        {
            return "http://bytehand.com:3800/send?id=" + bytehandId + "&key=" + bytehandKey + "&to=" + 
                Uri.EscapeUriString(number) + "&from=" + Uri.EscapeUriString(from) + "&text=" + Uri.EscapeUriString(text);
        }
    }
}
