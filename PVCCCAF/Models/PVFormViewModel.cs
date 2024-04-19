using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace PVCCCAF.Models
{
    public class PVFormViewModel
    {
        public PVFHeader PVHeader { get; set; } = new PVFHeader();
        public List<PVFLine> PVLine { get; set; } = new List<PVFLine>();

        public ShowSyncButton _syncbutton = new ShowSyncButton();
    }
    public class PVFHeader
    {
        [Display(Name = "Entity")]
        public string Entity { get; set; }

        [Display(Name = "Customer Applicable")]
        public string CustomerApplicable { get; set; }

        [Display(Name = "Account Selection")]
        public string AccountSelection { get; set; }

       public string Branch { get; set; }

        [Display(Name = "Price Tier")]
        public string PriceTier { get; set; }
        [Display(Name = "Description")]
        public string Description { get; set; }

        public int HeaderStatus { get; set; }
        public bool ISCHANGEREQUEST { get; set; }
        public bool ISPOSTPRICE { get; set;}

        public bool ISEDITAFTERAXSYNC { get; set; }
        public string PVCNO { get; set; }
        public string firstApproverComment { get; set; }
        public string secondApproverComment { get; set; }
        public string thirdApprovercomment { get; set; }

        public List<PVIMAGES> PvImages = new List<PVIMAGES>();
        public int CUSTAPPLICABLETYPEID { get; set; }
        public string ENTITYID { get; set; }
        public string TIERID { get; set; }
        public string ACCOUNTSELECTIONID { get; set; } = "";
        public decimal BRANCHID { get; set; }
        public string BranchWithVat { get; set; }
        public bool ShowCopyButton { get; set; } = false;

        public bool ShowExtendDateButton { get; set; } = true;

    }

    public class PVFLine
    {

        public Int64 No { get; set; }
        public string ItemType { get; set; }
        public string ItemTypeDesc { get; set; }
        public string ItemSelection { get; set; }
        public string ItemName { get; set; }
        public decimal CurrentPriceVEP { get; set; }
        public decimal NewPriceVEP { get; set; }
        public decimal NewPriceVIP { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountPer { get; set; }
        public Int64 ExpectedVolumn { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int LineStatus { get; set; }
        public int ISAPPROVE { get; set; }
        public string LINESTATUSDESC { get; set; }
    }

    public class PVIMAGES
    {
        public string ImageUrl { get; set; }
        public string PVCIMAGENAME { get; set; }
    }
    public class PVDateParm
    {
        [Display(Name = "Start Date")]
        [Required]
        public string StartDate { get; set; }

        [Display(Name = "End Date")]
        [Required]
        public string EndDate { get; set; }
    }

    public class PVFRELEASEDATA
    {

        public string DataAreaId { get; set; }
        public string PVNo { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string PriceTier { get; set; }
        public string AccountCode { get; set; }
        public string AccountSelection { get; set; }
        public string ItemType { get; set; }
        public string ItemCode { get; set; }
        public decimal NewPriceVIP { get; set; }
        public string Description { get; set; }
        public int LineNo { get; set; }
    }

    public class ShowSyncButton
    {
        public bool ShowAccountSelectionbtn { get; set; } = false;
        public bool ShowItemSelectionbtn { get; set; } = false;
    }
}