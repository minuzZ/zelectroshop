// RTL Support provided by Credo inc (www.credo.co.il  ||   info@credo.co.il)

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Html;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;

namespace Nop.Services.Common
{
    /// <summary>
    /// PDF service
    /// </summary>
    public partial class PdfService : IPdfService
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly IMeasureService _measureService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingContext;
        private readonly IWebHelper _webHelper;

        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly MeasureSettings _measureSettings;
        private readonly PdfSettings _pdfSettings;
        private readonly TaxSettings _taxSettings;
        private readonly AddressSettings _addressSettings;

        #endregion

        #region Ctor

        public PdfService(ILocalizationService localizationService, 
            ILanguageService languageService,
            IWorkContext workContext,
            IOrderService orderService,
            IPaymentService paymentService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            ICurrencyService currencyService, 
            IMeasureService measureService,
            IPictureService pictureService,
            IProductService productService, 
            IProductAttributeParser productAttributeParser,
            IStoreService storeService,
            IStoreContext storeContext,
            ISettingService settingContext,
            IWebHelper webHelper, 
            CatalogSettings catalogSettings, 
            CurrencySettings currencySettings,
            MeasureSettings measureSettings,
            PdfSettings pdfSettings,
            TaxSettings taxSettings,
            AddressSettings addressSettings)
        {
            this._localizationService = localizationService;
            this._languageService = languageService;
            this._workContext = workContext;
            this._orderService = orderService;
            this._paymentService = paymentService;
            this._dateTimeHelper = dateTimeHelper;
            this._priceFormatter = priceFormatter;
            this._currencyService = currencyService;
            this._measureService = measureService;
            this._pictureService = pictureService;
            this._productService = productService;
            this._productAttributeParser = productAttributeParser;
            this._storeService = storeService;
            this._storeContext = storeContext;
            this._settingContext = settingContext;
            this._webHelper = webHelper;
            this._currencySettings = currencySettings;
            this._catalogSettings = catalogSettings;
            this._measureSettings = measureSettings;
            this._pdfSettings = pdfSettings;
            this._taxSettings = taxSettings;
            this._addressSettings = addressSettings;
        }

        #endregion

        #region Utilities

        protected virtual Font GetFont()
        {
            //nopCommerce supports unicode characters
            //nopCommerce uses Free Serif font by default (~/App_Data/Pdf/FreeSerif.ttf file)
            //It was downloaded from http://savannah.gnu.org/projects/freefont
            string fontPath = Path.Combine(_webHelper.MapPath("~/App_Data/Pdf/"), _pdfSettings.FontFileName);
            var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var font = new Font(baseFont, 10, Font.NORMAL);
            return font;
        }

        /// <summary>
        /// Get font direction
        /// </summary>
        /// <param name="lang">Language</param>
        /// <returns>Font direction</returns>
        protected virtual int GetDirection(Language lang)
        {
            return lang.Rtl ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR;
        }

        /// <summary>
        /// Get element alignment
        /// </summary>
        /// <param name="lang">Language</param>
        /// <param name="isOpposite">Is opposite?</param>
        /// <returns>Element alignment</returns>
        protected virtual int GetAlignment(Language lang, bool isOpposite = false)
        {
            //if we need the element to be opposite, like logo etc`.
            if (!isOpposite)
                return lang.Rtl ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT;
            else
                return lang.Rtl ? Element.ALIGN_LEFT : Element.ALIGN_RIGHT;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Print an order to PDF
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        /// <returns>A path of generates file</returns>
        public virtual string PrintOrderToPdf(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            string fileName = string.Format("order_{0}_{1}.pdf", order.OrderGuid, CommonHelper.GenerateRandomDigitCode(4));
            string filePath = Path.Combine(_webHelper.MapPath("~/content/files/ExportImport"), fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                var orders = new List<Order>();
                orders.Add(order);
                PrintOrdersToPdf(fileStream, orders, languageId);
            }
            return filePath;
        }

        /// <summary>
        /// Print orders to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="orders">Orders</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        public virtual void PrintOrdersToPdf(Stream stream, IList<Order> orders, int languageId = 0)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (orders == null)
                throw new ArgumentNullException("orders");

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.LETTER;
            }


            var doc = new Document(pageSize);
            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.BLACK;
            var nameFont = GetFont();
            nameFont.SetStyle(Font.BOLD);
            nameFont.Color = BaseColor.BLACK;
            nameFont.Size = 14;
            var totalFont = GetFont();
            totalFont.SetStyle(Font.BOLD);
            totalFont.Color = BaseColor.BLACK;
            totalFont.Size = 12;
            var targetFont = GetFont();
            targetFont.Color = BaseColor.BLACK;
            targetFont.Size = 12;
            var underlinedFont = GetFont();
            underlinedFont.SetStyle(Font.BOLD);
            underlinedFont.Color = BaseColor.BLACK;
            underlinedFont.SetStyle(Font.UNDERLINE);
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            int ordCount = orders.Count;
            int ordNum = 0;

            foreach (var order in orders)
            {
                //by default _pdfSettings contains settings for the current active store
                //and we need PdfSettings for the store which was used to place an order
                //so let's load it based on a store of the current order
                var pdfSettingsByStore = _settingContext.LoadSetting<PdfSettings>(order.StoreId);


                var lang = _languageService.GetLanguageById(languageId == 0 ? order.CustomerLanguageId : languageId);
                if (lang == null || !lang.Published)
                    lang = _workContext.WorkingLanguage;

                #region Header

                //header
                PdfPTable headerTable = new PdfPTable(1);
                headerTable.RunDirection = GetDirection(lang);
                headerTable.DefaultCell.Border = Rectangle.NO_BORDER;

                var cellHeader = new PdfPCell(new Phrase(Environment.NewLine, nameFont));
                cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
                string invoiceNum = String.Format("{0}-{1}", order.Id, _orderService.UpdateNextInvoceNum(order));
                string date = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc).ToString("D", new CultureInfo(lang.LanguageCulture));
                string title = String.Format(_localizationService.GetResource("PDFInvoice.InvoiceNum", lang.Id), invoiceNum, date);
                cellHeader.Phrase.Add(new Phrase(title));
                cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
                cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
                cellHeader.HorizontalAlignment = Element.ALIGN_CENTER;
                cellHeader.Border = Rectangle.NO_BORDER;

                headerTable.AddCell(cellHeader);
                headerTable.WidthPercentage = 100f;

                doc.Add(headerTable); 

                #endregion

                #region Details

                var detailsTable  = new PdfPTable(2);
                detailsTable.RunDirection = GetDirection(lang);
                //detailsTable.DefaultCell.Border = Rectangle.NO_BORDER;
                detailsTable.WidthPercentage = 100f;
                detailsTable.SetWidths(new[] { 65, 35 });

                //seller
                var sellerTable = new PdfPTable(1);
                sellerTable.DefaultCell.Border = Rectangle.NO_BORDER;
                sellerTable.RunDirection = GetDirection(lang);
                sellerTable.AddCell(new Paragraph(_localizationService.GetResource("PDFInvoice.Seller", lang.Id), titleFont));
                sellerTable.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.IPShort", lang.Id), pdfSettingsByStore.IP), font));
                sellerTable.AddCell(new Paragraph("   " + pdfSettingsByStore.InvoiceAddress, font));
                sellerTable.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.INN", lang.Id), pdfSettingsByStore.INN), font));
                sellerTable.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.OGRN", lang.Id), pdfSettingsByStore.OGRN), font));
                sellerTable.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.RS", lang.Id), pdfSettingsByStore.RS), font));
                sellerTable.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.KS", lang.Id), pdfSettingsByStore.KS), font));
                sellerTable.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.BIK", lang.Id), pdfSettingsByStore.BIK), font));
                sellerTable.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Info", lang.Id), pdfSettingsByStore.Info), font));
                detailsTable.AddCell(sellerTable);

                var customerTable = new PdfPTable(1);
                customerTable.DefaultCell.Border = Rectangle.NO_BORDER;
                customerTable.RunDirection = GetDirection(lang);
                customerTable.AddCell(new Paragraph(_localizationService.GetResource("PDFInvoice.Customer", lang.Id), titleFont));
                customerTable.AddCell(new Paragraph("   " + String.Format("{0} {1}", order.ShippingAddress.FirstName, order.ShippingAddress.LastName), font));
                customerTable.AddCell(new Paragraph("   " + order.ShippingAddress.PhoneNumber, font));
                detailsTable.AddCell(customerTable);

                doc.Add(detailsTable);
                doc.Add(new Paragraph(" "));
                #endregion

                #region Products

                //products
                PdfPTable productsHeader = new PdfPTable(1);
                productsHeader.RunDirection = GetDirection(lang);
                productsHeader.WidthPercentage = 100f;
                var cellProducts = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.Product(s)", lang.Id), titleFont));
                cellProducts.Border = Rectangle.NO_BORDER;
                productsHeader.AddCell(cellProducts);
                doc.Add(productsHeader);
                doc.Add(new Paragraph(" "));


                var orderItems = _orderService.GetAllOrderItems(order.Id, null, null, null, null, null, null);

                var productsTable = new PdfPTable(_catalogSettings.ShowProductSku ? 5 : 4);
                productsTable.RunDirection = GetDirection(lang);
                productsTable.WidthPercentage = 100f;
                if (lang.Rtl)
                {
                    productsTable.SetWidths(_catalogSettings.ShowProductSku
                        ? new[] {15, 10, 15, 15, 45}
                        : new[] {20, 10, 20, 50});
                }
                else
                {
                    productsTable.SetWidths(_catalogSettings.ShowProductSku
                        ? new[] {45, 15, 15, 10, 15}
                        : new[] {50, 20, 10, 20});
                }

                //product name
                var cellProductItem = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.ProductName", lang.Id), font));
                cellProductItem.BackgroundColor = BaseColor.LIGHT_GRAY;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);

                //SKU
                if (_catalogSettings.ShowProductSku)
                {
                    cellProductItem = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.SKU", lang.Id), font));
                    cellProductItem.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cellProductItem);
                }

                //price
                cellProductItem = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.ProductPrice", lang.Id), font));
                cellProductItem.BackgroundColor = BaseColor.LIGHT_GRAY;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);

                //qty
                cellProductItem = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.ProductQuantity", lang.Id), font));
                cellProductItem.BackgroundColor = BaseColor.LIGHT_GRAY;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);

                //total
                cellProductItem = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.ProductTotal", lang.Id), font));
                cellProductItem.BackgroundColor = BaseColor.LIGHT_GRAY;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);

                for (int i = 0; i < orderItems.Count; i++)
                {
                    var pAttribTable = new PdfPTable(1);
                    pAttribTable.RunDirection = GetDirection(lang);
                    pAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;

                    var orderItem = orderItems[i];
                    var p = orderItem.Product;

                    //product name
                    string name = p.GetLocalized(x => x.Name, lang.Id);
                    pAttribTable.AddCell(new Paragraph(name, font));
                    cellProductItem.AddElement(new Paragraph(name, font));
                    if (!String.IsNullOrEmpty(orderItem.AttributeDescription))
                    {
                        var attributesParagraph = new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true), attributesFont);
                        pAttribTable.AddCell(attributesParagraph);
                    }
                    productsTable.AddCell(pAttribTable);

                    //SKU
                    if (_catalogSettings.ShowProductSku)
                    {
                        var sku = p.FormatSku(orderItem.AttributesXml, _productAttributeParser);
                        cellProductItem = new PdfPCell(new Phrase(sku ?? String.Empty, font));
                        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                        productsTable.AddCell(cellProductItem);
                    }

                    //price
                    string unitPrice = string.Empty;
                    if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                    {
                        //including tax
                        var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                        unitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);
                    }
                    else
                    {
                        //excluding tax
                        var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                        unitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);
                    }
                    cellProductItem = new PdfPCell(new Phrase(unitPrice, font));
                    cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                    productsTable.AddCell(cellProductItem);

                    //qty
                    cellProductItem = new PdfPCell(new Phrase(orderItem.Quantity.ToString(), font));
                    cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                    productsTable.AddCell(cellProductItem);

                    //total
                    string subTotal = string.Empty; 
                    if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                    {
                        //including tax
                        var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                        subTotal = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);
                    }
                    else
                    {
                        //excluding tax
                        var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                        subTotal = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);
                    }
                    cellProductItem = new PdfPCell(new Phrase(subTotal, font));
                    cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                    productsTable.AddCell(cellProductItem);
                }
                doc.Add(productsTable);

                #endregion

                #region Totals

                //subtotal
                PdfPTable totalsTable = new PdfPTable(1);
                totalsTable.RunDirection = GetDirection(lang);
                totalsTable.DefaultCell.Border = Rectangle.NO_BORDER;
                totalsTable.WidthPercentage = 100f;

                //shipping
                if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
                {
                    if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                    {
                        //including tax
                        var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                        string orderShippingInclTaxStr = _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);

                        var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Shipping", lang.Id), orderShippingInclTaxStr), font));
                        p.HorizontalAlignment = Element.ALIGN_RIGHT;
                        p.Border = Rectangle.NO_BORDER;
                        totalsTable.AddCell(p);
                    }
                    else
                    {
                        //excluding tax
                        var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                        string orderShippingExclTaxStr = _priceFormatter.FormatShippingPrice(orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);

                        var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Shipping", lang.Id), orderShippingExclTaxStr), font));
                        p.HorizontalAlignment = Element.ALIGN_RIGHT;
                        p.Border = Rectangle.NO_BORDER;
                        totalsTable.AddCell(p);
                    }
                }
                //order total
                var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                string orderTotalStr = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, lang);
                var pTotal = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.OrderTotal", lang.Id), orderTotalStr), totalFont));
                pTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
                pTotal.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(pTotal);
                doc.Add(totalsTable);

                #endregion

                #region Footer

                //payment target

                PdfPTable targetTable = new PdfPTable(1);
                targetTable.RunDirection = GetDirection(lang);
                targetTable.DefaultCell.Border = Rectangle.NO_BORDER;

                var cellTarget = new PdfPCell(new Phrase(Environment.NewLine, targetFont));
                cellTarget.Phrase.Add(new Phrase(String.Format(_localizationService.GetResource("PDFInvoice.PaymentTarget", lang.Id), invoiceNum)));
                cellTarget.Border = Rectangle.NO_BORDER;
                cellTarget.HorizontalAlignment = Element.ALIGN_LEFT;
                targetTable.AddCell(cellTarget);
                targetTable.WidthPercentage = 100f;

                doc.Add(targetTable);

                //footer
                PdfPTable footerTable = new PdfPTable(2);
                footerTable.RunDirection = GetDirection(lang);
                footerTable.DefaultCell.Border = Rectangle.NO_BORDER;
                footerTable.SetWidths(new[] { 75, 25 });

                var cellFooter = new PdfPCell(new Phrase(Environment.NewLine, font));
                cellFooter.Phrase.Add(new Phrase(Environment.NewLine));
                cellFooter.Phrase.Add(new Phrase(_localizationService.GetResource("PDFInvoice.IPFull", lang.Id)));
                cellFooter.Phrase.Add(new Phrase("   "));
                cellFooter.Phrase.Add(new Phrase(_pdfSettings.IP, underlinedFont));
                cellFooter.HorizontalAlignment = Element.ALIGN_RIGHT;
                cellFooter.Border = Rectangle.NO_BORDER;

                var cellSignImg = new PdfPCell(Image.GetInstance(Path.Combine(_webHelper.MapPath("~/App_Data/Pdf/"), _pdfSettings.SignImageFile)));
                cellSignImg.HorizontalAlignment = Element.ALIGN_LEFT;
                cellSignImg.Border = Rectangle.NO_BORDER;

                footerTable.AddCell(cellFooter);
                footerTable.AddCell(cellSignImg);
                footerTable.WidthPercentage = 100f;

                doc.Add(footerTable);

                #endregion

                ordNum++;
                if (ordNum < ordCount)
                {
                    doc.NewPage();
                }
            }
            doc.Close();
        }
        
        /// <summary>
        /// Print packaging slips to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="shipments">Shipments</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        public virtual void PrintPackagingSlipsToPdf(Stream stream, IList<Shipment> shipments, int languageId = 0)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (shipments == null)
                throw new ArgumentNullException("shipments");

            var lang = _languageService.GetLanguageById(languageId);
            if (lang == null)
                throw new ArgumentException(string.Format("Cannot load language. ID={0}", languageId));

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.LETTER;
            }

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.BLACK;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);
            
            int shipmentCount = shipments.Count;
            int shipmentNum = 0;

            foreach (var shipment in shipments)
            {
                var order = shipment.Order;

                if (languageId == 0)
                {
                    lang = _languageService.GetLanguageById(order.CustomerLanguageId);
                    if (lang == null || !lang.Published)
                        lang = _workContext.WorkingLanguage;
                }

                var addressTable = new PdfPTable(1);
                if (lang.Rtl)
                    addressTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                addressTable.DefaultCell.Border = Rectangle.NO_BORDER;
                addressTable.WidthPercentage = 100f;

                addressTable.AddCell(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Shipment", lang.Id), shipment.Id), titleFont));
                addressTable.AddCell(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Order", lang.Id), order.Id), titleFont));

                if (!order.PickUpInStore)
                {
                    if (order.ShippingAddress == null)
                        throw new NopException(string.Format("Shipping is required, but address is not available. Order ID = {0}", order.Id));
                    
                    if (_addressSettings.CompanyEnabled && !String.IsNullOrEmpty(order.ShippingAddress.Company))
                        addressTable.AddCell(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Company", lang.Id),
                                    order.ShippingAddress.Company), font));

                    addressTable.AddCell(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Name", lang.Id),
                                order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName), font));
                    if (_addressSettings.PhoneEnabled)
                        addressTable.AddCell(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Phone", lang.Id),
                                    order.ShippingAddress.PhoneNumber), font));
                    if (_addressSettings.StreetAddressEnabled)
                        addressTable.AddCell(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Address", lang.Id),
                                    order.ShippingAddress.Address1), font));

                    if (_addressSettings.StreetAddress2Enabled && !String.IsNullOrEmpty(order.ShippingAddress.Address2))
                        addressTable.AddCell(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.Address2", lang.Id),
                                    order.ShippingAddress.Address2), font));

                    if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled || _addressSettings.ZipPostalCodeEnabled)
                        addressTable.AddCell(new Paragraph(String.Format("{0}, {1} {2}", order.ShippingAddress.City, order.ShippingAddress.StateProvince != null
                                        ? order.ShippingAddress.StateProvince.GetLocalized(x => x.Name, lang.Id)
                                        : "", order.ShippingAddress.ZipPostalCode), font));

                    if (_addressSettings.CountryEnabled && order.ShippingAddress.Country != null)
                        addressTable.AddCell(new Paragraph(String.Format("{0}", order.ShippingAddress.Country != null
                                        ? order.ShippingAddress.Country.GetLocalized(x => x.Name, lang.Id)
                                        : ""), font));
                }

                addressTable.AddCell(new Paragraph(" "));

                addressTable.AddCell(new Paragraph(String.Format(_localizationService.GetResource("PDFPackagingSlip.ShippingMethod", lang.Id), order.ShippingMethod), font));
                addressTable.AddCell(new Paragraph(" "));
                doc.Add(addressTable);

                var productsTable = new PdfPTable(3);
                productsTable.WidthPercentage = 100f;
                if (lang.Rtl)
                {
                    productsTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    productsTable.SetWidths(new[] {20, 20, 60});
                }
                else
                {
                    productsTable.SetWidths(new[] {60, 20, 20});
                }

                //product name
                var cell = new PdfPCell(new Phrase(_localizationService.GetResource("PDFPackagingSlip.ProductName", lang.Id),font));
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                //SKU
                cell = new PdfPCell(new Phrase(_localizationService.GetResource("PDFPackagingSlip.SKU", lang.Id), font));
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                //qty
                cell = new PdfPCell(new Phrase(_localizationService.GetResource("PDFPackagingSlip.QTY", lang.Id), font));
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                foreach (var si in shipment.ShipmentItems)
                {
                    var productAttribTable = new PdfPTable(1);
                    if (lang.Rtl)
                        productAttribTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    productAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;

                    //product name
                    var orderItem = _orderService.GetOrderItemById(si.OrderItemId);
                    if (orderItem == null)
                        continue;

                    var p = orderItem.Product;
                    string name = p.GetLocalized(x => x.Name, lang.Id);
                    productAttribTable.AddCell(new Paragraph(name, font));
                    if (!String.IsNullOrEmpty(orderItem.AttributeDescription))
                    {
                        var attributesParagraph = new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true), attributesFont);
                        productAttribTable.AddCell(attributesParagraph);
                    }
                    productsTable.AddCell(productAttribTable);

                    //SKU
                    var sku = p.FormatSku(orderItem.AttributesXml, _productAttributeParser);
                    cell = new PdfPCell(new Phrase(sku ?? String.Empty, font));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);

                    //qty
                    cell = new PdfPCell(new Phrase(si.Quantity.ToString(), font));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);
                }
                doc.Add(productsTable);

                shipmentNum++;
                if (shipmentNum < shipmentCount)
                {
                    doc.NewPage();
                }
            }


            doc.Close();
        }

        /// <summary>
        /// Print product collection to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="products">Products</param>
        public virtual void PrintProductsToPdf(Stream stream, IList<Product> products)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (products == null)
                throw new ArgumentNullException("products");

            var lang = _workContext.WorkingLanguage;

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.LETTER;
            }

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.BLACK;
            var font = GetFont();

            int productNumber = 1;
            int prodCount = products.Count;

            foreach (var product in products)
            {
                string productName = product.GetLocalized(x => x.Name, lang.Id);
                string productDescription = product.GetLocalized(x => x.FullDescription, lang.Id);

                var productTable = new PdfPTable(1);
                productTable.WidthPercentage = 100f;
                productTable.DefaultCell.Border = Rectangle.NO_BORDER;
                if (lang.Rtl)
                {
                    productTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                }

                productTable.AddCell(new Paragraph(String.Format("{0}. {1}", productNumber, productName), titleFont));
                productTable.AddCell(new Paragraph(" "));
                productTable.AddCell(new Paragraph(HtmlHelper.StripTags(HtmlHelper.ConvertHtmlToPlainText(productDescription, decode: true)), font));
                productTable.AddCell(new Paragraph(" "));

                if (product.ProductType == ProductType.SimpleProduct)
                {
                    //simple product
                    //render its properties such as price, weight, etc
                    productTable.AddCell(new Paragraph(String.Format("{0}: {1} {2}", _localizationService.GetResource("PDFProductCatalog.Price", lang.Id), product.Price.ToString("0.00"), _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode), font));
                    productTable.AddCell(new Paragraph(String.Format("{0}: {1}", _localizationService.GetResource("PDFProductCatalog.SKU", lang.Id), product.Sku), font));

                    if (product.IsShipEnabled && product.Weight > Decimal.Zero)
                        productTable.AddCell(new Paragraph(String.Format("{0}: {1} {2}", _localizationService.GetResource("PDFProductCatalog.Weight", lang.Id), product.Weight.ToString("0.00"), _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name), font));

                    if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                        productTable.AddCell(new Paragraph(String.Format("{0}: {1}", _localizationService.GetResource("PDFProductCatalog.StockQuantity", lang.Id), product.StockQuantity), font));

                    productTable.AddCell(new Paragraph(" "));
                }
                var pictures = _pictureService.GetPicturesByProductId(product.Id);
                if (pictures.Count > 0)
                {
                    var table = new PdfPTable(2);
                    table.WidthPercentage = 100f;
                    if (lang.Rtl)
                    {
                        table.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    }

                    for (int i = 0; i < pictures.Count; i++)
                    {
                        var pic = pictures[i];
                        if (pic != null)
                        {
                            var picBinary = _pictureService.LoadPictureBinary(pic);
                            if (picBinary != null && picBinary.Length > 0)
                            {
                                var pictureLocalPath = _pictureService.GetThumbLocalPath(pic, 200, false);
                                var cell = new PdfPCell(Image.GetInstance(pictureLocalPath));
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                cell.Border = Rectangle.NO_BORDER;
                                table.AddCell(cell);
                            }
                        }
                    }

                    if (pictures.Count % 2 > 0)
                    {
                        var cell = new PdfPCell(new Phrase(" "));
                        cell.Border = Rectangle.NO_BORDER;
                        table.AddCell(cell);
                    }

                    productTable.AddCell(table);
                    productTable.AddCell(new Paragraph(" "));
                }


                if (product.ProductType == ProductType.GroupedProduct)
                {
                    //grouped product. render its associated products
                    int pvNum = 1;
                    foreach (var associatedProduct in _productService.GetAssociatedProducts(product.Id, showHidden: true))
                    {
                        productTable.AddCell(new Paragraph(String.Format("{0}-{1}. {2}", productNumber, pvNum, associatedProduct.GetLocalized(x => x.Name, lang.Id)), font));
                        productTable.AddCell(new Paragraph(" "));

                        //uncomment to render associated product description
                        //string apDescription = associatedProduct.GetLocalized(x => x.ShortDescription, lang.Id);
                        //if (!String.IsNullOrEmpty(apDescription))
                        //{
                        //    productTable.AddCell(new Paragraph(HtmlHelper.StripTags(HtmlHelper.ConvertHtmlToPlainText(apDescription)), font));
                        //    productTable.AddCell(new Paragraph(" "));
                        //}

                        //uncomment to render associated product picture
                        //var apPicture = _pictureService.GetPicturesByProductId(associatedProduct.Id).FirstOrDefault();
                        //if (apPicture != null)
                        //{
                        //    var picBinary = _pictureService.LoadPictureBinary(apPicture);
                        //    if (picBinary != null && picBinary.Length > 0)
                        //    {
                        //        var pictureLocalPath = _pictureService.GetThumbLocalPath(apPicture, 200, false);
                        //        productTable.AddCell(Image.GetInstance(pictureLocalPath));
                        //    }
                        //}

                        productTable.AddCell(new Paragraph(String.Format("{0}: {1} {2}", _localizationService.GetResource("PDFProductCatalog.Price", lang.Id), associatedProduct.Price.ToString("0.00"), _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode), font));
                        productTable.AddCell(new Paragraph(String.Format("{0}: {1}", _localizationService.GetResource("PDFProductCatalog.SKU", lang.Id), associatedProduct.Sku), font));

                        if (associatedProduct.IsShipEnabled && associatedProduct.Weight > Decimal.Zero)
                            productTable.AddCell(new Paragraph(String.Format("{0}: {1} {2}", _localizationService.GetResource("PDFProductCatalog.Weight", lang.Id), associatedProduct.Weight.ToString("0.00"), _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name), font));

                        if (associatedProduct.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                            productTable.AddCell(new Paragraph(String.Format("{0}: {1}", _localizationService.GetResource("PDFProductCatalog.StockQuantity", lang.Id), associatedProduct.StockQuantity), font));

                        productTable.AddCell(new Paragraph(" "));

                        pvNum++;
                    }
                }

                doc.Add(productTable);

                productNumber++;

                if (productNumber <= prodCount)
                {
                    doc.NewPage();
                }
            }

            doc.Close();
        }

        #endregion
    }
}