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
using WpCadCore.Model;

namespace WpCadCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        VectorLayer layer1 = new VectorLayer();
        VectorLayer layer2 = new VectorLayer();
        WpCadCore.Model.Polyline polyline1 = new WpCadCore.Model.Polyline();
        WpCadCore.Model.Polyline polyline2 = new WpCadCore.Model.Polyline();

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(MainWindow_MouseLeftButtonDown);
            this.MouseMove += new MouseEventHandler(MainWindow_MouseMove);
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                layer1.ClearSelectedObjects();
            }
        }

        void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {

            }
        }

        void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            layer1.HitTest(space.WorldPosition);
            layer2.HitTest(space.WorldPosition);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            polyline1.Point3dCollection.Add(new Point3d(10, 10));
            polyline1.Point3dCollection.Add(new Point3d(100, 150));
            polyline1.Point3dCollection.Add(new Point3d(345, 123));

            polyline2.Point3dCollection.Add(new Point3d(34, 56));
            polyline2.Point3dCollection.Add(new Point3d(233, 564));
            polyline2.Point3dCollection.Add(new Point3d(123, 243));

            layer1.AddChild(polyline1);
            layer2.AddChild(polyline2);

            space.Children.Add(new ScreenLayer());
            space.Children.Add(layer1);
            space.Children.Add(layer2);
        }
    }
}
