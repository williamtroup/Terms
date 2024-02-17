using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Terms.UI.Tools.ViewModels.Base;

namespace Terms.UI.Tools.Actions
{
    public static class ListViewAction
    {
        public static void FocusSelectedItem(ListView listview, int listIndex)
        {
            ItemContainerGenerator itemContainerGenerator = listview.ItemContainerGenerator;
            DependencyObject dependencyObject = itemContainerGenerator.ContainerFromIndex(listIndex);

            if (dependencyObject != null)
            {
                ListViewItem listViewItem = (ListViewItem)dependencyObject;
                listViewItem.Focus();
            }
        }

        public static bool Find(string name, ListView listview, int ignoreIndex = -1)
        {
            bool found = false;

            int listIndex = 0;

            foreach (object item in listview.Items)
            {
                IDataModel connection = (IDataModel) item;

                if (ignoreIndex != listIndex && string.Equals(connection.Name, name, StringComparison.CurrentCultureIgnoreCase))
                {
                    found = true;
                    break;
                }

                listIndex++;
            }

            return found;
        }

        public static void FindOnKeydown(string keydown, ListView listview)
        {
            bool firstTry = true;

            while (true)
            {
                int listIndex = 0;
                int startIndex = firstTry ? listview.SelectedIndex : 0;

                if (listview.Items.Count > 0 && startIndex > -1)
                {
                    keydown = keydown.ToLower();

                    bool found = false;

                    foreach (object item in listview.Items)
                    {
                        if (!firstTry || listIndex > startIndex)
                        {
                            IDataModel dataModel = (IDataModel) item;

                            if (dataModel.Name.ToLower().StartsWith(keydown))
                            {
                                FocusSelectedItem(listview, listIndex);
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

        public static void SelectInverse(ListView listview)
        {
            List<int> selectedIndexes = (from object t in listview.SelectedItems select listview.Items.IndexOf(t)).ToList();

            listview.SelectedItems.Clear();

            for (int itemIndex = 0; itemIndex < listview.Items.Count; itemIndex++)
            {
                if (!selectedIndexes.Contains(itemIndex))
                {
                    listview.SelectedItems.Add(listview.Items[itemIndex]);
                }
            }
        }
    }
}