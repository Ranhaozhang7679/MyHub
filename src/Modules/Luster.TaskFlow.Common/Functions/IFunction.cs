#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IFunction
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Module
* 文 件 名:       IFunction
* 创建时间:       2021/10/29 18:06:43
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      8594dfbc-2b50-458b-93da-dd84d578c63e
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/29 18:06:43
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Module;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Common.DataStruct.DataModels;

namespace Luster.TaskFlow.Common.Functions
{
    public interface IFunction : IDisposable, INameInfo, IXMLParser
    {
        /// <summary>
        /// 隶属于模块
        /// </summary>
        IModule Owner { get; set; }

        /// <summary>
        /// 隶属组
        /// </summary>
        string Group { get; }

        /// <summary>
        /// 对应的图标
        /// </summary>
        string GroupIcon { get; }

        /// <summary>
        /// 是否可见
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// 是否要显示进度体条,针对耗时操作
        /// </summary>
        bool IsProgressing { get; set; }

        /// <summary>
        /// 是否有动画
        /// </summary>
        bool Animation { get; set; }

        /// <summary>
        /// 动态参数
        /// </summary>
        bool DynParam { get; set; }

        /// <summary>
        /// 当前状态
        /// </summary>
        LStatus Status { get; set; }

        /// <summary>
        /// 预处理
        /// </summary>
        void PrevExcute();

        /// <summary>
        /// 函数执行
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        bool DoExcute(out string errMsg);

        /// <summary>
        /// 属性回调
        /// </summary>
        void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV);


        /// <summary>
        /// 界面绘制
        /// </summary>
        /// <param name="window"></param>
        /// <param name="actorColor">颜色</param>
        bool Draw(IInteractor window, string actorColor = "");

        /// <summary>
        /// 设置渲染对象是否可见
        /// </summary>
        /// <param name="window"></param>
        /// <param name="isVisible"></param>
        void SetDrawVisible(IInteractor window, bool isVisible);

        /// <summary>
        /// 执行动画效果
        /// </summary>
        /// <param name="animationFunc">动画完成回调</param>
        void DoAnimation(Action<bool> animationFunc);
    }
}