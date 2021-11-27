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
using Nop.Core.Domain.Catalog;
using Nop.Services.Mailing;

namespace Nop.Services.Monitoring
{
    public partial class ProductNotAvailable : IScheduleTask
    {
        #region Fields

        private readonly IStoreService _storeService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public ProductNotAvailable(IStoreService storeService, IOrderService orderService, IProductService productService, ILogger logger)
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


            var products = (await _productService.GetAllProductsAsync()).Where(p=> p.Published && !p.Deprecated);

            var publishErrors = products.Where(p =>     p.Sku==null
                                                    ||  p.Sku==""
                                                    || !p.ShowOnHomepage
                                                    ||  p.MarkAsNew
                                                    ||  p.AvailableStartDateTimeUtc != null
                                                    ||  p.AvailableEndDateTimeUtc != null
                                                    ||  p.ManageInventoryMethodId !=1
                                                    ||  p.ManageInventoryMethod != ManageInventoryMethod.ManageStock
                                                    ||  p.BackorderModeId!=1
                                                    ||  p.BackorderMode != BackorderMode.AllowQtyBelow0
                                               );
            List<string> errors = new List<string>();
            foreach (var product in publishErrors)
            {
                if (product.Sku == null || product.Sku == "")
                {
                    errors.Add(product.Id + "," + product.Name + "," + product.Sku + "," + "Sku,"+ product.Sku +",No puede ser nulo ni vacío");
                }

                if (!product.ShowOnHomepage)
                {
                    errors.Add(product.Id+","+ product.Name+ "," + product.Sku + "," + "Mostrar en la página de inicio,No,Si");
                }

                if (product.MarkAsNew)
                {
                    errors.Add(product.Id + "," + product.Name + "," + product.Sku + "," + "Marcar como nuevo,Si,No");
                }

                if (product.AvailableStartDateTimeUtc != null)
                {
                    errors.Add(product.Id + "," + product.Name + "," + product.Sku + "," + "Fecha de inicio disponible,"+ product.AvailableStartDateTimeUtc + ",En Blanco (vacío/nulo)");
                }

                if (product.AvailableEndDateTimeUtc != null)
                {
                    errors.Add(product.Id + "," + product.Name + "," + product.Sku + "," + "Fecha de finalización disponible," + product.AvailableEndDateTimeUtc + ",En Blanco (vacío/nulo)");
                }

                if (product.ManageInventoryMethodId != 1 || product.ManageInventoryMethod != ManageInventoryMethod.ManageStock)
                {
                    if (product.ManageInventoryMethodId != 1 && product.ManageInventoryMethod != ManageInventoryMethod.ManageStock)
                    {
                        errors.Add(product.Id + "," + product.Name + "," + product.Sku + "," + "Método de inventario," + product.ManageInventoryMethod + ",Seguimiento Del Inventario (Manage Stock)");
                    }
                    else
                    {
                        errors.Add(product.Id + "," + product.Name + "," + product.Sku + "," + "Método de inventario," + product.ManageInventoryMethod + ",Seguimiento Del Inventario (Manage Stock)," +
                            "Además el campo:'ManageInventoryMethodId' no tiene la relación correcta con el campo 'ManageInventoryMethod'");
                    }
                }


                if (product.BackorderModeId != 1 || product.BackorderMode != BackorderMode.AllowQtyBelow0)
                {
                    if (product.BackorderModeId != 1 && product.BackorderMode != BackorderMode.AllowQtyBelow0)
                    {
                        errors.Add(product.Id + "," + product.Name + "," + product.Sku + "," + "Pedidos pendientes," + product.BackorderMode + ",Permitir QTY por Debajo de 0 (AllowQtyBelow0)");
                    }
                    else
                    {
                        errors.Add(product.Id + "," + product.Name + "," + product.Sku + "," + "Pedidos pendientes," + product.BackorderMode + ",Permitir QTY por Debajo de 0 (AllowQtyBelow0)," +
                            "Además el campo:'BackorderModeId' no tiene la relación correcta con el campo 'BackorderMode'");
                    }
                }
            }
            if (errors.Count() > 0)
            {
                //Send Product Alert
                string info = "Id,Name,Sku,Cammpo,Valor Actual,Valor Requerido" + Environment.NewLine;

                foreach (var error in errors)
                {
                    info = info + error + Environment.NewLine;
                }

                List<string> correos = new List<string>() {
                    "andres.posada@neta.mx",
                    //"paulina@neta.mx",
                    //"samuel.wong@neta.mx",
                    "miguel.zamora@neta.mx"
                    };

                var email = new Email();
                email.SetEmailOrigen("redash.server.netamx@gmail.com", "sht5$29.!");
                email.SetSubject("Alerta: Productos con errores para mostrar en Home Page");
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
