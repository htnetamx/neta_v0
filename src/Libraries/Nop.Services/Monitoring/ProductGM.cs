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
using Nop.Services.Mailing;


namespace Nop.Services.Monitoring
{
    public partial class ProductGM : IScheduleTask
    {
        #region Fields

        private readonly IStoreService _storeService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public ProductGM(IStoreService storeService, IOrderService orderService, IProductService productService, ILogger logger)
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
            var email = new Email();
            email.SetEmailOrigen("redash.server.netamx@gmail.com", "sht5$29.!");

            var errors_GM_products = await _orderService.GetErrorsGMProductsFromOrdersAsync();

            if (errors_GM_products.Count()>0)
            {
                string info = "Order Id,Product Id,Name,Sku,Cost,Price" + Environment.NewLine;

                foreach (var error in errors_GM_products)
                {
                    info = info + error.OrderId + "," + error.ProductId + "," + error.Name + ","+ error.Sku + "," + error.Cost + "," + error.Price + Environment.NewLine;
                }

                List<string> correos = new List<string>() {
                    "andres.posada@neta.mx",
                    //"paulina@neta.mx",
                    //"samuel.wong@neta.mx",
                    //"enrique.roman@neta.mx",
                    //"diana@neta.mx",
                    "miguel.zamora@neta.mx"
                };

                email.SetSubject("Alerta: Productos Con GM Negativo");
                email.SetBody(info);

                foreach (var correo in correos)
                {
                    email.SetEmailDestino(correo);
                    email.Send();
                }
            }
        }

        #endregion
    }
}
