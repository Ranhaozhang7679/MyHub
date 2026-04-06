#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IDbManager
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.DbManager
* 文 件 名:       IDbManager.cs
* 创建时间:       2022/8/9 8:40:02
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      fcc13201-e6f5-4092-ac0a-a532d28d28b4
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/9 8:40:02
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataAccess.Tables;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.TaskFlow.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI
{
    public interface IDbManager
    {
        /// <summary>
        /// 获取开班时间
        /// </summary>
        /// <returns></returns>
        DateTime GetStartTime();

        /// <summary>
        /// 项目路径
        /// </summary>
        string ProjectPath { get; set; }

        #region 首页统计相关
        /// <summary>
        /// 获取指定产品质量规则
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        TbLTolerance GetLTolerance(Guid id, string name);

        /// <summary>
        /// 添加产品质量判断规则
        /// </summary>
        /// <param name="models"></param>
        /// <param name="recipeName"></param>
        void AddTbLTolerance(IEnumerable<TbLTolerance> models);

        /// <summary>
        /// 删除指定质量规则
        /// </summary>
        /// <param name="tbLTolerance"></param>
        /// <param name="recipeName"></param>
        void DeleteQualityItem(TbLTolerance tbLTolerance, string recipeName);

        /// <summary>
        /// 删除不匹配的质量规则项
        /// </summary>
        /// <param name="tbLTolerances"></param>
        /// <param name="recipeName"></param>
        void DeleteQualityItems(IEnumerable<TbLTolerance> tbLTolerances, string recipeName);

        /// <summary>
        /// 获取某一菜单下所有质量规则项
        /// </summary>
        /// <param name="recipeName"></param>
        /// <returns></returns>
        IEnumerable<TbLTolerance> GetQualityItems(string recipeName);
        #endregion

        #region 报警相关

        /// <summary>
        /// 获取一段时间内报警总数
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="searchParas"></param>
        /// <returns></returns>
        int GetAlarmCount(DateTime startTime, DateTime endTime, string searchParas = "");

        /// <summary>
        /// 获取一页报警数据
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="searchContent"></param>
        /// <param name="sort"></param>
        /// <param name="pageIndex"></param>
        /// <param name="perPageCount"></param>
        /// <param name="count"></param> 
        /// <param name="filterTypes">需要过滤的报警类型列表</param>
        /// <param name="excludeTypes">是否排除指定的报警类型</param>
        /// <returns></returns>
        IEnumerable<TbAlarm> GetAlarmPageData(DateTime startTime, DateTime endTime, string searchContent,
            string sort, int pageIndex, int perPageCount, out long count, List<string> filterTypes = null, bool excludeTypes = false);

        /// <summary>
        /// 获取统计分析数据源
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="isDay"></param>
        /// <returns></returns>
        IEnumerable<TbAlarm> GetAnalyzeModels(DateTime startTime, DateTime endTime);

        /// <summary>
        /// 清楚当前产能信息
        /// </summary>
        /// <param name="lastTime"></param>
        void ClearCurrentCapacityInfo();
        #endregion


        #region 用户操作相关
        /// <summary>
        /// 获取时间段用户操作数目
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="searchParas"></param>
        /// <returns></returns>
        int GetLogCount(DateTime startTime, DateTime endTime, string searchParas = "");

        /// <summary>
        /// 获取时间段用户操作
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="searchContent"></param>
        /// <param name="sort"></param>
        /// <param name="pageIndex"></param>
        /// <param name="perPageCount"></param>
        /// <param name="count"></param>
        /// /// <returns></returns>
        IEnumerable<TbSysOperation> GetOperationPageData(DateTime startTime, DateTime endTime, string searchContent,
            string sort, int pageIndex, int perPageCount, out long count);

        /// <summary>
        /// 获取稼动数据
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="searchParas"></param>
        /// <returns></returns>
        IEnumerable<TbSysOperation> GetOperationList(DateTime startTime, DateTime endTime, string searchParas = "");

        /// <summary>
        /// 获取时间段内最新一次机台记录
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        IEnumerable<TbSysOperation> GetLastOperation(DateTime startTime, DateTime endTime);
        #endregion

        /// <summary>
        /// 添加一条完整报警信息
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="alarmType"></param>
        /// <param name="errMessage"></param>
        /// <param name="startTime"></param>
        /// <param name="alarmProc"></param>
        void AddSysOperation(SystemOperation systemOperation, string memo = "", string user = "Operator");

        /// <summary>
        /// 新增变更记录
        /// </summary>
        /// <param name="tbRecord"></param>
        void AddChangeRecord(TbChangeRecord tbRecord);

        #region 辅料管理

        /// <summary>
        /// 添加辅料
        /// </summary>
        /// <param name="name"></param>
        /// <param name="persent"></param>
        /// <param name="pieceCount"></param>
        void AddAccessory(string name, int persent, int pieceCount);

        /// <summary>
        /// 删除辅料
        /// </summary>
        /// <param name="id"></param>
        void DeleteAccessory(long id);

        /// <summary>
        /// 添加辅料更换记录
        /// </summary>
        /// <param name="name"></param>
        /// <param name="batcgNo"></param>
        /// <param name="count"></param>
        /// <param name="stationName"></param>
        void AddChangeAccessoryLog(string name, string batcgNo, int count, string stationName);

        /// <summary>
        /// 获取全部辅料信息
        /// </summary>
        /// <returns></returns>
        List<TbAccessory> GetAllAccessory();

        IEnumerable<TbProductInfo> GetLast50Products();
        #endregion

        /// <summary>
        /// 更新产能信息
        /// </summary>
        /// <param name="all"></param>
        /// <param name="ok"></param>
        /// <param name="ng"></param>
        /// <param name="throwMats"></param>
        void UpdateProYield(TbProductYeild tbProductYeild);

        /// <summary>
        /// 获取产能信息
        /// </summary>
        /// <returns></returns>
        TbProductYeild GetProYield();

        /// <summary>
        /// 获取当前UPH
        /// </summary>
        /// <returns></returns>
        long GetCurrentUPH();
        void LoadCtConfig(string configDir);
        /// 获取CT配置中的站名称列表（listA）
        List<string> GetCTConfigStationNames();

        // 关键参数写入CSV
        void WriteParameterToCSV(); // List<VAxis> vAxises


    }
}
