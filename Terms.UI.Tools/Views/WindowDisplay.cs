using System;
using System.Windows;
using Terms.UI.Tools.Enums;

namespace Terms.UI.Tools.Views;

public class WindowDisplay
{
    private const double OpacityIncriment = 0.08;
    private const double OpacityMinimum = 0.0;

    private WindowMode m_windowMode = WindowMode.Load;

    private readonly Window m_window;
    private readonly double m_opacityIncriment;
    private readonly double m_maximumOpacity;

    public WindowDisplay(Window window, double opacityIncriment = OpacityIncriment, double maximumOpacity = 1.0)
    {
        m_window = window;
        m_opacityIncriment = opacityIncriment;
        m_maximumOpacity = maximumOpacity;

        window.Opacity = OpacityMinimum;
    }

    public void Close()
    {
        m_windowMode = WindowMode.Close;
    }

    public void UpdateWindow()
    {
        if (m_windowMode == WindowMode.Load)
        {
            if (m_window.Opacity <= OpacityMinimum)
            {
                OnBeforeWindowShown?.Invoke();
            }

            if (m_window.Opacity < m_maximumOpacity)
            {
                if (!IncrimentOpacity)
                {
                    m_window.Opacity = m_maximumOpacity;
                }
                else
                {
                    m_window.Opacity += m_opacityIncriment;
                }

                if (m_window.Opacity >= m_maximumOpacity)
                {
                    OnAfterWindowShown?.Invoke();
                }
            }
        }
        else
        {
            if (m_window.Opacity > OpacityMinimum)
            {
                if (!IncrimentOpacity)
                {
                    m_window.Opacity = OpacityMinimum;
                }
                else
                {
                    m_window.Opacity -= m_opacityIncriment;
                }
            }
            else
            {
                m_window.Close();
            }
        }
    }

    public Action OnBeforeWindowShown { private get; set; }
    public Action OnAfterWindowShown { private get; set; }
    public bool IncrimentOpacity { private get; set; }
}