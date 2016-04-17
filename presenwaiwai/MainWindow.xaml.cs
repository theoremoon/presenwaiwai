using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace presenwaiwai
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private CommentList commentListWindow;
        public MainWindow()
        {
            InitializeComponent();
        }

        private Canvas CreateCanvasWindow()
        {
            Window childWin = new Window();

            childWin.Owner = Window.GetWindow(this);
            childWin.ShowInTaskbar = false;
            childWin.AllowsTransparency = true;
            childWin.Background = Brushes.Transparent;
            childWin.WindowStyle = WindowStyle.None;
            childWin.WindowState = WindowState.Maximized;
            childWin.Topmost = true;
            childWin.Content = new Canvas();
            childWin.Show();

            return childWin.Content as Canvas;
        }

        private void CommentListWindowInit()
        {
            if (commentListWindow == null)
            {
                commentListWindow = new CommentList();
                commentListWindow.Show();
                commentListWindow.Closed += CommentListWindow_Closed;
            }
        }

        private void CommentListWindow_Closed(object sender, EventArgs e)
        {
            commentListWindow = null;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void mainwindow_Loaded(object sender, RoutedEventArgs e)
        {
            CommentListWindowInit();
        }

        private void openCommentList_Click(object sender, RoutedEventArgs e)
        {
            CommentListWindowInit();
        }
    }
}
