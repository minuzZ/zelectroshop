using System;
using System.Net;
using Nop.Core.Domain.SMS;

namespace Nop.Services.Messages
{
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

        private string makeBytehandGetRequestURL(string bytehandId, string bytehandKey, 
            string number, string from, string text)
        {
            return String.Format("http://bytehand.com:3800/send?id={0}&key={1}&to={2}&from={3}&text={4}",
                bytehandId, bytehandKey, Uri.EscapeUriString(number), 
                Uri.EscapeUriString(from), Uri.EscapeUriString(text));
        }
    }
}
