using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Tasks;

namespace Nop.Services.Catalog
{
    public partial class ProductPriceUpdaterService : IScheduleTask
    {

        #region Fields

        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public ProductPriceUpdaterService(IProductService productService)
        {
            _productService = productService;
        }

        #endregion

        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            if (DateTime.UtcNow.Hour != 5)
                return;

            var products = (await _productService.GetAllProductsDisplayedOnHomepageAsync())
                .Where(p => p.MarkAsNew && p.AvailableEndDateTimeUtc.Value.Date <= DateTime.UtcNow.Date);
            foreach (var product in products)
            {
                if (product.OldPrice != product.Price)
                {
                    product.Price = product.OldPrice;
                    await _productService.UpdateProductAsync(product);
                }
            }
        }
    }
}
