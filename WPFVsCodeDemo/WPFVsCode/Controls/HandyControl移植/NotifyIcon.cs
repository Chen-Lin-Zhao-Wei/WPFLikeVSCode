// 开源项目         https://github.com/HandyOrg/HandyControl/  
// 来源：git项目：   https://github.com/HandyOrg/HandyControl/releases?page=2
// 开源协议：        MIT
// 版本：            HandyControl-2.1.0
// 时间：            2022-05-06

namespace WPFVsCode.Controls
{
    /*用法
           <NotifyIcon:NotifyIcon Text="111" IsBlink="{Binding ElementName=ChkIsBLik,Path=IsChecked}" Click="NotifyIcon_Click"
                    Visibility="{Binding ElementName=ChkVisb,Path=IsChecked,Converter={StaticResource BoolToVisbCrv}}"
                        Icon="n10.ico">

        这里去掉试试
            <NotifyIcon:NotifyIcon.ContextContent>
                <Border CornerRadius="4" Margin="10" Background="White" >
                    <StackPanel VerticalAlignment="Center" Margin="16">
                        <Path Width="100" Height="100" Fill="#f06632"/>
                        <StackPanel Margin="0,16,0,0" HorizontalAlignment="Center" Orientation="Horizontal">
                            <Button Content="111"/>
                        </StackPanel>
                    </StackPanel>
                </Border>
            </NotifyIcon:NotifyIcon.ContextContent>
            <NotifyIcon:NotifyIcon.ContextMenu>
                <ContextMenu>
                    <MenuItem Header="1111"/>
                    <MenuItem Header="222"/>
                </ContextMenu>
            </NotifyIcon:NotifyIcon.ContextMenu>
        </NotifyIcon:NotifyIcon>
    */

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Threading;
    using System.Collections.ObjectModel;
    using System.Security;
    using System.Windows.Media.Imaging;
    using System.Runtime.ConstrainedExecution;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Security.Permissions;
    using System.Runtime.Versioning;
    using Microsoft.Win32.SafeHandles;

    public class NotifyIcon : FrameworkElement, IDisposable
    {
        #region 默认属性显示，Visib改变时重新更新图标
        static NotifyIcon()
        {   //重写，默认显示
            VisibilityProperty.OverrideMetadata(typeof(NotifyIcon), new PropertyMetadata(Visibility.Visible, OnVisibilityChanged));
        }
        private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = (NotifyIcon)d;
            var v = (Visibility)e.NewValue;
            if (v == Visibility.Visible)
            {
                if (ctl._iconCurrentHandle == IntPtr.Zero)
                {
                    ctl.OnIconChanged();
                }
                ctl.UpdateIcon(true);
            }
            else if (ctl._iconCurrentHandle != IntPtr.Zero)
            {
                ctl.UpdateIcon(false);
            }
        }
        #endregion

        #region 构造函数，析构函数
        private readonly int _id;   //当前托盘Id
        private static int NextId;  //静态标识，每实例化一个NotifyIcon加1
        public NotifyIcon()
        {
            _id = ++NextId;
            _callback = Callback;
            Loaded += (s, e) => Init();
            if (Application.Current != null) Application.Current.Exit += (s, e) => Dispose();
        }

        ~NotifyIcon()
        {
            Dispose(false);
        }
        #endregion

        #region Load事件触发初始化
        private readonly WndProc _callback;     //创建托盘窗口时加入的钩子回调
        private IntPtr _messageWindowHandle;    //创建后的消息窗口句柄
        private int _wmTaskbarCreated;          //是否加入托盘
        private string _windowClassName;        //注入的托盘窗口类名

        //初始化
        public void Init()
        {
            RegisterClass();
            if (Visibility == Visibility.Visible)
            {
                OnIconChanged();
                UpdateIcon(true);
            }

            _dispatcherTimerPos = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _dispatcherTimerPos.Tick += DispatcherTimerPos_Tick;
        }

        //注册WNDCLASS
        private void RegisterClass()
        {
            _windowClassName = $"Alarm.Main.WPFClient_{Guid.NewGuid()}";
            var wndclass = new WNDCLASS
            {
                style = 0,
                lpfnWndProc = _callback,
                cbClsExtra = 0,
                cbWndExtra = 0,
                hInstance = IntPtr.Zero,
                hIcon = IntPtr.Zero,
                hCursor = IntPtr.Zero,
                hbrBackground = IntPtr.Zero,
                lpszMenuName = "",
                lpszClassName = _windowClassName
            };
            UnsafeNativeMethods.RegisterClass(wndclass);
            _wmTaskbarCreated = NativeMethods.RegisterWindowMessage("TaskbarCreated");           
            _messageWindowHandle = UnsafeNativeMethods.CreateWindowEx(0, _windowClassName, "", 0, 0, 0, 1, 1,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }


        //定时器触发不断检测光标进入和退出，默认200ms
        private DispatcherTimer _dispatcherTimerPos;

        private bool _isMouseOver;  //关闭是否光标进入
        //定时器触发MouseEnter事件和MouseLeaveEvent事件
        private void DispatcherTimerPos_Tick(object sender, EventArgs e)
        {
            if (CheckMouseIsEnter())
            {
                if (!_isMouseOver)
                {
                    _isMouseOver = true;
                    RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount)
                    {
                        RoutedEvent = MouseEnterEvent
                    });
                    _dispatcherTimerPos.Interval = TimeSpan.FromMilliseconds(500);
                }
            }
            else
            {
                _dispatcherTimerPos.Stop();
                _isMouseOver = false;
                RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount)
                {
                    RoutedEvent = MouseLeaveEvent
                });
            }
        }
        //检查光标是否进入托盘图标
        private bool CheckMouseIsEnter()
        {
            NativeMethods.RECT rectNotify;
            var isTrue = FindNotifyIcon(out rectNotify);
            if (!isTrue) return false;
            NativeMethods.POINT point;
            NativeMethods.GetCursorPos(out point);
            if (point.X >= rectNotify.Left && point.X <= rectNotify.Right &&
                point.Y >= rectNotify.Top && point.Y <= rectNotify.Bottom)
            {
                return true;
            }
            return false;
        }
        //查找托盘图标区域位置
        private bool FindNotifyIcon(out NativeMethods.RECT rect)
        {
            var rectNotify = new NativeMethods.RECT();
            var hTrayWnd = FindTrayToolbarWindow();
            var isTrue = FindNotifyIcon(hTrayWnd, ref rectNotify);
            if (!isTrue)
            {
                hTrayWnd = FindTrayToolbarOverFlowWindow();
                isTrue = FindNotifyIcon(hTrayWnd, ref rectNotify);
            }
            rect = rectNotify;
            return isTrue;
        }

        //referenced from http://www.cnblogs.com/sczmzx/p/5158127.html
        private IntPtr FindTrayToolbarWindow()
        {
            var hWnd = NativeMethods.FindWindow("Shell_TrayWnd", null);
            if (hWnd != IntPtr.Zero)
            {
                hWnd = NativeMethods.FindWindowEx(hWnd, IntPtr.Zero, "TrayNotifyWnd", null);
                if (hWnd != IntPtr.Zero)
                {
                    hWnd = NativeMethods.FindWindowEx(hWnd, IntPtr.Zero, "SysPager", null);
                    if (hWnd != IntPtr.Zero)
                    {
                        hWnd = NativeMethods.FindWindowEx(hWnd, IntPtr.Zero, "ToolbarWindow32", null);

                    }
                }
            }
            return hWnd;
        }

        //referenced from http://www.cnblogs.com/sczmzx/p/5158127.html
        private IntPtr FindTrayToolbarOverFlowWindow()
        {
            var hWnd = NativeMethods.FindWindow("NotifyIconOverflowWindow", null);
            if (hWnd != IntPtr.Zero)
            {
                hWnd = NativeMethods.FindWindowEx(hWnd, IntPtr.Zero, "ToolbarWindow32", null);
            }
            return hWnd;
        }

        //referenced from http://www.cnblogs.com/sczmzx/p/5158127.html
        private bool FindNotifyIcon(IntPtr hTrayWnd, ref NativeMethods.RECT rectNotify)
        {
            NativeMethods.RECT rectTray;
            NativeMethods.GetWindowRect(hTrayWnd, out rectTray);
            var count = (int)NativeMethods.SendMessage(hTrayWnd, NativeMethods.TB_BUTTONCOUNT, 0, IntPtr.Zero);

            var isFind = false;
            if (count > 0)
            {
                uint trayPid;
                NativeMethods.GetWindowThreadProcessId(hTrayWnd, out trayPid);
                var hProcess = NativeMethods.OpenProcess(NativeMethods.ProcessAccess.VMOperation |
                    NativeMethods.ProcessAccess.VMRead | NativeMethods.ProcessAccess.VMWrite, false, trayPid);
                var address = NativeMethods.VirtualAllocEx(hProcess, IntPtr.Zero, 1024, NativeMethods.AllocationType.Commit, NativeMethods.MemoryProtection.ReadWrite);

                var btnData = new NativeMethods.TBBUTTON();
                var trayData = new NativeMethods.TRAYDATA();
                var handel = Process.GetCurrentProcess().Id;

                for (uint i = 0; i < count; i++)
                {
                    NativeMethods.SendMessage(hTrayWnd, NativeMethods.TB_GETBUTTON, i, address);
                    int _;
                    var isTrue = NativeMethods.ReadProcessMemory(hProcess, address, out btnData, Marshal.SizeOf(btnData), out _);
                    if (!isTrue) continue;
                    if (btnData.dwData == IntPtr.Zero)
                    {
                        btnData.dwData = btnData.iString;
                    }
                    NativeMethods.ReadProcessMemory(hProcess, btnData.dwData, out trayData, Marshal.SizeOf(trayData), out _);

                    uint dwProcessId;
                    NativeMethods.GetWindowThreadProcessId(trayData.hwnd, out dwProcessId);
                    if (dwProcessId == (uint)handel)
                    {
                        var rect = new NativeMethods.RECT();
                        var lngRect = NativeMethods.VirtualAllocEx(hProcess, IntPtr.Zero, Marshal.SizeOf(typeof(Rect)), NativeMethods.AllocationType.Commit, NativeMethods.MemoryProtection.ReadWrite);
                        NativeMethods.SendMessage(hTrayWnd, NativeMethods.TB_GETITEMRECT, i, lngRect);
                        NativeMethods.ReadProcessMemory(hProcess, lngRect, out rect, Marshal.SizeOf(rect), out _);

                        NativeMethods.VirtualFreeEx(hProcess, lngRect, Marshal.SizeOf(rect), NativeMethods.FreeType.Decommit);
                        NativeMethods.VirtualFreeEx(hProcess, lngRect, 0, NativeMethods.FreeType.Release);

                        var left = rectTray.Left + rect.Left;
                        var top = rectTray.Top + rect.Top;
                        var botton = rectTray.Top + rect.Bottom;
                        var right = rectTray.Left + rect.Right;
                        rectNotify = new NativeMethods.RECT
                        {
                            Left = left,
                            Right = right,
                            Top = top,
                            Bottom = botton
                        };
                        isFind = true;
                        break;
                    }
                }
                NativeMethods.VirtualFreeEx(hProcess, address, 0x4096, NativeMethods.FreeType.Decommit);
                NativeMethods.VirtualFreeEx(hProcess, address, 0, NativeMethods.FreeType.Release);
                NativeMethods.CloseHandle(hProcess);
            }
            return isFind;
        }
        #endregion

        #region Text属性
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(NotifyIcon), new PropertyMetadata(default(string)));

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty); ;
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }
        #endregion

        #region 托盘图标
        private ImageSource _icon;                  //ICon的值
        private IconHandle _iconHandle;
        private IntPtr _iconCurrentHandle;
        private IntPtr _iconDefaultHandle;
        private bool _isTransparent;                //托盘图标是否透明

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof(ImageSource), typeof(NotifyIcon), new PropertyMetadata(default(ImageSource), OnIconChanged));

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = (NotifyIcon)d;
            ctl._icon = (ImageSource)e.NewValue;
            ctl.OnIconChanged();
        }

        public ImageSource Icon
        {
            get
            {
                return (ImageSource)GetValue(IconProperty);
            }
            set
            {
                SetValue(IconProperty, value);
            }
        }
        private void OnIconChanged()
        {
            if (_icon != null)
            {
                IconHandle _;
                IconHelper.GetIconHandlesFromImageSource(_icon, out _, out _iconHandle);
                _iconCurrentHandle = _iconHandle.CriticalGetHandle();
            }
            else
            {
                if (_iconDefaultHandle == IntPtr.Zero)
                {
                    IconHandle _;
                    IconHelper.GetDefaultIconHandles(out _, out _iconHandle);
                    _iconDefaultHandle = _iconHandle.CriticalGetHandle();
                }
                _iconCurrentHandle = _iconDefaultHandle;
            }
        }
        #endregion

        #region 更新托盘图标
        private bool _added;                                //是否添加到任务栏图标
        private readonly object _syncObj = new object();    //只读对象用于锁定托盘图标显示
        private const int WmTrayMouseMessage = NativeMethods.WM_USER + 1024;        //托盘光标消息
        private void UpdateIcon(bool showIconInTray, bool isTransparent = false)
        {
            lock (_syncObj)
            {
                if (DesignerHelper.IsInDesignMode) return;

                _isTransparent = isTransparent;
                var data = new NOTIFYICONDATA
                {
                    uCallbackMessage = WmTrayMouseMessage,
                    uFlags = NativeMethods.NIF_MESSAGE | NativeMethods.NIF_ICON | NativeMethods.NIF_TIP,
                    hWnd = _messageWindowHandle,
                    uID = _id,
                    dwInfoFlags = NativeMethods.NIF_TIP,
                    hIcon = isTransparent ? IntPtr.Zero : _iconCurrentHandle,
                    szTip = Text
                };

                if (showIconInTray)
                {
                    if (!_added)
                    {
                        UnsafeNativeMethods.Shell_NotifyIcon(NativeMethods.NIM_ADD, data);
                        _added = true;
                    }
                    else
                    {
                        UnsafeNativeMethods.Shell_NotifyIcon(NativeMethods.NIM_MODIFY, data);
                    }
                }
                else if (_added)
                {
                    UnsafeNativeMethods.Shell_NotifyIcon(NativeMethods.NIM_DELETE, data);
                    _added = false;
                }
            }
        }
        #endregion

        #region ContextContent【不需要ContextMenu时使用ContextContent】
        public static readonly DependencyProperty ContextContentProperty = DependencyProperty.Register(
            "ContextContent", typeof(object), typeof(NotifyIcon), new PropertyMetadata(default(object)));

        public object ContextContent
        {
            get
            {
                return GetValue(ContextContentProperty);
            }
            set
            {
                SetValue(ContextContentProperty, value);
            }
        }
        #endregion

        //闪烁定时器
        private DispatcherTimer _dispatcherTimerBlink;
        #region 托盘图标闪烁间隔

        public static readonly DependencyProperty BlinkIntervalProperty = DependencyProperty.Register(
            "BlinkInterval", typeof(TimeSpan), typeof(NotifyIcon), new PropertyMetadata(TimeSpan.FromMilliseconds(500), OnBlinkIntervalChanged));

        private static void OnBlinkIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = (NotifyIcon)d;
            if (ctl._dispatcherTimerBlink != null)
            {
                ctl._dispatcherTimerBlink.Interval = (TimeSpan)e.NewValue;
            }
        }

        public TimeSpan BlinkInterval
        {
            get
            {
                return (TimeSpan)GetValue(BlinkIntervalProperty);
            }
            set
            {
                SetValue(BlinkIntervalProperty, value);
            }
        }
        #endregion

        #region 当前托盘图标是否在双闪
        public bool IsBlink
        {
            get
            {
                return (bool)GetValue(IsBlinkProperty);
            }
            set
            {
                SetValue(IsBlinkProperty, value);
            }
        }
        public static readonly DependencyProperty IsBlinkProperty = DependencyProperty.Register(
            "IsBlink", typeof(bool), typeof(NotifyIcon), new PropertyMetadata(false, OnIsBlinkChanged));

        private static void OnIsBlinkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = (NotifyIcon)d;
            if (ctl.Visibility != Visibility.Visible) return;
            if ((bool)e.NewValue)
            {
                if (ctl._dispatcherTimerBlink == null)
                {
                    ctl._dispatcherTimerBlink = new DispatcherTimer
                    {
                        Interval = ctl.BlinkInterval
                    };
                    ctl._dispatcherTimerBlink.Tick += ctl.DispatcherTimerBlinkTick;
                }
                ctl._dispatcherTimerBlink.Start();
            }
            else
            {
                ctl._dispatcherTimerBlink?.Stop();
                ctl._dispatcherTimerBlink = null;
                ctl.UpdateIcon(true);
            }
        }
        private void DispatcherTimerBlinkTick(object sender, EventArgs e)
        {
            if (Visibility != Visibility.Visible || _iconCurrentHandle == IntPtr.Zero) return;
            UpdateIcon(true, !_isTransparent);
        }
        #endregion

        #region 钩子回调
        private IntPtr Callback(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            if (IsLoaded)
            {
                if (msg == _wmTaskbarCreated)
                {
                    UpdateIcon(true);
                }
                else
                {
                    switch (lparam.ToInt64())
                    {
                        case NativeMethods.WM_LBUTTONDBLCLK:
                            WmMouseDown(MouseButton.Left, 2);
                            break;
                        case NativeMethods.WM_LBUTTONUP:
                            WmMouseUp(MouseButton.Left);
                            break;
                        case NativeMethods.WM_RBUTTONUP:
                            ShowContextMenu();
                            WmMouseUp(MouseButton.Right);
                            break;
                        case NativeMethods.WM_MOUSEMOVE:
                            if (!_dispatcherTimerPos.IsEnabled)
                            {
                                _dispatcherTimerPos.Interval = TimeSpan.FromMilliseconds(200);
                                _dispatcherTimerPos.Start();
                            }
                            break;
                    }
                }
            }
            return UnsafeNativeMethods.DefWindowProc(hWnd, msg, wparam, lparam);
        }


        private bool _doubleClick;  //是否双击
        #region W32窗体消息触发的 双击事件
        private void WmMouseDown(MouseButton button, int clicks)
        {
            if (clicks == 2)
            {
                RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, button)
                {
                    RoutedEvent = MouseDoubleClickEvent
                });
                _doubleClick = true;
            }
        }
        #endregion

        #region Win32窗体消息触发的单击事件
        private void WmMouseUp(MouseButton button)
        {
            if (!_doubleClick && button == MouseButton.Left)
            {
                RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, button)
                {
                    RoutedEvent = ClickEvent
                });
            }
            _doubleClick = false;
        }
        #endregion

        #region 托盘弹出
        private Popup _contextContent;
        private void ShowContextMenu()
        {
            if (ContextContent != null)
            {
                if (_contextContent == null)
                {
                    _contextContent = new Popup
                    {
                        Placement = PlacementMode.Mouse,
                        AllowsTransparency = true,
                        StaysOpen = false,
                        UseLayoutRounding = true,
                        SnapsToDevicePixels = true
                    };
                }

                _contextContent.Child = new ContentControl
                {
                    Content = ContextContent
                };
                _contextContent.IsOpen = true;
                var handle = IntPtr.Zero;
                var hwndSource = (HwndSource)PresentationSource.FromVisual(_contextContent.Child);
                if (hwndSource != null)
                {
                    handle = hwndSource.Handle;
                }
                UnsafeNativeMethods.SetForegroundWindow(handle);
            }
            else if (ContextMenu != null)
            {
                ContextMenu.Placement = PlacementMode.Mouse;
                ContextMenu.IsOpen = true;

                var handle = IntPtr.Zero;
                var hwndSource = (HwndSource)PresentationSource.FromVisual(ContextMenu);
                if (hwndSource != null)
                {
                    handle = hwndSource.Handle;
                }
                UnsafeNativeMethods.SetForegroundWindow(handle);
            }
        }

        public void CloseContextControl()
        {
            if (_contextContent != null)
            {
                _contextContent.IsOpen = false;
            }
            else if (ContextMenu != null)
            {
                ContextMenu.IsOpen = false;
            }
        }
        #endregion
        #endregion

        #region 自定义路由事件 单击事件
        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(NotifyIcon));

        public event RoutedEventHandler Click
        {
            add
            {
                AddHandler(ClickEvent, value);
            }
            remove
            {
                RemoveHandler(ClickEvent, value);
            }
        }
        #endregion

        #region 自定义路由事件 双击事件
        public static readonly RoutedEvent MouseDoubleClickEvent =
            EventManager.RegisterRoutedEvent("MouseDoubleClick", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(NotifyIcon));

        public event RoutedEventHandler MouseDoubleClick
        {
            add
            {
                AddHandler(MouseDoubleClickEvent, value);
            }
            remove
            {
                RemoveHandler(MouseDoubleClickEvent, value);
            }
        }
        #endregion

        #region IDispose接口
        private bool _isDisposed;
        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                if (_dispatcherTimerBlink != null && IsBlink)
                {
                    _dispatcherTimerBlink.Stop();
                }
                UpdateIcon(false);
            }
            _isDisposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion




        #region 类中类
        #region 是否设计时模式
        private class DesignerHelper
        {
            private static bool? _isInDesignMode;

            public static bool IsInDesignMode
            {
                get
                {
                    if (!_isInDesignMode.HasValue)
                    {
                        _isInDesignMode = (bool)DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty,
                            typeof(FrameworkElement)).Metadata.DefaultValue;
                    }
                    return _isInDesignMode.Value;
                }
            }
        }
        #endregion

        #region WinAPI接口调用
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private struct BITMAPINFO
        {
            public int bmiHeader_biSize;

            public int bmiHeader_biWidth;

            public int bmiHeader_biHeight;

            public short bmiHeader_biPlanes;

            public short bmiHeader_biBitCount;

            public int bmiHeader_biCompression;

            public int bmiHeader_biSizeImage;

            public int bmiHeader_biXPelsPerMeter;

            public int bmiHeader_biYPelsPerMeter;

            public int bmiHeader_biClrUsed;

            public int bmiHeader_biClrImportant;

            public BITMAPINFO(int width, int height, short bpp)
            {
                bmiHeader_biSize = SizeOf();
                bmiHeader_biWidth = width;
                bmiHeader_biHeight = height;
                bmiHeader_biPlanes = 1;
                bmiHeader_biBitCount = bpp;
                bmiHeader_biCompression = 0;
                bmiHeader_biSizeImage = 0;
                bmiHeader_biXPelsPerMeter = 0;
                bmiHeader_biYPelsPerMeter = 0;
                bmiHeader_biClrUsed = 0;
                bmiHeader_biClrImportant = 0;
            }

            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(BITMAPINFO));
            }
        }
        private static class Win32Constant
        {
            internal const int MAX_PATH = 260;
            internal const int INFOTIPSIZE = 1024;
            internal const int TRUE = 1;
            internal const int FALSE = 0;
        }

        private static class ExternDll
        {
            public const string
                User32 = "user32.dll",
                Gdi32 = "gdi32.dll",
                Kernel32 = "kernel32.dll",
                Shell32 = "shell32.dll";
        }

        [StructLayout(LayoutKind.Sequential)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private class ICONINFO
        {
            public bool fIcon = false;
            public int xHotspot = 0;
            public int yHotspot = 0;
            public BitmapHandle hbmMask = null;
            public BitmapHandle hbmColor = null;
        }

        private sealed class SafeFileMappingHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            [SecurityCritical]
            internal SafeFileMappingHandle(IntPtr handle) : base(false)
            {
                SetHandle(handle);
            }

            [SecurityCritical, SecuritySafeCritical]
            internal SafeFileMappingHandle() : base(true)
            {
            }

            public override bool IsInvalid
            {
                [SecurityCritical, SecuritySafeCritical]
                get { return handle == IntPtr.Zero; }
            }

            [SecurityCritical, SecuritySafeCritical]
            protected override bool ReleaseHandle()
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
                try
                {
                    return CloseHandleNoThrow(new HandleRef(null, handle));
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }

            [SecurityCritical]
            public static bool CloseHandleNoThrow(HandleRef handle)
            {
                HandleCollector.Remove((IntPtr)handle, CommonHandles.Kernel);
                var result = UnsafeNativeMethods.IntCloseHandle(handle);
                return result;
            }
        }
        private enum SM
        {
            CXSCREEN = 0,
            CYSCREEN = 1,
            CXVSCROLL = 2,
            CYHSCROLL = 3,
            CYCAPTION = 4,
            CXBORDER = 5,
            CYBORDER = 6,
            CXFIXEDFRAME = 7,
            CYFIXEDFRAME = 8,
            CYVTHUMB = 9,
            CXHTHUMB = 10,
            CXICON = 11,
            CYICON = 12,
            CXCURSOR = 13,
            CYCURSOR = 14,
            CYMENU = 15,
            CXFULLSCREEN = 16,
            CYFULLSCREEN = 17,
            CYKANJIWINDOW = 18,
            MOUSEPRESENT = 19,
            CYVSCROLL = 20,
            CXHSCROLL = 21,
            DEBUG = 22,
            SWAPBUTTON = 23,
            CXMIN = 28,
            CYMIN = 29,
            CXSIZE = 30,
            CYSIZE = 31,
            CXFRAME = 32,
            CXSIZEFRAME = CXFRAME,
            CYFRAME = 33,
            CYSIZEFRAME = CYFRAME,
            CXMINTRACK = 34,
            CYMINTRACK = 35,
            CXDOUBLECLK = 36,
            CYDOUBLECLK = 37,
            CXICONSPACING = 38,
            CYICONSPACING = 39,
            MENUDROPALIGNMENT = 40,
            PENWINDOWS = 41,
            DBCSENABLED = 42,
            CMOUSEBUTTONS = 43,
            SECURE = 44,
            CXEDGE = 45,
            CYEDGE = 46,
            CXMINSPACING = 47,
            CYMINSPACING = 48,
            CXSMICON = 49,
            CYSMICON = 50,
            CYSMCAPTION = 51,
            CXSMSIZE = 52,
            CYSMSIZE = 53,
            CXMENUSIZE = 54,
            CYMENUSIZE = 55,
            ARRANGE = 56,
            CXMINIMIZED = 57,
            CYMINIMIZED = 58,
            CXMAXTRACK = 59,
            CYMAXTRACK = 60,
            CXMAXIMIZED = 61,
            CYMAXIMIZED = 62,
            NETWORK = 63,
            CLEANBOOT = 67,
            CXDRAG = 68,
            CYDRAG = 69,
            SHOWSOUNDS = 70,
            CXMENUCHECK = 71,
            CYMENUCHECK = 72,
            SLOWMACHINE = 73,
            MIDEASTENABLED = 74,
            MOUSEWHEELPRESENT = 75,
            XVIRTUALSCREEN = 76,
            YVIRTUALSCREEN = 77,
            CXVIRTUALSCREEN = 78,
            CYVIRTUALSCREEN = 79,
            CMONITORS = 80,
            SAMEDISPLAYFORMAT = 81,
            IMMENABLED = 82,
            CXFOCUSBORDER = 83,
            CYFOCUSBORDER = 84,
            TABLETPC = 86,
            MEDIACENTER = 87,
            REMOTESESSION = 0x1000,
            REMOTECONTROL = 0x2001,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        private class WNDCLASS
        {
            public int style;
            public WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private class UnsafeNativeMethods
        {

            [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
            public static extern bool SetForegroundWindow(IntPtr hWnd);

            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            [DllImport(ExternDll.User32, CharSet = CharSet.Unicode, SetLastError = true, BestFitMapping = false)]
            public static extern short RegisterClass(WNDCLASS wc);

            [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
            public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            [DllImport(ExternDll.User32)]
            public static extern int GetSystemMetrics(SM nIndex);


            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            [DllImport(ExternDll.Gdi32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
            public static extern int GetDeviceCaps(HandleRef hDC, int nIndex);

            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            [DllImport(ExternDll.User32, SetLastError = true, ExactSpelling = true, EntryPoint = nameof(GetDC), CharSet = CharSet.Auto)]
            public static extern IntPtr IntGetDC(HandleRef hWnd);

            [SecurityCritical]
            public static IntPtr GetDC(HandleRef hWnd)
            {
                var hDc = IntGetDC(hWnd);
                if (hDc == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }
                return HandleCollector.Add(hDc, CommonHandles.HDC);
            }

            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            [DllImport(ExternDll.User32, EntryPoint = nameof(DestroyIcon), CharSet = CharSet.Auto, SetLastError = true)]
            private static extern bool IntDestroyIcon(IntPtr hIcon);

            [SecurityCritical]
            public static bool DestroyIcon(IntPtr hIcon)
            {
                var result = IntDestroyIcon(hIcon);
                return result;
            }

            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            [DllImport(ExternDll.Gdi32, EntryPoint = nameof(DeleteObject), CharSet = CharSet.Auto, SetLastError = true)]
            private static extern bool IntDeleteObject(IntPtr hObject);

            [SecurityCritical]
            public static bool DeleteObject(IntPtr hObject)
            {
                var result = IntDeleteObject(hObject);
                return result;
            }


            [SecurityCritical]
            public static BitmapHandle CreateDIBSection(HandleRef hdc, ref BITMAPINFO bitmapInfo, int iUsage, ref IntPtr ppvBits, SafeFileMappingHandle hSection, int dwOffset)
            {
                if (hSection == null)
                {
                    hSection = new SafeFileMappingHandle(IntPtr.Zero);
                }

                var hBitmap = PrivateCreateDIBSection(hdc, ref bitmapInfo, iUsage, ref ppvBits, hSection, dwOffset);
                return hBitmap;
            }

            [SecurityCritical, SuppressUnmanagedCodeSecurity]
            [DllImport(ExternDll.Gdi32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = nameof(CreateDIBSection))]
            private static extern BitmapHandle PrivateCreateDIBSection(HandleRef hdc, ref BITMAPINFO bitmapInfo, int iUsage, ref IntPtr ppvBits, SafeFileMappingHandle hSection, int dwOffset);


            [DllImport(ExternDll.Kernel32, EntryPoint = "CloseHandle", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool IntCloseHandle(HandleRef handle);


            [SecurityCritical, SuppressUnmanagedCodeSecurity]
            [DllImport(ExternDll.Gdi32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = nameof(CreateBitmap))]
            private static extern BitmapHandle PrivateCreateBitmap(int width, int height, int planes, int bitsPerPixel, byte[] lpvBits);

            [SecurityCritical]
            internal static BitmapHandle CreateBitmap(int width, int height, int planes, int bitsPerPixel, byte[] lpvBits)
            {
                var hBitmap = PrivateCreateBitmap(width, height, planes, bitsPerPixel, lpvBits);
                return hBitmap;
            }

            [SecurityCritical, SuppressUnmanagedCodeSecurity]
            [DllImport(ExternDll.User32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = nameof(CreateIconIndirect))]
            private static extern IconHandle PrivateCreateIconIndirect([In, MarshalAs(UnmanagedType.LPStruct)]ICONINFO iconInfo);

            [SecurityCritical]
            public static IconHandle CreateIconIndirect([In, MarshalAs(UnmanagedType.LPStruct)]ICONINFO iconInfo)
            {
                var hIcon = PrivateCreateIconIndirect(iconInfo);
                return hIcon;
            }

            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            [DllImport(ExternDll.User32, ExactSpelling = true, EntryPoint = nameof(ReleaseDC), CharSet = CharSet.Auto)]
            public static extern int IntReleaseDC(HandleRef hWnd, HandleRef hDC);

            [SecurityCritical]
            public static int ReleaseDC(HandleRef hWnd, HandleRef hDC)
            {
                HandleCollector.Remove((IntPtr)hDC, CommonHandles.HDC);
                return IntReleaseDC(hWnd, hDC);
            }

            [SecurityCritical, SuppressUnmanagedCodeSecurity]
            [DllImport(ExternDll.Kernel32, EntryPoint = "GetModuleFileName", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int IntGetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);

            [SecurityCritical]
            internal static string GetModuleFileName(HandleRef hModule)
            {
                var buffer = new StringBuilder(Win32Constant.MAX_PATH);
                while (true)
                {
                    var size = IntGetModuleFileName(hModule, buffer, buffer.Capacity);
                    if (size == 0)
                    {
                        throw new Win32Exception();
                    }

                    if (size == buffer.Capacity)
                    {
                        buffer.EnsureCapacity(buffer.Capacity * 2);
                        continue;
                    }

                    return buffer.ToString();
                }
            }

            [SecurityCritical, SuppressUnmanagedCodeSecurity]
            [DllImport(ExternDll.Shell32, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
            internal static extern int ExtractIconEx(string szExeFileName, int nIconIndex, out IconHandle phiconLarge, out IconHandle phiconSmall, int nIcons);


            [DllImport(ExternDll.Shell32, CharSet = CharSet.Auto)]
            public static extern int Shell_NotifyIcon(int message, NOTIFYICONDATA pnid);



            [SecurityCritical]
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            [DllImport(ExternDll.User32, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CreateWindowExW")]
            public static extern IntPtr CreateWindowEx(
                int dwExStyle,
                [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
                [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
                int dwStyle,
                int x,
                int y,
                int nWidth,
                int nHeight,
                IntPtr hWndParent,
                IntPtr hMenu,
                IntPtr hInstance,
                IntPtr lpParam);
        }



        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private static class CommonHandles
        {
            public static readonly int Icon = HandleCollector.RegisterType(nameof(Icon), 20, 500);

            public static readonly int HDC = HandleCollector.RegisterType(nameof(HDC), 100, 2);

            public static readonly int GDI = HandleCollector.RegisterType(nameof(GDI), 50, 500);

            public static readonly int Kernel = HandleCollector.RegisterType(nameof(Kernel), 0, 1000);
        }

        public delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        private class NativeMethods
        {
            [Flags]
            public enum FreeType
            {
                Decommit = 0x4000,
                Release = 0x8000,
            }


            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct TBBUTTON
            {
                public int iBitmap;
                public int idCommand;
                public IntPtr fsStateStylePadding;
                public IntPtr dwData;
                public IntPtr iString;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct TRAYDATA
            {
                public IntPtr hwnd;
                public uint uID;
                public uint uCallbackMessage;
                public uint bReserved0;
                public uint bReserved1;
                public IntPtr hIcon;
            }


            [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;

                public RECT(int left, int top, int right, int bottom)
                {
                    Left = left;
                    Top = top;
                    Right = right;
                    Bottom = bottom;
                }
            }

            public const int
                      WS_CHILD = 0x40000000,
                      BITSPIXEL = 12,
                      PLANES = 14,
                      BI_RGB = 0,
                      DIB_RGB_COLORS = 0,
                      NIF_MESSAGE = 0x00000001,
                      NIF_ICON = 0x00000002,
                      NIF_TIP = 0x00000004,
                      NIF_INFO = 0x00000010,
                      NIN_BALLOONSHOW = WM_USER + 2,
                      NIN_BALLOONHIDE = WM_USER + 3,
                      NIN_BALLOONTIMEOUT = WM_USER + 4,
                      NIM_ADD = 0x00000000,
                      NIM_MODIFY = 0x00000001,
                      NIM_DELETE = 0x00000002,
                      NIIF_NONE = 0x00000000,
                      NIIF_INFO = 0x00000001,
                      NIIF_WARNING = 0x00000002,
                      NIIF_ERROR = 0x00000003,
                      WM_KEYDOWN = 0x0100,
                      WM_KEYUP = 0x0101,
                      WM_SYSKEYDOWN = 0x0104,
                      WM_SYSKEYUP = 0x0105,
                      WM_MOUSEMOVE = 0x0200,
                      WM_LBUTTONDOWN = 0x0201,
                      WM_LBUTTONUP = 0x0202,
                      WM_LBUTTONDBLCLK = 0x0203,
                      WM_RBUTTONDOWN = 0x0204,
                      WM_RBUTTONUP = 0x0205,
                      WM_RBUTTONDBLCLK = 0x0206,
                      WM_MBUTTONDOWN = 0x0207,
                      WM_MBUTTONUP = 0x0208,
                      WM_MBUTTONDBLCLK = 0x0209,
                      WM_USER = 0x0400,
                      TB_GETBUTTON = WM_USER + 23,
                      TB_BUTTONCOUNT = WM_USER + 24,
                      TB_GETITEMRECT = WM_USER + 29,
                      STANDARD_RIGHTS_REQUIRED = 0x000F0000,
                      SYNCHRONIZE = 0x00100000,
                      PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF,
                      MEM_COMMIT = 0x1000,
                      MEM_RELEASE = 0x8000,
                      PAGE_READWRITE = 0x04,
                      TBSTATE_HIDDEN = 0x08;




            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int X;
                public int Y;

                public POINT(int x, int y)
                {
                    X = x;
                    Y = y;
                }
            }

            [Flags]
            public enum ProcessAccess
            {
                AllAccess = CreateThread | DuplicateHandle | QueryInformation | SetInformation | Terminate | VMOperation | VMRead | VMWrite | Synchronize,
                CreateThread = 0x2,
                DuplicateHandle = 0x40,
                QueryInformation = 0x400,
                SetInformation = 0x200,
                Terminate = 0x1,
                VMOperation = 0x8,
                VMRead = 0x10,
                VMWrite = 0x20,
                Synchronize = 0x100000
            }

            [DllImport(ExternDll.Kernel32, SetLastError = true, CharSet = CharSet.Auto)]
            public static extern IntPtr OpenProcess(ProcessAccess dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwProcessId);


            [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
            public static extern bool GetCursorPos(out POINT pt);


            [DllImport(ExternDll.User32, SetLastError = true)]
            public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

            [DllImport(ExternDll.User32, SetLastError = true)]
            public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

            [DllImport(ExternDll.User32)]
            public static extern int GetWindowRect(IntPtr hwnd, out RECT lpRect);

            [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
            public static extern uint SendMessage(IntPtr hWnd, uint Msg, uint wParam, IntPtr lParam);

            [DllImport(ExternDll.User32, SetLastError = true, CharSet = CharSet.Auto)]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);


            [Flags]
            public enum MemoryProtection
            {
                Execute = 0x10,
                ExecuteRead = 0x20,
                ExecuteReadWrite = 0x40,
                ExecuteWriteCopy = 0x80,
                NoAccess = 0x01,
                ReadOnly = 0x02,
                ReadWrite = 0x04,
                WriteCopy = 0x08,
                GuardModifierflag = 0x100,
                NoCacheModifierflag = 0x200,
                WriteCombineModifierflag = 0x400
            }

            [Flags]
            public enum AllocationType
            {
                Commit = 0x1000,
                Reserve = 0x2000,
                Decommit = 0x4000,
                Release = 0x8000,
                Reset = 0x80000,
                Physical = 0x400000,
                TopDown = 0x100000,
                WriteWatch = 0x200000,
                LargePages = 0x20000000
            }
            [DllImport(ExternDll.Kernel32, SetLastError = true, CharSet = CharSet.Auto)]
            public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, AllocationType flAllocationType, MemoryProtection flProtect);



            [DllImport(ExternDll.Kernel32, SetLastError = true, CharSet = CharSet.Auto)]
            public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out TBBUTTON lpBuffer, int dwSize, out int lpNumberOfBytesRead);

            [DllImport(ExternDll.Kernel32, SetLastError = true)]
            public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out RECT lpBuffer, int dwSize, out int lpNumberOfBytesRead);

            [DllImport(ExternDll.Kernel32, SetLastError = true)]
            public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out TRAYDATA lpBuffer, int dwSize, out int lpNumberOfBytesRead);

            [DllImport(ExternDll.Kernel32, SetLastError = true, CharSet = CharSet.Auto)]
            public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, FreeType dwFreeType);

            [DllImport(ExternDll.Kernel32, SetLastError = true, CharSet = CharSet.Auto)]
            public static extern int CloseHandle(IntPtr hObject);

            [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
            [ResourceExposure(ResourceScope.None)]
            public static extern int RegisterWindowMessage(string msg);
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private class NOTIFYICONDATA
        {
            public int cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATA));
            public IntPtr hWnd;
            public int uID;
            public int uFlags;
            public int uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip = string.Empty;
            public int dwState = 0x01;
            public int dwStateMask = 0x01;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo = string.Empty;
            public int uTimeoutOrVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle = string.Empty;
            public int dwInfoFlags;
        }

        private static class HandleCollector
        {
            private static HandleType[] HandleTypes;
            private static int HandleTypeCount;

            private static readonly object HandleMutex = new object();

            internal static IntPtr Add(IntPtr handle, int type)
            {
                HandleTypes[type - 1].Add();
                return handle;
            }

            [System.Security.SecuritySafeCritical]
            internal static SafeHandle Add(SafeHandle handle, int type)
            {
                HandleTypes[type - 1].Add();
                return handle;
            }

            internal static void Add(int type)
            {
                HandleTypes[type - 1].Add();
            }

            internal static int RegisterType(string typeName, int expense, int initialThreshold)
            {
                lock (HandleMutex)
                {
                    if (HandleTypeCount == 0 || HandleTypeCount == HandleTypes.Length)
                    {
                        HandleType[] newTypes = new HandleType[HandleTypeCount + 10];
                        if (HandleTypes != null)
                        {
                            Array.Copy(HandleTypes, 0, newTypes, 0, HandleTypeCount);
                        }
                        HandleTypes = newTypes;
                    }

                    HandleTypes[HandleTypeCount++] = new HandleType(expense, initialThreshold);
                    return HandleTypeCount;
                }
            }

            internal static IntPtr Remove(IntPtr handle, int type)
            {
                HandleTypes[type - 1].Remove();
                return handle;
            }

            [System.Security.SecuritySafeCritical]
            internal static SafeHandle Remove(SafeHandle handle, int type)
            {
                HandleTypes[type - 1].Remove();
                return handle;
            }

            internal static void Remove(int type)
            {
                HandleTypes[type - 1].Remove();
            }

            private class HandleType
            {
                private readonly int _initialThreshHold;
                private int _threshHold;
                private int _handleCount;
                private readonly int _deltaPercent;

                internal HandleType(int expense, int initialThreshHold)
                {
                    _initialThreshHold = initialThreshHold;
                    _threshHold = initialThreshHold;
                    _deltaPercent = 100 - expense;
                }

                internal void Add()
                {
                    lock (this)
                    {
                        _handleCount++;
                        var performCollect = NeedCollection();

                        if (!performCollect)
                        {
                            return;
                        }
                    }

                    GC.Collect();

                    var sleep = (100 - _deltaPercent) / 4;
                    System.Threading.Thread.Sleep(sleep);
                }

                private bool NeedCollection()
                {

                    if (_handleCount > _threshHold)
                    {
                        _threshHold = _handleCount + _handleCount * _deltaPercent / 100;
                        return true;
                    }

                    var oldThreshHold = 100 * _threshHold / (100 + _deltaPercent);
                    if (oldThreshHold >= _initialThreshHold && _handleCount < (int)(oldThreshHold * .9F))
                    {
                        _threshHold = oldThreshHold;
                    }

                    return false;
                }

                internal void Remove()
                {
                    lock (this)
                    {
                        _handleCount--;

                        _handleCount = Math.Max(0, _handleCount);
                    }
                }
            }
        }

        private abstract class WpfSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private readonly int _collectorId;

            [SecurityCritical]
            protected WpfSafeHandle(bool ownsHandle, int collectorId) : base(ownsHandle)
            {
                HandleCollector.Add(collectorId);
                _collectorId = collectorId;
            }

            [SecurityCritical, SecuritySafeCritical]
            protected override void Dispose(bool disposing)
            {
                HandleCollector.Remove(_collectorId);
                base.Dispose(disposing);
            }
        }
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private sealed class BitmapHandle : WpfSafeHandle
        {
            [SecurityCritical]
            private BitmapHandle() : this(true)
            {
                //请不要删除此构造函数，否则当使用自定义ico文件时会报错
            }

            [SecurityCritical]
            private BitmapHandle(bool ownsHandle) : base(ownsHandle, CommonHandles.GDI)
            {
            }

            [SecurityCritical]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            protected override bool ReleaseHandle()
            {
                return UnsafeNativeMethods.DeleteObject(handle);
            }

            [SecurityCritical]
            internal HandleRef MakeHandleRef(object obj)
            {
                return new HandleRef(obj, handle);
            }

            [SecurityCritical]
            internal static BitmapHandle CreateFromHandle(IntPtr hbitmap, bool ownsHandle = true)
            {
                return new BitmapHandle(ownsHandle)
                {
                    handle = hbitmap,
                };
            }
        }
        private sealed class IconHandle : WpfSafeHandle
        {
            [SecurityCritical]
            private IconHandle() : base(true, CommonHandles.Icon)
            {
            }

            [SecurityCritical]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            protected override bool ReleaseHandle()
            {
                return UnsafeNativeMethods.DestroyIcon(handle);
            }

            [SecurityCritical, SecuritySafeCritical]
            internal static IconHandle GetInvalidIcon()
            {
                return new IconHandle();
            }

            [SecurityCritical]
            internal IntPtr CriticalGetHandle()
            {
                return handle;
            }
        }

        private static class IconHelper
        {
            private static Size SmallIconSize;

            private static Size IconSize;

            private static int SystemBitDepth;

            [SecurityCritical, SecuritySafeCritical]
            public static void GetIconHandlesFromImageSource(ImageSource image, out IconHandle largeIconHandle, out IconHandle smallIconHandle)
            {
                EnsureSystemMetrics();
                largeIconHandle = CreateIconHandleFromImageSource(image, IconSize);
                smallIconHandle = CreateIconHandleFromImageSource(image, SmallIconSize);
            }

            [SecurityCritical]
            public static IconHandle CreateIconHandleFromImageSource(ImageSource image, Size size)
            {
                EnsureSystemMetrics();

                var asGoodAsItGets = false;

                var bf = image as BitmapFrame;
                if (bf?.Decoder?.Frames != null)
                {
                    bf = GetBestMatch(bf.Decoder.Frames, size);

                    asGoodAsItGets = bf.Decoder is IconBitmapDecoder || bf.PixelWidth == (int)size.Width && bf.PixelHeight == (int)size.Height;

                    image = bf;
                }

                if (!asGoodAsItGets)
                {
                    bf = BitmapFrame.Create(GenerateBitmapSource(image, size));
                }

                return CreateIconHandleFromBitmapFrame(bf);
            }

            [SecurityCritical]
            private static IconHandle CreateIconHandleFromBitmapFrame(BitmapFrame sourceBitmapFrame)
            {
                BitmapSource bitmapSource = sourceBitmapFrame;

                if (bitmapSource.Format != PixelFormats.Bgra32 && bitmapSource.Format != PixelFormats.Pbgra32)
                {
                    bitmapSource = new FormatConvertedBitmap(bitmapSource, PixelFormats.Bgra32, null, 0.0);
                }

                var w = bitmapSource.PixelWidth;
                var h = bitmapSource.PixelHeight;
                var bpp = bitmapSource.Format.BitsPerPixel;
                var stride = (bpp * w + 31) / 32 * 4;
                var sizeCopyPixels = stride * h;
                var xor = new byte[sizeCopyPixels];
                bitmapSource.CopyPixels(xor, stride, 0);

                return CreateIconCursor(xor, w, h, 0, 0, true);
            }

            [SecurityCritical]
            internal static IconHandle CreateIconCursor(byte[] colorArray, int width, int height, int xHotspot, int yHotspot, bool isIcon)
            {
                BitmapHandle colorBitmap = null;
                BitmapHandle maskBitmap = null;

                try
                {
                    var bi = new BITMAPINFO(width, -height, 32)
                    {
                        bmiHeader_biCompression = NativeMethods.BI_RGB
                    };

                    var bits = IntPtr.Zero;
                    colorBitmap = UnsafeNativeMethods.CreateDIBSection(new HandleRef(null, IntPtr.Zero), ref bi, NativeMethods.DIB_RGB_COLORS, ref bits, null, 0);

                    if (colorBitmap.IsInvalid || bits == IntPtr.Zero)
                    {
                        return IconHandle.GetInvalidIcon();
                    }

                    Marshal.Copy(colorArray, 0, bits, colorArray.Length);
                    var maskArray = GenerateMaskArray(width, height, colorArray);

                    maskBitmap = UnsafeNativeMethods.CreateBitmap(width, height, 1, 1, maskArray);
                    if (maskBitmap.IsInvalid)
                    {
                        return IconHandle.GetInvalidIcon();
                    }

                    var iconInfo = new ICONINFO
                    {
                        fIcon = isIcon,
                        xHotspot = xHotspot,
                        yHotspot = yHotspot,
                        hbmMask = maskBitmap,
                        hbmColor = colorBitmap
                    };

                    return UnsafeNativeMethods.CreateIconIndirect(iconInfo);
                }
                finally
                {
                    colorBitmap?.Dispose();
                    maskBitmap?.Dispose();
                }
            }

            private static byte[] GenerateMaskArray(int width, int height, byte[] colorArray)
            {
                var nCount = width * height;
                var bytesPerScanLine = AlignToBytes(width, 2) / 8;
                var bitsMask = new byte[bytesPerScanLine * height];

                for (var i = 0; i < nCount; i++)
                {
                    var hPos = i % width;
                    var vPos = i / width;
                    var byteIndex = hPos / 8;
                    var offsetBit = (byte)(0x80 >> (hPos % 8));

                    if (colorArray[i * 4 + 3] == 0x00)
                    {
                        bitsMask[byteIndex + bytesPerScanLine * vPos] |= offsetBit;
                    }
                    else
                    {
                        bitsMask[byteIndex + bytesPerScanLine * vPos] &= (byte)~offsetBit;
                    }

                    if (hPos == width - 1 && width == 8)
                    {
                        bitsMask[1 + bytesPerScanLine * vPos] = 0xff;
                    }
                }

                return bitsMask;
            }

            internal static int AlignToBytes(double original, int nBytesCount)
            {
                var nBitsCount = 8 << (nBytesCount - 1);
                return ((int)Math.Ceiling(original) + (nBitsCount - 1)) / nBitsCount * nBitsCount;
            }

            private static BitmapSource GenerateBitmapSource(ImageSource img, Size renderSize)
            {
                var drawingDimensions = new Rect(0, 0, renderSize.Width, renderSize.Height);

                var renderRatio = renderSize.Width / renderSize.Height;
                var aspectRatio = img.Width / img.Height;

                if (img.Width <= renderSize.Width && img.Height <= renderSize.Height)
                {
                    drawingDimensions = new Rect((renderSize.Width - img.Width) / 2, (renderSize.Height - img.Height) / 2, img.Width, img.Height);
                }
                else if (renderRatio > aspectRatio)
                {
                    var scaledRenderWidth = (img.Width / img.Height) * renderSize.Width;
                    drawingDimensions = new Rect((renderSize.Width - scaledRenderWidth) / 2, 0, scaledRenderWidth, renderSize.Height);
                }
                else if (renderRatio < aspectRatio)
                {
                    var scaledRenderHeight = img.Height / img.Width * renderSize.Height;
                    drawingDimensions = new Rect(0, (renderSize.Height - scaledRenderHeight) / 2, renderSize.Width, scaledRenderHeight);
                }

                var dv = new DrawingVisual();
                var dc = dv.RenderOpen();
                dc.DrawImage(img, drawingDimensions);
                dc.Close();

                var bmp = new RenderTargetBitmap((int)renderSize.Width, (int)renderSize.Height, 96, 96, PixelFormats.Pbgra32);
                bmp.Render(dv);

                return bmp;
            }

            private static BitmapFrame GetBestMatch(ReadOnlyCollection<BitmapFrame> frames, Size size)
            {
                var bestScore = int.MaxValue;
                var bestBpp = 0;
                var bestIndex = 0;

                var isBitmapIconDecoder = frames[0].Decoder is IconBitmapDecoder;

                for (var i = 0; i < frames.Count && bestScore != 0; ++i)
                {
                    var currentIconBitDepth = isBitmapIconDecoder ? frames[i].Thumbnail.Format.BitsPerPixel : frames[i].Format.BitsPerPixel;

                    if (currentIconBitDepth == 0)
                    {
                        currentIconBitDepth = 8;
                    }

                    var score = MatchImage(frames[i], size, currentIconBitDepth);
                    if (score < bestScore)
                    {
                        bestIndex = i;
                        bestBpp = currentIconBitDepth;
                        bestScore = score;
                    }
                    else if (score == bestScore)
                    {
                        if (bestBpp < currentIconBitDepth)
                        {
                            bestIndex = i;
                            bestBpp = currentIconBitDepth;
                        }
                    }
                }

                return frames[bestIndex];
            }

            private static int MatchImage(BitmapFrame frame, Size size, int bpp)
            {
                var score = 2 * MyAbs(bpp, SystemBitDepth, false) +
                            MyAbs(frame.PixelWidth, (int)size.Width, true) +
                            MyAbs(frame.PixelHeight, (int)size.Height, true);

                return score;
            }

            private static int MyAbs(int valueHave, int valueWant, bool fPunish)
            {
                var diff = (valueHave - valueWant);

                if (diff < 0)
                {
                    diff = (fPunish ? -2 : -1) * diff;
                }

                return diff;
            }

            [SecurityCritical, SecuritySafeCritical]
            private static void EnsureSystemMetrics()
            {
                if (SystemBitDepth == 0)
                {
                    var hdcDesktop = new HandleRef(null, UnsafeNativeMethods.GetDC(new HandleRef()));
                    try
                    {
                        var sysBitDepth = UnsafeNativeMethods.GetDeviceCaps(hdcDesktop, NativeMethods.BITSPIXEL);
                        sysBitDepth *= UnsafeNativeMethods.GetDeviceCaps(hdcDesktop, NativeMethods.PLANES);

                        if (sysBitDepth == 8)
                        {
                            sysBitDepth = 4;
                        }

                        var cxSmallIcon = UnsafeNativeMethods.GetSystemMetrics(SM.CXSMICON);
                        var cySmallIcon = UnsafeNativeMethods.GetSystemMetrics(SM.CYSMICON);
                        var cxIcon = UnsafeNativeMethods.GetSystemMetrics(SM.CXICON);
                        var cyIcon = UnsafeNativeMethods.GetSystemMetrics(SM.CYICON);

                        SmallIconSize = new Size(cxSmallIcon, cySmallIcon);
                        IconSize = new Size(cxIcon, cyIcon);
                        SystemBitDepth = sysBitDepth;
                    }
                    finally
                    {
                        UnsafeNativeMethods.ReleaseDC(new HandleRef(), hdcDesktop);
                    }
                }
            }

            [SecurityCritical, SecuritySafeCritical]
            public static void GetDefaultIconHandles(out IconHandle largeIconHandle, out IconHandle smallIconHandle)
            {
                largeIconHandle = null;
                smallIconHandle = null;

                SecurityHelper.DemandUIWindowPermission();

                var iconModuleFile = UnsafeNativeMethods.GetModuleFileName(new HandleRef());
                UnsafeNativeMethods.ExtractIconEx(iconModuleFile, 0, out largeIconHandle, out smallIconHandle, 1);
            }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private class SecurityHelper
        {
            private static UIPermission _allWindowsUIPermission;

            [SecurityCritical]
            internal static void DemandUIWindowPermission()
            {
                if (_allWindowsUIPermission == null)
                {
                    _allWindowsUIPermission = new UIPermission(UIPermissionWindow.AllWindows);
                }
                _allWindowsUIPermission.Demand();
            }
        }
        #endregion
        #endregion
    }
}
