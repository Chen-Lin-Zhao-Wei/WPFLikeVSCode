using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFVsCode.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;


    [TemplatePart(Name =PART_Border, Type = typeof(Border))]
    public class SideTabItem : TabItem
    {
        private const string PART_Border = "PART_Border";

        #region 静态构造函数
        static SideTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SideTabItem), new FrameworkPropertyMetadata(typeof(SideTabItem)));
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(SideTabItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(SideTabItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));
        }
        #endregion

        #region Icon
        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof(object), typeof(SideTabItem),
            new PropertyMetadata(null)
        );
        #endregion

        #region TabControlParent
        private TabControl TabControlParent
        {
            get
            {
                return ItemsControl.ItemsControlFromItemContainer(this) as TabControl;
            }
        }
        #endregion

        Border partBorder;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (partBorder != null)
                partBorder.MouseLeftButtonDown -= OnPartBorder_MouseLeftButtonDown;
            partBorder = GetTemplateChild(PART_Border) as Border;
            if(partBorder!=null)
                partBorder.MouseLeftButtonDown += OnPartBorder_MouseLeftButtonDown;
        }
        private void OnPartBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {           
            SideTabControl parent = TabControlParent as SideTabControl;
            if (parent != null)
            {
                if (this.IsSelected == false)
                {
                    if (parent.IsOpen == false)
                        parent.IsOpen = true;
                    return;
                }
                parent.IsOpen = !parent.IsOpen;
            }
        }
    }
}
