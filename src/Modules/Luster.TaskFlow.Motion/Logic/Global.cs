#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Global
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Logic
* 文 件 名:       Global.cs
* 创建时间:       2022/6/29 18:33:04
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      bd5f5fed-aefd-46e3-a67d-4ecbdf9f1c29
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/29 18:33:04
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Logics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

namespace Luster.TaskFlow.Motion.Logic
{
    /// <summary>
    /// 全局模块
    /// </summary>
    public class GlobalModule : MotionModule, ILogic, IGlobal
    {
        public static Guid GlobalID = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        /// <summary>
        /// 记忆型全局变量存储路径
        /// </summary>
        private readonly static string DirectoryName = "D:\\Luster\\Var\\";

        private readonly static string FileName = "GlobalVar.Xml";

        public GlobalModule() : base()
        {
            // Root节点的ID为空
            ID = GlobalID;
            Name = "Global";
            this.Icon = "\xe64e";
        }

        public override void InitFunctions()
        {
            AddFunction<Global>();
        }

        public override XElement ExportXml()
        {
            XElement xGlobal = new XElement("Global");
            foreach (var item in Parameters)
            {
                xGlobal.Add(item.Value.ExportXml());
            }
            ///单独保存记忆型
            WriteGlobalToXml();
            return xGlobal;
        }

        /// <summary>
        /// 单独存入另一个文件
        /// </summary>
        public  void WriteGlobalToXml()
        {
            bool ContainMomericGlobalVar = Parameters.Values.Any(x => x.IsMemoric);
            if (!ContainMomericGlobalVar)
            {
                return;
            }

            try
            {
                if (!Directory.Exists(DirectoryName))
                {
                    Directory.CreateDirectory(DirectoryName);
                }

                if (!File.Exists(DirectoryName + FileName))
                {
                    File.Create(DirectoryName + FileName);
                }
                XElement xGlobal = new XElement("Global");
                foreach (var item in Parameters)
                {
                    if (item.Value.IsMemoric)
                    {
                        xGlobal.Add(item.Value.ExportXml());
                    }
                }
                xGlobal.Save(DirectoryName + FileName);
            }
            catch(IOException iex)
            {
                ///防止多线程同时操作文件
            }
            catch (Exception ex) 
            {
               ///防止多线程同时操作文件
            }
            finally 
            {
            
            }
        } 

        public override void ParserXml(XElement xElement)
        {
            ///常规读取
            var xParam = xElement.Elements();
            foreach (XElement xItem in xParam)
            {
                ParameterAttribute parameter = new ParameterAttribute("", 0);
                parameter.Owner = this;
                parameter.ParserXml(xItem);

                if (!Parameters.ContainsKey(parameter.Name))
                {
                    Parameters.Add(parameter.Name, parameter);
                }
            }
            ///记忆型文件单独读取
            ReadGlobalFromXml();
        }

        public void ReadGlobalFromXml()
        {
            if (!Directory.Exists(DirectoryName))
            {
                return;
            }

            if (!File.Exists(DirectoryName + FileName))
            {
                return;
            }
            try
            {
                XElement xElement = XElement.Load(DirectoryName + FileName);
                ///防止文件里面没有内容
                if (xElement == null)
                {
                    return;
                }
                var xParam = xElement.Elements();
                foreach (XElement xItem in xParam)
                {
                    ParameterAttribute parameter = new ParameterAttribute("", 0);
                    parameter.Owner = this;
                    parameter.ParserXml(xItem);

                    if (!Parameters.ContainsKey(parameter.Name))
                    {
                        ///防止因为文件没有删除，而出现记忆型变量在配方里不存在的情况
                        continue;
                    }
                    else
                    {
                        Parameters.Remove(parameter.Name);
                    }
                    Parameters.Add(parameter.Name, parameter);
                }
            }
            catch (Exception ex)
            {


            }
            }

    }

    /// <summary>
    /// 
    /// </summary>
    public class Global : MotionFunction
    {
        public Global()
        {
            this.IsVisible = false;
            DynParam = true;
            this.Icon = "\xe627";
        }
    }
}