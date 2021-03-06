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
            var products = await _productService.GetAllProductsDisplayedOnHomepageAsync();
            var pp = products.Where(p => p.DisplayOrder == 0);

            //ACL and store mapping
            products = await products.WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p)).ToListAsync();
            pp = products.Where(p => p.DisplayOrder == 0);
            //availability dates
            products = products.Where(p => isLoggedIn || _productService.ProductIsAvailable(p)).ToList();
            //visible individually
            products = await products.Where(p => p.VisibleIndividually).ToListAsync();

            var fase = await _storeContext.GetCurrentStoreAsync();
            if (fase.DisplayOrder == 1)
            {
                products = products.Where(v => v.Sku.EndsWith("LH") || v.Sku.EndsWith("L1")).ToList();
            }
            else if (fase.DisplayOrder == 2)
            {
                products = products.Where(v => !(v.Sku.EndsWith("LH") || v.Sku.EndsWith("L1"))).ToList();
            }
            //else
            //{
            //    products = products.Where(v => !(v.Sku.EndsWith("F1") || v.Sku.EndsWith("F2"))).ToList();
            //}

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

            var defaultIndex = "1";
            var defaultOrderBy = "0";
            var defaultPageSize = "200";
            var defaultIncrement = "100";
            var defaultMinimumProducts = "2";

            var pageSize = HttpUtility.ParseQueryString(myUri.Query).Get("pagesize");
            var pageNumber = HttpUtility.ParseQueryString(myUri.Query).Get("pagenumber");
            var orderBy = HttpUtility.ParseQueryString(myUri.Query).Get("orderby");
            
            if (pageSize is null)
            {
                pageSize = defaultPageSize;
               
            }
            if (pageNumber is null)
            {
                pageNumber = defaultIndex;
            }
            if (orderBy is null)
            {
                orderBy = defaultOrderBy;
            }
            var search_Model_Type = "custom";
            //var search_Model_Type = "default";

            var model = new SearchModel();

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
            var old_i=0;
            var new_i=0;

            //For Homepage with deafult paging and _selector
            /*
            Page Index
            var old_i = model.CatalogProductsModel.PageSizeOptions.ToList().FindIndex(p => p.Selected);
            var new_i = model.CatalogProductsModel.PageSizeOptions.ToList().FindIndex(p => p.Value==pageSize);

            if (old_i==-1 || new_i==-1)
            {
                old_i = Int32.Parse(defaultIndex);
                new_i = Int32.Parse(defaultIndex);
                pageSize = model.CatalogProductsModel.PageSizeOptions[Int32.Parse(defaultIndex)].Value;
            }
            model.CatalogProductsModel.PageSizeOptions[old_i].Selected = false;
            model.CatalogProductsModel.PageSizeOptions[new_i].Selected = true;
            */

            //Sorting
            old_i = model.CatalogProductsModel.AvailableSortOptions.ToList().FindIndex(p => p.Selected);
            new_i = model.CatalogProductsModel.AvailableSortOptions.ToList().FindIndex(p => p.Value == orderBy);
            if (old_i == -1 || new_i == -1)
            {
                old_i = Int32.Parse(defaultOrderBy);
                new_i = Int32.Parse(defaultOrderBy);
                orderBy = model.CatalogProductsModel.AvailableSortOptions[Int32.Parse(defaultOrderBy)].Value;
            }
            model.CatalogProductsModel.AvailableSortOptions[old_i].Selected = false;
            model.CatalogProductsModel.AvailableSortOptions[new_i].Selected = true;


            //Update Command
            command.PageSize = Convert.ToInt32(pageSize);
            command.PageNumber = Convert.ToInt32(pageNumber) - 1;
            command.OrderBy = Convert.ToInt32(orderBy);

            var modelList = (await _productModelFactory.PrepareProductOverviewModelsAsync1(products, true, true, productThumbPictureSize, discounts: discProducts,command: command));
            model.CatalogProductsModel.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(modelList.Where(v => !v.MarkAsNew))).ToList();

            var modelList1 = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, productThumbPictureSize, discounts: discProducts));
            model.CatalogProductsModel.Promos = modelList1.Where(v => v.MarkAsNew).ToList();

            //For Homepage with only Show less and Show More buttons
            model.CatalogProductsModel.Increment = defaultIncrement;
            model.CatalogProductsModel.StartingProducts = defaultPageSize;
            model.CatalogProductsModel.MinimumProducts = defaultMinimumProducts;
            model.CatalogProductsModel.MaxProducts = products.Count;
            model.CatalogProductsModel.LoadPagedList(modelList);
            return View(model);
        }
    }   
}