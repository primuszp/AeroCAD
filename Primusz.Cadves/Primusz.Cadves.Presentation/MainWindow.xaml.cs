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
using Primusz.Cadves.Core.Drawing;
using Primusz.Cadves.Core.Drawing.Entities;
using Primusz.Cadves.Core.Drawing.Layers;

namespace Primusz.Cadves.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                ModelSpace ms = new ModelSpace(Viewport);

                Line line1 = new Line(new Point(0, 0), new Point(100, 100)) { Thickness = 3 };
                Line line2 = new Line(new Point(100, 100), new Point(400, 100)) { Thickness = 3 };

                Layer layer1 = new Layer();
                Layer layer2 = new Layer();

                Viewport.AddLayer(layer1);
                Viewport.AddLayer(layer2);

                layer1.Color = Colors.Red;
                layer2.Color = Colors.Turquoise;

                layer1.Add(line1);
                layer2.Add(line2);
            };
        }
    }
}
