using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using PVCCCAF.Models;

namespace PVCCCAF.Controllers
{
    public class RoleMenuController : Controller
    {
        // GET: RoleMenu
        PVCCCAF.AppCode.Global obj = new PVCCCAF.AppCode.Global();
        public ActionResult RoleMenu()
        {
            return View("_RoleMenuUserLinking");
        }
        
        [HttpPost]
        public ActionResult BindRole()
        {

            List<Role> prd1 = new List<Role>();
            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();

                _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());

                DataTable _dtObj1 = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCUSERROLE", System.Data.CommandType.StoredProcedure, _plist, _vlist);

                int i;
                for (i = 0; i < _dtObj1.Rows.Count; i++)
                {

                    prd1.Add(new Role(_dtObj1.Rows[i]["ROLECODE"].ToString()));
                }
                return Json(prd1, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public void GetDragValI(string dragitem, string rolec)
        {
            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@MENU"); _vlist.Add(dragitem);
                _plist.Add("@ROLEC"); _vlist.Add(rolec);
                _plist.Add("@ADMINCODE"); _vlist.Add(Session["USERCODE"].ToString());
                _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());

                DataTable dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCInsertMenu", System.Data.CommandType.StoredProcedure, _plist, _vlist);
            }
            catch (Exception ex)
            {

            }

        }

        public void GetDragValR(string dragitem, string rolec)
        {
            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@MENU"); _vlist.Add(dragitem);
                _plist.Add("@ROLEC"); _vlist.Add(rolec);
                _plist.Add("@ADMINCODE"); _vlist.Add(Session["USERCODE"].ToString());
                _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());

                DataTable dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCUpdateRoleMenuMasterStatus", System.Data.CommandType.StoredProcedure, _plist, _vlist);
            }
            catch (Exception ex)
            {

            }

        }

        public void GetDragValIu(string dragitem, string rolec)
        {


            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
              
                _plist.Add("@USER"); _vlist.Add(dragitem);
                _plist.Add("@ROLEC"); _vlist.Add(rolec);
                _plist.Add("@ADMINCODE"); _vlist.Add(Session["USERCODE"].ToString());
                _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());

                DataTable dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCInsertUser", System.Data.CommandType.StoredProcedure, _plist, _vlist);
            }
            catch (Exception ex)
            {

            }

        }

        public void GetDragValRu(string dragitem, string rolec)
        {


            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
               
                _plist.Add("@USER"); _vlist.Add(dragitem);
                _plist.Add("@ROLEC"); _vlist.Add(rolec);
                _plist.Add("@ADMINCODE"); _vlist.Add(Session["USERCODE"].ToString());
                _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());

                DataTable dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCRemoveUser", System.Data.CommandType.StoredProcedure, _plist, _vlist);
            }
            catch (Exception ex)
            {

            }

        }

        public ActionResult LinkRole(String usr)
        {

            List<Menu> prd = new List<Menu>();
            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@RoleCode"); _vlist.Add(usr);
                _plist.Add("@ADMINCODE"); _vlist.Add(Session["USERCODE"].ToString());
                _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());

                DataTable _dtObj1 = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCUSERROLELinking", System.Data.CommandType.StoredProcedure, _plist, _vlist);
                int i;
                for (i = 0; i < _dtObj1.Rows.Count; i++)
                {

                    prd.Add(new Menu(_dtObj1.Rows[i]["DESCRIPTION"].ToString()));
                }
                return Json(prd, JsonRequestBehavior.AllowGet); ;

            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public ActionResult NLinkRole(String usr)
        {

            List<Menu> prd = new List<Menu>();
            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@RoleCode"); _vlist.Add(usr);
                _plist.Add("@ADMINCODE"); _vlist.Add(Session["USERCODE"].ToString());
                _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());

                DataTable _dtObj1 = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCUSERROLENLinking", System.Data.CommandType.StoredProcedure, _plist, _vlist);
                int i;
                for (i = 0; i < _dtObj1.Rows.Count; i++)
                {

                    prd.Add(new Menu(_dtObj1.Rows[i]["DESCRIPTION"].ToString()));
                }
                return Json(prd, JsonRequestBehavior.AllowGet); ;

            }
            catch (Exception ex)
            {
                return null;
            }

        }
        //Linking for user
        public ActionResult LinkRole1(String usr)
        {

            List<User> prd = new List<User>();
            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@RoleCode"); _vlist.Add(usr);
                _plist.Add("@ADMINCODE"); _vlist.Add(Session["USERCODE"].ToString());
                _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());

                DataTable _dtObj1 = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCLINKROLE", System.Data.CommandType.StoredProcedure, _plist, _vlist);
                int i;
                for (i = 0; i < _dtObj1.Rows.Count; i++)
                {

                    prd.Add(new User(_dtObj1.Rows[i]["USERCODE"].ToString()));
                }
                return Json(prd, JsonRequestBehavior.AllowGet); ;

            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public ActionResult NLinkRole1(String usr)
        {

            List<User> prd = new List<User>();
            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@RoleCode"); _vlist.Add(usr);
                _plist.Add("@ADMINCODE"); _vlist.Add(Session["USERCODE"].ToString());
                _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());

                DataTable _dtObj1 = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCNLINKROLE", System.Data.CommandType.StoredProcedure, _plist, _vlist);
                int i;
                for (i = 0; i < _dtObj1.Rows.Count; i++)
                {

                    prd.Add(new User(_dtObj1.Rows[i]["USERCODE"].ToString()));
                }
                return Json(prd, JsonRequestBehavior.AllowGet); ;

            }
            catch (Exception ex)
            {
                return null;
            }

        }
    }
}