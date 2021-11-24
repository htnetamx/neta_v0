using Nop.Core.Domain.Promotion;
using Nop.Services.Catalog;
using Nop.Services.Promotion;
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

        public NetaPromotionModelFactory(INetaPromotionService netaPromotionService,
            IProductService productService)
        {
            _netaPromotionService = netaPromotionService;
            _productService = productService;
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
    }
}
