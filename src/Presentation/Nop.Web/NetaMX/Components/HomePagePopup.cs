using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Helpers;
using Nop.Services.Media;
using Nop.Services.Promotion;
using Nop.Web.Framework.Components;
using Nop.Web.Models;

namespace Nop.Web.Components
{
    public class HomePagePopupViewComponent : NopViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly IDateTimeHelper _dateTimeHelper;

        public HomePagePopupViewComponent(IStoreContext storeContext,
            IDateTimeHelper dateTimeHelper)
        {
            _storeContext = storeContext;
            _dateTimeHelper = dateTimeHelper;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var createdDate = ((await _storeContext.GetCurrentStoreAsync()).CreatedOnUtc).AddHours(-6);
            var currentUserDate = (DateTime.UtcNow).AddHours(-6);
            if((currentUserDate - createdDate).Days < 8)
            {
                return View(true);
            }
            else
            {
                return View(false);
            }
        }
    }
}