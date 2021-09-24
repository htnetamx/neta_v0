using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Tasks;
using Nop.Services.Catalog;
using Nop.Services.Orders;

namespace Nop.Services.Stores
{
    public class ChurnStoreMessagesTask : IScheduleTask
    {
        #region Fields
        private readonly IStoreService _storeService;
        private readonly IOrderService _orderService;
        #endregion

        #region Ctor

        public ChurnStoreMessagesTask(IStoreService storeService, IOrderService orderService)
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
            var stores = await _storeService.GetAllStoresAsync();
            foreach (var info in stores)
            {
                if (info.DisplayOrder == 1 || info.DisplayOrder == 2 || info.DisplayOrder == 3)
                {
                    if (!string.IsNullOrWhiteSpace(info.CompanyPhoneNumber) && string.Compare(info.CompanyPhoneNumber, "Sin numero") != 0)
                    {
                        var orders = (await _orderService.GetOrdersByStoreIdsAsync(info.Id))
                                .OrderByDescending(v => v.CreatedOnUtc).FirstOrDefault();
                        if (orders != null)
                        {
                            var days = DateTime.UtcNow.Subtract(orders.CreatedOnUtc).Days;
                            if (days >= 5 && days <= 15)
                            {
                                var rta = await Send(info.CompanyPhoneNumber,
                                    "02c89181_e473_461e_9e66_8f6b75af9b5e:churn__shops_v2",
                                    info.Name,
                                    days.ToString(),
                                    "https://forms.gle/VsEBH3hiWySeQNSAA");

                                //TODO: Guardo el envio
                                //TODO: La proxima vez, pregunto por envio (si dif es de 5 dias)
                            }
                        }
                    }
                }
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
