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
using System.Net;
using System.Web;
using System.Collections.Specialized;
using System.Collections;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using System.Media;

namespace presenwaiwai
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private CommentList commentListWindow;
        private Thread listenThread;
        private List<string> commentList;
        private Canvas canvas;
        private Window canvasWindow;
        private HttpListener listener;
        private string se = null;
        public MainWindow()
        {
            InitializeComponent();

        }


        private void CreateCanvasWindow()
        {
            canvasWindow = new Window();

            canvasWindow.Owner = Window.GetWindow(this);
            canvasWindow.ShowInTaskbar = false;
            canvasWindow.AllowsTransparency = true;
            canvasWindow.Background = Brushes.Transparent;
            canvasWindow.WindowStyle = WindowStyle.None;
            canvasWindow.WindowState = WindowState.Maximized;
            canvasWindow.Topmost = true;
            canvasWindow.Content = new Canvas();
            canvasWindow.Show();

            canvas = canvasWindow.Content as Canvas;
        }

        private int GetPortNum()
        {
            if (!this.CheckAccess())
            {
                int ret = -1;
                Dispatcher.BeginInvoke(new Action(() => { ret = GetPortNum(); })).Wait();
                return ret;
            }
            try
            {
                int portNum = int.Parse(port.Text);
                return portNum;
            }
            catch
            {
                MessageBox.Show("ポート番号が不正です", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                port.Text = "8000";
                return -1;
            }
        }
        private void StartReceiveComment()
        {
            string html = @"
<!doctype html>
<style>
  *{ font-size: 20px; }
  div, button { margin: 0.5em; }
  input[type='text'] {     padding: 0.2em;
    border-radius: 5px;
    box-shadow: none;
    border: 2px solid #fcc;
    cursor: text; 
  } 
  
  input[type='submit'],button { text-shadow: 1px 1px 0 #ccc; color: #B873BC; background-color: transparent; border: 2px solid #B873BC; border-radius: 5px; padding: .5em 1em; }
  input[type='submit']:hover,button:hover { border: 2px solid #cf00b5; cursor: pointer; color: #cf00b5; }
  input[type='submit']:active,button:active { text-shadow: none; }
</style>
<form action='#' method='post'>
  <div>
    <input type='text' name='text' />
  </div>
  <div>
    <label for='color'>文字色</label>
    <input type='color' name='color' id='color' value='#c0ffee' />
  </div>
  <div>
    <label for='size'>フォントサイズ</label>
    <input type='range' min='30' max='50' name='size' id='size' />
  </div>
  <div>
    <input type='submit' value='送信' />
  </div>
</form>
<button id='good'>+1</button>
<script>
document.getElementById('good').onclick = function()
{
  var req = new XMLHttpRequest();
  req.open('POST', document.URL, true);
  req.send('good=1');
}

</script>
";
            int portNum = GetPortNum();
            if (portNum < 1)
            {
                return;
            }

            string urlPrefix = "http://*:" + portNum.ToString() + "/";

            listener = new HttpListener();
            listener.Prefixes.Add(urlPrefix);
            listener.Start();

            while (true)
            {
                var context = listener.GetContext();
                var request = context.Request;
                var responseStream = context.Response.OutputStream;

                NameValueCollection data = null;
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    data = HttpUtility.ParseQueryString(reader.ReadToEnd());
                }

                byte[] htmlbytes = Encoding.UTF8.GetBytes(html);
                responseStream.Write(htmlbytes, 0, htmlbytes.Length);
                responseStream.Close();

                if (data["good"] != null)
                {
                    AddGood();
                }
                else {
                    AddComment(data);
                }
            }
        }

        private void AddGood()
        {
            if (!CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => AddGood()));
                return;
            }
            if (se != null)
            {
                new Task(() =>
                {
                    var p = new MediaPlayer();
                    p.Open(new Uri(se));
                    p.Play();
                }).Start();
            }
            int x = int.Parse(good_count.Content.ToString());
            good_count.Content = (x + 1).ToString();
        }

        private void AddComment(NameValueCollection data)
        {
            try {
                if (data["text"].Length <= 0)
                {
                    return;
                }
            } catch (Exception)
            {
                return;
            }
            commentList.Add(data["text"]);
            commentListWindow.AddComment(data["text"]);
            CreateComment(data);
        }
        private void CreateComment(NameValueCollection data)
        {
            if (!CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => CreateComment(data)));
                return;
            }

            TextBlock t = new TextBlock();
            try
            {
                t.Text = data["text"];
            }
            catch (Exception)
            {
                return;
            }
            try
            {
                t.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(data["color"]));
            }
            catch (Exception)
            {
                t.Foreground = Brushes.Black;
            }
            
            
            try
            {
                t.FontSize = double.Parse(data["size"]);
            }
            catch (Exception)
            {
                t.FontSize = 20;
            }
           

            DropShadowEffect effect = new DropShadowEffect();
            effect.Color = Color.FromRgb(0, 0, 0);
            effect.Direction = 315;
            effect.BlurRadius = 2;
            effect.ShadowDepth = 2;
            t.Effect = effect;

            // animation        
            var trans = new TranslateTransform();
            Duration d = new Duration(TimeSpan.FromSeconds(11 - text_speed.Value));
            DoubleAnimation animation = new DoubleAnimation(canvasWindow.Width, 0 - t.FontSize * t.Text.Length, d);

            // register
            t.RenderTransform = trans;
            
            canvas.Children.Add(t);
            Canvas.SetTop(t, 25);
            animation.Completed += (a, b) => canvas.Children.Remove(t);

            trans.BeginAnimation(TranslateTransform.XProperty, animation);
        }
        private void CommentListWindowInit()
        {
            if (commentListWindow == null)
            {
                commentListWindow = new CommentList();
                commentListWindow.Owner = Window.GetWindow(this);
                commentListWindow.ShowInTaskbar = false;
                commentListWindow.Show();
                commentListWindow.Closed += CommentListWindow_Closed;
 
                commentList.ForEach(x => commentListWindow.AddComment(x));
            }
        }

        private void CommentListWindow_Closed(object sender, EventArgs e)
        {
            commentListWindow = null;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (listenThread == null)
            {
                CreateCanvasWindow();
                commentList = new List<string>();
                CommentListWindowInit();

                listenThread = new Thread(new ThreadStart(StartReceiveComment));
                listenThread.Start();

                button.Content = "Stop";
            }
            else
            {
                

                listenThread.Abort();
                listenThread = null;

                listener.Close();

                canvasWindow.Close();
                canvasWindow = null;

                MessageBox.Show(GetWindow(this), "総コメント数:" + commentList.Count.ToString() + "\n総Good数:" + good_count.Content.ToString(), "result", MessageBoxButton.OK);

                button.Content = "Run";
            }
            

        }

        private void mainwindow_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void openCommentList_Click(object sender, RoutedEventArgs e)
        {
            CommentListWindowInit();
        }

        private void mainwindow_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void text_speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            label3.Content = (Math.Round(text_speed.Value*10)/10.0).ToString();
        }

        private void mainwindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.M)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = false;
                dialog.CheckFileExists = true;
                dialog.Filter = "Wave File (*.wav)|*.wav";
                if (dialog.ShowDialog(GetWindow(this)) == true)
                {
                    se = dialog.FileName;
                } else
                {
                    se = null;
                }
            }
        }
    }
}
