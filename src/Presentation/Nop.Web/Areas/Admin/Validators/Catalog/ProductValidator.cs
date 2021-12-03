using FluentValidation;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Validators.Catalog
{
    public partial class ProductValidator : BaseNopValidator<ProductModel>
    {
        public ProductValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.Name.Required"));
            
            RuleFor(x => x.SeName)
                .Length(0, NopSeoDefaults.SearchEngineNameLength)
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.SEO.SeName.MaxLengthValidation"), NopSeoDefaults.SearchEngineNameLength);
            
            RuleFor(x => x.RentalPriceLength)
                .GreaterThan(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.RentalPriceLength.ShouldBeGreaterThanZero"))
                .When(x => x.IsRental);

            RuleFor(x => x.PerTaras)
                .NotEmpty()
                .GreaterThan(0)
                .WithMessageAwait(localizationService.GetResourceAsync("PerTaras es obligatorio y debe de ser mayor que 0"));

            RuleFor(x => x.Sku)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Sku es obligatorio"));

            RuleFor(x => x.SelectedCategoryIds)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Las categorias son obligatorias"));

            RuleFor(x => x.SelectedManufacturerIds)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Los manufacturers (proveedores) son obligatorios"));

            SetDatabaseValidationRules<Product>(dataProvider);
        }
    }
}