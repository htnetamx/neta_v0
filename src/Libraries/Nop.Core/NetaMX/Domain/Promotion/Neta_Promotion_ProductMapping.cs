namespace Nop.Core.Domain.Promotion
{
    public partial class Neta_Promotion_ProductMapping : BaseEntity
    {
        public int Neta_PromotionId { get; set; }
        public int ProductId { get; set; }
        public int DisplayOrder { get; set; }

        public bool AllowToShowProductOnlyPromotion { get; set; }
    }
}
