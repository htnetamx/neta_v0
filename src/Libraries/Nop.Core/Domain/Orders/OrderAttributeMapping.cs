using Nop.Core.Domain.Localization;

namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents a order attribute mapping
    /// </summary>
    public partial class OrderAttributeMapping : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the order attribute identifier
        /// </summary>
        public int OrderAttributeId { get; set; }

        /// <summary>
        /// Gets or sets a value a text prompt
        /// </summary>
        public string TextPrompt { get; set; }

        /// <summary>
        /// Gets or sets Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets Route
        /// </summary>
        public string Route { get; set; }



        ///// <summary>
        ///// Gets the attribute control type
        ///// </summary>
        //public AttributeControlType AttributeControlType
        //{
        //    get => (AttributeControlType)AttributeControlTypeId;
        //    set => AttributeControlTypeId = (int)value;
        //}
    }
}