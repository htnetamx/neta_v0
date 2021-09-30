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

namespace Nop.Services.Customers
{
    class ChurnCustomerTask : IScheduleTask
    {
        #region Fields
        private readonly IAddressService _addressService;
        private readonly IOrderService _orderService;
        #endregion

        #region Ctor

        public ChurnCustomerTask(IAddressService addressService, IOrderService orderService)
        {
            _addressService = addressService;
            _orderService = orderService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            var phoneNumberList = await _addressService.GetAllAddressesAsync();
            foreach (var info in phoneNumberList)
            {
                var orders = (await _orderService.GetOrdersByPhoneNumberAsync(info))
                    .OrderByDescending(v => v.CreatedOnUtc).FirstOrDefault();
                if (orders != null)
                {
                    var days = DateTime.UtcNow.Subtract(orders.CreatedOnUtc).Days;
                    if (days >= 7 && days <= 22)
                    {
                        var customer = await _addressService.GetAddressByIdAsync(orders.BillingAddressId);
                        if(customer != null)
                        {
                            if (!string.IsNullOrWhiteSpace(customer.FirstName))
                            {
                                //var rta = await Send(info,
                                //    "02c89181_e473_461e_9e66_8f6b75af9b5e:churn_end_clients_v2",
                                //    customer.FirstName.Split(" ")[0],
                                //    days.ToString(),
                                //    "https://forms.gle/MYr7Gh1y8M7kUjSt9");

                                Send_SMS(info,
                                    $"¡Hola {customer.FirstName.Split(" ")[0]}! Te hablamos de NetaMx, los de las promos del día.\nVimos que tu último pedido fue hace {days.ToString()} días y nos gustaría saber porque no has vuelto a pedir.\n¿Podrías ayudarnos a mejorar contestando esta encuesta?\n{"https://forms.gle/MYr7Gh1y8M7kUjSt9"}\n\nEsperamos tenerte devuelta muy pronto");

                                //TODO: Guardo el envio
                                //TODO: La proxima vez, pregunto por envio (si dif es de 7 dias)
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
