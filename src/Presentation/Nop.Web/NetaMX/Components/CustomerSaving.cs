using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using System;
using System.Threading.Tasks;

namespace Nop.Web.NetaMX.Components
{
    public class CustomerSaving : NopViewComponent
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        public CustomerSaving(IShoppingCartService shoppingCartService,
            IWorkContext workContext, IStoreContext storeContext,
            IProductService productService)
        {
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _storeContext = storeContext;
            _productService = productService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            var totalSaving = decimal.Zero;
            foreach (var sci in cart)
            {
                var product = await _productService.GetProductByIdAsync(sci.ProductId);
                var productSaving = (product.OldPrice - product.Price) * sci.Quantity;
                totalSaving += productSaving;
                totalSaving = Math.Round(totalSaving, 2);
            }
            return View(totalSaving);
        }
    }
}
