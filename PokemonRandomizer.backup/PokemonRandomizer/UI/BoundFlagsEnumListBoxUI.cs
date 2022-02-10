using PokemonRandomizer.UI.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;

namespace PokemonRandomizer.UI
{
    public class BoundFlagsEnumListBoxUI<T> : StackPanel where T : Enum
    {
        private static ItemsPanelTemplate Template { get; }
        public ListBox ListBox { get; }
        private readonly IReadOnlyList<MenuBoxItem> referenceList;
        private readonly Func<T, T, T> orEquals;

        static BoundFlagsEnumListBoxUI()
        {
            string panelString = @"<ItemsPanelTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                                                       xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>";
            panelString += "<WrapPanel IsItemsHost=\"True\" Orientation=\"Horizontal\" Width=\"710\" VerticalAlignment=\"Center\"/>";
            panelString += "</ItemsPanelTemplate>";               
            StringReader stringReader = new StringReader(panelString);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            Template = (ItemsPanelTemplate)XamlReader.Load(xmlReader);
        }

        public BoundFlagsEnumListBoxUI(string label, Box<T> combinedFlags, Func<IReadOnlyList<MenuBoxItem>> getChoiceList, Func<T, T, T> orEquals)
        {
            Orientation = Orientation.Horizontal;
            this.orEquals = orEquals;
            referenceList = getChoiceList();
            this.Add(new Label() { Content = label, Width = 125, FontSize = 14, VerticalAlignment = System.Windows.VerticalAlignment.Center });
            ListBox = this.Add(new ListBox()
            {
                ItemsSource = referenceList,
                SelectionMode = SelectionMode.Multiple,
                ItemsPanel = Template,
                ItemContainerStyle = (System.Windows.Style)App.Current.FindResource("EnumListBoxItemStyle")
            });
            SetSelectedFromCombinedFlags(combinedFlags.Value);
            ListBox.SelectionChanged += (_, _2) => combinedFlags.Value = GetCombinedFlagFromSelected();
        }

        private void SetSelectedFromCombinedFlags(T combinedFlags)
        {
            foreach (var item in referenceList)
            {
                item.IsSelected = combinedFlags.HasFlag(item.Item);
            }
        }

        private T GetCombinedFlagFromSelected()
        {
            T combined = default;
            foreach(var item in referenceList)
            {
                if (item.IsSelected)
                {
                    combined = orEquals(combined, item.Item);
                }
            }
            return combined;
        }

        public class MenuBoxItem : ListBoxItem
        {
            public T Item { get; set; }
        }
    }
}
