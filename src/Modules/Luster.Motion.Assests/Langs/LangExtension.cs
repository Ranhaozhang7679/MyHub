#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LangExtension
* 机器名称:       L05123-NB
* 命名空间:       Luster.SubSystem.ThreeD.Extension
* 文 件 名:       LangExtension
* 创建时间:       2021/11/3 11:28:03
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      172b8b74-c857-43cc-9b72-4c6b26dc11ca
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/3 11:28:03
* 修 改 人:		  luster
************************************************************************************/

#endregion

using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using HandyControl.Properties.Langs;

namespace Luster.Motion.Assests.Langs
{
    public class LangExtension : HandyControl.Tools.Extension.LangExtension
    {
        public LangExtension()
        {
            Source = Langs.LangProvider.Instance;
        }
    }
}