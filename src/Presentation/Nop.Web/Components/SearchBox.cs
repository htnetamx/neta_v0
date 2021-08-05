using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components
{
    public class SearchBoxViewComponent : NopViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly ICatalogModelFactory _catalogModelFactory;

        public SearchBoxViewComponent(ICatalogModelFactory catalogModelFactory,
            IStoreContext storeContext)
        {
            _catalogModelFactory = catalogModelFactory;
            _storeContext = storeContext;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _catalogModelFactory.PrepareSearchBoxModelAsync();
            model.CurrentStoreName = (await _storeContext.GetCurrentStoreAsync()).Name;
            return View(model);
        }
    }
}
