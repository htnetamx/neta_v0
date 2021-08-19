using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Discounts;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models;

namespace Nop.Web.Components
{
    public class HomepageProductsViewComponent : NopViewComponent
    {
        private readonly IAclService _aclService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IDiscountService _discountService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ICatalogModelFactory _catalogModelFactory;

        public HomepageProductsViewComponent(IAclService aclService,
            IProductModelFactory productModelFactory,
            IDiscountService discountService,
            IProductService productService,
            IStoreMappingService storeMappingService,
            IStoreContext storeContext,
            IWorkContext workContext,
            ICatalogModelFactory catalogModelFactory)
        {
            _aclService = aclService;
            _discountService = discountService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeMappingService = storeMappingService;
            _storeContext = storeContext;
            _workContext = workContext;
             _catalogModelFactory= catalogModelFactory;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize)
        {
            var currentUser = (await _workContext.GetCurrentCustomerAsync()).Username;
            bool isLoggedIn = string.IsNullOrWhiteSpace(currentUser) ? false : (currentUser == "admin@neta.mx" || currentUser == "admin@yourstore.com");
            var products = await (await _productService.GetAllProductsDisplayedOnHomepageAsync())
            //ACL and store mapping
            .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
            //availability dates
            .Where(p => isLoggedIn || _productService.ProductIsAvailable(p))
            //visible individually
            .Where(p => p.VisibleIndividually).ToListAsync();

            if (!products.Any())
                return Content("");

            var discounts = await _discountService.GetAllDiscountsAsync1(
                (await _storeContext.GetCurrentStoreAsync()).Id,
                System.DateTime.UtcNow, System.DateTime.UtcNow);

            //Descuentos con productos asociados
            var discProducts = discounts.Where(v => v.DiscountTypeId == 2).ToList();

            //Descuentos con para la orden completa
            if (discounts.Where(v => v.DiscountTypeId == 1).Any())
            {
                ViewBag.RoyaltyMessage = "Royalty Program a Nivel de Orden";
            }
            var model2 = new Models.Catalog.SearchModel();
            model2.CatalogProductsModel.AllowProductViewModeChanging =false;
            model2.CatalogProductsModel.AllowProductSorting = true;
            model2.CatalogProductsModel.AllowCustomersToSelectPageSize = true;
            var cat = new Models.Catalog.CatalogProductsCommand();
            await _catalogModelFactory.PrepareSortingOptionsAsync(model2.CatalogProductsModel, cat);
            await _catalogModelFactory.PreparePageSizeOptionsAsync(model2.CatalogProductsModel, cat,true,"3,6,10,20",10);
            var modelList = (await _productModelFactory.PrepareProductOverviewModelsAsync1(products, true, true, productThumbPictureSize, discounts: discProducts,command: cat));
            model2.CatalogProductsModel.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(modelList)).ToList();
            model2.CatalogProductsModel.LoadPagedList(modelList);
            return View(model2);
        }
    }   
}