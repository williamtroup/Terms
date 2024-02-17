using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Terms.UI.Tools.Actions;

public static class BackgroundAction
{
    public static void Run(Action action)
    {
        Application.Current?.Dispatcher.Invoke(new ThreadStart(delegate {
            action();
        }), DispatcherPriority.Background);
    }
}