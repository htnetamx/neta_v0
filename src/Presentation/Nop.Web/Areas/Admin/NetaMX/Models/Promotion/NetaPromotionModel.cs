using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nop.Web.Areas.Admin.Models.Promotion
{
    public partial record NetaPromotionModel : BaseNopEntityModel, ILocalizedModel<NetaPromotionLocalizedModel>
    {
        public NetaPromotionModel()
        {
            Locales = new List<NetaPromotionLocalizedModel>();
            PromotionProductSearchModel = new PromotionProductSearchModel();
        }
        [NopResourceDisplayName("Admin.Promotion.Fields.Name")]
        public string Name { get; set; }
        [NopResourceDisplayName("Admin.Promotion.Fields.Picture")]
        [UIHint("Picture")]
        public int PictureId { get; set; }
        [NopResourceDisplayName("Admin.Promotion.Fields.StartDate")]
        [UIHint("DateTime")]
        public DateTime StartDateUtc { get; set; }
        [NopResourceDisplayName("Admin.Promotion.Fields.EndDate")]
        [UIHint("DateTime")]
        public DateTime EndDateUtc { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.SeName")]
        public string SeName { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Discounts.Fields.UsePercentage")]
        public bool UsePercentage { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Discounts.Fields.DiscountPercentage")]
        public decimal DiscountPercentage { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Discounts.Fields.DiscountAmount")]
        public decimal DiscountAmount { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Discounts.Fields.MaximumDiscountAmount")]
        [UIHint("DecimalNullable")]
        public decimal? MaximumDiscountAmount { get; set; }
        public string PrimaryStoreCurrencyCode { get; set; }
        public IList<NetaPromotionLocalizedModel> Locales { get; set; }
        public PromotionProductSearchModel PromotionProductSearchModel { get; set; }

        public int DiscountId { get; set; }
    }

    public partial record NetaPromotionLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Promotion.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Fields.SeName")]
        public string SeName { get; set; }
    }
}