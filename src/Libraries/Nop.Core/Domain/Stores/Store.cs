using System;
using Nop.Core.Domain.Localization;

namespace Nop.Core.Domain.Stores
{
    /// <summary>
    /// Represents a store
    /// </summary>
    public partial class Store : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the store name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the store URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SSL is enabled
        /// </summary>
        public bool SslEnabled { get; set; }

        /// <summary>
        /// Gets or sets the comma separated list of possible HTTP_HOST values
        /// </summary>
        public string Hosts { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the default language for this store; 0 is set when we use the default language display order
        /// </summary>
        public int DefaultLanguageId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the company name
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the company address
        /// </summary>
        public string CompanyAddress { get; set; }

        /// <summary>
        /// Gets or sets the store phone number
        /// </summary>
        public string CompanyPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the company VAT (used in Europe Union countries)
        /// </summary>
        public string CompanyVat { get; set; }

        /// <summary>
        /// Gets or sets the company Latitude
        /// </summary>
        public decimal Latitud { get; set; }

        /// <summary>
        /// Gets or sets the company Longitude
        /// </summary>
        public decimal Longitud { get; set; }

        /// <summary>
        /// Gets or sets NetaCoin
        /// </summary>
        public decimal NetaCoin { get; set; }

        /// <summary>
        /// Gets or sets the date and time of order creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the zipcode
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// Gets or sets the bonus GMV flag
        /// </summary>
        public bool FirstGmvBonusApplied { get; set; }
        /// <summary>
        /// Gets or sets the first goal 15 clients flag
        /// </summary>
        public bool FirstGoalCustomerBonusApplied { get; set; }
        /// Gets or sets Monday Day Start
        /// </summary>
        public DateTime? MondayStartDateTimeUtc { get; set; }
        /// <summary>
        /// Gets or sets Monday Day End
        /// </summary>
        public DateTime? MondayEndDateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets Tuesday Day Start
        /// </summary>
        public DateTime? TuesdayStartDateTimeUtc { get; set; }
        /// <summary>
        /// Gets or sets Tuesday Day End
        /// </summary>
        public DateTime? TuesdayEndDateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets Wednesday Day Start
        /// </summary>
        public DateTime? WednesdayStartDateTimeUtc { get; set; }
        /// <summary>
        /// Gets or sets Wednesday Day End
        /// </summary>
        public DateTime? WednesdayEndDateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets Thursday Day Start
        /// </summary>
        public DateTime? ThursdayStartDateTimeUtc { get; set; }
        /// <summary>
        /// Gets or sets Thursday Day End
        /// </summary>
        public DateTime? ThursdayEndDateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets Friday Day Start
        /// </summary>
        public DateTime? FridayStartDateTimeUtc { get; set; }
        /// <summary>
        /// Gets or sets Friday Day End
        /// </summary>
        public DateTime? FridayEndDateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets Saturday Day Start
        /// </summary>
        public DateTime? SaturdayStartDateTimeUtc { get; set; }
        /// <summary>
        /// Gets or sets Saturday Day End
        /// </summary>
        public DateTime? SaturdayEndDateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets Sunday Day Start
        /// </summary>
        public DateTime? SundayStartDateTimeUtc { get; set; }
        /// <summary>
        /// Gets or sets Sunday Day End
        /// </summary>
        public DateTime? SundayEndDateTimeUtc { get; set; }

        public bool Comm20 { get; set; } = false;
        public decimal AmountComm20 { get; set; }
    }
}
