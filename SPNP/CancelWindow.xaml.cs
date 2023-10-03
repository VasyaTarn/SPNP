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
using System.Windows.Shapes;

namespace SPNP
{
    /// <summary>
    /// Interaction logic for CancelWindow.xaml
    /// </summary>
    public partial class CancelWindow : Window
    {
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _cancellationTokenSource2;
        private CancellationTokenSource _cancellationTokenSource3;

        public CancelWindow()
        {
            InitializeComponent();
            _cancellationTokenSource = null!;
        }

        private void StopBtn1_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }

        private async void StartBtn1_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            RunProgressCancellable(ProgressBar10, _cancellationTokenSource.Token);
            RunProgressCancellable(ProgressBar11, _cancellationTokenSource.Token, 4);
            RunProgressCancellable(ProgressBar12, _cancellationTokenSource.Token, 2);
            
        }

        private async void RunProgress(ProgressBar progressBar, CancellationToken ct, int time = 5)
        {
            Dispatcher.Invoke(() => progressBar.Value = 0);
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    Dispatcher.Invoke(() => progressBar.Value += 10);
                    await Task.Delay(1000 * time / 10);
                    ct.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException)
            {
                for (int i = Convert.ToInt32(progressBar.Value); i > 0; i--)
                {
                    progressBar.Value -= 10;
                    await Task.Delay(1000 * time / 10);
                    if (progressBar.Value == 0)
                        break;
                }
            }
        }

        private async Task RunProgressWaitable(ProgressBar progressBar, CancellationToken ct, int time = 4)
        {
            progressBar.Value = 0;
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    progressBar.Value += 10;
                    await Task.Delay(1000 * time / 10);
                    ct.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException)
            {
                for (int i = Convert.ToInt32(progressBar.Value); i > 0; i--)
                {
                    progressBar.Value -= 10;
                    await Task.Delay(1000 * time / 10);
                    if (progressBar.Value == 0)
                        break;
                }
            }
        }

        private async void RunProgressCancellable(ProgressBar progressBar, CancellationToken ct, int time = 3)
        {
            progressBar.Value = 0;
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    progressBar.Value += 10;
                    await Task.Delay(1000 * time / 10);
                    ct.ThrowIfCancellationRequested();
                }               
                
            }
            catch (OperationCanceledException) 
            {
                for (int i = Convert.ToInt32(progressBar.Value); i > 0; i--)
                {
                    progressBar.Value -= 10;
                    await Task.Delay(1000 * time / 10);
                    if (progressBar.Value == 0)
                        break;
                }
            }
            
        }

        private async void StartBtn2_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource2 = new CancellationTokenSource();
            await Task.WhenAll(RunProgressWaitable(ProgressBar20, _cancellationTokenSource2.Token, 5), RunProgressWaitable(ProgressBar21, _cancellationTokenSource2.Token, 2), RunProgressWaitable(ProgressBar22, _cancellationTokenSource2.Token, 6));
            MessageBox.Show("All tasks have finished.");
        }

        private void StopBtn2_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource2.Cancel();
        }

        private void StartBtn3_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource3 = new CancellationTokenSource();
            RunProgress(ProgressBar30, _cancellationTokenSource3.Token);
            RunProgress(ProgressBar31, _cancellationTokenSource3.Token, 4);
            RunProgress(ProgressBar32, _cancellationTokenSource3.Token, 2);
        }

        private void StopBtn3_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource3.Cancel();
        }
    }
}
