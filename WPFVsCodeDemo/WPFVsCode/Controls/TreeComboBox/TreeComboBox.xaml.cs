using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;

namespace WPFVsCode.Controls
{
    /// <summary>
    /// TreeComboBox.xaml 的交互逻辑
    /// </summary>
    public partial class TreeComboBox : UserControl
    {

        public TreeComboBox()
        {
            InitializeComponent();
            this.Loaded += OnThis_Loaded;

            EventManager.RegisterClassHandler(typeof(TreeView), TreeView.SelectedItemChangedEvent,
                new RoutedPropertyChangedEventHandler<object>(OnTreeView_SelectedItemChangedEvent));


            EventManager.RegisterClassHandler(typeof(TreeViewItem), TreeViewItem.MouseLeftButtonUpEvent,
                new MouseButtonEventHandler(OnTreeViewItem_MouseUP));

            EventManager.RegisterClassHandler(typeof(TreeViewItem), TreeViewItem.MouseLeftButtonDownEvent,
                new MouseButtonEventHandler(OnTreeViewItem_MouseDown));

            this.ComboBox.DropDownOpened += OnComboBox_DropDownOpened;
        }

        #region TreeViewItem的MouseDown事件
        private void OnTreeViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = sender as TreeViewItem;
            if (treeViewItem != null)
            {
                object commandParameter = TreeViewItemClickCommandParameter;
                if (commandParameter == null)
                    commandParameter = treeViewItem.DataContext;

                TreeView treeParent = FindVisualParent<TreeView>(treeViewItem);
                if (treeParent != null && treeParent == this.DropDownTreeViewer)
                {
                    RoutedCommand command = TreeViewItemClickCommand as RoutedCommand;
                    if (command != null)
                        command.Execute(commandParameter, TreeViewItemClickTarget);
                    else if (TreeViewItemClickCommand != null)
                        this.TreeViewItemClickCommand.Execute(commandParameter);
                }
            }
        }
        #endregion

        #region TreeItem的MouseUP事件
        private void OnTreeViewItem_MouseUP(object sender, MouseButtonEventArgs e)
        {
            if (this.ComboBox.IsDropDownOpen == true)
            {
                this.ComboBox.IsDropDownOpen = false;
            }
        }

        /// <summary>
        /// 利用VisualTreeHelper寻找指定依赖对象的父级对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T FindVisualParent<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj == null) return null;
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parent is T)
                    return parent as T;
                else
                    parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }
        #endregion

        #region Cobobox下拉时的事件
        private void OnComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (DropDownTreeViewer != null && this.SelectedItem != null)
            {
                if (DropDownTreeViewer.SelectedValue != this.SelectedItem)
                {
                    this.SetTreeViewSelectedItem(this.SelectedItem);
                }
            }
        }
        #endregion

        #region TreeView选中项发生改变
        private void OnTreeView_SelectedItemChangedEvent(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView treeView = sender as TreeView;
            if (treeView != this.DropDownTreeViewer) return;
            if (e.NewValue != null)
            {
                this.SelectedItem = e.NewValue;
                SetDisplayText(e.NewValue);
            }
            //写这个线程启动因为选中项之后下拉框不会关闭，如果在选中时关闭，将会重新触发一次此事件，导致选中的值回归选中前
            new System.Threading.Thread(() =>
            {
                System.Threading.Thread.Sleep(50);
                this.Dispatcher.Invoke(() =>
                {
                    this.ComboBox.IsDropDownOpen = false;
                });
            }).Start();
        }

        private void SetDisplayText(object item)
        {
            if (item == null)
            {
                this.ComboBox.Items.Clear();
                this.ComboBox.Text = "";
                return;
            }
            if (!string.IsNullOrEmpty(this.DisplayPath))
            {
                string[] propName = this.DisplayPath.Split('.');
                if (propName.Length > 0)
                {
                    object tmpVAlue = item;
                    for (int i = 0; i < propName.Length; i++)
                    {
                        PropertyInfo property = tmpVAlue.GetType().GetProperty(propName[i], BindingFlags.Instance | BindingFlags.Public);
                        if (property != null)
                        {
                            tmpVAlue = property.GetValue(tmpVAlue);
                        }
                        if (tmpVAlue == null || i == propName.Length - 1)
                        {
                            this.ComboBox.Items.Clear();
                            string value = (tmpVAlue ?? "NaN??").ToString();
                            this.ComboBox.Items.Add(value);
                            this.ComboBox.Text = value;
                            return;
                        }
                    }
                }
            }

            this.ComboBox.Items.Clear();
            this.ComboBox.Items.Add(this.SelectedItem.ToString());
            this.ComboBox.Text = this.ComboBox.Items[0].ToString();
        }
        #endregion

        #region 选中项
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem", typeof(object), typeof(TreeComboBox),
            new PropertyMetadata(null, OnSelectedItem_Changed));

        private static void OnSelectedItem_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeComboBox treeComboBox = d as TreeComboBox;
            if (treeComboBox != null && treeComboBox.DropDownTreeViewer != null)
            {
                treeComboBox.SetDisplayText(e.NewValue);
            }
            if (treeComboBox != null && e.NewValue != e.OldValue)
            {
                // Create a RoutedEventArgs instance.
                RoutedEventArgs args = new RoutedEventArgs(routedEvent: SelectedItemChangedEvent);
                treeComboBox.RaiseEvent(args);
            }
        }
        #endregion

        #region 设置选中项目
        private void SetTreeViewSelectedItem(object selectedValue)
        {
            if (selectedValue == null) return;
            if (this.DropDownTreeViewer != null)
            {
                if (this.DropDownTreeViewer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    SelectedTheTreeViewItem(selectedValue);
                }
                else
                {
                    EventHandler[] eventHandler = new EventHandler[1];
                    eventHandler[0] = new EventHandler((o1, e1) =>
                    {
                        if (this.DropDownTreeViewer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                        {
                            this.DropDownTreeViewer.ItemContainerGenerator.StatusChanged -= eventHandler[0];
                            SelectedTheTreeViewItem(selectedValue);
                        };
                    });
                    this.DropDownTreeViewer.ItemContainerGenerator.StatusChanged += eventHandler[0];
                }
            }
        }
        private void SelectedTheTreeViewItem(object selectedValue)
        {
            for (int i = 0; i < this.DropDownTreeViewer.ItemContainerGenerator.Items.Count; i++)
            {
                TreeViewItem treeViewItem = this.DropDownTreeViewer.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                if (treeViewItem != null)
                {
                    object item = this.DropDownTreeViewer.ItemContainerGenerator.ItemFromContainer(treeViewItem);
                    if (item == selectedValue)
                    {
                        treeViewItem.IsSelected = true;
                        TunThreadSetIsOpen();
                        return;
                    }
                    else
                    {
                        SetTreeViewItemSelected(treeViewItem, selectedValue);
                    }
                }
            }
        }
        private void TunThreadSetIsOpen()
        {
            //这个事件实在下拉时候发生的，因为选中了之后会触发下拉框自动回收，这就很操蛋，只能延迟后重新打开
            new System.Threading.Thread(() =>
            {
                System.Threading.Thread.Sleep(100);
                this.Dispatcher.Invoke(() =>
                {
                    if (this.ComboBox.IsDropDownOpen == false)
                    {
                        this.ComboBox.IsDropDownOpen = true;
                    }
                });
            }).Start();
        }

        void SetTreeViewItemSelected(TreeViewItem treeViewItem, object selectedValue)
        {
            if (treeViewItem == null || selectedValue == null) return;
            if (treeViewItem.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                for (int i = 0; i < treeViewItem.ItemContainerGenerator.Items.Count; i++)
                {
                    TreeViewItem childTreeViewItem = treeViewItem.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                    if (childTreeViewItem != null)
                    {
                        object item = treeViewItem.ItemContainerGenerator.ItemFromContainer(childTreeViewItem);
                        if (item == selectedValue)
                        {
                            childTreeViewItem.IsSelected = true;
                            TunThreadSetIsOpen();
                            return;
                        }
                        else
                        {
                            SetTreeViewItemSelected(childTreeViewItem, selectedValue);
                        }
                    }
                }
            }
            else
            {
                EventHandler[] eventHandler = new EventHandler[1];
                eventHandler[0] = new EventHandler((o1, e1) =>
                {
                    if (treeViewItem.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                    {
                        treeViewItem.ItemContainerGenerator.StatusChanged -= eventHandler[0];
                        SetTreeViewItemSelected(treeViewItem, selectedValue);
                    };
                });
                treeViewItem.ItemContainerGenerator.StatusChanged += eventHandler[0];
            }
        }
        #endregion

        #region ItemsSource
        /// <summary>
        /// 数据源
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IEnumerable), typeof(TreeComboBox),
            new PropertyMetadata(null, OnItemsSource_Changed));

        private static void OnItemsSource_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeComboBox treeComboBox = d as TreeComboBox;
            if (treeComboBox != null && treeComboBox.DropDownTreeViewer != null)
            {
                treeComboBox.DropDownTreeViewer.ItemsSource = e.NewValue as IEnumerable;
            }
        }
        #endregion

        #region 显示项
        public string DisplayPath
        {
            get { return (string)GetValue(DisplayPathProperty); }
            set { SetValue(DisplayPathProperty, value); }
        }
        public static readonly DependencyProperty DisplayPathProperty = DependencyProperty.Register(
            "DisplayPath", typeof(string), typeof(TreeComboBox),
            new PropertyMetadata("", OnDisplayPath_Changed));

        private static void OnDisplayPath_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeComboBox treeCmb = d as TreeComboBox;
            if (treeCmb != null)
            {
                treeCmb.SetDisplayText(treeCmb.SelectedItem);
            }
        }
        #endregion

        #region TreeItemStyle
        public Style TreeItemStyle
        {
            get { return (Style)GetValue(TreeItemStyleProperty); }
            set { SetValue(TreeItemStyleProperty, value); }
        }
        public static readonly DependencyProperty TreeItemStyleProperty = DependencyProperty.Register(
            "TreeItemStyle", typeof(Style), typeof(TreeComboBox),
            new PropertyMetadata(null, OnTreeItemStyle_Changed));

        private static void OnTreeItemStyle_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeComboBox treeCombo = d as TreeComboBox;
            if (treeCombo != null && treeCombo.DropDownTreeViewer != null)
            {
                treeCombo.DropDownTreeViewer.ItemContainerStyle = e.NewValue as Style;
            }
        }
        #endregion

        #region 树子项显示模板
        public HierarchicalDataTemplate TreeItemItemTemplate
        {
            get { return (HierarchicalDataTemplate)GetValue(TreeItemItemTemplateProperty); }
            set { SetValue(TreeItemItemTemplateProperty, value); }
        }
        public static readonly DependencyProperty TreeItemItemTemplateProperty = DependencyProperty.Register(
            "TreeItemItemTemplate", typeof(HierarchicalDataTemplate), typeof(TreeComboBox),
            new PropertyMetadata(null, OnTreeItemItemTemplate_Changed));

        private static void OnTreeItemItemTemplate_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeComboBox treeComboBox = d as TreeComboBox;
            if (treeComboBox != null && treeComboBox.DropDownTreeViewer != null)
            {
                treeComboBox.DropDownTreeViewer.ItemTemplate = e.NewValue as HierarchicalDataTemplate;
            }
        }
        #endregion

        TreeView DropDownTreeViewer = null;

        #region Load时候获取TreeView控件并设置数据
        private void OnThis_Loaded(object sender, RoutedEventArgs e)
        {
            if (DropDownTreeViewer == null)
            {
                DropDownTreeViewer = this.ComboBox.Template.FindName("InnerTreeView", this.ComboBox) as TreeView;
                if (DropDownTreeViewer != null)
                {
                    if (DropDownTreeViewer.ItemTemplate != this.TreeItemItemTemplate)
                    {
                        DropDownTreeViewer.ItemTemplate = this.TreeItemItemTemplate;
                    }
                    if (DropDownTreeViewer.ItemsSource != this.ItemsSource)
                    {
                        DropDownTreeViewer.Items.Clear();
                        DropDownTreeViewer.ItemsSource = this.ItemsSource;
                    }
                    if (DropDownTreeViewer.ItemContainerStyle != this.TreeItemStyle)
                    {
                        DropDownTreeViewer.ItemContainerStyle = this.TreeItemStyle;
                    }
                }
            }
            if (this.SelectedItem != null && string.IsNullOrEmpty(this.ComboBox.Text))
            {
                SetDisplayText(this.SelectedItem);
            }
            else if (this.SelectedItem == null)
            {
                SetDisplayText(null);
            }
        }
        #endregion

        #region 声明选中项发生改变事件
        // Register a custom routed event using the Bubble routing strategy.
        public static readonly RoutedEvent SelectedItemChangedEvent = EventManager.RegisterRoutedEvent(
            name: "SelectedItemChanged",
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(TreeComboBox));

        // Provide CLR accessors for assigning an event handler.
        public event RoutedEventHandler SelectedItemChanged
        {
            add { AddHandler(SelectedItemChangedEvent, value); }
            remove { RemoveHandler(SelectedItemChangedEvent, value); }
        }
        #endregion

        #region 点击命令
        /// <summary>
        /// 此命令会将
        /// </summary>
        public ICommand TreeViewItemClickCommand
        {
            get { return (ICommand)GetValue(TreeViewItemClickCommandProperty); }
            set { SetValue(TreeViewItemClickCommandProperty, value); }
        }
        public static readonly DependencyProperty TreeViewItemClickCommandProperty =
            DependencyProperty.Register("TreeViewItemClickCommand", typeof(ICommand), typeof(TreeComboBox), new PropertyMetadata(null));


        public object TreeViewItemClickCommandParameter
        {
            get { return (object)GetValue(TreeViewItemClickCommandParameterProperty); }
            set { SetValue(TreeViewItemClickCommandParameterProperty, value); }
        }
        public static readonly DependencyProperty TreeViewItemClickCommandParameterProperty =
            DependencyProperty.Register("TreeViewItemClickCommandParameter", typeof(object), typeof(TreeComboBox), new PropertyMetadata(null));

        public IInputElement TreeViewItemClickTarget
        {
            get { return (IInputElement)GetValue(TreeViewItemClickTargetProperty); }
            set { SetValue(TreeViewItemClickTargetProperty, value); }
        }
        public static readonly DependencyProperty TreeViewItemClickTargetProperty =
            DependencyProperty.Register("TreeViewItemClickTarget", typeof(IInputElement), typeof(TreeComboBox), new PropertyMetadata(null));
        #endregion
    }
}
