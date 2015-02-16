using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Robokassa
{
    public class RobokassaSettings: ISettings
    {
        public string Login { get; set; }
        public string Password1 { get; set; }
        public string Password2 { get; set; }
        public string PaymentDescription { get; set; }
        public string ApiURL { get; set; }
    }
}
