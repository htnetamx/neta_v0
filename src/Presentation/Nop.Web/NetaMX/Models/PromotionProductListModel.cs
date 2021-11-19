using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Models
{
    public partial record PromotionProductListModel
    {
        public PromotionProductListModel()
        {
            PromotionProductOverviewModel = new List<ProductOverviewModel>();
        }

        public int PromotionId { get; set; }
        public string PromotionName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Published { get; set; }
        public IList<ProductOverviewModel> PromotionProductOverviewModel { get; set; }
    }
}
