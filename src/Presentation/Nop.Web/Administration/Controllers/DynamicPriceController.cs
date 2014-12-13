using System;
using System.Web.Mvc;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Security;

namespace Nop.Admin.Controllers
{
    public class DynamicPriceController : BaseAdminController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Constructors

        public DynamicPriceController(IProductService productService,
            ILocalizationService localizationService, IPermissionService permissionService)
		{
            this._productService = productService;
            this._permissionService = permissionService;
            this._localizationService = localizationService;
		}

		#endregion 

        public ActionResult Recalculate()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            _productService.RecalculatePrices();

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.DynamicPrice.Recalculated"));

            return RedirectToAction("DynamicPrice", "Setting");
        }
	}
}