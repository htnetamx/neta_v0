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
    public partial class AnalisisTotales : IScheduleTask
    {
        #region Fields

        private readonly IStoreService _storeService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public AnalisisTotales(IStoreService storeService, IOrderService orderService, IProductService productService, ILogger logger)
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
            //Ventas anormales SKU la más fácil para mí es revisar margen,
            //casi siempre una venta anómala implica GM negativo. En esta query suelo revisar->GM negativo debería ser una alerta.
            //Ventas totales arriba->Ir > 1.5X vs. timestamp t-1 debería soltar alerta.Uso este query
            //Ventas totales debajo, me falta data por minutos -> 10 minutos sin venta me hace sentido

            var orders_products_cost = await _orderService.GetAllOrdersWithProductInfoLastDayAsync();
            var hey = "mio";

            //var saleErrors = orders_products_cos.Where(p => (p.OldPrice < p.Price) && p.OldPrice!=0);

            //if(saleErrors.Count()>0)
            //{
            //    //Hay Errores en las promociones de los productos
            //    var alerta = "Alerta";
            //}
        }

        #endregion
    }
}
