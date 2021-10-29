using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Orders;
using Nop.Services.Tasks;

namespace Nop.Services.Common
{
    public partial class ControlSubaccounts : IScheduleTask
    {
        #region Fields

        private readonly IAddressService _addressService;
        #endregion

        #region Ctor

        public ControlSubaccounts(IAddressService addressService)
        {
            _addressService = addressService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            var addresses = await _addressService.GetAllMainAccounts();
            var service = EngineContext.Current.Resolve<IAddressService>();
            var maxNumberSubAccounts = 2;


            foreach (var address in addresses)
            {
                var children = service.GetRelatedAddressByIdAsync(address.PhoneNumber).Result;
                
                if (children.Count > maxNumberSubAccounts+1)
                {
                    int i = 0;
                    foreach (var child in children)
                    {
                        if (i > maxNumberSubAccounts)
                        {
                            await _addressService.DeleteAddressAsync(child);
                        }
                        i++;
                    }
                }
            }
        }

        #endregion
    }
}
