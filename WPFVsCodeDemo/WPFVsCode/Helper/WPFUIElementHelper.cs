using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WPFVsCode.Helper
{
    partial class WPFUIElementHelper
    {
        #region 获取子元素成IEnumerable
        public IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : class
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                T t = child as T;
                if (t != null)
                    yield return t;

                var children = FindVisualChildren<T>(child);
                foreach (var item in children)
                    yield return item;
            }
        }
        #endregion

        #region 获取第一个T类型元素
        /// <summary>
        /// 获取第一个T类型元素
        /// </summary>
        /// <typeparam name="T">WPF控件类型</typeparam>
        /// <param name="parent">父控件</param>
        /// <returns>符合的第一个元素，没有返回null</returns>
        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;

                if (child == null && v != null)  //此时v不等于null
                {
                    child = GetVisualChild<T>(v);
                }

                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
        #endregion

        #region 根据名称获取第一个T类型元素
        /// <summary>
        /// 获取第一个T类型元素
        /// </summary>
        /// <typeparam name="T">WPF控件类型</typeparam>
        /// <param name="obj">父控件</param>
        /// <param name="name">控件名称</param>
        /// <returns>符合的第一个元素，没有返回null</returns>
        public static T GetVisualChild<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            T child = null;
            int numVisuals = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < numVisuals; i++)
            {
                DependencyObject childUI = VisualTreeHelper.GetChild(obj, i);
                child = childUI as T;
                if (child != null && (child.Name == name || string.IsNullOrEmpty(name)))
                {
                    break;
                }
                else
                {
                    child = GetVisualChild<T>(childUI, name);
                }
            }
            return null;
        }
        #endregion

        #region 更具Predicate<T>获取第一个T类型元素
        public static T GetVisualChild<T>(DependencyObject obj, Predicate<T> condition) where T : FrameworkElement
        {
            T child = null;
            int numVisuals = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < numVisuals; i++)
            {
                DependencyObject childUI = VisualTreeHelper.GetChild(obj, i);
                child = childUI as T;
                if (child != null && (condition == null || condition(child)))
                {
                    break;
                }
                else
                {
                    child = GetVisualChild<T>(childUI, condition);
                }
            }
            return null;
        }
        #endregion

        #region 获取List<T>,即获取所有的T类型元素
        /// <summary>
        /// 获取List<T>,即获取所有的T类型元素
        /// </summary>
        public static List<T> GetVisualChildren<T>(DependencyObject obj) where T : FrameworkElement
        {
            DependencyObject child = null;
            List<T> childList = new List<T>();
            int numVisuals = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < numVisuals; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                {
                    childList.Add((T)child);
                }
                if (child != null)
                    childList.AddRange(GetVisualChildren<T>(child));
            }
            return childList;
        }
        #endregion

        #region 根据名称获取List<T>,即获取所有的T类型元素
        /// <summary>
        ///  根据名称获取List<T>,即获取所有的T类型元素
        /// </summary>
        public static List<T> GetVisualChildren<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            DependencyObject child = null;
            List<T> childList = new List<T>();
            int numVisuals = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < numVisuals; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);
                if (child is T && (((T)child).Name == name || string.IsNullOrEmpty(name)))
                {
                    childList.Add((T)child);
                }
                if (child != null)
                    childList.AddRange(GetVisualChildren<T>(child, name));
            }
            return childList;
        }
        #endregion

        #region 根据条件获取List<T>，即获取所有的T类型元素
        /// <summary>
        ///  根据名称获取List<T>,即获取所有的T类型元素
        /// </summary>
        public static List<FrameworkElement> GetVisualChildren(DependencyObject obj, Predicate<FrameworkElement> condition, bool acceptSelf = false)
        {
            DependencyObject child = null;
            FrameworkElement curUI = obj as FrameworkElement;
            List<FrameworkElement> childList = new List<FrameworkElement>();

            if (acceptSelf == true)      //包含自己的情况下则主动添加退出
            {
                if (condition == null || (curUI != null && condition(curUI) == true))
                {
                    childList.Add(curUI);
                    return childList;
                }
            }


            int numVisuals = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < numVisuals; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);
                curUI = child as FrameworkElement;
                if (condition == null && curUI != null)
                {
                    childList.Add(curUI);
                }
                else if (condition != null && curUI != null && condition(curUI) == true)
                {
                    childList.Add(curUI);
                }
                else if (child != null)
                {
                    childList.AddRange(GetVisualChildren(child, condition));
                }
            }
            return childList;
        }
        #endregion


        #region 便利VirtualTree获取父控件
        public static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }
                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }
        /// <summary>
        /// 便利逻辑树获取父控件
        /// </summary>
        /// <param name="AncestorLevel">祖先级别，最低为1</param>
        public static T FindVisualParent<T>(UIElement element, int AncestorLevel) where T : UIElement
        {
            if (AncestorLevel < 1 || element == null) return default(T);

            UIElement parent = element;
            int curLevel = 1;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    if (curLevel == AncestorLevel)
                    {
                        return correctlyTyped;
                    }
                    else
                    {
                        curLevel++;
                    }
                }
                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }
        #endregion

        #region 便利VirtualTree获取一堆父控件
        public static T FindVisualParentS<T>(UIElement element) where T : UIElement
        {
            List<T> listTargets = new List<T>();
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    listTargets.Add(correctlyTyped);
                }
                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }
        #endregion

        #region 适用于控件模板中便利父控件
        public static T FindParent<T>(FrameworkElement element) where T : FrameworkElement
        {
            FrameworkElement parent = element.TemplatedParent as FrameworkElement;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }
                parent = parent.TemplatedParent as FrameworkElement;
            }
            return null;
        }
        #endregion
    }
}
