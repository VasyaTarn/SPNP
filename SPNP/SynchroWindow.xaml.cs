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
        private int threadCount;  // кількість активних потоків

        private static Mutex? mutex;  // синхронізація між процесами
        private const String mutexName = "SPNP_SW_MUTEX";
        public SynchroWindow()
        {
            WaitOtherInstance();
            InitializeComponent();
        }

        private void WaitOtherInstance()
        {
            try { mutex = Mutex.OpenExisting(mutexName); } catch { }
            if (mutex == null)  // перший запуск
            {
                mutex = new Mutex(true, mutexName);
            }
            else
            {
                if (!mutex.WaitOne(1))
                {
                    // Мьютекс закритий - чекаємо довше і запускаємо 
                    // вікно - таймер
                    if (new CountDownWindow(mutex).ShowDialog() != true)
                    {
                        throw new ApplicationException();
                    }
                    mutex.WaitOne();
                }
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            mutex?.ReleaseMutex();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            sum = 100;
            LogTextBlock.Text = String.Empty;
            threadCount = 12;
            for (int i = 0; i < threadCount; i++)
            {
                new Thread(AddPercentS).Start(
                    new MonthData { Month = i + 1 }
                );
            }
        }

        private Semaphore semaphore = new(2, 2);  // кількість вільних, макс. кількість
        private void AddPercentS(object? data)
        {
            var monthData = data as MonthData;
            semaphore.WaitOne();  // зменшуємо вільні місця, якщо немає - чекаємо
            Thread.Sleep(1000);
            double localSum;
            localSum = sum = sum * 1.1;
            semaphore.Release();   // звільняемо одну чергу
            Dispatcher.Invoke(() =>
                LogTextBlock.Text += $"{monthData?.Month} {localSum}\n");
        }

        // об'єкт для синхронізації
        private object sumLocker = new();
        private object countLocker = new();
        private void AddPercent(object? data)
        {
            var monthData = data as MonthData;
            Thread.Sleep(200);       // тривала операція - поза транзакцією
            double localSum;
            lock (sumLocker)         // Транзакція - зміна спільного 
            {                        // ресурсу
                localSum =           // + копія у локальну змінну
                    sum = sum * 1.1; // 
            }
            Dispatcher.Invoke(() =>   // Виведення - поза транзакцією 
            {                         // але з локальною змінною, яка не
                LogTextBlock.Text +=  // поділяється з іншими потоками
                    $"{monthData?.Month} {localSum}\n";
                // Порядок цих операцій також довільний,
            });                       // гарантується, що всі будуть виведені
            // через те, що порядок не гарантується, номер місяця не
            // годиться для визначення останнього потоку, використовуємо
            // зменшення лічильнику
            int myNumber;   // порядковий номер при завершенні
            lock (countLocker)
            {
                myNumber = --threadCount;
            }
            Thread.Sleep(1);
            if (myNumber == 0)
            {
                // додаємо підсумковий запис
                Dispatcher.Invoke(() =>
                 LogTextBlock.Text += $"---------\nresult = {sum}");
            }
        }
        /* Д.З. Реалізувати відмінність у відсотках для кожного місяця,
         * передавати ці дані у потік (серед аргументів), виводити їх у лог.
         * Реалізувати "безпечний" (синхронізований) варіант
         * перевірки потоку на те, що він виконується останній.
         * Не прибираючи та не переміщуючи Thread.Sleep(1); 
         * досягти того, щоб підсумковий
         * надпис був лише один.
         */

        class MonthData
        {
            public int Month { get; set; }
        }

        private void AddPercent4()
        {
            Thread.Sleep(200);       // тривала операція - поза транзакцією

            lock (sumLocker)         // Транзакція - зміна спільного 
            {                        // ресурсу
                sum = sum * 1.1;     // 
            }                        // 

            Dispatcher.Invoke(() =>   // Виведення - поза транзакцією 
            {                         // тому і результати непередбачувані
                LogTextBlock.Text +=  // але гарантується, що останній
                    $"{sum}\n";       // результат буде правильний
            });                       //      
        }

        private void AddPercent3()
        {                         // блок синхронізації (lock)
            lock (sumLocker)      // переводить sumLocker у "закритий" стан
            {                     // 
                double localSum = sum;    // поки sumLocker "закритий"
                Thread.Sleep(200);        // інші інструкції з блоком
                localSum *= 1.1;          // lock не починають роботу,
                sum = localSum;           // чекаючи на "відкриття"
                Dispatcher.Invoke(() =>   // об'єкту синхронізації
                {                         // Але вміщення всього тіла методу
                    LogTextBlock.Text +=  // у синхроблок призводить по 
                        $"{sum}\n";       // повної серіалізації роботи
                });                       // - втрачається асинхронність
            }                      // завершення блоку "відкриває" sumLocker
        }

        private void AddPercent2()
        {
            Thread.Sleep(200);  // ~запит  // Перенесення операцій
            double localSum = sum;         // зменшує ефект, але
            localSum *= 1.1;    // 10%     // не позбувається його
            sum = localSum;                // Числа виводяться різні
            Dispatcher.Invoke(() =>        // але з дублюванням 
            {                              // замість поступового 
                LogTextBlock.Text +=       // зростання
                    $"{sum}\n";            // 
            });                            // 
        }

        private void AddPercent1()
        {
            // Метод, що імітує звернення до мережі з одержанням
            // даних про інфляцію за місяць та додає її до загальної суми
            double localSum = sum;          // Проблема - всі потоки виводять
            Thread.Sleep(200);  // ~запит   // одне число - 110
            localSum *= 1.1;    // 10%      // Затримка підсилює проблему
            sum = localSum;                 // гарантуючи, що всі потоки
            Dispatcher.Invoke(() =>         // почнуться з 100
            {                               // Це ілюструє загальну проблему
                LogTextBlock.Text +=        // асинхронних задач - при
                    $"{sum}\n";             // роботі з спільним ресурсом
            });                             // необхідна синхронізація
        }
    }
}
