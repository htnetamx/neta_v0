using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;

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

        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        #endregion

        #region Ctor
        public CustomProductController(IAclService aclService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IStoreMappingService storeMappingService,
            IStoreContext storeContext,
            CustomeSetting customeSetting,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext)
        {
            _aclService = aclService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeMappingService = storeMappingService;
            _storeContext = storeContext;
            _customeSetting = customeSetting;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;

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
        #endregion
    }
}
