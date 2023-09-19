using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for SynchroWindow.xaml
    /// </summary>
    public partial class SynchroWindow : Window
    {
        private double sum;
        private CancellationTokenSource cts;
        //private double _percent; 

        public SynchroWindow()
        {
            InitializeComponent();
        }

        private int threadCount;

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Random rnd = new Random();
            cts = new CancellationTokenSource();
            sum = 100;
            LogTextBlock.Text = String.Empty;
            for (int i = 0; i < 12; i++)
            {
                double _percent = rnd.Next(0, 10) * 10;
                new Thread(AddPercent).Start(new MonthDate { Month = i + 1, PercentMonth = _percent, CancelToken = cts.Token});
                
            }
            threadCount = 12;
        }

        private object sumLocker = new();
        private void AddPercent(object? data)
        {

            var monthData = data as MonthDate; 

            Thread.Sleep(200);
            double localSum;
            lock (sumLocker)
            {
                localSum = sum = sum * (1 + monthData.PercentMonth / 100);
                
            }
            Dispatcher.Invoke(() => LogTextBlock.Text += $"{ monthData?.Month } {localSum} ({monthData.PercentMonth}%)\n");

            threadCount--;
            Thread.Sleep(1);
            if(threadCount == 0 ) 
            {
                Dispatcher.Invoke(() => {
                    if (!monthData.CancelToken.IsCancellationRequested)
                    {
                        cts.Cancel();
                        LogTextBlock.Text += $"----\nresult = {sum}";
                    }
                });
            }

        }

        class MonthDate
        {
            public int Month { get; set; }
            public CancellationToken CancelToken { get; set; }
            public double PercentMonth { get; set; }

        }
        private void AddPercent3()
        {
            lock (sumLocker)
            {
                double localSum = sum;

                Thread.Sleep(200);

                localSum *= 1.1;
                sum = localSum;

                Dispatcher.Invoke(() => LogTextBlock.Text += $"{sum}\n");

            }
        }

        private void AddPercent2()
        {
            Thread.Sleep(200);
            double localSum = sum;
            localSum *= 1.1;
            sum = localSum;
            Dispatcher.Invoke(() => LogTextBlock.Text += $"{sum}\n");
        }

        private void AddPercent1()
        {
            double localSum = sum;

            Thread.Sleep(200);

            localSum *= 1.1;
            sum = localSum;

            Dispatcher.Invoke(() => LogTextBlock.Text += $"{sum}\n");
        }
    }
}
