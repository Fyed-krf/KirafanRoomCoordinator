using System;
using System.Collections.Generic;
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
    /// MessageWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MessageWindow : Window {
        public string Message {
            get {
                return this.messageTextBox.Text;
            }
            set {
                this.messageTextBox.Text = value;
            }
        }

        public MessageWindow(string message) {
            InitializeComponent();
            this.Message = message;
        }
    }
}
