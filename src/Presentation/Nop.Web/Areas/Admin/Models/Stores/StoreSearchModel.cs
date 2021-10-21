using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Stores
{
    /// <summary>
    /// Represents a store search model
    /// </summary>
    public partial record StoreSearchModel : BaseSearchModel
    {
        public string SearchName { get; set; }
    }
}