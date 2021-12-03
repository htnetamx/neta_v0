using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Promotion;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Nop.Services.Promotion
{
    public partial interface INetaPromotionService
    {
        Task InsertNetaPromotionAsync(Neta_Promotion netaPromotion);
        Task UpdateNetaPromotionAsync(Neta_Promotion netaPromotion);
        Task DeleteNetaPromotionAsync(Neta_Promotion netaPromotion);
        Task<Neta_Promotion> GetNetaPromotionByIdAsync(int id);
        Task<IList<Neta_Promotion>> GetAllNetaPromotionAsync();
        Task<IPagedList<Neta_Promotion>> GetAllPagedNetaPromotionAsync(int pageIndex = 0, int pageSize = int.MaxValue);
        Task<IList<Product>> GetPromotionProductsByPromotionId(int promotionId);
        Task DeletePromotionProductsByPromotionId(int id);
        Task<IPagedList<Neta_Promotion_ProductMapping>> GetPromotionProductsByPromotionId(int promotionId, int pageIndex = 0, int pageSize = int.MaxValue - 1);
        Task<Neta_Promotion_ProductMapping> GetPromotionProductById(int id);
        Task UpdatePromotionProductAsync(Neta_Promotion_ProductMapping neta_Promotion_ProductMapping);
        Task InsertPromotionProductAsync(Neta_Promotion_ProductMapping neta_Promotion_ProductMapping,int? discountId);
        Task DeletePromotionProductAsync(Neta_Promotion_ProductMapping neta_Promotion_ProductMapping);
        Task<IPagedList<Neta_Promotion_ProductMapping>> GetPromotionsProductsByPromotionIdAsync(int promotionId,
            int pageIndex = 0, int pageSize = int.MaxValue);
        Neta_Promotion_ProductMapping FindProductPromotion(IList<Neta_Promotion_ProductMapping> source, int productId, int promotionId);

        Task ImportProductsFromXlsxAsync(Stream stream, int promotionId);
    }
}
