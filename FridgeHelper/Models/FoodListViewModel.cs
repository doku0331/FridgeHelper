using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//驗證模型的函式庫
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PagedList;
using FridgeHelper.Models;

namespace FridgeHelper.Models
{
    public class FoodListViewModel
    {
        public int fridgeId { get; set; }
        public IEnumerable<Fridges> fridge { get; set; }
        public IPagedList<Foods> food { get; set; }
        public string search { get; set; }
        public int page { get; set; }
    }
}