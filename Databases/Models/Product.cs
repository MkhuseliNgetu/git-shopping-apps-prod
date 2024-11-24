using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace git_shopping_apps_prod.Databases.Models
{
    internal class Product
    {
        [MinLength(32)]
        public string ProductID { get; set; }
        [MinLength(3)]
        public string ProductName { get; set; }
        [MinLength(4)]
        public decimal ProductPrice { get; set; }
        [MinLength(3)]
        public string ProductColor { get; set; }
        [MinLength(2)]
        public int ProductSize { get; set; }
        [MinLength(2)]
        public bool ProductSpecialDiscountStatus { get; set; }
        [MinLength(2)]
        public decimal ProductSpecialDiscount { get; set; }
    }
}
