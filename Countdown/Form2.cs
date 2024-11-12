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

        // 保存倒计时设置
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

        // 在窗体加载时读取文件并设置倒计时
        private void Form2_Load(object sender, EventArgs e)
        {
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectPath = Path.Combine(currentDirectory, "project.txt");
            string timePath = Path.Combine(currentDirectory, "time.txt");
            string startedTimePath = Path.Combine(currentDirectory, "Startedtime.txt");

            SetTextBoxText(projectPath, textBoxProject);
            SetDateFromUnixTime(timePath, textBoxYear, textBoxMonth, textBoxDay);
            SetDateFromUnixTime(startedTimePath, textBoxStartedYear, textBoxStartedMonth, textBoxStartedDay);
            ReadAutoStartSetting();
        }

        private void SetTextBoxText(string filePath, TextBox textBox)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"文件不存在：{filePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                textBox.Text = ReadFileContent(filePath);
            }
        }

        private void SetDateFromUnixTime(string timePath, params TextBox[] textBoxes)
        {
            if (!File.Exists(timePath))
            {
                MessageBox.Show($"文件不存在：{timePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                long timeDifference;
                if (long.TryParse(ReadFileContent(timePath), out timeDifference))
                {
                    DateTime countdownDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timeDifference);
                    string formattedDate = countdownDate.ToString("yyyy-MM-dd");
                    SetTextBoxesText(formattedDate.Split('-'), textBoxes);
                }
            }
        }

        private void SetTextBoxesText(string[] dateParts, params TextBox[] textBoxes)
        {
            for (int i = 0; i < textBoxes.Length; i++)
            {
                textBoxes[i].Text = dateParts[i];
            }
        }

        // 处理自动启动设置的更改
        private void checkBoxAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            SetAutoStart(checkBoxAutoStart.Checked);
        }

        private void SetAutoStart(bool enable)
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (enable)
                {
                    registryKey.SetValue("countdown", Application.ExecutablePath);
                }
                else
                {
                    registryKey.DeleteValue("countdown", false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误：" + ex.Message);
            }
        }

        // 保存开始日期
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

        // 保存所有设置
        private void SaveAll_Click(object sender, EventArgs e)
        {
            SaveCountdown_Click(sender, e);
            SaveStartedDate_Click(sender, e);
        }

        // 将文本转换为DateTime对象
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

        // 将日期保存到文件
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

        // 从文件读取内容
        private string ReadFileContent(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"文件不存在：{filePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return File.ReadAllText(filePath);
        }

        // 读取自动启动设置
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