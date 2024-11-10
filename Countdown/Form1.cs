using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Countdown
{
    public partial class Form1 : Form
    {
        private Timer presentationMonitorTimer;

        public Form1()
        {
            InitializeComponent();
            InitializePresentationMonitorTimer();
        }

        private void InitializePresentationMonitorTimer()
        {
            presentationMonitorTimer = new Timer();
            presentationMonitorTimer.Interval = 1000; // Set the timer interval to 1 second
            presentationMonitorTimer.Tick += CheckPresentationApplications;
            presentationMonitorTimer.Start();
        }

        private void CheckPresentationApplications(object sender, EventArgs e)
        {
            Process[] powerPointProcesses = Process.GetProcessesByName("powerpnt"); // PowerPoint
            Process[] wpsPresentationProcesses = Process.GetProcessesByName("wpp"); // WPS Presentation

            bool isPowerPointRunning = powerPointProcesses.Length > 0;
            bool isWpsPresentationRunning = wpsPresentationProcesses.Length > 0;

            ToggleWindowState(isPowerPointRunning || isWpsPresentationRunning);
        }

        private void ToggleWindowState(bool isMinimized)
        {
            if (isMinimized && WindowState != FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Minimized;
            }
            else if (!isMinimized && WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string projectPath = Path.Combine(currentDirectory, "project.txt");
                string timePath = Path.Combine(currentDirectory, "time.txt");
                string StartedTimePath = Path.Combine(currentDirectory, "Startedtime.txt");

                string projectText = ReadFileContent(projectPath);
                label1.Text = $"距离{projectText}还有";

                DateTime today = DateTime.Today;
                long unixTimestampToday = ((DateTimeOffset)today).ToUnixTimeSeconds();
                long timestampFromFile = long.Parse(ReadFileContent(timePath));
                long timeDifference = timestampFromFile - unixTimestampToday;
                long differenceInDays = timeDifference / 86400;
                label2.Text = differenceInDays.ToString() + "天";

                label1.Location = new Point((this.ClientSize.Width - label1.Width) / 2, 52); // 距离xx还有
                label2.Location = new Point((this.ClientSize.Width - label2.Width) / 2, 119); // xx天
                pictureBox1.Location = new Point((this.ClientSize.Width - pictureBox1.Width) / 2, 245); // logo

                if (!File.Exists(StartedTimePath))
                {
                    ProgressToTime.Visible = false;
                }
                else
                {
                    long startedUnix = long.Parse(ReadFileContent(StartedTimePath));
                    DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(startedUnix);
                    long startedUnixTime = ((DateTimeOffset)start).ToUnixTimeSeconds();
                    int startedDays = (int)(startedUnix / 86400);
                    int endDays = (int)(timestampFromFile / 86400);
                    int todayDays = (int)(unixTimestampToday / 86400);

                    // 计算进度条的最大值
                    int maxDays = endDays - startedDays;
                    ProgressToTime.Maximum = maxDays;

                    // 计算当前进度
                    int currentDays = todayDays - startedDays;
                    ProgressToTime.Value = currentDays;

                    // 设置进度条的最小值
                    ProgressToTime.Minimum = 0;

                    ProgressToTime.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\n你的目标已经结束了，快来再设置一个吧~", "Error");
                Form2 form2 = new Form2();
                form2.Show();
            }
        }

        private void MinimizeForm(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }

        private string ReadFileContent(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"文件不存在：{filePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return File.ReadAllText(filePath);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            label1.Location = new Point((this.ClientSize.Width - label1.Width) / 2, this.ClientSize.Height / 8); // 距离xx还有
            label2.Location = new Point((this.ClientSize.Width - label2.Width) / 2, label1.Location.Y + 60); // xx天
            pictureBox1.Location = new Point((this.ClientSize.Width - pictureBox1.Width) / 2, label2.Location.Y + 120); // logo
        }
    }
}