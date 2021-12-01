using FluentValidation;
using Tvt.Plugin.DiscountRules.MinSubtotal.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Tvt.Plugin.DiscountRules.MinSubtotal.Validators
{
    /// <summary>
    /// Represents an <see cref="RequirementModel"/> validator.
    /// </summary>
    public class RequirementModelValidator : BaseNopValidator<RequirementModel>
    {
        public RequirementModelValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.DiscountId)
                .NotEmpty()
               .WithMessageAwait(localizationService.GetResourceAsync("Plugins.DiscountRules.MinSubtotal.Fields.DiscountId.Required"));
            RuleFor(model => model.MinSubtotal)
                .NotEmpty()
               .WithMessageAwait(localizationService.GetResourceAsync("Plugins.DiscountRules.MinSubtotal.Fields.MinSubtotal.Required"));
        }
    }
}
