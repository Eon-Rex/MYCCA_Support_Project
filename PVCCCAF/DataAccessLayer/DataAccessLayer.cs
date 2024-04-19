using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Runtime.InteropServices;
using PVCCCAF.Models;

namespace PVCCCAF.DataAccessLayer
{
    public class DataAccessLayer
    {
        public static DataTable GetMyProfile(string usercode)
        {
            DataTable dt = new DataTable();
            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@USERCODE"); _vlist.Add(usercode);
                dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCGETUSERPROFILE", CommandType.StoredProcedure, _plist, _vlist);
                dt.AcceptChanges();
            }
            catch (Exception ex)
            {

            }
            return dt;
        }
        public static DataTable GetOrderHistory(string usercode, string dataareaid, [Optional] string FromDate, [Optional] string ToDate)
        {
            DataTable dt = new DataTable();
            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@CUSTOMERID"); _vlist.Add(usercode);
                _plist.Add("@DATAAREAID"); _vlist.Add(dataareaid);
                if(!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(FromDate))
                {
                    _plist.Add("@FROMDATE"); _vlist.Add(FromDate);
                    _plist.Add("@TODATE"); _vlist.Add(ToDate);
                }
                dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXGETORDERHISTORY", CommandType.StoredProcedure, _plist, _vlist);
                dt.AcceptChanges();
            }
            catch (Exception ex)
            {

            }
            return dt;
        }
        public static void SaveUserAuditDataToDB(AuditTB userAuditTBVM)
        {
            DataTable dt = new DataTable();
            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@USERID"); _vlist.Add(userAuditTBVM.UserID);
                _plist.Add("@USERAUDITID"); _vlist.Add(userAuditTBVM.UsersAuditID.ToString());
                _plist.Add("@SESSIONID"); _vlist.Add(userAuditTBVM.SessionID);
                _plist.Add("@PAGEACCESSED"); _vlist.Add(userAuditTBVM.PageAccessed);
                _plist.Add("@LOGINSTATUS"); _vlist.Add(userAuditTBVM.LoginStatus);
                _plist.Add("@LOGGEDINAT"); _vlist.Add(userAuditTBVM.LoggedInAt.ToString());
                _plist.Add("@LOGGEDOUTAT"); _vlist.Add(userAuditTBVM.LoggedOutAt.ToString());
                _plist.Add("@IPADDRESS"); _vlist.Add(userAuditTBVM.IPAddress);
                _plist.Add("@CONTROLLERNAME"); _vlist.Add(userAuditTBVM.ControllerName);
                _plist.Add("@ACTIONNAME"); _vlist.Add(userAuditTBVM.ActionName);
                dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPOSTUSERAUDITDATATODB", CommandType.StoredProcedure, _plist, _vlist);
                dt.AcceptChanges();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
