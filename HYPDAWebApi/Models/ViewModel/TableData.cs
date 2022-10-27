using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HYPDAWebApi.Models.ViewModel
{
    public class TableData
    {
       
        /// <param name="intent">需要获取的表字段名</param>
        public string[] intent { get; set; }
        /// <param name="table">表名</param>
        public string table { get; set; }
        /// <param name="ColNam">WHERE条件的字段名</param>
        public string[] ColNam { get; set; }
        /// <param name="content">WHERE对应的字段内容</param>
        public string[] content { get; set; }
        /// <param name="sort">需要ORDER BY的字段</param>
        public string[] sort { get; set; }
    }
}