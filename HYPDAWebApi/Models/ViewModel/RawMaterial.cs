using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HYPDAWebApi.Models.ViewModel
{
    public class RawMaterial
    {
        // string sTxnDiv, string sLotID, string sITNBR, string sITDSC, string sTxnEmpNo, string sTxnQty, string sFacDiv, string sBackEmpNo, string sBackDepNo, string sUnit, bool bAllQty
        public string sTxnDiv { get; set; }
       
        public string sLotID { get; set; }

        public string sITNBR { get; set; }

        public string sITDSC { get; set; }
        public string sTxnEmpNo { get; set; }
        public string sTxnQty { get; set; }
        public string sFacDiv { get; set; }
        public string sBackEmpNo { get; set; }
        public string sBackDepNo { get; set; }
        public string sUnit { get; set; }
        public bool bAllQty { get; set; }
       


    }
}