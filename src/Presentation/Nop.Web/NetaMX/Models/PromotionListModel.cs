using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Models
{
    public partial record PromotionListModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PictureId { get; set; }
        public string PictureURL { get; set; }
    }
}
