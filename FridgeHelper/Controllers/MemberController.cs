using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using FridgeHelper.Models;

namespace FridgeHelper.Controllers
{
    public class MemberController : Controller
    {
        // 建立存取資料庫的實體
        FridgeHelperEntities db = new FridgeHelperEntities();


        //Get: Member/Login
        public ActionResult Login()
        {
            return View();
        }
        //Post: Member/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string account, string password)
        {
            // 依帳密取得會員並指定給member
            var member = db.Members
                .Where(m => m.account == account && m.password == password)
                .FirstOrDefault();
            //若member為null，表示會員未註冊
            if (member == null)
            {
                ViewBag.Message = "帳密錯誤，登入失敗";
                return View();
            }
            //使用Session變數記錄歡迎詞
            Session["WelCome"] = member.name + "歡迎";
            FormsAuthentication.RedirectFromLoginPage(account, true);
            return RedirectToAction("Index", "Home");
        }

        //Get:Member/Register
        public ActionResult Register()
        {
            return View();
        }

        //Post:Member/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Members newMember)
        {
            //若模型沒有通過驗證則顯示目前的View
            if (ModelState.IsValid == false)
            {
                return View();
            }
            // 依帳號取得會員並指定給member
            var member = db.Members
                .Where(m => m.account == newMember.account)
                .FirstOrDefault();

            //若member為null，表示會員未註冊
            if (member == null)
            {
                //將會員記錄新增到tMember資料表
                db.Members.Add(newMember);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "未知原因註冊失敗，請聯絡管理者";
                    return View();
                }
                //執行Home控制器的Index動作方法
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Message = "此帳號己有人使用，註冊失敗";
            return View();
        }
        public ActionResult Logout()
        {

            //清除Session中的資料
            Session.Abandon();
            //登出表單驗證
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    
    }
}