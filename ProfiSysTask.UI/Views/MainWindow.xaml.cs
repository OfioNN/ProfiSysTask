using ProfiSysTask.UI.ViewModels;
using System.Windows;

namespace ProfiSysTask.UI.Views {
    public partial class MainWindow : Window {
        public MainWindow(MainViewModel viewModel) {
            InitializeComponent();

            DataContext = viewModel;
        }
    }
}