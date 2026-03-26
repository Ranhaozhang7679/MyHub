#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       WebClient
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Protocol.Functions
* 文 件 名:       WebClient.cs
* 创建时间:       2022/9/14 11:27:09
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      38380eb4-be84-421a-b85c-10779a42e7b6
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/14 11:27:09
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.Tools;
using Luster.Common.Tools.Tools;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Protocol.Functions
{

    public enum HttpMethod
    {
        [Description("Get")]
        Get,

        [Description("Post")]
        Post
    }

    /// <summary>
    /// WebHttp 通信
    /// </summary>
    public class WebHttp : MotionFunction, IPauseFunction
    {
        [NotEmpty]
        [Parameter("Http的请求地址", 0, CN = "URL地址")]
        public string Url { get; set; }

        [Parameter("启用", 1, CN = "启用", CanRef = ParamRef.Ref)]
        public bool IsEnable { get; set; }

        [Parameter("Http的请求方式", 1, CN = "请求方式", DefaultV = HttpMethod.Get)]
        public HttpMethod Method { get; set; }

        [Parameter("Url参数", 2, CN = "Url参数")]
        public LStringEx UrlParam { get; set; }

        [DependOn("Method", HttpMethod.Post)]
        [Parameter("Post对应的JSON数据", 3, CN = "JsonData")]
        public LStringEx JsonData { get; set; }

        [Parameter("Http的Body类型", 4, CN = "MIME类型", DefaultV = "application/json", Datas = new string[] { "text/html", "application/json", "application/x-www-form-urlencoded", "multipart/form-data", "application/xml" })]
        public string ContentType { get; set; }


        [Parameter("正则表达式解析", 5, CN = "正则表达式")]
        public string Pattern { get; set; }


        [Parameter("通信结果", 6, CN = "请求结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string OutString { get; set; }

        /// <summary>
        /// 请求结果
        /// </summary>
        public WebHttp()
        {
            this.Tips = "支持Web Http请求";
            this.Icon = "\xe6a8";
        }

        /// <summary>
        /// web 通信组件
        /// </summary>
        private WebTool webTool;


        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            if (IsEmptyMode || !IsEnable)
            {
                OutString = nameof(IsEmptyMode);
                return true;
            }

            if (webTool == null)
            {
                webTool = new WebTool() { Encoding = Encoding.UTF8 };
            }

            string url = Url;
            string urlParam = UrlParam?.GetString(Owner);

            if (!string.IsNullOrEmpty(urlParam))
            {
                url = $"{url}?{urlParam}";
            }

            // url需要经过去除换行符
            url = url.Replace("\r", "").Replace("\n", "");

            if (Method == HttpMethod.Post)
            {
                string postData = "";
                if (JsonData != null)
                {
                    postData = JsonData.GetString(Owner);
                }

                OutString = webTool.Post(url, postData, ContentType);
            }
            else
            {
                OutString = webTool.GetHtml(url);
            }

            // 如果有正则表达式
            if (!string.IsNullOrEmpty(Pattern))
            {
                if (RegexTool.IsMatch(OutString, Pattern))
                {
                    OutString = RegexTool.GetValue(OutString, Pattern);
                }
                else
                {
                    OutString = "NG";
                }
            }

            return base.DoExcute(out errMsg);
        }
    }
}