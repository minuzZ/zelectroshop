namespace Nop.Services.Messages
{
    /// <summary>
    /// SMS sender
    /// </summary>
    public interface ISMSSender
    {
        /// <summary>
        /// Sends sms message
        /// </summary>
        /// <param name="from">SMS sender name</param>
        /// <param name="number">Receiver number</param>
        /// <param name="text">Text message</param>
        void SendSMS(string from, string number, string text);
    }
}
