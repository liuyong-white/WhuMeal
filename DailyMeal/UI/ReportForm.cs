using System;
using System.Drawing;
using System.Windows.Forms;
using DailyMeal.BLL;
using DailyMeal.Model;
using DailyMeal.UI.Theme;

namespace DailyMeal.UI
{
    public partial class ReportForm : UserControl
    {
        private MainForm _mainForm;
        private StatisticBLL _statBll = new StatisticBLL();
        private FileOperateBLL _fileBll = new FileOperateBLL();
        private ComboBox _cmbType;
        private DateTimePicker _dtpMonth;
        private Panel _dataPanel;

        public ReportForm(MainForm mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(0xFF, 0xF8, 0xE7);

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = Color.FromArgb(0xFF, 0xF5, 0xE1) };
            var lblType = new Label { Text = "报告类型:", Location = new Point(10, 12), AutoSize = true };
            _cmbType = new ComboBox { Location = new Point(85, 9), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbType.Items.AddRange(new object[] { "月度", "学期" });
            _cmbType.SelectedIndex = 0;
            var lblMonth = new Label { Text = "月份:", Location = new Point(200, 12), AutoSize = true };
            _dtpMonth = new DateTimePicker { Location = new Point(240, 9), Width = 150, Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy年MM月" };
            var btnGenerate = new Button { Text = "生成报告", Location = new Point(410, 6), Size = new Size(90, 30) };
            ButtonStyler.ApplyPrimary(btnGenerate);
            btnGenerate.Click += BtnGenerate_Click;
            topPanel.Controls.AddRange(new Control[] { lblType, _cmbType, lblMonth, _dtpMonth, btnGenerate });

            _dataPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(0xFF, 0xF5, 0xE1), Padding = new Padding(20), AutoScroll = true };

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(0xFF, 0xF5, 0xE1) };
            var btnCsv = new Button { Text = "导出CSV", Location = new Point(20, 10), Size = new Size(100, 30) };
            ButtonStyler.ApplyPrimary(btnCsv);
            var btnExcel = new Button { Text = "导出Excel", Location = new Point(140, 10), Size = new Size(100, 30) };
            ButtonStyler.ApplyAccent(btnExcel);
            btnCsv.Click += BtnExport_Click;
            btnExcel.Click += BtnExport_Click;
            btnCsv.Tag = ExportFormat.CSV;
            btnExcel.Tag = ExportFormat.Excel;
            bottomPanel.Controls.AddRange(new Control[] { btnCsv, btnExcel });

            this.Controls.Add(_dataPanel);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(topPanel);
        }

        private ReportData _currentData;

        private async void BtnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                var date = _dtpMonth.Value;
                var start = new DateTime(date.Year, date.Month, 1);
                var end = start.AddMonths(1);
                var type = _cmbType.SelectedIndex == 0 ? ReportType.Monthly : ReportType.Semester;
                _currentData = await _statBll.GenerateReportAsync(type, start, end);
                DisplayReport(_currentData);
            }
            catch (Exception ex) { MessageBox.Show($"生成失败：{ex.Message}"); }
        }

        private void DisplayReport(ReportData data)
        {
            _dataPanel.Controls.Clear();
            int y = 20;

            var lblNormal = new Label { Text = "常规数据", Font = new Font("微软雅黑", 11, FontStyle.Bold), ForeColor = Color.FromArgb(0x1A, 0x6B, 0x3C), Location = new Point(20, y), AutoSize = true };
            y += 35;
            _dataPanel.Controls.Add(lblNormal);
            _dataPanel.Controls.Add(new Label { Text = $"总用餐次数: {data.TotalCount}", Location = new Point(30, y), AutoSize = true }); y += 25;
            _dataPanel.Controls.Add(new Label { Text = $"总消费金额: ¥{data.TotalExpense:F2}", Location = new Point(30, y), AutoSize = true }); y += 25;
            _dataPanel.Controls.Add(new Label { Text = $"平均热量: {data.AvgCalorie:F1}千卡", Location = new Point(30, y), AutoSize = true }); y += 25;
            _dataPanel.Controls.Add(new Label { Text = $"高频就餐地点: {data.TopLocation}", Location = new Point(30, y), AutoSize = true }); y += 40;

            var lblFun = new Label { Text = "趣味数据", Font = new Font("微软雅黑", 11, FontStyle.Bold), ForeColor = Color.FromArgb(0xF5, 0xA6, 0x23), Location = new Point(20, y), AutoSize = true };
            y += 35;
            _dataPanel.Controls.Add(lblFun);
            if (!string.IsNullOrEmpty(data.MostFrequentCanteen))
                _dataPanel.Controls.Add(new Label { Text = $"最常去食堂: {data.MostFrequentCanteen} ({data.MostFrequentCanteenCount}次)", Location = new Point(30, y), AutoSize = true }); y += 25;
            if (!string.IsNullOrEmpty(data.HighestExpenseCanteen))
                _dataPanel.Controls.Add(new Label { Text = $"消费最高食堂: {data.HighestExpenseCanteen} (¥{data.HighestExpenseCanteenAmount:F2})", Location = new Point(30, y), AutoSize = true }); y += 25;
            if (!string.IsNullOrEmpty(data.MostFrequentStall))
                _dataPanel.Controls.Add(new Label { Text = $"最爱档口: {data.MostFrequentStall} ({data.MostFrequentStallCount}次)", Location = new Point(30, y), AutoSize = true }); y += 25;
            if (!string.IsNullOrEmpty(data.TopBuddy))
                _dataPanel.Controls.Add(new Label { Text = $"最佳饭搭子: {data.TopBuddy} ({data.TopBuddyCount}次)", Location = new Point(30, y), AutoSize = true }); y += 25;
        }

        private async void BtnExport_Click(object sender, EventArgs e)
        {
            if (_currentData == null) { MessageBox.Show("请先生成报告"); return; }
            var btn = (Button)sender;
            var format = (ExportFormat)btn.Tag;
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = format == ExportFormat.CSV ? "CSV文件|*.csv" : "Excel文件|*.xls";
                dlg.FileName = $"珞珈食记报告_{DateTime.Now:yyyyMMdd}";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        await _fileBll.ExportReportAsync(dlg.FileName, _currentData, format, new Progress<int>(p => { }));
                        Program.SoundBLL.PlayAsync(SoundType.Success);
                        MessageBox.Show("导出成功！");
                    }
                    catch (Exception ex) { Program.SoundBLL.PlayAsync(SoundType.Error); MessageBox.Show($"导出失败：{ex.Message}"); }
                }
            }
        }
    }
}
