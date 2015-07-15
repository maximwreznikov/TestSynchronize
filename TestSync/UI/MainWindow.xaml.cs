using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestSync.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<SynchronizableObject> _shapes = new ObservableCollection<SynchronizableObject>();
        public ObservableCollection<SynchronizableObject> Shapes
        {
            get{ return _shapes;}
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }


        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var syncManager = new NotificationManager();

            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(syncManager.WndProc);

            SyncPool.Instance.Startup(syncManager);

            SyncPool.Instance.AttachCollection(_shapes);
        }

        private void AddObject_Click(object sender, RoutedEventArgs e)
        {
            _shapes.Add(ObjectFactory.Instance.CreateRandomRectangle());
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            SyncPool.Instance.Destroy();
        }

        private void TestX_Click(object sender, RoutedEventArgs e)
        {
            if (_shapes.Count <= 0) return;

            var rect = _shapes[0] as SyncRectangle;
            var x = rect.X;
            rect.X = x + 20;
        }

        private void TestY_Click(object sender, RoutedEventArgs e)
        {
            if (_shapes.Count <= 0) return;

            var rect = _shapes[0] as SyncRectangle;
            var y = rect.Y;
            rect.Y = y + 20;
        }

        private void TestDelete(object sender, RoutedEventArgs e)
        {
            if (_shapes.Count <= 0) return;

            _shapes.RemoveAt(0);
        }

        private void TestAdd(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 10; i++)
                _shapes.Add(ObjectFactory.Instance.CreateRandomRectangle());
        }
    }
}
