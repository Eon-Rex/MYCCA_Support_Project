using PVCCCAF.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PVCCCAF.BussinessLayer
{
    public class Email
    {
        public async Task<string> SendEmail(int eventid, string parmObj)
        {
            DataTable dt = GetEmailConfigProfile(eventid);

            SmtpClient smtpClient = new SmtpClient(dt.Rows[0]["SERVERNAME"].ToString(), (int)dt.Rows[0]["PORTNO"]);

            smtpClient.UseDefaultCredentials = false;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = (bool)dt.Rows[0]["REQUIRESSL"];
            smtpClient.Credentials = new System.Net.NetworkCredential(dt.Rows[0]["FROMEMAILID"].ToString(), dt.Rows[0]["PASSWORD"].ToString());

            MailMessage mail = new MailMessage();

            //Setting From EmailID from SQL Config Profile Table
            mail.From = new MailAddress(dt.Rows[0]["FROMEMAILID"].ToString(), dt.Rows[0]["EVENTDESC"].ToString());
            //Getting URL Parameters
            DataTable dtParmList = new DataTable();
            try
            {
                if (parmObj.Length > 0)
                    dtParmList = (DataTable)JsonConvert.DeserializeObject("[" + parmObj + "]", (typeof(DataTable)));
            }
            catch (Exception ex)
            {
                return "Wrong Parameter Construction for parmObj, " + ex.Message.ToString();
            }

            DataColumnCollection URLcolumns = dtParmList.Columns;
            DataColumnCollection SQLcolumns = dt.Columns;
            StringBuilder _sbstring = new StringBuilder();

            //Setting Body and Subject from URL
            //mail.Body = dtParmList.Rows[0]["mailbody"].ToString();
            mail.Subject = dtParmList.Rows[0]["mailsubject"].ToString();
            mail.IsBodyHtml = true;
          
            mail.Body = dtParmList.Rows[0]["mailbody"].ToString();
            //Setting TO from URL and SQL Config Table
            _sbstring = new StringBuilder();
            if (SQLcolumns.Contains("TOEMAILID"))
                _sbstring.Append(dt.Rows[0]["TOEMAILID"].ToString());
            if (URLcolumns.Contains("mailtoid"))
                _sbstring.Append(dtParmList.Rows[0]["mailtoid"].ToString());

            string[] _tomailparm = _sbstring.ToString().Split(';');
            for (int i = 0; i < _tomailparm.Length; i++)
                if (_tomailparm[i].Trim().Length > 0)
                    mail.To.Add(new MailAddress(_tomailparm[i].Trim()));

            //Setting CC from URL and SQL Config Table
            _sbstring.Clear();
            if (SQLcolumns.Contains("CCEMAILID"))
                _sbstring.Append(dt.Rows[0]["CCEMAILID"].ToString());
            if (URLcolumns.Contains("mailccid"))
                _sbstring.Append(dtParmList.Rows[0]["mailccid"].ToString());

            string[] _ccmailparm = _sbstring.ToString().Split(';');
            for (int i = 0; i < _ccmailparm.Length; i++)
                if (_ccmailparm[i].Trim().Length > 0)
                    mail.CC.Add(new MailAddress(_ccmailparm[i].Trim()));

            //Setting BCC from URL and SQL Config Table
            _sbstring.Clear();
            if (SQLcolumns.Contains("BCCEMAILID"))
                _sbstring.Append(dt.Rows[0]["BCCEMAILID"].ToString());
            if (URLcolumns.Contains("mailbccid"))
                _sbstring.Append(dtParmList.Rows[0]["mailbccid"].ToString());

            string[] _bccmailparm = _sbstring.ToString().Split(';');
            for (int i = 0; i < _bccmailparm.Length; i++)
                if (_bccmailparm[i].Trim().Length > 0)
                    mail.Bcc.Add(new MailAddress(_bccmailparm[i].Trim()));
            try
            {
                //smtpClient.Send(mail);
                await smtpClient.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                InsertEmailLog(eventid: eventid, eventname: dt.Rows[0]["EVENTDESC"].ToString(), mailfrom: dt.Rows[0]["FROMEMAILID"].ToString(),
                    password: dt.Rows[0]["PASSWORD"].ToString(), servername: dt.Rows[0]["SERVERNAME"].ToString(), portno: (int)dt.Rows[0]["PORTNO"],
                    requiressl: dt.Rows[0]["REQUIRESSL"].ToString(), mailto: mail.To.ToString(), mailcc: mail.CC.ToString(),
                    mailbcc: mail.Bcc.ToString(), mailsubject: mail.Subject, mailbody: mail.Body, sendstatus: "failure", errordescription: ex.Message.ToString(), parmObj: parmObj);
                return "failure";
                PVCCCAF.AppCode.Global.WriteLog("EMail C#: "+ex.Message.ToString(), "\\LogFiles\\ErrorLog\\MailError" + DateTime.Now.ToString("ddMMyyyy") + ".log");
            }
            InsertEmailLog(eventid: eventid, eventname: dt.Rows[0]["EVENTDESC"].ToString(), mailfrom: dt.Rows[0]["FROMEMAILID"].ToString(),
                   password: dt.Rows[0]["PASSWORD"].ToString(), servername: dt.Rows[0]["SERVERNAME"].ToString(), portno: (int)dt.Rows[0]["PORTNO"],
                   requiressl: dt.Rows[0]["REQUIRESSL"].ToString(), mailto: mail.To.ToString(), mailcc: mail.CC.ToString(),
                   mailbcc: mail.Bcc.ToString(), mailsubject: mail.Subject, mailbody: mail.Body, sendstatus: "success", errordescription: "", parmObj: parmObj);
            return "success";
        }

        public DataTable GetEmailConfigProfile(int id)
        {
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@EVENTID"); _vlist.Add(id.ToString());
            DataTable _dt = new DataTable();
            _dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXGETMAILCONFIGBYID", CommandType.StoredProcedure, _plist, _vlist);
            return _dt;
        }

        public void InsertEmailLog(int eventid, string eventname, string mailfrom, string password, string servername, int portno, string requiressl
            , string mailto, string mailcc, string mailbcc, string mailsubject, string mailbody, string sendstatus, string errordescription, string parmObj)
        {
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@EVENTID"); _vlist.Add(eventid.ToString());
            _plist.Add("@EVENTDESCRIPTION"); _vlist.Add(eventname);
            _plist.Add("@FROMEMAILID"); _vlist.Add(mailfrom);
            _plist.Add("@PASSWORD"); _vlist.Add(password);
            _plist.Add("@SERVERNAME"); _vlist.Add(servername);
            _plist.Add("@PORTNO"); _vlist.Add(portno.ToString());
            _plist.Add("@REQUIRESSL"); _vlist.Add(requiressl);
            _plist.Add("@MAILSUBJECT"); _vlist.Add(mailsubject);
            _plist.Add("@MAILBODY"); _vlist.Add(mailbody);
            _plist.Add("@TOEMAILID"); _vlist.Add(mailto);
            _plist.Add("@CCEMAILID"); _vlist.Add(mailcc);
            _plist.Add("@BCCEMAILID"); _vlist.Add(mailbcc);
            _plist.Add("@SENDSTATUS"); _vlist.Add(sendstatus);
            _plist.Add("@ERRORDESCRIPTION"); _vlist.Add(errordescription);
            _plist.Add("@PARMOBJECT"); _vlist.Add(parmObj);

            DataTable _dt = new DataTable();
            try
            {
                _dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXINSEMAILLOG", CommandType.StoredProcedure, _plist, _vlist);
            }
            catch (Exception ex)
            {

            }

        }

        public async Task SendApproveOrderEmail(string weborder, string customerid, string status)
        {
            //string parmObj= @"{"mailtoid":"sachin.bharadwaj @acxiomconsulting.com","mailsubject":"API Mail","mailbody":" < b > test </ b > Email Body from URL Parm."}"
            string introline = string.Empty;
            if (status == "2")
            {
                introline = "Your order has been processed. The following are the confirmed order details:";
            }
            else
            {
                introline = "Your order has been cancelled. The following are the cancelled order details:";
            }
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@CUSTOMERID"); _vlist.Add(customerid);
            _plist.Add("@USERCODE"); _vlist.Add(HttpContext.Current.Session["USERCODE"].ToString());
            _plist.Add("@DATAAREAID"); _vlist.Add(HttpContext.Current.Session["DATAAREAID"].ToString());
            _plist.Add("@WEBORDERNO"); _vlist.Add(weborder);
            DataTable _dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXGETEMAILBODYFORAPPROVAL", CommandType.StoredProcedure, _plist, _vlist);
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/EmailTemplates/ApproveOrderTemplate.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{UserName}", _dt.Rows[0]["CUSTOMERNAME"].ToString());
            body = body.Replace("{OrderNo}", _dt.Rows[0]["WEBORDERNO"].ToString());
            body = body.Replace("{OrderDate}", _dt.Rows[0]["ORDERDATE"].ToString());
            body = body.Replace("{DeliveryDate}", _dt.Rows[0]["DELIVERYDATE"].ToString());
            body = body.Replace("{OrderAmount}", _dt.Rows[0]["TOTALAMOUNT"].ToString());
            body = body.Replace("{IntroLine}", _dt.Rows[0]["INTROLINE"].ToString());
            body = body.Replace("{Title}", _dt.Rows[0]["TITLE"].ToString());
            string _LineTable = "<tbody>";

            foreach (DataRow dr in _dt.Rows)
            {
                _LineTable += "<tr><td style='width: 56px;'>" + dr["SRNO"].ToString();
                _LineTable += "</td><td style='width: 278px;'>" + dr["PRODUCTNAME"].ToString();
                _LineTable += "</td><td style='width: 77px;text-align: center;'>" + dr["CONFIRMQTY"].ToString();
                _LineTable += "</td><td style='width: 109px;text-align: right;'>" + dr["EXTPRICEVIP"].ToString() + "</td></tr>";
            }
            body = body.Replace("{LineTable}", _LineTable);

            EmailParmObject emailParm = new EmailParmObject();
            emailParm.mailtoid = _dt.Rows[0]["TOEMAILID"].ToString();
            emailParm.mailccid = _dt.Rows[0]["CCEMAILID"].ToString();
            emailParm.mailsubject = _dt.Rows[0]["MAILSUBJECT"].ToString();
            emailParm.mailbody = body;

            string JSONparmObject = JsonConvert.SerializeObject(emailParm);

            // await SendEmail((int)_dt.Rows[0]["EVENTID"], JSONparmObject);
            string response = SendDBMail(emailParm);

        }

        public void SendFOCOrderEmail(string weborder, string customerid, string status)
        {
            string introline = string.Empty;
            if (status == "2")
            {
                introline = "Your order has been processed. The following are the confirmed order details:";
            }
            else
            {
                introline = "Your order has been cancelled. The following are the cancelled order details:";
            }
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@CUSTOMERID"); _vlist.Add(customerid);
            _plist.Add("@USERCODE"); _vlist.Add(HttpContext.Current.Session["USERCODE"].ToString());
            _plist.Add("@DATAAREAID"); _vlist.Add(HttpContext.Current.Session["DATAAREAID"].ToString());
            _plist.Add("@WEBORDERNO"); _vlist.Add(weborder);
            DataTable _dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXGETEMAILBODYFORAPPROVAL", CommandType.StoredProcedure, _plist, _vlist);
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/EmailTemplates/ApproveOrderTemplate.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{UserName}", _dt.Rows[0]["CUSTOMERNAME"].ToString());
            body = body.Replace("{OrderNo}", _dt.Rows[0]["WEBORDERNO"].ToString());
            body = body.Replace("{OrderDate}", _dt.Rows[0]["ORDERDATE"].ToString());
            body = body.Replace("{DeliveryDate}", _dt.Rows[0]["DELIVERYDATE"].ToString());
            body = body.Replace("{OrderAmount}", _dt.Rows[0]["TOTALAMOUNT"].ToString());
            body = body.Replace("{IntroLine}", _dt.Rows[0]["INTROLINE"].ToString());
            body = body.Replace("{Title}", _dt.Rows[0]["TITLE"].ToString());
            string _LineTable = "<tbody>";

            foreach (DataRow dr in _dt.Rows)
            {
                _LineTable += "<tr><td style='width: 56px;'>" + dr["SRNO"].ToString();
                _LineTable += "</td><td style='width: 278px;'>" + dr["PRODUCTNAME"].ToString();
                _LineTable += "</td><td style='width: 77px;text-align: center;'>" + dr["CONFIRMQTY"].ToString();
                _LineTable += "</td><td style='width: 109px;text-align: right;'>" + dr["EXTPRICEVIP"].ToString() + "</td></tr>";
            }
            body = body.Replace("{LineTable}", _LineTable);

            EmailParmObject emailParm = new EmailParmObject();
            emailParm.mailtoid = _dt.Rows[0]["TOEMAILID"].ToString();
            emailParm.mailccid = _dt.Rows[0]["CCEMAILID"].ToString();
            emailParm.mailsubject = _dt.Rows[0]["MAILSUBJECT"].ToString();
            emailParm.mailbody = body;

            string JSONparmObject = JsonConvert.SerializeObject(emailParm);

            // await SendEmail((int)_dt.Rows[0]["EVENTID"], JSONparmObject);
            string response = SendDBMail(emailParm);

        }

        public async Task SendSaveOrderEmail(string weborder, string customerid, string status)
        {
            //string parmObj= @"{"mailtoid":"sachin.bharadwaj @acxiomconsulting.com","mailsubject":"API Mail","mailbody":" < b > test </ b > Email Body from URL Parm."}"
            string introline = string.Empty;
            if (status == "1")
            {
                introline = "Thank you for your order. The following are your order details:";
            }
            else
            {
                return;
            }
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@CUSTOMERID"); _vlist.Add(customerid);
            _plist.Add("@USERCODE"); _vlist.Add(HttpContext.Current.Session["USERCODE"].ToString());
            _plist.Add("@DATAAREAID"); _vlist.Add(HttpContext.Current.Session["DATAAREAID"].ToString());
            _plist.Add("@WEBORDERNO"); _vlist.Add(weborder);
            DataTable _dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXGETEMAILBODYFORORDERSAVE", CommandType.StoredProcedure, _plist, _vlist);
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/EmailTemplates/SaveOrderTemplate.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{UserName}", _dt.Rows[0]["CUSTOMERNAME"].ToString());
            body = body.Replace("{OrderNo}", _dt.Rows[0]["WEBORDERNO"].ToString());
            body = body.Replace("{OrderDate}", _dt.Rows[0]["ORDERDATE"].ToString());
            body = body.Replace("{DeliveryDate}", _dt.Rows[0]["DELIVERYDATE"].ToString());
            body = body.Replace("{OrderAmount}", _dt.Rows[0]["TOTALAMOUNT"].ToString());
            body = body.Replace("{IntroLine}", introline);
            string _LineTable = "<tbody>";

            foreach (DataRow dr in _dt.Rows)
            {
                _LineTable += "<tr><td style='width: 56px;'>" + dr["SRNO"].ToString();
                _LineTable += "</td><td style='width: 278px;'>" + dr["PRODUCTNAME"].ToString();
                _LineTable += "</td><td style='width: 77px;text-align: center;'>" + dr["CONFIRMQTY"].ToString();
                _LineTable += "</td><td style='width: 109px;text-align: right;'>" + dr["EXTPRICEVIP"].ToString() + "</td></tr>";
            }
            body = body.Replace("{LineTable}", _LineTable);
            body = body.Replace("{CCAREPNAME}", _dt.Rows[0]["CCAREPNAME"].ToString());
            body = body.Replace("{CCAEMAIL}", _dt.Rows[0]["CCAREPEMAILID"].ToString());
            body = body.Replace("{CCACONTACT}", _dt.Rows[0]["CCAREPCONTACT"].ToString());

            EmailParmObject emailParm = new EmailParmObject();
            emailParm.mailtoid = _dt.Rows[0]["TOEMAILID"].ToString();
            emailParm.mailccid = _dt.Rows[0]["CCEMAILID"].ToString();
            emailParm.mailsubject = _dt.Rows[0]["MAILSUBJECT"].ToString();
            emailParm.mailbody = body;

            string JSONparmObject = JsonConvert.SerializeObject(emailParm);

            //BussinessLayer.Email emailobj = new BussinessLayer.Email();
            string response = SendDBMail(emailParm);
            //await SendEmail((int)_dt.Rows[0]["EVENTID"], JSONparmObject);
        }

        //Not used Currently
        private string PopulateBodyForOrderApprovel(string weborder, string customerid)
        {
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@CUSTOMERID"); _vlist.Add(customerid);
            _plist.Add("@USERCODE"); _vlist.Add(HttpContext.Current.Session["USERCODE"].ToString());
            _plist.Add("@DATAAREAID"); _vlist.Add(HttpContext.Current.Session["DATAAREAID"].ToString());
            _plist.Add("@WEBORDERNO"); _vlist.Add(weborder);
            DataTable _dt = PVCCCAF.AppCode.Global.GetData_New("USP_ACXGETEMAILBODYFORAPPROVAL", CommandType.StoredProcedure, _plist, _vlist);
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/EmailTemplates/ApproveOrderTemplate.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{UserName}", _dt.Rows[0]["CUSTOMERNAME"].ToString());
            body = body.Replace("{OrderNo}", _dt.Rows[0]["WEBORDERNO"].ToString());
            body = body.Replace("{OrderDate}", _dt.Rows[0]["ORDERDATE"].ToString());
            body = body.Replace("{DeliveryDate}", _dt.Rows[0]["DELIVERYDATE"].ToString());
            body = body.Replace("{OrderAmount}", _dt.Rows[0]["TOTALAMOUNT"].ToString());
            string _LineTable = "<tbody>";

            foreach (DataRow dr in _dt.Rows)
            {
                _LineTable += "<tr><td style='width: 56px;'>" + dr["SRNO"].ToString();
                _LineTable += "</td><td style='width: 278px;'>" + dr["PRODUCTNAME"].ToString();
                _LineTable += "</td><td style='width: 77px;'>" + dr["CONFIRMQTY"].ToString();
                _LineTable += "</td><td style='width: 109px;'>" + dr["EXTPRICEVIP"].ToString() + "</td></tr>";
            }
            body = body.Replace("{LineTable}", _LineTable);
            return body;
        }

        public string SendDBMail(EmailParmObject emailParm)
        {

            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();

            _plist.Add("@Mailsubject"); _vlist.Add(emailParm.mailsubject);
            _plist.Add("@MailBody"); _vlist.Add(emailParm.mailbody);
            _plist.Add("@EmailTo"); _vlist.Add(emailParm.mailtoid);
            _plist.Add("@ccmailto"); _vlist.Add(emailParm.mailccid);
           
            DataTable _dt = new DataTable();
            try
            {
                _dt = PVCCCAF.AppCode.Global.GetData_New("USP_SEND_DBEMAILS", CommandType.StoredProcedure, _plist, _vlist);
            }
            catch (Exception ex)
            {
                return "failure";
            }
            return "success";
        }
    }
}