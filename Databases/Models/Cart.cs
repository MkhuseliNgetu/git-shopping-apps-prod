using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace git_shopping_apps_prod.Databases.Models
{
    internal class Cart
    {
        [MinLength(7)]
        public string CartID { get; set; }

        [MinLength(1)]
        public Dictionary<string, string[]> CartProducts { get; set; }

    }
}
