using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PVCCCAF.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Table;

namespace PVCCCAF.Controllers
{
    public class PriceVarianceController : Controller
    {
        // GET: PriceVariance
        PVCCCAF.AppCode.Global obj = new AppCode.Global();
        #region"PVC Form"
        public ActionResult PriceVariance(string PVCNO, string Pvstatus, bool iscopy = false)
        {
            Session["PVLINE"] = null;
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            PVFormViewModel _pvformmodel = new PVFormViewModel();
            ViewBag.AccountSelection = Enumerable.Empty<SelectListItem>();
            ViewBag.ISPOSTPRICE = 0;
            if (!string.IsNullOrEmpty(PVCNO))
            {
                ViewBag.PVCNUMSEQ = PVCNO;
                _pvformmodel = GetSavedPVData(PVCNO, Pvstatus);
                DataTable _dtEntity = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_ENTITYMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                ViewBag.EntityDropdown = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectList(_dtEntity, "ENTITYID", "ENTITYDESC", _pvformmodel.PVHeader.ENTITYID);

                DataTable _dtBranch = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_BRANCHMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                ViewBag.BranchDropdown = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectList(_dtBranch, "VATPERCENT", "BRANCHDESC", Convert.ToString(_pvformmodel.PVHeader.BRANCHID));


                DataTable _dtCustApplicable = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_CUSTAPPLICABLETYPEMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                ViewBag.CustomerApplicableDropdown = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectList(_dtCustApplicable, "CUSTAPPLICABLETYPEID", "CUSTAPPLICABLETYPEDESC", Convert.ToString(_pvformmodel.PVHeader.CUSTAPPLICABLETYPEID));

                DataTable _dtItemType = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_ITEMTYPEMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                ViewBag.ItemType = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectListWithOption(_dtItemType, "ITEMTYPEID", "ITEMTYPEDESC");


                _plist.Add("@DATAAREAID"); _vlist.Add(_pvformmodel.PVHeader.ENTITYID);
                DataTable _dtPriceTier = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_PRICETIERMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                ViewBag.PriceTierDropdown = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectList(_dtPriceTier, "TIERID", "TIERDESC", _pvformmodel.PVHeader.TIERID);

                DataTable _dt = new DataTable();
                string CustType = Convert.ToString(_pvformmodel.PVHeader.CUSTAPPLICABLETYPEID);
                if (CustType == "1")
                {
                    _dt = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_SUBCHANNELMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                    ViewBag.AccountSelection = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectList(_dt, "SUBCHANNELID", "SUBCHANNELDESC", _pvformmodel.PVHeader.TIERID);
                }
                else if (CustType == "2")
                {
                    _dt = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_CHAINMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                    ViewBag.AccountSelection = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectList(_dt, "CHAINID", "CHAINDESC", _pvformmodel.PVHeader.TIERID);
                }
                else if (CustType == "3")
                {
                    _dt = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_CUSTPPRICEGROUPMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                    ViewBag.AccountSelection = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectList(_dt, "CUSTPRICEGROUPID", "CUSTPRICEGROUPDESC", _pvformmodel.PVHeader.TIERID);
                }
                else if (CustType == "4")
                {
                    _dt = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_SINGLECUSTOMERLIST]", CommandType.StoredProcedure, _plist, _vlist);
                    ViewBag.AccountSelection = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectList(_dt, "AccountNum", "CUSTOMERNAME", _pvformmodel.PVHeader.TIERID);
                }
                if (iscopy)
                {
                    List<string> _plist1 = new List<string>();
                    List<string> _vlist1 = new List<string>();
                    _plist1.Add("@DATAAREAID"); _vlist1.Add(Session["DATAAREAID"].ToString());
                    _plist1.Add("@CREATEDBY"); _vlist1.Add(Session["USERCODE"].ToString());
                    _plist1.Add("@PVCNO"); _vlist1.Add(PVCNO);
                    _plist1.Add("@STATUS"); _vlist1.Add("-1");
                    DataTable dtPVLine = new DataTable();
                    dtPVLine = PVCCCAF.AppCode.Global.GetData_New("USP_GET_PVCLINEDETAILS", CommandType.StoredProcedure, _plist1, _vlist1);

                    if (dtPVLine != null && dtPVLine.Rows.Count > 0)
                    {
                        dtPVLine.Columns["StartDate"].ReadOnly = false;
                        dtPVLine.Columns["EndDate"].ReadOnly = false;
                        foreach (DataRow row in dtPVLine.Rows)
                        {
                            
                            row["StartDate"] = DateTime.Now.ToString("dd-MMM-yyyy");
                            row["EndDate"] = DateTime.Now.ToString("dd-MMM-yyyy");
                        }
                        dtPVLine.AcceptChanges();
                        Session["PVLINE"] = dtPVLine;
                    }
                    ViewBag.PVCNUMSEQ = "";
                    _pvformmodel.PVHeader.firstApproverComment = "";
                    _pvformmodel.PVHeader.secondApproverComment = "";
                    _pvformmodel.PVHeader.thirdApprovercomment = "";
                    _pvformmodel.PVHeader.ISCHANGEREQUEST = false;
                    _pvformmodel.PVHeader.ISEDITAFTERAXSYNC = false;
                    _pvformmodel.PVHeader.HeaderStatus = 0;
                    _pvformmodel.PVHeader.PvImages = new List<PVIMAGES>();
                    if (_pvformmodel.PVLine.Count > 0)
                    {
                        _pvformmodel.PVLine[0].StartDate = DateTime.Now.ToString("dd-MMM-yyyy");
                        _pvformmodel.PVLine[0].EndDate = DateTime.Now.ToString("dd-MMM-yyyy");
                    }
                }


                return View(_pvformmodel);
            }
            else
            {
                ViewBag.PVCNUMSEQ = "";
                DataTable _dtEntity = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_ENTITYMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                ViewBag.EntityDropdown = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectListWithOption(_dtEntity, "ENTITYID", "ENTITYDESC");

                DataTable _dtBranch = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_BRANCHMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                ViewBag.BranchDropdown = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectListWithOption(_dtBranch, "VATPERCENT", "BRANCHDESC");

                DataTable _dtCustApplicable = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_CUSTAPPLICABLETYPEMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                ViewBag.CustomerApplicableDropdown = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectListWithOption(_dtCustApplicable, "CUSTAPPLICABLETYPEID", "CUSTAPPLICABLETYPEDESC");

                DataTable _dtItemType = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_ITEMTYPEMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                ViewBag.ItemType = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectListWithOption(_dtItemType, "ITEMTYPEID", "ITEMTYPEDESC");


                _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());
                DataTable _dtPriceTier = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_PRICETIERMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                ViewBag.PriceTierDropdown = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectListWithOption(_dtPriceTier, "TIERID", "TIERDESC");

                return View(_pvformmodel);
            }


        }

        [HttpGet]
        public JsonResult GetItemSelection(string itemtype, string Entityid)
        {
            DataTable _dt = new DataTable();
            string json = string.Empty;

            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();

            _plist.Add("@DATAAREAID"); _vlist.Add(Entityid);
            _plist.Add("@ITEMTYPE"); _vlist.Add(itemtype);
            _dt = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_GETITEMSELECTIONLIST]", CommandType.StoredProcedure, _plist, _vlist);

            string DataResult = obj.ConvertDataTableTojSonString(_dt);
            return Json(DataResult, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult GetAccountSelection(string CustType, string Entityid)
        {
            DataTable _dt = new DataTable();
            string json = string.Empty;

            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();

            _plist.Add("@DATAAREAID"); _vlist.Add(Entityid);
            if (CustType == "1")
            { _dt = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_SUBCHANNELMASTER]", CommandType.StoredProcedure, _plist, _vlist); }
            else if (CustType == "2")
            { _dt = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_CHAINMASTER]", CommandType.StoredProcedure, _plist, _vlist); }
            else if (CustType == "3")
            { _dt = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_CUSTPPRICEGROUPMASTER]", CommandType.StoredProcedure, _plist, _vlist); }
            else if (CustType == "4")
            { _dt = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_SINGLECUSTOMERLIST]", CommandType.StoredProcedure, _plist, _vlist); }

            string DataResult = obj.ConvertDataTableTojSonString(_dt);
            return Json(DataResult, JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public JsonResult AddPVCLineItem(string Itemtype, string Itemselection, decimal CurrentVEPprice, decimal NewVEPprice, decimal VatPerBranch
            , int ExpectedVol, string PVCNUM, string Entityid, string Startdate, string Enddate, string ItemName, string ItemTypeName,string Status="0")
        {
            string[] itemnamearray = ItemName.Split('-');
            ItemName = itemnamearray[1];

            if (string.IsNullOrEmpty(PVCNUM))
            {

                DataTable _dtpvcline = PVCLINE(Itemtype, Itemselection, CurrentVEPprice, NewVEPprice, VatPerBranch
                    , ExpectedVol, Entityid, Startdate, Enddate, ItemName, ItemTypeName);
                if (Session["PVLINE"] == null)
                {
                    Session["PVLINE"] = _dtpvcline;
                    return Json(new { success = true, data = obj.ConvertDataTableTojSonString(_dtpvcline) });
                }
                else
                {
                    bool b = CheckItemExist(Itemselection);
                    DataTable Line = Session["PVLINE"] as DataTable;
                    if (!b)
                    {
                        return Json(new { success = false, data = "Item Id is already Exist" }, JsonRequestBehavior.AllowGet);
                    }
                    {

                        Line.Merge(_dtpvcline, true, MissingSchemaAction.Ignore);
                        Session["PVLINE"] = Line;
                    }
                    return Json(new { success = true, data = obj.ConvertDataTableTojSonString(Line) });
                }


            }
            else
            {


                DataTable _dt = new DataTable();
                string json = string.Empty;

                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();

                decimal DISCPERCASE = Convert.ToDecimal(CurrentVEPprice - NewVEPprice);
                decimal DISCPERCENTAGE = Convert.ToDecimal(DISCPERCASE / CurrentVEPprice) * 100;
                _plist.Add("@DATAAREAID"); _vlist.Add(Entityid);
                _plist.Add("@PVCNO"); _vlist.Add(PVCNUM);
                _plist.Add("@ITEMTYPE"); _vlist.Add(Itemtype);
                _plist.Add("@ITEMTYPECODE"); _vlist.Add(Itemselection);
                _plist.Add("@STARTDATE"); _vlist.Add(Startdate);
                _plist.Add("@ENDDATE"); _vlist.Add(Enddate);
                _plist.Add("@CURRENTVEP"); _vlist.Add(CurrentVEPprice.ToString());
                _plist.Add("@PROPOSEDVEP"); _vlist.Add(NewVEPprice.ToString());
                _plist.Add("@PROPOSEDVIP"); _vlist.Add(((VatPerBranch * NewVEPprice / 100) + NewVEPprice).ToString());
                _plist.Add("@DISCPERCASE"); _vlist.Add(DISCPERCASE.ToString());
                _plist.Add("@DISCPERCENTAGE"); _vlist.Add(DISCPERCENTAGE.ToString("#0.0"));
                _plist.Add("@EXPECTEDVOL"); _vlist.Add(ExpectedVol.ToString());
                _plist.Add("@STATUS"); _vlist.Add(Status);
                _plist.Add("@CREATEDBY"); _vlist.Add(Session["USERCODE"].ToString());
                _plist.Add("@ITEMDESC"); _vlist.Add(ItemName);

                try
                {
                    _dt = PVCCCAF.AppCode.Global.GetData_New("USP_INS_PVCLINEDETAILS", CommandType.StoredProcedure, _plist, _vlist);
                    return Json(new { success = true, data = obj.ConvertDataTableTojSonString(_dt) });
                }

                catch (Exception ex)
                {
                    return Json(new { success = false, data = ex.Message.ToString() });
                }


            }

        }
        protected bool CheckItemExist(string ItemID)
        {
            bool b;
            b = false;
            DataTable Line = new DataTable();
            Line = (DataTable)Session["PVLINE"];
            DataRow[] Rows = Line.Select("ITEMTYPECODE='" + ItemID.ToString() + "'");
            if (Rows.Count() == 0)
            {
                b = true;
            }
            return b;
        }

        public DataTable PVCLINE(string Itemtype, string Itemselection, decimal CurrentVEPprice, decimal NewVEPprice, decimal VatPerBranch
            , int ExpectedVol, string Entityid, string Startdate, string Enddate, string itemname, string ItemTypeName)
        {



            DataTable dtheader = new DataTable();
            dtheader.Columns.Add("ITEMTYPE", typeof(string));
            dtheader.Columns.Add("ITEMTYPEDESC", typeof(string));
            dtheader.Columns.Add("ITEMTYPECODE", typeof(string));
            dtheader.Columns.Add("ITEMNAME", typeof(string));
            dtheader.Columns.Add("CURRENTVEP", typeof(decimal));
            dtheader.Columns.Add("PROPOSEDVEP", typeof(decimal));
            dtheader.Columns.Add("PROPOSEDVIP", typeof(decimal));
            dtheader.Columns.Add("DISCPERCASE", typeof(decimal));
            dtheader.Columns.Add("DISCPERCENTAGE", typeof(decimal));
            dtheader.Columns.Add("EXPECTEDVOL", typeof(int));
            dtheader.Columns.Add("DATAAREAID", typeof(string));
            dtheader.Columns.Add("Startdate", typeof(string));
            dtheader.Columns.Add("Enddate", typeof(string));
            DataRow row;
            row = dtheader.NewRow();
            row["ITEMTYPE"] = Itemtype;
            row["ITEMTYPEDESC"] = ItemTypeName;
            row["ITEMTYPECODE"] = Itemselection;
            row["ITEMNAME"] = itemname;
            row["CURRENTVEP"] = CurrentVEPprice;
            row["PROPOSEDVEP"] = NewVEPprice;
            row["PROPOSEDVIP"] = (VatPerBranch * NewVEPprice / 100) + NewVEPprice;
            row["DISCPERCASE"] = CurrentVEPprice - NewVEPprice;
            row["DISCPERCENTAGE"] = Convert.ToDecimal((((CurrentVEPprice - NewVEPprice) / CurrentVEPprice) * 100).ToString("#0.0"));
            row["EXPECTEDVOL"] = ExpectedVol;
            row["DATAAREAID"] = Entityid;
            row["Startdate"] = Startdate;
            row["Enddate"] = Enddate;
            dtheader.Rows.Add(row);
            return dtheader;
        }



        [HttpPost]
        public JsonResult DeletePVLineItem(string PVCNUM, string Entityid, string SelectedItem, string Actionstatus = "0")
        {
            if (Session["PVLINE"] != null)
            {
                string[] ItemID = SelectedItem.Split(',');
                if (ItemID.Count() > 0)
                {
                    DataTable Line = new DataTable();
                    Line = (DataTable)Session["PVLINE"];
                    for (int i = 0; i < ItemID.Count(); i++)
                    {


                        DataRow[] Rows = Line.Select("ITEMTYPECODE='" + ItemID[i].ToString() + "'");
                        if (Rows.Length > 0)
                        {
                            Rows[0].Delete();
                            Line.AcceptChanges();
                        }


                    }
                    string data = obj.ConvertDataTableTojSonString(Line);
                    return Json(new { success = true, data = data });
                }
            }
            else
            {
                DataTable _dt = new DataTable();
                string json = string.Empty;

                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();


                _plist.Add("@DATAAREAID"); _vlist.Add(Entityid);
                _plist.Add("@PVCNO"); _vlist.Add(PVCNUM);
                _plist.Add("@SELECTITEM"); _vlist.Add(SelectedItem);
                _plist.Add("@CREATEDBY"); _vlist.Add(Session["USERCODE"].ToString());
                _plist.Add("@STATUS"); _vlist.Add("1");
                _plist.Add("@ACTIONSTATUS"); _vlist.Add(Actionstatus);
                try
                {
                    _dt = PVCCCAF.AppCode.Global.GetData_New("USP_REMOVE_PVCLINEDETAILS", CommandType.StoredProcedure, _plist, _vlist);
                    return Json(new { success = true, data = obj.ConvertDataTableTojSonString(_dt) });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, data = ex.Message.ToString() });
                }
            }
            return Json(new { success = false, data = "" });
        }


        [HttpPost]
        public JsonResult RemoveItemslineOnEntityChange(string PVCNUM, string Entityid,string Status)
        {
            if (Session["PVLINE"] != null)
            {

             Session["PVLINE"] = null;
             return Json(new { success = true, data = "" });
               
            }
            else
            {
                DataTable _dt = new DataTable();
                string json = string.Empty;

                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@DATAAREAID"); _vlist.Add(Entityid);
                _plist.Add("@PVCNO"); _vlist.Add(PVCNUM);
                _plist.Add("@STATUS"); _vlist.Add(Status);
                try
                {
                    _dt = PVCCCAF.AppCode.Global.GetData_New("USP_REMOVE_ITEMLINES_ONEntityChange", CommandType.StoredProcedure, _plist, _vlist);
                    return Json(new { success = true, data = obj.ConvertDataTableTojSonString(_dt) });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, data = ex.Message.ToString() });
                }
              
            }
            

        }


        [HttpPost]
        public JsonResult GetAccountlinkedcustomer(string AccountSelectionID, string CustomerApplicable, string Entityid)
        {

            DataTable _dt = new DataTable();
            DataTable _dtacc = new DataTable();
            string json = string.Empty;

            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();

            _plist.Add("@CUSTAPPLICABLE"); _vlist.Add(CustomerApplicable);
            _plist.Add("@ACCOUNTSELECTION"); _vlist.Add(AccountSelectionID);
            _plist.Add("@DATAAREAID"); _vlist.Add(Entityid);

            try
            {
                _dt = PVCCCAF.AppCode.Global.GetData_New("USP_GET_PVCACCOUNTLINKEDCUSTOMER", CommandType.StoredProcedure, _plist, _vlist);
                _plist.Add("@SELECTIONTYPE"); _vlist.Add("0");
                _dtacc = PVCCCAF.AppCode.Global.GetData_New("USP_GET_SELECTIONSYNCDETAILS", CommandType.StoredProcedure, _plist, _vlist);
                return Json(new { success = true, data = obj.ConvertDataTableTojSonString(_dt), data2 = obj.ConvertDataTableTojSonString(_dtacc) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message.ToString() });
            }


        }


        [HttpPost]
        public JsonResult GetGrouplinkedItems(string ITEMGROUPID, string Dataareaid)
        {

            DataTable _dt = new DataTable();
            DataTable _dtacc = new DataTable();
            string json = string.Empty;

            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            List<string> _plistitem = new List<string>();
            List<string> _vlistitem = new List<string>();

            _plist.Add("@ITEMGROUPID"); _vlist.Add(ITEMGROUPID);
            _plist.Add("@DATAAREAID"); _vlist.Add(Dataareaid);
            _plistitem.Add("@ACCOUNTSELECTION"); _vlistitem.Add(ITEMGROUPID);
            _plistitem.Add("@SELECTIONTYPE"); _vlistitem.Add("1");
            _plistitem.Add("@CUSTAPPLICABLE"); _vlistitem.Add("0");
            _plistitem.Add("@DATAAREAID"); _vlistitem.Add(Dataareaid);

            try
            {
                _dt = PVCCCAF.AppCode.Global.GetData_New("USP_GET_PVCGROUPLINKEDITEMS", CommandType.StoredProcedure, _plist, _vlist);
                _dtacc = PVCCCAF.AppCode.Global.GetData_New("USP_GET_SELECTIONSYNCDETAILS", CommandType.StoredProcedure, _plistitem, _vlistitem);
                return Json(new { success = true, data = obj.ConvertDataTableTojSonString(_dt), data2 = obj.ConvertDataTableTojSonString(_dtacc) });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message.ToString() });
            }


        }

        [HttpPost]
        public JsonResult UploadPVfiles()
        {
            string msg = string.Empty;
            if (Request.Files.Count > 0)
            {
                var pic = System.Web.HttpContext.Current.Request.Files["PVFile"];
                String PVCNUMSEQ = System.Web.HttpContext.Current.Request.Form["PVNumseq"].ToString();
                string entityid = System.Web.HttpContext.Current.Request.Form["Entityid"].ToString();
                HttpPostedFileBase file = new HttpPostedFileWrapper(pic);
                //add more conditions like file type, file size etc as per your need.
                if (file != null && file.ContentLength > 0)
                {
                    string fileName = System.IO.Path.GetFileName(file.FileName);
                    string fileurl = System.Configuration.ConfigurationManager.AppSettings["PVCuploadUrl"].ToString();
                    fileName = PVCNUMSEQ + '-' + Path.GetFileName(file.FileName);
                    file.SaveAs(Server.MapPath("~/Upload/" + fileName));
                    string filepath = fileurl + fileName;
                    string conString = string.Empty;
                    string extension = Path.GetExtension(file.FileName);
                    PVCCCAF.AppCode.Global obj = new AppCode.Global();
                    List<string> _plist = new List<string>();
                    List<string> _vlist = new List<string>();
                    _plist.Add("@IMAGEPATH"); _vlist.Add(filepath);
                    _plist.Add("@CREATEDBY"); _vlist.Add(Session["USERCODE"].ToString());
                    _plist.Add("@DATAAREAID"); _vlist.Add(entityid);
                    _plist.Add("@PVCNO"); _vlist.Add(PVCNUMSEQ);
                    DataTable _dt = PVCCCAF.AppCode.Global.GetData_New("USP_INS_PVCIMAGE", CommandType.StoredProcedure, _plist, _vlist);
                    if (_dt.Rows.Count > 0)
                    {
                        string responsetext = _dt.Rows[0]["RESPONSE"].ToString();
                        string[] arrayimage = responsetext.Split('|');
                        if (arrayimage[0].ToString() == "SUCCESS")
                        {
                            return Json(new { success = true, responseText = filepath, exttype = extension, imagename = arrayimage[1].ToString() }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { success = false, responseText = "file not Upload" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { success = false, responseText = "Please Upload Files in .xls, .xlsx or .csv format" }, JsonRequestBehavior.AllowGet);

                }
            }
            else
            {
                return Json(new { success = false, responseText = "Please select a file" }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { success = false, responseText = "Please select a file" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UploadNewPriceGroupExcel()
        {
            string msg = string.Empty;
            if (Request.Files.Count > 0)
            {
                var pic = System.Web.HttpContext.Current.Request.Files["PVFile"];
                string entityid = System.Web.HttpContext.Current.Request.Form["Entityid"].ToString();
                HttpPostedFileBase FileUpload = new HttpPostedFileWrapper(pic);
                //add more conditions like file type, file size etc as per your need.
                if (FileUpload != null && FileUpload.ContentLength > 0)
                {

                    try
                    {
                        var supportedTypes = new[] { "xlsx" };
                        var fileExt = System.IO.Path.GetExtension(FileUpload.FileName).Substring(1);
                        if (!supportedTypes.Contains(fileExt))
                        {
                            return Json(new { success = false, responseText = "File Extension Is InValid - Only Upload EXCEL File" }, JsonRequestBehavior.AllowGet);
                        }
                        string fileName = System.IO.Path.GetFileName(FileUpload.FileName);
                        FileUpload.SaveAs(Server.MapPath("~/Upload/" + fileName));
                        string excelPath = Server.MapPath("~/Upload/") + Path.GetFileName(FileUpload.FileName);
                        string conString = string.Empty;
                        string extension = Path.GetExtension(FileUpload.FileName);
                        DataTable dtExcelData = new DataTable();
                        dtExcelData = PVCCCAF.App_Code.ExcelUpload.ImportExcelXLS(Server.MapPath("~/Upload/" + fileName), true);
                        dtExcelData.TableName = "PriceGroup";
                        PVCCCAF.AppCode.Global obj = new AppCode.Global();
                        DataSet lds = new DataSet();
                        lds.Tables.Add(dtExcelData);
                        string ls_xml = lds.GetXml();
                        List<string> _plist = new List<string>();
                        List<string> _vlist = new List<string>();
                        _plist.Add("@XMLDATA"); _vlist.Add(ls_xml);
                        _plist.Add("@USERCODE"); _vlist.Add(Session["USERCODE"].ToString());
                        _plist.Add("@DATAAREAID"); _vlist.Add(entityid);
                        DataTable temp = PVCCCAF.AppCode.Global.GetData_New("USP_INS_XMLNEWPRICEGROUP", CommandType.StoredProcedure, _plist, _vlist);
                        if (temp != null && temp.Rows.Count > 0)
                        {
                            temp.TableName = "UploadPriceCustlinking";
                            GenerateExcelupload(temp, "UploadPriceCustlinking");

                        }
                        return Json(new { success = true, data = obj.ConvertDataTableTojSonString(temp) });
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, responseText = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    return Json(new { success = false, responseText = "Please Upload Files in .xls, .xlsx or .csv format" }, JsonRequestBehavior.AllowGet);

                }
            }
            else
            {
                return Json(new { success = false, responseText = "Please select a file" }, JsonRequestBehavior.AllowGet);

            }

        }
        private void GenerateExcelupload(DataTable Dt, string filename)
        {
            var memoryStream = new MemoryStream();
            using (var excelPackage = new ExcelPackage(memoryStream))
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add(filename);
                worksheet.Cells["A1"].LoadFromDataTable(Dt, true, TableStyles.None);
                OfficeOpenXml.Table.ExcelTable table = worksheet.Tables[Dt.TableName];
                table.ShowFilter = false;
                worksheet.Column(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(1).Width = 15;

                worksheet.Column(2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(2).Width = 15;

                worksheet.Column(3).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(3).Width = 25;

                worksheet.Column(4).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(4).Width = 15;

                worksheet.Column(5).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(5).Width = 25;

                worksheet.Column(6).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(6).Width = 15;

                worksheet.Column(7).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(7).Width = 25;

                worksheet.Column(8).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(8).Width = 15;

                worksheet.Column(9).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(9).Width = 25;

                worksheet.Column(10).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(10).Width = 15;

                worksheet.Column(11).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(11).Width = 25;

                worksheet.Column(12).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(12).Width = 30;

                worksheet.Column(13).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(13).Width = 25;

                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;


                //worksheet.Column(2).AutoFit();

                Session["DownloadExcel_UploadCustlinking"] = excelPackage.GetAsByteArray();

            }
        }
        public ActionResult DownloadExcelPriceUpload()
        {

            if (Session["DownloadExcel_UploadCustlinking"] != null)
            {
                byte[] data = Session["DownloadExcel_UploadCustlinking"] as byte[];
                return File(data, "application/octet-stream", "UploadPriceCustlinking.xlsx");

            }
            else
            {
                return new EmptyResult();
            }

        }

        [HttpPost]
        public JsonResult UploadNewitemsGroupExcel()
        {
            string msg = string.Empty;
            if (Request.Files.Count > 0)
            {
                var pic = System.Web.HttpContext.Current.Request.Files["PVFile"];
                string entityid = System.Web.HttpContext.Current.Request.Form["Entityid"].ToString();
                HttpPostedFileBase FileUpload = new HttpPostedFileWrapper(pic);
                //add more conditions like file type, file size etc as per your need.
                if (FileUpload != null && FileUpload.ContentLength > 0)
                {

                    try
                    {
                        var supportedTypes = new[] { "xlsx" };
                        var fileExt = System.IO.Path.GetExtension(FileUpload.FileName).Substring(1);
                        if (!supportedTypes.Contains(fileExt))
                        {
                            return Json(new { success = false, responseText = "File Extension Is InValid - Only Upload EXCEL File" }, JsonRequestBehavior.AllowGet);
                        }
                        string fileName = System.IO.Path.GetFileName(FileUpload.FileName);
                        FileUpload.SaveAs(Server.MapPath("~/Upload/" + fileName));
                        string excelPath = Server.MapPath("~/Upload/") + Path.GetFileName(FileUpload.FileName);
                        string conString = string.Empty;
                        string extension = Path.GetExtension(FileUpload.FileName);
                        DataTable dtExcelData = new DataTable();
                        dtExcelData = PVCCCAF.App_Code.ExcelUpload.ImportExcelXLS(Server.MapPath("~/Upload/" + fileName), true);
                        dtExcelData.TableName = "ItemGroup";
                        PVCCCAF.AppCode.Global obj = new AppCode.Global();
                        DataSet lds = new DataSet();
                        lds.Tables.Add(dtExcelData);
                        string ls_xml = lds.GetXml();
                        List<string> _plist = new List<string>();
                        List<string> _vlist = new List<string>();
                        _plist.Add("@XMLDATA"); _vlist.Add(ls_xml);
                        _plist.Add("@USERCODE"); _vlist.Add(Session["USERCODE"].ToString());
                        _plist.Add("@DATAAREAID"); _vlist.Add(entityid);
                        DataTable temp = PVCCCAF.AppCode.Global.GetData_New("USP_INS_XMLNEWITEMGROUPLINKING", CommandType.StoredProcedure, _plist, _vlist);
                        if (temp != null && temp.Rows.Count > 0)
                        {
                            temp.TableName = "UploadItemGrouplinking";
                            GenerateExceluploadItems(temp, "UploadItemGrouplinking");

                        }
                        return Json(new { success = true, data = obj.ConvertDataTableTojSonString(temp) });
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, responseText = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    return Json(new { success = false, responseText = "Please Upload Files in .xls, .xlsx or .csv format" }, JsonRequestBehavior.AllowGet);

                }
            }
            else
            {
                return Json(new { success = false, responseText = "Please select a file" }, JsonRequestBehavior.AllowGet);

            }

        }
        private void GenerateExceluploadItems(DataTable Dt, string filename)
        {
            var memoryStream = new MemoryStream();
            using (var excelPackage = new ExcelPackage(memoryStream))
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add(filename);
                worksheet.Cells["A1"].LoadFromDataTable(Dt, true, TableStyles.None);
                OfficeOpenXml.Table.ExcelTable table = worksheet.Tables[Dt.TableName];
                table.ShowFilter = false;
                worksheet.Column(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(1).Width = 15;
                worksheet.Column(2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(2).Width = 15;

                worksheet.Column(3).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(3).Width = 25;
                worksheet.Column(4).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(4).Width = 15;
                worksheet.Column(5).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(5).Width = 25;
                worksheet.Column(6).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(6).Width = 15;
                worksheet.Column(7).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(7).Width = 25;
                worksheet.Column(8).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(8).Width = 15;
                worksheet.Column(9).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(9).Width = 25;

                worksheet.Column(10).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(10).Width = 15;
                worksheet.Column(11).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(11).Width = 25;
                worksheet.Column(12).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(12).Width = 30;
                worksheet.Column(13).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Column(13).Width = 25;
                //worksheet.Column(2).AutoFit();
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;


                Session["DownloadExcel_Uploaditemslinking"] = excelPackage.GetAsByteArray();

            }
        }
        public ActionResult DownloadExcelitemsUpload()
        {

            if (Session["DownloadExcel_Uploaditemslinking"] != null)
            {
                byte[] data = Session["DownloadExcel_Uploaditemslinking"] as byte[];
                return File(data, "application/octet-stream", "UploadItemGrouplinking.xlsx");

            }
            else
            {
                return new EmptyResult();
            }

        }

        [HttpPost]
        public JsonResult SavePVCDetail(string PriceTier, string AccountSelection, string Desctiption, string CustApplicable
            , string BranchId, string Entityid, string AccountSelectionDesc, int ISPOSTPRICE)
        {
            DataTable _dt = new DataTable();
            string json = string.Empty;

            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();

            _plist.Add("@DATAAREAID"); _vlist.Add(Entityid);
            _plist.Add("@AccountSelection"); _vlist.Add(AccountSelection);
            _plist.Add("@AccountSelectionDesc"); _vlist.Add(AccountSelectionDesc);
            _plist.Add("@PriceTier"); _vlist.Add(PriceTier);
            _plist.Add("@PVCDESC"); _vlist.Add(Desctiption);
            _plist.Add("@CUSTAPPLICABLETYPEID"); _vlist.Add(CustApplicable);
            _plist.Add("@BRANCHID"); _vlist.Add(BranchId);
            _plist.Add("@STATUS"); _vlist.Add("0");
            _plist.Add("@CREATEDBY"); _vlist.Add(Session["USERCODE"].ToString());
            _plist.Add("@ISPOSTPRICE"); _vlist.Add(ISPOSTPRICE.ToString());
            try
            {
                _dt = PVCCCAF.AppCode.Global.GetData_New("USP_INS_PVCHEADERDETAILS", CommandType.StoredProcedure, _plist, _vlist);
                if (_dt.Rows.Count > 0)
                {
                    string pvno = _dt.Rows[0]["RESPONSE"].ToString();
                    if (Session["PVLINE"] != null)
                    {
                        DataTable Line = Session["PVLINE"] as DataTable;
                        foreach (DataRow row in Line.Rows)

                        {
                            DataTable _dttable = new DataTable();
                            string jsondata = string.Empty;

                            _plist = new List<string>();
                            _vlist = new List<string>();

                            _plist.Add("@DATAAREAID"); _vlist.Add(row["DATAAREAID"].ToString());
                            _plist.Add("@PVCNO"); _vlist.Add(pvno);
                            _plist.Add("@ITEMTYPE"); _vlist.Add(row["ITEMTYPE"].ToString());
                            _plist.Add("@ITEMTYPECODE"); _vlist.Add(row["ITEMTYPECODE"].ToString());
                            _plist.Add("@STARTDATE"); _vlist.Add(row["Startdate"].ToString());
                            _plist.Add("@ENDDATE"); _vlist.Add(row["Enddate"].ToString());
                            _plist.Add("@CURRENTVEP"); _vlist.Add(row["CURRENTVEP"].ToString());
                            _plist.Add("@PROPOSEDVEP"); _vlist.Add(row["PROPOSEDVEP"].ToString());
                            _plist.Add("@PROPOSEDVIP"); _vlist.Add(row["PROPOSEDVIP"].ToString());
                            _plist.Add("@DISCPERCASE"); _vlist.Add(row["DISCPERCASE"].ToString());
                            _plist.Add("@DISCPERCENTAGE"); _vlist.Add(row["DISCPERCENTAGE"].ToString());
                            _plist.Add("@EXPECTEDVOL"); _vlist.Add(row["EXPECTEDVOL"].ToString());
                            _plist.Add("@STATUS"); _vlist.Add("0");
                            _plist.Add("@CREATEDBY"); _vlist.Add(Session["USERCODE"].ToString());
                            _plist.Add("@ITEMDESC"); _vlist.Add(row["ITEMNAME"].ToString());
                            _dttable = PVCCCAF.AppCode.Global.GetData_New("USP_INS_PVCLINEDETAILS", CommandType.StoredProcedure, _plist, _vlist);
                        }
                        Session["PVLINE"] = null;

                    }



                    return Json(new { success = true, responseText = _dt.Rows[0]["RESPONSE"].ToString() }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, responseText = "Price Variance details not saved ! some error occur" }, JsonRequestBehavior.AllowGet);
                }




            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message.ToString() });
            }


        }

        [HttpPost]
        public JsonResult PVCUserAction(string PVCNUM, string Entityid, string ActionType)
        {
            DataTable _dt = new DataTable();
            string json = string.Empty;
            PVCCCAF.AppCode.Global global = new AppCode.Global();

            try
            {
                string query = "USP_INS_PVCUSERACTION '" + Entityid + "','" + PVCNUM + "','" + ActionType + "','" + Session["USERCODE"].ToString() + "'";
                int a = global.ExecuteCommand(query);
                if (ActionType == "0")
                {
                    if (a > 0)
                    {
                        bool value = PVCCCAF.AppCode.Global.ExecQuery("exec USP_ACXSENDPVAPPROVE_EMAIL '" + PVCNUM + "','" + Entityid + "'");
                        return Json(new { success = true, responseText = PVCNUM + " has been Submitted for Approval" }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        return Json(new { success = false, responseText = "Price Variance details not saved ! some error occur" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (ActionType == "1")
                {
                    if (a > 0)
                    {
                        bool value = PVCCCAF.AppCode.Global.ExecQuery("exec USP_ACXSENDPVAPPROVE_EMAIL '" + PVCNUM + "','" + Entityid + "'");
                        return Json(new { success = true, responseText = PVCNUM + " has been Redirected to Approver 2" }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        return Json(new { success = false, responseText = "for this Price Variance Skip Approval Not happend" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (ActionType == "2")
                {
                    if (a > 0)
                    {
                        return Json(new { success = true, responseText = PVCNUM+" has been Cancelled" }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        return Json(new { success = false, responseText = "for this Price Variance Cancellation Not happend" }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { success = false, responseText = "Price Variance details not saved ! some error occur" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message.ToString() });
            }


        }

        [HttpPost]
        public JsonResult PVCSaveRequestChangeData(string Itemselection, decimal CurrentVEPprice, decimal NewVEPprice, decimal VatPerBranch
            , int ExpectedVol, string PVCNUM, string Entityid, string Startdate, string Enddate,string EDITONRELEASE)
        {

            decimal DISCPERCASE = Convert.ToDecimal(CurrentVEPprice - NewVEPprice);
            decimal DISCPERCENTAGE = Convert.ToDecimal(DISCPERCASE / CurrentVEPprice) * 100;
            if (string.IsNullOrEmpty(PVCNUM))
            {

                if (Session["PVLINE"] != null)
                {

                    DataTable Line = new DataTable();
                    Line = (DataTable)Session["PVLINE"];
                    DataRow[] Rows = Line.Select("ITEMTYPECODE='" + Itemselection.ToString() + "'");

                    if (Rows.Length > 0)
                    {
                        Rows[0]["CURRENTVEP"] = CurrentVEPprice;
                        Rows[0]["PROPOSEDVEP"] = NewVEPprice;
                        Rows[0]["PROPOSEDVIP"] = ((VatPerBranch * NewVEPprice / 100) + NewVEPprice);
                        Rows[0]["DISCPERCASE"] = DISCPERCASE;
                        Rows[0]["DISCPERCENTAGE"] = DISCPERCENTAGE;
                        Rows[0]["EXPECTEDVOL"] = ExpectedVol;
                        Line.AcceptChanges();
                    }



                    string data = obj.ConvertDataTableTojSonString(Line);
                    return Json(new { success = true, data = data });

                }

            }
            else
            {
                DataTable _dt = new DataTable();
                string json = string.Empty;

                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();


                _plist.Add("@DATAAREAID"); _vlist.Add(Entityid);
                _plist.Add("@PVCNO"); _vlist.Add(PVCNUM);
                _plist.Add("@ITEMTYPECODE"); _vlist.Add(Itemselection);
                _plist.Add("@STARTDATE"); _vlist.Add(Startdate);
                _plist.Add("@ENDDATE"); _vlist.Add(Enddate);
                _plist.Add("@CURRENTVEP"); _vlist.Add(CurrentVEPprice.ToString());
                _plist.Add("@PROPOSEDVEP"); _vlist.Add(NewVEPprice.ToString());
                _plist.Add("@PROPOSEDVIP"); _vlist.Add(((VatPerBranch * NewVEPprice / 100) + NewVEPprice).ToString());
                _plist.Add("@DISCPERCASE"); _vlist.Add(DISCPERCASE.ToString());
                _plist.Add("@DISCPERCENTAGE"); _vlist.Add(DISCPERCENTAGE.ToString("#0.0"));
                _plist.Add("@EXPECTEDVOL"); _vlist.Add(ExpectedVol.ToString());
                _plist.Add("@EDITONRELEASE"); _vlist.Add(EDITONRELEASE.ToString());

                try
                {
                    _dt = PVCCCAF.AppCode.Global.GetData_New("USP_USP_PVCLINEDETAILS", CommandType.StoredProcedure, _plist, _vlist);
                    return Json(new { success = true, data = obj.ConvertDataTableTojSonString(_dt) });
                }

                catch (Exception ex)
                {
                    return Json(new { success = false, data = ex.Message.ToString() });
                }


            }
            return Json(new { success = false, data = "" });

        }


        [HttpPost]
        public JsonResult PVCUpdateHeaderData(string PriceTier, string AccountSelection, string Desctiption, string CustApplicable
           , string BranchId, string Entityid, string AccountSelectionDesc, string PVCNO, string StartDate, string EndDate, string updatetype = "0")
        {
            DataTable _dt = new DataTable();
            string json = string.Empty;

            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();

            _plist.Add("@DATAAREAID"); _vlist.Add(Entityid);
            _plist.Add("@AccountSelection"); _vlist.Add(AccountSelection);
            _plist.Add("@AccountSelectionDesc"); _vlist.Add(AccountSelectionDesc);
            _plist.Add("@PriceTier"); _vlist.Add(PriceTier);
            _plist.Add("@PVCDESC"); _vlist.Add(Desctiption);
            _plist.Add("@CUSTAPPLICABLETYPEID"); _vlist.Add(CustApplicable);
            _plist.Add("@BRANCHID"); _vlist.Add(BranchId);
            _plist.Add("@MODIFIEDBY"); _vlist.Add(Session["USERCODE"].ToString());
            _plist.Add("@PVCNO"); _vlist.Add(PVCNO);
            _plist.Add("@StartDate"); _vlist.Add(StartDate);
            _plist.Add("@EndDate"); _vlist.Add(EndDate);
            _plist.Add("@updatetype"); _vlist.Add(updatetype);
            try
            {
                _dt = PVCCCAF.AppCode.Global.GetData_New("USP_UPDATE_PVCHEADERDETAILS", CommandType.StoredProcedure, _plist, _vlist);

                return Json(new { success = true, responseText = "" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message.ToString() });
            }


        }


        public JsonResult GetNewGroupCode(string TYPE)
        {
            DataTable _dt = new DataTable();
            string newgroupcode = string.Empty;
            string json = string.Empty; List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@TYPE"); _vlist.Add(TYPE);
            _dt = PVCCCAF.AppCode.Global.GetData_New("USP_GET_NEWGROUPCODE", CommandType.StoredProcedure, _plist, _vlist);
            if (_dt.Rows.Count > 0)
            {
                newgroupcode = _dt.Rows[0]["RESPONSE"].ToString();
            }

            return Json(new { success = true, responseText = newgroupcode }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult SaveNewPriceDiscount(string AccountSelectionID, string AccountName, string Entityid)
        {
            try
            {

                PVCCCAF.AppCode.Global global = new AppCode.Global();
                string query = "INSERT INTO  Ax.CUSTPPRICEGROUPMASTER(DATAAREAID,CUSTPRICEGROUPID,CUSTPRICEGROUPDESC,ISPORTALCREATED,CREATEDBY)" + Environment.NewLine;
                query += "VALUES('" + Entityid + "','" + AccountSelectionID + "','" + AccountName + "',1,'" + Session["USERCODE"].ToString() + "')";
                int a = global.ExecuteCommand(query);

                if (a > 0)
                {
                    return Json(new { success = true, responseText = "" }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { success = false, responseText = "" }, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message.ToString() });
            }


        }

        [HttpPost]
        public JsonResult SaveNewItemGroup(string Itemid, string Itemname, string Entityid)
        {
            try
            {

                PVCCCAF.AppCode.Global global = new AppCode.Global();
                string query = "INSERT INTO  Ax.ITEMPRICEGROUPMASTER(DATAAREAID,ITEMPRICEGROUPID,ITEMPRICEGROUPDESC,ISPORTALCREATED,CREATEDBY)" + Environment.NewLine;
                query += "VALUES('" + Entityid + "','" + Itemid + "','" + Itemname + "',1,'" + Session["USERCODE"].ToString() + "')";
                int a = global.ExecuteCommand(query);

                if (a > 0)
                {
                    return Json(new { success = true, responseText = "" }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { success = false, responseText = "" }, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message.ToString() });
            }


        }

        [HttpPost]
        public JsonResult AddLinkedCustomer(string custAplicantArray, string PriceGroupId, string Actiontype, string Entityid)
        {
            DataTable _dt = new DataTable();
            string json = string.Empty;

            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@DATAAREAID"); _vlist.Add(Entityid);
            _plist.Add("@CUSTAPPLICABLEArray"); _vlist.Add(custAplicantArray);
            _plist.Add("@ACCOUNTSELECTION"); _vlist.Add(PriceGroupId);
            _plist.Add("@TYPE"); _vlist.Add(Actiontype);
            _plist.Add("@USERCODE"); _vlist.Add(Session["USERCODE"].ToString());

            try
            {
                _dt = PVCCCAF.AppCode.Global.GetData_New("USP_ADD_REMOVE_CUSTOMERLINKED", CommandType.StoredProcedure, _plist, _vlist);
                return Json(new { success = true, data = obj.ConvertDataTableTojSonString(_dt) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message.ToString() });
            }


        }

        [HttpPost]
        public JsonResult RemoveLinkedCustomer(string custAplicantArray, string PriceGroupId, string Actiontype, string Entityid)
        {
            DataTable _dt = new DataTable();
            string json = string.Empty;

            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@DATAAREAID"); _vlist.Add(Entityid);
            _plist.Add("@CUSTAPPLICABLEArray"); _vlist.Add(custAplicantArray);
            _plist.Add("@ACCOUNTSELECTION"); _vlist.Add(PriceGroupId);
            _plist.Add("@TYPE"); _vlist.Add(Actiontype);

            try
            {
                _dt = PVCCCAF.AppCode.Global.GetData_New("USP_ADD_REMOVE_CUSTOMERLINKED", CommandType.StoredProcedure, _plist, _vlist);
                return Json(new { success = true, data = obj.ConvertDataTableTojSonString(_dt) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message.ToString() });
            }


        }



        [HttpPost]
        public JsonResult AddLinkedGroupItem(string ITEMGROUPID, string ITEMID, string Actiontype, string Entityid)
        {
            DataTable _dt = new DataTable();
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@DATAAREAID"); _vlist.Add(Entityid);
            _plist.Add("@ITEMGROUPID"); _vlist.Add(ITEMGROUPID);
            _plist.Add("@ITEMID"); _vlist.Add(ITEMID);
            _plist.Add("@TYPE"); _vlist.Add(Actiontype);
            _plist.Add("@USERCODE"); _vlist.Add(Session["USERCODE"].ToString());
            try
            {
                _dt = PVCCCAF.AppCode.Global.GetData_New("USP_ADD_REMOVE_ITEMLINKED", CommandType.StoredProcedure, _plist, _vlist);
                return Json(new { success = true, data = obj.ConvertDataTableTojSonString(_dt) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message.ToString() });
            }


        }

        [HttpPost]
        public JsonResult RemoveLinkedGroupItem(string ITEMGROUPID, string ITEMID, string Actiontype, string Entityid)
        {
            DataTable _dt = new DataTable();
            string json = string.Empty;

            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@DATAAREAID"); _vlist.Add(Entityid);
            _plist.Add("@ITEMGROUPID"); _vlist.Add(ITEMGROUPID);
            _plist.Add("@ITEMID"); _vlist.Add(ITEMID);
            _plist.Add("@TYPE"); _vlist.Add(Actiontype);

            try
            {
                _dt = PVCCCAF.AppCode.Global.GetData_New("USP_ADD_REMOVE_ITEMLINKED", CommandType.StoredProcedure, _plist, _vlist);
                return Json(new { success = true, data = obj.ConvertDataTableTojSonString(_dt) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message.ToString() });
            }


        }

        [HttpPost]
        public JsonResult RemoveGroupItem(string ITEMGROUPID, string PVCNO, string Entityid)
        {
            if (Session["PVLINE"] != null)
            {
                string[] ItemID = ITEMGROUPID.Split(',');
                if (ItemID.Count() > 0)
                {
                    DataTable Line = new DataTable();
                    Line = (DataTable)Session["PVLINE"];
                    for (int i = 0; i < ItemID.Count(); i++)
                    {


                        DataRow[] Rows = Line.Select("ITEMTYPECODE='" + ItemID[i].ToString() + "'");
                        if (Rows.Length > 0)
                        {
                            Rows[0].Delete();
                            Line.AcceptChanges();
                        }


                    }
                    string data = obj.ConvertDataTableTojSonString(Line);
                    return Json(new { success = true, data = data });
                }
            }
            else
            {

                DataTable _dt = new DataTable();
                string json = string.Empty;

                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@DATAAREAID"); _vlist.Add(Entityid);
                _plist.Add("@ITEMGROUPID"); _vlist.Add(ITEMGROUPID);
                _plist.Add("@PVCNO"); _vlist.Add(PVCNO);

                try
                {
                    _dt = PVCCCAF.AppCode.Global.GetData_New("USP_REMOVE_GROUPLINES", CommandType.StoredProcedure, _plist, _vlist);
                    return Json(new { success = true, data = obj.ConvertDataTableTojSonString(_dt) });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, responseText = ex.Message.ToString() });
                }
            }
            return Json(new { success = false, responseText = "" });
        }


        #endregion

        #region"Price Variance Approval"

        public ActionResult PriceVarianceApproval(string PVCNO, string Pvstatus)
        {
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            PVFormViewModel _pvformmodel = new PVFormViewModel();
            if (!string.IsNullOrEmpty(PVCNO))
            {
                ViewBag.PVCNUMSEQ = PVCNO;
                _pvformmodel = GetSavedPVData(PVCNO, Pvstatus);
                DataTable _dtEntity = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_ENTITYMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                ViewBag.EntityDropdown = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectList(_dtEntity, "ENTITYID", "ENTITYDESC", _pvformmodel.PVHeader.ENTITYID);

                DataTable _dtBranch = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_BRANCHMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                ViewBag.BranchDropdown = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectList(_dtBranch, "VATPERCENT", "BRANCHDESC", Convert.ToString(_pvformmodel.PVHeader.BRANCHID));


                DataTable _dtCustApplicable = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_CUSTAPPLICABLETYPEMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                ViewBag.CustomerApplicableDropdown = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectList(_dtCustApplicable, "CUSTAPPLICABLETYPEID", "CUSTAPPLICABLETYPEDESC", Convert.ToString(_pvformmodel.PVHeader.CUSTAPPLICABLETYPEID));

                DataTable _dtItemType = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_ITEMTYPEMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                ViewBag.ItemType = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectListWithOption(_dtItemType, "ITEMTYPEID", "ITEMTYPEDESC");


                _plist.Add("@DATAAREAID"); _vlist.Add(_pvformmodel.PVHeader.ENTITYID);
                DataTable _dtPriceTier = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_PRICETIERMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                ViewBag.PriceTierDropdown = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectList(_dtPriceTier, "TIERID", "TIERDESC", _pvformmodel.PVHeader.TIERID);

                DataTable _dt = new DataTable();
                string CustType = Convert.ToString(_pvformmodel.PVHeader.CUSTAPPLICABLETYPEID);
                if (CustType == "1")
                {
                    _dt = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_SUBCHANNELMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                    ViewBag.AccountSelection = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectList(_dt, "SUBCHANNELID", "SUBCHANNELDESC", _pvformmodel.PVHeader.TIERID);
                }
                else if (CustType == "2")
                {
                    _dt = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_CHAINMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                    ViewBag.AccountSelection = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectList(_dt, "CHAINID", "CHAINDESC", _pvformmodel.PVHeader.TIERID);
                }
                else if (CustType == "3")
                {
                    _dt = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_CUSTPPRICEGROUPMASTER]", CommandType.StoredProcedure, _plist, _vlist);
                    ViewBag.AccountSelection = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectList(_dt, "CUSTPRICEGROUPID", "CUSTPRICEGROUPDESC", _pvformmodel.PVHeader.TIERID);
                }
                else if (CustType == "4")
                {
                    _dt = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_SINGLECUSTOMERLIST]", CommandType.StoredProcedure, _plist, _vlist);
                    ViewBag.AccountSelection = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectList(_dt, "AccountNum", "CUSTOMERNAME", _pvformmodel.PVHeader.TIERID);
                }
              


                return View(_pvformmodel);
            }
            return View(_pvformmodel);

        }

        [HttpPost]
        public JsonResult PVCApproverLineAction(string PVCNUM, string Entityid, string ActionType, string ItemCode)
        {
            DataTable _dt = new DataTable();
            string json = string.Empty;
            PVCCCAF.AppCode.Global global = new AppCode.Global();

            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();

                _plist.Add("@DATAAREAID"); _vlist.Add(Entityid);
                _plist.Add("@PVCNO"); _vlist.Add(PVCNUM);
                _plist.Add("@ITEMCODE"); _vlist.Add(ItemCode);
                _plist.Add("@CREATEDBY"); _vlist.Add(Session["USERCODE"].ToString());
                _plist.Add("@ACTIONTYPE"); _vlist.Add(ActionType);
                if (ActionType == "2" || ActionType == "3")
                {
                    _plist.Add("@APPROVERTYPE"); _vlist.Add("1");
                }
                else if (ActionType == "4" || ActionType == "5")
                {
                    _plist.Add("@APPROVERTYPE"); _vlist.Add("2");
                }
                else if (ActionType == "6" || ActionType == "7")
                {
                    _plist.Add("@APPROVERTYPE"); _vlist.Add("3");
                }
                _dt = PVCCCAF.AppCode.Global.GetData_New("USP_UPDATE_PVCAPPROVERLINEACTION", CommandType.StoredProcedure, _plist, _vlist);
                return Json(new { success = true, data = obj.ConvertDataTableTojSonString(_dt) });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message.ToString() });
            }


        }

        [HttpPost]
        public JsonResult PVCApproverHeaderAction(string PVCNUM, string Entityid, string Approvercmmt,
            string ActionType, string Status)
        {
            DataTable _dt = new DataTable();
            string json = string.Empty;
            PVCCCAF.AppCode.Global global = new AppCode.Global();

            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();

                _plist.Add("@DATAAREAID"); _vlist.Add(Entityid);
                _plist.Add("@PVCNO"); _vlist.Add(PVCNUM);
                _plist.Add("@CREATEDBY"); _vlist.Add(Session["USERCODE"].ToString());
                _plist.Add("@REQUESTTYPE"); _vlist.Add(ActionType);
                _plist.Add("@STAUS"); _vlist.Add(Status);
                _plist.Add("@APPROVERCMMT"); _vlist.Add(Approvercmmt);

                _dt = PVCCCAF.AppCode.Global.GetData_New("USP_UPDATE_PVCAPPROVERHEADERACTION", CommandType.StoredProcedure, _plist, _vlist);
                return Json(new { success = true, data = obj.ConvertDataTableTojSonString(_dt) });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message.ToString() });
            }


        }



        [HttpPost]
        public JsonResult PVCSendReleaseData(string PVCNUM, int Actiontype)
        {
            try
            {
                if (Actiontype == 0)
                {
                    PVCCCAF.AppCode.Global global = new AppCode.Global();
                    string query = "update ax.PVCMASTER set STATUS=11,RELEASEDBY='" + Session["USERCODE"].ToString() + "' ,RELEASEDDATETIME=GETDATE() where PVCNO='" + PVCNUM + "'";
                    int a = global.ExecuteCommand(query);

                    string query2 = "update ax.PVCRELEASEMASTER set STATUS=11,RELEASEDBY='" + Session["USERCODE"].ToString() + "' ,RELEASEDDATETIME=GETDATE() where PVCNO='" + PVCNUM + "'";
                    int b = global.ExecuteCommand(query2);
                    if (a > 0 && b > 0)
                    {
                        return Json(new { success = true, responseText = PVCNUM + " has been saved in History" }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        return Json(new { success = false, responseText = PVCNUM + " cannot be saved in History" }, JsonRequestBehavior.AllowGet);
                    }

                }
                else if (Actiontype == 1)
                {
                    PVCCCAF.AppCode.Global global = new AppCode.Global();
                    string result = PVCCCAF.AppCode.Global.SendReleasePVFormData(PVCNUM);
                    if (result == "success")
                    {
                        string query = "update ax.PVCMASTER set STATUS=8,ISSYNCTOAX=1,RELEASEDBY='" + Session["USERCODE"].ToString() + "' ,RELEASEDDATETIME=GETDATE() where PVCNO='" + PVCNUM + "'";
                        int a = global.ExecuteCommand(query);

                        string query2 = "update ax.PVCRELEASEMASTER set STATUS=8,ISSYNCTOAX=1,RELEASEDBY='" + Session["USERCODE"].ToString() + "' ,RELEASEDDATETIME=GETDATE() where PVCNO='" + PVCNUM + "'";
                        int b = global.ExecuteCommand(query2);


                        if (a > 0 && b > 0)
                        {
                            bool value = PVCCCAF.AppCode.Global.ExecQuery("exec USP_ACXSENDPVRELEASE_EMAIL '" + PVCNUM + "','" + Session["DATAAREAID"].ToString() + "'");
                            //return Json(new { success = true, responseText = "Price Variance synced successfully into AX." }, JsonRequestBehavior.AllowGet);
                            return Json(new { success = true, responseText = PVCNUM + " has been Released." }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { success = false, responseText = PVCNUM + " Number run into an error" }, JsonRequestBehavior.AllowGet);
                            //return Json(new { success = false, responseText = "Price Variance details  not Sync To AX" }, JsonRequestBehavior.AllowGet);
                        }


                    }
                    else
                    {
                        return Json(new { success = false, responseText = result });
                    }
                }
                else if (Actiontype == 2)
                {
                    PVCCCAF.AppCode.Global global = new AppCode.Global();
                    string result = PVCCCAF.AppCode.Global.updateReleasePVFormData(PVCNUM);
                    if (result == "success")
                    {
                        string query = "update ax.PVCMASTER set STATUS=8,ISSYNCTOAX=1,ISEDIT_AFTER_AXSYNC=0," + Environment.NewLine;
                        query += " EDIT_AFTER_AXSYNCBY='',EDIT_AFTER_AXSYNCDATETIME='',RELEASEDBY='" + Session["USERCODE"].ToString() + "' ,RELEASEDDATETIME=GETDATE() where PVCNO='" + PVCNUM + "'";
                        int a = global.ExecuteCommand(query);

                        string query2 = "update ax.PVCRELEASEMASTER set STATUS=8,ISSYNCTOAX=1,ISEDIT_AFTER_AXSYNC=0," + Environment.NewLine;
                        query2 += " EDIT_AFTER_AXSYNCBY='',EDIT_AFTER_AXSYNCDATETIME='',RELEASEDBY='" + Session["USERCODE"].ToString() + "' ,RELEASEDDATETIME=GETDATE() where PVCNO='" + PVCNUM + "'";
                        int b = global.ExecuteCommand(query2);


                        if (a > 0 && b>0)
                        {
                            return Json(new { success = true, responseText = "Price Variance data  updated successfully in AX." }, JsonRequestBehavior.AllowGet);

                        }
                        else
                        {
                            return Json(new { success = false, responseText = "Price Variance details  not Sync To AX" }, JsonRequestBehavior.AllowGet);
                        }


                    }
                    else
                    {
                        return Json(new { success = false, responseText = result });
                    }
                }
                return Json(new { success = false, responseText = "" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message.ToString() });
            }


        }

        public PVFormViewModel GetSavedPVData(string PVCNO, string Pvstatus)
        {
            List<PVFHeader> _pvheader = new List<PVFHeader>();
            List<PVFLine> _pvline = new List<PVFLine>();
            List<PVIMAGES> pVIMAGEs = new List<PVIMAGES>();
            PVFormViewModel pVFormViewModel = new PVFormViewModel();
            PVCCCAF.AppCode.Global obj = new AppCode.Global();
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@DATAAREAID"); _vlist.Add("");
            _plist.Add("@CREATEDBY"); _vlist.Add(Session["USERCODE"].ToString());
            _plist.Add("@PVCNO"); _vlist.Add(PVCNO);


            DataTable dtPVHeader = new DataTable();
            dtPVHeader = PVCCCAF.AppCode.Global.GetData_New("USP_GET_PVCHEADERDETAILS", CommandType.StoredProcedure, _plist, _vlist);

            _pvheader = PVCCCAF.BussinessLayer.BussinessLayer.ConvertToList<PVFHeader>(dtPVHeader);

            DataTable dtPvImage = new DataTable();
            dtPvImage = PVCCCAF.AppCode.Global.GetData_New("USP_GET_PVCIMAGEDETAILS", CommandType.StoredProcedure, _plist, _vlist);
            pVIMAGEs = PVCCCAF.BussinessLayer.BussinessLayer.ConvertToList<PVIMAGES>(dtPvImage);


            _plist.Add("@STATUS"); _vlist.Add(Pvstatus);
            DataTable dtPVLine = new DataTable();
            dtPVLine = PVCCCAF.AppCode.Global.GetData_New("USP_GET_PVCLINEDETAILS", CommandType.StoredProcedure, _plist, _vlist);
            _pvline = PVCCCAF.BussinessLayer.BussinessLayer.ConvertToList<PVFLine>(dtPVLine);

            _plist = new List<string>();
            _vlist = new List<string>();
            _plist.Add("@SYNCTYPE"); _vlist.Add("0");
            _plist.Add("@PVCNO"); _vlist.Add(PVCNO);
            DataTable dtSyncAcc = new DataTable();
            dtSyncAcc = PVCCCAF.AppCode.Global.GetData_New("USP_GET_PVCCHECKSYNCDETAILS", CommandType.StoredProcedure, _plist, _vlist);
            if (dtSyncAcc != null && dtSyncAcc.Rows.Count > 0)
            {

                int count = Convert.ToInt16(dtSyncAcc.Rows[0]["TotalCount"]);
                pVFormViewModel._syncbutton.ShowAccountSelectionbtn = count >= 1 ? true : false;
            }

            _plist = new List<string>();
             _vlist = new List<string>();
            _plist.Add("@SYNCTYPE"); _vlist.Add("1");
            _plist.Add("@PVCNO"); _vlist.Add(PVCNO);
            DataTable dtSyncItem = new DataTable();
            dtSyncItem = PVCCCAF.AppCode.Global.GetData_New("USP_GET_PVCCHECKSYNCDETAILS", CommandType.StoredProcedure, _plist, _vlist);
            if (dtSyncItem != null && dtSyncItem.Rows.Count > 0)
            {

               int  count = Convert.ToInt16(dtSyncItem.Rows[0]["TotalCount"]);
                pVFormViewModel._syncbutton.ShowItemSelectionbtn = count >= 1 ? true : false;
            }



            if (_pvheader != null && _pvheader.Count > 0)
            {
                pVFormViewModel.PVHeader = _pvheader[0];
            }
            if (_pvline != null && _pvline.Count > 0)
            {
                pVFormViewModel.PVLine = _pvline;
            }
            if (pVIMAGEs != null && pVIMAGEs.Count > 0)
            {
                pVFormViewModel.PVHeader.PvImages = pVIMAGEs;
            }
            return pVFormViewModel;
        }


        public PVFormViewModel GetSavedPVDataView(string PVCNO, string Pvstatus,string DataView)
        {
            List<PVFHeader> _pvheader = new List<PVFHeader>();
            List<PVFLine> _pvline = new List<PVFLine>();
            List<PVIMAGES> pVIMAGEs = new List<PVIMAGES>();
            PVFormViewModel pVFormViewModel = new PVFormViewModel();
            PVCCCAF.AppCode.Global obj = new AppCode.Global();
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@DATAAREAID"); _vlist.Add("");
            _plist.Add("@CREATEDBY"); _vlist.Add(Session["USERCODE"].ToString());
            _plist.Add("@PVCNO"); _vlist.Add(PVCNO);
            _plist.Add("@DataView"); _vlist.Add(DataView);
            DataTable dtPVHeader = new DataTable();
            dtPVHeader = PVCCCAF.AppCode.Global.GetData_New("USP_GET_PVCHEADERDETAILSRELEASE", CommandType.StoredProcedure, _plist, _vlist);

            _pvheader = PVCCCAF.BussinessLayer.BussinessLayer.ConvertToList<PVFHeader>(dtPVHeader);

            DataTable dtPvImage = new DataTable();
            dtPvImage = PVCCCAF.AppCode.Global.GetData_New("USP_GET_PVCIMAGEDETAILS", CommandType.StoredProcedure, _plist, _vlist);
            pVIMAGEs = PVCCCAF.BussinessLayer.BussinessLayer.ConvertToList<PVIMAGES>(dtPvImage);


            _plist.Add("@STATUS"); _vlist.Add(Pvstatus);
            DataTable dtPVLine = new DataTable();
            dtPVLine = PVCCCAF.AppCode.Global.GetData_New("USP_GET_PVCLINEDETAILSRELEASED", CommandType.StoredProcedure, _plist, _vlist);
            _pvline = PVCCCAF.BussinessLayer.BussinessLayer.ConvertToList<PVFLine>(dtPVLine);
            if (_pvheader != null && _pvheader.Count > 0)
            {
                pVFormViewModel.PVHeader = _pvheader[0];
            }
            if (_pvline != null && _pvline.Count > 0)
            {
                pVFormViewModel.PVLine = _pvline;
            }
            if (pVIMAGEs != null && pVIMAGEs.Count > 0)
            {
                pVFormViewModel.PVHeader.PvImages = pVIMAGEs;
            }
            return pVFormViewModel;
        }

        [HttpPost]
        public JsonResult SyncAccountSelection(string PVCNUM, string Actiontype)
        {
            try
            {
                if (Actiontype == "1")//Account Selection
                {
                    PVCCCAF.AppCode.Global global = new AppCode.Global();
                    string result = PVCCCAF.AppCode.Global.SendAccountSelectionToAx(PVCNUM, Actiontype);
                    if (result == "success")
                    {
                        return Json(new { success = true, responseText = "Account Selection has been synced" }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        return Json(new { success = false, responseText = result });
                    }

                }
                else if (Actiontype == "2")//Item Selection
                {
                    PVCCCAF.AppCode.Global global = new AppCode.Global();
                    string result = PVCCCAF.AppCode.Global.SendItemSelectionToAx(PVCNUM, Actiontype);
                    if (result == "success")
                    {
                        return Json(new { success = true, responseText = "Item Selection has been Synced" }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        return Json(new { success = false, responseText = result });
                    }

                }
                return Json(new { success = false, responseText = "" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message.ToString() });
            }


        }


        [HttpPost]
        public JsonResult DiscCalculateOnbranchchange(string PVCNUM, decimal Vatper, string Status)
        {
            DataTable _dt = new DataTable();
            string json = string.Empty;
            PVCCCAF.AppCode.Global global = new AppCode.Global();

            try
            {

                if (string.IsNullOrEmpty(PVCNUM))
                {
                    if (Session["PVLINE"] != null)
                    {
                        DataTable _dtpvcline = Session["PVLINE"] as DataTable;
                        DataTable _dtnew = DiscCalculateOnchange(_dtpvcline, Vatper);
                        return Json(new { success = true, data = obj.ConvertDataTableTojSonString(_dtnew) });
                    }
                }
                else
                {
                    List<string> _plist = new List<string>();
                    List<string> _vlist = new List<string>();

                    _plist.Add("@PVCNUM"); _vlist.Add(PVCNUM);
                    _plist.Add("@VATPER"); _vlist.Add(Vatper.ToString());
                    _plist.Add("@Status"); _vlist.Add(Status);
                    _dt = PVCCCAF.AppCode.Global.GetData_New("USP_UPDATE_DISCCHANGEONBRANCHCHANGE", CommandType.StoredProcedure, _plist, _vlist);
                    return Json(new { success = true, data = obj.ConvertDataTableTojSonString(_dt) });

                }
                return Json(new { success = false, data = "" });
            }

            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message.ToString() });
            }


        }

        public DataTable DiscCalculateOnchange(DataTable _dt, decimal VatPerBranch)
        {
            DataTable dtheader = new DataTable();
            dtheader.Columns.Add("ITEMTYPE", typeof(string));
            dtheader.Columns.Add("ITEMTYPEDESC", typeof(string));
            dtheader.Columns.Add("ITEMTYPECODE", typeof(string));
            dtheader.Columns.Add("ITEMNAME", typeof(string));
            dtheader.Columns.Add("CURRENTVEP", typeof(decimal));
            dtheader.Columns.Add("PROPOSEDVEP", typeof(decimal));
            dtheader.Columns.Add("PROPOSEDVIP", typeof(decimal));
            dtheader.Columns.Add("DISCPERCASE", typeof(decimal));
            dtheader.Columns.Add("DISCPERCENTAGE", typeof(decimal));
            dtheader.Columns.Add("EXPECTEDVOL", typeof(int));
            dtheader.Columns.Add("DATAAREAID", typeof(string));
            dtheader.Columns.Add("Startdate", typeof(string));
            dtheader.Columns.Add("Enddate", typeof(string));
            foreach (DataRow dr in _dt.Rows)
            {
                DataRow row;
                row = dtheader.NewRow();
                decimal CurrentVEPprice = Convert.ToDecimal(dr["CURRENTVEP"]);
                decimal NewVEPprice = Convert.ToDecimal(dr["PROPOSEDVEP"]);
                row["ITEMTYPE"] = dr["ITEMTYPE"];
                row["ITEMTYPEDESC"] = dr["ITEMTYPEDESC"];
                row["ITEMTYPECODE"] = dr["ITEMTYPECODE"];
                row["ITEMNAME"] = dr["ITEMNAME"];
                row["CURRENTVEP"] = dr["CURRENTVEP"];
                row["PROPOSEDVEP"] = dr["PROPOSEDVEP"];
                row["PROPOSEDVIP"] = (VatPerBranch * NewVEPprice / 100) + NewVEPprice;
                row["DISCPERCASE"] = CurrentVEPprice - NewVEPprice;
                row["DISCPERCENTAGE"] = Convert.ToDecimal((((CurrentVEPprice - NewVEPprice) / CurrentVEPprice) * 100).ToString("#0.0"));
                row["EXPECTEDVOL"] = dr["EXPECTEDVOL"];
                row["DATAAREAID"] = dr["DATAAREAID"];
                row["Startdate"] = dr["Startdate"];
                row["Enddate"] = dr["Enddate"];
                dtheader.Rows.Add(row);
                dtheader.AcceptChanges();


            }
            return dtheader;

        }


        [HttpPost]
        public JsonResult UploadImagedelete(string PVCNUM, string ImageUrl)
        {
            DataTable _dt = new DataTable();
            string json = string.Empty;
            PVCCCAF.AppCode.Global global = new AppCode.Global();

            try
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>();
                _plist.Add("@PVCNO"); _vlist.Add(PVCNUM);
                _plist.Add("@ImageUrl"); _vlist.Add(ImageUrl);
                _dt = PVCCCAF.AppCode.Global.GetData_New("USP_UPDATE_IMAGESTAUTS", CommandType.StoredProcedure, _plist, _vlist);
                return Json(new { success = true, data = obj.ConvertDataTableTojSonString(_dt) });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message.ToString() });
            }


        }

        [HttpPost]
        public JsonResult UpdatepriceGroup(string PriceGroupName, string PriceGroupId, string olDPriceGroupId, string olDPriceGroupName
            , string PVCNO, string Actiontype, string Entityid)
        {
            DataTable _dt = new DataTable();
            string json = string.Empty;

            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@DATAAREAID"); _vlist.Add(Entityid);
            _plist.Add("@ACCOUNTOLDID"); _vlist.Add(olDPriceGroupId);
            _plist.Add("@ACCOUNTSELECTION"); _vlist.Add(PriceGroupId);
            _plist.Add("@ACCOUNTSELECTIONNAME"); _vlist.Add(PriceGroupName);
            _plist.Add("@PVCNO"); _vlist.Add(PVCNO);
            _plist.Add("@OLDACCOUNTSELECTIONNAME"); _vlist.Add(olDPriceGroupName);
            _plist.Add("@TYPE"); _vlist.Add(Actiontype);

            try
            {

                _dt = PVCCCAF.AppCode.Global.GetData_New("USP_UPDATE_ACCOUNTSELCETION", CommandType.StoredProcedure, _plist, _vlist);
                return Json(new { success = true, data = obj.ConvertDataTableTojSonString(_dt) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message.ToString() });
            }


        }


        [HttpPost]
        public JsonResult UpdateItemGroup(string ItemGroupName, string ItemGroupId, string OlDItemGroupId, string OlDItemGroupName
            , string PVCNO, string Actiontype, string Entityid)
        {
            DataTable _dt = new DataTable();
            string json = string.Empty;

            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@DATAAREAID"); _vlist.Add(Entityid);
            _plist.Add("@ACCOUNTOLDID"); _vlist.Add(OlDItemGroupId);
            _plist.Add("@ACCOUNTSELECTION"); _vlist.Add(ItemGroupId);
            _plist.Add("@ACCOUNTSELECTIONNAME"); _vlist.Add(ItemGroupName);
            _plist.Add("@PVCNO"); _vlist.Add(PVCNO);
            _plist.Add("@OLDACCOUNTSELECTIONNAME"); _vlist.Add(OlDItemGroupName);
            _plist.Add("@TYPE"); _vlist.Add(Actiontype);

            try
            {

                _dt = PVCCCAF.AppCode.Global.GetData_New("USP_UPDATE_ACCOUNTSELCETION", CommandType.StoredProcedure, _plist, _vlist);
                return Json(new { success = true, data = obj.ConvertDataTableTojSonString(_dt) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message.ToString() });
            }


        }


        #endregion

        #region PV Form History
        [HttpGet]
        public ActionResult PVFormHistory()
        {
            DataTable _dt = new DataTable();
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());
            _plist.Add("@USERCODE"); _vlist.Add(Session["USERCODE"].ToString());
            _plist.Add("@TYPE"); _vlist.Add(Convert.ToString(3));// for getting the First status drop down
            _dt = PVCCCAF.AppCode.Global.GetData_New("USP_GETPRICBINDPVHISTORYMASTERTYPE", CommandType.StoredProcedure, _plist, _vlist);
            ViewBag.StatusTypeDropdown = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectListWithOption(_dt, "VALUE", "DESC");
            return View();
        }
        public JsonResult GetPVHistory(string FILTERID, string FILTERVALUE, string FromDate, string ToDate)
        {
            DataTable _dt = new DataTable();
            if (FILTERID == "0")
            {
                int statusvalue = Convert.ToInt32(FILTERVALUE);

                if (statusvalue == 0 || statusvalue == 1)
                {
                    _dt = GetPVAPPROVAL_REJECT_DATAUSERLEVELWISE(statusvalue, FromDate, ToDate);
                }
                else if (statusvalue == 2 || statusvalue == 3 || statusvalue == 4 || statusvalue == 6)
                {
                    _dt = GetPVCANCELLATION_RELEASEDMSD_RELEASED_DATAUSERLEVELWISE(statusvalue, FromDate, ToDate);
                }
                else if (statusvalue == 5)
                {
                    _dt = GetPVCALLSTATUSDATAUSERLEVELWISE(statusvalue, FromDate, ToDate);
                }
            }
            else if (FILTERID == "1")
            {
                _dt = GetPVDATA_ACCPVNOHistory(FILTERVALUE, FromDate, ToDate);
            }
            else if (FILTERID == "2")
            {
                _dt = GetPVDATASUBMITTEDUSERWISE(FILTERVALUE, FromDate, ToDate);
            }
            string DataResult = obj.ConvertDataTableTojSonString(_dt);
            return Json(DataResult, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPVType(int TYPE)
        {
            DataTable _dt = new DataTable();
            if (TYPE == 1)// GET ALL PV NO
            {
                _dt = GetPVNOUserlevelWise();
            }
            else if (TYPE == 2)// GET SUBMITTED BY
            {
                List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>(); 
                _dt = PVCCCAF.AppCode.Global.GetData_New("USP_GET_SUBMITTEDBYUSER", CommandType.StoredProcedure, _plist, _vlist);
            }
            else
            {
                string json = string.Empty; List<string> _plist = new List<string>();
                List<string> _vlist = new List<string>(); _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());
                _plist.Add("@USERCODE"); _vlist.Add(Session["USERCODE"].ToString());
                _plist.Add("@TYPE"); _vlist.Add(Convert.ToString(TYPE));
                _dt = PVCCCAF.AppCode.Global.GetData_New("USP_GETPRICBINDPVHISTORYMASTERTYPE", CommandType.StoredProcedure, _plist, _vlist);
            }
            string DataResult = obj.ConvertDataTableTojSonString(_dt);
            return Json(DataResult, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetPVHistoryExcel(string FILTERID, string FILTERVALUE, string FromDate, string ToDate)
        {
            DataTable _dt = new DataTable();
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());
            _plist.Add("@FILTERID"); _vlist.Add(Convert.ToString(FILTERID));
            _plist.Add("@FILTERVALUE"); _vlist.Add(Convert.ToString(FILTERVALUE));
            if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
            {
                _plist.Add("@FROMDATE"); _vlist.Add(FromDate);
                _plist.Add("@TODATE"); _vlist.Add(ToDate);
            }
            if (FILTERID == "0")
            {
                int statusvalue = Convert.ToInt32(FILTERVALUE);

                if (statusvalue == 0 || statusvalue == 1)
                {
                    _dt = GetPVAPPROVAL_REJECT_DATAUSERLEVELWISEEXCEL(statusvalue, FromDate, ToDate);
                }
                else if (statusvalue == 2 || statusvalue == 3 || statusvalue == 4)
                {
                    _dt = GetPVCANCELLATION_RELEASEDMSD_RELEASED_DATAUSERLEVELWISEEXCEL(statusvalue, FromDate, ToDate);
                }
                else if (statusvalue == 5)
                {
                    _dt = GetPVCALLSTATUSDATAUSERLEVELWISEEXCEL(statusvalue, FromDate, ToDate);
                }
            }
            else if (FILTERID == "1")
            {
                _dt = GetPVDATA_ACCPVNOHistoryEXCEL(FILTERVALUE, FromDate, ToDate);
            }
            else if (FILTERID == "2")
            {
                _dt = GetPVDATASUBMITTEDUSERWISEEXCEL(FILTERVALUE, FromDate, ToDate);
            }

            _dt.TableName = "FormHistory";
            PVCCCAF.AppCode.Global obj = new AppCode.Global();
            DataSet lds = new DataSet();
            lds.Tables.Add(_dt);
            GenerateExcel(_dt);
            return Json("", JsonRequestBehavior.AllowGet);
        }
        private void GenerateExcel(DataTable Dt)
        {
            var memoryStream = new MemoryStream();
            using (var excelPackage = new ExcelPackage(memoryStream))
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add("PVFormHistory");
                worksheet.Cells["A1"].LoadFromDataTable(Dt, true, TableStyles.None);
                OfficeOpenXml.Table.ExcelTable table = worksheet.Tables[Dt.TableName];
                table.ShowFilter = false;
                worksheet.Column(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Column(1).Width = 15;
                worksheet.Column(2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Column(2).Width = 15;

                worksheet.Column(3).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Column(3).Width = 25;
                worksheet.Column(4).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Column(4).Width = 15;
                worksheet.Column(5).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Column(5).Width = 25;
                worksheet.Column(6).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Column(6).Width = 15;
                worksheet.Column(7).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Column(7).Width = 25;
                worksheet.Column(8).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Column(8).Width = 15;
                worksheet.Column(9).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Column(9).Width = 25;

                worksheet.Column(10).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Column(10).Width = 15;
                worksheet.Column(11).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Column(11).Width = 25;
                worksheet.Column(12).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Column(12).Width = 30;
                worksheet.Column(13).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Column(13).Width = 25;
                //worksheet.Column(2).AutoFit();

                Session["DownloadExcel_FileManager"] = excelPackage.GetAsByteArray();

            }
        }
        public ActionResult DownloadExcel()
        {

            if (Session["DownloadExcel_FileManager"] != null)
            {
                byte[] data = Session["DownloadExcel_FileManager"] as byte[];
                return File(data, "application/octet-stream", "PVFormHistory.xlsx");

            }
            else
            {
                return new EmptyResult();
            }

        }

        private DataTable GetPVAPPROVAL_REJECT_DATAUSERLEVELWISEEXCEL(int FILTERVALUE, string FromDate, string ToDate)
        {
            DataTable _dt = new DataTable();
            DataTable _userlevel = getUserHierarchyLevel();
            string query = string.Empty;
            string DataAreaid = Session["DATAAREAID"].ToString();
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            if (_userlevel != null && _userlevel.Rows.Count > 0)
            {

                foreach (DataRow _dtrow in _userlevel.Rows)
                {
                    if (FILTERVALUE == 0 || FILTERVALUE == 1)// 0 for Approved case and 1 for reject case
                    {
                        int level1status = FILTERVALUE == 0 ? 2 : 3;
                        int level2status = FILTERVALUE == 0 ? 4 : 5;
                        int level3status = FILTERVALUE == 0 ? 6 : 7;
                        if (Convert.ToInt32(_dtrow["LEVELID"]) == 0)
                        {

                            query += "SELECT  PVNumber as [PV Number],SubmittedBy as[Submitted By],Submitteddateandtime as [Submitted Date/Time],firstApprover as [First Approver],firstApproveddateandtime as [Approved Date/Time],secondApprover as [Second Approver],SecondApproveddateandtime as [Approved Date/Time],thirdApprover [Third Approver],thirdApproveddateandtime as [Approved Date/Time],RELEASEDBY [Released By],RELEASEDDATETIME [Released Date/Time],REJECTBY [Rejected By],REJECTDATETIME [Rejected Date/Time],Description,STATUSDESC as Status FROM VW_GETPVCHISTORY PVC WHERE  " + Environment.NewLine;
                            if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                            {
                                query += " CAST(PVC.CREATEDDATETIME AS DATE)>= '" + FromDate + "' AND CAST(PVC.CREATEDDATETIME AS DATE)<= '" + ToDate + "' AND" + Environment.NewLine;

                            }

                            if (level1status == 2)
                            {
                                query += "  PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;

                            }
                            else if (level1status == 3)
                            {
                                query += " PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;
                            }
                          
                        }

                        else if (Convert.ToInt32(_dtrow["LEVELID"]) == 1)
                        {

                            query += "SELECT PVNumber as [PV Number],SubmittedBy as[Submitted By],Submitteddateandtime as [Submitted Date/Time],firstApprover as [First Approver],firstApproveddateandtime as [Approved Date/Time],secondApprover as [Second Approver],SecondApproveddateandtime as [Approved Date/Time],thirdApprover [Third Approver],thirdApproveddateandtime as [Approved Date/Time],RELEASEDBY [Released By],RELEASEDDATETIME [Released Date/Time],REJECTBY [Rejected By],REJECTDATETIME [Rejected Date/Time],Description,STATUSDESC as Status  FROM VW_GETPVCHISTORY PVC WHERE " + Environment.NewLine;
                       
                            if (level1status == 2)
                            {
                                query += "  PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;
                                if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                                {
                                    query += "AND CAST(PVC.firstApproveddateandtime AS DATE)>= '" + FromDate + "' AND CAST(PVC.firstApproveddateandtime AS DATE)<= '" + ToDate + "' AND" + Environment.NewLine;

                                }

                            }
                            else if (level1status == 3)
                            {
                                query += "  PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;
                            }


                           
                        }
                        else if (Convert.ToInt32(_dtrow["LEVELID"]) == 2)
                        {
                            query += "SELECT PVNumber as [PV Number],SubmittedBy as[Submitted By],Submitteddateandtime as [Submitted Date/Time],firstApprover as [First Approver],firstApproveddateandtime as [Approved Date/Time],secondApprover as [Second Approver],SecondApproveddateandtime as [Approved Date/Time],thirdApprover [Third Approver],thirdApproveddateandtime as [Approved Date/Time],RELEASEDBY [Released By],RELEASEDDATETIME [Released Date/Time],REJECTBY [Rejected By],REJECTDATETIME [Rejected Date/Time],Description,STATUSDESC as Status FROM VW_GETPVCHISTORY PVC WHERE " + Environment.NewLine;
                         
                            if (level2status == 4)
                            {
                                query += "  PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;

                                if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                                {
                                    query += "AND CAST(PVC.SecondApproveddateandtime AS DATE)>= '" + FromDate + "' AND CAST(PVC.SecondApproveddateandtime AS DATE)<= '" + ToDate + "' AND" + Environment.NewLine;

                                }
                            }
                            else if (level2status == 5)
                            {
                                query += "  PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;
                            }

                           
                        }
                        else if (Convert.ToInt32(_dtrow["LEVELID"]) == 3)
                        {
                            query += "SELECT PVNumber as [PV Number],SubmittedBy as[Submitted By],Submitteddateandtime as [Submitted Date/Time],firstApprover as [First Approver],firstApproveddateandtime as [Approved Date/Time],secondApprover as [Second Approver],SecondApproveddateandtime as [Approved Date/Time],thirdApprover [Third Approver],thirdApproveddateandtime as [Approved Date/Time],RELEASEDBY [Released By],RELEASEDDATETIME [Released Date/Time],REJECTBY [Rejected By],REJECTDATETIME [Rejected Date/Time],Description,STATUSDESC as Status FROM VW_GETPVCHISTORY PVC WHERE  " + Environment.NewLine;
                          
                            if (level3status == 6)
                            {
                                query += "  PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;

                                if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                                {
                                    query += "AND CAST(PVC.thirdApproveddateandtime AS DATE)>= '" + FromDate + "' AND CAST(PVC.thirdApproveddateandtime AS DATE)<= '" + ToDate + "' AND" + Environment.NewLine;

                                }
                            }
                            else if (level3status == 7)
                            {
                                query += " PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;
                            }

                           
                        }
                        else if (Convert.ToInt32(_dtrow["LEVELID"]) == 4)
                        {
                            query += "SELECT PVNumber as [PV Number],SubmittedBy as[Submitted By],Submitteddateandtime as [Submitted Date/Time],firstApprover as [First Approver],firstApproveddateandtime as [Approved Date/Time],secondApprover as [Second Approver],SecondApproveddateandtime as [Approved Date/Time],thirdApprover [Third Approver],thirdApproveddateandtime as [Approved Date/Time],RELEASEDBY [Released By],RELEASEDDATETIME [Released Date/Time],REJECTBY [Rejected By],REJECTDATETIME [Rejected Date/Time],Description,STATUSDESC as Status FROM VW_GETPVCHISTORY PVC WHERE  " + Environment.NewLine;

                            if (level3status == 6)
                            {
                                query += "  PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;

                                if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                                {
                                    query += "AND CAST(PVC.Submitteddateandtime AS DATE)>= '" + FromDate + "' AND CAST(PVC.Submitteddateandtime AS DATE)<= '" + ToDate + "' AND" + Environment.NewLine;

                                }
                            }
                            else if (level3status == 7)
                            {
                                query += " PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;
                            }

                           
                        }

                    }

                }

            }
            
            _dt = PVCCCAF.AppCode.Global.GetData_New(query, CommandType.Text, _plist, _vlist);
            return _dt;

        }

        private DataTable GetPVCANCELLATION_RELEASEDMSD_RELEASED_DATAUSERLEVELWISEEXCEL(int FILTERVALUE, string FromDate, string ToDate)
        {
            DataTable _dt = new DataTable();
            
            string query = string.Empty;
            string DataAreaid = Session["DATAAREAID"].ToString();
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
                    
                    if (FILTERVALUE == 3 || FILTERVALUE == 4)// 2 for CANCELLATION case and 3 for RELEASED  case AND 4 for POST RELEASED  case
                    {
                        int levelstatus = FILTERVALUE == 3 ? 8 : FILTERVALUE == 4 ? 11 : 0;
                        query += "SELECT PVNumber as [PV Number],SubmittedBy as[Submitted By],Submitteddateandtime as [Submitted Date/Time],firstApprover as [First Approver],firstApproveddateandtime as [Approved Date/Time],secondApprover as [Second Approver],SecondApproveddateandtime as [Approved Date/Time],thirdApprover [Third Approver],thirdApproveddateandtime as [Approved Date/Time],RELEASEDBY [Released By],RELEASEDDATETIME [Released Date/Time],REJECTBY [Rejected By],REJECTDATETIME [Rejected Date/Time],Description,STATUSDESC as Status FROM VW_GETPVCHISTORYRELEASE PVC WHERE " + Environment.NewLine;
                        if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                        {
                            query += " CAST(PVC.CREATEDDATETIME AS DATE)>= '" + FromDate + "' AND CAST(PVC.CREATEDDATETIME AS DATE)<= '" + ToDate + "' AND" + Environment.NewLine;

                        }
                        query += " PVC.STATUS='" + levelstatus + "'" + Environment.NewLine;
                        query += " UNION ALL ";
                    }
                    if (FILTERVALUE == 2)// 2 for CANCELLATION case and 3 for RELEASED  case AND 4 for POST RELEASED  case
                    {
                        query += "SELECT PVNumber as [PV Number],SubmittedBy as[Submitted By],Submitteddateandtime as [Submitted Date/Time],firstApprover as [First Approver],firstApproveddateandtime as [Approved Date/Time],secondApprover as [Second Approver],SecondApproveddateandtime as [Approved Date/Time],thirdApprover [Third Approver],thirdApproveddateandtime as [Approved Date/Time],RELEASEDBY [Released By],RELEASEDDATETIME [Released Date/Time],REJECTBY [Rejected By],REJECTDATETIME [Rejected Date/Time],Description,STATUSDESC as Status FROM VW_GETPVCHISTORY PVC WHERE " + Environment.NewLine;
                        if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                        {
                            query += " CAST(PVC.CREATEDDATETIME AS DATE)>= '" + FromDate + "' AND CAST(PVC.CREATEDDATETIME AS DATE)<= '" + ToDate + "' AND" + Environment.NewLine;

                        }
                        query += "  PVC.STATUS='9'" + Environment.NewLine;
                        query += " UNION ALL ";
                    }
                    if (FILTERVALUE ==6)
                    {
                     
                        query += "SELECT PVNumber as [PV Number],SubmittedBy as[Submitted By],Submitteddateandtime as [Submitted Date/Time],firstApprover as [First Approver],firstApproveddateandtime as [Approved Date/Time],secondApprover as [Second Approver],SecondApproveddateandtime as [Approved Date/Time],thirdApprover [Third Approver],thirdApproveddateandtime as [Approved Date/Time],RELEASEDBY [Released By],RELEASEDDATETIME [Released Date/Time],Description,STATUSDESC as Status FROM  VW_GETPVCHISTORY PVC WHERE" + Environment.NewLine;
                        if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                        {
                            query += " CAST(PVC.CREATEDDATETIME AS DATE)>= '" + FromDate + "' AND CAST(PVC.CREATEDDATETIME AS DATE)<= '" + ToDate + "' AND" + Environment.NewLine;

                        }
                        query += " PVC.STATUS IN(8,11) AND  ISEDIT_ON_RELEASE=1 " + Environment.NewLine;
                        query += " UNION ALL ";
                    }

              if (query.Length > 0)
            {
            
                if (FILTERVALUE == 6)
                { query += "order by RELEASEDDATETIME desc"; }
            }
            _dt = PVCCCAF.AppCode.Global.GetData_New(query, CommandType.Text, _plist, _vlist);
            return _dt;

        }

        private DataTable GetPVCALLSTATUSDATAUSERLEVELWISEEXCEL(int FILTERVALUE, string FromDate, string ToDate)
        {
            DataTable _dt = new DataTable();
            string query = string.Empty;
            string DataAreaid = Session["DATAAREAID"].ToString();
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            query += "SELECT PVNumber as [PV Number],SubmittedBy as[Submitted By],Submitteddateandtime as [Submitted Date/Time],firstApprover as [First Approver],firstApproveddateandtime as [Approved Date/Time],secondApprover as [Second Approver],SecondApproveddateandtime as [Approved Date/Time],thirdApprover [Third Approver],thirdApproveddateandtime as [Approved Date/Time],RELEASEDBY [Released By],RELEASEDDATETIME [Released Date/Time],REJECTBY [Rejected By],REJECTDATETIME [Rejected Date/Time],Description,STATUSDESC as Status FROM VW_GETPVCHISTORY PVC WHERE  " + Environment.NewLine;
            if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
            {
                query += " CAST(PVC.CREATEDDATETIME AS DATE)>= '" + FromDate + "' AND CAST(PVC.CREATEDDATETIME AS DATE)<= '" + ToDate + "' AND" + Environment.NewLine;

            }
            query += "PVC.STATUS NOT IN (0)";

            _dt = PVCCCAF.AppCode.Global.GetData_New(query, CommandType.Text, _plist, _vlist);
            return _dt;

        }

        private DataTable GetPVDATA_ACCPVNOHistoryEXCEL(string PVCNO, string FromDate, string ToDate)
        {
            DataTable _dt = new DataTable();
            string query = string.Empty;
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            query += "SELECT PVNumber as [PV Number],SubmittedBy as[Submitted By],Submitteddateandtime as [Submitted Date/Time],firstApprover as [First Approver],firstApproveddateandtime as [Approved Date/Time],secondApprover as [Second Approver],SecondApproveddateandtime as [Approved Date/Time],thirdApprover [Third Approver],thirdApproveddateandtime as [Approved Date/Time],RELEASEDBY [Released By],RELEASEDDATETIME [Released Date/Time],REJECTBY [Rejected By],REJECTDATETIME [Rejected Date/Time],Description,STATUSDESC as Status FROM VW_GETPVCHISTORY PVC WHERE " + Environment.NewLine;
            query += "  PVC.PVCNO = '" + PVCNO + "'" + Environment.NewLine;
          
            _dt = PVCCCAF.AppCode.Global.GetData_New(query, CommandType.Text, _plist, _vlist);
            return _dt;

        }

        private DataTable GetPVDATASUBMITTEDUSERWISEEXCEL(string usercode, string FromDate, string ToDate)
        {
            DataTable _dt = new DataTable();
            DataTable _userlevel = getUserHierarchyLevel();
            string query = string.Empty;
            string DataAreaid = Session["DATAAREAID"].ToString();
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
              query += "SELECT PVNumber as [PV Number],SubmittedBy as[Submitted By],Submitteddateandtime as [Submitted Date/Time],firstApprover as [First Approver],firstApproveddateandtime as [Approved Date/Time],secondApprover as [Second Approver],SecondApproveddateandtime as [Approved Date/Time],thirdApprover [Third Approver],thirdApproveddateandtime as [Approved Date/Time],RELEASEDBY [Released By],RELEASEDDATETIME [Released Date/Time],REJECTBY [Rejected By],REJECTDATETIME [Rejected Date/Time],Description,STATUSDESC as Status FROM VW_GETPVCHISTORY PVC WHERE PVC.DATAAREAID = '" + DataAreaid + "' " + Environment.NewLine;
              if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
              {
                  query += "AND CAST(PVC.CREATEDDATETIME AS DATE)>= '" + FromDate + "' AND CAST(PVC.CREATEDDATETIME AS DATE)<= '" + ToDate + "'" + Environment.NewLine;

              }

                    query += " AND PVC.SubmittedBy = '" + usercode + "'" + Environment.NewLine;
            _dt = PVCCCAF.AppCode.Global.GetData_New(query, CommandType.Text, _plist, _vlist);
            return _dt;

        }


        #endregion

        #region Post Price Setup Form

        public ActionResult PostPriceSetupForm()
        {
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            PVFormViewModel _pvformmodel = new PVFormViewModel();
            ViewBag.PVCNUMSEQ = "";
            ViewBag.ISPOSTPRICE = 1;
            DataTable _dtEntity = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_ENTITYMASTER]", CommandType.StoredProcedure, _plist, _vlist);
            ViewBag.EntityDropdown = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectListWithOption(_dtEntity, "ENTITYID", "ENTITYDESC");

            DataTable _dtBranch = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_BRANCHMASTER]", CommandType.StoredProcedure, _plist, _vlist);
            ViewBag.BranchDropdown = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectListWithOption(_dtBranch, "VATPERCENT", "BRANCHDESC");

            DataTable _dtCustApplicable = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_CUSTAPPLICABLETYPEMASTER]", CommandType.StoredProcedure, _plist, _vlist);
            ViewBag.CustomerApplicableDropdown = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectListWithOption(_dtCustApplicable, "CUSTAPPLICABLETYPEID", "CUSTAPPLICABLETYPEDESC");

            DataTable _dtItemType = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_ITEMTYPEMASTER]", CommandType.StoredProcedure, _plist, _vlist);
            ViewBag.ItemType = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectListWithOption(_dtItemType, "ITEMTYPEID", "ITEMTYPEDESC");


            _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());
            DataTable _dtPriceTier = PVCCCAF.AppCode.Global.GetData_New("[dbo].[USP_PRICETIERMASTER]", CommandType.StoredProcedure, _plist, _vlist);
            ViewBag.PriceTierDropdown = PVCCCAF.BussinessLayer.BussinessLayer.ToSelectListWithOption(_dtPriceTier, "TIERID", "TIERDESC");

            return View("PriceVariance", _pvformmodel);

        }



        #endregion

        #region Forms in Queue

        public ActionResult QueueForm()
        {
            return View();
        }


        public JsonResult GetPVCNOStatusWiseQueue(int TYPE, string PVCNO = "")
        {
            DataTable _dt = new DataTable();

            if (TYPE == 4)//getting approval record
            {
                _dt = GetPVDATA_FORAPPROVALWISE();
            }
            else if (TYPE == 5 && !string.IsNullOrEmpty(PVCNO))
            {
                _dt = GetPVDATA_ACCPVNO(PVCNO, "", "");
            }
            string DataResult = obj.ConvertDataTableTojSonString(_dt);
            return Json(DataResult, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Draft And Change Request

        public ActionResult DraftAndChangeRequest()
        {
            return View();
        }

        public JsonResult GetPVCNOStatusWise(int TYPE, string PVCNO = "")
        {
            DataTable _dt = new DataTable();
            string json = string.Empty; List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            _plist.Add("@DATAAREAID"); _vlist.Add(Session["DATAAREAID"].ToString());
            _plist.Add("@USERCODE"); _vlist.Add(Session["USERCODE"].ToString());
            _plist.Add("@TYPE"); _vlist.Add(Convert.ToString(TYPE));// GET THE DRAFT AND CHANGE FOR REQUEST RECORDS
            _plist.Add("@PVCNO"); _vlist.Add(PVCNO);
            _dt = PVCCCAF.AppCode.Global.GetData_New("USP_GETPVCNOSTATUSWISE", CommandType.StoredProcedure, _plist, _vlist);
            string DataResult = obj.ConvertDataTableTojSonString(_dt);
            return Json(DataResult, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region "Get PV DATA STATUS WISE"

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

        private DataTable GetPVNOUserlevelWise()
        {
            DataTable _dt = new DataTable();
            DataTable _userlevel = getUserHierarchyLevel();
            string query = string.Empty;
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            query += "SELECT CREATEDBY,PVCNO FROM AX.PVCMASTER WHERE STATUS not in (0) " + Environment.NewLine;
            _dt = PVCCCAF.AppCode.Global.GetData_New(query, CommandType.Text, _plist, _vlist);
            return _dt;

        }
        private DataTable GetPVAPPROVAL_REJECT_DATAUSERLEVELWISE(int FILTERVALUE, string FromDate, string ToDate)
        {
            DataTable _dt = new DataTable();
            DataTable _userlevel = getUserHierarchyLevel();
            string query = string.Empty;
            string DataAreaid = Session["DATAAREAID"].ToString();
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            if (_userlevel != null && _userlevel.Rows.Count > 0)
            {
                
                string a = @"'<a href=""#"" onclick=""SendDataApproval('''+pvc.PVCNO + ''',''' +convert(nvarchar,pvc.STATUS )+ ''','''+pvc.SubmittedBy + ''')"" >'+pvc.PVCNO+'</a>'";
                foreach (DataRow _dtrow in _userlevel.Rows)
                {
                    //query += query == string.Empty ? "" : " UNION ALL ";

                    if (FILTERVALUE == 0 || FILTERVALUE == 1)// 0 for Approved case and 1 for reject case
                    {
                        int level1status = FILTERVALUE == 0 ? 2 : 3;
                        int level2status = FILTERVALUE == 0 ? 4 : 5;
                        int level3status = FILTERVALUE == 0 ? 6 : 7;
                        if (Convert.ToInt32(_dtrow["LEVELID"]) == 0)
                        {

                            query += "SELECT  " + a + " PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE  " + Environment.NewLine;
                            if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                            {
                                query += " CAST(PVC.CREATEDDATETIME AS DATE)>= '" + FromDate + "' AND CAST(PVC.CREATEDDATETIME AS DATE)<= '" + ToDate + "' AND" + Environment.NewLine;
                            }
                         
                            if (level1status == 2)
                            {
                                query += "  PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;

                            }
                            else if (level1status == 3)
                            {
                                query += "  PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;
                            }

                           
                        }
                        else if (Convert.ToInt32(_dtrow["LEVELID"]) == 1)
                        {

                            query += "SELECT  " + a + "  PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE " + Environment.NewLine;

                            //  query += "  PVC.DATAAREAID = '" + DataAreaid + "'  AND PVC.SubmittedBy = '" + _dtrow["USERID"].ToString() + "' AND" + Environment.NewLine;

                            if (level1status == 2)
                            {
                                query += "  PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;
                                if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                                {
                                    query += "AND CAST(PVC.firstApproveddateandtime AS DATE)>= '" + FromDate + "' AND CAST(PVC.firstApproveddateandtime AS DATE)<= '" + ToDate + "'" + Environment.NewLine;

                                }

                            }
                            else if (level1status == 3)
                            {
                                query += " PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;
                            }


                           
                        }
                        else if (Convert.ToInt32(_dtrow["LEVELID"]) == 2)
                        {
                            query += "SELECT  " + a + "   PVCLINK, * FROM VW_GETPVCHISTORY PVC WHERE " + Environment.NewLine;
                            //    query += "  PVC.DATAAREAID = '" + DataAreaid + "' AND PVC.SubmittedBy = '" + _dtrow["USERID"].ToString() + "' AND" + Environment.NewLine;
                            if (level2status == 4)
                            {
                                query += "  PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;

                                if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                                {
                                    query += "AND CAST(PVC.SecondApproveddateandtime AS DATE)>= '" + FromDate + "' AND CAST(PVC.SecondApproveddateandtime AS DATE)<= '" + ToDate + "'" + Environment.NewLine;

                                }
                            }
                            else if (level2status == 5)
                            {
                                query += "  PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;
                            }

                            
                        }
                        else if (Convert.ToInt32(_dtrow["LEVELID"]) == 3)
                        {
                            query += "SELECT  " + a + "  PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE " + Environment.NewLine;
                            //  query += "  PVC.DATAAREAID = '" + DataAreaid + "' AND PVC.SubmittedBy = '" + _dtrow["USERID"].ToString() + "' AND" + Environment.NewLine;
                            if (level3status == 6)
                            {
                                query += "  PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;

                                if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                                {
                                    query += "AND CAST(PVC.thirdApproveddateandtime AS DATE)>= '" + FromDate + "' AND CAST(PVC.thirdApproveddateandtime AS DATE)<= '" + ToDate + "'" + Environment.NewLine;

                                }
                            }
                            else if (level3status == 7)
                            {
                                query += "  PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;
                            }
                           
                        }
                        else if (Convert.ToInt32(_dtrow["LEVELID"]) == 4)
                        {
                            query += "SELECT  " + a + "  PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE " + Environment.NewLine;
                            //  query += "  PVC.DATAAREAID = '" + DataAreaid + "' AND PVC.SubmittedBy = '" + _dtrow["USERID"].ToString() + "' AND" + Environment.NewLine;
                            if (level3status == 6)
                            {
                                query += "  PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;

                                if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                                {
                                    query += "AND CAST(PVC.Submitteddateandtime AS DATE)>= '" + FromDate + "' AND CAST(PVC.Submitteddateandtime AS DATE)<= '" + ToDate + "'" + Environment.NewLine;

                                }
                            }
                            else if (level3status == 7)
                            {
                                query += "  PVC.STATUS in(" + level1status + "," + level2status + "," + level3status + ")" + Environment.NewLine;
                            }
                          
                        }
                    }

                }

            }
            
            _dt = PVCCCAF.AppCode.Global.GetData_New(query, CommandType.Text, _plist, _vlist);
            return _dt;

        }

        private DataTable GetPVCANCELLATION_RELEASEDMSD_RELEASED_DATAUSERLEVELWISE(int FILTERVALUE, string FromDate, string ToDate)
        {
            DataTable _dt = new DataTable();
            DataTable _userlevel = getUserHierarchyLevel();
            string query = string.Empty;
            string DataAreaid = Session["DATAAREAID"].ToString();
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
                string a = @"'<a href=""#"" onclick=""SendDataApproval('''+pvc.PVCNO + ''',''' +convert(nvarchar,pvc.STATUS )+ ''','''+pvc.SubmittedBy + ''')"" >'+pvc.PVCNO+'</a>'";
              
                    if (FILTERVALUE == 3 || FILTERVALUE == 4)// 2 for CANCELLATION case and 3 for RELEASED  case AND 4 for POST RELEASED  case
                    {
                        int levelstatus = FILTERVALUE == 3 ? 8 : FILTERVALUE == 4 ? 11 : 0;

                        query += "SELECT " + a + " PVCLINK,* FROM  VW_GETPVCHISTORYRELEASE PVC WHERE" + Environment.NewLine;
                        if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                        {
                            query += " CAST(PVC.CREATEDDATETIME AS DATE)>= '" + FromDate + "' AND CAST(PVC.CREATEDDATETIME AS DATE)<= '" + ToDate + "' AND" + Environment.NewLine;

                        }
                        query += "  PVC.STATUS='" + levelstatus + "'" + Environment.NewLine;
                        
                    }
                    if (FILTERVALUE == 2)// 2 for CANCELLATION case and 3 for RELEASED  case AND 4 for POST RELEASED  case
                    {
                        query += "SELECT " + a + " PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE " + Environment.NewLine;
                        if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                        {
                            query += " CAST(PVC.CREATEDDATETIME AS DATE)>= '" + FromDate + "' AND CAST(PVC.CREATEDDATETIME AS DATE)<= '" + ToDate + "' AND" + Environment.NewLine;

                        }
                        query += "   PVC.STATUS='9'" + Environment.NewLine;
                        
                    }
                    if (FILTERVALUE == 6)// 2 for CANCELLATION case and 3 for RELEASED  case AND 4 for POST RELEASED  case
                    {
                     
                        query += "SELECT " + a + " PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE  " + Environment.NewLine;
                        if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                        {
                            query += " CAST(PVC.CREATEDDATETIME AS DATE)>= '" + FromDate + "' AND CAST(PVC.CREATEDDATETIME AS DATE)<= '" + ToDate + "' AND" + Environment.NewLine;

                        }
                        query += " PVC.STATUS IN(8,11) AND ISEDIT_ON_RELEASE=1 " + Environment.NewLine;
                        
          
                    }
            if (query.Length > 0)
            {
                if (FILTERVALUE == 3)
                { query += "order by RELEASEDDATETIME desc"; }
            }
            _dt = PVCCCAF.AppCode.Global.GetData_New(query, CommandType.Text, _plist, _vlist);
            return _dt;

        }

        private DataTable GetPVCALLSTATUSDATAUSERLEVELWISE(int FILTERVALUE, string FromDate, string ToDate)
        {
            DataTable _dt = new DataTable();
            string query = string.Empty;
            string DataAreaid = Session["DATAAREAID"].ToString();
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();

            string a = @"'<a href=""#"" onclick=""SendDataApproval('''+pvc.PVCNO + ''',''' +convert(nvarchar,pvc.STATUS )+ ''','''+pvc.SubmittedBy + ''')"" >'+pvc.PVCNO+'</a>'";

         
                query += "SELECT   " + a + " PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE " + Environment.NewLine;
                if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                {
                    query += "  CAST(PVC.CREATEDDATETIME AS DATE)>= '" + FromDate + "' AND CAST(PVC.CREATEDDATETIME AS DATE)<= '" + ToDate + "' AND " + Environment.NewLine;

                }
            query += "PVC.STATUS NOT IN (0)";
          
            _dt = PVCCCAF.AppCode.Global.GetData_New(query, CommandType.Text, _plist, _vlist);
            return _dt;

        }

        private DataTable GetPVDATASUBMITTEDUSERWISE(string usercode, string FromDate, string ToDate)
        {
            DataTable _dt = new DataTable();
            DataTable _userlevel = getUserHierarchyLevel();
            string query = string.Empty;
            string DataAreaid = Session["DATAAREAID"].ToString();
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
             string a = @"'<a href=""#"" onclick=""SendDataApproval('''+pvc.PVCNO + ''',''' +convert(nvarchar,pvc.STATUS )+ ''','''+pvc.SubmittedBy + ''')"" >'+pvc.PVCNO+'</a>'";
              query += "SELECT    " + a + " PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE PVC.DATAAREAID = '" + DataAreaid + "'" + Environment.NewLine;
              if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
              {
                  query += "AND CAST(PVC.CREATEDDATETIME AS DATE)>= '" + FromDate + "' AND CAST(PVC.CREATEDDATETIME AS DATE)<= '" + ToDate + "'" + Environment.NewLine;

              }
              query += " AND PVC.SubmittedBy = '" + usercode + "'" + Environment.NewLine;
            _dt = PVCCCAF.AppCode.Global.GetData_New(query, CommandType.Text, _plist, _vlist);
            return _dt;

        }
        private DataTable GetPVDATA_ACCPVNOHistory(string PVCNO, string FromDate, string ToDate)
        {
            DataTable _dt = new DataTable();
            string query = string.Empty;
            string DataAreaid = Session["DATAAREAID"].ToString();
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
        
           string a = @"'<a href=""#"" onclick=""SendDataApproval('''+pvc.PVCNO + ''',''' +convert(nvarchar,pvc.STATUS )+ ''','''+pvc.SubmittedBy + ''')"" >'+pvc.PVCNO+'</a>'";
          query += "SELECT    " + a + " PVCLINK,* FROM VW_GETPVCHISTORY PVC where pvcno='"+PVCNO+"'" + Environment.NewLine;
                  

            _dt = PVCCCAF.AppCode.Global.GetData_New(query, CommandType.Text, _plist, _vlist);
            return _dt;

        }

        private DataTable GetPVDATA_ACCPVNO(string PVCNO, string FromDate, string ToDate)
        {
            DataTable _dt = new DataTable();
            DataTable _userlevel = getUserHierarchyLevel();
            string query = string.Empty;
            string sessionusercode = Session["USERCODE"].ToString();
            int isviewonly = 0;
            string DataAreaid = Session["DATAAREAID"].ToString();
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            if (_userlevel != null && _userlevel.Rows.Count > 0)
            {
                _userlevel = _userlevel.DefaultView.ToTable(true, "USERID", "LEVELID");
                foreach (DataRow _dtrow in _userlevel.Rows)
                {

                    string a = @"'<a href=""#"" onclick=""SendDataApproval('''+pvc.PVCNO + ''',''' +convert(nvarchar,pvc.STATUS )+ ''','''+pvc.SubmittedBy + ''')"" >'+pvc.PVCNO+'</a>'";
                    string usercode = _dtrow["USERID"].ToString();
                    isviewonly = usercode == sessionusercode ? 1 : 0;
                    if (Convert.ToInt32(_dtrow["LEVELID"]) == 0)
                    {

                        query += "SELECT  " + isviewonly + " ISVIEWONLY, CASE WHEN PVC.STATUS IN(0,10) THEN " + a + " ELSE  pvc.PVCNO END PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE PVC.DATAAREAID = '" + DataAreaid + "' AND " + Environment.NewLine;
                        // query += "CAST(PVC.CREATEDDATETIME AS DATE)>= '" + FromDate + "' AND CAST(PVC.CREATEDDATETIME AS DATE)<= '" + ToDate + "'" + Environment.NewLine;
                        query += "  PVC.SubmittedBy = '" + _dtrow["USERID"].ToString() + "' AND PVC.PVCNO = '" + PVCNO + "'" + Environment.NewLine;
                        query += " UNION ALL ";
                    }
                    else if (Convert.ToInt32(_dtrow["LEVELID"]) == 1)
                    {
                        query += "SELECT  " + isviewonly + " ISVIEWONLY, CASE WHEN PVC.STATUS=1 THEN  " + a + " ELSE  pvc.PVCNO END PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE PVC.DATAAREAID = '" + DataAreaid + "' AND " + Environment.NewLine;
                        // query += "CAST(PVC.CREATEDDATETIME AS DATE)>= '" + FromDate + "' AND CAST(PVC.CREATEDDATETIME AS DATE)<= '" + ToDate + "'" + Environment.NewLine;
                        query += "   PVC.SubmittedBy = '" + _dtrow["USERID"].ToString() + "' AND PVC.PVCNO = '" + PVCNO + "'" + Environment.NewLine;
                        query += " UNION ALL ";
                    }
                    else if (Convert.ToInt32(_dtrow["LEVELID"]) == 2)
                    {
                        query += "SELECT  " + isviewonly + " ISVIEWONLY, CASE WHEN PVC.STATUS=2 THEN  " + a + " ELSE  pvc.PVCNO END PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE PVC.DATAAREAID = '" + DataAreaid + "' AND " + Environment.NewLine;
                        //  query += "CAST(PVC.CREATEDDATETIME AS DATE)>= '" + FromDate + "' AND CAST(PVC.CREATEDDATETIME AS DATE)<= '" + ToDate + "'" + Environment.NewLine;
                        query += "  PVC.SubmittedBy = '" + _dtrow["USERID"].ToString() + "' AND PVC.PVCNO = '" + PVCNO + "'" + Environment.NewLine;
                        query += " UNION ALL ";
                    }
                    else if (Convert.ToInt32(_dtrow["LEVELID"]) == 3)
                    {
                        query += "SELECT   " + isviewonly + " ISVIEWONLY, CASE WHEN PVC.STATUS=4 THEN  " + a + " ELSE  pvc.PVCNO END PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE PVC.DATAAREAID = '" + DataAreaid + "' AND " + Environment.NewLine;
                        // query += "CAST(PVC.CREATEDDATETIME AS DATE)>= '" + FromDate + "' AND CAST(PVC.CREATEDDATETIME AS DATE)<= '" + ToDate + "'" + Environment.NewLine;
                        query += " PVC.SubmittedBy = '" + _dtrow["USERID"].ToString() + "' AND  PVC.PVCNO = '" + PVCNO + "'" + Environment.NewLine;
                        query += " UNION ALL ";
                    }
                    else if (Convert.ToInt32(_dtrow["LEVELID"]) == 4)
                    {
                        query += "SELECT  " + isviewonly + " ISVIEWONLY, CASE WHEN PVC.STATUS=6 THEN  " + a + " ELSE  pvc.PVCNO END PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE PVC.DATAAREAID = '" + DataAreaid + "' AND " + Environment.NewLine;
                        //  query += "CAST(PVC.CREATEDDATETIME AS DATE)>= '" + FromDate + "' AND CAST(PVC.CREATEDDATETIME AS DATE)<= '" + ToDate + "'" + Environment.NewLine;
                        query += "  PVC.SubmittedBy = '" + _dtrow["USERID"].ToString() + "' AND PVC.PVCNO = '" + PVCNO + "'" + Environment.NewLine;
                        query += " UNION ALL ";
                    }

                }

            }
            if (query.Length > 0)
            {
                query = query.Substring(0, query.Length - 10);
            }
            _dt = PVCCCAF.AppCode.Global.GetData_New(query, CommandType.Text, _plist, _vlist);
            return _dt;

        }


        private DataTable GetPVDATA_FORAPPROVALWISE()
        {
            DataTable _dt = new DataTable();
            DataTable _userlevel = getUserHierarchyLevel();
            string query = string.Empty;
            string DataAreaid = Session["DATAAREAID"].ToString();
            string sessionusercode = Session["USERCODE"].ToString();
            int isviewonly = 0;
            List<string> _plist = new List<string>();
            List<string> _vlist = new List<string>();
            if (_userlevel != null && _userlevel.Rows.Count > 0)
            {
                _userlevel = _userlevel.DefaultView.ToTable(true, "USERID", "LEVELID");
                foreach (DataRow _dtrow in _userlevel.Rows)
                {
                    string a = @"'<a href=""#"" onclick=""SendDataApproval('''+pvc.PVCNO + ''',''' +convert(nvarchar,pvc.STATUS )+ ''','''+pvc.SubmittedBy + ''')"" >'+pvc.PVCNO+'</a>'";

                    string usercode = _dtrow["USERID"].ToString();
                    isviewonly = usercode == sessionusercode ? 1 : 0;

                    if (Convert.ToInt32(_dtrow["LEVELID"]) == 0)
                    {

                        //query += "SELECT  " + isviewonly + " ISVIEWONLY, " + a + " PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE PVC.DATAAREAID = '" + DataAreaid + "' AND " + Environment.NewLine;//29-08-22
                        query += "SELECT  " + isviewonly + " ISVIEWONLY, " + a + " PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE " + Environment.NewLine;
                        query += " PVC.STATUS IN(1,2,4,6) AND  PVC.SubmittedBy = '" + usercode + "' " + Environment.NewLine;
                        query += " UNION ALL ";
                    }
                    else if (Convert.ToInt32(_dtrow["LEVELID"]) == 1)
                    {
                        //query += "SELECT " + isviewonly + " ISVIEWONLY,CASE WHEN PVC.STATUS=1 THEN  " + a + " ELSE  pvc.PVCNO END PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE PVC.DATAAREAID = '" + DataAreaid + "' AND " + Environment.NewLine;//29-08-22
                        query += "SELECT " + isviewonly + " ISVIEWONLY,CASE WHEN PVC.STATUS=1 THEN  " + a + " ELSE  pvc.PVCNO END PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE " + Environment.NewLine;
                        query += "  PVC.STATUS IN(1) AND PVC.SubmittedBy = '" + usercode + "'" + Environment.NewLine;
                        query += " UNION ALL ";
                    }
                    else if (Convert.ToInt32(_dtrow["LEVELID"]) == 2)
                    {
                        //query += "SELECT " + isviewonly + " ISVIEWONLY, CASE WHEN PVC.STATUS=2 THEN  " + a + " ELSE  pvc.PVCNO END PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE PVC.DATAAREAID = '" + DataAreaid + "' AND " + Environment.NewLine;//29-08-22
                        query += "SELECT " + isviewonly + " ISVIEWONLY, CASE WHEN PVC.STATUS=2 THEN  " + a + " ELSE  pvc.PVCNO END PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE " + Environment.NewLine;
                        query += " PVC.STATUS IN(2) AND PVC.SubmittedBy = '" + usercode + "'" + Environment.NewLine;
                        query += " UNION ALL ";
                    }
                    else if (Convert.ToInt32(_dtrow["LEVELID"]) == 3)
                    {
                        //query += "SELECT " + isviewonly + " ISVIEWONLY,  CASE WHEN PVC.STATUS=4 THEN  " + a + " ELSE  pvc.PVCNO END PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE PVC.DATAAREAID = '" + DataAreaid + "' AND " + Environment.NewLine;//29-08-22
                        query += "SELECT " + isviewonly + " ISVIEWONLY,  CASE WHEN PVC.STATUS=4 THEN  " + a + " ELSE  pvc.PVCNO END PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE " + Environment.NewLine;
                        query += "  PVC.STATUS IN(4) AND PVC.SubmittedBy = '" + usercode + "'" + Environment.NewLine;
                        query += " UNION ALL ";
                    }
                    else if (Convert.ToInt32(_dtrow["LEVELID"]) == 4)
                    {
                        //query += "SELECT  " + isviewonly + " ISVIEWONLY, CASE WHEN PVC.STATUS=6 THEN  " + a + " ELSE  pvc.PVCNO END PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE PVC.DATAAREAID = '" + DataAreaid + "' AND " + Environment.NewLine;//29-08-22
                        query += "SELECT  " + isviewonly + " ISVIEWONLY, CASE WHEN PVC.STATUS=6 THEN  " + a + " ELSE  pvc.PVCNO END PVCLINK,* FROM VW_GETPVCHISTORY PVC WHERE " + Environment.NewLine;
                        query += "  PVC.STATUS IN(6) AND PVC.SubmittedBy = '" + usercode + "'" + Environment.NewLine;
                        query += " UNION ALL ";
                    }

                }

            }
            if (query.Length > 0)
            {
                query = query.Substring(0, query.Length - 10);
            }
            _dt = PVCCCAF.AppCode.Global.GetData_New(query, CommandType.Text, _plist, _vlist);
            return _dt;

        }


        #endregion

        #region"PVC Form View"
        public ActionResult PVCDataView(string PVCNO, string Pvstatus,string DataView="0",string Createduser="")
        {
            PVFormViewModel _pvformmodel = new PVFormViewModel();
            if (!string.IsNullOrEmpty(PVCNO))
            {
                ViewBag.PVCNUMSEQ = PVCNO;
                if (DataView == "6")
                {
                    
                   _pvformmodel = GetSavedPVDataView(PVCNO, Pvstatus,"1");//edit released
                    _pvformmodel.PVHeader.ShowExtendDateButton = false;
                }
                else
                {
                    _pvformmodel = GetSavedPVDataView(PVCNO, Pvstatus, "0");// other data
                }

                DataTable _userlevel = getUserHierarchyLevel();
                foreach (DataRow _dtrow in _userlevel.Rows)
                {
                    if (Convert.ToInt32(_dtrow["LEVELID"]) == 0)
                    {
                        if (_dtrow["USERID"].ToString() == Createduser && DataView == "6")
                        {
                            _pvformmodel.PVHeader.ShowExtendDateButton = true;
                        }
                        else
                        { _pvformmodel.PVHeader.ShowExtendDateButton = false;  
                        }
                        _pvformmodel.PVHeader.ShowCopyButton = true;
                    }
                    else
                    {
                        _pvformmodel.PVHeader.ShowCopyButton = false;
                        _pvformmodel.PVHeader.ShowExtendDateButton = false;
                    }
                }

            }
            return View(_pvformmodel);
        }

        #endregion


    }
}