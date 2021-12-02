using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Tvt.Plugin.DiscountRules.MinSubtotal.Models
{
    public class RequirementModel
    {
        public RequirementModel()
        {
            AvailableCustomerRoles = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.DiscountRules.MinSubtotal.Fields.MinSubtotal")]
        public decimal MinSubtotal { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }

        public IList<SelectListItem> AvailableCustomerRoles { get; set; }
    }
}