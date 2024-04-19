using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Web;

namespace PVCCCAF.App_Code
{
    public class ExcelUpload
    {
        public static DataTable ImportExcelXLS(string FileName, bool hasHeaders)
        {
            string HDR = hasHeaders ? "Yes" : "No";
            string strConn;
            if (FileName.Substring(FileName.LastIndexOf('.')).ToLower() == ".xlsx")
                strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FileName + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=0\"";
            //strConn = "provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + FileName + ";Extended Properties=Excel 8.0;HDR=" + HDR + ";IMEX=0\"";
            else
                strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + FileName + ";Extended Properties=\"Excel 8.0;HDR=" + HDR + ";IMEX=0\"";


            List<SheetName> sheetNames = new List<SheetName>();
            DataTable output = new DataTable();

            using (OleDbConnection conn = new OleDbConnection(strConn))
            {
                conn.Open();

                DataTable schemaTable = conn.GetOleDbSchemaTable(
                    OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                string sheet = string.Empty;
                foreach (DataRow schemaRow in schemaTable.Rows)
                {
                    sheet = schemaRow["TABLE_NAME"].ToString();

                    if (!schemaRow["TABLE_NAME"].ToString().Contains("FilterDatabase"))
                    {
                        sheetNames.Add(new SheetName() { sheetName = schemaRow["TABLE_NAME"].ToString(), sheetType = schemaRow["TABLE_TYPE"].ToString(), sheetCatalog = schemaRow["TABLE_CATALOG"].ToString(), sheetSchema = schemaRow["TABLE_SCHEMA"].ToString() });
                    }

                }
                if (!sheet.EndsWith("_"))
                {
                    try
                    {
                        OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + sheetNames[0].sheetName + "]", conn);
                        cmd.CommandType = CommandType.Text;

                        output = new DataTable(sheetNames[0].sheetName);
                        new OleDbDataAdapter(cmd).Fill(output);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message + string.Format("Sheet:{0}.File:F{1}", sheet, FileName), ex);
                    }
                    finally
                    {
                        if (conn != null)
                        {
                            if (conn.State.ToString() == "Open") { conn.Close(); }
                        }
                    }
                }
            }
            return output;
        }
    }
    public class SheetName
    {
        public string sheetName { get; set; }
        public string sheetType { get; set; }
        public string sheetCatalog { get; set; }
        public string sheetSchema { get; set; }
    }
}