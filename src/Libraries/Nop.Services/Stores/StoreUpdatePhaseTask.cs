using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Tasks;

namespace Nop.Services.Stores
{
    public partial class StoreUpdatePhaseTask : IScheduleTask
    {
        #region Fields

        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public StoreUpdatePhaseTask(IStoreService storeService)
        {
            _storeService = storeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            var stores = (await _storeService.GetAllStoresAsync()).Where(s => s.DisplayOrder != 3);
            foreach(var store in stores)
            {
                if (store.DisplayOrder == 1)
                    store.DisplayOrder = 2;
                else if (store.DisplayOrder == 2)
                    store.DisplayOrder = 3;

                await _storeService.UpdateStoreAsync(store);
            }
        }

        #endregion
    }
}
