﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace git_shopping_apps_prod.Databases.Models
{
    internal class Clients
    {
        public string _id { get; set; }
        [MinLength(5)]
        public string UserName { get; set; }
        [MinLength(8)]
        public string PassCode { get; set; }
    }
}
