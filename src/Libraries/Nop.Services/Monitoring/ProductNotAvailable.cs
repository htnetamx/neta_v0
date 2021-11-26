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

            foreach (var product in publishErrors)
            {
                List<string> errors = new List<string>();
                if (!product.ShowOnHomepage)
                {
                    errors.Add("Campo: 'Mostrar en la página de inicio' es: No y debe de ser: Si");
                }

                if (product.MarkAsNew)
                {
                    errors.Add("Campo: 'Marcar como nuevo' es: Si y debe de ser: No");
                }

                if (product.AvailableStartDateTimeUtc != null)
                {
                    errors.Add("Campo: 'Fecha de inicio disponible' es: "+product.AvailableStartDateTimeUtc+" y debe de ser: En Blanco (vacío/nulo)");
                }

                if (product.AvailableEndDateTimeUtc != null)
                {
                    errors.Add("Campo: 'Fecha de finalización disponible' es: " + product.AvailableEndDateTimeUtc + " y debe de ser: En Blanco (vacío/nulo)");
                }

                if (product.ManageInventoryMethodId != 1 || product.ManageInventoryMethod != ManageInventoryMethod.ManageStock)
                {
                    if (product.ManageInventoryMethodId != 1 && product.ManageInventoryMethod != ManageInventoryMethod.ManageStock)
                    {
                        errors.Add("Campo: 'Método de inventario' es: " + product.ManageInventoryMethod + " y debe de ser: Seguimiento Del Inventario (Manage Stock)");
                    }
                    else
                    {
                        errors.Add("Campo: 'Método de inventario' es: " + product.ManageInventoryMethod + " y debe de ser: Seguimiento Del Inventario (Manage Stock). Además" +
                            "el campo:'ManageInventoryMethodId' no tiene la relación correcta con el campo 'ManageInventoryMethod'");
                    }
                }

                if (product.ManageInventoryMethodId != 1 || product.ManageInventoryMethod != ManageInventoryMethod.ManageStock)
                {
                    if (product.ManageInventoryMethodId != 1 && product.ManageInventoryMethod != ManageInventoryMethod.ManageStock)
                    {
                        errors.Add("Campo: 'Método de inventario' es: " + product.ManageInventoryMethod + " y debe de ser: Seguimiento Del Inventario (Manage Stock)");
                    }
                    else
                    {
                        errors.Add("Campo: 'Método de inventario' es: " + product.ManageInventoryMethod + " y debe de ser: Seguimiento Del Inventario (Manage Stock). Además" +
                            "el campo:'ManageInventoryMethodId' no tiene la relación correcta con el campo 'ManageInventoryMethod'");
                    }
                }

                if (product.BackorderModeId != 1 || product.BackorderMode != BackorderMode.AllowQtyBelow0)
                {
                    if (product.BackorderModeId != 1 && product.BackorderMode != BackorderMode.AllowQtyBelow0)
                    {
                        errors.Add("Campo: 'Pedidos pendientes' es: " + product.BackorderMode + " y debe de ser: Permitir QTY por Debajo de 0 (AllowQtyBelow0)");
                    }
                    else
                    {
                        errors.Add("Campo: 'Pedidos pendientes' es: " + product.BackorderMode + " y debe de ser: Permitir QTY por Debajo de 0 (AllowQtyBelow0). Además" +
                            "el campo:'BackorderModeId' no tiene la relación correcta con el campo 'BackorderMode'");
                    }
                }

                if(errors.Count()>0)
                {
                    //Send Product Alert
                }
            }
        }

        #endregion
    }
}
