using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Plugins;

namespace Tvt.Plugin.DiscountRules.MinSubtotal
{
   public partial class MinSubtotalDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
        #region Fields

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWebHelper _webHelper;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IProductService _productService;
        #endregion

        #region Ctor

        public MinSubtotalDiscountRequirementRule(IActionContextAccessor actionContextAccessor,
            IDiscountService discountService,
            ICustomerService customerService,
            ILocalizationService localizationService,
            ISettingService settingService,
            IUrlHelperFactory urlHelperFactory,
            IWebHelper webHelper,
            IShoppingCartService shoppingCartService,
         IStoreContext storeContext,
        IWorkContext workContext,
        IOrderTotalCalculationService orderTotalCalculationService,
        IProductService productService)
        {
            _actionContextAccessor = actionContextAccessor;
            _customerService = customerService;
            _discountService = discountService;
            _localizationService = localizationService;
            _settingService = settingService;
            _urlHelperFactory = urlHelperFactory;
            _webHelper = webHelper;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _workContext = workContext;
            _orderTotalCalculationService = orderTotalCalculationService;
            _productService = productService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>Result</returns>
        public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //invalid by default
            var result = new DiscountRequirementValidationResult();

            if (request.Customer == null)
                return result;

            //try to get saved restricted customer role identifier
            var minSubtotal = await _settingService.GetSettingByKeyAsync<int>(string.Format(DiscountRequirementDefaults.SettingsKey, request.DiscountRequirementId));
            if (minSubtotal == 0)
                return result;

            //result is valid if the customer belongs to the restricted role
            var cart =await  _shoppingCartService.GetShoppingCartAsync(request.Customer, ShoppingCartType.ShoppingCart,(await _storeContext.GetCurrentStoreAsync()).Id);
            decimal subTotalWithoutDiscount = decimal.Zero;
   

            var requirementSettings =(await  _settingService.GetAllSettingsAsync()).AsQueryable();
            requirementSettings = requirementSettings.Where(setting => setting.Name.ToLowerInvariant().Contains(DiscountRequirementDefaults.SystemName.ToLowerInvariant()));
        
            var allDiscountProducts = new List<Product>();

            if (requirementSettings != null)
            {
              
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
            }

            int regalos = 0;
            foreach (var item in cart) {
                if (allDiscountProducts.Any() && allDiscountProducts.Select(x => x.Id).Contains(item.ProductId)) {
                    regalos++;
                }
                else
                {
                    var (scSubTotal, discountAmount, scDiscounts, _) = await _shoppingCartService.GetSubTotalAsync(item, true);
                    subTotalWithoutDiscount += scSubTotal;
                }

            }
            if(subTotalWithoutDiscount==decimal.Zero)
                return result;


            result.IsValid = subTotalWithoutDiscount >= minSubtotal && regalos<=1;

            return result;
        }

       

        /// <summary>
        /// Get URL for rule configuration
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
        /// <returns>URL</returns>
        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("Configure", "DiscountRulesMinSubtotal",
                new { discountId = discountId, discountRequirementId = discountRequirementId }, _webHelper.GetCurrentRequestProtocol());
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override async Task InstallAsync()
        {
            //locales
            await _localizationService.AddLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.DiscountRules.MinSubtotal.Fields.CustomerRole"] = "Required min Subtotal",
                ["Plugins.DiscountRules.MinSubtotal.Fields.CustomerRole.Hint"] = "Discount will be applied if customer is in the selected customer role.",
                ["Plugins.DiscountRules.MinSubtotal.Fields.CustomerRole.Select"] = "Select customer role",
                ["Plugins.DiscountRules.MinSubtotal.Fields.CustomerRoleId.Required"] = "Min subtotal is required",
                ["Plugins.DiscountRules.MinSubtotal.Fields.DiscountId.Required"] = "Discount is required"
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //discount requirements
            var discountRequirements = (await _discountService.GetAllDiscountRequirementsAsync())
                .Where(discountRequirement => discountRequirement.DiscountRequirementRuleSystemName == DiscountRequirementDefaults.SystemName);
            foreach (var discountRequirement in discountRequirements)
            {
               await _discountService.DeleteDiscountRequirementAsync(discountRequirement, false);
            }

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.DiscountRules.MinSubtotal");

            await base.UninstallAsync();
        }

        #endregion
    }
}