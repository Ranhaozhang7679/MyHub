using HandyControl.Data;
using HandyControl.Tools;
using Luster.SimDevice.SubSystem.Langs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Luster.SimDevice.SubSystem.Views
{
    /// <summary>
    /// NonClientAreaContent.xaml 的交互逻辑
    /// </summary>
    public partial class NonClientAreaContent
    {
        /// <summary>
        /// 
        /// </summary>
        public event Action<SkinType> ChangeSkinEvent;

        public NonClientAreaContent()
        {
            InitializeComponent();
        }

        private void ButtonConfig_OnClick(object sender, RoutedEventArgs e)
        {
            PopupConfig.IsOpen = true;
        }

        private void ButtonLangs_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button btn)
            {
                string langName = btn.Tag.ToString();
                PopupConfig.IsOpen = false;
                if (langName.Equals(AppConfig.Lang))
                {
                    return;
                }

                // 更新切换后的语言
                ConfigHelper.Instance.SetLang(langName);
                LangProvider.Culture = new CultureInfo(langName);
                //Messenger.Default.Send<object>(null, MessageToken.LangUpdated);

                AppConfig.Lang = langName;
                //GlobalData.Save();
            }
        }

        private void ButtonSkins_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button btn)
            {
                SkinType skinType = (SkinType)btn.Tag;
                PopupConfig.IsOpen = false;
                if (skinType.Equals(AppConfig.Skin))
                {
                    return;
                }

                AppConfig.Skin = skinType;
                ChangeSkinEvent?.Invoke(skinType);
            }
        }
    }
}
