using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPFVsCode.Controls
{
    public class GroupBoxExtend:GroupBox
    {
        static GroupBoxExtend()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GroupBoxExtend), new FrameworkPropertyMetadata(typeof(GroupBoxExtend)));
        }

        #region 附加内容是否显示
        public Visibility AdditionContentVisibility
        {
            get { return (Visibility)GetValue(AdditionContentVisibilityProperty); }
            set { SetValue(AdditionContentVisibilityProperty, value); }
        }
        public static readonly DependencyProperty AdditionContentVisibilityProperty = DependencyProperty.Register(
            "AdditionContentVisibility", typeof(Visibility), typeof(GroupBoxExtend),
            new FrameworkPropertyMetadata(Visibility.Visible, 
                FrameworkPropertyMetadataOptions.AffectsMeasure| 
                FrameworkPropertyMetadataOptions.AffectsArrange));
        #endregion
        #region AdditionContent
        public object AdditionContent
        {
            get { return (object)GetValue(AdditionContentProperty); }
            set { SetValue(AdditionContentProperty, value); }
        }
        public static readonly DependencyProperty AdditionContentProperty = DependencyProperty.Register(
            "AdditionContent", typeof(object), typeof(GroupBoxExtend),
            new FrameworkPropertyMetadata(null, OnAdditionContentChanged_Callback));

        private static void OnAdditionContentChanged_Callback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GroupBoxExtend contentControl = (GroupBoxExtend)d;
            contentControl.SetValue(GroupBoxExtend.HasAdditionContentPropertyKey, (e.NewValue != null));
            contentControl.OnContentChanged(e.OldValue, e.NewValue);
        }
        #endregion
        #region HasAdditionContent
        private static readonly DependencyPropertyKey HasAdditionContentPropertyKey = DependencyProperty.RegisterReadOnly(
            "HasAdditionContent", typeof(bool), typeof(GroupBoxExtend), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None));
        public static readonly DependencyProperty HasAdditionContentProperty = GroupBoxExtend.HasAdditionContentPropertyKey.DependencyProperty;
        [Browsable(false)]
        [ReadOnly(true)]
        public bool HasAdditionContent
        {
            get
            {
                return (bool)base.GetValue(GroupBoxExtend.HasAdditionContentProperty);
            }
        }
        #endregion                
        #region AdditionContentTemplate
        public static readonly DependencyProperty AdditionContentTemplateProperty = DependencyProperty.Register(
           "AdditionContentTemplate", typeof(DataTemplate), typeof(GroupBoxExtend),
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
            "AdditionContentStringFormat", typeof(string), typeof(GroupBoxExtend),
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
            "AdditionContentTemplateSelector", typeof(DataTemplateSelector), typeof(GroupBoxExtend),
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
        

        #region AdditionCommand

        public static readonly DependencyProperty AdditionCommandProperty = DependencyProperty.Register(
            "AdditionCommand", typeof(ICommand), typeof(GroupBoxExtend), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnAdditionCommandChanged)));


        [Bindable(true)]
        [Category("Action")]
        [Localizability(LocalizationCategory.NeverLocalize)]
        public ICommand AdditionCommand
        {
            get
            {
                return (ICommand)base.GetValue(AdditionCommandProperty);
            }
            set
            {
                base.SetValue(AdditionCommandProperty, value);
            }
        }


        private static void OnAdditionCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GroupBoxExtend buttonBase = (GroupBoxExtend)d;

            ICommand oldCommand = (ICommand)e.OldValue;
            ICommand newCommand = (ICommand)e.NewValue;

            if (oldCommand != null)
            {
                buttonBase.UnhookCommand(oldCommand);
            }
            if (newCommand != null)
            {
                buttonBase.HookCommand(newCommand);
            }
        }
        private void UnhookCommand(ICommand command)
        {
            CanExecuteChangedEventManager.RemoveHandler(command, new EventHandler<EventArgs>(this.OnCanExecuteChanged));
            this.UpdateCanExecute();
        }
        private void HookCommand(ICommand command)
        {
            CanExecuteChangedEventManager.AddHandler(command, new EventHandler<EventArgs>(this.OnCanExecuteChanged));
            this.UpdateCanExecute();
        }

        private void OnCanExecuteChanged(object sender, EventArgs e)
        {
            this.UpdateCanExecute();
        }
        private void UpdateCanExecute()
        {
            //Do SomeThing Here
        }
        #endregion
        #region AdditonCommandParameter
        public static readonly DependencyProperty AdditonCommandParameterProperty = DependencyProperty.Register(
            "AdditonCommandParameter", typeof(object), typeof(GroupBoxExtend), new FrameworkPropertyMetadata(null));
        [Bindable(true)]
        [Category("Action")]
        [Localizability(LocalizationCategory.NeverLocalize)]
        public object AdditonCommandParameter
        {
            get
            {
                return base.GetValue(AdditonCommandParameterProperty);
            }
            set
            {
                base.SetValue(AdditonCommandParameterProperty, value);
            }
        }
        #endregion
        #region AdditionCommandTarget
        public static readonly DependencyProperty AdditionCommandTargetProperty = DependencyProperty.Register(
            "AdditionCommandTarget", typeof(IInputElement), typeof(GroupBoxExtend), 
            new FrameworkPropertyMetadata(null));
        [Bindable(true)]
        [Category("Action")]
        public IInputElement AdditionCommandTarget
        {
            get
            {
                return (IInputElement)base.GetValue(AdditionCommandTargetProperty);
            }
            set
            {
                base.SetValue(AdditionCommandTargetProperty, value);
            }
        }
        #endregion
    }
}
