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
            if ((DateTime.UtcNow.AddHours(-5)).DayOfWeek==DayOfWeek.Sunday)
                return;
            
            var stores = (await _storeService.GetAllStoresAsync())
                .Where(s => s.DisplayOrder >0 && s.DisplayOrder<4);
            var productos = (await _productService.GetAllProductsAsync());
            var store_mapping = (await _storeMapping.GetFullStoreMappingsAsync());

            var max_discounts = (productos.Join(store_mapping, p => p.Id, sm => sm.EntityId,
                (p, sm) => new { StoreId = sm.StoreId, ProductId = p.Id, OldPrice = p.OldPrice, Price=p.Price}))
                .GroupBy(sm => sm.StoreId,(storeId, sm_stores) => new{StoreId = storeId, Best_Promo = sm_stores.Max(sm=>sm.OldPrice-sm.Price)});
    
            var query = (max_discounts.Join(stores, md => md.StoreId, s => s.Id,
                (md, s) => new { CompanyPhoneNumber = s.CompanyPhoneNumber, StoreName = s.Name, CompanyURL =s.Url, md.Best_Promo }))
                .Join(productos, store => store.Best_Promo, p => (p.OldPrice - p.Price),
                (s,p) => new { CompanyPhoneNumber=s.CompanyPhoneNumber, ProductName=p.Name, ProductOldPrice=p.OldPrice, ProductPrice=p.Price, s.CompanyURL});

            var template_Link_Viralizacion = "02c89181_e473_461e_9e66_8f6b75af9b5e:promo_hoy";
            foreach (var info in query)
            {
                if (!string.IsNullOrWhiteSpace(info.CompanyPhoneNumber) && string.Compare(info.CompanyPhoneNumber, "Sin numero") != 0)
                {
                    if (info.CompanyURL == "Hteijiz.com" || info.CompanyURL == "Aposada.com" || info.CompanyURL == "DCorona.com")
                    {
                        Send(info.CompanyPhoneNumber, template_Link_Viralizacion, info.ProductName, info.ProductOldPrice, info.ProductPrice, info.CompanyURL);
                    }
                }
            }
        }
   
        private async void Send(string number, string template, params object[] data)
        {
            var url = "https://netamx.calixtachat.com/api/v1/chats?api_token=59cFxxN0bAFnGtRviXp51ac4irjFDv&language=es_MX&";
            using var client = new HttpClient();

            var builder = new StringBuilder();
            builder.Append("channel_id=10&");
            builder.Append($"template_id={template}&");
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

            var response = await client.PostAsync(url + builder.ToString().TrimEnd('&'), null);
            string result = await response.Content.ReadAsStringAsync();
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
