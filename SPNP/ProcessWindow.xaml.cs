using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for ProcessWindow.xaml
    /// </summary>
    public partial class ProcessWindow : Window
    {

        private static Mutex? mutex;
        private const string mutexName = "SPNP_PW_MUTEX";

        public ProcessWindow()
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
                    //Close();
                    //return;
                    throw new ApplicationException(message);
                }
            }
            else
            {
                mutex = new Mutex(true, mutexName);
            }
        }

        private void ShowProcesses_Click(object sender, RoutedEventArgs e)
        {
            Process[] processes = Process.GetProcesses();
            //ProcTextBlock.Text = "";
            ProcTreeView.Items.Clear();
            string prevName = "";
            TreeViewItem item = null;
            foreach (Process process in processes.OrderBy(p => p.ProcessName)) 
            {
                var subItem = new TreeViewItem() { Header = String.Format("{0} {1}", process.Id, process.ProcessName), Tag = process };

                if (prevName != process.ProcessName)
                {
                    prevName = process.ProcessName;
                    item = new TreeViewItem() { Header = prevName };
                    ProcTreeView.Items.Add(item);
                }

                subItem.MouseDoubleClick += TreeViewItem_MouseDoubleClick;
                item?.Items.Add(subItem);
                //ProcTextBlock.Text += String.Format("{0} {1}\n", process.Id, process.ProcessName);
            }
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(sender is TreeViewItem item) 
            {
                string message = "";
                if(item.Tag is Process process) 
                {
                    TimeSpan cpuTime = process.TotalProcessorTime;
                    double cpuTimeMilliseconds = cpuTime.TotalMilliseconds;
                    long memoryUsage = process.WorkingSet64;
                    double memoryUsageKB = memoryUsage / 1024.0;
                    double memoryUsageMB = memoryUsageKB / 1024.0;
                    int threadCount = process.Threads.Count;
                    message += "CPU time consumption: " + cpuTimeMilliseconds + " miliseconds\r\n";
                    message += "RAM consumption: " + memoryUsageMB.ToString("F2") + " MB\r\n";
                    message += "Total number of threads: " + threadCount + "\r\n";
                }
                else
                {
                    message = "No process in tag";
                }
                MessageBox.Show(message);
            }
        }

        private Process? notepadProcess;

        private void StartNotepad_Click(object sender, RoutedEventArgs e)
        {
            notepadProcess ??= Process.Start("notepad.exe");
        }

        private void StopNotepad_Click(object sender, RoutedEventArgs e)
        {
            notepadProcess?.CloseMainWindow();
            notepadProcess?.Kill(true);
            notepadProcess?.WaitForExit();
            notepadProcess?.Dispose();
            notepadProcess = null;
        }

        private void StartEditNotepad_Click(object sender, RoutedEventArgs e)
        {
            string dir = AppContext.BaseDirectory;
            int binPos = dir.LastIndexOf("bin");
            string projectRoot = dir[..binPos];

            notepadProcess ??= Process.Start("notepad.exe", $"{projectRoot}ProcessWindow.xaml.cs");
        }

        private Process? browserProcess;

        private void StartBrowserNotepad_Click(object sender, RoutedEventArgs e)
        {
            string fileName = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
            if (System.IO.File.Exists(fileName))
            {
                browserProcess ??= Process.Start(fileName, "-url itstep.org");
            }
            else
            {
                MessageBox.Show("Browser not installed");
            }
        }
        private Process? calcProcess;

        private void StartCalculatorNotepad_Click(object sender, RoutedEventArgs e)
        {
            string fileName = @"C:\WINDOWS\system32\calc.exe";
            if (System.IO.File.Exists(fileName))
            {
                calcProcess ??= Process.Start(fileName);
            }
            else
            {
                MessageBox.Show("Calculator not installed");
            }
        }

        private void StopCalculatorNotepad_Click(object sender, RoutedEventArgs e)
        {
            calcProcess?.CloseMainWindow();
            calcProcess?.Kill();
            calcProcess?.WaitForExit();
            calcProcess?.Dispose();
            calcProcess = null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    mutex = Mutex.OpenExisting(mutexName);
            //} catch { }
            //if(mutex != null ) 
            //{
            //    if (!mutex.WaitOne(1))
            //    {
            //        MessageBox.Show("Запущено інший екземпляр вікна");
            //        mutex = null;
            //        Close();
            //        return;
            //    }
            //}
            //else 
            //{
            //    mutex = new Mutex(true, mutexName);
            //}
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            mutex?.ReleaseMutex();
            mutex?.Dispose();

        }

        private Process? dispatcherProcess;
        private void StartDispatcher_Click(object sender, RoutedEventArgs e)
        {
            dispatcherProcess ??= Process.Start(@"C:\WINDOWS\system32\Taskmgr.exe");
        }

        private void StopDispatcher_Click(object sender, RoutedEventArgs e)
        {
            dispatcherProcess?.CloseMainWindow();
            dispatcherProcess?.Kill(true);
            dispatcherProcess?.WaitForExit();
            dispatcherProcess?.Dispose();
            dispatcherProcess = null;
        }

        private Process? parametersProcess;
        private void StartParameters_Click(object sender, RoutedEventArgs e)
        {
            string settingsUri = "ms-settings:";

            parametersProcess ??= Process.Start(new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = settingsUri,
                UseShellExecute = true
            });
        }

        private void StopParameters_Click(object sender, RoutedEventArgs e)
        {
            parametersProcess?.CloseMainWindow();
            parametersProcess?.Kill(true);
            parametersProcess?.WaitForExit();
            parametersProcess?.Dispose();
            parametersProcess = null;
        }
    }
}
