using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Luster.Module.Motion.Business.Functions
{
    /// <summary>
    /// 蓝思项目LP236MCH弹片位置高度量测Mes数据上传专用
    /// </summary>
    public class LSMesUnLoad : MotionFunction
    {

        [Parameter("服务地址", 0, CN = "服务地址", ParamType = TaskFlow.Common.Enums.ParamType.IN, CanRef = ParamRef.Ref,DefaultV = "http://mesnex.catcher-local.com.cn:56373/HttpHandler.ashx")]
        public string ServiceUrl { get; set; }


        [Parameter("上传数据", 0, CN = "数据", ParamType = TaskFlow.Common.Enums.ParamType.IN, CanRef = ParamRef.Ref)]
        public string Data { get; set; }

        [Limit(1, 10)]
        [Parameter("回复NG后，重复上传次数", 1, CN = "重复次数", DefaultV = 2)]
        public int LoopNum { get; set; }


        [Parameter("Mes回复数据", 2, CN = "回复数据", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string ReplyData { get; set; }

        public LSMesUnLoad()
        {
            this.Tips = "MCH弹片测量数据上传";
            this.Icon = "\xe6dc";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";

            for (int i = 0; i < LoopNum; i++)
            {
                //先根据用户请求的uri构造请求地址
                string serviceUrl = string.Format("{0}", ServiceUrl);
                //创建Web访问对象
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(serviceUrl);
                NameValueCollection data1 = System.Web.HttpUtility.ParseQueryString(String.Empty);
                data1.Add("command", "AssyService");
                data1.Add("payload", Data);
                //把用户传过来的数据转成“UTF-8”的字节流
                byte[] buf = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(data1.ToString());

                myRequest.Method = "POST";
                myRequest.ContentLength = buf.Length;
                myRequest.ContentType = "application/x-www-form-urlencoded";
                myRequest.MaximumAutomaticRedirections = 1;
                myRequest.AllowAutoRedirect = true;
                //发送请求
                Stream stream = myRequest.GetRequestStream();

                stream.Write(buf, 0, buf.Length);
                stream.Close();

                //获取接口返回值
                //通过Web访问对象获取响应内容
                HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
                //通过响应内容流创建StreamReader对象，因为StreamReader更高级更快
                StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                string returnXml = reader.ReadToEnd();
                MyOwner.OnLog(Common.DataStruct.Enums.LogType.Debug, $"MES回复,{returnXml}");
                string str = returnXml.Remove(0, 11);
                string[] strSeries = str.Split(',');

                if (0 == strSeries[0].CompareTo("true"))
                {
                    ReplyData = returnXml;
                    reader.Close();
                    myResponse.Close();
                    break;
                }
                else
                {
                    if (i == LoopNum - 1)
                    {
                        reader.Close();
                        myResponse.Close();
                        errMsg = "Mes已经上传但是Mes系统回复失败";
                        MyOwner.OnLog(Common.DataStruct.Enums.LogType.Error, errMsg);
                    }
                }               
            }

            return base.DoExcute(out errMsg);
        }


    }
}
