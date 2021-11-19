using Nop.Core.Domain.Promotion;
using Nop.Web.Areas.Admin.Models.Promotion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    public partial interface INetaPromotionModelFactory
    {
        Task<NetaPromotionModel> PrepareNetaPromotionModelAsync(NetaPromotionModel model, Neta_Promotion netaPromotion);
        Task<PromotionProductListModel> PreparePromotionProductListModelAsync(PromotionProductSearchModel searchModel, Neta_Promotion neta_Promotion);
    }
}
