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
    /// Interaction logic for TaskWindow.xaml
    /// </summary>
    public partial class TaskWindow : Window
    {
        public TaskWindow()
        {
            InitializeComponent();
        }

        private void DemoButton1_Click(object sender, RoutedEventArgs e)
        {
            Task task = new Task(demo1); // об'єкт
            task.Start();                // та запуск

            Task task2 = Task.Run(demo1);  // запуск і повернення запущенної задачі
        }
        private void demo1()
        {
            Dispatcher.Invoke(() => LogTextBlock.Text += "demo1 starts\n");
            Thread.Sleep(1000);
            Dispatcher.Invoke(() => LogTextBlock.Text += "demo1 finishes\n");
        }

        // WPF дозволяє створювати async обробники подій
        private async void DemoButton2_Click(object sender, RoutedEventArgs e)
        {
            // Task<String> task = demo2();  // метод повертає Task у "робочому" стані
            // // те, що може виконуватись паралельно
            // String str = await task;  // чекає переходу Task у "завершений" стан
            // LogTextBlock.Text += $"demo2 result: {str} \n";

            // неоптимальний варіант - задачі виконуються послідовно
            // LogTextBlock.Text += $"demo2-1 result: {await demo2()} \n";
            // LogTextBlock.Text += $"demo2-2 result: {await demo2()} \n";

            Task<String> task1 = demo2();
            Task<String> task2 = demo2();
            // те, що може виконуватись паралельно
            String res = $"demo2-1 result: {await task1} \n";
            LogTextBlock.Text += res;
            res = $"demo2-1 result: {await task2} \n";
            LogTextBlock.Text += res;
        }
        private async Task<String> demo2()
        {
            // З async методів можна звертатись до UI без Dispatcher
            LogTextBlock.Text += "demo2 starts\n";
            await Task.Delay(1000);  // альтернатива Thread.Sleep
            return "Done";
        }

        private async void SequenceButton_Click(object sender, RoutedEventArgs e)
        {
            await UpdatePrBar(ProgressBar1, 300);
            await UpdatePrBar(ProgressBar2, 400);
            await UpdatePrBar(ProgressBar3, 500);
        }

        private async void ParallelismButton_Click(object sender, RoutedEventArgs e)
        {
            await Task.WhenAll(UpdatePrBar(ProgressBar1, 500), UpdatePrBar(ProgressBar2, 600), UpdatePrBar(ProgressBar3, 1000));
        }

        private async Task UpdatePrBar(ProgressBar bar, int time)
        {
            for(int i = 0; i <= 10; i++)
            {
                bar.Value = i * 10;
                await Task.Delay(time);
            }
        }
    }
}
