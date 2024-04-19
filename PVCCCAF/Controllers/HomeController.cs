using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using PVCCCAF.Models;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Data.OleDb;
using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;
using PVCCCAF.App_Code;
using PVCCCAF.AXWO;


namespace PVCCCAF.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult MyProfile()
        {
            DataTable dt = PVCCCAF.DataAccessLayer.DataAccessLayer.GetMyProfile(Session["USERCODE"].ToString());
            dt.AcceptChanges();
            return View("_MyProfile", dt);
        }

        #region UserManagement

        [HttpGet]
        public ActionResult UserManagement()
        {
            return View("UserManagement");
        }

        public ActionResult GetUserManagement()
        {
            DataTable dt = new DataTable();
            PVCCCAF.AppCode.Global obj = new AppCode.Global();
            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());
                _plist.Add("@ADMINCODE"); _vlist.Add(Session["USERCODE"].ToString());
                dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCGETUSERMANAGEMENT", CommandType.StoredProcedure, _plist, _vlist);
                dt.AcceptChanges();

            }
            catch
            {

            }
            string DataResult = obj.ConvertDataTableTojSonString(dt);

            return Json(DataResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BlockUser(string id, string code, string userid)
        {
            if (Request.IsAjaxRequest())
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@USERCODE"); _vlist.Add(userid);
                _plist.Add("@TYPE"); _vlist.Add(id == "BLOCK" ? "1" : "2");
                DataTable dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCUPDATEUSERSTATUS", CommandType.StoredProcedure, _plist, _vlist);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return Content("SUCCESS");
                }
                return Content("FAILURE");
            }
            if (id == "NA")
            {
                TempData["msg"] = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('Not Applicable');});</script>";
            }
            else
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@USERCODE"); _vlist.Add(userid);
                _plist.Add("@TYPE"); _vlist.Add(id == "BLOCK" ? "1" : "2");
                DataTable dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCUPDATEUSERSTATUS", CommandType.StoredProcedure, _plist, _vlist);
                if (dt != null && dt.Rows.Count > 0)
                {
                    string _message = userid + " User Has been ";
                    _message += id == "BLOCK" ? "Blocked" : "UnBlocked.";
                    TempData["msg"] = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('" + _message + "');});</script>";
                }
            }
            return RedirectToAction("UserManagement", "Home");
        }
        [HttpGet]
        public ActionResult Create(string code, string userid, string UserName,
            string address, string contact, string Email, string Usertype, string RoleCode, string AuthorizationMode)
        {
            if (code == "EXISTING")
            {
                TempData["msg"] = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('The User Already Exists.');});</script>";
            }
            else if (code == "CREATE")
            {
                try
                {
                    string _uniquepassword = PVCCCAF.BussinessLayer.BussinessLayer.GetUniqueKey(6);
                    List<string> _plist = new List<string>();
                    List<string> _vlist = new List<string>();
                    _plist.Add("@USERCODE"); _vlist.Add(userid);
                    _plist.Add("@USERNAME"); _vlist.Add(UserName);
                    _plist.Add("@USERTYPE"); _vlist.Add(Usertype);
                    _plist.Add("@EMAILID"); _vlist.Add(string.IsNullOrEmpty(Email) ? "" : Email);
                    _plist.Add("@ROLECODE"); _vlist.Add(RoleCode);
                    _plist.Add("@CONTACTNO"); _vlist.Add(string.IsNullOrEmpty(contact) ? "" : contact);
                    _plist.Add("@AUTHORIZATIONMODE"); _vlist.Add(AuthorizationMode);
                    _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());
                    _plist.Add("@ADMINCODE"); _vlist.Add(Session["USERCODE"].ToString());
                    _plist.Add("@PASSWORD"); _vlist.Add(_uniquepassword);
                    _plist.Add("@PASSWORDENC"); _vlist.Add(BussinessLayer.CustomSecurityLayer.HashPassword(_uniquepassword));
                    PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCINSUSERMASTER", CommandType.StoredProcedure, _plist, _vlist);
                    string _message = "User Code:" + userid + " User has been Created.";
                    return Content("Success");
                    //TempData["msg"] = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('" + _message + "');});</script>";
                }
                catch (Exception ex)
                {
                    return Content(ex.Message.ToString());
                    //TempData["msg"] = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('" + ex.Message.ToString() + "');});</script>";
                }
            }
            return Content("EXISTING USER");
            //return RedirectToAction("UserManagement", "Home");
        }

        public ActionResult ChangeAuthMode(string code, string userid, string AuthorizationMode)
        {
            if (code == "CREATE")
            {
                return Content("Failure");
            }
            else if (code == "EXISTING")
            {
                try
                {
                    string _uniquepassword = PVCCCAF.BussinessLayer.BussinessLayer.GetUniqueKey(6);
                    string _message = string.Empty;
                    List<string> _plist = new List<string>();
                    List<string> _vlist = new List<string>();
                    _plist.Add("@ADMINCODE"); _vlist.Add(Session["USERCODE"].ToString());
                    _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());
                    _plist.Add("@AUTHORIZATIONMODE"); _vlist.Add(AuthorizationMode);
                    _plist.Add("@CUSTOMERID"); _vlist.Add(userid);
                    _plist.Add("@PASSWORD"); _vlist.Add(_uniquepassword);
                    _plist.Add("@PASSWORDENC"); _vlist.Add(PVCCCAF.BussinessLayer.CustomSecurityLayer.HashPassword(_uniquepassword));
                    DataTable dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCCHANGEUSERAUTHENTICATIONMODE", CommandType.StoredProcedure, _plist, _vlist);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        if (dt.Rows[0][0].ToString() == "Success")
                        {
                            return Content("Success");
                        }
                    }
                    return Content("Failure");
                }
                catch (Exception ex)
                {
                    TempData["msg"] = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('" + ex.Message.ToString() + "');});</script>";
                    return Content(ex.Message.ToString());
                }
            }
            return Content("Failure");
        }

        //[HttpGet]
        public ActionResult BlockUserTest()
        {
            ManageUserViewModel mjn = new ManageUserViewModel();
            TryUpdateModel(mjn);
            if (Request.IsAjaxRequest())
            {
                TempData["msg"] = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('Not Applicable');});</script>";
                return RedirectToAction("UserManagement", "Home");
                //return Content("Successfull");
            }
            TempData["msg"] = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('Not Applicable');});</script>";
            return RedirectToAction("UserManagement", "Home");
        }

        #endregion

    
     
        #region Dashboard


        [HttpGet]
        public ActionResult Dashboard()
        {
            return View("_Dashboard");
        }

        public JsonResult DashboardTab()
        {
            PVCCCAF.AppCode.Global obj = new AppCode.Global();
            DataTable dtusertyp = new DataTable();
            dtusertyp = GetPVCALLSTATUSDATAUSERLEVELWISE();
            string DataResult = obj.ConvertDataTableTojSonString(dtusertyp);
            return Json(DataResult, JsonRequestBehavior.AllowGet);
        }

        private DataTable getUserHierarchyLevel()
        {
            DataTable _dt = new DataTable();
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@DATAAREID"); _vlist.Add(Session["DATAAREAID"].ToString());
            _plist.Add("@USERID"); _vlist.Add(Session["USERCODE"].ToString());
            _dt = PVCCCAF.AppCode.Global.GetData_New("USP_GETPVCHIERARCHYBYUSERLEVEL", CommandType.StoredProcedure, _plist, _vlist);
            return _dt;
        }

        private DataTable GetPVCALLSTATUSDATAUSERLEVELWISE()
        {
            DataTable _dt = new DataTable();
            DataTable _userlevel = getUserHierarchyLevel();
            string query = string.Empty;
            string newquery = string.Empty;
            string DataAreaid = Session["DATAAREAID"].ToString();
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            if (_userlevel != null && _userlevel.Rows.Count > 0)
            {
               
                _userlevel = _userlevel.DefaultView.ToTable(true, "USERID", "LEVELID");
                foreach (DataRow _dtrow in _userlevel.Rows)
                {
                    string a = @"'<a href=""#"" onclick=""SendDataApproval('''+pvc.PVCNO + ''',''' +convert(nvarchar,pvc.STATUS )+ ''','''+pvc.SubmittedBy + ''')"" >'+pvc.PVCNO+'</a>'";


                    if (Convert.ToInt32(_dtrow["LEVELID"]) == 0)
                    {

                        query += "SELECT CASE WHEN PVC.STATUS IN (0,10) THEN 0 ELSE 1 END ISVIEWONLY ," + a + " PVCLINK, * FROM VW_GETPVCHISTORY PVC WHERE  " + Environment.NewLine;
                        query += " PVC.SubmittedBy = '" + _dtrow["USERID"].ToString() + "' AND PVC.STATUS IN (0,1,10) " + Environment.NewLine;
                        query += " UNION ALL ";
                    }
                    else if (Convert.ToInt32(_dtrow["LEVELID"]) == 1)
                    {
                        query += "SELECT  CASE WHEN PVC.STATUS IN (1) THEN 2 ELSE 1 END ISVIEWONLY ," + a + " PVCLINK, * FROM VW_GETPVCHISTORY PVC WHERE " + Environment.NewLine;
                        query += "  PVC.SubmittedBy = '" + _dtrow["USERID"].ToString() + "' AND PVC.STATUS IN(1,2,3) " + Environment.NewLine;
                        query += " UNION ALL ";
                    }
                    else if (Convert.ToInt32(_dtrow["LEVELID"]) == 2)
                    {
                        query += "SELECT  CASE WHEN PVC.STATUS IN (2) THEN 2 ELSE 1 END ISVIEWONLY , " + a + " PVCLINK, * FROM VW_GETPVCHISTORY PVC WHERE  " + Environment.NewLine;
                        query += "  PVC.SubmittedBy = '" + _dtrow["USERID"].ToString() + "' AND PVC.STATUS IN(2,4,5)  " + Environment.NewLine;
                        query += " UNION ALL ";
                    }
                    else if (Convert.ToInt32(_dtrow["LEVELID"]) == 3)
                    {
                        query += "SELECT  CASE WHEN PVC.STATUS IN (4) THEN 2 ELSE 1 END ISVIEWONLY ,  " + a + " PVCLINK, * FROM VW_GETPVCHISTORY PVC WHERE " + Environment.NewLine;
                        query += "  PVC.SubmittedBy = '" + _dtrow["USERID"].ToString() + "' AND PVC.STATUS IN(4,6,7) " + Environment.NewLine;
                        query += " UNION ALL ";
                    }
                    else if (Convert.ToInt32(_dtrow["LEVELID"]) == 4)
                    {
                        query += "SELECT  CASE WHEN PVC.STATUS IN (6) THEN 2 ELSE 1 END ISVIEWONLY , " + a + " PVCLINK, * FROM VW_GETPVCHISTORY PVC WHERE " + Environment.NewLine;
                        query += "  PVC.SubmittedBy = '" + _dtrow["USERID"].ToString() + "' AND PVC.STATUS IN(6,8,11) " + Environment.NewLine;
                        query += " UNION ALL ";
                      
                    }
                }

            }
            if (query.Length > 0)
            {
                query = query.Substring(0, query.Length - 10);
                newquery= "select  top 10 * from ( "+query + " ) A  ORDER BY PVCNO DESC";
            }
            _dt = PVCCCAF.AppCode.Global.GetData_New(newquery, CommandType.Text, _plist, _vlist);
            return _dt;

        }

        #endregion


    }
}