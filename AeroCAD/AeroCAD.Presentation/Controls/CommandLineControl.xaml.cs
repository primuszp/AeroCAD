using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Input;
using Primusz.AeroCAD.Presentation.ViewModels;

namespace Primusz.AeroCAD.Presentation.Controls
{
    public partial class CommandLineControl : UserControl
    {
        public CommandLineControl()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void CmdLine_KeyDown(object sender, KeyEventArgs e)
        {
            var viewModel = DataContext as CommandLineViewModel;
            if (viewModel == null)
                return;

            switch (e.Key)
            {
                case Key.Enter:
                    viewModel.SubmitCurrentInput();
                    scroll.ScrollToEnd();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    viewModel.RequestCancel();
                    e.Handled = true;
                    break;
                case Key.Up:
                    viewModel.RecallPrevious();
                    MoveCaretToEnd();
                    e.Handled = true;
                    break;
                case Key.Down:
                    viewModel.RecallNext();
                    MoveCaretToEnd();
                    e.Handled = true;
                    break;
                case Key.Right:
                    viewModel.AutocompleteFromLast();
                    MoveCaretToEnd();
                    e.Handled = true;
                    break;
            }
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is CommandLineViewModel oldViewModel)
                oldViewModel.Messages.CollectionChanged -= OnMessagesChanged;

            if (e.NewValue is CommandLineViewModel newViewModel)
                newViewModel.Messages.CollectionChanged += OnMessagesChanged;
        }

        private void OnMessagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            scroll.ScrollToEnd();
        }

        private void MoveCaretToEnd()
        {
            input.SelectionLength = 0;
            input.SelectionStart = input.Text.Length;
        }
    }
}

