

namespace WPFVsCode.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    [TemplatePart(Name = PART_MinSizeBtn, Type = typeof(Button))]
    [TemplatePart(Name = PART_RestoreBtn, Type = typeof(Button))]
    [TemplatePart(Name = PART_MaxSizeBtn, Type = typeof(Button))]
    [TemplatePart(Name = PART_CloseBtn, Type = typeof(Button))]
    [TemplatePart(Name = PART_ICOBtn, Type = typeof(Button))]
    public class CusWindow:Window
    {
        static CusWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CusWindow), new FrameworkPropertyMetadata(typeof(CusWindow)));
        }
        private const string PART_MinSizeBtn = "PART_MinSizeBtn";
        private const string PART_RestoreBtn = "PART_RestoreBtn";
        private const string PART_CloseBtn = "PART_CloseBtn";
        private const string PART_MaxSizeBtn = "PART_MaxSizeBtn";
        private const string PART_ICOBtn = "PART_ICOBtn";



        public Brush WindowCaptionBackground
        {
            get { return (Brush)GetValue(WindowCaptionBackgroundProperty); }
            set { SetValue(WindowCaptionBackgroundProperty, value); }
        }
        public static readonly DependencyProperty WindowCaptionBackgroundProperty = DependencyProperty.Register(
            "WindowCaptionBackground", typeof(Brush), typeof(CusWindow), 
            new PropertyMetadata(new SolidColorBrush(Colors.WhiteSmoke)));




        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Button btnMinSize = GetTemplateChild(PART_MinSizeBtn) as Button;
            Button btnRestore = GetTemplateChild(PART_RestoreBtn) as Button;
            Button btnClose = GetTemplateChild(PART_CloseBtn) as Button;
            Button btnMaxSize = GetTemplateChild(PART_MaxSizeBtn) as Button;
            Button btnIcon = GetTemplateChild(PART_ICOBtn) as Button;

            if (btnMinSize!=null)
            {
                btnMinSize.Click += OnBtnMin_Click;
            }
            if(btnRestore!=null)
            {
                btnRestore.Click += OnBtnRestore_Click;
            }
            if (btnClose!=null)
            {
                btnClose.Click += OnBtnClose_Click;
            }
            if (btnMaxSize != null)
            {
                btnMaxSize.Click += OnBtnMaxSize_Click;
            }
            if(btnIcon!=null)
            {
                btnIcon.Click += OnBtnIcon_Click;
                btnIcon.MouseDoubleClick += OnBtnIcon_DoubleClick;

                if(this.Icon==null)
                {
                    string path = Process.GetCurrentProcess().MainModule.FileName;
                    this.Icon = ICONHelper.GetIcon(path, false, false);
                }
            }
        }

        private void OnBtnIcon_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void OnBtnIcon_Click(object sender, RoutedEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element == null)
                return;

            var point = WindowState == WindowState.Maximized ? new Point(0, element.ActualHeight)
                 : new Point(Left + BorderThickness.Left, element.ActualHeight + Top + BorderThickness.Top);
            point = element.TransformToAncestor(this).Transform(point);
            SystemCommands.ShowSystemMenu(this, point);
        }

        private void OnBtnMaxSize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        private void OnBtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close(); 
        }

        private void OnBtnRestore_Click(object sender, RoutedEventArgs e)
        {
            if(e.Source!=null)
            {
                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
                else if (WindowState == WindowState.Normal)
                    WindowState = WindowState.Maximized;
            }
        }

        private void OnBtnMin_Click(object sender, RoutedEventArgs e)
        {
            if(e.Source!=null)
            {
                this.WindowState = WindowState.Minimized;
            }
        }




        public object HeaderContent
        {
            get { return (object)GetValue(HeaderContentProperty); }
            set { SetValue(HeaderContentProperty, value); }
        }
        public static readonly DependencyProperty HeaderContentProperty =
            DependencyProperty.Register("HeaderContent", typeof(object), typeof(CusWindow), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure| FrameworkPropertyMetadataOptions.AffectsRender));


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            if (SizeToContent == SizeToContent.WidthAndHeight && System.Windows.Shell.WindowChrome.GetWindowChrome(this) != null)
            {
                InvalidateMeasure();
            }
        }

        #region 获取文件图标
        class ICONHelper
        {
            public static ImageSource GetIcon(string path, bool smallIcon, bool isDirectory)
            {
                // SHGFI_USEFILEATTRIBUTES takes the file name and attributes into account if it doesn't exist
                uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;
                if (smallIcon)
                    flags |= SHGFI_SMALLICON;

                uint attributes = FILE_ATTRIBUTE_NORMAL;
                if (isDirectory)
                    attributes |= FILE_ATTRIBUTE_DIRECTORY;

                SHFILEINFO shfi;
                if (0 != SHGetFileInfo(
                            path,
                            attributes,
                            out shfi,
                            (uint)Marshal.SizeOf(typeof(SHFILEINFO)),
                            flags))
                {
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                                shfi.hIcon,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());
                }
                return null;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct SHFILEINFO
            {
                public IntPtr hIcon;
                public int iIcon;
                public uint dwAttributes;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string szDisplayName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
                public string szTypeName;
            }

            [DllImport("shell32")]
            private static extern int SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint flags);

            private const uint FILE_ATTRIBUTE_READONLY = 0x00000001;
            private const uint FILE_ATTRIBUTE_HIDDEN = 0x00000002;
            private const uint FILE_ATTRIBUTE_SYSTEM = 0x00000004;
            private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
            private const uint FILE_ATTRIBUTE_ARCHIVE = 0x00000020;
            private const uint FILE_ATTRIBUTE_DEVICE = 0x00000040;
            private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
            private const uint FILE_ATTRIBUTE_TEMPORARY = 0x00000100;
            private const uint FILE_ATTRIBUTE_SPARSE_FILE = 0x00000200;
            private const uint FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400;
            private const uint FILE_ATTRIBUTE_COMPRESSED = 0x00000800;
            private const uint FILE_ATTRIBUTE_OFFLINE = 0x00001000;
            private const uint FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000;
            private const uint FILE_ATTRIBUTE_ENCRYPTED = 0x00004000;
            private const uint FILE_ATTRIBUTE_VIRTUAL = 0x00010000;

            private const uint SHGFI_ICON = 0x000000100;     // get icon
            private const uint SHGFI_DISPLAYNAME = 0x000000200;     // get display name
            private const uint SHGFI_TYPENAME = 0x000000400;     // get type name
            private const uint SHGFI_ATTRIBUTES = 0x000000800;     // get attributes
            private const uint SHGFI_ICONLOCATION = 0x000001000;     // get icon location
            private const uint SHGFI_EXETYPE = 0x000002000;     // return exe type
            private const uint SHGFI_SYSICONINDEX = 0x000004000;     // get system icon index
            private const uint SHGFI_LINKOVERLAY = 0x000008000;     // put a link overlay on icon
            private const uint SHGFI_SELECTED = 0x000010000;     // show icon in selected state
            private const uint SHGFI_ATTR_SPECIFIED = 0x000020000;     // get only specified attributes
            private const uint SHGFI_LARGEICON = 0x000000000;     // get large icon
            private const uint SHGFI_SMALLICON = 0x000000001;     // get small icon
            private const uint SHGFI_OPENICON = 0x000000002;     // get open icon
            private const uint SHGFI_SHELLICONSIZE = 0x000000004;     // get shell size icon
            private const uint SHGFI_PIDL = 0x000000008;     // pszPath is a pidl
            private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;     // use passed dwFileAttribute
        }
        #endregion
    }
}
