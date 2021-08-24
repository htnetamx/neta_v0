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
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWebHelper _webHelper;

        public HomepageProductsViewComponent(IAclService aclService,
            IProductModelFactory productModelFactory,
            IDiscountService discountService,
            IProductService productService,
            IStoreMappingService storeMappingService,
            IStoreContext storeContext,
            IWorkContext workContext,
            ICatalogModelFactory catalogModelFactory,
            IGenericAttributeService genericAttributeService,
            IWebHelper webHelper)
        {
            _aclService = aclService;
            _discountService = discountService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeMappingService = storeMappingService;
            _storeContext = storeContext;
            _workContext = workContext;
             _catalogModelFactory= catalogModelFactory;
            _genericAttributeService = genericAttributeService;
            _webHelper = webHelper;
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
                var first = discounts.Where(v => v.DiscountTypeId == 1).First();
                ViewBag.RoyaltyMessage = $"{first.DiscountPercentage.ToString()}% de descuento parejo para nuestros clientes los más neta!";
            }

            
            var command = new CatalogProductsCommand();
            var url = _webHelper.GetThisPageUrl(true);
            

            Uri myUri = new Uri(_webHelper.GetThisPageUrl(true));

            var defaultIndex = 1;
            var pageSize = HttpUtility.ParseQueryString(myUri.Query).Get("pagesize");
            var pageNumber = HttpUtility.ParseQueryString(myUri.Query).Get("pagenumber");
            var orderBy = HttpUtility.ParseQueryString(myUri.Query).Get("orderby");
            
            if (pageSize is null)
            {
                pageSize = defaultIndex.ToString();
            }
            if (pageNumber is null)
            {
                pageNumber = "1";
            }
            if (orderBy is null)
            {
                orderBy = "0";
            }
            var search_Model_Type = "custom";
            //var search_Model_Type = "default";

            var model = new SearchModel();


            //Page Size
            if (search_Model_Type == "custom")
            {
                var pageSizeOptions = "3,6,10,20";
                var fixedPageSize = 10;
                model.CatalogProductsModel.AllowProductViewModeChanging = false;
                model.CatalogProductsModel.AllowProductSorting = true;
                model.CatalogProductsModel.AllowCustomersToSelectPageSize = true;
                await _catalogModelFactory.PreparePageSizeOptionsAsync(model.CatalogProductsModel, command, true, pageSizeOptions, fixedPageSize);
                await _catalogModelFactory.PrepareSortingOptionsAsync(model.CatalogProductsModel, command);
            }
            else
            {
                model = await _catalogModelFactory.PrepareSearchModelAsync(model, command);
            }


            //Page Index
            var old_i = model.CatalogProductsModel.PageSizeOptions.ToList().FindIndex(p => p.Selected);
            var new_i = model.CatalogProductsModel.PageSizeOptions.ToList().FindIndex(p => p.Value==pageSize);

            if (old_i==-1 || new_i==-1)
            {
                old_i = defaultIndex;
                new_i = defaultIndex;
                pageSize = model.CatalogProductsModel.PageSizeOptions[defaultIndex].Value;
            }
            model.CatalogProductsModel.PageSizeOptions[old_i].Selected = false;
            model.CatalogProductsModel.PageSizeOptions[new_i].Selected = true;

            var mi = model.CatalogProductsModel.AvailableSortOptions.ToList();
            //Sorting
            old_i = model.CatalogProductsModel.AvailableSortOptions.ToList().FindIndex(p => p.Selected);
            new_i = model.CatalogProductsModel.AvailableSortOptions.ToList().FindIndex(p => p.Value == orderBy);
            if (old_i == -1 || new_i == -1)
            {
                old_i = 0;
                new_i = 0;
                pageSize = model.CatalogProductsModel.AvailableSortOptions[0].Value;
            }
            model.CatalogProductsModel.AvailableSortOptions[old_i].Selected = false;
            model.CatalogProductsModel.AvailableSortOptions[new_i].Selected = true;


            //Update Command

            //Force the input
                pageSize = "6";
                orderBy = "0";
            command.PageSize = Convert.ToInt32(pageSize);
            command.PageNumber = Convert.ToInt32(pageNumber) - 1;
            command.OrderBy = Convert.ToInt32(orderBy);
   
            var modelList = (await _productModelFactory.PrepareProductOverviewModelsAsync1(products, true, true, productThumbPictureSize, discounts: discProducts,command: command));
            model.CatalogProductsModel.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(modelList)).ToList();
            model.CatalogProductsModel.LoadPagedList(modelList);
            return View(model);
        }
    }   
}