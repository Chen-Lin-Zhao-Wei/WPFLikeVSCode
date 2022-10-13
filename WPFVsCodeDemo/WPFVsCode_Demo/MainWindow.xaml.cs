using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFVsCode.Controls;

namespace WPFVsCode_Demo
{
    public interface IGoToPage
    {
        //关闭选项卡
        bool CloseFrameTabItem(string FrameName);
        object FindContentInTabItem(string FrameName);
        object SelectedFrameTabItem(string FrameName);
        void AddFrameTabItem(string FrameName, FrameworkElement Content, string IconResource);
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : IGoToPage
    {
        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }


        private void Initialize()
        {
            Paragraph.Inlines.AddRange(new Inline[] {
                new Run(@"//$$\      $$\ $$\   $$\   $$$$$$$\  $$$$$$$$\ $$\    $$\   $$$$$$\    $$\     $$\    $$$$$$\   $$$$$$\  $$\   $$\ "), new LineBreak(),
                new Run(@"//$$ | $\  $$ |$$ | $$  |  $$  __$$\ $$  _____|$$ |   $$ | $$  __$$\ $$$$ |  $$$$ |  $$  __$$\ $$  __$$\ $$ |  $$ |"), new LineBreak(),
                new Run(@"//$$ |$$$\ $$ |$$ |$$  /   $$ |  $$ |$$ |      $$ |   $$ | \__/  $$ |\_$$ |  \_$$ |  \__/  $$ |\__/  $$ |$$ |  $$ |"), new LineBreak(),
                new Run(@"//$$ $$ $$\$$ |$$$$$  /    $$ |  $$ |$$$$$\    \$$\  $$  |  $$$$$$  |  $$ |    $$ |   $$$$$$  | $$$$$$  |$$$$$$$$ |"), new LineBreak(),
                new Run(@"//$$$$  _$$$$ |$$  $$<     $$ |  $$ |$$  __|    \$$\$$  /  $$  ____/   $$ |    $$ |  $$  ____/ $$  ____/ \_____$$ |"), new LineBreak(),
                new Run(@"//$$$  / \$$$ |$$ |\$$\    $$ |  $$ |$$ |        \$$$  /   $$ |        $$ |    $$ |  $$ |      $$ |            $$ |"), new LineBreak(),
                new Run(@"//$$  /   \$$ |$$ | \$$\   $$$$$$$  |$$$$$$$$\    \$  /    $$$$$$$$\ $$$$$$\ $$$$$$\ $$$$$$$$\ $$$$$$$$\       $$ |"), new LineBreak(),
                new Run(@"//\__/     \__|\__|  \__|  \_______/ \________|    \_/     \________|\______|\______|\________|\________|      \__|"), new LineBreak(),
                new Run(@"//WK簡易搭建，屎山代碼																						"), new LineBreak()
            });


            TreeViewItem_Alarm1.MouseLeftButtonUp += OnTreeViewItem_MouseLeftButtonUp;
            TreeViewItem_Alarm2.MouseLeftButtonUp += OnTreeViewItem_MouseLeftButtonUp;
            TreeViewItem_Alarm3.MouseLeftButtonUp += OnTreeViewItem_MouseLeftButtonUp;
            TreeViewItem_HandleAlarm.MouseLeftButtonUp += OnTreeViewItem_MouseLeftButtonUp;
            TreeViewItem_OtherTest.MouseLeftButtonUp += OnTreeViewItem_MouseLeftButtonUp;
            TreeViewItem_DataCenter.MouseLeftButtonUp += OnTreeViewItem_MouseLeftButtonUp;
            TreeViewItem_VoiceSpeakTest.MouseLeftButtonUp += OnTreeViewItem_MouseLeftButtonUp;
            TreeViewItem_DIOBoardTest.MouseLeftButtonUp += OnTreeViewItem_MouseLeftButtonUp;
            TreeViewItem_AlarmList.MouseLeftButtonUp += OnTreeViewItem_MouseLeftButtonUp;
            TreeViewItem_MainView.MouseLeftButtonUp += OnTreeViewItem_MouseLeftButtonUp;
            TreeViewItem_PieView.MouseLeftButtonUp += OnTreeViewItem_MouseLeftButtonUp;

            MenuItemSystemSetting.Click += OnMenuItem_Click;
            MenuItemLayoutSetting.Click += OnMenuItem_Click;
            MenuItemBackgroungSetting.Click += OnMenuItem_Click;
            MenuItemAlarmSubscription.Click += OnMenuItem_Click;
            MenuItemDeviceSetting.Click += OnMenuItem_Click;

            MenuItemWelcome.Click += OnMenuItem_Click;
            MenuItemUserInfo.Click += OnMenuItem_Click;
            MenuItemModifyUser.Click += OnMenuItem_Click;
            MenuItemLoginOut.Click += OnMenuItem_Click;


            MenuItem_ShowMainMenu.Click += OnMenuItem_Click;
            MenuItem_ReLogin.Click += OnMenuItem_Click;
            MenuItem_ExitWindow.Click += OnMenuItem_Click;
            MenuItem_HideMe.Click += OnMenuItem_Click;
            Notificy.MouseDoubleClick += OnMenuItem_Click;
        }

        private void OnMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item != null)
            {
                MessageBox.Show((item.Header ?? "NaN").ToString());
            }
        }

        private void OnTreeViewItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeview = sender as TreeViewItem;
            if(treeview!=null)
            {
                string treeHEader = (treeview.Header ?? treeview.Name).ToString();
                TextBox textBox = new TextBox() { Text = treeview.Name };
                AddFTabItemIfNotExit(treeHEader, textBox, "Path_NewFile2");
                //Path_NewFile2 见 当前窗体的资源字典
            }
        }



        #region 添加选项卡
        private void AddFTabItemIfNotExit(string FrameName, FrameworkElement Content, string IconResource)
        {
            Path iconPath = CreatePath(IconResource);
            AddFTabItemIfNotExit(FrameName, Content, iconPath);
        }
        private void AddFTabItemIfNotExit(string FrameName, FrameworkElement Content, object Icon = null)
        {
            FrameworkElement exitItem = GetFTabItem(FrameName);
            if (exitItem != null)
            {
                FrameTabCtrl.SelectedItem = exitItem;
                return; //元素存在
            }

            FrameTabItem newTabItem = new FrameTabItem();
            newTabItem.Tag = FrameName;
            newTabItem.Header = FrameName;
            newTabItem.Icon = Icon;
            newTabItem.Content = Content;
            newTabItem.ContextMenu = CreateContextMenu(newTabItem);


            FrameTabCtrl.Items.Add(newTabItem);
            FrameTabCtrl.SelectedItem = newTabItem;
        }

        private Path CreatePath(string resourceName)
        {
            StreamGeometry pathFigure = this.TryFindResource(resourceName) as StreamGeometry;
            if (pathFigure != null)
            {
                Path path = new Path();
                path.Width = 12d;
                path.Height = 12d;
                path.Stretch = Stretch.Uniform;
                path.Fill = Brushes.Black;
                path.Data = pathFigure;
                return path;
            }
            return null;
        }
        private FrameworkElement GetFTabItem(string FrameName)
        {
            foreach (object item in FrameTabCtrl.Items)
            {
                if (item != null && item is FrameworkElement)
                {
                    FrameworkElement fe = (FrameworkElement)item;
                    if (fe.Tag != null && fe.Tag.ToString() == FrameName)
                    {
                        return fe;
                    }
                }
            }
            return null;
        }

        private ContextMenu CreateContextMenu(FrameworkElement newTabItem)
        {
            MenuItem menuItemCloseCurItem = new MenuItem();
            menuItemCloseCurItem.Header = "关闭当前选项卡";
            menuItemCloseCurItem.Tag = newTabItem;
            menuItemCloseCurItem.Click += OnMenuItemCloseCurItem_Click;

            MenuItem menuItemCloseALL = new MenuItem();
            menuItemCloseALL.Header = "关闭所有选项卡";
            menuItemCloseALL.Click += OnMenuItemCloseALL_Click;

            MenuItem menuItemCloseExcept = new MenuItem();
            menuItemCloseExcept.Tag = newTabItem;
            menuItemCloseExcept.Header = "除此之外全部关闭";
            menuItemCloseExcept.Click += OnMenuItemCloseExcept_Click;

            ContextMenu contextMenu = new ContextMenu();
            contextMenu.Items.Add(menuItemCloseCurItem);
            contextMenu.Items.Add(menuItemCloseALL);
            contextMenu.Items.Add(menuItemCloseExcept);

            return contextMenu;
        }
        private void OnMenuItemCloseExcept_Click(object sender, RoutedEventArgs e)
        {
            MenuItem curMenuItem = sender as MenuItem;
            if (curMenuItem != null && curMenuItem.Tag != null)
            {
                for (int i = this.FrameTabCtrl.Items.Count - 1; i >= 0; i--)
                {
                    object tabItem = this.FrameTabCtrl.Items[i];
                    if (!object.ReferenceEquals(tabItem, curMenuItem.Tag))
                    {
                        this.FrameTabCtrl.Items.RemoveAt(i);
                    }
                }
            }
        }
        private void OnMenuItemCloseALL_Click(object sender, RoutedEventArgs e)
        {
            MenuItem curMenuItem = sender as MenuItem;
            if (curMenuItem != null)
            {
                curMenuItem.Click -= OnMenuItemCloseALL_Click;
            }
            this.FrameTabCtrl.Items.Clear();
        }
        private void OnMenuItemCloseCurItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem curMenuItem = sender as MenuItem;
            if (curMenuItem != null && curMenuItem.Tag != null)
            {
                if (this.FrameTabCtrl.Items.Contains(curMenuItem.Tag))
                {
                    this.FrameTabCtrl.Items.Remove(curMenuItem.Tag);
                    curMenuItem.Tag = null;
                    curMenuItem.Click -= OnMenuItemCloseCurItem_Click;
                }
            }
        }
        #endregion

        #region 外部接口
        bool IGoToPage.CloseFrameTabItem(string FrameName)
        {
            for (int i = 0; i < FrameTabCtrl.Items.Count; i++)
            {
                object item = FrameTabCtrl.Items[i];
                if (item != null && item is FrameworkElement)
                {
                    FrameworkElement fe = (FrameworkElement)item;
                    if (fe.Tag != null && fe.Tag.ToString() == FrameName)
                    {
                        FrameTabCtrl.Items.Remove(fe);
                        return true;
                    }
                }
            }
            return false;
        }
        object IGoToPage.FindContentInTabItem(string FrameName)
        {
            TabItem findTabItem = GetFTabItem(FrameName) as TabItem;
            if (findTabItem != null)
                return findTabItem.Content;
            return null;
        }
        object IGoToPage.SelectedFrameTabItem(string FrameName)
        {
            TabItem findTabItem = GetFTabItem(FrameName) as TabItem;
            if (findTabItem != null)
            {
                FrameTabCtrl.SelectedItem = findTabItem;
                return findTabItem.Content;
            }
            return null;
        }

        void IGoToPage.AddFrameTabItem(string FrameName, FrameworkElement Content, string IconResource)
        {
            this.AddFTabItemIfNotExit(FrameName, Content, IconResource);
        }
        #endregion
    }
}
