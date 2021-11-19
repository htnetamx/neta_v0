using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Promotion
{
    public record AddProductToPromotionModel : BaseNopModel
    {
        #region Ctor

        public AddProductToPromotionModel()
        {
            SelectedProductIds = new List<int>();
        }
        #endregion

        #region Properties

        public int PromotionId { get; set; }

        public IList<int> SelectedProductIds { get; set; }

        #endregion
    }
}
