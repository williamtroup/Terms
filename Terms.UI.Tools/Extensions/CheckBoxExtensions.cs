﻿using System.Windows.Controls;

namespace Terms.UI.Tools.Extensions;

public static class CheckBoxExtensions
{
    public static bool IsReallyChecked(this CheckBox checkBox)
    {
        return checkBox.IsChecked != null && checkBox.IsChecked.Value;
    }
}