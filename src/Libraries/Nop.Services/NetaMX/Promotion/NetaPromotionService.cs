using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Promotion;
using Nop.Data;
using Nop.Services.Catalog;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Services.Promotion
{
    public partial class NetaPromotionService : INetaPromotionService
    {
        #region Fields
        private readonly IRepository<Neta_Promotion> _netaPromotionRepository;
        private readonly IRepository<Neta_Promotion_ProductMapping> _netaPromotionProductMappingRepository;
        protected readonly IRepository<Product> _productRepository;
        private readonly IProductService _productService;
        protected readonly IRepository<DiscountProductMapping> _discountProductMappingRepository;
        #endregion

        #region Ctor
        public NetaPromotionService(IRepository<Neta_Promotion> netaPromotionRepository, IRepository<Neta_Promotion_ProductMapping> netaPromotionProductMappingRepository,
            IRepository<Product> productRepository, IProductService productService,
            IRepository<DiscountProductMapping> discountProductMappingRepository)
        {
            _netaPromotionRepository = netaPromotionRepository;
            _netaPromotionProductMappingRepository = netaPromotionProductMappingRepository;
            _productRepository = productRepository;
            _productService = productService;
            _discountProductMappingRepository = discountProductMappingRepository;
        }
        #endregion

        #region Methods
        public async Task InsertNetaPromotionAsync(Neta_Promotion netaPromotion)
        {
            await _netaPromotionRepository.InsertAsync(netaPromotion);
        }

        public async Task UpdateNetaPromotionAsync(Neta_Promotion netaPromotion)
        {
            await _netaPromotionRepository.UpdateAsync(netaPromotion);
        }

        public async Task DeleteNetaPromotionAsync(Neta_Promotion netaPromotion)
        {
            netaPromotion.Deleted = true;
            await _netaPromotionRepository.UpdateAsync(netaPromotion);
        }

        public async Task<Neta_Promotion> GetNetaPromotionByIdAsync(int id)
        {
            if (id == 0)
                return null;

            return await _netaPromotionRepository.GetByIdAsync(id);
        }

        public async Task<IList<Neta_Promotion>> GetAllNetaPromotionAsync()
        {
            var result = await _netaPromotionRepository.GetAllAsync(query =>
            {
                return from p in query
                       where !p.Deleted
                       select p;
            });
            return result;
        }

        public async Task<IPagedList<Neta_Promotion>> GetAllPagedNetaPromotionAsync(int pageIndex = 0, int pageSize = int.MaxValue - 1)
        {
            var result = await _netaPromotionRepository.GetAllPagedAsync(query =>
            {
                return from p in query
                       where !p.Deleted
                       select p;
            }, pageIndex, pageSize);
            return result;
        }

        public async Task DeletePromotionProductsByPromotionId(int id)
        {
            var promotion = await _netaPromotionRepository.GetByIdAsync(id);
            var promotionmapping = _netaPromotionProductMappingRepository.Table.Where(x => x.Neta_PromotionId == id).ToList();
            if (promotionmapping.Count > 0)
            {
                foreach (var item in promotionmapping)
                {
                    var product = _productRepository.Table.Where(x => x.Id == item.ProductId).FirstOrDefault();
                    if (product != null)
                    {
                        var productpromotionmappingcount = _netaPromotionProductMappingRepository.Table.Where(x => x.ProductId == product.Id).Count();
                        if (productpromotionmappingcount == 1)
                        {
                            product.IsPromotionProduct = false;
                            await _productRepository.UpdateAsync(product);
                        }
                    }
                }
                await _netaPromotionProductMappingRepository.DeleteAsync(promotionmapping);
            }
            if (promotion != null)
            {
                promotion.Published = false;
                await _netaPromotionRepository.UpdateAsync(promotion);
            }
        }

        public async Task<IList<Product>> GetPromotionProductsByPromotionId(int promotionId)
        {
            var netaPromotionProductMapping = _netaPromotionProductMappingRepository.Table;

            var products = await _productRepository.GetAllAsync(query =>
            {
                return from p in query
                       join np in netaPromotionProductMapping on p.Id equals np.ProductId
                       where p.Published &&
                             !p.Deleted &&
                             np.Neta_PromotionId == promotionId
                       select p;
            });

            return products.OrderBy(v => v.DisplayOrder).ToList();
        }

        public async Task<IPagedList<Neta_Promotion_ProductMapping>> GetPromotionProductsByPromotionId(int promotionId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var result = await _netaPromotionProductMappingRepository.GetAllPagedAsync(query =>
            {
                return from p in query
                       where p.Neta_PromotionId == promotionId
                       select p;
            }, pageIndex, pageSize);
            return result;
        }

        public async Task<Neta_Promotion_ProductMapping> GetPromotionProductById(int id)
        {
            return await _netaPromotionProductMappingRepository.GetByIdAsync(id);
        }

        public async Task UpdatePromotionProductAsync(Neta_Promotion_ProductMapping neta_Promotion_ProductMapping)
        {
            await _netaPromotionProductMappingRepository.UpdateAsync(neta_Promotion_ProductMapping);
            var product = _productRepository.Table.FirstOrDefault(x => x.Id == neta_Promotion_ProductMapping.ProductId);
            await UpdateIsPromotionProductApplyAsync(product, neta_Promotion_ProductMapping.AllowToShowProductOnlyPromotion);
        }

        public async Task InsertPromotionProductAsync(Neta_Promotion_ProductMapping neta_Promotion_ProductMapping, int? discountId)
        {
            await _netaPromotionProductMappingRepository.InsertAsync(neta_Promotion_ProductMapping);

            int descId = discountId ?? 0;
            if (descId > 0)
            {
                await _productService.InsertDiscountProductMappingAsync(new DiscountProductMapping { EntityId = neta_Promotion_ProductMapping.ProductId, DiscountId = descId });
            }

            var product = _productRepository.Table.FirstOrDefault(x => x.Id == neta_Promotion_ProductMapping.ProductId);
            await UpdateIsPromotionProductApplyAsync(product,neta_Promotion_ProductMapping.AllowToShowProductOnlyPromotion);
        }

        public async Task DeletePromotionProductAsync(Neta_Promotion_ProductMapping neta_Promotion_ProductMapping)
        {
            await _netaPromotionProductMappingRepository.DeleteAsync(neta_Promotion_ProductMapping);

            var product = _productRepository.Table.Where(x => x.Id == neta_Promotion_ProductMapping.ProductId).FirstOrDefault();

            await UpdateIsPromotionProductApplyAsync(product, true);
        }

        public virtual async Task UpdateIsPromotionProductApplyAsync(Product product,bool AllowToShowProductOnlyPromotion)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (AllowToShowProductOnlyPromotion)
                product.IsPromotionProduct = _netaPromotionProductMappingRepository.Table.Any(p => p.ProductId == product.Id);

            product.HasDiscountsApplied = _discountProductMappingRepository.Table.Any(dpm => dpm.EntityId == product.Id);
            await _productService.UpdateProductAsync(product);
        }

        public virtual async Task<IPagedList<Neta_Promotion_ProductMapping>> GetPromotionsProductsByPromotionIdAsync(int promotionId,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (promotionId == 0)
                return new PagedList<Neta_Promotion_ProductMapping>(new List<Neta_Promotion_ProductMapping>(), pageIndex, pageSize);

            var query = from pc in _netaPromotionProductMappingRepository.Table
                        join p in _productRepository.Table on pc.ProductId equals p.Id
                        where pc.Neta_PromotionId == promotionId
                        orderby pc.DisplayOrder, pc.Id
                        select pc;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public virtual Neta_Promotion_ProductMapping FindProductPromotion(IList<Neta_Promotion_ProductMapping> source, int productId, int promotionId)
        {
            foreach (var productPromotion in source)
                if (productPromotion.ProductId == productId && productPromotion.Neta_PromotionId == promotionId)
                    return productPromotion;

            return null;
        }

        /// <summary>
        /// Import products from XLSX file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task ImportProductsFromXlsxAsync(Stream stream, int promotionId)
        {
            using var xlPackage = new ExcelPackage(stream);
            // get the first worksheet in the workbook
            var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new NopException("No worksheet found");

            var rowCount = worksheet.Dimension.Rows;
            var columnsCount = worksheet.Dimension.Columns;
            bool checkValidHeader = true;
            var promotion = await _netaPromotionRepository.GetByIdAsync(promotionId);
            if (columnsCount > 0)
            {
                if (worksheet.Cells[1, 1].Value.ToString().ToLower().Trim() != "sku")
                {
                    checkValidHeader = false;
                }
            }
            else
                checkValidHeader = false;

            if (checkValidHeader && rowCount > 1)
            {
                for (int row = 2; row <= rowCount; row++)
                {
                    if (worksheet.Cells[row, 1].Value != null)
                    {
                        var sku = worksheet.Cells[row, 1].Value.ToString().Trim();

                        if (!string.IsNullOrEmpty(sku))
                        {
                            var product = await _productService.GetProductBySkuAsync(sku);

                            if (product != null && promotion != null)
                            {
                                var isSuccess = false;
                                var IsExist = _netaPromotionProductMappingRepository.Table.Where(x => x.Neta_PromotionId == promotionId && x.ProductId == product.Id).Count();
                                if (IsExist == 0)
                                {
                                    Neta_Promotion_ProductMapping netaPromotionProductMapping = new Neta_Promotion_ProductMapping();
                                    netaPromotionProductMapping.Neta_PromotionId = promotionId;
                                    netaPromotionProductMapping.ProductId = product.Id;
                                    netaPromotionProductMapping.DisplayOrder = 1;
                                    netaPromotionProductMapping.AllowToShowProductOnlyPromotion = true;

                                    await InsertPromotionProductAsync(netaPromotionProductMapping, promotion.DiscountId);
                                    isSuccess = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
