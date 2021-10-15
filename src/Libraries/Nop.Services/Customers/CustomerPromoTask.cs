using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Tasks;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Services.Common;
using Nop.Services.Stores;

namespace Nop.Services.Customers
{
    class CustomerPromoTask : IScheduleTask
    {
        #region Fields
        private readonly IAddressService _addressService;
        private readonly IOrderService _orderService;
        private readonly IStoreService _storeService;
        private readonly IProductService _productService;
        private readonly IStoreMappingService _storeMapping;
        #endregion

        #region Ctor

        public CustomerPromoTask(IAddressService addressService, IOrderService orderService, IStoreService storeService, IProductService productService, IStoreMappingService storeMapping)
        {
            _addressService = addressService;
            _orderService = orderService;
            _storeService = storeService;
            _productService = productService;
            _storeMapping = storeMapping;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            int cnt = 0;
            var phoneNumberList = (await _addressService.GetAllAddressesAsync()).OrderBy(v => v);
            foreach (var info in phoneNumberList)
            {
                var orders = (await _orderService.GetOrdersByPhoneNumberAsync(info))
                    .OrderByDescending(v => v.CreatedOnUtc).FirstOrDefault();
                if (orders != null)
                {
                    var store = await _storeService.GetStoreByIdAsync(orders.StoreId);
                    if(store != null)
                    {
                        var customer = await _addressService.GetAddressByIdAsync(orders.BillingAddressId);
                        if (customer != null)
                        {
                            if (!string.IsNullOrWhiteSpace(customer.FirstName))
                            {
                                var prodMap = (await _storeMapping.GetFullStoreMappingsAsync()).Where(v => v.StoreId == store.Id);
                                var products = (await _productService.GetAllProductsAsync()).Where(v =>
                                prodMap.Any(x => x.EntityId == v.Id)).OrderByDescending(v => v.OldPrice - v.Price).Take(3);

                                var prodList = string.Join("\n", products.Select(v => $"{v.Name} a ${v.Price.ToString("N2")}").ToArray());

                                //Send_SMS(info, 
                                //    $"¡No te pierdas las promos de hoy en NetaMx!\n\n{prodList}\nCompra y recoge en {store.Name} haciendo clic en la liga\n{store.Url}\n\nBaja los precios reenviando este mensaje a {"5"} contactos");

                                if(cnt >1000 && cnt <= 2000)
                                {
                                    var rta = await Send(info,
                                        "02c89181_e473_461e_9e66_8f6b75af9b5e:end_client_promos2",
                                        "14",
                                        prodList,
                                        store.Name,
                                        store.Url,
                                        "5");
                                    cnt++;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static async void Send_SMS(string number, string message)
        {
            using (var client = new HttpClient())
            {
                var url = "https://netamx.calixtachat.com/api/v1/chats?";

                var builder = new StringBuilder();
                builder.Append("api_token=59cFxxN0bAFnGtRviXp51ac4irjFDv&");
                builder.Append($"session={number}&");
                builder.Append($"message={message}&");
                builder.Append("channel_id=5");

                var response = await client.PostAsync(url + builder.ToString(), null);
                string result = await response.Content.ReadAsStringAsync();
            }
        }

        private async Task<string> Send(string number, string template, string channelId, params object[] data)
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
                builder.Append($"channel_id={channelId}&");
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
