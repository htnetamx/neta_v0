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
            //Define at what time we want his to be running, for testing <5 BEFORE 5 AM
            if (DateTime.UtcNow.Hour < 5)
                return;

            var stores = (await _storeService.GetAllStoresAsync())
                .Where(s => s.DisplayOrder != 3);

            foreach (var store in stores)
            {
                var orderExists = (await _orderService.GetOrdersByStoreIdsAsync(store.Id)).Any();
                if (orderExists)
                {
                    if (store.DisplayOrder == 1)
                        store.DisplayOrder = 2;
                    else if (store.DisplayOrder == 2)
                        store.DisplayOrder = 3;

                    //at this point there is no store with phase 1, and at least one order exists in the store. Evaluate if phase 2 can move up to phase 3

                    //CONDITIONS
                    // case1 : 7 days after first order or 
                    // case2 : 500 GMV, 5 distinct customers

                    var firstOrderDate = (await _orderService.GetOrdersByStoreIdsAsync(store.Id)).Select(x  =>x.CreatedOnUtc).Min();

                    //evaluate if there are 5 distinct clients
                    var disntinctCustomers = (await _orderService.GetOrdersByStoreIdsAsync(store.Id)).Select(x => x.CustomerId).Distinct();
                    var disntinctCustomersCounter = disntinctCustomers.Count();

                    //SUM of order total value (all)
                    var orderValueGMV = (await _orderService.GetOrdersByStoreIdsAsync(store.Id)).Select(x => x.OrderTotal).Sum();

                    //Case1
                    if (store.DisplayOrder == 2 && DateTime.UtcNow.DayOfYear - firstOrderDate.DayOfYear > 6  )
                    {
                        store.DisplayOrder = 3;
                    }
                    
                    //Case2
                    else if (store.DisplayOrder == 2 && disntinctCustomersCounter > 5 && orderValueGMV > 400)
                    {
                        store.DisplayOrder = 3;
                    }                    
                    await _storeService.UpdateStoreAsync(store);
                }
            }

            //Update database?
        }

        #endregion
    }
}
