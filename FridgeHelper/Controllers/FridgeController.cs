using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FridgeHelper.Models;

namespace FridgeHelper.Controllers
{
    [Authorize]
    public class FridgeController : Controller
    {
        FridgeHelperEntities db = new FridgeHelperEntities();


        //GET: fridge/frigList
        public ActionResult FrigList()
        {
            //撈出使用者擁有的所有冰箱冰箱
            var fridges = (from m in db.OwnFridges
                           join n in db.Fridges on m.fId equals n.fId
                           where m.account == User.Identity.Name
                           select n).ToList();

            return View(fridges);
        }

        //GET: fridge/FrigCreate
        public ActionResult FrigCreate()
        {
            return View();
        }

        //Post: fridge/FrigCreate
        [HttpPost]
        public ActionResult FrigCreate(Fridges fridge)
        {
            //驗證模型是否通過
            if (ModelState.IsValid)
            {
                //實體化新的使用者模型
                OwnFridges owntemp = new OwnFridges
                {
                    account = User.Identity.Name
                };
                //實體化新的冰箱模型
                Fridges newFridge = new Fridges
                {
                    fName = fridge.fName
                };
                //新增
                db.Fridges.Add(newFridge);
                db.OwnFridges.Add(owntemp);
                db.SaveChanges();
                //重導向回冰箱列表
                return RedirectToAction("FrigList");
            }
            else
            {
                return View(fridge);
            }
        }
        public ActionResult FrigEdit(int fridgeid)
        {
            //實體化編輯冰箱的viewmodel
            var model = new FrigEditViewModel
            {
                //當前冰箱的id
                fId = fridgeid,
                //撈出當前冰箱名稱
                fName = db.Fridges.Where(m => m.fId == fridgeid).FirstOrDefault().fName.ToString(),
                //撈出冰箱的其他使用者
                Owner = db.OwnFridges.Where(n => n.fId == fridgeid).ToList()
            };

            return View(model);
        }


        [HttpPost]
        public ActionResult FrigEdit(FrigEditViewModel frigEdit)
        {
            //為變更錯誤重新冰箱使用者提取資料
            frigEdit.Owner = db.OwnFridges.Where(n => n.fId == frigEdit.fId).ToList();
            //撈出當下冰箱資料
            var fdg = db.Fridges.Where(m => m.fId == frigEdit.fId).FirstOrDefault();
            string fname = fdg.fName.ToString();
            //確認是否有更改冰箱名稱
            if (frigEdit.fName != fname)
            {
                fdg.fName = frigEdit.fName;
                db.SaveChanges();
            }
            //撈出會員
            var member = db.Members
                        .Where(m => m.account == frigEdit.user)
                        .FirstOrDefault();
            //新增使用者不為空
            if (!string.IsNullOrWhiteSpace(frigEdit.user))
            {
                //使用者存在 且不存在當下使用者清單 則加入使用者為共用
                if (member != null)
                {
                    //確認輸入的使用者是否已經是該冰箱擁有者
                    var owner = db.OwnFridges.Where(x => x.fId == frigEdit.fId && x.account == frigEdit.user).FirstOrDefault();
                    if (owner == null)
                    {
                        OwnFridges owntemp = new OwnFridges
                        {
                            fId = frigEdit.fId,
                            account = frigEdit.user
                        };
                        //新增
                        db.OwnFridges.Add(owntemp);
                        db.SaveChanges();
                    }
                    else
                    {
                        ViewBag.error = "會員已經是冰箱使用者";
                        return View(frigEdit);
                    }
                }
                else
                {
                    ViewBag.error = "會員不存在";
                    return View(frigEdit);
                }
            }
            //重導向到編輯葉面 同重新整理之意思
            return RedirectToAction("FrigEdit", new { fridgeid = frigEdit.fId });
        }
        //GET: fridge/delete?fridgeid=
        public ActionResult Delete(int fridgeid)
        {
            try
            {
                //撈出該冰箱
                var fridge = (from m in db.Fridges
                              where m.fId == fridgeid
                              select m).FirstOrDefault();

                //撈出該冰箱相關之擁有者與食物全部刪除
                var del1 = db.OwnFridges.Where(m => m.fId == fridgeid);
                var del2 = db.Foods.Where(m => m.fId == fridgeid);
                db.OwnFridges.RemoveRange(del1);
                db.Foods.RemoveRange(del2);
                //刪除冰箱
                db.Fridges.Remove(fridge);
                db.SaveChanges();
            }
            catch (Exception)
            {
                ViewBag.ChangeError = true;
            }

            return RedirectToAction("FrigList", "Fridge");
        }

        //Get:fridge/ownerDel?fid=&&userid=
        public ActionResult OwnerDel(int fid, string userid)
        {
            //撈出該擁有者
            var owner = db.OwnFridges.Where(x => x.account == userid && x.fId == fid).FirstOrDefault();
            //移除
            db.OwnFridges.Remove(owner);
            db.SaveChanges();

            //重導向到編輯葉面 同重新整理之意思
            return RedirectToAction("FrigEdit", new { fridgeid = fid });
        }
    }
}