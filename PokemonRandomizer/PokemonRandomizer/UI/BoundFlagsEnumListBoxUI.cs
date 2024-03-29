﻿using PokemonRandomizer.UI.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
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

        private readonly Box<T> combinedFlags;

        public BoundFlagsEnumListBoxUI(string label, Box<T> combinedFlags, Func<IReadOnlyList<MenuBoxItem>> getChoiceList, Func<T, T, T> orEquals, string tooltip = null)
            : this(new Label() { Content = label }, combinedFlags, getChoiceList, orEquals, tooltip)
        {

        }
        public BoundFlagsEnumListBoxUI(Label label, Box<T> combinedFlags, Func<IReadOnlyList<MenuBoxItem>> getChoiceList, Func<T, T, T> orEquals, string tooltip = null)
        {
            this.combinedFlags = combinedFlags;
            Orientation = Orientation.Horizontal;
            this.orEquals = orEquals;
            referenceList = getChoiceList();
            foreach(var item in referenceList)
            {
                item.Selected += OnItemSelected;
                item.Unselected += OnItemSelected;
            }
            var labelElement = this.Add(label);
            if(!string.IsNullOrWhiteSpace(tooltip))
            {
                labelElement.ToolTip = tooltip;
            }
            ListBox = this.Add(new ListBox()
            {
                ItemsSource = referenceList,
                SelectionMode = SelectionMode.Multiple,
                ItemsPanel = Template,
                ItemContainerStyle = (System.Windows.Style)App.Current.FindResource("EnumListBoxItemStyle")
            });
            SetSelectedFromCombinedFlags(this.combinedFlags.Value);
        }

        private void OnItemSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            combinedFlags.Value = GetCombinedFlagFromSelected();
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
