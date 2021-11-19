using FluentValidation;
using Nop.Core.Domain.Promotion;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Promotion;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Validators.NetaPromotions
{
    public partial class NetaPromotionValidator : BaseNopValidator<NetaPromotionModel>
    {
        public NetaPromotionValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Promotion.Fields.Name.Required"));
            RuleFor(x => x.StartDateUtc).NotNull().NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Promotion.Fields.StartDate.Required"));
            RuleFor(x => x.EndDateUtc).NotNull().NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Promotion.Fields.EndDate.Required"));

            RuleFor(x => x.EndDateUtc).GreaterThan(x => x.StartDateUtc).WithMessageAwait(localizationService.GetResourceAsync("Admin.Promotion.Fields.EndDate.GreaterThenStart"));

            SetDatabaseValidationRules<Neta_Promotion>(dataProvider);
        }
    }

}
