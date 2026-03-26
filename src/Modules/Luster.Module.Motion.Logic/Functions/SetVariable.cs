#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SetVariable
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Logic.Functions
* 文 件 名:       SetVariable.cs
* 创建时间:       2022/7/5 16:55:47
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      b37c0679-6f0a-44e5-9faa-1ecec25933ce
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/5 16:55:47
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Logic.Functions
{
    public enum GlobalType
    {
        /// <summary>
        /// bool类型
        /// </summary>
        Bool,

        /// <summary>
        /// 字符串
        /// </summary>
        String,


        Int,

        /// <summary>
        /// double
        /// </summary>
        Double,
    }

    public class SetVariable : MotionFunction, IRefFunction
    {
        [NotEmpty]
        [Parameter("全局变量", 1, CN = "全局变量", EditorType = typeof(IGlobal))]
        public string GlobalVar { get; set; }

        [Parameter("变量类型", 2, CN = "变量类型", DefaultV = GlobalType.Bool)]
        public GlobalType GlabalType { get; set; }

        [DependOn("GlabalType", GlobalType.Bool)]
        [Parameter("全局变量变量值", 3, CN = "变量值", CanRef = ParamRef.Ref)]
        public bool GBoolVal { get; set; }

        [DependOn("GlabalType", GlobalType.String)]
        [Parameter("全局变量变量值", 4, CN = "变量值", CanRef = ParamRef.Ref, DefaultV = "")]
        public string GStringVal { get; set; }



        [DependOn("GlabalType", GlobalType.Double)]
        [Parameter("全局变量变量值", 6, CN = "变量值", CanRef = ParamRef.Ref)]
        public double GDoubleVal { get; set; }

        [DependOn("GlabalType", GlobalType.Int)]
        [Parameter("全局变量变量值", 7, CN = "变量值", CanRef = ParamRef.Ref)]
        public int GIntVal { get; set; }

        [DependOn("GlabalType", GlobalType.String)]
        [Parameter("将当前工站SN配置到全局变量中", 5, CN = "设置SNTo全局变量", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsSetSNToValue { get; set; }

        [DependOn("GlabalType", GlobalType.String)]
        [Parameter("将当前全局变量配置到工站SN中", 6, CN = "设置全局变量ToSN", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsSetValueToSN { get; set; }

        [DependOn("GlabalType", GlobalType.String)]
        [Parameter("将当前模块的SN取走", 7, CN = "取走SN", DefaultV = false)]
        public bool IsGetSN { get; set; }

        [DependOn("GlabalType", GlobalType.String)]
        [DependOn("GlabalType", GlobalType.Double)]
        [Parameter("异常赋值", 8, CN = "异常赋值", DefaultV = false)]
        public bool IsForceVal { get; set; }


        [DependOn("IsForceVal", true)]
        [Parameter("标准值", 9, CN = "标准值",DefaultV =0)]
        public double ForceVal { get; set; }

        [DependOn("IsForceVal", true)]
        [Parameter("公差", 10, CN = "公差", DefaultV = 0)]
        public double ForceTorlence { get; set; }




        /// <summary>
        /// 注释信息
        /// </summary>
        public override string[] NoteParams => new string[] { "设置", nameof(GlobalVar) };

        /// <summary>
        /// 全局变量
        /// </summary>
        //private IMotionModule gModule = null;

        /// <summary>
        /// 参数
        /// </summary>
        //private ParameterAttribute gParameter = null;
        /// <summary>
        /// 构造函数
        /// </summary>
        public SetVariable()
        {
            this.Tips = "更新全局变量";
            this.Icon = "\xe6a4";
        }

        /// <summary>
        /// 防止多线程更新，造成变量更新异常的BUG
        /// </summary>
        private static object lockObj = new object();

        private int Index = 0;


        Random random = new Random();

        /// <summary>
        /// 函数处理
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";

            //lock (lockObj)
            //{

            IMotionModule gModule = MyOwner.TaskModules[GlobalModule.GlobalID] as IMotionModule;

            if (!gModule.Parameters.ContainsKey(GlobalVar))
            {
                errMsg = $"全局变量:{GlobalVar}不存在!";
                return false;
            }

            ParameterAttribute gParameter = gModule.Parameters[GlobalVar];

            //if (gModule == null)
            //{
            //    gModule = MyOwner.TaskModules[GlobalModule.GlobalID] as IMotionModule;
            //}

            //if (gParameter == null)
            //{
            //    if (!gModule.Parameters.ContainsKey(GlobalVar))
            //    {
            //        errMsg = $"全局变量:{GlobalVar}不存在!";
            //        return false;
            //    }

            //    gParameter = gModule.Parameters[GlobalVar];
            //}


            StringBuilder sb = new StringBuilder();
            sb.Append($"全局变量:{GlobalVar} src value={gParameter.Value} ");
            switch (GlabalType)
            {
                case GlobalType.Bool:
                    gParameter.Value = GBoolVal;
                    sb.Append($" new value={GBoolVal} actual value={gParameter.Value}");
                    break;
                case GlobalType.String:
                    gParameter.Value = GStringVal;
                    if( string.IsNullOrEmpty(gParameter.Value.ToString()))
                    {
                        if (IsForceVal)
                            gParameter.Value = NextDouble(random, ForceVal - ForceTorlence, ForceVal + ForceTorlence);
                    }

                    // 将工站SN设置到全局变量中
                    if (IsSetSNToValue)
                    {
                        gParameter.Value = MyOwner.DataID;

                    }
                    // 将全局变量设置到工站SN
                    if (IsSetValueToSN)
                    {
                        MyOwner.Station.DataID = gParameter.Value as string;
                    }
                    sb.Append($" new value={GStringVal} actual value={gParameter.Value}");

                    if (IsGetSN)
                    {
                        if (MyOwner.Station == null)
                        {
                            MyOwner.UpdateStation();
                        }

                        if (MyOwner.Station.TaskFunction is IStation s)
                        {
                            if (s.TryDequeue(out string dataID))
                            {
                                MyOwner.OnLog(Luster.Common.DataStruct.Enums.LogType.Debug, $"SN转移:原工站:{MyOwner.Alias}->目标站:{gParameter?.CN} 转移SN:{dataID}");
                            }
                        }
                    }

                    break;
                case GlobalType.Int:
                    gParameter.Value = GIntVal;
                    sb.Append($" new value={GIntVal} actual value={gParameter.Value}");
                    break;
                case GlobalType.Double:
                    gParameter.Value = GDoubleVal;
                    if ( string.IsNullOrEmpty(gParameter.Value.ToString()))
                    {
                        if (IsForceVal)
                            gParameter.Value = NextDouble(random, ForceVal - ForceTorlence, ForceVal + ForceTorlence);
                    }
                    sb.Append($" new value={GDoubleVal} actual value={gParameter.Value}");
                    break;
            }
            if (gParameter.IsMemoric&&Index<1)
            {
                lock (lockObj)
                {
                    ///增加原子锁,避免出现同时操作文件的bug
                    Interlocked.Increment(ref Index);
                    GlobalModule Module = MyOwner.TaskModules[GlobalModule.GlobalID] as GlobalModule;
                    Module.WriteGlobalToXml();
                    Interlocked.Decrement(ref Index);
                }
            }

            MyOwner.OnLog(Common.DataStruct.Enums.LogType.Debug, sb.ToString());
            //}

            return base.DoExcute(out errMsg);
        }
        
        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == "GlobalVar")
            {
                SetParameterVal(nameof(GlobalVar), newV);
                //gModule = null;
                //gParameter = null;
            }
        }

        /// <summary>
        /// 添加引用
        /// </summary>
        /// <returns></returns>
        public List<LReference> GetReferences()
        {
            List<LReference> lRefs = new List<LReference>();

            var paramVal = MyOwner.Parameters[nameof(GlobalVar)].Value;
            if (paramVal != null)
            {
                var refName = paramVal.ToString();
                lRefs.Add(new LReference()
                {
                    RefID = GlobalModule.GlobalID,
                    RefName = refName,
                    ByRefName = nameof(GlobalVar),
                });
            }


            return lRefs;
        }

        public static double NextDouble(Random random, double miniDouble, double maxiDouble)
        {
            if (random != null)
            {
                return Math.Round(random.NextDouble() * (maxiDouble - miniDouble) + miniDouble, 3);
            }
            else
            {
                return 0.0d;
            }
        }
    }
}