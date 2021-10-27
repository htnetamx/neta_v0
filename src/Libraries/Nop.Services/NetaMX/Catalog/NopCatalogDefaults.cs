using Nop.Core.Caching;

namespace Nop.Services.Catalog
{
    public partial class NopCatalogDefaults
    {
        /// <summary>
        /// Gets a key for caching
        /// </summary>
        public static CacheKey ProductsMarkAsNewHomepageCacheKey => new CacheKey("Nop.product.MarkAsNew.homepage.");
    }
}
