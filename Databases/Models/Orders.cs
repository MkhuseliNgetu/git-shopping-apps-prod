using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace git_shopping_apps_prod.Databases.Models
{
    internal class Orders
    {
        [MinLength(5)]
        public string ClientUsername { get; set; }
        [MinLength(7)]
        public string ClientOrderID { get; set; }
        [MinLength(1)]
        public Dictionary<string, string[]> OrderProducts { get; set; }
    }
}
