namespace Nop.Services.ExportImport
{
    public partial class ExportOrderAttribute
    {
        /// <summary>
        ///  Gets or sets the cell offset
        /// </summary>
        public static int OrderAttributeCellOffset { get; } = 2;

        /// <summary>
        /// Gets or sets the attribute identifier
        /// </summary>
        public int AttributeId { get; set; }

        /// <summary>
        /// Gets or sets the attribute route
        /// </summary>
        public string AttributeRoute { get; set; }

        /// <summary>
        /// Gets or sets the OrderId
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the attribute route
        /// </summary>
        public string Route { get; set; }
        
    }
}