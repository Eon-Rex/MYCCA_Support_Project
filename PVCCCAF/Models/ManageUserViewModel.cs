using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PVCCCAF.Models
{
    public class ManageUserViewModel
    {
        public string myTextBox { get; set; }
        public List<ManageUserModel> dealerlist { get; set; }
    }

    public class ManageUserModel
    {
        public string SITEID { get; set; }
        public string NAME { get; set; }
        public string ROLECODE { get; set; }
        public string MOBILE { get; set; }
        public string ADDRESS { get; set; }
        public string EMAIL { get; set; }
        public string PASSWORD { get; set; }
        public string AuthorizationMode { get; set; }
        public string BLOCKED { get; set; }
        public string STATUS { get; set; }
        public string ROLE { get; set; }
        public string USERTYPE { get; set; }
    }
}