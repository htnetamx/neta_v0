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

            var errors_GM_products = await _orderService.GetErrorsGMProductsFromOrdersAsync();

            var email = new Email();
            email.SetEmailOrigen("redash.server.netamx@gmail.com", "sht5$29.!");
            
            

            if (errors_GM_products.Count()>0)
            {
                // Samuel Wong
                // Enrique 
                // Migue
                // Diana

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


            var errors_GMV = await _orderService.GetErrorFromGMVAsync();
            if (errors_GMV!="OK")
            {
                // Samuel Wong
                // Enrique 
                // Migue
                // Nico

                List<string> correos = new List<string>() {
                    "andres.posada@neta.mx",
                    //"samuel.wong@neta.mx",
                    //"enrique.roman@neta.mx",
                    //"diana@neta.mx",
                    //"nicolas@neta.mx",
                    "miguel.zamora@neta.mx"
                };

                email.SetSubject("Alerta: Error This Week's GMV vs Last Week");
                email.SetBody(errors_GMV);

                foreach (var correo in correos)
                {
                    email.SetEmailDestino(correo);
                    email.Send();
                }

                //alerta
                email.Send();
            }

            var errors_missing_orders = await _orderService.GetErrorNoSalesAsync();
            if (errors_missing_orders != "OK")
            {
                // Diana
                // Nico
                // Enrique
                // Migue

                List<string> correos = new List<string>() {
                    "andres.posada@neta.mx",
                    //"enrique.roman@neta.mx",
                    //"diana@neta.mx",
                    //"nicolas@neta.mx",
                    "miguel.zamora@neta.mx"
                };

                email.SetSubject("Alerta: No Orders In Time Lapse");
                email.SetBody(errors_missing_orders);

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
