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
                string info = "Id,Name,Sku,Old Price,Price\n";

                foreach(var p in promoErrors)
                {
                    info = info + p.Id + "," + p.Name + "," + p.Sku + "," + p.Price + "," + p.OldPrice + Environment.NewLine;
                }


                List<string> correos = new List<string>() {
                    //"andres.posada@neta.mx",
                    //"paulina@neta.mx",
                    //"samuel.wong@neta.mx",
                    //"diana@neta.mx",
                    "miguel.zamora@neta.mx",
                    "bicho@neta.mx",
                    "david.gomes@neta.mx"
                };

                var email = new Email();
                email.SetEmailOrigen("redash.server.netamx@gmail.com", "sht5$29.!");
                email.SetSubject("Alerta: Productos con errores en promociones");
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
