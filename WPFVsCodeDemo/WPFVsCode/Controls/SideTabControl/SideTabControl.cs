using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Threading;

namespace WPFVsCode.Controls
{
    [TemplatePart(Name = PART_ContentPanel, Type = typeof(FrameworkElement))]
    public class SideTabControl: TabControl
    {
        private const string PART_ContentPanel = "PART_ContentPanel";
        FrameworkElement contentPanelHost = null;

        #region 静态构造函数
        static SideTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SideTabControl), new FrameworkPropertyMetadata(typeof(SideTabControl)));
        }
        #endregion
        
        #region 附加内容是否显示
        public Visibility AdditionContentVisibility
        {
            get { return (Visibility)GetValue(AdditionContentVisibilityProperty); }
            set { SetValue(AdditionContentVisibilityProperty, value); }
        }
        public static readonly DependencyProperty AdditionContentVisibilityProperty = DependencyProperty.Register(
            "AdditionContentVisibility", typeof(Visibility), typeof(SideTabControl),
            new FrameworkPropertyMetadata(Visibility.Visible,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsArrange));
        #endregion
        #region AdditionContent
        public object AdditionContent
        {
            get { return (object)GetValue(AdditionContentProperty); }
            set { SetValue(AdditionContentProperty, value); }
        }
        public static readonly DependencyProperty AdditionContentProperty = DependencyProperty.Register(
            "AdditionContent", typeof(object), typeof(SideTabControl),
            new FrameworkPropertyMetadata(null, OnAdditionContentChanged_Callback));

        private static void OnAdditionContentChanged_Callback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SideTabControl contentControl = (SideTabControl)d;
            contentControl.SetValue(SideTabControl.HasAdditionContentPropertyKey, (e.NewValue != null));
        }
        #endregion
        #region HasAdditionContent
        private static readonly DependencyPropertyKey HasAdditionContentPropertyKey = DependencyProperty.RegisterReadOnly(
            "HasAdditionContent", typeof(bool), typeof(SideTabControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None));
        public static readonly DependencyProperty HasAdditionContentProperty = SideTabControl.HasAdditionContentPropertyKey.DependencyProperty;
        [Browsable(false)]
        [ReadOnly(true)]
        public bool HasAdditionContent
        {
            get
            {
                return (bool)base.GetValue(SideTabControl.HasAdditionContentProperty);
            }
        }
        #endregion                
        #region AdditionContentTemplate
        public static readonly DependencyProperty AdditionContentTemplateProperty = DependencyProperty.Register(
           "AdditionContentTemplate", typeof(DataTemplate), typeof(SideTabControl),
           new FrameworkPropertyMetadata(null));
        [Bindable(true)]
        public DataTemplate AdditionContentTemplate
        {
            get
            {
                return (DataTemplate)base.GetValue(AdditionContentTemplateProperty);
            }
            set
            {
                base.SetValue(AdditionContentTemplateProperty, value);
            }
        }
        #endregion
        #region AdditionContentStringFormat
        public static readonly DependencyProperty AdditionContentStringFormatProperty = DependencyProperty.Register(
            "AdditionContentStringFormat", typeof(string), typeof(SideTabControl),
            new FrameworkPropertyMetadata(null));
        [Bindable(true)]
        public string AdditionContentStringFormat
        {
            get
            {
                return (string)base.GetValue(AdditionContentStringFormatProperty);
            }
            set
            {
                base.SetValue(AdditionContentStringFormatProperty, value);
            }
        }
        #endregion
        #region AdditionContentTemplateSelector
        public static readonly DependencyProperty AdditionContentTemplateSelectorProperty = DependencyProperty.Register(
            "AdditionContentTemplateSelector", typeof(DataTemplateSelector), typeof(SideTabControl),
            new FrameworkPropertyMetadata(null));

        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataTemplateSelector AdditionContentTemplateSelector
        {
            get
            {
                return (DataTemplateSelector)base.GetValue(AdditionContentTemplateSelectorProperty);
            }
            set
            {
                base.SetValue(AdditionContentTemplateSelectorProperty, value);
            }
        }
        #endregion


        private Storyboard showStoryboard;                  //显示情节
        private Storyboard showDirectStoryboard;            //直接显示情节
        private Storyboard hideStoryboard;                  //隐藏情节

        private DoubleAnimation AnimteHide_Width;             //关闭时       宽度动画
        private DoubleAnimation AnimteHide_Opacity;           //关闭时       透明动画
        private DoubleAnimation AnimteShow_Width;             //显示时       宽度动画
        private DoubleAnimation AnimteShow_Opacity;           //显示时       透明动画
        private DoubleAnimation AnimteShowDirect_Width;       //直接显示时   宽度动画
        private DoubleAnimation AnimteShowDirect_Opacity;     //直接显示时   透明动画



        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            contentPanelHost = GetTemplateChild(PART_ContentPanel) as FrameworkElement;


            contentPanelHost.ClearValue(FrameworkElement.WidthProperty);
            contentPanelHost.Width = ContentLength;

            //情节
            this.showStoryboard = this.GetTemplateChild("ShowStoryboard") as Storyboard;
            this.showDirectStoryboard = this.GetTemplateChild("ShowStoryboard") as Storyboard;
            this.hideStoryboard = this.GetTemplateChild("HideStoryboard") as Storyboard;
            
            //动画
            this.AnimteHide_Width           = this.GetTemplateChild("AnimteHide_Width")             as DoubleAnimation;
            this.AnimteHide_Opacity         = this.GetTemplateChild("AnimteHide_Opacity")           as DoubleAnimation;

            this.AnimteShow_Width           = this.GetTemplateChild("AnimteShow_Width")             as DoubleAnimation;
            this.AnimteShow_Opacity         = this.GetTemplateChild("AnimteShow_Opacity")           as DoubleAnimation;

            this.AnimteShowDirect_Width     = this.GetTemplateChild("AnimteShowDirect_Width")       as DoubleAnimation;
            this.AnimteShowDirect_Opacity   = this.GetTemplateChild("AnimteShowDirect_Opacity")     as DoubleAnimation;
        }

        #region AnimateOpacity          【Content动画切换时是否显示透明】
        public static readonly DependencyProperty AnimateOpacityProperty = DependencyProperty.Register(
            nameof(AnimateOpacity), typeof(bool), typeof(SideTabControl),
            new FrameworkPropertyMetadata(true, (d, args) => (d as SideTabControl)?.UpdateOpacityChange()));

        public bool AnimateOpacity
        {
            get
            {
                return (bool)this.GetValue(AnimateOpacityProperty);
            }
            set
            {
                this.SetValue(AnimateOpacityProperty, value);
            }
        }
        private void UpdateOpacityChange()
        {
            if (contentPanelHost == null || System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            if (!AnimateOpacity)
            {
                AnimteHide_Opacity.Duration = TimeSpan.FromSeconds(0);
                AnimteShow_Opacity.Duration = TimeSpan.FromSeconds(0);
                contentPanelHost.Opacity = 1;
            }
            else
            {
                AnimteHide_Opacity.Duration = TimeSpan.FromSeconds(1);
                AnimteShow_Opacity.Duration = TimeSpan.FromSeconds(1);
                if (!IsOpen)
                {
                    contentPanelHost.Opacity = 0;
                }
            }
        }
        #endregion

        #region AreAnimationsEnabled    【Content显示时是否使用动画】
        public bool AreAnimationsEnabled
        {
            get { return (bool)GetValue(AreAnimationsEnabledProperty); }
            set { SetValue(AreAnimationsEnabledProperty, value); }
        }
        public static readonly DependencyProperty AreAnimationsEnabledProperty = DependencyProperty.Register(
            "AreAnimationsEnabled", typeof(bool), typeof(SideTabControl),
            new PropertyMetadata(true));

        #endregion

        #region AllowFocusElement       【允许焦点元素】
        public static readonly DependencyProperty AllowFocusElementProperty
        = DependencyProperty.Register(nameof(AllowFocusElement),
                                      typeof(bool),
                                      typeof(SideTabControl),
                                      new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Flyout"/> should try focus an element.
        /// </summary>
        public bool AllowFocusElement
        {
            get
            {
                return (bool)this.GetValue(AllowFocusElementProperty); ;
            }
            set
            {
                this.SetValue(AllowFocusElementProperty, value);
            }
        }
        #endregion

        #region FocusedElement          【焦点元素】
        public static readonly DependencyProperty FocusedElementProperty = DependencyProperty.Register(
            "FocusedElement", typeof(FrameworkElement), typeof(SideTabControl), new UIPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the focused element.
        /// </summary>
        public FrameworkElement FocusedElement
        {
            get
            {
                return (FrameworkElement)this.GetValue(FocusedElementProperty);
            }
            set
            {
                this.SetValue(FocusedElementProperty, value);
            }
        }
        #endregion

        #region TryFocusElement     尝试聚焦焦点
        private void TryFocusElement()
        {
            if (this.AllowFocusElement)
            {
                // first focus itself
                this.Focus();

                if (this.FocusedElement != null)
                {
                    this.FocusedElement.Focus();
                }
                else if (this.contentPanelHost != null || !this.contentPanelHost.MoveFocus(new TraversalRequest(FocusNavigationDirection.First)))
                {
                   // this.flyoutHeader?.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                }
            }
        }
        #endregion

        #region IsShown                 【Content是否显示，只读】
        private static readonly DependencyPropertyKey IsShownPropertyKey = DependencyProperty.RegisterReadOnly(
           nameof(IsShown), typeof(bool), typeof(SideTabControl),
           new PropertyMetadata(true));
        public static readonly DependencyProperty IsShownProperty = IsShownPropertyKey.DependencyProperty;

        public bool IsShown
        {
            get
            {
                return (bool)this.GetValue(IsShownProperty);
            }
            protected set
            {
                this.SetValue(IsShownPropertyKey, value);
            }
        }
        #endregion

        #region IsOpen                  【Content是否打开】
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
            "IsOpen", typeof(bool), typeof(SideTabControl),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenPropertyChanged));

        public bool IsOpen
        {
            get
            {
                return (bool)this.GetValue(IsOpenProperty);
            }
            set
            {
                this.SetValue(IsOpenProperty, value);
            }
        }
        #endregion


        #region ContentLength
        public double ContentLength
        {
            get { return (double)GetValue(ContentLengthProperty); }
            set { SetValue(ContentLengthProperty, value); }
        }
        public static readonly DependencyProperty ContentLengthProperty = DependencyProperty.Register(
            "ContentLength", typeof(double), typeof(SideTabControl),
            new PropertyMetadata(200d, OnContentLength_ProperthChanged));

        private static void OnContentLength_ProperthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SideTabControl tabControl = d as SideTabControl;
            if(tabControl!=null && e.NewValue != e.OldValue)
            {
                if(tabControl.IsOpen==true && tabControl.IsShown == true)
                {
                    if(tabControl.contentPanelHost!=null)
                    {
                        tabControl.contentPanelHost.Width = (double)e.NewValue;
                    }
                }
            }
        }
        #endregion



        private static void OnIsOpenPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            SideTabControl tabCtrl = dependencyObject as SideTabControl;
            if (tabCtrl != null)
            {
                bool newValue = (bool)e.NewValue;
                if(newValue != (bool)e.OldValue)
                {
                    tabCtrl.ChangeIsOpen(newValue);
                }
            }
        }

        private void ChangeIsOpen(bool newValue)
        {
            Action openedChangedAction = () =>
            {
                if (AreAnimationsEnabled)
                {
                    if ((bool)newValue)
                    {
                        if (hideStoryboard != null)
                        {
                            //  不想让隐藏情节结束触发情节结束事件，否则将会在动画开始时contentPanelHost就被隐藏
                            // don't let the storyboard end it's completed event
                            // otherwise it could be hidden on start
                            hideStoryboard.Completed -= HideStoryboardCompleted;
                        }
                        contentPanelHost.Visibility = Visibility.Visible;

                        this.ApplyAnimation(AnimateOpacity);
                        this.TryFocusElement();

                        if (this.showStoryboard != null)
                        {
                            this.showStoryboard.Completed += ShowStoryboardCompleted;
                        }
                        else
                        {
                            Shown();    //没有显示情节，故触发
                        }
                    }
                    else
                    {
                        if (showStoryboard != null)
                        {
                            showStoryboard.Completed -= ShowStoryboardCompleted;
                        }

                        SetValue(IsShownPropertyKey, false);
                        if (hideStoryboard != null)
                        {
                            hideStoryboard.Completed += HideStoryboardCompleted;
                        }
                        else
                        {
                            Hide();
                        }
                    }

                    VisualStateManager.GoToState(this, newValue == false ? "Hide" : "Show", true);
                }
                else
                {
                    if (newValue)
                    {
                        contentPanelHost.Visibility = Visibility.Visible;
                        TryFocusElement();
                        Shown();    //禁止动画，  故触发
                    }
                    else
                    {
                        SetValue(IsShownPropertyKey, false);
                        Hide();
                    }
                    VisualStateManager.GoToState(this, newValue == false ? "HideDirect" : "ShowDirect", true);
                }
                RaiseEvent(new RoutedEventArgs(IsOpenChangedEvent));
            };

            Dispatcher.BeginInvoke(DispatcherPriority.Background, openedChangedAction);
        }


        //隐藏情节完成事件
        private void HideStoryboardCompleted(object sender, EventArgs e)
        {
            if (this.hideStoryboard != null)
            {
                this.hideStoryboard.Completed -= this.HideStoryboardCompleted;
            }
            this.Hide();
        }

        //显示情节完成事件
        private void ShowStoryboardCompleted(object sender, EventArgs e)
        {
            if (showStoryboard != null)
            {
                showStoryboard.Completed -= this.ShowStoryboardCompleted;
            }
            Shown();        //显示情节完成时触发
        }


        //关闭
        private void Hide()
        {
            // hide the flyout, we should get better performance and prevent showing the flyout on any resizing events
            contentPanelHost.Visibility = Visibility.Collapsed;
            this.RaiseEvent(new RoutedEventArgs(ClosingFinishedEvent));
        }

        //显示
        private void Shown()
        {
            this.SetValue(IsShownPropertyKey, true);
            this.RaiseEvent(new RoutedEventArgs(OpeningFinishedEvent));
        }


        #region ApplyAnimation
        private void ApplyAnimation(bool animateOpacity, bool resetShowFrame = true)
        {
            if (contentPanelHost == null || 
                AnimteHide_Width == null || AnimteHide_Opacity == null ||
                AnimteShow_Width == null || AnimteShow_Opacity == null ||
                AnimteShowDirect_Width == null || AnimteShowDirect_Opacity == null)
            {
                return;
            }



            if (animateOpacity)
            {
                if (!IsOpen)
                {
                    contentPanelHost.Opacity = 0;
                }
                AnimteShow_Opacity.Duration = TimeSpan.FromSeconds(1);
            }
            else
            {
                contentPanelHost.Opacity = 1;
                AnimteShow_Opacity.Duration = TimeSpan.FromSeconds(0);
            }
        }
        #endregion

        #region IsOpenChanged       【IsOpen改变事件】
        public static readonly RoutedEvent IsOpenChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(IsOpenChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SideTabControl));
        public event RoutedEventHandler IsOpenChanged
        {
            add
            {
                this.AddHandler(IsOpenChangedEvent, value);
            }
            remove
            {
                this.RemoveHandler(IsOpenChangedEvent, value);
            }
        }
        #endregion

        #region OpeningFinished     【打开完成事件】
        public static readonly RoutedEvent OpeningFinishedEvent = EventManager.RegisterRoutedEvent(
           "OpeningFinished", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SideTabControl));
        public event RoutedEventHandler OpeningFinished
        {
            add
            {
                this.AddHandler(OpeningFinishedEvent, value);
            }
            remove
            {
                this.RemoveHandler(OpeningFinishedEvent, value);
            }
        }
        #endregion
        
        #region ClosingFinished     【关闭完成事件】
        public static readonly RoutedEvent ClosingFinishedEvent = EventManager.RegisterRoutedEvent(
           "ClosingFinished", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SideTabControl));
        public event RoutedEventHandler ClosingFinished
        {
            add
            {
                this.AddHandler(ClosingFinishedEvent, value);
            }
            remove
            {
                this.RemoveHandler(ClosingFinishedEvent, value);
            }
        }
        #endregion
    }
}
