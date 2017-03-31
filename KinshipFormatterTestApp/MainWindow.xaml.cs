using ACKinshipFormatter;
using System.Windows;

namespace ACKinshipFormatterTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinshipFormatter formatter = new KinshipFormatter();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var kinship = kinshipTextBox.Text;
            kinshipFormatted.Text = formatter.Format(kinship);
        }
    }
}
