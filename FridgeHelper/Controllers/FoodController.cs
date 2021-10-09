using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FridgeHelper.Models;
using System.Security;
using PagedList;
using System.IO;

namespace FridgeHelper.Controllers
{
    [Authorize]
    public class FoodController : Controller
    {
        FridgeHelperEntities db = new FridgeHelperEntities();

        int pageSize = 5;

        #region 列出冰箱的內容
        //列出指定冰箱的內容
        // GET: Food/List?fridge=
        public ActionResult List(int fridgeid, int page = 1)
        {
            //找出冰箱跟其使用者資料
            var OwnFridge = db.OwnFridges.Where(x => x.fId == fridgeid && x.account == User.Identity.Name).FirstOrDefault();
            //傳送冰箱名稱
            ViewBag.FridgeName = db.Fridges.Where(x => x.fId == fridgeid).First().fName;

            //判斷使用者是否擁有該冰箱
            if (User.Identity.Name != OwnFridge.account)
            {
                ViewBag.error = true;
            }

            int currentPage = page < 1 ? 1 : page;

            //讓post可以傳入冰箱id 
            ViewBag.fridgeId = fridgeid;

            //把資料放入viewModel
            var model = new FoodListViewModel
            {
                //當前冰箱的id
                fridgeId = fridgeid,
                //冰箱內食物資料
                food = db.Foods.Where(m => m.fId == fridgeid).OrderBy(m => m.food_name).ToPagedList(currentPage, pageSize),
                //搜尋字串
                search = "",
                //現在頁數
                page = currentPage,
                //使用者的其他冰箱
                fridge = (from m in db.OwnFridges
                            join n in db.Fridges on m.fId equals n.fId
                            where m.account == User.Identity.Name
                            select n).ToList()
            };
            return View(model);
        }


        //POST:Food/List 
        [HttpPost]
        public ActionResult List(FoodListViewModel model)
        {
            //找出冰箱跟其使用者資料
            var OwnFridge = db.OwnFridges.Where(x => x.fId == model.fridgeId).FirstOrDefault();
            //傳送冰箱名稱
            ViewBag.FridgeName = db.Fridges.Where(x => x.fId == model.fridgeId).First().fName;

            //把當前的fridgeid傳入viewbag 使下次能夠知道在哪個冰箱
            ViewBag.fridgeId = model.fridgeId;
            //撈出所有在指定冰箱中的食物
            var query = db.Foods.Where(x => x.fId == model.fridgeId).AsQueryable();

            //搜尋字串不為空白則搜尋
            if (!string.IsNullOrWhiteSpace(model.search))
            {
                query = query.Where(
                    x => x.food_name.Contains(model.search));
            }
            //將搜尋到的資料按照日期排列
            query = query.OrderBy(x => x.expire_date);

            //頁數設定
            int currentPage = model.page < 1 ? 1 : model.page;

            //把資料放入viewModel
            var result = new FoodListViewModel
            {
                fridgeId = model.fridgeId,
                search = model.search,
                fridge = (from m in db.OwnFridges
                            join n in db.Fridges on m.fId equals n.fId
                            where m.account == User.Identity.Name
                            select n).ToList(),
                page = model.page < 1 ? 1 : model.page,
                food = query.ToPagedList(currentPage, pageSize)
            };

            return View(result);

        }
        #endregion
        #region 新增食物進指定冰箱

        // GET: Food/Create?fridge=
        public ActionResult Create(int fridgeid)
        {
            //提供類別的下拉選單
            List<SelectListItem> type = new List<SelectListItem>(){
            new SelectListItem { Text = "蔬果", Value = "蔬果" },
            new SelectListItem { Text = "肉食", Value = "肉食" },
            new SelectListItem { Text = "熟食", Value = "熟食" },
            new SelectListItem { Text = "其他", Value = "其他" },
        };
            ViewBag.type = type;

            //讓post可以傳入冰箱id 
            ViewBag.fridgeId = fridgeid;
            return View();
        }


        //POST:Food/Create 
        [HttpPost]
        public ActionResult Create(FoodCreateViewModel model)
        {
            //提供錯誤時返回view的fridgeId
            ViewBag.fridgeId = model.fridgeId;
            //提供錯誤時返回view的類別的下拉選單
            ViewBag.type = new List<SelectListItem>(){
            new SelectListItem { Text = "蔬果", Value = "蔬果" },
            new SelectListItem { Text = "肉食", Value = "肉食" },
            new SelectListItem { Text = "熟食", Value = "熟食" },
            new SelectListItem { Text = "其他", Value = "其他" },
        };

            //檢查image有沒有
            if (model.ItemImage != null)
            {
                //取得檔名
                string filename = Path.GetFileName(model.ItemImage.FileName);
                //將檔案和伺服器上路徑合併
                string Url = Path.Combine(Server.MapPath("~/Upload/"), filename);
                //將檔案儲存於伺服器上
                model.ItemImage.SaveAs(Url);
                //設定路徑
                model.NewFood.photo = filename;

            }
            else
            {
                ModelState.AddModelError("ItemImage", "請選擇上傳檔案");
                //返回
                return View(model);
            }

            //檢查模型有沒有通過驗證
            if (ModelState.IsValid != false)
            {
                model.NewFood.fId = model.fridgeId;
                try
                {
                    //將食物新增到資料表
                    db.Foods.Add(model.NewFood);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    ViewBag.ChangeError = true;
                }
                return RedirectToAction("List", "Food", new { fridgeid = model.fridgeId });
            }
            //模型未驗證通過
            return View(model);

        }
        #endregion

        public ActionResult delete(int fridgeid, string foodname)
        {
            try
            {
                var deletefood = db.Foods.Where(x => x.fId == fridgeid && x.food_name == foodname).FirstOrDefault();
                db.Foods.Remove(deletefood);
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                ViewBag.ChangeError = true;
            }

            return RedirectToAction("List", new { fridgeid = fridgeid });
        }

    }
}
