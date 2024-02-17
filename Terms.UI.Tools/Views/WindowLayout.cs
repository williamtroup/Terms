using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Terms.UI.Tools.Actions;

namespace Terms.UI.Tools.Views;

public class WindowLayout
{
    private readonly Window m_window;
    private readonly Border m_windowBorder;
    private readonly bool m_handlePreviewKeyDown;

    public static readonly Brush BorderActivatedColor = Brushes.Gray;
    public static readonly Brush BorderDeactivatedColor = Brushes.DarkGray;

    public WindowLayout(Window window, Border windowBorder = null, bool handlePreviewKeyDown = true)
    {
        m_window = window;
        m_windowBorder = windowBorder;
        m_handlePreviewKeyDown = handlePreviewKeyDown;

        SetupEvents();
    }

    public static WindowLayout Setup(Window window, Border windowBorder = null, bool handlePreviewKeyDown = true, List<Border> sideBorders = null)
    {
        WindowLayout windowLayout = new(window, windowBorder, handlePreviewKeyDown);

        if (sideBorders != null)
        {
            windowLayout.SideBorders = sideBorders;
        }

        return windowLayout;
    }

    private void SetupEvents()
    {
        if (m_windowBorder != null)
        {
            m_window.Activated += Window_OnActivated;
            m_window.Deactivated += Window_OnDeactivated;
        }

        if (m_handlePreviewKeyDown)
        {
            m_window.PreviewKeyDown += Window_OnPreviewKeyDown;
        }
    }

    public List<Border> SideBorders { private get; set; } = new List<Border>();

    private void Window_OnActivated(object sender, EventArgs e)
    {
        m_windowBorder.BorderBrush = BorderActivatedColor;

        foreach (Border border in SideBorders)
        {
            border.Background = BorderActivatedColor;
        }
    }

    private void Window_OnDeactivated(object sender, EventArgs e)
    {
        m_windowBorder.BorderBrush = BorderDeactivatedColor;

        foreach (Border border in SideBorders)
        {
            border.Background = BorderDeactivatedColor;
        }
    }

    private static void Window_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (KeyStroke.IsAltKey(Key.Space))
        {
            e.Handled = true;
        }
    }
}