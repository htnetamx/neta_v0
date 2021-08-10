using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Orders;
using Nop.Services.Tasks;

namespace Nop.Services.Stores
{
    public partial class StoreUpdatePhaseTask : IScheduleTask
    {
        #region Fields

        private readonly IStoreService _storeService;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public StoreUpdatePhaseTask(IStoreService storeService, IOrderService orderService)
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
            if (DateTime.UtcNow.Hour != 5)
                return;

            var stores = (await _storeService.GetAllStoresAsync())
                .Where(s => s.DisplayOrder != 3);
            foreach(var store in stores)
            {
                var orderExists = (await _orderService.GetOrdersByStoreIdsAsync(store.Id)).Any();
                if (orderExists)
                {
                    if (store.DisplayOrder == 1)
                        store.DisplayOrder = 2;
                    else if (store.DisplayOrder == 2)
                        store.DisplayOrder = 3;

                    await _storeService.UpdateStoreAsync(store);
                }
            }
        }

        #endregion
    }
}
