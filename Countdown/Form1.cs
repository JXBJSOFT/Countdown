using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

/*
    我服了这个汪杨迪！！！！
        竟然敢不理我！
 */

namespace Countdown
{
    public partial class Form1 : Form
    {
        private Timer presentationMonitorTimer;
        private const string ProjectFilePath = "project.txt";
        private const string TimeFilePath = "time.txt";
        private const string StartedTimeFilePath = "Startedtime.txt";
        private const string ColorFilePath = "color.txt";

        public Form1()
        {
            InitializeComponent();
            InitializePresentationMonitorTimer();
        }

        private void InitializePresentationMonitorTimer()
        {
            presentationMonitorTimer = new Timer();
            presentationMonitorTimer.Interval = 1000; // 1s刷新1次
            presentationMonitorTimer.Tick += CheckPresentationApplications;
            presentationMonitorTimer.Start();
        }

        private void CheckPresentationApplications(object sender, EventArgs e)
        {
            Process[] powerPointProcesses = Process.GetProcessesByName("powerpnt"); // PowerPoint
            Process[] wpsPresentationProcesses = Process.GetProcessesByName("wpp"); // WPS

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
            LoadSettings();
            SetLabelsAndPictureBoxLocations();
            CheckAndSetProgressBarVisibility();
        }

        private void LoadSettings()
        {
            string projectText = ReadFileContent(ProjectFilePath);
            if (projectText != null)
            {
                label1.Text = $"距离{projectText}还有";
            }

            long unixTimestampToday = ((DateTimeOffset)DateTime.Today).ToUnixTimeSeconds();
            string timeContent = ReadFileContent(TimeFilePath);
            long timestampFromFile = timeContent != null ? long.Parse(timeContent) : unixTimestampToday;
            long timeDifference = timestampFromFile - unixTimestampToday;
            long differenceInDays = timeDifference / 86400;
            label2.Text = differenceInDays.ToString() + "天";

            try
            {
                string hexColor = ReadFileContent(ColorFilePath);
                if (hexColor != null)
                {
                    Color color = ColorTranslator.FromHtml(hexColor);
                    this.BackColor = color;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法更改背景颜色: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetLabelsAndPictureBoxLocations()
        {
            label1.Location = new Point((this.ClientSize.Width - label1.Width) / 2, 52);
            label2.Location = new Point((this.ClientSize.Width - label2.Width) / 2, 119);
            pictureBox1.Location = new Point((this.ClientSize.Width - pictureBox1.Width) / 2, 245);
        }

        private void CheckAndSetProgressBarVisibility()
        {
            if (!File.Exists(StartedTimeFilePath))
            {
                ProgressToTime.Visible = false;
                return;
            }

            string startedTimeContent = ReadFileContent(StartedTimeFilePath);
            long startedUnix = startedTimeContent != null ? long.Parse(startedTimeContent) : 0;
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(startedUnix);
            long startedUnixTime = ((DateTimeOffset)start).ToUnixTimeSeconds();
            int startedDays = (int)(startedUnix / 86400);
            string timeContent = ReadFileContent(TimeFilePath);
            int endDays = timeContent != null ? (int)(long.Parse(timeContent) / 86400) : 0;
            int todayDays = (int)(((DateTimeOffset)DateTime.Today).ToUnixTimeSeconds() / 86400);

            int maxDays = endDays - startedDays;
            ProgressToTime.Maximum = maxDays;
            int currentDays = todayDays - startedDays;
            ProgressToTime.Value = currentDays;
            ProgressToTime.Minimum = 0;
            ProgressToTime.Visible = true;
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
            SetLabelsAndPictureBoxLocations();
            ProgressToTime.Width = this.ClientSize.Width;
        }
    }
}