using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Promotion
{
    public partial record PromotionProductSearchModel : BaseSearchModel
    {
        public int PromotionId { get; set; }
    }
}
