using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace git_shopping_apps_prod.Databases.Models
{
    internal class Cards
    {
        [MinLength(7)]
        public string CardHolderFullName { get; set; }
        [MinLength(16)]
        public int CardNo { get; set; }
        [MinLength(2)]
        public int CardExpMonth { get; set; }
        [MinLength(2)]
        public int CardExpYear { get; set; }
        [MinLength(3)]
        public int CardCVV { get; set; }
    }
}
