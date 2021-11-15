using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Discounts;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Common;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models;
using Nop.Web.Models.Catalog;
using System.Web;

namespace Nop.Web.Components
{
    public class HomepageMarkAsNewProductsViewComponent : NopViewComponent
    {
        private readonly IAclService _aclService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IStoreMappingService _storeMappingService;

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IDiscountService _discountService;

        public HomepageMarkAsNewProductsViewComponent(IAclService aclService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IDiscountService discountService)
        {
            _aclService = aclService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeMappingService = storeMappingService;

            _workContext = workContext;
            _storeContext = storeContext;
            _discountService = discountService;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize)
        {
            //var currentUser = (await _workContext.GetCurrentCustomerAsync()).Username;
            //bool isLoggedIn = string.IsNullOrWhiteSpace(currentUser) ? false : (currentUser == "admin@neta.mx" || currentUser == "admin@yourstore.com");

            var products = await (await _productService.GetAllProductsDisplayedOnHomepageWithMarkAsNewAsync())
            //ACL and store mapping
            .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
            //availability dates
            .Where(p => _productService.ProductIsAvailable(p))
            //visible individually
            .Where(p => p.VisibleIndividually).ToListAsync();

            //availability dates
            //products = products.Where(p => isLoggedIn || _productService.ProductIsAvailable(p)).ToList();

            var fase = await _storeContext.GetCurrentStoreAsync();
            if (fase.DisplayOrder == 1)
            {
                products = products.Where(v => v.Sku.ToUpper().EndsWith("LH") /*|| v.Sku.EndsWith("L1")*/).ToList();
            }
            else if (fase.DisplayOrder == 2)
            {
                //products = products.Where(v => !(v.Sku.EndsWith("LH") /*|| v.Sku.EndsWith("L1")*/)).ToList();
                products = products.Where(v => !v.Sku.ToUpper().EndsWith("LH")).ToList();
            }
            
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
                var first = discounts.Where(v => v.DiscountTypeId == 1).First();
                ViewBag.RoyaltyMessage = $"{first.DiscountPercentage.ToString()}% de descuento parejo para nuestros clientes los más neta!";
            }

            var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, productThumbPictureSize)).ToList();

            return View(model);
        }
    }   
}