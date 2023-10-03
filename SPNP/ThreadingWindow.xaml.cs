using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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
    /// Interaction logic for ThreadingWindow.xaml
    /// </summary>
    public partial class ThreadingWindow : Window
    {

        private static Mutex? mutex;
        private const string mutexName = "SPNP_TW_MUTEX";

        public ThreadingWindow()
        {
            CheckPreviousLunch();
            InitializeComponent();
        }

        private void CheckPreviousLunch()
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
                    mutex = null;
                    throw new ApplicationException(message);
                }
            }
            else
            {
                mutex = new Mutex(true, mutexName);
            }
        }


        private void StartButton1_Click(object sender, RoutedEventArgs e)
        {
            // демонстрація проблеми - зависання інтерфейсу
            // протягом роботи методу-обробника події усі інші події
            // стають у чергу і не обробляються
            for (int i = 0; i < 10; i++)
            {
                ProgressBar1.Value = i * 10;
                Thread.Sleep(300);
            }
            ProgressBar1.Value = 100;
            // оновлення вікна - це теж одна з подій, тому бігунок
            // відображається відразу заповненим, а не покроково
        }

        private void StopButton1_Click(object sender, RoutedEventArgs e)
        {
            // через зависання інтерфейсу кнопка не натискається протягом
            // роботи "Старт", жодні дії не зможуть зупинити її роботу
        }

        private void StartButton2_Click(object sender, RoutedEventArgs e)
        {
            new Thread(IncrementProgress).Start();
        }

        private void StopButton2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void IncrementProgress()
        {
            /* Проблема - з даного потоку не можна змінювати елементи,
             * які належать іншому потоку. Для доступу до елементів
             * інтерфейсу (вікна) слід делегувати виконання змін до
             * UI (user interface) потоку.
             */
            for (int i = 0; i < 10; i++)
            {
                ProgressBar2.Value = i * 10;
                Thread.Sleep(300);
            }
            ProgressBar2.Value = 100;
        }


        private bool isStopped3 { get; set; }
        private void StartButton3_Click(object sender, RoutedEventArgs e)
        {
            new Thread(IncrementProgress3).Start();
            isStopped3 = false;
        }

        private void StopButton3_Click(object sender, RoutedEventArgs e)
        {
            isStopped3 = true;
        }
        private void IncrementProgress3()
        {
            for (int i = 0; i <= 10 && !isStopped3; i++)
            {
                /* Делегування виконання дії (лямбди) до віконного (UI) потоку
                 * 
                 */
                this.Dispatcher.Invoke(
                    () => ProgressBar3.Value = i * 10
                );
                Thread.Sleep(300);
            }
        }

        #region 4
        private bool IsStopped4 { get; set; }
        private Thread? thread4;
        private void StartButton4_Click(object sender, RoutedEventArgs e)
        {
            if (thread4 == null)
            {
                IsStopped4 = false;
                thread4 = new Thread(IncrementProgress4);
                thread4.Start();
                StartButton4.IsEnabled = false;
            }
            // Завдання: поки виконується потік кнопка Старт - неактивна,
            // Стоп - активна; коли зупиняється чи завершується - навпаки
        }

        private void StopButton4_Click(object sender, RoutedEventArgs e)
        {
            stopHadle();
        }
        private void stopHadle()
        {
            IsStopped4 = true;
            thread4 = null;
            StartButton4.IsEnabled = true;
        }
        private void IncrementProgress4()
        {
            for (int i = 0; i <= 10 && !IsStopped4; i++)
            {
                this.Dispatcher.Invoke(
                    () => ProgressBar4.Value = i * 10
                );
                Thread.Sleep(300);
            }
            // thread4 = null;  // до змінних можна звертатись на пряму
            // this.Dispatcher.Invoke(() =>  // а до елементів UI - тільки через Диспетчер
            // {  
            //     StartButton4.IsEnabled = true;
            // });
            this.Dispatcher.Invoke(stopHadle);  // комплексне рішення
        }
        #endregion

        #region 5 Передача даних у потік
        private Thread? thread5;
        // Зупинка потоків - сучасний підхід
        CancellationTokenSource cts;  // джерело токенів скасування
        private void StartButton5_Click(object sender, RoutedEventArgs e)
        {
            int workTime = Convert.ToInt32(WorktimeTextBox.Text);
            thread5 = new Thread(IncrementProgress5);
            cts = new();  // нове джерело
            thread5.Start(new ThreadData5  // об'єкт для потоку передається у Start
            {
                Worktime = workTime,
                CancelToken = cts.Token  // get - одержання токену з джерела
            });
        }

        private void StopButton5_Click(object sender, RoutedEventArgs e)
        {
            // скасування потоків здійснюється через джерело токенів
            cts?.Cancel();
            // після цієї команди усі токени даного джерела переходять
            // у скасований стан, але безпосередньо на потоки це не 
            // вплине, перевірка стану токенів
            // має окремо здійснюватись у кожному потоці у тих місцях,
            // у яких можливе припинення роботи.
        }

        private void IncrementProgress5(object? parameter)
        {
            /* Аргументом може бути довільний об'єкт, але параметр
             * прийме його як object. Відповідно, першими командами
             * є перетворення типу та перевірка успішності.
             * Для передачі кількох аргументів, їх поєднують в один
             * класс/об'єкт
             */
            if (parameter is ThreadData5 data)
            {
                for (int i = 0; i <= 10; i++)
                {
                    this.Dispatcher.Invoke(
                        () => ProgressBar5.Value = i * 10
                    );
                    Thread.Sleep(100 * data.Worktime);

                    // задача перевірки токена на скасованість - частина
                    // роботи потоку (скасування не впливає на потік, якщо
                    // ми це будемо ігнорувати)
                    if (data.CancelToken.IsCancellationRequested)
                    {
                        break;
                    }
                    // або за допомогою викидання виключення:
                    // data.CancelToken.ThrowIfCancellationRequested();
                }
            }
            else
            {
                MessageBox.Show("Thread 5 started with invalid argument");
            }
        }

        class ThreadData5
        {
            public int Worktime { get; set; }

            // токен, створений джерелом (CTS), передається серед даних у потік
            public CancellationToken CancelToken { get; set; }
        }

        #endregion

        // ---------------------------------- Д.З. ------------------------------------

        private Thread? thread6;
        private Thread? thread7;
        private Thread? thread8;
        CancellationTokenSource? cts2;  
        private void StopButton6_Click(object sender, RoutedEventArgs e)
        {
            cts2?.Cancel();
        }

        private void StartButton6_Click(object sender, RoutedEventArgs e)
        {
            int workTime1 = Convert.ToInt32(WorktimeTextBox2.Text);
            int workTime2 = Convert.ToInt32(WorktimeTextBox3.Text);
            int workTime3 = Convert.ToInt32(WorktimeTextBox4.Text);
            thread6 = new Thread(IncrementProgress6);
            thread7 = new Thread(IncrementProgress7);
            thread8 = new Thread(IncrementProgress8);
            cts2 = new();  
            thread6.Start(new ThreadData6  
            {
                Worktime1 = workTime1,
                Worktime2 = workTime2,
                Worktime3 = workTime3,
                CancelToken = cts2.Token  
            });

            thread7.Start(new ThreadData6  
            {
                Worktime1 = workTime1,
                Worktime2 = workTime2,
                Worktime3 = workTime3,
                CancelToken = cts2.Token  
            });

            thread8.Start(new ThreadData6  
            {
                Worktime1 = workTime1,
                Worktime2 = workTime2,
                Worktime3 = workTime3,
                CancelToken = cts2.Token  
            });
        }

        private void IncrementProgress6(object? parameter)
        {
            
            if (parameter is ThreadData6 data)
            {
                for (int i = 0; i <= 10; i++)
                {
                    this.Dispatcher.Invoke(
                        () => ProgressBar6.Value = i * 10
                    );
                    Thread.Sleep(100 * data.Worktime1);

                    
                    if (data.CancelToken.IsCancellationRequested)
                    {
                        break;
                    }
                    
                }
            }
            else
            {
                MessageBox.Show("Thread 5 started with invalid argument");
            }
        }

        private void IncrementProgress7(object? parameter)
        {

            if (parameter is ThreadData6 data)
            {
                for (int i = 0; i <= 10; i++)
                {
                    this.Dispatcher.Invoke(
                        () => ProgressBar7.Value = i * 10
                    );
                    Thread.Sleep(100 * data.Worktime2);

                    
                    if (data.CancelToken.IsCancellationRequested)
                    {
                        break;
                    }
                    
                }
            }
            else
            {
                MessageBox.Show("Thread 5 started with invalid argument");
            }
        }

        private void IncrementProgress8(object? parameter)
        {

            if (parameter is ThreadData6 data)
            {
                for (int i = 0; i <= 10; i++)
                {
                    this.Dispatcher.Invoke(
                        () => ProgressBar8.Value = i * 10
                    );
                    Thread.Sleep(100 * data.Worktime3);

                    
                    if (data.CancelToken.IsCancellationRequested)
                    {
                        break;
                    }
                    
                }
            }
            else
            {
                MessageBox.Show("Thread 5 started with invalid argument");
            }
        }

        class ThreadData6
        {
            public int Worktime1 { get; set; }
            public int Worktime2 { get; set; }
            public int Worktime3 { get; set; }

            // токен, створений джерелом (CTS), передається серед даних у потік
            public CancellationToken CancelToken { get; set; }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            mutex?.ReleaseMutex();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
