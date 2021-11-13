using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nop.Core.Domain.Common;
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
            List<Address> addressWithErrors = new List<Address>();
            List<int> addressesIndexesWithErrors = new List<int>();
            
            var address_index = 0;
            foreach (var address in addresses)
            {
                try
                { 
                    var children = service.GetRelatedAddressByIdAsyncForControlSubaccounts(address.PhoneNumber);
                    var countWhile = 0;
                    while (countWhile < 3 && children.Count > maxNumberSubAccounts + 1)
                    {
                        int i = 0;
                        int cantidadSubcuentas = 0;
                        List<string> names = new List<string>();
                        foreach (var child in children)
                        {
                            if (i > 0 || child.Email[0] == '_')
                            {
                                if (cantidadSubcuentas >= maxNumberSubAccounts)
                                {
                                    if (child.Email[0] != '_')
                                    {
                                        child.Email = "_" + child.Email + "_";
                                    }
                                }
                                else
                                {
                                    if (names.Contains(child.FirstName))
                                    {
                                        child.Email = "_" + child.Email + "_";
                                    }
                                    else
                                    {
                                        cantidadSubcuentas++;
                                        names.Append(child.FirstName);
                                    }
                                }
                                await _addressService.UpdateAddressAsync(child);
                            }
                            i++;
                        }
                        children = service.GetRelatedAddressByIdAsyncForControlSubaccounts(address.PhoneNumber);
                        countWhile++;
                    }
                    if (countWhile >= 3 && children.Count > maxNumberSubAccounts + 1)
                    {
                        addressWithErrors.Append(address);
                        addressesIndexesWithErrors.Append(address_index);
                    }
                    address_index++;
                }
                catch (Exception e)
                {
                    var error = e;
                }
            }
            var hey = addressWithErrors;
            var hey2 = addressesIndexesWithErrors;


            addresses = await _addressService.GetAllDeletedMainAccountsOrSubaccountsExtraChar();
            foreach (var address in addresses)
            {
                try
                {
                    address.Email = address.Email.Replace("\"\"", "_");
                    address.Email = address.Email.Replace("______", "_");
                    address.Email = address.Email.Replace("_____", "_");
                    address.Email = address.Email.Replace("____", "_");
                    address.Email = address.Email.Replace("___", "_");
                    address.Email = address.Email.Replace("__", "_");
                    address.PhoneNumber = address.PhoneNumber.Replace("\"\"", "_");
                    address.PhoneNumber = address.PhoneNumber.Replace("______", "_");
                    address.PhoneNumber = address.PhoneNumber.Replace("_____", "_");
                    address.PhoneNumber = address.PhoneNumber.Replace("____", "_");
                    address.PhoneNumber = address.PhoneNumber.Replace("___", "_");
                    address.PhoneNumber = address.PhoneNumber.Replace("__", "_");
                    await _addressService.UpdateAddressAsync(address);
                           
                }
                catch (Exception e)
                {
                    var error = e;
                }
            }
        }

        #endregion
    }
}
