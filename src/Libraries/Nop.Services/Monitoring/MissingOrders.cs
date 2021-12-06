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
    public partial class MissingOrders : IScheduleTask
    {
        #region Fields

        private readonly IStoreService _storeService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public MissingOrders(IStoreService storeService, IOrderService orderService, IProductService productService, ILogger logger)
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

            var errors_missing_orders = await _orderService.GetErrorNoSalesAsync();
            if (errors_missing_orders != "OK")
            {
                List<string> correos = new List<string>() {
                    //"andres.posada@neta.mx",
                    //"enrique.roman@neta.mx",
                    //"diana@neta.mx",
                    //"nicolas@neta.mx",
                    "miguel.zamora@neta.mx",
                    "bicho@neta.mx",
                    "david.gomes@neta.mx",
                    "nicolas@neta.mx",
                    "samuel@neta.mx",
                    "jorge.garduno@neta.mx",
                    "mateo.uribe@neta.mx",
                    "aurea.lopez@neta.mx",
                    "samuel.wong@neta.mx"

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
