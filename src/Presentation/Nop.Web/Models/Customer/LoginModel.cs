using System.ComponentModel.DataAnnotations;
using Nop.Core.Domain.Customers;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using System.ComponentModel;

namespace Nop.Web.Models.Customer
{
    public partial record LoginModel : BaseNopModel
    {
        public bool CheckoutAsGuest { get; set; }

        [DataType(DataType.Text)]
        //[NopResourceDisplayName("Account.Login.Fields.Email")]
        [DisplayName("Nombre Completo")]
        public string Email { get; set; }

        public bool UsernamesEnabled { get; set; }

        public UserRegistrationType RegistrationType { get; set; }

        //[NopResourceDisplayName("Account.Login.Fields.Username")]
        [DisplayName("Nombre Completo")]
        public string Username { get; set; }

        [DataType(DataType.Text)]
        //[NopResourceDisplayName("Account.Login.Fields.Password")]
        [DisplayName("Teléfono")]
        public string Password { get; set; }

        [NopResourceDisplayName("Account.Login.Fields.RememberMe")]
        public bool RememberMe { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}