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
            /*
             * 
             * 可恶的汪杨迪，不理她了，哼！
             * 就很无语😅😅😅😅😅😅😅
             *
             */
        }

        // 初始化用于监控演示软件的定时器
        private void InitializePresentationMonitorTimer()
        {
            presentationMonitorTimer = new Timer();
            presentationMonitorTimer.Interval = 1000; // 设置定时器间隔为1秒
            presentationMonitorTimer.Tick += CheckPresentationApplications;
            presentationMonitorTimer.Start();
        }

        // 检查是否有演示软件正在运行
        private void CheckPresentationApplications(object sender, EventArgs e)
        {
            Process[] powerPointProcesses = Process.GetProcessesByName("powerpnt"); // PowerPoint
            Process[] wpsPresentationProcesses = Process.GetProcessesByName("wpp"); // WPS Presentation

            bool isPowerPointRunning = powerPointProcesses.Length > 0;
            bool isWpsPresentationRunning = wpsPresentationProcesses.Length > 0;

            ToggleWindowState(isPowerPointRunning || isWpsPresentationRunning);
        }

        // 根据演示软件是否运行来切换窗口状态
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

        // 在窗体加载时读取文件并设置倒计时
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
                MessageBox.Show($"错误：{ex.Message}\n\n你的目标已经结束了，快来再设置一个吧~", "错误");
                Form2 form2 = new Form2();
                form2.Show();
            }
        }

        // 最小化窗体
        private void MinimizeForm(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        // 点击图片框时打开设置窗体
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }

        // 读取文件内容
        private string ReadFileContent(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"文件不存在：{filePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return File.ReadAllText(filePath);
        }

        // 调整控件位置以适应窗体大小变化
        private void Form1_Resize(object sender, EventArgs e)
        {
            ProgressToTime.Location = new Point((this.ClientSize.Width - ProgressToTime.Width) / 2, 0);
            label1.Location = new Point((this.ClientSize.Width - label1.Width) / 2, this.ClientSize.Height / 8); // 距离xx还有
            label2.Location = new Point((this.ClientSize.Width - label2.Width) / 2, label1.Location.Y + 60); // xx天
            pictureBox1.Location = new Point((this.ClientSize.Width - pictureBox1.Width) / 2, label2.Location.Y + 115); // logo
            ProgressToTime.Width = this.ClientSize.Width;
        }
    }
}