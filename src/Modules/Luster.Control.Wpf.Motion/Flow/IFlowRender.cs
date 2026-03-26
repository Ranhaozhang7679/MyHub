#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 接口名称:       IFlowRender
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.Flow
* 文 件 名:       IFlowRender.cs
* 创建时间:       2022/5/24 11:41:57
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      7c1ba10f-e777-4980-a965-d35c21112328
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/24 11:41:57
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Luster.Control.Wpf.Motion.Flow
{
    /// <summary>
    /// 流程渲染引擎
    /// </summary>
    public interface IFlowRender
    {
        /// <summary>
        /// 内触发渲染事件
        /// </summary>
        event Action InvalidateEvent;

        Dispatcher Dispatcher { get; set; }

        /// <summary>
        /// 前一个render
        /// </summary>
        object PrevRender { get; }

        /// <summary>
        /// ID
        /// </summary>
        Guid ID { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 真实对象
        /// </summary>
        object Tag { get; set; }

        /// <summary>
        /// 尺寸
        /// </summary>
        Size Size { get; set; }

        /// <summary>
        /// 所属行
        /// </summary>
        int Row { get; set; }

        /// <summary>
        /// 所属列
        /// </summary>
        int Column { get; set; }
        /// <summary>
        /// 是否选中
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        Point Location { get; }

        /// <summary>
        /// 对象的最小外接矩形
        /// </summary>
        Rect Rect { get; }

        /// <summary>
        /// 外接矩形
        /// </summary>
        Rect OuterRect { get; set; }

        /// <summary>
        /// 索引信息
        /// </summary>
        int Index { get; }

        /// <summary>
        /// 隶属的矩形
        /// </summary>
        Rectangle MyRectangle { get; set; }

        int OffsetX { get; set; }

        int OffsetY { get; set; }


        /// <summary>
        /// 绘制对象
        /// </summary>
        /// <param name="drawingContext"></param>
        /// <param name="renderW">渲染尺寸</param>
        void Render(DrawingContext drawingContext, Rect rect);

        /// <summary>
        /// 判断当前是否被选中
        /// </summary>
        /// <param name="point">点位置</param>
        /// <returns>是否选中</returns>
        bool IsHitItem(System.Windows.Point point);

        /// <summary>
        /// 创建连接线
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        bool CreateNewLink(Point pos);

        /// <summary>
        /// 是否和矩形有相交
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        bool IsHitItem(Rect rect);

        /// <summary>
        /// 缩放比例
        /// </summary>
        /// <param name="scaleDelta"></param>
        void ScaleBy(double scaleDelta);

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="offsetX">x轴移动</param>
        /// <param name="offsetY">y轴移动</param>
        void Move(double offsetX, double offsetY);

        /// <summary>
        /// 选择
        /// </summary>
        void Select(bool isSelected = true);

        /// <summary>
        /// 下部位置
        /// </summary>
        /// <returns></returns>
        Point GetBottomPos();

        /// <summary>
        /// 上部位置
        /// </summary>
        /// <returns></returns>
        Point GetTopPos();

        /// <summary>
        /// 左部位置
        /// </summary>
        /// <returns></returns>
        Point GetLeftPos();

        /// <summary>
        /// 右部位置
        /// </summary>
        /// <returns></returns>
        Point GetRightPos();

        /// <summary>
        /// 获取对应的位置
        /// </summary>
        /// <returns></returns>
        Tuple<int, int> GetRowColumn();

        /// <summary>
        /// 获取当前工站之前的数据
        /// </summary>
        /// <returns></returns>
        List<Guid> GetPrevItems();

        /// <summary>
        /// 更新位置信息
        /// </summary>
        /// <param name="row">隶属行</param>
        /// <param name="col">隶属列</param>
        void SetRowColumn(int row, int col);

        /// <summary>
        /// 设置下一个连接点
        /// </summary>
        /// <param name="flowItem"></param>
        bool AddNextItem(IFlowRender flowItem);
        /// <summary>
        /// 获取提示信息
        /// </summary>
        /// <returns></returns>
        string GetTipMessage();

        /// <summary>
        /// 鼠标悬浮交互
        /// </summary>
        /// <param name="mousePos"></param>
        void Hover(Point mousePos);

        /// <summary>
        /// 开始动画
        /// </summary>
        void BeginStory();

        /// <summary>
        /// 结束动画
        /// </summary>
        void StopStory();
    }
}