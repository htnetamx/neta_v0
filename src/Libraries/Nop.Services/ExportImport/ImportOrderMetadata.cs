using System.Collections.Generic;
using Nop.Core.Domain.Orders;
using Nop.Services.ExportImport.Help;

namespace Nop.Services.ExportImport
{
    public class ImportOrderMetadata
    {
        public int EndRow { get; internal set; }

        public PropertyManager<Order> Manager { get; internal set; }

        public IList<PropertyByName<Order>> Properties { get; set; }

        public int CountOrdersInFile => OrdersInFile.Count;

        public PropertyManager<ExportOrderAttribute> OrderAttributeManager { get; internal set; }

        public PropertyManager<ExportSpecificationAttribute> SpecificationAttributeManager { get; internal set; }

        public int IdCellNum { get; internal set; }

        public List<int> Allids { get; set; }

        public List<int> OrdersInFile { get; set; }
    }
}
