using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;

namespace Nop.Plugin.Shipping.RussianPost
{
    /// <summary>
    /// Russian post computation method
    /// </summary>
    public class RussianPostComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        #region Constants

        private const int DEFAULT_WEIGHT = 500;
        private const string OUTPUT_FORMAT = "PLAIN";
        private const int ROUND = 10;
        private const string PARCEL_NAME = "ЦеннаяПосылка";
        private const string EMS_NAME = "EMS";
        private const string RUSSIAN_POST = "Почта России. Ценная посылка.";
        private const string RP_DESCR = "Доставка осуществляется до ближайшего почтового отделения в любом населённом пункте России. Заявленное время ожидания для вашей посылки {0} (дней).";
        private const string EMS_DESCR = "Служба «EMS Почта России» работает быстрее и надёжнее обычной почты и доставляет до двери покупателя. Заявленное время ожидания для вашей посылки {0} (дней).";
        private const string DEF_NAME = "Упс :( Проблемы с расчетом стоимости отправки";
        private const string DEF_DESCR = "Но ничего страшного! Продолжайте оформлять заказ, а мы в свою очередь свяжемся с вами и уточним стоимость доставки.";

        #endregion

        #region Fields
        private readonly IShippingService _shippingService;
        private readonly ISettingService _settingService;
        private readonly RussianPostSettings _RussianPostSettings;

        #endregion

        #region Ctor
        public RussianPostComputationMethod(IMeasureService measureService,
            IShippingService shippingService, ISettingService settingService,
            RussianPostSettings RussianPostSettings)
        {
            this._shippingService = shippingService;
            this._settingService = settingService;
            this._RussianPostSettings = RussianPostSettings;
        }
        #endregion

        #region Utilities

        private string GetGatewayUrl()
        {
            return _RussianPostSettings.GatewayUrl;
        }

        private List<ShippingOption> ParseResponse(string response)
        {
            var result = new List<ShippingOption>();
            string[] lines = response.Split('\n');
            string parcelLine = lines.First(x => x.StartsWith(PARCEL_NAME));
            string emsLine = lines.First(x => x.StartsWith(EMS_NAME));
            if (String.IsNullOrEmpty(parcelLine) && String.IsNullOrEmpty(emsLine))
            {
                throw new Exception("Ошибка при расчете стоимости доставки");
            }
            string[] parcelVals = parcelLine.Split(' ');
            string[] emsVals = emsLine.Split(' ');
            if (parcelVals.Length != 3 || emsVals.Length != 3)
            {
                throw new Exception("Ошибка при расчете стоимости доставки");
            }

            if (parcelVals[1].Contains('.'))
            {
                int i = parcelVals[1].IndexOf('.');
                parcelVals[1] = parcelVals[1].Substring(0, i);
            };

            if (emsVals[1].Contains('.'))
            {
                int i = emsVals[1].IndexOf('.');
                emsVals[1] = emsVals[1].Substring(0, i);
            };

            var parcelOption = new ShippingOption()
            {
                Name = RUSSIAN_POST,
                Rate = Convert.ToDecimal(parcelVals[1]),
                Description = String.Format(RP_DESCR, parcelVals[2])
            };

            var emsOption = new ShippingOption()
            {
                Name = EMS_NAME,
                Rate = Convert.ToDecimal(emsVals[1]),
                Description = String.Format(EMS_DESCR, emsVals[2])
            };

            result.Add(parcelOption);
            result.Add(emsOption);

            return result;
        }

        private List<ShippingOption> GetDefaultOption()
        {
            return new List<ShippingOption>() 
            { 
                new ShippingOption() 
                {
                    Name = DEF_NAME,
                    Rate = 0,
                    Description = DEF_DESCR
                }
            };
        }

        private List<ShippingOption> RequestShippingOptions(string url,
            string st, string ml, string f,
            string t, int w, int v, string o, int r)
        {
            try
            {
                string format = "{0}?st={1}&ml={2}&f={3}&t={4}&w={5}&v={6}&o={7}&r={8}";
                string requestStr = String.Format(format, url, st, ml, f, t, w, v, o, r);
                HttpWebRequest request = WebRequest.Create(requestStr) as HttpWebRequest;
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.Method = "GET";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string str = response.ContentEncoding;
                string rspContent;
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    rspContent = reader.ReadToEnd();
                }
                return ParseResponse(rspContent);
            }
            catch
            {
                return GetDefaultOption();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Represents a response of getting shipping rate options</returns>
        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException("getShippingOptionRequest");

            var response = new GetShippingOptionResponse();

            if (getShippingOptionRequest.Items == null)
            {
                response.AddError("No shipment items");
                return response;
            }

            if (getShippingOptionRequest.ShippingAddress == null)
            {
                response.AddError("Shipping address is not set");
                return response;
            }

            try
            {
                var options = RequestShippingOptions(_RussianPostSettings.GatewayUrl,
                    _RussianPostSettings.Site,
                    _RussianPostSettings.Email,
                    getShippingOptionRequest.ZipPostalCodeFrom,
                    getShippingOptionRequest.ShippingAddress.ZipPostalCode,
                    DEFAULT_WEIGHT,
                    Convert.ToInt32(_shippingService.GetTotalPrice(getShippingOptionRequest.Items)),
                    OUTPUT_FORMAT,
                    ROUND);

                foreach (var opt in options)
                {
                    response.ShippingOptions.Add(opt);
                }
            }
            catch (Exception ex)
            {
                response.AddError(ex.Message);
            }
            return response;
        }

        /// <summary>
        /// Gets fixed shipping rate (if shipping rate computation method allows it and the rate can be calculated before checkout).
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Fixed shipping rate; or null in case there's no fixed shipping rate</returns>
        public decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            return null;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "ShippingRussianPost";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Shipping.RussianPost.Controllers" }, { "area", null } };
        }
        
        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new RussianPostSettings()
            {
                GatewayUrl = "http://test.postcalc.ru/",
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.RussianPost.Fields.GatewayUrl", "Gateway URL");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.RussianPost.Fields.GatewayUrl.Hint", "Specify gateway URL.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.RussianPost.Fields.Email", "Shop owner email");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.RussianPost.Fields.Email.Hint", "Enter shop owner email.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.RussianPost.Fields.Site", "Shop owner site url");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.RussianPost.Fields.Site.Hint", "Enter shop owner site url.");            
            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<RussianPostSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Shipping.RussianPost.Fields.GatewayUrl");
            this.DeletePluginLocaleResource("Plugins.Shipping.RussianPost.Fields.GatewayUrl.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.RussianPost.Fields.Email");
            this.DeletePluginLocaleResource("Plugins.Shipping.RussianPost.Fields.Email.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.RussianPost.Fields.Site");
            this.DeletePluginLocaleResource("Plugins.Shipping.RussianPost.Fields.Site.Hint");

            base.Uninstall();
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets a shipping rate computation method type
        /// </summary>
        public ShippingRateComputationMethodType ShippingRateComputationMethodType
        {
            get
            {
                return ShippingRateComputationMethodType.Realtime;
            }
        }

        /// <summary>
        /// Gets a shipment tracker
        /// </summary>
        public IShipmentTracker ShipmentTracker 
        { 
            get { return null; }
        }

        #endregion
    }
}