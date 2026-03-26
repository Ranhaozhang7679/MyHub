#region 作者和版本
/*----------------------------------------------------------------
 * 版权所有 (c) 2023 P R C  保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：D05149
 * 公司名称：P R C
 * 命名空间：Luster.Module.Motion.Logic.Functions
 * 唯一标识：f33ca07d-d1fa-4c57-bc2a-18691716304a
 * 文件名：ResetVariable
 * 当前用户域：LUSTERINC
 * 
 * 创建者：D05149
 * 电子邮箱：huidong@lusterinc.com
 * 创建时间：2023/2/1 13:11:10
 * 版本：V1.0.0
 * 描述：
 *
 * ----------------------------------------------------------------
 * 修改人：
 * 时间：
 * 修改说明：
 *
 * 版本：V1.0.1
 *----------------------------------------------------------------*/
#endregion 作者和版本

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
using System.Text;
using System.Threading.Tasks;


namespace Luster.Module.Motion.Logic.Functions
{
    public class ResetVariable : MotionFunction, IRefFunction
    {
        [NotEmpty]
        [Parameter("全局变量", 1, CN = "全局变量", EditorType = typeof(IGlobal))]
        public string GlobalVar { get; set; }

        [Parameter("变量类型", 2, CN = "变量类型", DefaultV = GlobalType.Bool)]
        public GlobalType GlabalType { get; set; }


        /// <summary>
        /// 注释信息
        /// </summary>
        public override string[] NoteParams => new string[] { "设置", nameof(GlobalVar) };

        /// <summary>
        /// 全局变量
        /// </summary>
        //private IMotionModule gModule = null;

        //private ParameterAttribute gParameter = null;

        //private static object lockObj = new object();

        /// <summary>
        /// 构造函数
        /// </summary>
        public ResetVariable()
        {
            this.Tips = "复位全局变量";
            this.Icon = "\xe6a4";
        }

        /// <summary>
        /// 函数处理
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";

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

            //lock (lockObj)
            //{
                gParameter.Value = gParameter.DefaultV;
                MyOwner.OnLog(Common.DataStruct.Enums.LogType.Debug, $"{MyOwner.Alias} 全局变量:{GlobalVar} 复位:{gParameter.Value}");
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
    }
}


