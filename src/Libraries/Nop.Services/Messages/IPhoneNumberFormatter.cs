namespace Nop.Services.Messages
{
    public interface IPhoneNumberFormatter
    {
        /// <summary>
        /// Gets formatted number according to settings
        /// Admin.Configuration.Settings.SMS.CountryCode with 
        /// last Admin.Configuration.Settings.SMS.NumberLength numbers from input number
        /// </summary>
        /// <param name="number">Not formatted number</param>
        /// <returns></returns>
        string FormatNumber(string number);
    }
}
