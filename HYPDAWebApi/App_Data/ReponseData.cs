using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HYPDAWebApi.App_Data
{
    public class ReponseData
    {
        public string code { get; set; }
        public string msg { get; set; }
        public object data { get; set; }
        public Exception ex { get; set; }
    }
}
