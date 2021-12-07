using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Promotion;
using Nop.Services.Catalog;
using Nop.Services.Discounts;
using Nop.Services.Promotion;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Promotion;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    public partial class NetaPromotionModelFactory : INetaPromotionModelFactory
    {
        private readonly INetaPromotionService _netaPromotionService;
        private readonly IProductService _productService; 
        private readonly IDiscountService _discountService;
        private readonly IUrlRecordService _urlRecordService;

        public NetaPromotionModelFactory(INetaPromotionService netaPromotionService,
            IProductService productService, IDiscountService discountService,IUrlRecordService urlRecordService)
        {
            _netaPromotionService = netaPromotionService;
            _productService = productService;
            _discountService = discountService;
            _urlRecordService = urlRecordService;
        }
        public async Task<NetaPromotionModel> PrepareNetaPromotionModelAsync(NetaPromotionModel model, Neta_Promotion netaPromotion)
        {
            if(netaPromotion != null)
            {
                model ??= new NetaPromotionModel();
                model.Id = netaPromotion.Id;
                model.Name = netaPromotion.Name;
                model.StartDateUtc = netaPromotion.StartDateUtc;
                model.EndDateUtc = netaPromotion.EndDateUtc;
                model.PictureId = netaPromotion.PictureId;
                model.PromotionProductSearchModel.PromotionId = netaPromotion.Id;
                model.DiscountId = netaPromotion.DiscountId ?? 0;
                if (model.DiscountId > 0)
                {
                    var discount = await _discountService.GetDiscountByIdAsync(model.DiscountId);
                    if (discount != null)
                    {
                        model.UsePercentage = discount.UsePercentage;
                        model.DiscountPercentage = discount.DiscountPercentage;
                        model.DiscountAmount = discount.DiscountAmount;
                        model.MaximumDiscountAmount = discount.MaximumDiscountAmount;
                    }
                }
            }
            return model;
        }

        public virtual async Task<PromotionProductListModel> PreparePromotionProductListModelAsync(PromotionProductSearchModel searchModel, Neta_Promotion neta_Promotion)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (neta_Promotion == null)
                throw new ArgumentNullException(nameof(neta_Promotion));

            //get product categories
            var promotionProducts = await _netaPromotionService.GetPromotionProductsByPromotionId(neta_Promotion.Id, searchModel.Page - 1, searchModel.PageSize);

            //prepare grid model
            var model = await new PromotionProductListModel().PrepareToGridAsync(searchModel, promotionProducts, () =>
            {
                return promotionProducts.SelectAwait(async promotionProduct =>
                {
                    //fill in model values from the entity
                    var promotionProductModel = new PromotionProductModel() 
                    {
                        Id = promotionProduct.Id,
                        ProductId = promotionProduct.ProductId,
                        DisplayOrder = promotionProduct.DisplayOrder,
                        AllowToShowProductOnlyPromotion = promotionProduct.AllowToShowProductOnlyPromotion
                    };

                    //fill in additional values (not existing in the entity)
                    promotionProductModel.ProductName = (await _productService.GetProductByIdAsync(promotionProduct.ProductId))?.Name;

                    return promotionProductModel;
                });
            });

            return model;
        }

        public virtual async Task<AddProductToCategoryListModel> PrepareAddProductToCategoryListModelAsync(AddProductToCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get products
            var products = await _productService.SearchProductsAsync(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                searchSKUString: searchModel.SearchSKU,IsPromotionProductExist:true);

            //prepare grid model
            var model = await new AddProductToCategoryListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                return products.SelectAwait(async product =>
                {
                    var productModel = product.ToModel<ProductModel>();

                    productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);

                    return productModel;
                });
            });

            return model;
        }
    }
}
