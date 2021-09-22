using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Tasks;
using Nop.Services.Catalog;
namespace Nop.Services.Stores
{
    class AuronixMessaging : IScheduleTask
    {
        #region Fields

        private readonly IStoreService _storeService;
        private readonly IProductService _productService;
        private readonly IStoreMappingService _storeMapping;
        #endregion

        #region Ctor

        public AuronixMessaging(IStoreService storeService,IProductService productService, IStoreMappingService storeMapping)
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
            //if ((DateTime.UtcNow.AddHours(-5)).DayOfWeek==DayOfWeek.Sunday)
            //    return;

            var stores = await _storeService.GetAllStoresAsync();
            var fase1 = stores.Where(s => s.DisplayOrder == 1);
            foreach (var info in fase1)
            {
                if (!string.IsNullOrWhiteSpace(info.CompanyPhoneNumber) && string.Compare(info.CompanyPhoneNumber, "Sin numero") != 0)
                {
                    var rta = await Send(info.CompanyPhoneNumber,
                        "02c89181_e473_461e_9e66_8f6b75af9b5e:promos_f1",
                        info.Name,
                        info.Url);
                }
            }

            var fase2 = stores.Where(s => s.DisplayOrder == 2);
            foreach (var info in fase2)
            {
                if (!string.IsNullOrWhiteSpace(info.CompanyPhoneNumber) && string.Compare(info.CompanyPhoneNumber, "Sin numero") != 0)
                {
                    var prodMap = (await _storeMapping.GetFullStoreMappingsAsync()).Where(v => v.StoreId == info.Id);
                    var products = (await _productService.GetAllProductsAsync()).Where(v =>
                    prodMap.Any(x => x.EntityId == v.Id) && v.Sku.EndsWith("F2")).OrderByDescending(v => v.OldPrice - v.Price).Take(4);

                    var prodList = string.Join(" /", products.Select(v => $"{v.Name} a ${v.Price.ToString("N2")} En otros lugares a ${v.OldPrice.ToString("N2")}").ToArray());

                    var rta = await Send(info.CompanyPhoneNumber,
                        "02c89181_e473_461e_9e66_8f6b75af9b5e:promos_f3_2",
                        info.Url, 
                        info.Name,
                        prodList + " *y muchas promos más!!*");
                }
            }

            var fase3 = stores.Where(s => s.DisplayOrder == 3);
            foreach (var info in fase3)
            {
                if (!string.IsNullOrWhiteSpace(info.CompanyPhoneNumber) && string.Compare(info.CompanyPhoneNumber, "Sin numero") != 0)
                {
                    var prodMap = (await _storeMapping.GetFullStoreMappingsAsync()).Where(v => v.StoreId == info.Id);
                    var products = (await _productService.GetAllProductsAsync()).Where(v =>
                    prodMap.Any(x => x.EntityId == v.Id) && v.OldPrice > v.Price &&
                    DateTime.UtcNow >= v.MarkAsNewStartDateTimeUtc &&
                    DateTime.UtcNow <= v.MarkAsNewEndDateTimeUtc).OrderByDescending(v => v.OldPrice - v.Price).Take(3);

                    var prodList = string.Join(" /", products.Select(v => $"{v.Name} a ${v.Price.ToString("N2")} En otros lugares a ${v.OldPrice.ToString("N2")}").ToArray());

                    var rta = Send(info.CompanyPhoneNumber,
                        "02c89181_e473_461e_9e66_8f6b75af9b5e:promos_f3_2",
                        info.Url,
                        info.Name,
                        prodList + " *y muchas promos más!!*");
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

        private async void Blank(string number, string template, params object[] data) {

            var a = number;
            var b = template;
            var c = data;
            var d = "done";
        
        }
    #endregion
    }
}
