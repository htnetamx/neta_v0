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
    class SmsPromosShopTask : IScheduleTask
    {
        #region Fields

        private readonly IStoreService _storeService;
        private readonly IProductService _productService;
        private readonly IStoreMappingService _storeMapping;
        #endregion

        #region Ctor

        public SmsPromosShopTask(IStoreService storeService, IProductService productService, IStoreMappingService storeMapping)
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
                    var prodMap = (await _storeMapping.GetFullStoreMappingsAsync()).Where(v => v.StoreId == info.Id);
                    var products = (await _productService.GetAllProductsAsync()).Where(v =>
                    prodMap.Any(x => x.EntityId == v.Id) && v.Sku.EndsWith("LH")).OrderByDescending(v => v.OldPrice - v.Price).Take(4);

                    var prodList = string.Join(" /", products.Select(v => $"{v.Name} a ${v.Price.ToString("N2")} En otros lugares a ${v.OldPrice.ToString("N2")}").ToArray());

                    Send_SMS(info.CompanyPhoneNumber,
                        $"Encuentra {prodList} en {info.Name} si juntamos 10 clientes entre todos! Ingresa a esta liga para comprar y compártela cuando termines para que lleguemos a 10: {info.Url} Recuerda agregar el contacto para que te aparezca la liga {info.Url}");
                }
            }

            var fase2 = stores.Where(s => s.DisplayOrder == 2);
            foreach (var info in fase2)
            {
                if (!string.IsNullOrWhiteSpace(info.CompanyPhoneNumber) && string.Compare(info.CompanyPhoneNumber, "Sin numero") != 0)
                {
                    var prodMap = (await _storeMapping.GetFullStoreMappingsAsync()).Where(v => v.StoreId == info.Id);
                    var products = (await _productService.GetAllProductsAsync()).Where(v =>
                    prodMap.Any(x => x.EntityId == v.Id) && !(v.Sku.EndsWith("LH") || v.Sku.EndsWith("L1"))).OrderByDescending(v => v.OldPrice - v.Price).Take(4);

                    var prodList = string.Join(" /", products.Select(v => $"{v.Name} a ${v.Price.ToString("N2")} En otros lugares a ${v.OldPrice.ToString("N2")}").ToArray());

                    Send_SMS(info.CompanyPhoneNumber,
                        $"Te recuerdo la PROMO DEL DÍA netamx Elige el producto que quieras y juntemos solo 5 personas para comprar todo a precios mayoristas: {info.Url} Checa la liga para más productos y recoge en {info.Name}: {prodList} Y mucho más en la liga! Apúntate, comparte y guarda esta liga personalizada, promos nuevas todos los días! Recuerda agregar el contacto para que te aparezca la liga {info.Url}");
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
