using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace SPNP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ThreadingButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            new ThreadingWindow().ShowDialog();
            this.Show();
        }

        private void SynchroButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            try
            {
                new SynchroWindow().ShowDialog();
            }
            catch { }
            this.Show();
        }

        private void TaskButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            new TaskWindow().ShowDialog();
            this.Show();
        }

        private void CancellingButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            new CancelWindow().ShowDialog();
            this.Show();
        }

        private static Mutex? mutex;
        private const string mutexName = "SPNP_MPW_MUTEX";

        private void ProcessButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mutex = Mutex.OpenExisting(mutexName);
            }
            catch { }
            if (mutex != null)
            {
                if (!mutex.WaitOne(1))
                {
                    string message = "Запущено інший екземпляр вікна";
                    MessageBox.Show(message);
                    return;
                }
            }
            else
            {
                mutex = new Mutex(true, mutexName);
            }
            this.Hide();
            try { new ProcessWindow().ShowDialog(); } catch { }
            this.Show();
            mutex?.ReleaseMutex();
        }

        private void ChainingButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            new ChainingWindow().ShowDialog();
            this.Show();
        }
    }
}
