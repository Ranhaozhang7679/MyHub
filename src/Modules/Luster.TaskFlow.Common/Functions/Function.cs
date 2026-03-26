#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Function
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Functions
* 文 件 名:       Function
* 创建时间:       2021/10/30 10:22:38
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      a1272ea3-4d3b-4a23-b152-baf496516674
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/30 10:22:38
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Module;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;
using System.Configuration;

namespace Luster.TaskFlow.Common.Functions
{
    public class Function : IFunction
    {
        #region 名称信息

        /// <summary>
        /// 名称
        /// </summary>
        [Browsable(false)]
        public string Name { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        [Browsable(false)]
        public string Alias { get; set; }

        /// <summary>
        /// 提示信息
        /// </summary>
        [Browsable(false)]
        public string Tips { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        [Browsable(false)]
        public string Icon { get; set; }

        #endregion

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// 分组
        /// </summary>
        [Browsable(false)]
        public virtual string Group => string.Empty;

        /// <summary>
        /// 对应的图标
        /// </summary>
        [Browsable(false)]
        public virtual string GroupIcon => string.Empty;

        /// <summary>
        /// 运行状态
        /// </summary>
        [Parameter("运行状态", 100, Visible = false, CN = "运行状态", ParamType = ParamType.OUT)]
        public LStatus Status { get; set; } = new LStatus();

        /// <summary>
        /// 临时变量
        /// </summary>
        private IModule module;

        /// <summary>
        /// 隶属模块
        /// </summary>
        [Browsable(false)]
        public virtual IModule Owner
        {
            get { return module; }
            set
            {
                module = value;
                InitParameters();
            }
        }

        /// <summary>
        /// 是否要显示进度体条
        /// </summary>
        public bool IsProgressing { get; set; } = false;

        /// <summary>
        /// 是否有动画
        /// </summary>
        public bool Animation { get; set; } = false;

        /// <summary>
        /// 动态参数
        /// </summary>
        public bool DynParam { get; set; } = false;

        /// <summary>
        /// 构建函数
        /// </summary>
        public Function()
        {
            this.Name = GetType().Name;
            this.Alias = this.Name;
            this.Tips = string.Empty;
            this.Icon = "\xea1a";
        }


        #region 虚方法

        /// <summary>
        /// 初始化参数方法
        /// </summary>
        protected void InitParameters()
        {
            var props = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var item in props)
            {
                if (!item.IsDefined(typeof(ParameterAttribute)) || item.GetSetMethod() == null) continue;

                var name = item.Name;
                if (!Owner.Parameters.ContainsKey(name))
                {
                    var pItem = ParameterAttribute.CreateByProperty(item, Owner);
                    if (!Owner.Parameters.ContainsKey(item.Name))
                    {
                        Owner.Parameters.Add(item.Name, pItem);
                    }
                }
            }

            // 更新Owner对象
            Status.Owner = Owner;
        }

        /// <summary>
        /// 搜索所有同类型的参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="excluded">排除的参数</param>
        /// <returns>对应的值</returns>
        protected List<T> GetParametersByType<T>(params string[] excluded)
        {
            List<T> results = new List<T>();
            Type type = typeof(T);
            foreach (var item in Owner.Parameters)
            {
                if (excluded.Contains(item.Key) || item.Value.ParamType == Enums.ParamType.OUT) continue;

                ParameterAttribute pVal = item.Value;
                if (pVal.Type == type && pVal != null)
                {
                    results.Add((T)pVal.Value);
                }
            }

            return results;
        }

        /// <summary>
        /// 渲染方法
        /// </summary>
        /// <param name="window"></param>
        public virtual bool Draw(IInteractor window, string actorColor = "")
        {
            return false;
        }

        /// <summary>
        /// 设置渲染对象是否可见
        /// </summary>
        /// <param name="window"></param>
        /// <param name="isVisible"></param>
        public virtual void SetDrawVisible(IInteractor window, bool isVisible)
        {
            window?.SetActorVisible(isVisible, Owner.ID);
        }

        /// <summary>
        /// UI 交互需要在执行前做的一些动作,涉及到仿射矩阵，所以需要在运行前进行调整
        /// </summary>
        public virtual void PrevExcute()
        {

        }


        /// <summary>
        /// 执行动画效果
        /// </summary>
        /// <param name="animationFunc">动画完成回调</param>
        public virtual void DoAnimation(Action<bool> animationFunc)
        {

        }

        /// <summary>
        /// 函数执行
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public virtual bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            return string.IsNullOrEmpty(errMsg);
        }

        /// <summary>
        /// 属性回调
        /// </summary>
        public virtual void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
        }

        /// <summary>
        /// 获取相同类型的Sort排序
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>最后一个类型的序号</returns>
        private int GetSameTypeSort(Type type)
        {
            int sort = -1;
            foreach (var item in Owner.Parameters)
            {
                if (item.Value.Type == type && item.Value.ParamType == ParamType.IN)
                {
                    sort = item.Value.Sort;
                }
            }

            return sort;
        }

        /// <summary>
        /// 构建动态参数
        /// </summary>
        /// <param name="startNum">构建第一个值</param>
        /// <param name="allNum">总数量</param>
        /// <param name="key">英文参数</param>
        /// <param name="cn">中文参数名</param>
        /// <param name="createType">构建类型</param>
        public void BuildExternParameters(int startNum, int allNum, string key, string cn, Type createType, object defaultV = null, string group = "")
        {
            // 动态构建参数

            // 0.记录目前的包含Cloud名称的参数数量
            int srcNum = 0;

            // 1.移除历史参数
            List<string> extNames = new List<string>();
            foreach (var item in Owner.Parameters)
            {
                if (item.Value.ParamType == ParamType.OUT) continue;

                if (item.Key.IndexOf(key) == 0)
                {
                    srcNum++;
                }

                if (item.Value.Property == null)
                {
                    extNames.Add(item.Key);
                }
            }

            // 2.和原来PointCloud参数数量做对比，如果不同，才进行对象更新
            if (srcNum == 0 || srcNum == allNum)
                return;

            // 5.找到现有的这个参数
            List<KeyValue> dependOns = new List<KeyValue>();
            string startParamterName = $"{key}{startNum}";
            if (Owner.Parameters.ContainsKey(startParamterName))
            {
                dependOns = Owner.Parameters[startParamterName].DependOns;
            }

            // 3.构建动态参数
            for (int i = startNum; i < allNum; i++)
            {
                string name = $"{key}{i + 1}";
                string cnName = $"{cn}{i + 1}";

                // 已经存在该对象的情况
                if (Owner.Parameters.ContainsKey(name))
                {
                    continue;
                }

                int sort = GetSameTypeSort(createType) + 1;

                ParameterAttribute p = ParameterAttribute.CreateByType(name, createType, Owner, cnName, sort);
                //增加描述语言
                foreach (var item in Owner.Parameters)
                {
                    if (item.Value.Type.Name == p.Type.Name && item.Value.Tips != null)
                    {
                        p.Tips = item.Value.Tips;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(group))
                {
                    p.Group = group;
                }

                // 设置默认值
                if (defaultV != null)
                {
                    p.Value = defaultV;
                }

                p.DependOns = dependOns;
                Owner.Parameters.Add(name, p);
            }

            // 4.参数变少，那么就需要移除多余的参数
            if (srcNum > allNum)
            {
                for (int i = allNum; i < srcNum; i++)
                {
                    string name = $"{key}{i + 1}";
                    Owner.Parameters.Remove(name);
                }
            }

            // 4.通知参数发生变更
            Owner.OnUpdate(TaskFlow.Common.Enums.ModuleUpdate.ParameterNum);
        }

        /// <summary>
        /// 构建动态参数
        /// </summary>
        /// <param name="startNum">构建第一个值</param>
        /// <param name="allNum">总数量</param>
        /// <param name="key">英文参数</param>
        /// <param name="cn">中文参数名</param>
        /// <param name="createType">构建类型</param>
        public void BuildExternParameters(int startNum, int allNum, string key, string cn, Type createType, Action<ParameterAttribute> action)
        {
            // 0.记录目前的包含Cloud名称的参数数量
            int srcNum = 0;

            // 1.移除历史参数
            List<string> extNames = new List<string>();
            foreach (var item in Owner.Parameters)
            {
                if (item.Value.ParamType == Enums.ParamType.OUT) continue;
                if (item.Key.IndexOf(key) == 0)
                {
                    srcNum++;
                }

                if (item.Value.Property == null)
                {
                    extNames.Add(item.Key);
                }
            }

            // 2.和原来PointCloud参数数量做对比，如果不同，才进行对象更新
            if (srcNum == 0 || srcNum == allNum)
                return;

            // 5.找到现有的这个参数
            List<KeyValue> dependOns = new List<KeyValue>();
            string startParamterName = $"{key}{startNum}";
            if (Owner.Parameters.ContainsKey(startParamterName))
            {
                dependOns = Owner.Parameters[startParamterName].DependOns;
            }

            // 3.构建动态参数
            for (int i = startNum; i < allNum; i++)
            {
                string name = $"{key}{i + 1}";
                string cnName = $"{cn}{i + 1}";

                // 已经存在该对象的情况
                if (Owner.Parameters.ContainsKey(name))
                {
                    continue;
                }

                int sort = GetSameTypeSort(createType) + 1;

                ParameterAttribute p = ParameterAttribute.CreateByType(name, createType, Owner, cnName, sort);
                //增加描述语言
                foreach (var item in Owner.Parameters)
                {
                    if (item.Value.Type.Name == p.Type.Name && item.Value.Tips != null)
                    {
                        p.Tips = item.Value.Tips;
                        break;
                    }
                }

                p.DependOns = dependOns;

                action?.Invoke(p);

                Owner.Parameters.Add(name, p);
            }

            // 4.参数变少，那么就需要移除多余的参数
            if (srcNum > allNum)
            {
                for (int i = allNum; i < srcNum; i++)
                {
                    string name = $"{key}{i + 1}";
                    Owner.Parameters.Remove(name);
                }
            }

            // 4.通知参数发生变更
            Owner.OnUpdate(TaskFlow.Common.Enums.ModuleUpdate.ParameterNum);
        }

        /// <summary>
        /// 通过类型固定生成对应的参数
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="key"></param>
        /// <param name="cn"></param>
        /// <param name="createType"></param>
        /// <param name="action"></param>
        public void BuildExternParameter(string key, string cn, int sort, Type createType, Action<ParameterAttribute> action)
        {
            // 1.构建动态参数
            string name = $"{key}";
            string cnName = $"{cn}";

            // 2.已经存在该对象的情况
            if (Owner.Parameters.ContainsKey(name))
            {
                return;
            }

            // 3.增加描述语言
            ParameterAttribute p = ParameterAttribute.CreateByType(name, createType, Owner, cnName, sort);
            foreach (var item in Owner.Parameters)
            {
                if (item.Value.Type.Name == p.Type.Name && item.Value.Tips != null)
                {
                    p.Tips = item.Value.Tips;
                    break;
                }
            }

            // 4.自定义配置
            action?.Invoke(p);

            // 5.更新参数对象
            p.Group = (p.ParamType == ParamType.IN) ? "InputParameter" : "OutputParameter";

            // 6.新增对象
            Owner.Parameters.Add(name, p);

            // 7.通知参数发生变更
            Owner.OnUpdate(TaskFlow.Common.Enums.ModuleUpdate.ParameterNum);
        }
        /// <summary>
        /// 设置引用参数
        /// </summary>
        /// <param name="refName">引用参数名称</param>
        /// <param name="refType">引用参数类型</param>
        /// <param name="parameters">参数列表</param>
        protected bool SetParameterRef(string refName, Type refType, params ParameterAttribute[] parameters)
        {
            bool isRef = false;
            var pAttr = parameters.FirstOrDefault(u => refType.IsAssignableFrom(u.Type) || u.Type == refType);
            if (pAttr != null && Owner.Parameters.ContainsKey(refName))
            {
                Owner.Parameters[refName].RefOut = pAttr;
                isRef = true;
            }

            return isRef;
        }

        /// <summary>
        /// 设置参数值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        protected void SetParameterVal(string name, object val)
        {
            if (Owner == null) return;
            if (Owner.Parameters.ContainsKey(name))
            {
                Owner.Parameters[name].Value = val;
            }
        }

        /// <summary>
        /// 获取参数
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected object GetParameterVal(string name)
        {
            if (Owner == null) return null;
            if (Owner.Parameters.ContainsKey(name))
            {
                return Owner.Parameters[name].GetValue(out string err);
            }

            return null;
        }

        /// <summary>
        /// 更新数据源
        /// </summary>
        /// <param name="propName"></param>
        protected void OnDataSource(string propName, object[] datas)
        {
            if (Owner == null) return;
            if (Owner.Parameters.ContainsKey(propName))
            {
                Owner.Parameters[propName].Datas = datas;
            }
        }
        #endregion

        #region 对象释放

        private bool isDisposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            isDisposed = true;
        }

        #endregion

        #region 对象导入和导出

        public virtual XElement ExportXml()
        {
            // 设置功能模块
            XElement xFunc = new XElement("Function");
            xFunc.SetAttributeValue("Name", GetType().Name);

            // 对输出进行排序
            List<ParameterAttribute> parameters = new List<ParameterAttribute>();
            foreach (var item in Owner.Parameters)
            {
                bool isIn = item.Value.ParamType == ParamType.IN || (item.Value.ParamType == ParamType.OUT && item.Value.Property == null);

                var alias = item.Value.Alias;
                // 参数名称和别名不一致，进行保存
                bool isOutAndAlias = (item.Value.ParamType == ParamType.OUT && !string.IsNullOrEmpty(alias) && alias != item.Value.CN);

                if (isIn || isOutAndAlias || item.Value.IsOutData)
                {
                    parameters.Add(item.Value);
                    //parameters.Add(item.c)
                }
            }

            // 按照顺序进行保持
            foreach (var item in parameters.OrderBy(u => u.Sort))
            {
                // 只对输入参数进行导出
                xFunc.Add(item.ExportXml());
            }

            return xFunc;
        }

        public virtual void ParserXml(XElement xFunc)
        {
            var parameters = Owner.Parameters;
            foreach (var xItem in xFunc.Elements())
            {
                var xName = xItem.Name.LocalName;

                if (parameters.ContainsKey(xName))
                {
                    parameters[xName].ParserXml(xItem);
                }
                else
                {
                    if (DynParam)
                    {
                        ParameterAttribute parameter = new ParameterAttribute("", 0);
                        parameter.Owner = Owner;
                        parameter.ParserXml(xItem);

                        parameters.Add(parameter.Name, parameter);
                    }
                }
            }
        }
        #endregion
    }
}