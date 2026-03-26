#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 接口名称:       IMultiElement
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.DataStruct.Interfaces
* 文 件 名:       IMultiElement.cs
* 创建时间:       2022/2/21 9:02:37
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      fb73f08a-5a96-4037-80be-ed4fcb7a5706
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/2/21 9:02:37
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.TaskFlow.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Common.Interfaces
{
    //多个特征构建

    //点和点->构建线、                    两点测距离、
    //多点->拟合点云、多点建面、          多点合并
    //点和线->投影点、求垂线、构建面      点线距离
    
    //点和面->求垂线、投影点、           点面距离、


    //线和线->求交点、                   线线角度、两线距离、
    //线和面->投影线、求交点             线面距离、线面角度

    //三面->三面相交点、                 
    //面面->面与面相交线                 面面角度、

    //单特征右键
    //线-> 平移、变换

    /// <summary>
    /// 多选自动管理模块
    /// </summary>
    public interface IMultiSelected
    {
        /// <summary>
        /// 1.验证对象是否满足多几何生成
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        bool GetCategories(List<Type> parameters, out object[] retCategories);

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="category">对应的类别</param>
        /// <param name="parameters">对应的参数</param>
        void SetParameters(string category, params ParameterAttribute[] parameters);
    }
}