using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FridgeHelper.Models;

namespace FridgeHelper.Models
{
    public class FrigEditViewModel
    {
        public IEnumerable<OwnFridges> Owner { get; set; }
        public int fId { get; set; }
        public string fName { get; set; }
        public string user { get; set; }
    }
}