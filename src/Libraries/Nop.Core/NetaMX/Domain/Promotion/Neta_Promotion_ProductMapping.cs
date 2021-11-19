using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Promotion
{
    public partial class Neta_Promotion_ProductMapping : BaseEntity
    {
        public int Neta_PromotionId { get; set; }
        public int ProductId { get; set; }
        public int DisplayOrder { get; set; }
    }
}
