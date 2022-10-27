using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HYPDAWebApi.Models.ViewModel
{
    public class Regist
    {
        /// <summary>
        /// string sBarcode, string sModID, string sTaoID, string sQuanID,
        /// string sDIV, string sCUTOTIM, string sWKCOD, string sHEMOLI,
        /// string sPLANID, string sDNAM, string sREMARK, string[] sCheckItems
        /// </summary>
        public string sBarcode { get; set; }
      
        public string sModID { get; set; }
        public string sTaoID { get; set; }
        public string sQuanID { get; set; }
        public string sDIV { get; set; }
        public string sCUTOTIM { get; set; }
        public string sWKCOD { get; set; }
        public string sHEMOLI { get; set; }
        public string sPLANID { get; set; }
        public string sDNAM { get; set; }
        public string sREMARK { get; set; }
        public string[] sCheckItems { get; set; }



    }
}