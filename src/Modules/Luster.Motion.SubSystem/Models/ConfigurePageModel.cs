#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PageModel
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.SubSystem.Models
* 文 件 名:       PageModel.cs
* 创建时间:       2022/7/12 9:09:12
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      6ebf29df-d05d-4ad7-9bdf-6791332abd82
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/12 9:09:12
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.SubSystem.Models
{
    public class ConfigurePageModel : BindableBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        private string _name = "";
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// 是否选中
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        /// <summary>
        /// 名称
        /// </summary>
        private string _region = "";
        public string Region
        {
            get { return _region; }
            set { SetProperty(ref _region, value); }
        }

        /// <summary>
        /// 图片信息
        /// </summary>
        private string _iconfont;
        public string Iconfont
        {
            get { return _iconfont; }
            set { SetProperty(ref _iconfont, value); }
        }

        private static ObservableCollection<ConfigurePageModel> _pages;
        public static ObservableCollection<ConfigurePageModel> Pages
        {
            get
            {
                if (_pages == null)
                {
                    _pages = new ObservableCollection<ConfigurePageModel>();
                    if (ConfigurationManager.AppSettings.AllKeys.Contains("UITemplate"))
                    {
                        var keyvalue = ConfigurationManager.AppSettings["UITemplate"];
                        if (keyvalue != "" && keyvalue != "PCB")
                        {
                            List<ConfigurePageModel> listLusshare = new List<ConfigurePageModel>()
                        {
                             new ConfigurePageModel() { Name = "Project",IsSelected=true,Region="ProjectContent",Iconfont="\xe609"   },
                             //new ConfigurePageModel() { Name = "Flow" ,IsSelected=false, Region = "FlowContent",Iconfont="\xe609" },
                             //new ConfigurePageModel() { Name = "SpecSet",IsSelected=false,Region="QualitySetContent",Iconfont="\xe609"   },             
                           
                        };
                            _pages.AddRange(listLusshare);
                        }
                    }
                    List<ConfigurePageModel> list = new List<ConfigurePageModel>()
                        {
                              new ConfigurePageModel() { Name = "MachineConfigure",IsSelected=false,Region="SoftConfigureContent",Iconfont="\xe609"  },
                              new ConfigurePageModel() { Name = "PLCConfigure",IsSelected=false,Region="PlcAlarmContent",Iconfont="\xe609"  },
                              //new ConfigurePageModel() { Name = "SoftConfigure",IsSelected=false,Region="UserContent",Iconfont="\xe609"   },
                              new ConfigurePageModel() { Name = "Cockpit",IsSelected=false,Region="ProductInfoContent",Iconfont="\xe609"   },
                              new ConfigurePageModel() { Name = "RobotInfo" ,IsSelected=false, Region = "RobotConfigureContent",Iconfont="\xe609" },
                              new ConfigurePageModel() { Name = "FileConfig" ,IsSelected=false, Region = "FileConfigContent",Iconfont="\xe609" },
                              new ConfigurePageModel() { Name = "VisionInformation",IsSelected=false,Region="VisionConfig",Iconfont="\xe609"   },
                              new ConfigurePageModel() { Name = "FXTCP",IsSelected=false,Region="FXTCPContent",Iconfont="\xe609"  },
                              new ConfigurePageModel() { Name = "FunctionEnable",IsSelected=false,Region="FunctionEnableContent",Iconfont="\xe609"  }
                              //new ConfigurePageModel() { Name = "SoftInformation" ,IsSelected=false, Region = "SysConfingContent",Iconfont="\xe609" },
                    };
                    _pages.AddRange(list);

                    var Prjmodel = _pages.FirstOrDefault(m => m.Name == "Project");
                    if (Prjmodel == null)
                    {
                        var machineSetmodel = _pages.FirstOrDefault(m => m.Name == "MachineConfigure");
                        if (machineSetmodel != null)
                        {
                            machineSetmodel.IsSelected = true;
                        }
                    }

                }
                return _pages;
            }
        }

        /// <summary>
        /// 选中项为互斥
        /// </summary>
        /// <param name="name"></param>
        public void SetSelected(string Name)
        {
            foreach (var item in ConfigurePageModel.Pages)
            {
                if (item.Name != Name)
                {
                    item.IsSelected = false;
                }
                else
                {
                    item.IsSelected = true;
                }
            }
        }

        public void SetUnSelected(string Name)
        {
            foreach (var item in ConfigurePageModel.Pages)
            {
                if (item.Name == Name)
                {
                    item.IsSelected = false;
                }
            }
        }
    }
}