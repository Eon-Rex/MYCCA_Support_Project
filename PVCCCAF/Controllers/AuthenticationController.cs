using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Security;
using System.DirectoryServices.AccountManagement;
using PVCCCAF.Models;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PVCCCAF.Controllers
{
    public class AuthenticationController : Controller
    {

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login(string APPROVE)
        {
            LogOut();
            //Random objRandom = new Random();
            //#pragma warning disable 618
            //var seed = FormsAuthentication.HashPasswordForStoringInConfigFile(Convert.ToString(objRandom.Next()), "MD5").Substring(0, 16);
            //#pragma warning restore 618
            LoginViewModel cred = new LoginViewModel();
            cred.hdrandomseed = GetRandomSeed();
            APPROVE = APPROVE == null ? "0" : APPROVE;
            ViewBag.redirectval = APPROVE;
            return View("Login", cred);
        }

        [NonAction]
        public string GetRandomSeed()
        {
            Random objRandom = new Random();
            #pragma warning disable 618
            return FormsAuthentication.HashPasswordForStoringInConfigFile(Convert.ToString(objRandom.Next()), "MD5").Substring(0, 16);
            #pragma warning restore 618
        }

        //Login Form Post
        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(string submit, string _redirectval)
        {
            if (!Url.IsLocalUrl(Request.RawUrl.ToUpper()))
            {
                return RedirectToAction("Login");
            }
            try
            {
                bool _loginValid = false;
                LoginViewModel cred = new LoginViewModel();
                RequestOTPViewModel ReqOTP = new RequestOTPViewModel();
                SetPaswordViewModel SetPassVM = new SetPaswordViewModel();
                string _userType = string.Empty;
                string _AuhtorizationMode = string.Empty;
                switch (submit)
                {
                    case "SignIn":
                        if (ModelState.IsValid)
                        {
                            TryUpdateModel(cred);
                            cred = GetDecryptedCred(cred);
                            _userType = GetUserType(cred);
                            _AuhtorizationMode =GetUserAuthorizationMode(cred);
                            if (_AuhtorizationMode != null && _AuhtorizationMode == "AD")
                            {
                               string DomainName = System.Configuration.ConfigurationManager.AppSettings["ADAuthenticateDomain"].ToString();
                                _loginValid = IsADAuthenticate(DomainName, cred.UserId, cred.Password);
                                if (!_loginValid)
                                {
                                    throw new Exception("Incorrect UserID or Password. AD Authentiction Failed!");
                                }
                            }
                            else if (_AuhtorizationMode != null && _AuhtorizationMode == "DB")
                            {
                                _loginValid = IsSQLAuthenticateOnServer(cred.UserId, cred.Password);
                            }
                        }
                        break;

                    case "ForgetPasword":
                        TryUpdateModel(cred);
                        cred = GetDecryptedCred(cred);
                        if (ModelState.IsValid)
                        {
                            _userType = GetUserType(cred);
                            _AuhtorizationMode = GetUserAuthorizationMode(cred);
                            if (_AuhtorizationMode != null && _AuhtorizationMode == "AD")//_userType != null && _userType == "R02"
                            {
                                ModelState.AddModelError(cred.UserId, "Please Contact your CCA REP for This Request");
                                //ViewBag.div = @"<script type='text/javascript'> $(document).ready(function(){
                                //            debugger; 
                                //        var lblErrorMessage = 'Please Contact your CCA REP for Forget Password Request, ADUser.'; 
                                //        $('#lblErrorMessage').text(lblErrorMessage)
                                //        $('.demo-overlay').fadeIn('500'); });</script>";
                                //return View("Login", cred);
                                string _message = "Please Contact your CCA REP for Forget Password Request, ADUser.";
                                TempData["msg"] = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('" + _message + "');});</script>";
                                return RedirectToAction("Login");
                            }
                            else if (_userType != null && _AuhtorizationMode != null && _AuhtorizationMode == "DB")
                            {
                                bool mailsentresult = SendOTPForgetPassword(cred.UserId);
                                if (mailsentresult)
                                {
                                    DataTable dt = PVCCCAF.DataAccessLayer.DataAccessLayer.GetMyProfile(cred.UserId);
                                    string message = "OTP Has been sent to your Registered Email ID";
                                    if (dt.Rows.Count > 0)
                                        message = message + " : " + dt.Rows[0]["EMAIL"].ToString();
                                    ViewBag.div = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('" + message + "');});</script>";
                                    ReqOTP.UserId = cred.UserId;
                                    return View("ForgetPassword", ReqOTP);
                                }
                                else
                                {
                                    ModelState.AddModelError(cred.UserId, "OTP Cant be send, Please contact Admin!");
                                    //ViewBag.div = @"<script type='text/javascript'> $(document).ready(function(){
                                    //        debugger; 
                                    //    var lblErrorMessage = 'OTP Cant be send, Please contact CCA REP!'; 
                                    //    $('#lblErrorMessage').text(lblErrorMessage)
                                    //    $('.demo-overlay').fadeIn('500'); });</script>";
                                    //return View("Login", cred);
                                    string _message = "OTP Cant be send, Please contact CCA REP!";
                                    TempData["msg"] = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('" + _message + "');});</script>";
                                    return RedirectToAction("Login");
                                }
                            }
                        }
                        break;

                    case "OTPSubmit":
                        TryUpdateModel(ReqOTP);
                        if (ModelState.IsValid)
                        {
                            if (!string.IsNullOrEmpty(ReqOTP.OTP) && ValidateOTP(ReqOTP))
                            {
                                ViewBag.div = @"<script type='text/javascript'> $(document).ready(function(){
                                        var lblErrorMessage = 'Your OTP has been Validated, Please Set Password!'; 
                                        $('#lblErrorMessage').text(lblErrorMessage)
                                        $('.demo-overlay').fadeIn('500'); });</script>";
                                SetPassVM.OTP = ReqOTP.OTP;
                                SetPassVM.UserId = ReqOTP.UserId;
                                SetPassVM.hdrandomseed = GetRandomSeed();
                                return View("_SetPassword", SetPassVM);
                            }
                            else
                            {
                                ViewBag.div = @"<script type='text/javascript'> $(document).ready(function(){
                                        var lblErrorMessage = 'Wrong OTP. Please input correct OTP as shared in the Email.'; 
                                        $('#lblErrorMessage').text(lblErrorMessage)
                                        $('.demo-overlay').fadeIn('500'); });</script>";
                                return View("ForgetPassword", ReqOTP);
                            }
                        }

                        break;

                    case "SetPassword":
                        TryUpdateModel(SetPassVM);
                        SetPassVM = GetDecryptedSETVM(SetPassVM);
                        if (ModelState.IsValid)
                        {
                            if (DoSetPassword(SetPassVM))
                            {
                                string _message = "Password has been Updated. Please Login in to the System again with New Password.";
                                //ViewBag.div = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('" + _message + "');});</script>";
                                TempData["msg"] = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('" + _message + "');});</script>";
                                return RedirectToAction("Login");
                            }
                        }
                        else
                        {
                            StringBuilder _message = new StringBuilder();
                            _message.Append("You have a bunch of errors:");

                            foreach (ModelState modelState in ModelState.Values)
                            {
                                foreach (ModelError error in modelState.Errors)
                                {
                                    _message.Append(error + "\n");
                                }
                            }
                            ViewBag.div = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('" + _message + "');});</script>";
                            //TempData["msg"] = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('" + _message + "');});</script>";
                            return View("_SetPassword", SetPassVM);
                        }
                        break;
                    default:
                        return RedirectToAction("Login", "Authentication");
                        break;
                }

                if (_loginValid)
                {
                    FormsAuthentication.SetAuthCookie("PVCCCAF" + cred.UserId, false);
                    Session["USERCODE"] = cred.UserId.ToString();
                    Session["ROLECODE"] = _userType;
                    SetUserVisibleMenuOnLogin();
                    SetUserSessionVariableOnLogin();
                    return RedirectToAction("Dashboard", "Home");
                }
                else
                {
                    ViewBag.div = @"<script type='text/javascript'> $(document).ready(function(){
                                        var lblErrorMessage = 'Incorrect Username or Password.'; 
                                        $('#lblErrorMessage').text(lblErrorMessage)
                                        $('.demo-overlay').fadeIn('500'); });</script>";
                    string _message = "Incorrect Username or Password.";
                    TempData["msg"] = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('" + _message + "');});</script>";
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                //string terf = @"<script type='text/javascript'>$(document).ready(function(){ var lblErrorMessage = '"+ error + "'; $('#lblErrorMessage').text(lblErrorMessage) $('.demo-overlay').fadeIn('500'); });</script>";
                //ViewBag.div = terf;
                string _message = ex.Message.ToString();
                TempData["msg"] = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('" + _message + "');});</script>";
                return RedirectToAction("Login");
            }

        }
        [AllowAnonymous]
        [NonAction]
        public void LogOut()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            Session.Clear();
            Session.RemoveAll();
            //Removing ASP.NET_SessionId Cookie
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-10);
            }

            if (Request.Cookies["AuthenticationToken"] != null)
            {
                Response.Cookies["AuthenticationToken"].Value = string.Empty;
                Response.Cookies["AuthenticationToken"].Expires = DateTime.Now.AddMonths(-10);
            }
        }

        [NonAction]
        public bool IsADAuthenticate(string DomainName, string UserId, string Password)
        {
            bool IsValid = true;
            try
            {
                //try Validating using UserID
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, DomainName))
                {
                    IsValid = pc.ValidateCredentials(UserId, Password);
                }
                if (!IsValid)
                {
                    //try Validating using User EmailID
                    List<string> _plist = new List<string>();
                    List<string> _vlist = new List<string>();
                    _plist.Add("@USERCODE"); _vlist.Add(UserId);

                    DataTable _dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCGETUSERDETAILSTOSESSION",
                        System.Data.CommandType.StoredProcedure, _plist, _vlist);
                    if (_dt != null & _dt.Rows.Count > 0)
                        using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, DomainName))
                        {
                            IsValid = pc.ValidateCredentials(_dt.Rows[0]["EMAILID"].ToString(), Password);
                        }
                }
                return IsValid;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString() + " , Failed For AD User");
            }
        }

        [NonAction]
        public bool IsSQLAuthenticate(string userid, string password)
        {
            bool IsValid = false;
            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@USERCODE"); _vlist.Add(userid);
                _plist.Add("@PASSWORD"); _vlist.Add(BussinessLayer.BussinessLayer.Encrypt(password));
                DataTable dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCGETUSERLOGIN", System.Data.CommandType.StoredProcedure, _plist, _vlist);
                if (dt != null && dt.Rows.Count > 0)
                    IsValid = true;
                else
                    IsValid = false;
            }
            catch
            {
                IsValid = false;
            }
            return IsValid;
        }

        [NonAction]
        public bool IsSQLAuthenticateOnServer(string userid, string password)
        {
            bool IsValid = false;
            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@USERCODE"); _vlist.Add(userid);
                //_plist.Add("@PASSWORD"); _vlist.Add(BussinessLayer.CustomSecurityLayer.HashPassword(password));
                DataTable dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCGETUSERHASH", System.Data.CommandType.StoredProcedure, _plist, _vlist);
                if (dt != null && dt.Rows.Count > 0)
                    IsValid = BussinessLayer.CustomSecurityLayer.ValidatePassword(password, dt.Rows[0]["PASSWORD"].ToString());
                else
                    IsValid = false;
            }
            catch
            {
                IsValid = false;
            }
            return IsValid;
        }

        [HttpGet]
        public ActionResult Dashboard()
        {
            return RedirectToAction("Dashboard", "Home");
        }

        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View("ResetPassword");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult PostResetPassword()
        {
            try
            {
                bool _resetValid = false;
                ResetPasswordViewModel cred = new ResetPasswordViewModel();
                TryUpdateModel(cred);
                LoginViewModel cred1 = new LoginViewModel();
                cred1.UserId = Session["USERCODE"].ToString();
                string _userType = GetUserType(cred1);
                string _AuhtorizationMode = GetUserAuthorizationMode(cred1);
                string _currentPassword = string.Empty; string _newPassword = string.Empty;
                if (ModelState.IsValid)
                {
                    if (_AuhtorizationMode != null && _AuhtorizationMode == "AD")
                    {
                        _resetValid = false;
                        ModelState.AddModelError("", "Change Password is not applicable");
                        string _message = "Change Password is not applicable for AD User, Please Contact Admin.";
                        ViewBag.div = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('" + _message + "');});</script>";
                        return View("ResetPassword");
                    }
                    else if (_userType != null && _AuhtorizationMode == "DB")
                    {
                        _resetValid = DoResetPassword(cred.CurrentPassword, cred.NewPassword);
                    }
                }
                if (_resetValid)
                {
                    string _message = "Password has been Updated. Please Login in to the System again with New Password.";
                    ViewBag.div = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('" + _message + "');});</script>";
                    return View("Login");
                }
                else
                {
                    string _message = "Please Input Correct Password.";
                    ViewBag.div = "<script type='text/javascript'>$(document).ready(function() { ShowMessage('" + _message + "');});</script>";
                    return View("ResetPassword");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ResetPassword", "Authentication");
            }

        }

        [NonAction]
        private bool DoResetPassword(string currentPassword, string newPassword)
        {
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@USERCODE"); _vlist.Add(Session["USERCODE"].ToString());
            _plist.Add("@NEWPASSWORD"); _vlist.Add(PVCCCAF.BussinessLayer.CustomSecurityLayer.HashPassword(newPassword));
            if (!IsSQLAuthenticateOnServer(Session["USERCODE"].ToString(), currentPassword))
            {
                return false;
            }
            _plist.Add("@CURRENTPASSWORD"); _vlist.Add(PVCCCAF.BussinessLayer.CustomSecurityLayer.HashPassword(currentPassword));
            DataTable dt = AppCode.Global.GetData_New("USP_ACXPVCRESETPASSWORD", System.Data.CommandType.StoredProcedure, _plist, _vlist);
            if (dt != null && dt.Rows.Count > 0)
                return true;
            else
                return false;
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult PostForgetPassword(string userid, string materialOTP)
        {
            return View("_SetPassword");
        }

        //[HttpPost]
        //[AllowAnonymous]
        //public ActionResult PostSetPassword(string userid, string materialOTP,string NewPassword)
        //{
        //    //DoSetPassword(userid, materialOTP, NewPassword);
        //    return View("_SetPassword");
        //}

        [NonAction]
        private bool DoSetPassword(SetPaswordViewModel SPVM)
        {
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@USERCODE"); _vlist.Add(SPVM.UserId);
            _plist.Add("@NEWPASSWORD"); _vlist.Add(BussinessLayer.CustomSecurityLayer.HashPassword(SPVM.NewPassword));
            _plist.Add("@OTP"); _vlist.Add(SPVM.OTP);
            DataTable dt = AppCode.Global.GetData_New("USP_ACXPVCSETPASSWORD", System.Data.CommandType.StoredProcedure, _plist, _vlist);
            if (dt != null && dt.Rows.Count > 0)
                return true;
            else
                return false;
        }

        //Not In Use,
        //in Use from 10 Oct 2018
        [NonAction]
        private bool SendOTPForgetPassword(string username)
        {
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@USERID"); _vlist.Add(username);
            DataTable dt = AppCode.Global.GetData_New("USP_PVCGENERATEOTPREQUEST", System.Data.CommandType.StoredProcedure, _plist, _vlist);
            if (dt != null && dt.Rows.Count > 0)
                return true;
            else
                return false;
        }

        //Not In Use,
        [NonAction]
        public async Task<bool> SendForgetPasswordServer(string username)
        {
            //string parmObj= @"{"mailtoid":"sachin.bharadwaj @acxiomconsulting.com","mailsubject":"API Mail","mailbody":" < b > test </ b > Email Body from URL Parm."}"

            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@USERID"); _vlist.Add(username);
            DataTable _dt = AppCode.Global.GetData_New("USP_PVCGENERATEOTPREQUESTSERVER", System.Data.CommandType.StoredProcedure, _plist, _vlist);

            EmailParmObject emailParm = new EmailParmObject();
            emailParm.mailtoid = _dt.Rows[0]["TOEMAILID"].ToString();
            emailParm.mailccid = _dt.Rows[0]["CCEMAILID"].ToString();
            emailParm.mailsubject = _dt.Rows[0]["MAILSUBJECT"].ToString();
            emailParm.mailbody = _dt.Rows[0]["MAILBODY"].ToString(); ;

            string JSONparmObject = JsonConvert.SerializeObject(emailParm);

            BussinessLayer.Email emailobj = new BussinessLayer.Email();

            string response = await emailobj.SendEmail((int)_dt.Rows[0]["EVENTID"], JSONparmObject);
            if (response == "success")
                return true;
            else
                return false;
        }

        [NonAction]
        private bool ValidateOTP(RequestOTPViewModel ReqOTP)
        {
            DataTable dt = new DataTable();
            bool result = false;
            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@USERID"); _vlist.Add(ReqOTP.UserId);
                _plist.Add("@OTP"); _vlist.Add(ReqOTP.OTP);
                dt = AppCode.Global.GetData_New("USP_ACXPVCVALIDATEOTP", System.Data.CommandType.StoredProcedure, _plist, _vlist);
                if (dt != null && dt.Rows.Count > 0)
                    if (dt.Rows[0][0].ToString() == "Success")
                        result = true;
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }

        [NonAction]
        private static string GetUserType(LoginViewModel cred)
        {
            DataTable dt = new DataTable();
            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@USERCODE"); _vlist.Add(cred.UserId);
                dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCGETUSERTYPE", System.Data.CommandType.StoredProcedure, _plist, _vlist);
                if (dt != null && dt.Rows.Count > 0)
                    return dt.Rows[0][0].ToString();
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        [NonAction]
        private static string GetUserAuthorizationMode(LoginViewModel cred)
        {
            DataTable dt = new DataTable();
            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@USERCODE"); _vlist.Add(cred.UserId);
                dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCGETAUTHORIZATIONMODE", System.Data.CommandType.StoredProcedure, _plist, _vlist);
                if (dt != null && dt.Rows.Count > 0)
                    return dt.Rows[0]["AUTHORIZATIONMODE"].ToString();
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        [NonAction]
        private void SetUserVisibleMenuOnLogin()
        {
            DataTable _dtMenulist = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCGETMENUROLE",
                System.Data.CommandType.StoredProcedure, new List<string> { }, new List<string> { });
            Session["MENULIST"] = _dtMenulist;
        }

        [NonAction]
        private void SetUserSessionVariableOnLogin()
        {

            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@USERCODE"); _vlist.Add(Session["USERCODE"].ToString());


            DataTable _dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXPVCGETUSERDETAILSTOSESSION",
                System.Data.CommandType.StoredProcedure, _plist, _vlist);

            Session["USERCODE"] = _dt.Rows[0]["USERCODE"].ToString();
            Session["DILVMODE"] = _dt.Rows[0]["DIVMODE"].ToString();
            Session["PRICEGROUP"] = _dt.Rows[0]["PRICEGROUP"].ToString();
            Session["USERTYPE"] = _dt.Rows[0]["USERTYPE"].ToString();
            Session["DATAAREAID"] = _dt.Rows[0]["DATAAREAID"].ToString();
            Session["NAME"] = _dt.Rows[0]["Name"].ToString();
            Session["ADDRESS"] = _dt.Rows[0]["Street"].ToString();
            Session["CUSTOMERCODE"] = _dt.Rows[0]["AccountNum"].ToString();
            Session["CCARNAME"] = _dt.Rows[0]["CCARepName"].ToString();
            Session["CCAREMAIL"] = _dt.Rows[0]["CCARepEmailID"].ToString();
            Session["CCARCONTACT"] = _dt.Rows[0]["CCARepContact"].ToString();
            Session["ORDERTYPE"] = _dt.Rows[0]["ORDERTYPE"].ToString();
            Session["WOCUSTOMERTYPE"] = _dt.Rows[0]["WOCUSTOMERTYPE"].ToString();  //0 STAFF, 1 CUSTOMER
            Session["APIUser"] = System.Configuration.ConfigurationManager.AppSettings["APIUser"].ToString();//"ausdom\\sys_fjacxiom";
            Session["APIPassword"] = System.Configuration.ConfigurationManager.AppSettings["APIKey"].ToString();//"5y5_fj4cx10m";
        }

        private LoginViewModel GetDecryptedCred(LoginViewModel cred)
        {
            cred.Password = PVCCCAF.BussinessLayer.AESEncrytDecry.DecryptStringAES(cred.HDPassword, cred.hdrandomseed);
            cred.UserId = PVCCCAF.BussinessLayer.AESEncrytDecry.DecryptStringAES(cred.HDUserId, cred.hdrandomseed);
            return cred;
        }

        private SetPaswordViewModel GetDecryptedSETVM(SetPaswordViewModel cred)
        {
            cred.ConfirmPassword = PVCCCAF.BussinessLayer.AESEncrytDecry.DecryptStringAES(cred.HDConfirmPassword, cred.hdrandomseed);
            cred.NewPassword = PVCCCAF.BussinessLayer.AESEncrytDecry.DecryptStringAES(cred.HDConfirmPassword, cred.hdrandomseed);
            cred.UserId = PVCCCAF.BussinessLayer.AESEncrytDecry.DecryptStringAES(cred.UserId, cred.hdrandomseed);
            return cred;
        }
    }
}