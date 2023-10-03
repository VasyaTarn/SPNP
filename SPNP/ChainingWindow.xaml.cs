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
    /// Interaction logic for ChainingWindow.xaml
    /// </summary>
    public partial class ChainingWindow : Window
    {

        private CancellationTokenSource _cancellationToken;

        public ChainingWindow()
        {
            InitializeComponent();
        }

        private void StartBtn1_Click(object sender, RoutedEventArgs e)
        {
            _cancellationToken = new CancellationTokenSource();
            showProgress(ProgressBar10, _cancellationToken.Token).ContinueWith(task => showProgress(ProgressBar11, _cancellationToken.Token));

            showProgress(ProgressBar20, _cancellationToken.Token).ContinueWith(task => showProgress(ProgressBar21, _cancellationToken.Token).ContinueWith(task2 => showProgress(ProgressBar22, _cancellationToken.Token)));
        }

        private void StopBtn1_Click(object sender, RoutedEventArgs e)
        {
            _cancellationToken.Cancel();
        }

        private async Task showProgress(ProgressBar progressBar, CancellationToken cancellationToken)
        {
            int delay = 100;
            if(progressBar == ProgressBar10) 
                delay = 100;
            if (progressBar == ProgressBar11)
                delay = 200;
            if (progressBar == ProgressBar12)
                delay = 300;

            if (progressBar == ProgressBar20)
                delay = 300;
            if (progressBar == ProgressBar21)
                delay = 200;
            if (progressBar == ProgressBar22)
                delay = 100;

            try
            {
                for (int i = 0; i <= 10; i++)
                {
                    await Task.Delay(delay);
                    Dispatcher.Invoke(() =>
                    {
                        progressBar.Value = i * 10;
                    });
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException e)
            {
                if (progressBar.Value != 100)
                {
                    int a = Convert.ToInt32(progressBar.Value) / 10;
                    for (int i = a; i > 0; i--)
                    {

                        await Task.Delay(delay);
                        Dispatcher.Invoke(() =>
                        {
                            progressBar.Value -= 10;
                        });
                    }
                }
                return;
            }
        }

        private async void StopBtn2_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private async void StartBtn2_Click(object sender, RoutedEventArgs e)
        {
            //showProgress(ProgressBar10);
            //await showProgress(ProgressBar20);
            //showProgress(ProgressBar11);
            //await showProgress(ProgressBar21);
            //showProgress(ProgressBar12);
            //await showProgress(ProgressBar22);


            
        }

        private void StartButtton3_Click(object sender, RoutedEventArgs e)
        {
            string str = "";
            AddHello(str).ContinueWith(t1 =>
            {
                string res = t1.Result;
                Dispatcher.Invoke(() => LogTextBlock.Text = res);
                return AddWorld(res);
            }).Unwrap().
            ContinueWith(t2 =>
            {
                string res = t2.Result;
                Dispatcher.Invoke(() => LogTextBlock.Text = res);
                return AddExclamation(res);
            }).Unwrap().
            ContinueWith(t => Dispatcher.Invoke(() => LogTextBlock.Text = t.Result));




        }

        private async Task<string> AddHello(string str)
        {
            await Task.Delay(1000);
            return str + " Hello ";
        }

        private async Task<string> AddWorld(string str)
        {
            await Task.Delay(1000);
            return str + " World ";
        }

        private async Task<string> AddExclamation(string str)
        {
            await Task.Delay(1000);
            return str + " !!! ";
        }
    }
}
