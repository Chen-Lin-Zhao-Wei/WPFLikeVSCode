using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPFVsCode.Controls
{
    [TemplatePart(Name = PART_CloseBtn,Type = typeof(Button))]
    public class FrameTabItem : TabItem
    {
        private const string PART_CloseBtn = "PART_CloseBtn";

        #region Icon
        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof(object), typeof(FrameTabItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region 静态构造函数
        static FrameTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FrameTabItem), new FrameworkPropertyMetadata(typeof(FrameTabItem)));
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(FrameTabItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(FrameTabItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));
        }
        #endregion

        private Button btnClose = null;

        #region 运用模板
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (btnClose != null)
                btnClose.Click -= OnBtnClose_Click;
            btnClose = GetTemplateChild(PART_CloseBtn) as Button;
            if (btnClose != null)
                btnClose.Click += OnBtnClose_Click;
        }
        #endregion

        #region 关闭按钮逻辑
        private void OnBtnClose_Click(object sender, RoutedEventArgs e)
        {
            ItemsControl itemsOwner = ItemsControl.ItemsControlFromItemContainer(this);
            if (itemsOwner != null)
            {
                if (itemsOwner.ItemsSource != null && itemsOwner.ItemsSource is IList)
                {
                    IList dataSource = itemsOwner.ItemsSource as IList;
                    object currentItem = itemsOwner.ItemContainerGenerator.ItemFromContainer(this);
                    if (currentItem != null)
                    {
                        dataSource.Remove(currentItem);
                    }
                    else
                    {
                        int index = itemsOwner.ItemContainerGenerator.IndexFromContainer(this);
                        if (dataSource[index] == null)
                        {
                            dataSource.RemoveAt(index);
                        }
                    }
                }
                else if (itemsOwner.ItemsSource == null)
                {
                    if (itemsOwner.Items.Contains(this))
                    {
                        itemsOwner.Items.Remove(this);
                    }
                    else
                    {
                        object item = itemsOwner.ItemContainerGenerator.ItemFromContainer(this);
                        if (item != null)
                        {
                            itemsOwner.Items.Remove(item);
                        }
                        else
                        {
                            int idx = itemsOwner.ItemContainerGenerator.IndexFromContainer(this);
                            if (idx > 0 && itemsOwner.Items[idx] == null)
                            {
                                itemsOwner.Items.RemoveAt(idx);
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
