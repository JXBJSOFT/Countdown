using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Forms;

namespace Countdown
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void SaveCountdown_Click(object sender, EventArgs e)
        {
            string yearText = textBoxYear.Text;
            string monthText = textBoxMonth.Text;
            string dayText = textBoxDay.Text;

            DateTime countdownDate = ConvertToDateTime(yearText, monthText, dayText);
            if (countdownDate == DateTime.MinValue) return;

            TimeSpan timeDifference = countdownDate - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long unixTimestamp = (long)timeDifference.TotalSeconds;

            string projectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "project.txt");
            string timePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "time.txt");

            SaveDateToFile(projectPath, textBoxProject.Text);
            SaveDateToFile(timePath, unixTimestamp.ToString());
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectPath = Path.Combine(currentDirectory, "project.txt");
            string timePath = Path.Combine(currentDirectory, "time.txt");
            string startedTimePath = Path.Combine(currentDirectory, "Startedtime.txt");

            if (!File.Exists(projectPath))
            {
                MessageBox.Show($"文件不存在：{projectPath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                textBoxProject.Text = ReadFileContent(projectPath);
            }

            if (!File.Exists(timePath))
            {
                MessageBox.Show($"文件不存在：{timePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                long timeDifference;
                if (long.TryParse(ReadFileContent(timePath), out timeDifference))
                {
                    DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    DateTime countdownDate = start.AddSeconds(timeDifference);
                    string formattedDate = countdownDate.ToString("yyyy-MM-dd");
                    textBoxYear.Text = formattedDate.Split('-')[0];
                    textBoxMonth.Text = formattedDate.Split('-')[1];
                    textBoxDay.Text = formattedDate.Split('-')[2];
                }
            }

            if (!File.Exists(startedTimePath))
            {
                MessageBox.Show($"文件不存在：{startedTimePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                long startedTimeDifference;
                if (long.TryParse(ReadFileContent(startedTimePath), out startedTimeDifference))
                {
                    DateTime startedCountdownDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(startedTimeDifference);
                    string startedFormattedDate = startedCountdownDate.ToString("yyyy-MM-dd");
                    textBoxStartedYear.Text = startedFormattedDate.Split('-')[0];
                    textBoxStartedMonth.Text = startedFormattedDate.Split('-')[1];
                    textBoxStartedDay.Text = startedFormattedDate.Split('-')[2];
                }
            }

            ReadAutoStartSetting();
        }

        private void checkBoxAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkBoxAutoStart.Checked)
                {
                    RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    registryKey.SetValue("countdown", Application.ExecutablePath);
                }
                else
                {
                    RegistryKey key = Registry.CurrentUser;
                    key.DeleteValue("countdown", false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误：" + ex.Message);
            }
        }

        private void SaveStartedDate_Click(object sender, EventArgs e)
        {
            string StartedYearText = textBoxStartedYear.Text;
            string StartedMonthText = textBoxStartedMonth.Text;
            string StartedDayText = textBoxStartedDay.Text;

            DateTime StartedCountdownDate = ConvertToDateTime(StartedYearText, StartedMonthText, StartedDayText);
            if (StartedCountdownDate == DateTime.MinValue) return;

            TimeSpan timeDifference = StartedCountdownDate - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long unixTimestamp = (long)timeDifference.TotalSeconds;

            string timePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Startedtime.txt");
            SaveDateToFile(timePath, unixTimestamp.ToString());
        }

        private void SaveAll_Click(object sender, EventArgs e)
        {
            SaveCountdown_Click(sender, e);
            SaveStartedDate_Click(sender, e);
        }

        private DateTime ConvertToDateTime(string yearText, string monthText, string dayText)
        {
            try
            {
                return new DateTime(int.Parse(yearText), int.Parse(monthText), int.Parse(dayText));
            }
            catch (FormatException ex)
            {
                MessageBox.Show($"错误：请将此信息提供给程序开发者：{ex}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return DateTime.MinValue;
            }
        }

        private void SaveDateToFile(string filePath, string content)
        {
            try
            {
                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存文件时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void ReadAutoStartSetting()
        {
            string runKeyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
            using (RegistryKey runKey = Registry.CurrentUser.OpenSubKey(runKeyPath))
            {
                if (runKey != null)
                {
                    string countdownPath = (string)runKey.GetValue("countdown");
                    checkBoxAutoStart.Checked = countdownPath != null && countdownPath.Equals(Application.ExecutablePath, StringComparison.OrdinalIgnoreCase);
                }
            }
        }
    }
}