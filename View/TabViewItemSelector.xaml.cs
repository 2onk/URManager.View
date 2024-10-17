using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using URManager.Backend.Model;
using URManager.View.ViewModel;

namespace URManager.View.View
{
    public sealed partial class TabViewItemSelector : TabViewItem
    {
        private DataTemplate _tabTemplate;

        public TabViewItemSelector()
        {
            this.InitializeComponent();
        }

        // DependencyProperty update callback handles template load.  Never cleans up after itself.
        private static void SelectOrClearTemplate(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TabViewItemSelector selector) return;

            // Create the content
            if (e.NewValue is TabItems tab)
            {
                // Generate tab content
                selector._tabTemplate = tab switch
                {
                    RobotsViewModel => selector.RobotViewTemplate,
                    SettingsViewModel => selector.SettingsViewTemplate,
                    FlexibleEthernetIpViewModel => selector.FlexibleEthernetIpViewTemplate,
                    _ => throw new NotImplementedException()
                };
                var tabArgs = new ElementFactoryGetArgs { Data = selector._tabTemplate, Parent = selector };
                selector.Content = selector._tabTemplate.GetElement(tabArgs);

            }
        }

        public object TabContentSource
        {
            get => GetValue(TabContentSourceProperty);
            set => SetValue(TabContentSourceProperty, value);
        }
        public static readonly DependencyProperty TabContentSourceProperty = DependencyProperty
            .Register(nameof(TabContentSource), typeof(object), typeof(TabViewItemSelector), new PropertyMetadata(null, SelectOrClearTemplate));
    }
}
