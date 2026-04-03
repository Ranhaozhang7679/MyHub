using Luster.Motion.DigitalSetup.Datas;
using System;
using System.Collections.Generic;

namespace Luster.Motion.DigitalSetup.Services
{
    /// <summary>
    /// 点检状态服务 - 管理所有页面的点检状态
    /// </summary>
    public class CheckStatusService
    {
        private readonly CheckStatusPersistenceService _persistenceService;
        private CheckStatusPersistenceData _data;
        private readonly object _lock = new object();

        /// <summary>
        /// 状态变更事件
        /// </summary>
        public event Action<string, CheckStatus> StatusChanged;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="persistenceService">持久化服务</param>
        public CheckStatusService(CheckStatusPersistenceService persistenceService)
        {
            _persistenceService = persistenceService ?? throw new ArgumentNullException(nameof(persistenceService));
            _data = _persistenceService.Load() ?? new CheckStatusPersistenceData();
        }

        /// <summary>
        /// 设置配置路径（基于配方路径）
        /// </summary>
        /// <param name="recipePath">配方路径</param>
        public void SetConfigPath(string recipePath)
        {
            _persistenceService.SetConfigPath(recipePath);
            // 重新加载数据
            _data = _persistenceService.Load() ?? new CheckStatusPersistenceData();
        }

        /// <summary>
        /// 更新页面点检状态
        /// </summary>
        /// <param name="pageKey">页面唯一标识</param>
        /// <param name="status">点检状态</param>
        /// <param name="parentRegion">父页面Region</param>
        /// <param name="subPageName">子页面名称</param>
        /// <param name="operatorName">操作人员</param>
        /// <param name="remark">备注信息</param>
        public void UpdateStatus(string pageKey, CheckStatus status, string parentRegion, string subPageName, string operatorName = "", string remark = "")
        {
            if (string.IsNullOrEmpty(pageKey))
            {
                return;
            }

            lock (_lock)
            {
                var record = new PageCheckRecord
                {
                    PageKey = pageKey,
                    ParentRegion = parentRegion ?? "",
                    SubPageName = subPageName ?? "",
                    Status = status,
                    CheckTime = DateTime.Now,
                    Operator = operatorName ?? "",
                    Remark = remark ?? ""
                };

                _data.CheckRecords[pageKey] = record;
                _persistenceService.Save(_data);
            }

            // 触发状态变更事件
            StatusChanged?.Invoke(pageKey, status);
        }

        /// <summary>
        /// 获取页面点检状态
        /// </summary>
        /// <param name="pageKey">页面唯一标识</param>
        /// <returns>点检状态</returns>
        public CheckStatus GetStatus(string pageKey)
        {
            if (string.IsNullOrEmpty(pageKey))
            {
                return CheckStatus.NotChecked;
            }

            lock (_lock)
            {
                if (_data.CheckRecords.TryGetValue(pageKey, out var record))
                {
                    return record.Status;
                }
                return CheckStatus.NotChecked;
            }
        }

        /// <summary>
        /// 获取页面点检记录
        /// </summary>
        /// <param name="pageKey">页面唯一标识</param>
        /// <returns>点检记录，如果不存在则返回null</returns>
        public PageCheckRecord GetRecord(string pageKey)
        {
            if (string.IsNullOrEmpty(pageKey))
            {
                return null;
            }

            lock (_lock)
            {
                if (_data.CheckRecords.TryGetValue(pageKey, out var record))
                {
                    return record;
                }
                return null;
            }
        }

        /// <summary>
        /// 获取所有点检记录
        /// </summary>
        /// <returns>点检记录字典</returns>
        public Dictionary<string, PageCheckRecord> GetAllRecords()
        {
            lock (_lock)
            {
                return new Dictionary<string, PageCheckRecord>(_data.CheckRecords);
            }
        }

        /// <summary>
        /// 获取指定父页面下的所有点检记录
        /// </summary>
        /// <param name="parentRegion">父页面Region</param>
        /// <returns>点检记录列表</returns>
        public List<PageCheckRecord> GetRecordsByParentRegion(string parentRegion)
        {
            var result = new List<PageCheckRecord>();
            lock (_lock)
            {
                foreach (var record in _data.CheckRecords.Values)
                {
                    if (record.ParentRegion == parentRegion)
                    {
                        result.Add(record);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 重置所有状态为未点检
        /// </summary>
        public void ResetAllStatus()
        {
            lock (_lock)
            {
                _data.CheckRecords.Clear();
                _persistenceService.Save(_data);
            }
        }

        /// <summary>
        /// 重置指定父页面下的所有状态为未点检
        /// </summary>
        /// <param name="parentRegion">父页面Region</param>
        public void ResetStatusByParentRegion(string parentRegion)
        {
            lock (_lock)
            {
                var keysToRemove = new List<string>();
                foreach (var kvp in _data.CheckRecords)
                {
                    if (kvp.Value.ParentRegion == parentRegion)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    _data.CheckRecords.Remove(key);
                }

                if (keysToRemove.Count > 0)
                {
                    _persistenceService.Save(_data);
                }
            }
        }

        /// <summary>
        /// 应用状态到PageModel
        /// </summary>
        /// <param name="page">页面模型</param>
        public void ApplyStatusToPage(CommonPageModel page)
        {
            if (page == null)
            {
                return;
            }

            var record = GetRecord(page.PageKey);
            if (record != null)
            {
                page.CheckStatus = record.Status;
                page.LastCheckTime = record.CheckTime;
                page.LastCheckOperator = record.Operator;
                page.CheckRemark = record.Remark;
            }
            else
            {
                page.CheckStatus = CheckStatus.NotChecked;
                page.LastCheckTime = null;
                page.LastCheckOperator = null;
                page.CheckRemark = null;
            }
        }

        /// <summary>
        /// 批量应用状态到PageModel列表
        /// </summary>
        /// <param name="pages">页面模型集合</param>
        /// <param name="parentRegion">父页面Region</param>
        public void ApplyStatusToPages(System.Collections.Generic.IEnumerable<CommonPageModel> pages, string parentRegion)
        {
            if (pages == null)
            {
                return;
            }

            foreach (var page in pages)
            {
                page.ParentRegion = parentRegion;
                ApplyStatusToPage(page);
            }
        }

        /// <summary>
        /// 重新加载数据（从文件）
        /// </summary>
        public void Reload()
        {
            lock (_lock)
            {
                _data = _persistenceService.Load() ?? new CheckStatusPersistenceData();
            }
        }

        /// <summary>
        /// 获取最后保存时间
        /// </summary>
        /// <returns>最后保存时间</returns>
        public DateTime? GetLastSavedTime()
        {
            lock (_lock)
            {
                return _data.LastSavedTime;
            }
        }
    }
}
