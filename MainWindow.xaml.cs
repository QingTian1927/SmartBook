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
using SmartBook.Views;

namespace SmartBook
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainWindow? _instance;

        public static MainWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MainWindow();
                }
                return _instance;
            }
        }
        
        public MainWindow()
        {
            _instance = this;
            InitializeComponent();

            Title = "SmartBook - Login";
            Navigate(new LoginView());
        }

        public void Navigate(Page page)
        {
            MainFrame.Navigate(page);
        }
    }
}