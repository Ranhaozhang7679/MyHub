#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IWindow
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Interfaces
* 文 件 名:       IWindow
* 创建时间:       2021/10/30 10:40:19
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      4e824c17-1847-4d66-9f2c-c3b864477ab4
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/30 10:40:19
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.Interfaces
{
    /// <summary>
    /// 窗口渲染
    /// </summary>
    public interface IInteractor : IDisposable
    {
        /// <summary>
        /// 多语言
        /// </summary>
        string Lang { get; set; }

        /// <summary>
        /// 清除渲染对象
        /// </summary>
        /// <param name="actorID">基于标识</param>
        void RemoveActor(Guid actorID);

        /// <summary>
        /// 清除所有Actor
        /// </summary>
        void RemoveAll();

        /// <summary>
        /// 重新绘制
        /// </summary>
        void Render(bool restCamera = false);

        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="actorID">actor的ID</param>
        /// <param name="r">红色</param>
        /// <param name="g">绿色</param>
        /// <param name="b">蓝色</param>
        /// <param name="a">透明度</param>
        void SetActorColor(Guid actorID, byte r, byte g, byte b, byte a);

        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="actorID">actor的ID</param>
        /// <param name="actorColor">红色,绿色,蓝色,透明度</param>
        void SetActorColor(Guid actorID, string actorColor);

        /// <summary>
        /// 元素高亮显示
        /// </summary>
        /// <param name="actorID">几何高亮</param>
        void HighLight(Guid actorID);

        /// <summary>
        /// 显示文本可见性
        /// </summary>
        /// <param name="id">隶属模型ID</param>
        /// <param name="text">文本信息</param>
        /// <param name="visible">可见性</param>
        void SetTextVisible(Guid id, string text, bool visible, LTolerance tolerance = null);

        /// <summary>
        /// 设置Actor的隐藏或显示
        /// </summary>
        /// <param name="actorID">actor的ID</param>
        /// <param name="visible">可见/隐藏</param>
        void SetActorVisible(bool isVisible, params Guid[] actorIDs);

        /// <summary>
        /// 支持字符串
        /// </summary>
        /// <param name="visible"></param>
        /// <param name="actorIDs"></param>
        void SetActorVisible(bool visible, params string[] actorIDs);

        /// <summary>
        /// 设置矩阵
        /// </summary>
        /// <param name="actorID">actor的ID</param>
        /// <param name="matrix">矩阵</param>
        void SetActorMatrix(Guid actorID, double[] matrix);
    }
}