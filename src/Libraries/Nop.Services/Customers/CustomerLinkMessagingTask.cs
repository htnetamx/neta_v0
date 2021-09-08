using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Tasks;
using Nop.Services.Catalog;
using Nop.Services.Stores;

namespace Nop.Services.Customers
{
    class CustomerLinkMessagingTask : IScheduleTask
    {
        #region Fields

        private readonly IStoreService _storeService;
        private readonly IProductService _productService;
        private readonly IStoreMappingService _storeMapping;
        #endregion

        #region Ctor

        public CustomerLinkMessagingTask(IStoreService storeService, IProductService productService, IStoreMappingService storeMapping)
        {
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
            var template_Link_Viralizacion = "02c89181_e473_461e_9e66_8f6b75af9b5e:promo_hoy";
        }

        private async Task<string> Send(string number, string template, params object[] data)
        {
            var url = "https://netamx.calixtachat.com/api/v1/chats?";
            using var client = new HttpClient();

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
