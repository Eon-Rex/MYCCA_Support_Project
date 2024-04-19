using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PVCCCAF.Models
{

    public partial class AuditTB
    {
        public Nullable<System.DateTime> UsersAuditID { get; set; }
        public string UserID { get; set; }
        public string SessionID { get; set; }
        public string IPAddress { get; set; }
        public string PageAccessed { get; set; }
        public Nullable<System.DateTime> LoggedInAt { get; set; }
        public Nullable<System.DateTime> LoggedOutAt { get; set; }
        public string LoginStatus { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
    }
}