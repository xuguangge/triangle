using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HYPDAWebApi.Models.ViewModel
{
    public class CheckInfo
    {
        public string sBarcode { get; set; }
        public string sDiv { get; set; }
        public string sPLANID { get; set; }
        public string sNAM { get; set; }
        public string[] sCheckItems { get; set; }



    }
}