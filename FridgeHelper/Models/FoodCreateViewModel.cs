using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FridgeHelper.Models;

namespace FridgeHelper.Models
{
    public class FoodCreateViewModel
    {  public int fridgeId { get; set; }

        //新商品的圖片
        [DisplayName("食物圖片")]
        [Required(ErrorMessage = "請上傳圖片")]
        public HttpPostedFileBase ItemImage { get; set; }

        //新商品本體
        public Foods NewFood { get; set; }
    }
}