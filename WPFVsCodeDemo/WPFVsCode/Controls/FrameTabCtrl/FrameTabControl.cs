using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WPFVsCode.Helper;

namespace WPFVsCode.Controls
{
    [TemplatePart(Name = PART_ToogleBtnDropDown, Type = typeof(ToggleButton))]
    public class FrameTabControl:TabControl
    {
        private const string PART_ToogleBtnDropDown = "PART_ToogleBtnDropDown";

        #region 静态构造函数
        static FrameTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FrameTabControl), new FrameworkPropertyMetadata(typeof(FrameTabControl)));
        }
        #endregion

        private ToggleButton toggleBtnDropDown;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if(toggleBtnDropDown!=null)
            {
                toggleBtnDropDown.Checked    -= OnToggleDropDownBtn_Checked;
                toggleBtnDropDown.Unchecked  -= OnToggleDropDownBtn_UnChecked;
                toggleBtnDropDown.MouseEnter -= OnToggleDropDownBtn_MouseEnter;
                toggleBtnDropDown.MouseLeave -= OnToggleDropDownBtn_MouseLeave;
            }

            toggleBtnDropDown = GetTemplateChild(PART_ToogleBtnDropDown) as ToggleButton;

            if(toggleBtnDropDown!=null)
            {
                toggleBtnDropDown.Checked += OnToggleDropDownBtn_Checked;
                toggleBtnDropDown.Unchecked += OnToggleDropDownBtn_UnChecked;
                toggleBtnDropDown.MouseEnter += OnToggleDropDownBtn_MouseEnter;
                toggleBtnDropDown.MouseLeave += OnToggleDropDownBtn_MouseLeave;
            }
        }

        private void OnToggleDropDownBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            if(toggleBtnDropDown.ContextMenu!=null)
            {
                toggleBtnDropDown.ContextMenu.Closed -= OnDropDownCtxMenu_Closed;
            }
        }

        private void OnToggleDropDownBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            if (toggleBtnDropDown.ContextMenu != null)
            {
                toggleBtnDropDown.ContextMenu.Closed += OnDropDownCtxMenu_Closed;
            }
        }

        private void OnToggleDropDownBtn_UnChecked(object sender, RoutedEventArgs e)
        {
            if (this.toggleBtnDropDown != null)
            {
                if (toggleBtnDropDown.ContextMenu == null)
                {
                    toggleBtnDropDown.ContextMenu = new ContextMenu();
                }
                ContextMenu ctxMenu = toggleBtnDropDown.ContextMenu;                
                ClearCtxMenuItems(ctxMenu);
            }

            if(toggleBtnDropDown.ContextMenu != null && toggleBtnDropDown.ContextMenu.IsOpen==true)
            {
                toggleBtnDropDown.ContextMenu.Closed -= OnDropDownCtxMenu_Closed;
                toggleBtnDropDown.ContextMenu.IsOpen = false;
                toggleBtnDropDown.ContextMenu.Closed += OnDropDownCtxMenu_Closed;
            }
        }
        private void ClearCtxMenuItems(ContextMenu contextMenu)
        {
            for (int i = contextMenu.Items.Count - 1; i >= 0; i--)
            {
                MenuItem menuItem = contextMenu.Items[i] as MenuItem;
                if (menuItem != null)
                {
                    if(menuItem.ToolTip !=null)
                    {
                        Grid tooltipGrid = menuItem.ToolTip as Grid;

                        VisualBrush visualBrush = tooltipGrid.Background as VisualBrush;
                        if (visualBrush != null) visualBrush.Visual = null;
                        tooltipGrid.Background = null;

                        tooltipGrid.ClearValue(MenuItem.WidthProperty);
                        tooltipGrid.ClearValue(MenuItem.HeightProperty);
                    }
                    menuItem.Tag = null;


                    menuItem.ClearValue(MenuItem.VisibilityProperty);
                    menuItem.Visibility = Visibility.Collapsed;

                    menuItem.ClearValue(MenuItem.IsEnabledProperty);
                    menuItem.IsEnabled = false;

                    menuItem.Click -= OnDropDownMenuItem_Click;

                    contextMenu.Items.Remove(menuItem);
                }
                else
                {
                    if(contextMenu.Items[i] is FrameworkElement)
                    {
                        (contextMenu.Items[i] as FrameworkElement).Tag = null;
                    }
                    contextMenu.Items.RemoveAt(i);
                }
            }
        }
        private void OnToggleDropDownBtn_Checked(object sender, RoutedEventArgs e)
        {
            if(this.toggleBtnDropDown!=null)
            {
                if(toggleBtnDropDown.ContextMenu==null)
                {
                    toggleBtnDropDown.ContextMenu = new ContextMenu();
                    toggleBtnDropDown.ContextMenu.PlacementTarget = toggleBtnDropDown;
                    toggleBtnDropDown.ContextMenu.Placement = PlacementMode.Bottom;
                    toggleBtnDropDown.ContextMenu.Closed += OnDropDownCtxMenu_Closed;
                }

                ContextMenu ctxMenu = toggleBtnDropDown.ContextMenu;
                ClearCtxMenuItems(ctxMenu);

                int unknownCount = 0;
                for (int i = 0; i < this.Items.Count; i++)
                {
                    TabItem tabItem = this.ItemContainerGenerator.ContainerFromIndex(i) as TabItem;
                    if(tabItem!=null)
                    {
                        MenuItem menuItem = new MenuItem();

                        #region menuItem.Header
                        if (tabItem.Header != null && tabItem.Header is String)
                        {
                            menuItem.Header = tabItem.Header;
                        }
                        else
                        {
                            List<TextBlock> texts = WPFUIElementHelper.GetVisualChildren<TextBlock>(tabItem);

                            if (texts.Count > 0)
                            {
                                menuItem.Header = string.Join(" ", texts.Select(p => p.Text).ToArray());
                            }
                            else
                            {
                                unknownCount++;
                                menuItem.Header = "Known Name" + unknownCount;

                                Grid toolTipGrid = new Grid() { Margin = new Thickness(-10, -5, -10, -5) };
                                #region toolTipGrid
                                toolTipGrid.Background = new VisualBrush(tabItem);

                                Binding bindingWidth = new Binding();
                                bindingWidth.Path = new PropertyPath("ActualWidth");
                                bindingWidth.Source = tabItem;
                                toolTipGrid.SetBinding(Grid.WidthProperty, bindingWidth);

                                Binding bindingHeight = new Binding();
                                bindingHeight.Path = new PropertyPath("ActualHeight");
                                bindingHeight.Source = tabItem;
                                toolTipGrid.SetBinding(Grid.HeightProperty, bindingHeight);
                                #endregion
                                menuItem.ToolTip = toolTipGrid;
                            }
                        }
                        #endregion

                        Binding bindingVisibility = new Binding("Visibility");
                        bindingVisibility.Source = tabItem;
                        menuItem.SetBinding(MenuItem.VisibilityProperty, bindingVisibility);

                        Binding bindingIsEnabled =  new Binding("IsEnabled");
                        bindingIsEnabled.Source = tabItem;
                        menuItem.SetBinding(MenuItem.IsEnabledProperty, bindingIsEnabled);

                        menuItem.Tag = tabItem;
                        menuItem.Click += OnDropDownMenuItem_Click;

                        ctxMenu.Items.Add(menuItem);
                    }
                }

                if(ctxMenu.Items.Count>0)
                {
                    ctxMenu.IsOpen = true;
                }
                else
                {
                    toggleBtnDropDown.Unchecked -= OnToggleDropDownBtn_UnChecked;
                    toggleBtnDropDown.IsChecked = false;
                    toggleBtnDropDown.Unchecked += OnToggleDropDownBtn_UnChecked;
                }
            }
        }

        private void OnDropDownMenuItem_Click(object sender, RoutedEventArgs e)
        { 
            MenuItem menuitem = sender as MenuItem;
            if(menuitem !=null  && menuitem.Tag!=null)
            {
                TabItem tabitem = menuitem.Tag as TabItem;
                int idx = this.ItemContainerGenerator.IndexFromContainer(tabitem);
                this.SelectedIndex = idx;
            }
        }

        private void OnDropDownCtxMenu_Closed(object sender, RoutedEventArgs e)
        {
            if(toggleBtnDropDown!=null)
            {
                if(toggleBtnDropDown.IsChecked==true)
                {
                    toggleBtnDropDown.IsChecked = false;
                }
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);
            TabItem removeTabItem = element as TabItem;
            if (removeTabItem != null)
            {
                removeTabItem.Header = null;
                removeTabItem.DataContext = null;
                removeTabItem.Tag = null;

                IDisposable disposable = removeTabItem.Content as IDisposable;
                if(disposable!=null)
                {
                    disposable.Dispose();
                }
                removeTabItem.Content = null;
            }
        }
    }
}
