using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ControllersLibrary
{
    /// <summary>
    /// Interaction logic for ECSController.xaml
    /// </summary>
    public partial class ECSController : UserControl
    {
        public static readonly RoutedEvent NewECSEvent = EventManager.RegisterRoutedEvent("NewECSEventHandler",
           RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ECSController));

        public event RoutedEventHandler NewECSEventHandler
        {
            add { AddHandler(NewECSEvent, value); }
            remove { RemoveHandler(NewECSEvent, value); }
        }

        private Point currentECS;
        public Point CurrentECS
        {
            get { return currentECS; }
            set { SetECS(value); }
        }

        //private SolidColorBrush mySolidColorBrush;
        //public SolidColorBrush textColor
        //{
        //    get { return mySolidColorBrush; }
        //    set { mySolidColorBrush = value; }
        //}
        private DispatcherTimer timer;
        private Point inc = new Point();
        private int desiredsteps = 1000;
        private int stepcounter = 0;

        public ECSController()
        {
            InitializeComponent();

            currentECS = new Point(0, 0);

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);

            ECSCanvas.Background = new ImageBrush(new BitmapImage(new Uri(String.Format(@"pack://application:,,,/ControllersLibrary;component/Images/ECS/ECSBackground.png"))));            
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        public void SetECS(Point position)
        {
            currentECS = position;
            DrawCurrentECSpoint(currentECS);
            RaiseEvent(new RoutedEventArgs(ECSController.NewECSEvent, currentECS));
        }


        private void DrawCurrentECSpoint(Point point)
        {
            CurrentECSLabel.Margin = new Thickness((point.X + 1) * (ECSCanvas.ActualWidth / 2), ECSCanvas.ActualHeight - ((point.Y + 1) * (ECSCanvas.ActualHeight / 2)) + 5, 0, 0);
            CurrentECSLabel.Content = ("(" + (decimal.Round((decimal)point.X, 2)).ToString() + ", " + (decimal.Round((decimal)point.Y, 2)).ToString() + ")");
            Position_Star.Margin = new Thickness((point.X + 1) * (ECSCanvas.ActualWidth / 2), ECSCanvas.ActualHeight - ((point.Y + 1) * (ECSCanvas.ActualHeight / 2)), 0, 0);
        }

        private void DrawStoredECSpoint(string label, Point expression)
        {
            Ellipse p = new Ellipse();            
            p.Fill = Brushes.ForestGreen;
            p.StrokeThickness = 1;
            p.Stroke = Brushes.Black;
            p.Width = 12;
            p.Height = 12;
            p.Margin = new Thickness((expression.X + 1) * (ECSCanvas.Width / 2) -6 , ECSCanvas.Height - ((expression.Y + 1) * (ECSCanvas.Height / 2)) -6, 0, 0);
            ECSCanvas.Children.Add(p);

            Label l = new Label();
            l.Content = label;
            l.Foreground = Brushes.Black;

            //if (p.Margin.Top < (ECSCanvas.ActualHeight / 2))
            //{
            //    if (p.Margin.Left < (ECSCanvas.ActualWidth / 2))
            //        l.Margin = new Thickness((expression.X + 1) * (ECSCanvas.ActualWidth / 2), ECSCanvas.ActualHeight - ((expression.Y + 1) * (ECSCanvas.ActualHeight / 2)) + 5, 0, 0);
            //    else
            //        l.Margin = new Thickness((expression.X + 1) * (ECSCanvas.ActualWidth / 2) - 35, ECSCanvas.ActualHeight - ((expression.Y + 1) * (ECSCanvas.ActualHeight / 2)) + 5, 0, 0);
            //}
            //else
            //{
            //    if (p.Margin.Left < (ECSCanvas.ActualWidth / 2))
            //        l.Margin = new Thickness((expression.X + 1) * (ECSCanvas.ActualWidth / 2), ECSCanvas.ActualHeight - ((expression.Y + 1) * (ECSCanvas.ActualHeight / 2)) - 20, 0, 0);
            //    else
            //        l.Margin = new Thickness((expression.X + 1) * (ECSCanvas.ActualWidth / 2) - 35, ECSCanvas.ActualHeight - ((expression.Y + 1) * (ECSCanvas.ActualHeight / 2)) - 20, 0, 0);
            //}

            if (p.Margin.Top < (ECSCanvas.Height / 2))
            {
                if (p.Margin.Left < (ECSCanvas.Width / 2))
                    l.Margin = new Thickness((expression.X + 1) * (ECSCanvas.Width / 2), ECSCanvas.Height - ((expression.Y + 1) * (ECSCanvas.Height / 2)) + 5, 0, 0);
                else
                    l.Margin = new Thickness((expression.X + 1) * (ECSCanvas.Width / 2) - 35, ECSCanvas.Height - ((expression.Y + 1) * (ECSCanvas.Height / 2)) + 5, 0, 0);
            }
            else
            {
                if (p.Margin.Left < (ECSCanvas.Width / 2))
                    l.Margin = new Thickness((expression.X + 1) * (ECSCanvas.Width / 2), ECSCanvas.Height - ((expression.Y + 1) * (ECSCanvas.Height / 2)) - 20, 0, 0);
                else
                    l.Margin = new Thickness((expression.X + 1) * (ECSCanvas.Width / 2) - 35, ECSCanvas.Height - ((expression.Y + 1) * (ECSCanvas.Height / 2)) - 20, 0, 0);
            }
                
            ECSCanvas.Children.Add(l);
        }

        public void LoadECSPoint(string label, Point position)
        {
            DrawStoredECSpoint(label, position);
        }

        private void MouseOnCanvas(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Cross;
        }

        private void MouseClickOnCanvas(object sender, MouseButtonEventArgs e)
        {
            Point mouseposition = new Point((Mouse.GetPosition(ECSCanvas).X * 2 / ECSCanvas.ActualWidth) - 1, 1 - (Mouse.GetPosition(ECSCanvas).Y * 2 / ECSCanvas.ActualHeight));
            SetECS(mouseposition);
        }

        private void StartAnimation(Point newposition, int speed)
        {
            timer.Interval = new TimeSpan(speed);
            inc.X = (newposition.X - currentECS.X) / desiredsteps;
            inc.Y = (newposition.Y - currentECS.Y) / desiredsteps;
            stepcounter = 0;
            timer.Start();
        }
 
        private void timer_Tick(object sender, EventArgs e)
        {
            if (stepcounter < desiredsteps)
            {
                currentECS.X = currentECS.X + inc.X;
                currentECS.Y = currentECS.Y + inc.Y;
                SetECS(currentECS);
                stepcounter++;
            }
        }
     
    }
}
