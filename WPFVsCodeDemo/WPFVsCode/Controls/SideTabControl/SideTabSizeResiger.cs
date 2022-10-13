using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPFVsCode.Controls
{
    public class SideTabSizeResiger:Thumb
    {
        #region 静态构造函数
        static SideTabSizeResiger()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SideTabSizeResiger), new FrameworkPropertyMetadata(typeof(SideTabSizeResiger)));
        }
        #endregion



        #region SideTabCtrlMinContentLength
        public double SideTabCtrlMinContentLength
        {
            get { return (double)GetValue(SideTabCtrlMinContentLengthProperty); }
            set { SetValue(SideTabCtrlMinContentLengthProperty, value); }
        }
        public static readonly DependencyProperty SideTabCtrlMinContentLengthProperty = DependencyProperty.Register(
            "SideTabCtrlMinContentLength", typeof(double), typeof(SideTabSizeResiger),
            new PropertyMetadata(100d));
        #endregion

        #region SideTabCtrlMaxContentLength
        public double SideTabCtrlMaxContentLength
        {
            get { return (double)GetValue(SideTabCtrlMaxContentLengthProperty); }
            set { SetValue(SideTabCtrlMaxContentLengthProperty, value); }
        }
        public static readonly DependencyProperty SideTabCtrlMaxContentLengthProperty = DependencyProperty.Register(
            "SideTabCtrlMaxContentLength", typeof(double), typeof(SideTabSizeResiger),
            new PropertyMetadata(260d));
        #endregion


        public SideTabControl SideTabCtrlOwner
        {
            get { return (SideTabControl)GetValue(SideTabCtrlOwnerProperty); }
            set { SetValue(SideTabCtrlOwnerProperty, value); }
        }
        public static readonly DependencyProperty SideTabCtrlOwnerProperty =  DependencyProperty.Register(
            "SideTabCtrlOwner", typeof(SideTabControl), typeof(SideTabSizeResiger), 
            new PropertyMetadata(null)
        );

        public SideTabSizeResiger()
        {
            this.DragStarted += OnResiger_DragStarted;
            this.DragCompleted += OnResiger_DragCompleted;
            this.DragDelta += OnResiger_DragDelta;
        }

       

        Popup dragSplitter;
        double minBound = 0d;   //拖拽上届
        double maxBound = 0d;   //拖拽下届
        private void OnResiger_DragStarted(object sender, DragStartedEventArgs e)
        {
            if (SideTabCtrlOwner == null) return;
            Point pToScreen = this.PointToScreen(new Point(0, 0));  //此控件在屏幕的位置


            minBound = pToScreen.X - SideTabCtrlOwner.ContentLength + SideTabCtrlMinContentLength;
            maxBound = pToScreen.X - SideTabCtrlOwner.ContentLength + SideTabCtrlMaxContentLength;


            Rectangle rect = new Rectangle();
            rect.Height = this.ActualHeight;
            rect.Width = this.ActualWidth;
            rect.IsHitTestVisible = false;
            rect.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#80808080"));

            

            dragSplitter = new Popup();
            dragSplitter.Placement = PlacementMode.Absolute;
            dragSplitter.HorizontalOffset = pToScreen.X;
            dragSplitter.VerticalOffset = pToScreen.Y;
            dragSplitter.StaysOpen = true;
            dragSplitter.IsOpen = true;
            dragSplitter.AllowsTransparency = true;
            dragSplitter.Child = rect;

            Size sizeOfSpilt = new Size(rect.Width, rect.Height);
                                        
            if (pToScreen.X < 0)
            {
                dragSplitter.HorizontalOffset = 0;
                dragSplitter.Width = sizeOfSpilt.Width + pToScreen.X;
            }
            if (pToScreen.Y < 0)
            {
                dragSplitter.VerticalOffset = 0;
                dragSplitter.Height = sizeOfSpilt.Height + pToScreen.Y;
            }

            if (pToScreen.X + sizeOfSpilt.Width > SystemParameters.PrimaryScreenWidth)
                dragSplitter.Width = SystemParameters.PrimaryScreenWidth - pToScreen.X;
            if (pToScreen.Y + sizeOfSpilt.Height > SystemParameters.PrimaryScreenHeight)
                dragSplitter.Height = SystemParameters.PrimaryScreenHeight - pToScreen.Y;
        }


        private void OnResiger_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if(dragSplitter!=null && dragSplitter.IsOpen==true)
            {
                Point pToScreen = this.PointToScreen(new Point(0, 0));  //此控件在屏幕的位置
                double horizonValueChange = e.HorizontalChange;


                if(minBound > pToScreen.X + horizonValueChange)
                {
                    dragSplitter.HorizontalOffset = minBound;
                }
                else if(maxBound < pToScreen.X + horizonValueChange)
                {
                    dragSplitter.HorizontalOffset = maxBound;
                }
                else
                {
                    dragSplitter.HorizontalOffset = pToScreen.X + horizonValueChange;
                }
            }
        }

        private void OnResiger_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if(dragSplitter!=null)
            {
                Point pToScreen = this.PointToScreen(new Point(0, 0));  //此控件在屏幕的位置
                double delta = dragSplitter.HorizontalOffset - pToScreen.X;

                if(SideTabCtrlOwner!=null)
                {
                    SideTabCtrlOwner.ContentLength = SideTabCtrlOwner.ContentLength + delta;
                }

                if (dragSplitter.IsOpen)
                    dragSplitter.IsOpen = false;
            }
        }
    }
}
