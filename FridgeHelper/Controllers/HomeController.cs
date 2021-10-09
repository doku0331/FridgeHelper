using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using FridgeHelper.Models;
namespace WebFinalProject.Controllers
{
    public class HomeController : Controller
    {
        FridgeHelperEntities db = new FridgeHelperEntities();

        public ActionResult Index()
        {
            var fridge = (from m in db.OwnFridges
                          join n in db.Fridges on m.fId equals n.fId
                          where m.account == User.Identity.Name
                          select n).ToList();

            var list = new List<Foods>();
            foreach (var item in fridge)
            {
                var allfood = db.Foods.Where(x => x.fId == item.fId).ToList();
                foreach (var food in allfood)
                {
                    if ((food.expire_date - DateTime.Now) < TimeSpan.FromDays(5))
                    {
                        list.Add(food);
                    }
                }
            }
            var expireds = (from x in list
                            join y in fridge on x.fId equals y.fId
                            select new ExpiredFoodViewModel
                            {
                                food_name = x.food_name,
                                expire_date = x.expire_date,
                                fName = y.fName
                            }).ToList();


            return View(expireds);
        }

    }
}