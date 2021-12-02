using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Tvt.Plugin.DiscountRules.MinSubtotal.Infrastructure.Events
{
    public class ShoppingCartEvent : IConsumer<EntityInsertedEvent<ShoppingCartItem>>, IConsumer<EntityUpdatedEvent<ShoppingCartItem>>
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IDiscountService _discountService;
        private readonly INotificationService _notificationService;
        private readonly ILogger _logger;
        private readonly ICustomerService _customerService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISettingService _settingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;
        public ShoppingCartEvent(IWorkContext workContext,
          IStoreContext storeContext,
          IDiscountService discountService,
          INotificationService notificationService,
          ILogger logger,
          ICustomerService customerService,
          IHttpContextAccessor httpContextAccessor,
          IHttpClientFactory httpClientFactory,
          ISettingService settingService,
          IShoppingCartService shoppingCartService,
          IProductService productService)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _discountService = discountService;
            _notificationService = notificationService;
            _logger = logger;
            _customerService = customerService;
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
            _settingService = settingService;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
        }
        public async Task HandleEventAsync(EntityInsertedEvent<ShoppingCartItem> eventMessage)
        {

            var requirementSettings = (await _settingService.GetAllSettingsAsync()).AsQueryable();
            requirementSettings = requirementSettings.Where(setting => setting.Name.ToLowerInvariant().Contains(DiscountRequirementDefaults.SystemName.ToLowerInvariant()));
            
            if (requirementSettings != null)
            {
                var discountRequirementIds = new List<int>();
                var discountProducts = new List<Product>();
                var allDiscountProducts = new List<Product>();
                foreach (var requirementSetting in requirementSettings.OrderByDescending(x => x.Value))
                {
                    var discountRequirementAll =await _discountService.GetDiscountRequirementByIdAsync(Convert.ToInt32(requirementSetting.Name.Substring(requirementSetting.Name.LastIndexOf('-') + 1)));
                    if (discountRequirementAll != null)
                    {

                        int discountId = discountRequirementAll.DiscountId;
                        //get products with applied discount
                        allDiscountProducts.AddRange((await _productService.GetProductsWithAppliedDiscountAsync(discountId: discountId,
                           showHidden: false,
                           pageIndex: 0, pageSize: 99999)).ToList());
                    }
                }
                    foreach (var requirementSetting in requirementSettings.OrderByDescending(x => x.Value))
                {
                    decimal subTotalWithoutDiscount = decimal.Zero;

                    var discountRequirement =await _discountService.GetDiscountRequirementByIdAsync(Convert.ToInt32(requirementSetting.Name.Substring(requirementSetting.Name.LastIndexOf('-') + 1)));
                    if (discountRequirement != null)
                    {

                        int discountId = discountRequirement.DiscountId;
                        //get products with applied discount
                        discountProducts = (await _productService.GetProductsWithAppliedDiscountAsync(discountId: discountId,
                           showHidden: false,
                           pageIndex: 0, pageSize: 99999)).ToList();

                        if (allDiscountProducts.Select(x => x.Id).Contains(eventMessage.Entity.ProductId))
                            return;
                        //result is valid if the customer belongs to the restricted role
                        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart,(await _storeContext.GetCurrentStoreAsync()).Id);


                        foreach (var item in cart)
                        {
                            var(scSubTotal, discountAmount, scDiscounts, _) = await _shoppingCartService.GetSubTotalAsync(item, true);
                            subTotalWithoutDiscount += scSubTotal;

                            //subTotalWithoutDiscount -= shoppingCartItemDiscountBase;
                        }

                       


                        if (subTotalWithoutDiscount >= Convert.ToDecimal(requirementSetting.Value))
                        {
                            foreach (var promoproduct in discountProducts)
                            {
                                if (promoproduct.StockQuantity > 0)
                                {
                                    var shoppingCartItem =await  _shoppingCartService.FindShoppingCartItemInTheCartAsync(cart, ShoppingCartType.ShoppingCart, promoproduct);
                                    if (shoppingCartItem == null)
                                    {
                                      await  _shoppingCartService.AddToCartAsync(await _workContext.GetCurrentCustomerAsync(), promoproduct, ShoppingCartType.ShoppingCart,(await _storeContext.GetCurrentStoreAsync()).Id);
                                        break;
                                    }
                                    else
                                    {
                                        await _shoppingCartService.DeleteShoppingCartItemAsync(shoppingCartItem.Id);
                                    }
                                }
                            }
                            break;
                        }

                    }
                }
            }
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ShoppingCartItem> eventMessage)
        {
            var requirementSettings =(await _settingService.GetAllSettingsAsync()).AsQueryable();
            requirementSettings = requirementSettings.Where(setting => setting.Name.ToLowerInvariant().Contains(DiscountRequirementDefaults.SystemName.ToLowerInvariant()));

            if (requirementSettings != null)
            {
                var discountRequirementIds = new List<int>();
                var discountProducts = new List<Product>();
                var allDiscountProducts = new List<Product>();
                foreach (var requirementSetting in requirementSettings.OrderByDescending(x => x.Value))
                {
                    var discountRequirementAll =await _discountService.GetDiscountRequirementByIdAsync(Convert.ToInt32(requirementSetting.Name.Substring(requirementSetting.Name.LastIndexOf('-') + 1)));
                    if (discountRequirementAll != null)
                    {

                        int discountId = discountRequirementAll.DiscountId;
                        //get products with applied discount
                        allDiscountProducts.AddRange((await _productService.GetProductsWithAppliedDiscountAsync(discountId: discountId,
                           showHidden: false,
                           pageIndex: 0, pageSize: 99999)).ToList());
                    }
                }
                foreach (var requirementSetting in requirementSettings.OrderByDescending(x => x.Value))
                {
                    decimal subTotalWithoutDiscount = decimal.Zero;
                    var discountRequirement =await _discountService.GetDiscountRequirementByIdAsync(Convert.ToInt32(requirementSetting.Name.Substring(requirementSetting.Name.LastIndexOf('-') + 1)));
                    if (discountRequirement != null)
                    {

                        int discountId = discountRequirement.DiscountId;
                        //get products with applied discount
                        discountProducts =(await _productService.GetProductsWithAppliedDiscountAsync(discountId: discountId,
                           showHidden: false,
                           pageIndex: 0, pageSize: 99999)).ToList();

                        if (allDiscountProducts.Select(x => x.Id).Contains(eventMessage.Entity.ProductId))
                            return;
                        //result is valid if the customer belongs to the restricted role
                        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart,(await _storeContext.GetCurrentStoreAsync()).Id);


                        foreach (var item in cart)
                        {
                            var(scSubTotal, discountAmount, scDiscounts, _)=await _shoppingCartService.GetSubTotalAsync(item, true);
                            subTotalWithoutDiscount += scSubTotal;
                        }




                        if (subTotalWithoutDiscount >= Convert.ToDecimal(requirementSetting.Value))
                        {
                            foreach (var promoproduct in discountProducts)
                            {
                                if (promoproduct.StockQuantity > 0)
                                {
                                    var shoppingCartItem = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(cart, ShoppingCartType.ShoppingCart, promoproduct);
                                    if (shoppingCartItem == null)
                                    {
                                        await _shoppingCartService.AddToCartAsync(await _workContext.GetCurrentCustomerAsync(), promoproduct, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
                                        break;
                                    }
                                    else
                                    {
                                        await _shoppingCartService.DeleteShoppingCartItemAsync(shoppingCartItem.Id);
                                    }
                                }
                            }
                            break;
                        }

                    }
                }
            }
        }

      
    }
}
