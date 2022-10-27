using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HYPDAWebApi.Models.ViewModel
{
    public class User
    {
        public string LoginID { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        public string DEPNAM { get; set; }
        ///用户名
        public string NAME { get; set; }
        /// <summary>
        /// 工号
        /// </summary>
        public string LOGINNAME { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        public string PHONE { get; set; }
        /// <summary>
        /// 当前PDA发布的最新版本
        /// </summary>
        public string VERSION { get; set; }


    }
}