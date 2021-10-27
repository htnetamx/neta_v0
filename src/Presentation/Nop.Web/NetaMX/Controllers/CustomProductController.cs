using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Services.Catalog;
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
        #endregion

        #region Ctor
        public CustomProductController(IAclService aclService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IStoreMappingService storeMappingService,
            IStoreContext storeContext,
            CustomeSetting customeSetting)
        {
            _aclService = aclService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeMappingService = storeMappingService;
            _storeContext = storeContext;
            _customeSetting = customeSetting;
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
        #endregion
    }
}
