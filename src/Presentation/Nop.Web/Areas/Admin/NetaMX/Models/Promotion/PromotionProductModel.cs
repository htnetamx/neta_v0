using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Promotion
{
    public partial record PromotionProductModel : BaseNopEntityModel
    {
        #region Properties

        public int PromotionId { get; set; }

        public int ProductId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Products.Fields.Product")]
        public string ProductName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Products.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        #endregion
    }
}
