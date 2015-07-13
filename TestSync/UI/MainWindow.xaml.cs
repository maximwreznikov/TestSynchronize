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
            SyncPool.Instance.Startup();

            SyncPool.Instance.AttachCollection(_shapes);
            _shapes.Add(ObjectPool.Instance.CreateRandomRectangle());
        }


        private void AddObject_Click(object sender, RoutedEventArgs e)
        {
            _shapes.Add(ObjectPool.Instance.CreateRandomRectangle());
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            SyncPool.Instance.Destroy();
        }

        private void TestX_Click(object sender, RoutedEventArgs e)
        {
            var rect = _shapes[0] as SyncRectangle;
            var x = rect.X;
            rect.X = x + 100;
        }
    }
}
