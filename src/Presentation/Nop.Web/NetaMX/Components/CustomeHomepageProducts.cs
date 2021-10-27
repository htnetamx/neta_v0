using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Services.Catalog;
using Nop.Services.Discounts;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components
{
    public class CustomeHomepageProductsViewComponent : NopViewComponent
    {
        private readonly IAclService _aclService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IStoreMappingService _storeMappingService;

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IDiscountService _discountService;
        private readonly CustomeSetting _customeSetting;

        public CustomeHomepageProductsViewComponent(IAclService aclService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IDiscountService discountService,
            CustomeSetting customeSetting)
        {
            _aclService = aclService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeMappingService = storeMappingService;

            _workContext = workContext;
            _storeContext = storeContext;
            _discountService = discountService;
            _customeSetting = customeSetting;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var products = await (await _productService.GetProductsDisplayedOnHomepageAsync(0, _customeSetting.Loadhomepageproduct, store.DisplayOrder))
            //ACL and store mapping
            .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
            //availability dates
            .Where(p => _productService.ProductIsAvailable(p)).ToListAsync();
            
            if (!products.Any())
                return Content("");

            var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, productThumbPictureSize)).ToList();

            return View(model);
        }
    }   
}