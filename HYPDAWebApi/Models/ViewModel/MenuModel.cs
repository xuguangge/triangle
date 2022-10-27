using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HYPDAWebApi.Models.ViewModel
{
    public class MenuModel
    {
        private List<MenuModel> _childModels = new List<MenuModel>();
        //private Meta _Meta = new Meta();
        /// <summary>
        /// 菜单ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 父菜单ID
        /// </summary>
        public string PARENT_ID { get; set; }
        /// <summary>
        /// 菜单编码
        /// </summary>
        public string CODE { get; set; }
        /// <summary>
        /// 菜单名称
        /// </summary>
        public string NAME { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public string SORT_NO { get; set; }
        

        /// <summary>
        /// 子菜单列表
        /// </summary>
        public List<MenuModel> children
        {
            get { return _childModels; }

            set { _childModels = value; }

        }

        //public Meta meta
        //{
        //    get { return _Meta; }

        //    set { _Meta = value; }

        //}
    }
    //public class Meta
    //{
    //    private bool _noCache = false;
    //    /// <summary>
    //    ///  will control the page roles
    //    /// </summary>
    //    public string[] roles { get; set; }
    //    /// <summary>
    //    /// 模块名称
    //    /// </summary>
    //    public string title { get; set; }
    //    /// <summary>
    //    /// 功能图标
    //    /// </summary>
    //    public string icon { get; set; }
    //    /// <summary>
    //    /// the page will no be cached
    //    /// </summary>
    //    public bool noCache
    //    {
    //        get { return _noCache; }

    //        set { _noCache = value; }

    //    }
    //    public string pageCode { get; set; }
    //}
}