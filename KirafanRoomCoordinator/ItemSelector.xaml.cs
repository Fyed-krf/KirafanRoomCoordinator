using Fyed.Kirafan;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Fyed.Kirafan.UI {
    /// <summary>
    /// ItemSelector.xaml の相互作用ロジック
    /// </summary>
    public partial class ItemSelector : Window {
        public List<ItemSetup> Items { get; set; }

        public ItemSelector() {
            InitializeComponent();
            Items = new List<ItemSetup>();
            itemsListView.ItemsSource = Items;
        }

        public class ItemSetup {
            public RoomItem Item { get; set; }
            public int Count { get; set; }
            public bool IsSelected { get; set; }

            public ItemSetup(RoomItem item) {
                Item = item;
                Count = 1;
                IsSelected = false;
            }
        }

        private void CloseWindow(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
