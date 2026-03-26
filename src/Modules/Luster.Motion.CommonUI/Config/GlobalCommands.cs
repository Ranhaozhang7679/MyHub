#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       GlobalCommands
* 机器名称:       L05123-NB
* 命名空间:       Luster.SubSystem.ThreeD.Config
* 文 件 名:       GlobalCommands.cs
* 创建时间:       2021/11/23 9:07:31
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      565f0fd8-f542-4fe3-bb0e-67b5e4fec728
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/23 9:07:31
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI
{
    public static class GlobalCommands
    {
        /// <summary>
        /// 取消交互命令
        /// </summary>
        public static CompositeCommand SearchCommand = new CompositeCommand();

        /// <summary>
        /// 取消交互命令
        /// </summary>
        public static CompositeCommand CancelInteractiveCommand = new CompositeCommand();
        /// <summary>
        /// 全局保存命令
        /// </summary>
        public static CompositeCommand SaveCommand = new CompositeCommand();

        /// <summary>
        /// 全局新建项目命令
        /// </summary>
        public static CompositeCommand AddProjectCommand = new CompositeCommand();

        /// <summary>
        /// 全局打开项目命令
        /// </summary>
        public static CompositeCommand OpenProjectCommand = new CompositeCommand();

        /// <summary>
        /// 显示属性
        /// </summary>
        public static CompositeCommand ShowParameterCommand = new CompositeCommand();

        /// <summary>
        /// 全局忽略命令
        /// </summary>
        public static CompositeCommand SkipCommand = new CompositeCommand();

        /// <summary>
        /// 全局删除功能
        /// </summary>
        public static CompositeCommand DeleteCommand = new CompositeCommand();

        /// <summary>
        /// 运行
        /// </summary>
        public static CompositeCommand RunCommand = new CompositeCommand();

        /// <summary>
        /// 运行单步
        /// </summary>
        public static CompositeCommand RunOneCommand = new CompositeCommand();

        /// <summary>
        /// 运行下一步
        /// </summary>
        public static CompositeCommand RunNextCommand = new CompositeCommand();

        /// <summary>
        /// 循环运行
        /// </summary>
        public static CompositeCommand RunLoopCommand = new CompositeCommand();

        /// <summary>
        /// 模块查询
        /// </summary>
        public static CompositeCommand SearchModuleCommand = new CompositeCommand();

        /// <summary>
        /// 任务回退
        /// </summary>
        public static CompositeCommand BackCommand = new CompositeCommand();

        /// <summary>
        /// 任务前进
        /// </summary>
        public static CompositeCommand ForwardCommand = new CompositeCommand();

        /// <summary>
        /// 复制命令
        /// </summary>
        public static CompositeCommand CopyCommand = new CompositeCommand();

        /// <summary>
        /// 粘贴命令
        /// </summary>
        public static CompositeCommand PasteCommand = new CompositeCommand();

        /// <summary>
        /// 加载外部数据（点云、stl、模型）
        /// </summary>
        public static CompositeCommand LoadDatasCommand = new CompositeCommand();

        /// <summary>
        /// 参数设置窗显示命令
        /// </summary>
        public static CompositeCommand FuncParaShowCommand = new CompositeCommand();

        /// <summary>
        /// 另存项目命令
        /// </summary>
        public static CompositeCommand SaveAsProjectCommand = new CompositeCommand();

        /// <summary>
        /// 相机视角切换命令
        /// </summary>
        public static CompositeCommand CameraSwitchCommand = new CompositeCommand();

        /// <summary>
        /// 选中对象置中命令
        /// </summary>
        public static CompositeCommand CameraCenterCommand = new CompositeCommand();

        /// <summary>
        /// 退出软件命令
        /// </summary>
        public static CompositeCommand QuitCommand = new CompositeCommand();

        /// <summary>
        /// 增加输入输出参数命令
        /// </summary>
        public static CompositeCommand AddInOutParameterCommand = new CompositeCommand();

        /// <summary>
        /// 执行提取顺序命令
        /// </summary>
        public static CompositeCommand ExeStepGroupCommand = new CompositeCommand();

        /// <summary>
        /// 执行提取顺序序列命令
        /// </summary>
        public static CompositeCommand ExeAsyncGroupCommand = new CompositeCommand();

        /// <summary>
        /// 执行打开报告命令
        /// </summary>
        public static CompositeCommand OpenReportCommand = new CompositeCommand();

        /// <summary>
        /// 执行打开输入输出参数配置命令
        /// </summary>
        public static CompositeCommand OpenConfigInOutCommand = new CompositeCommand();

        /// <summary>
        /// 执行批量导入点命令
        /// </summary>
        public static CompositeCommand OpenBatchImportPointsCommand = new CompositeCommand();

        /// <summary>
        /// 执行批量导入点命令
        /// </summary>
        public static CompositeCommand InOutParaMoveCommand = new CompositeCommand();

        /// <summary>
        /// 执行批量导入点命令
        /// </summary>
        public static CompositeCommand UpDateBatchPoints = new CompositeCommand();

        /// <summary>
        /// 显示帮助文档
        /// </summary>
        public static CompositeCommand OpenHelpDocCommand = new CompositeCommand();

        /// <summary>
        /// 屏幕截图插入当前报告界面
        /// </summary>
        public static CompositeCommand InsertScreenShotCommand = new CompositeCommand();
    }
}