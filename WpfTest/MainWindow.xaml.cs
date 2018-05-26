using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace WpfTest
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("ntdll.dll")]
        private static extern uint RtlAdjustPrivilege(
            int Privilege,
            bool bEnablePrivilege,
            bool IsThreadPrivilege,
            out bool PreviousValue
        );

        [DllImport("ntdll.dll")]
        private static extern uint NtRaiseHardError(
            uint ErrorStatus,
            uint NumberOfParameters,
            uint UnicodeStringParameterMask,
            IntPtr Parameters,
            uint ValidResponseOption,
            out uint Response
        );

        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(Int32 vKey);

        [DllImport("User32.dll")]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("User32.dll")]
        private static extern bool CloseClipboard();

        [DllImport("User32.dll")]
        private static extern bool EmptyClipboard();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            startButton.IsEnabled = false;
            progressBar.Visibility = Visibility.Visible;
            progressBar.IsIndeterminate = true;

            Thread thread = new Thread(() =>
            {
                bool bl;
                uint response;
                RtlAdjustPrivilege(19, true, false, out bl);
                NtRaiseHardError(0xC0000420, 0, 0, (IntPtr)null, 6, out response);
            });
            thread.IsBackground = true;
            thread.Start();
        }

        bool keyloggerRunning = false;
        private void TabablzControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (menu.SelectedIndex == 1 && !keyloggerRunning)
            {
                Thread thread = new Thread(() =>
                {
                    while (true)
                    {
                        for (int i = 65; i < 90; i++)
                        {
                            if ((GetAsyncKeyState(i) & 0x0001) > 0)
                            {
                                Application.Current.Dispatcher.Invoke(new Action(() => { keyloggerTextBlock.Text += (char)i; }));
                            }

                        }
                        Thread.Sleep(100);
                    }
                });
                thread.IsBackground = true;
                thread.Start();
                keyloggerRunning = true;
            }
        }

        private void clipBoardBlockStart_Click(object sender, RoutedEventArgs e)
        {
            if (OpenClipboard((IntPtr)null))
            {
                clipBoardStartButton.IsEnabled = false;
                clipBoardStopButton.IsEnabled = true;
            }
        }

        private void clipBoardBlockStop_Click(object sender, RoutedEventArgs e)
        {
            clipBoardStartButton.IsEnabled = true;
            clipBoardStopButton.IsEnabled = false;

            EmptyClipboard();
            CloseClipboard();
        }

        Thread killExplorerThread;
        private void killExplorerStart_Click(object sender, RoutedEventArgs e)
        {
            killExplorerStartButton.IsEnabled = false;
            killExplorerStopButton.IsEnabled = true;

            killExplorerThread = new Thread(() => {
                for(int i = 0; i < 2; i++)
                {
                    var processes = Process.GetProcessesByName("explorer");
                    foreach (var process in processes)
                    {
                        process.Kill();
                        process.WaitForExit();
                    }
                    Thread.Sleep(100);
                }
            });
            killExplorerThread.IsBackground = true;
            killExplorerThread.Start();
        }

        private void killExplorerStop_Click(object sender, RoutedEventArgs e)
        {
            killExplorerStartButton.IsEnabled = true;
            killExplorerStopButton.IsEnabled = false;

            Process.Start(Environment.GetEnvironmentVariable("windir") + "\\explorer.exe");
        }

        private void change_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            Nullable<bool> result = dialog.ShowDialog();

            if (result == true)
            {
                string fileName = dialog.FileName;

                DateTime time = (DateTime)timePicker.SelectedTime;
                DateTime date = (DateTime)datePicker.SelectedDate;
                DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);

                File.SetCreationTime(fileName, dateTime);
                File.SetLastWriteTime(fileName, dateTime);
                File.SetLastAccessTime(fileName, dateTime);
            }
        }

        private void window_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        bool dateSet = false;
        bool timeSet = false;
        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            dateSet = true;
            if (timeSet)
                changeButton.IsEnabled = true;
        }

        private void timePicker_TextChanged(object sender, TextChangedEventArgs e)
        {
            timeSet = true;
            if (dateSet)
                changeButton.IsEnabled = true;
        }
    }
}
