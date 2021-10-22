using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Orders;
using Nop.Services.Tasks;

namespace Nop.Services.Stores
{
    public partial class StoreDailyUpdatePhaseTask : IScheduleTask
    {
        #region Fields

        private readonly IStoreService _storeService;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public StoreDailyUpdatePhaseTask(IStoreService storeService, IOrderService orderService)
        {
            _storeService = storeService;
            _orderService = orderService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            var stores = (await _storeService.GetAllStoresAsync())
                .Where(s => s.DisplayOrder > 0);

            foreach (var store in stores)
            {
                var orderExists = (await _orderService.GetOrdersByStoreIdsAsync(store.Id)).Any();
                if (orderExists)
                {
                    //Make sure old phase 3 values are updated to new phase 2 (growth phase)
                    if (store.DisplayOrder == 3)
                        store.DisplayOrder = 2;

                    //All stores are in the new phase scheme now
                    //At least one order exists in the store.
                    //Evaluate if phase 1 (initial) stores can move up to phase 2 (growth)

                    //CONDITIONS to move from display order 1 (Initial Phase) to 2 (Growth Phase)
                    // case1 : 7 days after first order or 
                    // case2 : 500 GMV, 5 distinct customers

                   var ordersFromStore = (await _orderService.GetOrdersByStoreIdsAsync(store.Id));
                   var firstOrderDate = ordersFromStore.Select(x => x.CreatedOnUtc).Min();

                    ////evaluate if there are 5 distinct clients
                    var disntinctCustomers = ordersFromStore.Select(x => x.CustomerId).Distinct();
                    var disntinctCustomersCounter = disntinctCustomers.Count();

                    ////SUM of order total value (all)
                    var orderValueGMV = ordersFromStore.Select(x => x.OrderTotal).Sum();

                    //Case1
                    if (store.DisplayOrder == 1 && DateTime.UtcNow.DayOfYear - firstOrderDate.DayOfYear > 6)
                    {
                        store.DisplayOrder = 2;
                    }

                    ////Case2 500 GMV, 5 distinct customers
                    else if (store.DisplayOrder == 1 && disntinctCustomersCounter > 5 && orderValueGMV > 500)
                    {
                        store.DisplayOrder = 2;
                    }
                    await _storeService.UpdateStoreAsync(store);
                }
            }
        }

        #endregion
    }
}
