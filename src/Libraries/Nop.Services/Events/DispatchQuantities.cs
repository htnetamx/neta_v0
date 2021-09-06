using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using Nop.Services.Orders;
using Nop.Services.Tasks;
using Nop.Services.Stores;
using Nop.Services.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Services.Google;
using Nop.Services.Localization;

namespace Nop.Services.Events
{
    public partial class DispatchQuantities : IScheduleTask
    {
        #region Fields
        
        private readonly IStoreService _storeService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<OrderNote> _orderNoteRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        private readonly IRepository<RecurringPayment> _recurringPaymentRepository;
        private readonly IRepository<RecurringPaymentHistory> _recurringPaymentHistoryRepository;
        private readonly IRepository<Store> _storeRepository;
        private readonly IShipmentService _shipmentService;
        #endregion

        #region Ctor

        public DispatchQuantities(
            IStoreService storeService,
            IOrderService orderService,
            IProductService productService,
            IRepository<Address> addressRepository,
            IRepository<Customer> customerRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<OrderNote> orderNoteRepository,
            IRepository<Product> productRepository,
            IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
            IRepository<RecurringPayment> recurringPaymentRepository,
            IRepository<RecurringPaymentHistory> recurringPaymentHistoryRepository,
            IRepository<Store> storeRepository,
            IShipmentService shipmentService)
        {
            _storeService = storeService;
            _orderService = orderService;
            _productService = productService;
            _addressRepository = addressRepository;
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _orderNoteRepository = orderNoteRepository;
            _productRepository = productRepository;
            _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
            _recurringPaymentRepository = recurringPaymentRepository;
            _recurringPaymentHistoryRepository = recurringPaymentHistoryRepository;
            _storeRepository = storeRepository;
            _shipmentService = shipmentService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            var spreadsheetId = "1-6Jl47MkqAnZMxi3I-MVV8AO8h_NgOHcDtm6HDC7vic";
            var sheet = "9 to 9 - Despacho";
            var range = "A:K";
            int gmvColumn = ((int)'A' % 32) - 1;
            int storeIdColumn = ((int)'B' % 32)-1;
            int decisionColumn = ((int)'I' % 32)-1;
            int byesterdayColumn =((int)'C' % 32)-1;
            int yesterdayColumn = ((int)'E' % 32)-1;
            int liberadaColumn = ((int)'K' % 32) - 1;
            var nine2Nine = GoogleAPI.ReadSpreadSheet(spreadsheetId, sheet, range);

            DateTime before_yesterday=new DateTime();
            DateTime yesterday= new DateTime();
            DateTime before_yesterday_mx = new DateTime();
            DateTime yesterday_mx = new DateTime();

            int count = 1;

            List < NineToNineOpsStoresDispatchDecision >  dispatchAllInfo= new List<NineToNineOpsStoresDispatchDecision>();

            foreach(var row in nine2Nine){
                if (count > 3)
                {
                    dispatchAllInfo.Add(new NineToNineOpsStoresDispatchDecision() { StoreId = Int32.Parse((string)row[storeIdColumn]), Decision = (string)row[decisionColumn], Liberada = (string)row[liberadaColumn], GMV = Decimal.Parse((string)row[gmvColumn])});
                }
                else
                {
                    if (count == 1)
                    {
                        before_yesterday_mx = DateTime.Parse((string)row[byesterdayColumn]);
                        yesterday_mx = DateTime.Parse((string)row[yesterdayColumn]);
                    }
                    if (count == 2)
                    {
                        before_yesterday = DateTime.Parse((string)row[byesterdayColumn]);
                        yesterday = DateTime.Parse((string)row[yesterdayColumn]);
                    }
                }
                count++;
            }


            List<Order> filtered_orders = (from o in _orderRepository.Table
                                   where o.CreatedOnUtc > before_yesterday && o.CreatedOnUtc <= yesterday
                                   select o).ToList();


            List<NineToNineOpsStoresDispatchDecision> stores_2_dispatch = (from dai in dispatchAllInfo
                                     where dai.Decision=="Despacho"
                                     join s in _storeRepository.Table
                                     on dai.StoreId equals s.Id
                                     select new NineToNineOpsStoresDispatchDecision { StoreId = dai.StoreId,StoreName=s.CompanyName, GMV=dai.GMV, Liberada=dai.Liberada}).ToList();
           
            
            var filter_stores_dispatch_1= (from o in filtered_orders
                                           join dai in stores_2_dispatch
                                           on o.StoreId equals dai.StoreId
                                           select new NineToNineOpsStoresDispatchDecision { StoreId = dai.StoreId, StoreName = dai.StoreName, GMV = dai.GMV, CreatedOnUtc=o.CreatedOnUtc,OrderId=o.Id, OrderTotal=o.OrderTotal }).ToList();
            
            var filter_stores_dispatch_2 = (from dai in stores_2_dispatch
                                            where dai.Liberada == "Liberada"
                                            join  o in _orderRepository.Table
                                            on dai.StoreId equals o.StoreId
                                            where o.CreatedOnUtc <= yesterday
                                            select new NineToNineOpsStoresDispatchDecision { StoreId = dai.StoreId, StoreName = dai.StoreName, GMV = dai.GMV, CreatedOnUtc = o.CreatedOnUtc, OrderId = o.Id, OrderTotal = o.OrderTotal }).ToList();
            var filter_stores_dispatch = (from so in filter_stores_dispatch_1.Union(filter_stores_dispatch_2, new NineToNineOpsStoresDispatchDecisionsComparer())
                                          where so.OrderId>0
                                          group so by new { so.StoreId, so.OrderId, so.GMV, so.CreatedOnUtc, so.StoreName } into agg_stores_orders
                                          orderby agg_stores_orders.Sum(store => store.OrderTotal) descending
                                          select new
                                          {
                                              StoreId = agg_stores_orders.Key.StoreId,
                                              OrderId = agg_stores_orders.Key.OrderId,
                                              GMV = agg_stores_orders.Key.GMV,
                                              CreatedOnUtc = agg_stores_orders.Key.CreatedOnUtc,
                                              StoreName = agg_stores_orders.Key.StoreName,
                                              OrderTotal = agg_stores_orders.Sum(so => so.OrderTotal),
                                          }).ToList();
          
            var dispatch = (from  s2d in filter_stores_dispatch
                             join oi in _orderItemRepository.Table
                             on s2d.OrderId equals oi.OrderId
                             join p in _productRepository.Table
                             on oi.ProductId equals p.Id
                             orderby s2d.GMV descending, s2d.StoreId ascending,s2d.CreatedOnUtc ascending
                             select new NineToNineOpsStoresDispatchDecision { StoreId = s2d.StoreId, StoreName=s2d.StoreName, GMV = s2d.GMV, OrderId = s2d.OrderId,  CreatedOnUtc = s2d.CreatedOnUtc, OrderTotal=s2d.OrderTotal, Sku=p.Sku, ProductName=p.Name, ProductQuantity=oi.Quantity,BatchPrice=oi.PriceInclTax, PerTaraRatio=p.PerTaras, CantidadTaras= (oi.Quantity * (1 / p.PerTaras)) }).ToList();
            
            var dispatch_orders=(from dor in dispatch
                                 group dor by new { dor.StoreId,dor.OrderId,dor.OrderTotal,dor.GMV } into agg_dispatch
                                 select new NineToNineOpsStoresDispatchOrderSummary {
                                     OrderId = agg_dispatch.Key.OrderId,
                                     StoreId = agg_dispatch.Key.StoreId, 
                                     OrderTotal = agg_dispatch.Key.OrderTotal,
                                     ProductQuantity = agg_dispatch.Sum(dor => dor.ProductQuantity),
                                     CantidadTaras= agg_dispatch.Sum(dor => dor.CantidadTaras),
                                     GMV = agg_dispatch.Key.GMV
                                 }).ToList();

            var dispatch_taras = (from dor in dispatch_orders
                                  group dor by new { dor.StoreId, dor.GMV } into agg_taras
                                   select new NineToNineOpsStoresDispatchTarasSymmary
                                   {
                                       GMV = agg_taras.Key.GMV,
                                       ProductQuantity = agg_taras.Sum(dor => dor.ProductQuantity),
                                       OrdenPromedio = agg_taras.Average(dor => dor.OrderTotal),
                                       CantidadOrdenes=agg_taras.Count(),
                                       TaraPromedioPorOrden = agg_taras.Average(dor => dor.CantidadTaras),
                                       CantidadTaras = agg_taras.Sum(dor => dor.CantidadTaras),
                                       CantidadProductosPromedioPorOrden = agg_taras.Average(dor => dor.ProductQuantity),
                                       CantidadProductos = agg_taras.Sum(dor => dor.ProductQuantity),
                                       StoreId = agg_taras.Key.StoreId
                                   }).ToList();

            var latLong = (from s in _storeRepository.Table
                           select new LatLong() {
                              StoreId=s.Id,
                              Latitud=s.Latitud,
                              Longitud=s.Longitud
                            }).ToList();

            sheet = "Q's Despacho";
            range = "A:N";
            var deleteResponse = GoogleAPI.DeleteSpreadSheetContent(spreadsheetId, sheet, range);
            range = "A:N";
            var datesMX = new List<object> { "", "MX -Información entre:", before_yesterday_mx.ToString("dd/MM/yyyy HH:mm:ss"), "y", yesterday_mx.ToString("dd/MM/yyyy HH:mm:ss") };
            var datesUTC = new List<object> { "", "UTC-Información entre:", before_yesterday.ToString("dd/MM/yyyy HH:mm:ss"), "y", yesterday.ToString("dd/MM/yyyy HH:mm:ss") };

            var appendResponse = GoogleAPI.AppendOnSpreadSheet929OpsDispatch(spreadsheetId, sheet, range, dispatch, datesMX, datesUTC);


            sheet = "Ordenes Despacho x Tienda";
            range = "A:E";
            deleteResponse = GoogleAPI.DeleteSpreadSheetContent(spreadsheetId, sheet, range);
            range = "A:E";
            datesMX = new List<object> { "", "MX -Información entre:", before_yesterday_mx.ToString("dd/MM/yyyy HH:mm:ss"), "y", yesterday_mx.ToString("dd/MM/yyyy HH:mm:ss") };
            datesUTC = new List<object> { "", "UTC-Información entre:", before_yesterday.ToString("dd/MM/yyyy HH:mm:ss"), "y", yesterday.ToString("dd/MM/yyyy HH:mm:ss") };

            appendResponse = GoogleAPI.AppendOnSpreadSheet929OpsDispatchOrders(spreadsheetId, sheet, range, dispatch_orders, datesMX, datesUTC);



            sheet = "Taras x Tienda";
            range = "A:H";
            deleteResponse = GoogleAPI.DeleteSpreadSheetContent(spreadsheetId, sheet, range);
            range = "A:H";
            datesMX = new List<object> { "", "MX -Información entre:", before_yesterday_mx.ToString("dd/MM/yyyy HH:mm:ss"), "y", yesterday_mx.ToString("dd/MM/yyyy HH:mm:ss") };
            datesUTC = new List<object> { "", "UTC-Información entre:", before_yesterday.ToString("dd/MM/yyyy HH:mm:ss"), "y", yesterday.ToString("dd/MM/yyyy HH:mm:ss") };

            appendResponse = GoogleAPI.AppendOnSpreadSheet929OpsDispatchTaras(spreadsheetId, sheet, range, dispatch_taras, datesMX, datesUTC);

            sheet = "NOP Lat-Long";
            range = "A:C";
            deleteResponse = GoogleAPI.DeleteSpreadSheetContent(spreadsheetId, sheet, range);
            range = "A:C";
            datesMX = new List<object> { "", "MX -Información entre:", before_yesterday_mx.ToString("dd/MM/yyyy HH:mm:ss"), "y", yesterday_mx.ToString("dd/MM/yyyy HH:mm:ss") };
            datesUTC = new List<object> { "", "UTC-Información entre:", before_yesterday.ToString("dd/MM/yyyy HH:mm:ss"), "y", yesterday.ToString("dd/MM/yyyy HH:mm:ss") };

            appendResponse = GoogleAPI.AppendOnSpreadSheetLatLong(spreadsheetId, sheet, range, latLong);
        }

        #endregion
    }
}