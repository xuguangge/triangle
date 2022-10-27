using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HYPDAWebApi.Models.ViewModel
{
    public class Record
    {
       // string sMchid, string sNam, string[] sLBarcodes, string[] sRBarcodes, string sDIV
        public string sMchid { get; set; }
       
        public string sNam { get; set; }

        public string sDIV { get; set; }

        public string[] sLBarcodes { get; set; }

        public string[] sRBarcodes { get; set; }


    }
}