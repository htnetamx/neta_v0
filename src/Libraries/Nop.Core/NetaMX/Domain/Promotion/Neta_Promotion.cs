using System;

namespace Nop.Core.Domain.Promotion
{
    public partial class Neta_Promotion : BaseEntity
    {
        public string Name { get; set; }
        public int PictureId { get; set; }
        public DateTime StartDateUtc { get; set; }
        public DateTime EndDateUtc { get; set; }
        public bool Deleted { get; set; }

        public bool Published { get; set; }

        public int? DiscountId { get; set; }
    }
}
