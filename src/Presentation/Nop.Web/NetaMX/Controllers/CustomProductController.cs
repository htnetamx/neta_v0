using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Promotion;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Models;

namespace Nop.Web.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class CustomProductController : BasePublicController
    {
        #region Fields
        private readonly IAclService _aclService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreContext _storeContext;
        private readonly CustomeSetting _customeSetting;
        private readonly INetaPromotionService _netaPromotionService;

        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        #endregion


        #region Fields

        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly IWebHelper _webHelper;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor
        public CustomProductController(IAclService aclService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IStoreMappingService storeMappingService,
            IStoreContext storeContext,
            CustomeSetting customeSetting,
            INetaPromotionService netaPromotionService,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext,

            ICatalogModelFactory catalogModelFactory,
            ICategoryService categoryService,
            IWebHelper webHelper,
            IGenericAttributeService genericAttributeService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService
            )
        {
            _aclService = aclService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeMappingService = storeMappingService;
            _storeContext = storeContext;
            _customeSetting = customeSetting;
            _netaPromotionService = netaPromotionService;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;

            _catalogModelFactory = catalogModelFactory;
            _categoryService = categoryService;
            _webHelper = webHelper;
            _genericAttributeService = genericAttributeService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;

        }
        #endregion

        #region Methods
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> LoadMoreProductOnHomePage(int pageIndex)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var products = await (await _productService.GetProductsDisplayedOnHomepageAsync(pageIndex, _customeSetting.Loadhomepageproduct, store.DisplayOrder))
            //ACL and store mapping
            .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
            //availability dates
            .Where(p => _productService.ProductIsAvailable(p)).ToListAsync();

            var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, 100)).ToList();

            pageIndex++;
            return Json(new
            {
                html = await RenderPartialViewToStringAsync("_HomePageProduct", model),
                pageNumber = pageIndex
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> GetTotalSaving()
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            var totalSaving = decimal.Zero;
            var totalCart = decimal.Zero;
            foreach (var sci in cart)
            {
                var product = await _productService.GetProductByIdAsync(sci.ProductId);
                var productSaving = (product.OldPrice - product.Price) * sci.Quantity;
                totalCart += product.Price * sci.Quantity;
                totalSaving += productSaving;
                totalSaving = Math.Round(totalSaving, 2);
            }

            return Json(new { totalSaving = totalSaving, totalCart = totalCart });
        }

        public virtual async Task<IActionResult> PromotionProduct(int id)
        {
            var model = new PromotionProductListModel();
            var netapromotion = await _netaPromotionService.GetNetaPromotionByIdAsync(id);
            var products = await _netaPromotionService.GetPromotionProductsByPromotionId(id);

            var prepareproductOverviewModel = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true)).ToList();
            model.PromotionId = netapromotion.Id;
            model.PromotionName = netapromotion.Name;
            model.StartDate = netapromotion.StartDateUtc;
            model.EndDate = netapromotion.EndDateUtc;
            model.PromotionProductOverviewModel = prepareproductOverviewModel;
            model.Published = netapromotion.Published;
            return View(model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> DeletePromotionProduct(int id)
        {
            await _netaPromotionService.DeletePromotionProductsByPromotionId(id);
            
            return Json(new
            {
                Result = true
            });
        }

        #endregion
    }
}
