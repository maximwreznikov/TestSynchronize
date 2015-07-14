﻿using System;
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

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
//            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
//            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Handle messages...

            return IntPtr.Zero;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            SyncPool.Instance.Startup();

            SyncPool.Instance.AttachCollection(_shapes);

            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
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
            var rect = _shapes[0] as SyncRectangle;
            var x = rect.X;
            rect.X = x + 100;
        }
    }
}
