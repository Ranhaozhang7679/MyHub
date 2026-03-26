#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       JSONParse
* 机器名称:       L05123-02
* 命名空间:       Luster.Module.Motion.DataProc.Functions
* 文 件 名:       JSONParse.cs
* 创建时间:       2022/12/19 9:14:15
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      014322f8-0991-4c7c-9f84-5f7f64d54007
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/19 9:14:15
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.DataProc.Functions
{
    /// <summary>
    /// 解析Json格式字符粗
    /// </summary>
    public class JSONParse : MotionFunction
    {
        [NotEmpty]
        [Parameter("需要进行解析的格式字符串", 0, CN = "JSON字符", CanRef = ParamRef.Ref)]
        public string JSONStr { get; set; }

        /// <summary>
        /// 原始工站
        /// </summary>
        [NotEmpty]
        [Parameter("要从JSON字符中提取的关键字", 1, CN = "关键字", CanRef = ParamRef.None)]
        public LArray<string> JSONKeys { get; set; }

        /// <summary>
        /// 数据转移
        /// </summary>
        public JSONParse()
        {
            this.Tips = "解析JSON格式字符串,JSON如果嵌套，关键字AA.BB";
            this.Icon = "\xe6bc";
            this.DynParam = true;
        }

        /// <summary>
        /// 将数据转移到任意工站中
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            var jsonDict = Luster.Common.Tools.JsonTool.ToDict(JSONStr);

            foreach (var item in JSONKeys)
            {
                string key = GetKey(item);
                if (MyOwner.Parameters.ContainsKey(key))
                {
                    if (jsonDict.ContainsKey(item))
                    {
                        MyOwner.Parameters[key].Value = jsonDict[item];
                    }
                }
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 获取key
        /// </summary>
        /// <param name="jsonKey"></param>
        /// <returns></returns>
        private string GetKey(string jsonKey)
        {
            return jsonKey.Replace(".", "_").Replace(":", "_");
        }

        /// <summary>
        /// 属性变更通知
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="newV"></param>
        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == nameof(JSONKeys))
            {
                LArray<string> keys = newV as LArray<string>;
                if (keys != null && keys.Count > 0)
                {
                    // 解析关键字、动态创建输出
                    int sort = 10;
                    foreach (var item in keys)
                    {
                        string key = GetKey(item);
                        BuildExternParameter(key, key, sort, typeof(string), p =>
                        {
                            p.ParamType = TaskFlow.Common.Enums.ParamType.OUT;
                        });

                        sort++;
                    }
                }
            }
        }
    }
}