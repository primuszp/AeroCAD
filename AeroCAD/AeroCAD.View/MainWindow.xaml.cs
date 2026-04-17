using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Primusz.AeroCAD.View.ViewModels;

namespace Primusz.AeroCAD.View
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
            PreviewTextInput += OnPreviewTextInput;

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

                CommandInput.FocusInput();
            };
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsTextInputSource(e.OriginalSource as DependencyObject))
            {
                if (e.Key == Key.Back)
                {
                    CommandInput.RemoveLastCharacter();
                    e.Handled = true;
                    return;
                }

                if (e.Key != Key.LeftShift &&
                    e.Key != Key.RightShift &&
                    e.Key != Key.LeftCtrl &&
                    e.Key != Key.RightCtrl &&
                    e.Key != Key.LeftAlt &&
                    e.Key != Key.RightAlt &&
                    e.Key != Key.Tab)
                {
                    CommandInput.FocusInput();
                }
            }

            var viewModel = DataContext as MainViewModel;
            if (viewModel == null)
                return;

            e.Handled = viewModel.TryHandleShortcut(e, IsTextInputSource(e.OriginalSource as DependencyObject));
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (IsTextInputSource(e.OriginalSource as DependencyObject))
                return;

            CommandInput.AppendText(e.Text);
            e.Handled = true;
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


