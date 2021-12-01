using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Stores
{
    /// <summary>
    /// Represents a store model
    /// </summary>
    public partial record StoreModel : BaseNopEntityModel, ILocalizedModel<StoreLocalizedModel>
    {
        #region Ctor

        public StoreModel()
        {
            Locales = new List<StoreLocalizedModel>();
            AvailableLanguages = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.Url")]
        public string Url { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.SslEnabled")]
        public virtual bool SslEnabled { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.Hosts")]
        public string Hosts { get; set; }

        //default language
        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.DefaultLanguage")]
        public int DefaultLanguageId { get; set; }

        public IList<SelectListItem> AvailableLanguages { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyName")]
        public string CompanyName { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyAddress")]
        public string CompanyAddress { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyPhoneNumber")]
        public string CompanyPhoneNumber { get; set; }

        [DisplayName("Longitud")]
        public string Longitud { get; set; }
        [DisplayName("Latitud")]
        public string Latitud { get; set; }
        [DisplayName("ZipCode")]
        public string ZipCode { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyVat")]
        public string CompanyVat { get; set; }

        [DisplayName("Saldo NetaBono")]
        public decimal NetaCoin { get; set; }

        [DisplayName("Lunes")]
        public DateTime? MondayStartDateTimeUtc { get; set; }
        [DisplayName("Lunes")]
        public DateTime? MondayEndDateTimeUtc { get; set; }
        [DisplayName("Martes")]
        public DateTime? TuesdayStartDateTimeUtc { get; set; }
        [DisplayName("Martes")]
        public DateTime? TuesdayEndDateTimeUtc { get; set; }
        [DisplayName("Miercoles")]
        public DateTime? WednesdayStartDateTimeUtc { get; set; }
        [DisplayName("Miercoles")]
        public DateTime? WednesdayEndDateTimeUtc { get; set; }
        [DisplayName("Jueves")]
        public DateTime? ThursdayStartDateTimeUtc { get; set; }
        [DisplayName("Jueves")]
        public DateTime? ThursdayEndDateTimeUtc { get; set; }
        [DisplayName("Viernes")]
        public DateTime? FridayStartDateTimeUtc { get; set; }
        [DisplayName("Viernes")]
        public DateTime? FridayEndDateTimeUtc { get; set; }
        [DisplayName("Sabado")]
        public DateTime? SaturdayStartDateTimeUtc { get; set; }
        [DisplayName("Sabado")]
        public DateTime? SaturdayEndDateTimeUtc { get; set; }
        [DisplayName("Domingo")]
        public DateTime? SundayStartDateTimeUtc { get; set; }
        [DisplayName("Domingo")]
        public DateTime? SundayEndDateTimeUtc { get; set; }

        [DisplayName("Habilita Comisión 2.0")]
        public virtual bool Comm20 { get; set; }

        [DisplayName("Monto Comisión 2.0")]
        public decimal AmountComm20 { get; set; }

        [DisplayName("Recibir Mensajería")]
        public bool ReceiveMessages { get; set; } = true;

        public IList<StoreLocalizedModel> Locales { get; set; }

        #endregion
    }

    public partial record StoreLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.Name")]
        public string Name { get; set; }
    }
}