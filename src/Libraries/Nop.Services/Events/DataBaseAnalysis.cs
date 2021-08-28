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

namespace Nop.Services.Events
{
    public partial class DataBaseAnalysis : IScheduleTask
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

        public DataBaseAnalysis(
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
            

            var spreadsheetId = "1qn_oyT1sxvqyJ7YyykaukARXzrq2kfp_OH7yLDcPg0k";
            var sheet = "Stores";

            var stores_With_Errors = (from s in _storeRepository.Table
                                     select new InfoShopsErrors()
                                     {
                                         StoreId = s.Id.ToString(),
                                         StoreName = s.Name,
                                         StoreUrl = s.Url,
                                         StorePhoneNumber = s.CompanyPhoneNumber,
                                         Error=""}).ToList();

            if (stores_With_Errors.Count>0)
            {
                var range = "A:CC";
                var deleteResponse = GoogleAPI.DeleteSpreadSheetContent(spreadsheetId, sheet, range);
                range = "A:D";
                var appendResponse = GoogleAPI.AppendOnSpreadSheet(spreadsheetId, sheet, range, stores_With_Errors);
            }
        }

        #endregion
    }
}