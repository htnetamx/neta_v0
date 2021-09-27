using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Tasks;
using Nop.Services.Catalog;
using Nop.Services.Stores;
using Nop.Services.Orders;
using Nop.Services.Common;

namespace Nop.Services.Customers
{
    class CustomerLinkMessagingTask : IScheduleTask
    {
        #region Fields
        private readonly IStoreService _storeService;
        private readonly IOrderService _orderService;
        private readonly IAddressService _addressService;
        #endregion

        #region Ctor

        public CustomerLinkMessagingTask(IStoreService storeService, IOrderService orderService, IAddressService addressService)
        {
            _storeService = storeService;
            _orderService = orderService;
            _addressService = addressService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            var template_Link_Viralizacion = "02c89181_e473_461e_9e66_8f6b75af9b5e:compartir";
            var orders = await _orderService.SearchOrdersAsync(
                createdFromUtc: DateTime.UtcNow.AddHours(-5).AddDays(20), 
                createdToUtc: DateTime.UtcNow.AddHours(-5).AddDays(1).AddSeconds(-1));

            var init = 0;
            var end = orders.Count;
            var rndList = new List<int>();
            while(rndList.Count < 1000)
            {
                var rndNumber = new Random().Next(init, end);
                if (!rndList.Contains(rndNumber))
                {
                    rndList.Add(rndNumber);
                    orders.RemoveAt(rndNumber);

                    end = orders.Count;
                }
            }

            foreach (var position in rndList)
            {
                var customer = orders[position];

                var address = await _addressService.GetAddressByIdAsync(customer.BillingAddressId);
                var store = await _storeService.GetStoreByIdAsync(customer.StoreId);

                var rta = await Send(address.PhoneNumber,
                    template_Link_Viralizacion,
                    "*" + address.FirstName.Split(" ")[0] + "*",
                    store.Url);
            }
        }

        private async Task<string> Send(string number, string template, params object[] data)
        {
            using (var client = new HttpClient())
            {
                var url = "https://netamx.calixtachat.com/api/v1/chats?";

                var builder = new StringBuilder();
                builder.Append($"template_id={template}&");
                builder.Append("api_token=59cFxxN0bAFnGtRviXp51ac4irjFDv&");
                builder.Append($"session={number}&");
                foreach (var item in data)
                {
                    if (item.GetType().IsArray)
                    {
                        var arr = item as string[];
                        foreach (var item1 in arr)
                        {
                            builder.Append($"vars[]={item1}&");
                        }
                    }
                    else
                    {
                        builder.Append($"vars[]={item}&");
                    }
                }
                builder.Append("channel_id=10&");
                builder.Append("language=es_MX");

                var response = await client.PostAsync(url + builder.ToString(), null);
                string result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }

        private async void Blank(string number, string template, params object[] data)
        {

            var a = number;
            var b = template;
            var c = data;
            var d = "done";

        }
        #endregion    
    }
}
