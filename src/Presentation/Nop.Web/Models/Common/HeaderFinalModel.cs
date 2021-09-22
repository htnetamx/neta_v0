using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Common
{
    public partial record HeaderFinalModel : BaseNopModel
    {
        public bool IsAuthenticated { get; set; }
        public string CustomerName { get; set; }
        
        public bool ShoppingCartEnabled { get; set; }
        public int ShoppingCartItems { get; set; }
    
        public decimal ShoppingCartTotal { get; set; }
        public Store Store { get; set; }

        public bool WishlistEnabled { get; set; }
        public int WishlistItems { get; set; }

        public bool AllowPrivateMessages { get; set; }
        public string UnreadPrivateMessages { get; set; }
        public string AlertMessage { get; set; }
        public UserRegistrationType RegistrationType { get; set; }
    }
}