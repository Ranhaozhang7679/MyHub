#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       UploadData
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Logic.Functions
* 文 件 名:       UploadData.cs
* 创建时间:       2022/9/14 21:41:20
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ce64b194-9c5c-4963-9999-53824f15b936
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/14 21:41:20
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Logic.Functions
{
    public class UploadData : MotionFunction
    {
        /// <summary>
        /// 产品码
        /// </summary>
        [NotEmpty]
        [Parameter("一般使用产品码", 0, CN = "数据ID", CanRef = ParamRef.Ref)]
        public string Code { get; set; }

        /// <summary>
        /// 支持动态参数
        /// </summary>
        public UploadData()
        {
            this.Tips = "上传数据,支持动态参数";
            this.Icon = "\xe6a9";
            this.DynParam = true;
        }

        /// <summary>
        /// 参数运行
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            List<LColumn> keyDatas = new List<LColumn>();
            foreach (var item in Owner.Parameters)
            {
                var p = item.Value;
                if (p.ParamType == TaskFlow.Common.Enums.ParamType.OUT) continue;

                if (item.Key == nameof(Code)) continue;

               
                if (p.RefOut != null)
                {
                    keyDatas.Add(new LColumn(p.RefOut));
                }
                else
                {
                    keyDatas.Add(new LColumn(p));
                }
            }

            MyOwner.OnDataUpload(Code, keyDatas);
            return base.DoExcute(out errMsg);
        }
    }
}