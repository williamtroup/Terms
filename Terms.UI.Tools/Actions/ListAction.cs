using System.Windows;
using System.Windows.Controls;

namespace Terms.UI.Tools.Actions
{
    public static class ListAction
    {
        public static void FocusSelectedItem(ListBox listBox, int listIndex)
        {
            ItemContainerGenerator itemContainerGenerator = listBox.ItemContainerGenerator;
            DependencyObject dependencyObject = itemContainerGenerator.ContainerFromIndex(listIndex);

            if (dependencyObject != null)
            {
                ListBoxItem listViewItem = (ListBoxItem) dependencyObject;
                listViewItem.Focus();
            }
        }

        public static void FindOnKeydown(string keydown, ListBox listBox)
        {
            bool firstTry = true;

            while (true)
            {
                int listIndex = 0;
                int startIndex = firstTry ? listBox.SelectedIndex : 0;

                if (listBox.Items.Count > 0 && startIndex > -1)
                {
                    keydown = keydown.ToLower();

                    bool found = false;

                    foreach (object item in listBox.Items)
                    {
                        if (!firstTry || listIndex > startIndex)
                        {
                            ListBoxItem listBoxItem = (ListBoxItem) item;

                            if (listBoxItem.Content.ToString().ToLower().StartsWith(keydown))
                            {
                                FocusSelectedItem(listBox, listIndex);
                                found = true;
                                break;
                            }
                        }

                        listIndex++;
                    }

                    if (!found && firstTry)
                    {
                        firstTry = false;
                        continue;
                    }
                }

                break;
            }
        }
    }
}