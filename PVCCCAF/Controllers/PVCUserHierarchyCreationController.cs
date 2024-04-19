using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PVCCCAF.Models;
using Newtonsoft.Json;

namespace PVCCCAF.Controllers
{
    public class PVCUserHierarchyCreationController : Controller
    {
        // GET: PVCUserHierarchyCreation
        PVCCCAF.AppCode.Global obj = new AppCode.Global();
        public ActionResult PVCUserHierarchyCreation()
        {
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            ViewBag.AccountSelection = Enumerable.Empty<SelectListItem>();

            DataTable _dtUser = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_PVCGETUSERMASTER]", CommandType.StoredProcedure, _plist, _vlist);
            ViewBag.UserDropdown = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectListWithOption(_dtUser, "USERID", "USERNAME");            
            return View();
        }
        public JsonResult GetHierarchyDetails()
        {
            DataTable _dt = new DataTable();
            string json = string.Empty;
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();

            _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());
            _dt = PVCCCAF.AppCode.Global.GetData_New("GETHIERARCHYDETAILS", CommandType.StoredProcedure, _plist, _vlist);
            string DataResult = obj.ConvertDataTableTojSonString(_dt);
            return Json(DataResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveUserDetails(string USERID,string APPROVERL1P1,string APPROVERL1P2, string APPROVERL2P1, string APPROVERL2P2
            , string APPROVERL3P1, string APPROVERL3P2, string RELEASEUSER1,string RELEASEUSER2,string Addedit)
        {
                DataTable _dt = new DataTable();
                string json = string.Empty;

                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();

                _plist.Add("@USERID"); _vlist.Add(USERID);
                _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());
                _plist.Add("@CREATEDBY"); _vlist.Add(Session["USERCODE"].ToString());
                _plist.Add("@APPROVER_L1_P1"); _vlist.Add(APPROVERL1P1);
                _plist.Add("@APPROVER_L1_P2"); _vlist.Add(APPROVERL1P2);
                _plist.Add("@APPROVER_L2_P1"); _vlist.Add(APPROVERL2P1);
                _plist.Add("@APPROVER_L2_P2"); _vlist.Add(APPROVERL2P2);
                _plist.Add("@APPROVER_L3_P1"); _vlist.Add(APPROVERL3P1);
                _plist.Add("@APPROVER_L3_P2"); _vlist.Add(APPROVERL3P2);
                _plist.Add("@RELEASE_USER1"); _vlist.Add(RELEASEUSER1);
                _plist.Add("@RELEASE_USER2"); _vlist.Add(RELEASEUSER2);
                _plist.Add("@ADDTYPE"); _vlist.Add(Addedit);

                _dt = PVCCCAF.AppCode.Global.GetData_New("USP_PVCHIERARCHYUSERCREATION", CommandType.StoredProcedure, _plist, _vlist);
                string DataResult = obj.ConvertDataTableTojSonString(_dt);
            if(Addedit == "0")
            { return Json(new { success = true, responseText = "User Created Successfully" }, JsonRequestBehavior.AllowGet); }
            else
            {  return Json(new { success = true, responseText = "User Updated Successfully" }, JsonRequestBehavior.AllowGet); } 
              
        }
    }
}