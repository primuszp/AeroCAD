using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Primusz.AeroCAD.Presentation.ViewModels;

namespace Primusz.AeroCAD.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PreviewKeyDown += OnPreviewKeyDown;

            Loaded += (s, e) =>
            {
                var vm = new MainViewModel(Viewport);
                DataContext = vm;

                // Sample data
                var layer1 = vm.AddLayer("Layer 1", Colors.Red);
                var layer2 = vm.AddLayer("Layer 2", Colors.Turquoise);

                layer1.IsActive = true;

                var line1 = new Core.Drawing.Entities.Line(new Point(0, 0), new Point(100, 100)) { Thickness = 3 };
                var line2 = new Core.Drawing.Entities.Line(new Point(100, 100), new Point(400, 100)) { Thickness = 3 };

                vm.AddEntity(layer1.Layer, line1);
                vm.AddEntity(layer2.Layer, line2);
            };
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel == null)
                return;

            e.Handled = viewModel.TryHandleShortcut(e, IsTextInputSource(e.OriginalSource as DependencyObject));
        }

        private static bool IsTextInputSource(DependencyObject source)
        {
            while (source != null)
            {
                if (source is TextBoxBase)
                    return true;

                source = VisualTreeHelper.GetParent(source);
            }

            return false;
        }
    }
}

