using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
    public partial interface IProductService
    {
        Task<IList<Product>> GetAllProductsDisplayedOnHomepageWithMarkAsNewAsync();

        Task<IPagedList<Product>> GetProductsDisplayedOnHomepageAsync(int pageIndex = 0, int pageSize = int.MaxValue, int storeDisplayOrder = 0);
    }
    public partial class ProductService
    {
        public virtual async Task<IList<Product>> GetAllProductsDisplayedOnHomepageWithMarkAsNewAsync()
        {
            var products = await _productRepository.GetAllAsync(query =>
            {
                return from p in query
                       orderby p.DisplayOrder, p.Id
                       where p.Published &&
                             !p.Deleted &&
                             p.ShowOnHomepage && 
                             p.MarkAsNew &&
                             !p.IsPromotionProduct
                       select p;
            }, cache => cache.PrepareKeyForDefaultCache(NopCatalogDefaults.ProductsMarkAsNewHomepageCacheKey));

            return products.OrderBy(v => v.DisplayOrder).ToList();
        }

        public virtual async Task<IPagedList<Product>> GetProductsDisplayedOnHomepageAsync(int pageIndex = 0,int pageSize = 30,int storeDisplayOrder = 0)
        {
            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var productsQuery = (from p in _productRepository.Table
                                 orderby p.DisplayOrder, p.Id
                                 where p.Published &&
                                       !p.Deleted &&
                                       !p.MarkAsNew &&
                                       p.ShowOnHomepage &&
                                      p.VisibleIndividually &&
                                      !p.IsPromotionProduct
                                select p);

            var currentUser = (await _workContext.GetCurrentCustomerAsync()).Username;
            if (!string.IsNullOrEmpty(currentUser))
                currentUser = currentUser.ToLower();
            
            if (storeDisplayOrder == 1)
            {
                productsQuery = productsQuery.Where(v => /*v.Sku.EndsWith("LH") ||*/ v.Sku.ToUpper().EndsWith("L1"));
            }
            else if (storeDisplayOrder == 2)
            {
                productsQuery = productsQuery.Where(v => !(v.Sku.ToUpper().EndsWith("LH") || v.Sku.ToUpper().EndsWith("L1")));
            }

            return await productsQuery.ToPagedListAsync(pageIndex, pageSize);

        }
    }
}
