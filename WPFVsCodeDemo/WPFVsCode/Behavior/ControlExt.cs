using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WPFVsCode.Behavior
{
    public static class ControlExt
    {
        #region HeaderForeground
        public static Brush GetHeaderForeground(DependencyObject obj)
        {
            return (Brush)obj.GetValue(HeaderForegroundProperty);
        }

        public static void SetHeaderForeground(DependencyObject obj, Brush value)
        {
            obj.SetValue(HeaderForegroundProperty, value);
        }

        public static readonly DependencyProperty HeaderForegroundProperty = DependencyProperty.RegisterAttached(
            "HeaderForeground", typeof(Brush), typeof(ControlExt),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region HeaderFontFamily
        public static FontFamily GetHeaderFontFamily(DependencyObject obj)
        {
            return (FontFamily)obj.GetValue(HeaderFontFamilyProperty);
        }

        public static void SetHeaderFontFamily(DependencyObject obj, FontFamily value)
        {
            obj.SetValue(HeaderFontFamilyProperty, value);
        }
        public static readonly DependencyProperty HeaderFontFamilyProperty = DependencyProperty.RegisterAttached(
            "HeaderFontFamily", typeof(FontFamily), typeof(ControlExt),
            new FrameworkPropertyMetadata(new FontFamily("微软雅黑"), FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region HeaderFontWeight
        public static FontWeight GetHeaderFontWeight(DependencyObject obj)
        {
            return (FontWeight)obj.GetValue(HeaderFontWeightProperty);
        }

        public static void SetHeaderFontWeight(DependencyObject obj, FontWeight value)
        {
            obj.SetValue(HeaderFontWeightProperty, value);
        }
        public static readonly DependencyProperty HeaderFontWeightProperty = DependencyProperty.RegisterAttached(
            "HeaderFontWeight", typeof(FontWeight), typeof(ControlExt),
            new FrameworkPropertyMetadata(FontWeights.Bold, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region HeaderFontSize
        public static double GetHeaderFontSize(DependencyObject obj)
        {
            return (double)obj.GetValue(HeaderFontSizeProperty);
        }

        public static void SetHeaderFontSize(DependencyObject obj, double value)
        {
            obj.SetValue(HeaderFontSizeProperty, value);
        }
        public static readonly DependencyProperty HeaderFontSizeProperty = DependencyProperty.RegisterAttached(
            "HeaderFontSize", typeof(double), typeof(ControlExt),
            new FrameworkPropertyMetadata(15d, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region HeaderFontStretch
        public static FontStretch GetHeaderFontStretch(DependencyObject obj)
        {
            return (FontStretch)obj.GetValue(HeaderFontStretchProperty);
        }

        public static void SetHeaderFontStretch(DependencyObject obj, FontStretch value)
        {
            obj.SetValue(HeaderFontStretchProperty, value);
        }
        public static readonly DependencyProperty HeaderFontStretchProperty = DependencyProperty.RegisterAttached(
            "HeaderFontStretch", typeof(FontStretch), typeof(ControlExt),
            new FrameworkPropertyMetadata(FontStretches.Normal, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region HeaderFontStyle
        public static FontStyle GetHeaderFontStyle(DependencyObject obj)
        {
            return (FontStyle)obj.GetValue(GetHeaderFontStyleProperty);
        }

        public static void SetHeaderFontStyle(DependencyObject obj, FontStyle value)
        {
            obj.SetValue(GetHeaderFontStyleProperty, value);
        }
        public static readonly DependencyProperty GetHeaderFontStyleProperty = DependencyProperty.RegisterAttached(
            "HeaderFontStyle", typeof(FontStyle), typeof(ControlExt),
            new FrameworkPropertyMetadata(FontStyles.Normal, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region HeaderTextAligment
        public static TextAlignment GetHeaderTextAligment(DependencyObject obj)
        {
            return (TextAlignment)obj.GetValue(HeaderTextAligmentProperty);
        }

        public static void SetHeaderTextAligment(DependencyObject obj, TextAlignment value)
        {
            obj.SetValue(HeaderTextAligmentProperty, value);
        }
        public static readonly DependencyProperty HeaderTextAligmentProperty = DependencyProperty.RegisterAttached(
            "HeaderTextAligment", typeof(TextAlignment), typeof(ControlExt),
            new FrameworkPropertyMetadata(TextAlignment.Left, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region HeaderMargin
        public static Thickness GetHeaderMargin(DependencyObject obj)
        {
            return (Thickness)obj.GetValue(HeaderMarginProperty);
        }

        public static void SetHeaderMargin(DependencyObject obj, Thickness value)
        {
            obj.SetValue(HeaderMarginProperty, value);
        }
        public static readonly DependencyProperty HeaderMarginProperty = DependencyProperty.RegisterAttached(
            "HeaderMargin", typeof(Thickness), typeof(ControlExt),
            new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region HeaderPadding
        public static Thickness GetHeaderPadding(DependencyObject obj)
        {
            return (Thickness)obj.GetValue(HeaderPaddingProperty);
        }

        public static void SetHeaderPadding(DependencyObject obj, Thickness value)
        {
            obj.SetValue(HeaderPaddingProperty, value);
        }
        public static readonly DependencyProperty HeaderPaddingProperty = DependencyProperty.RegisterAttached(
            "HeaderPadding", typeof(Thickness), typeof(ControlExt),
            new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion


        #region IconMargin

        public static Thickness GetIconMargin(DependencyObject obj)
        {
            return (Thickness)obj.GetValue(IconMarginProperty);
        }

        public static void SetIconMargin(DependencyObject obj, Thickness value)
        {
            obj.SetValue(IconMarginProperty, value);
        }
        public static readonly DependencyProperty IconMarginProperty = DependencyProperty.RegisterAttached(
            "IconMargin", typeof(Thickness), typeof(ControlExt),
             new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region IconBrush
        public static Brush GetIconBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(IconBrushProperty);
        }
        public static void SetIconBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(IconBrushProperty, value);
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.RegisterAttached(
            "IconBrush", typeof(Brush), typeof(ControlExt),
            new FrameworkPropertyMetadata(Brushes.Transparent,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsArrange |
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault |
                FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender |
                FrameworkPropertyMetadataOptions.AffectsParentMeasure));
        #endregion



        #region MenuItemPopupHorizontalOffset
        public static double GetMenuItemPopupHorizontalOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(MenuItemPopupHorizontalOffsetProperty);
        }

        public static void SetMenuItemPopupHorizontalOffset(DependencyObject obj, double value)
        {
            obj.SetValue(MenuItemPopupHorizontalOffsetProperty, value);
        }
        public static readonly DependencyProperty MenuItemPopupHorizontalOffsetProperty = DependencyProperty.RegisterAttached(
            "MenuItemPopupHorizontalOffset", typeof(double), typeof(ControlExt), new PropertyMetadata(0d));
        #endregion

        #region MenuItemPopupVerticalOffset
        public static double GetMenuItemPopupVerticalOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(MenuItemPopupVerticalOffsetProperty);
        }

        public static void SetMenuItemPopupVerticalOffset(DependencyObject obj, double value)
        {
            obj.SetValue(MenuItemPopupVerticalOffsetProperty, value);
        }
        public static readonly DependencyProperty MenuItemPopupVerticalOffsetProperty = DependencyProperty.RegisterAttached(
            "MenuItemPopupVerticalOffset", typeof(double), typeof(ControlExt), new PropertyMetadata(0d));
        #endregion










        #region HeaderBackground
        public static Brush GetHeaderBackground(DependencyObject obj)
        {
            return (Brush)obj.GetValue(HeaderBackgroundProperty);
        }

        public static void SetHeaderBackground(DependencyObject obj, Brush value)
        {
            obj.SetValue(HeaderBackgroundProperty, value);
        }
        public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.RegisterAttached(
            "HeaderBackground", typeof(Brush), typeof(ControlExt),
            new FrameworkPropertyMetadata(Brushes.Transparent,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsArrange |
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault |
                FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender |
                FrameworkPropertyMetadataOptions.AffectsParentMeasure));
        #endregion

        #region HeaderBorderBrush
        public static Brush GetHeaderBorderBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(HeaderBorderBrushProperty);
        }

        public static void SetHeaderBorderBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(HeaderBorderBrushProperty, value);
        }
        public static readonly DependencyProperty HeaderBorderBrushProperty = DependencyProperty.RegisterAttached(
            "HeaderBorderBrush", typeof(Brush), typeof(ControlExt),
            new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region HeaderBorderThickness
        public static Thickness GetHeaderBorderThickness(DependencyObject obj)
        {
            return (Thickness)obj.GetValue(HeaderBorderThicknessProperty);
        }

        public static void SetHeaderBorderThickness(DependencyObject obj, Thickness value)
        {
            obj.SetValue(HeaderBorderThicknessProperty, value);
        }
        public static readonly DependencyProperty HeaderBorderThicknessProperty = DependencyProperty.RegisterAttached(
            "HeaderBorderThickness", typeof(Thickness), typeof(ControlExt),
            new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region ContentBorderBrush
        public static Brush GetContentBorderBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(ContentBorderBrushProperty);
        }

        public static void SetContentBorderBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(ContentBorderBrushProperty, value);
        }

        public static readonly DependencyProperty ContentBorderBrushProperty = DependencyProperty.RegisterAttached(
            "ContentBorderBrush", typeof(Brush), typeof(ControlExt),
            new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region ContentBorderThickness
        public static Thickness GetContentBorderThickness(DependencyObject obj)
        {
            return (Thickness)obj.GetValue(ContentBorderThicknessProperty);
        }

        public static void SetContentBorderThickness(DependencyObject obj, Thickness value)
        {
            obj.SetValue(ContentBorderThicknessProperty, value);
        }
        public static readonly DependencyProperty ContentBorderThicknessProperty = DependencyProperty.RegisterAttached(
            "ContentBorderThickness", typeof(Thickness), typeof(ControlExt),
            new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region HeaderHorizonalAligment
        public static HorizontalAlignment GetHeaderHorizonalAligment(DependencyObject obj)
        {
            return (HorizontalAlignment)obj.GetValue(HeaderHorizonalAligmentProperty);
        }

        public static void SetHeaderHorizonalAligment(DependencyObject obj, HorizontalAlignment value)
        {
            obj.SetValue(HeaderHorizonalAligmentProperty, value);
        }
        public static readonly DependencyProperty HeaderHorizonalAligmentProperty = DependencyProperty.RegisterAttached(
            "HeaderHorizonalAligment", typeof(HorizontalAlignment), typeof(ControlExt),
            new FrameworkPropertyMetadata(HorizontalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region HeaderVerticalAligment

        public static VerticalAlignment GetHeaderVerticalAligment(DependencyObject obj)
        {
            return (VerticalAlignment)obj.GetValue(HeaderVerticalAligmentProperty);
        }

        public static void SetHeaderVerticalAligment(DependencyObject obj, VerticalAlignment value)
        {
            obj.SetValue(HeaderVerticalAligmentProperty, value);
        }
        public static readonly DependencyProperty HeaderVerticalAligmentProperty = DependencyProperty.RegisterAttached(
            "HeaderVerticalAligment", typeof(VerticalAlignment), typeof(ControlExt),
            new FrameworkPropertyMetadata(VerticalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion



        #region ContentWidth
        public static double GetContentWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(ContentWidthProperty);
        }

        public static void SetContentWidth(DependencyObject obj, double value)
        {
            obj.SetValue(ContentWidthProperty, value);
        }
        public static readonly DependencyProperty ContentWidthProperty = DependencyProperty.RegisterAttached(
            "ContentWidth", typeof(double), typeof(ControlExt),
            new PropertyMetadata(double.NaN));
        #endregion



        #region ContentHeight
        public static double GetContentHeight(DependencyObject obj)
        {
            return (double)obj.GetValue(ContentHeightProperty);
        }

        public static void SetContentHeight(DependencyObject obj, double value)
        {
            obj.SetValue(ContentHeightProperty, value);
        }
        public static readonly DependencyProperty ContentHeightProperty = DependencyProperty.RegisterAttached(
            "ContentHeight", typeof(double), typeof(ControlExt),
            new PropertyMetadata(double.NaN));
        #endregion
    }
}
