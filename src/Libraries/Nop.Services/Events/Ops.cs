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
    public partial class Ops : IScheduleTask
    {
        #region Fields
        
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Store> _storeRepository;

        #endregion

        #region Ctor

        public Ops(
            IRepository<Address> addressRepository,
            IRepository<Order> orderRepository,
            IRepository<Store> storeRepository
            )
        {
            _addressRepository = addressRepository;
            _orderRepository = orderRepository;
            _storeRepository = storeRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            var spreadsheetId = "1-6Jl47MkqAnZMxi3I-MVV8AO8h_NgOHcDtm6HDC7vic";
            var sheet = "";
            var range = "";
            DateTime firstOrder = new DateTime(2021, 7, 22, 21, 46, 22);
            DateTime nowDate = DateTime.UtcNow;
            DateTime firstOrder_MX = IDateService.ChangeUTCToMX(firstOrder);
            DateTime nowDate_MX = IDateService.ChangeUTCToMX(nowDate);

            if (nowDate_MX.DayOfWeek != DayOfWeek.Sunday)
            {
                if (nowDate_MX.Hour < 21)
                {
                    nowDate_MX = nowDate_MX.Subtract(new TimeSpan(1, 0, 0, 0));
                }

                DateTime yesterday_mx = nowDate_MX.Date.Add(new TimeSpan(21, 0, 0));

                int days_to_subract = 1;
                if (yesterday_mx.DayOfWeek == DayOfWeek.Sunday)
                {
                    days_to_subract = 2;
                }
                DateTime before_yesterday_mx = yesterday_mx.Subtract(new TimeSpan(days_to_subract, 0, 0, 0));
                DateTime yesterday = IDateService.ChangeMXToUTC(yesterday_mx);
                DateTime before_yesterday = IDateService.ChangeMXToUTC(before_yesterday_mx);
                var stores_orders = (from o in _orderRepository.Table
                                     join s in _storeRepository.Table
                                     on o.StoreId equals s.Id
                                     select new NineToNineOps()
                                     {
                                         Store = s,
                                         Order = o
                                     }).ToList();
               
                var stores_unique_clients_address = (from so2 in stores_orders
                                                     join a in _addressRepository.Table
                                                     on so2.Order.BillingAddressId equals a.Id
                                                     select new
                                                     {
                                                         Store = so2.Store.Id,
                                                         CreatedAt = so2.Order.CreatedOnUtc,
                                                         CustomerIp = so2.Order.CustomerIp,
                                                         PhoneNumber = a.PhoneNumber
                                                     });

                var stores_unique_clients_q = (from so in stores_unique_clients_address
                                               where so.CreatedAt <= yesterday
                                               group so by new { so.Store, so.CustomerIp, so.PhoneNumber } into agg_stores_unique_stores_ip_phone
                                               orderby agg_stores_unique_stores_ip_phone.Key.Store ascending, agg_stores_unique_stores_ip_phone.Min(so => so.CreatedAt)
                                               select new NineToNineOpsUniqueC10()
                                               {
                                                   Store = agg_stores_unique_stores_ip_phone.Key.Store,
                                                   CustomerPhone = agg_stores_unique_stores_ip_phone.Key.PhoneNumber,
                                                   CustomerIp = agg_stores_unique_stores_ip_phone.Key.CustomerIp,
                                                   FirstBuy = agg_stores_unique_stores_ip_phone.Min(so => so.CreatedAt),
                                                   UniqueCustomers = 1
                                               }).ToList();
               

                var stores_unique_clients = (from suc in stores_unique_clients_q
                                             group suc by new { suc.Store } into agg_stores_unique_stores_ip_phone
                                             orderby agg_stores_unique_stores_ip_phone.Key.Store
                                             select new NineToNineOpsUnique()
                                             {
                                                 Store = agg_stores_unique_stores_ip_phone.Key.Store,
                                                 UniqueCustomers = agg_stores_unique_stores_ip_phone.Sum(suc => suc.UniqueCustomers)
                                             }).ToList();
                NineToNineOpsUnique.Yesterday = yesterday;
                NineToNineOpsUnique.Before_Yesterday = before_yesterday;
                var stores_unique_clients2 = (from so in stores_unique_clients_address
                                              where so.CreatedAt <= before_yesterday
                                              group so by new { so.Store, so.CustomerIp, so.PhoneNumber } into agg_stores_unique_stores_ip_phone
                                              group agg_stores_unique_stores_ip_phone by new { agg_stores_unique_stores_ip_phone.Key.Store} into agg_stores_unique_stores_ip_phone_complete
                                              select new NineToNineOpsUnique()
                                              {
                                                  Store = agg_stores_unique_stores_ip_phone_complete.Key.Store,
                                                  UniqueCustomers = agg_stores_unique_stores_ip_phone_complete.Count()
                                              }).ToList();
                stores_unique_clients =(from suc in stores_unique_clients
                                       join suc2 in stores_unique_clients2 
                                       on suc.Store equals suc2.Store
                                       select new NineToNineOpsUnique()
                                       {
                                           Store = suc.Store,
                                           UniqueCustomers = suc.UniqueCustomers,
                                           UniqueCustomers_Day_Before = suc2.UniqueCustomers
                                       }).ToList();

                var orders_liberadas = (from suc in stores_unique_clients
                                        where suc.UniqueCustomers_Day_Before < 10 && suc.UniqueCustomers >= 10
                                        join s in _storeRepository.Table
                                        on suc.Store equals s.Id
                                        join o in _orderRepository.Table
                                        on s.Id equals o.StoreId
                                        where o.CreatedOnUtc <= yesterday
                                        select new NineToNineOps()
                                        {
                                            Store = s,
                                            Order = o
                                        }).ToList();
                
                var filtered_stores_orders = (from so in stores_orders
                                  where so.Order.CreatedOnUtc > before_yesterday && so.Order.CreatedOnUtc <= yesterday
                                  select new NineToNineOps() {
                                      Store = so.Store,
                                      Order = so.Order
                                  }
                                  ).ToList();

                var stores_GMV = (from so in filtered_stores_orders.Union(orders_liberadas, new AComparer())
                                  where so.Order.Id > 0
                                  group so by new { so.Store.Id } into agg_stores_orders
                                  orderby agg_stores_orders.Sum(store => store.Order.OrderTotal) descending
                                  select new NineToNineOpsGMV()
                                  {
                                      StoreId = agg_stores_orders.Key.Id,
                                      OrderTotal = agg_stores_orders.Sum(so => so.Order.OrderTotal),
                                  }).ToList();
                var all_Stores = (from s in _storeRepository.Table
                                  select new NineToNineOpsAllStores()
                                  {
                                      Store = s
                                  }).ToList();
                var phase1_Stores = (from s in all_Stores
                                     where s.Store.DisplayOrder < 2
                                     orderby s.Store.DisplayOrder descending, s.Store.Id ascending
                                     select new NineToNineOpsAllStores()
                                     {
                                         Store = s.Store
                                     }).ToList();
                List<object> datesMX = null;
                List<object> datesUTC = null;
                if (stores_GMV.Count > 0)
                {
                    sheet = "9 to 9 - Despacho";
                    range = "A:B";
                    var deleteResponse = GoogleAPI.DeleteSpreadSheetContent(spreadsheetId, sheet, range);
                    range = "A:B";
                    datesMX = new List<object> { "", "MX -Información entre:", before_yesterday_mx.ToString("dd/MM/yyyy HH:mm:ss"), "y", yesterday_mx.ToString("dd/MM/yyyy HH:mm:ss") };
                    datesUTC = new List<object> { "", "UTC-Información entre:", before_yesterday.ToString("dd/MM/yyyy HH:mm:ss"), "y", yesterday.ToString("dd/MM/yyyy HH:mm:ss") };
                    var appendResponse = GoogleAPI.AppendOnSpreadSheet929OpsGMV(spreadsheetId, sheet, range, stores_GMV, datesMX, datesUTC);
                }
                if (phase1_Stores.Count > 0)
                {
                    sheet = "Stores Phase 0-1";
                    range = "A:G";
                    var deleteResponse = GoogleAPI.DeleteSpreadSheetContent(spreadsheetId, sheet, range);
                    range = "A:G";
                    datesMX = new List<object> { "", "MX -Información entre:", before_yesterday_mx.ToString("dd/MM/yyyy HH:mm:ss"), "y", yesterday_mx.ToString("dd/MM/yyyy HH:mm:ss") };
                    datesUTC = new List<object> { "", "UTC-Información entre:", before_yesterday.ToString("dd/MM/yyyy HH:mm:ss"), "y", yesterday.ToString("dd/MM/yyyy HH:mm:ss") };
                    var appendResponse = GoogleAPI.AppendOnSpreadSheet929OpsAllStores(spreadsheetId, sheet, range, phase1_Stores, datesMX, datesUTC);
                }
                if (stores_unique_clients.Count > 0)
                {
                    sheet = "Unique Customers Per Store";
                    range = "A:E";
                    var deleteResponse = GoogleAPI.DeleteSpreadSheetContent(spreadsheetId, sheet, range);
                    range = "A:E";
                    datesMX = new List<object> { "", "MX -Información entre:", firstOrder_MX.ToString("dd/MM/yyyy HH:mm:ss"), "y", nowDate_MX.ToString("dd/MM/yyyy HH:mm:ss") };
                    datesUTC = new List<object> { "", "UTC-Información entre:", firstOrder.ToString("dd/MM/yyyy HH:mm:ss"), "y", nowDate.ToString("dd/MM/yyyy HH:mm:ss") };

                    var appendResponse = GoogleAPI.AppendOnSpreadSheet929OpsStoreUniqueClients(spreadsheetId, sheet, range, stores_unique_clients, datesMX, datesUTC);
                }
                if (stores_unique_clients_q.Count > 0)
                {
                    sheet = "Customers First Buy";
                    range = "A:E";
                    var deleteResponse = GoogleAPI.DeleteSpreadSheetContent(spreadsheetId, sheet, range);
                    range = "A:E";
                    datesMX = new List<object> { "", "MX -Información entre:", firstOrder_MX.ToString("dd/MM/yyyy HH:mm:ss"), "y", nowDate_MX.ToString("dd/MM/yyyy HH:mm:ss") };
                    datesUTC = new List<object> { "", "UTC-Información entre:", firstOrder.ToString("dd/MM/yyyy HH:mm:ss"), "y", nowDate.ToString("dd/MM/yyyy HH:mm:ss") };

                    var appendResponse = GoogleAPI.AppendOnSpreadSheet929OpsStoreUniqueClientsC10(spreadsheetId, sheet, range, stores_unique_clients_q, datesMX, datesUTC);
                }
                if (stores_orders.Count > 0)
                {
                    sheet = "All Orders";
                    range = "A:H";
                    var deleteResponse = GoogleAPI.DeleteSpreadSheetContent(spreadsheetId, sheet, range);
                    range = "A:H";
                    datesMX = new List<object> { "", "MX -Información entre:", firstOrder_MX.ToString("dd/MM/yyyy HH:mm:ss"), "y", nowDate_MX.ToString("dd/MM/yyyy HH:mm:ss") };
                    datesUTC = new List<object> { "", "UTC-Información entre:", firstOrder.ToString("dd/MM/yyyy HH:mm:ss"), "y", nowDate.ToString("dd/MM/yyyy HH:mm:ss") };

                    var appendResponse = GoogleAPI.AppendOnSpreadSheet929Ops(spreadsheetId, sheet, range, stores_orders, datesMX, datesUTC);
                }

                if (all_Stores.Count > 0)
                {
                    sheet = "All Stores";
                    range = "A:H";
                    var deleteResponse = GoogleAPI.DeleteSpreadSheetContent(spreadsheetId, sheet, range);
                    range = "A:H";
                    datesMX = new List<object> { "", "MX -Información entre:", before_yesterday_mx.ToString("dd/MM/yyyy HH:mm:ss"), "y", yesterday_mx.ToString("dd/MM/yyyy HH:mm:ss") };
                    datesUTC = new List<object> { "", "UTC-Información entre:", before_yesterday.ToString("dd/MM/yyyy HH:mm:ss"), "y", yesterday.ToString("dd/MM/yyyy HH:mm:ss") };
                    var appendResponse = GoogleAPI.AppendOnSpreadSheet929OpsAllStores(spreadsheetId, sheet, range, all_Stores, datesMX, datesUTC);
                }

            }
        }

        #endregion
    }
}