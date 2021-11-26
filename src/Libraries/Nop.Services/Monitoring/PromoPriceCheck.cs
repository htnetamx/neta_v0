using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Tasks;
using Nop.Services.Catalog;

namespace Nop.Services.Monitoring
{
    public partial class PromoPriceCheck : IScheduleTask
    {
        #region Fields

        private readonly IStoreService _storeService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public PromoPriceCheck(IStoreService storeService, IOrderService orderService, IProductService productService, ILogger logger)
        {
            _storeService = storeService;
            _orderService = orderService;
            _productService= productService;
            _logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public async System.Threading.Tasks.Task ExecuteAsync()
        {


            var products = await _productService.GetAllProductsAsync();

            var promoErrors = products.Where(p => (p.OldPrice < p.Price) && p.OldPrice!=0);

            if(promoErrors.Count()>0)
            {
                //Hay Errores en las promociones de los productos
                var alerta = "Alerta";
            }
        }

        #endregion
    }
}
