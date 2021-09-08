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

        public HomepageProductsViewComponent(IAclService aclService,
            IProductModelFactory productModelFactory,
            IDiscountService discountService,
            IProductService productService,
            IStoreMappingService storeMappingService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _aclService = aclService;
            _discountService = discountService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeMappingService = storeMappingService;
            _storeContext = storeContext;
            _workContext = workContext;
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

            var fase = await _storeContext.GetCurrentStoreAsync();
            if (fase.DisplayOrder == 1)
            {
                products = products.Where(v => v.Sku.EndsWith("F1")).ToList();
            }
            else if (fase.DisplayOrder == 2)
            {
                products = products.Where(v => v.Sku.EndsWith("F2")).ToList();
            }
            else
            {
                products = products.Where(v => !(v.Sku.EndsWith("F1") || v.Sku.EndsWith("F2"))).ToList();
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

            var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, productThumbPictureSize, discounts: discProducts)).ToList();

            return View(model.OrderBy(v => v.DisplayOrder).ToList());
        }
    }
}