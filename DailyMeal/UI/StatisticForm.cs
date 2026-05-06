using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using DailyMeal.BLL;
using DailyMeal.Model;

namespace DailyMeal.UI
{
    public partial class StatisticForm : UserControl
    {
        private MainForm _mainForm;
        private StatisticBLL _bll = new StatisticBLL();
        private RadioButton _rbWeek, _rbMonth, _rbYear, _rbSemester;
        private Chart _chart;
        private DataGridView _gvDetail;
        private PeriodType _currentPeriod = PeriodType.Week;
        private int? _drillCanteenId = null;
        private Button _btnBack;

        public StatisticForm(MainForm mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();
            LoadStats();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(0xFF, 0xF8, 0xE7);

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.FromArgb(0xFF, 0xF5, 0xE1) };
            var lbl = new Label { Text = "统计周期:", Location = new Point(10, 10), AutoSize = true };
            _rbWeek = new RadioButton { Text = "周", Location = new Point(90, 10), Checked = true };
            _rbMonth = new RadioButton { Text = "月", Location = new Point(140, 10) };
            _rbYear = new RadioButton { Text = "年", Location = new Point(190, 10) };
            _rbSemester = new RadioButton { Text = "学期", Location = new Point(240, 10) };
            _rbWeek.CheckedChanged += Period_Changed;
            _rbMonth.CheckedChanged += Period_Changed;
            _rbYear.CheckedChanged += Period_Changed;
            _rbSemester.CheckedChanged += Period_Changed;
            _btnBack = new Button { Text = "返回食堂维度", Location = new Point(350, 5), Size = new Size(110, 28), FlatStyle = FlatStyle.Flat, Visible = false };
            _btnBack.Click += (s, e) => { _drillCanteenId = null; _btnBack.Visible = false; LoadStats(); };
            topPanel.Controls.AddRange(new Control[] { lbl, _rbWeek, _rbMonth, _rbYear, _rbSemester, _btnBack });

            var leftPanel = new Panel { Dock = DockStyle.Left, Width = 400, BackColor = Color.FromArgb(0xFF, 0xF5, 0xE1) };
            _chart = new Chart { Dock = DockStyle.Fill };
            _chart.ChartAreas.Add(new ChartArea("Main"));
            _chart.Series.Add(new Series("Default"));
            _chart.Series["Default"].ChartType = SeriesChartType.Pie;
            _chart.Series["Default"]["PieLabelStyle"] = "Outside";
            _chart.Legends.Add(new Legend { Docking = Docking.Bottom });
            _chart.MouseClick += Chart_MouseClick;
            leftPanel.Controls.Add(_chart);

            _gvDetail = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            var rightPanel = new Panel { Dock = DockStyle.Fill };
            rightPanel.Controls.Add(_gvDetail);

            this.Controls.Add(rightPanel);
            this.Controls.Add(leftPanel);
            this.Controls.Add(topPanel);
        }

        private void Period_Changed(object sender, EventArgs e)
        {
            if (_rbWeek.Checked) _currentPeriod = PeriodType.Week;
            else if (_rbMonth.Checked) _currentPeriod = PeriodType.Month;
            else if (_rbYear.Checked) _currentPeriod = PeriodType.Year;
            else if (_rbSemester.Checked) _currentPeriod = PeriodType.Semester;
            _drillCanteenId = null;
            _btnBack.Visible = false;
            LoadStats();
        }

        private async void LoadStats()
        {
            try
            {
                if (_drillCanteenId.HasValue)
                {
                    var result = await _bll.CalculateStallStatsAsync(_drillCanteenId.Value, _currentPeriod, null, null);
                    UpdateChart(result.StallStats.Select(s => (s.StallName, s.Count, s.Percentage)).ToList());
                    _gvDetail.DataSource = result.StallStats.Select(s => new { 名称 = s.StallName, 次数 = s.Count, 消费 = s.TotalExpense.ToString("F2"), 占比 = s.Percentage.ToString("F1") + "%" }).ToList();
                }
                else
                {
                    var result = await _bll.CalculateCanteenStatsAsync(_currentPeriod, null, null);
                    UpdateChart(result.CanteenStats.Select(c => (c.CanteenName, c.Count, c.Percentage)).ToList());
                    _gvDetail.DataSource = result.CanteenStats.Select(c => new { 名称 = c.CanteenName, 次数 = c.Count, 消费 = c.TotalExpense.ToString("F2"), 占比 = c.Percentage.ToString("F1") + "%" }).ToList();
                }
                Program.SoundBLL.PlayAsync(SoundType.Interact);
            }
            catch { }
        }

        private void UpdateChart(System.Collections.Generic.List<(string name, int count, double pct)> data)
        {
            _chart.Series["Default"].Points.Clear();
            var colors = new[] { Color.FromArgb(0x1A, 0x6B, 0x3C), Color.FromArgb(0xF5, 0xA6, 0x23), Color.FromArgb(0x7B, 0xC0, 0x4A), Color.FromArgb(0xE7, 0x4C, 0x3C), Color.FromArgb(0x9B, 0x59, 0xB6), Color.FromArgb(0x1A, 0xBC, 0x9C), Color.FromArgb(0x2E, 0x8B, 0x57), Color.FromArgb(0xE6, 0x7E, 0x22) };
            for (int i = 0; i < data.Count; i++)
            {
                _chart.Series["Default"].Points.Add(data[i].count);
                _chart.Series["Default"].Points[i].LegendText = data[i].name;
                _chart.Series["Default"].Points[i].Label = $"{data[i].name}\n{data[i].pct:F1}%";
                _chart.Series["Default"].Points[i].Color = colors[i % colors.Length];
            }
        }

        private async void Chart_MouseClick(object sender, MouseEventArgs e)
        {
            if (_drillCanteenId.HasValue) return;
            var result = _chart.HitTest(e.X, e.Y);
            if (result.ChartElementType == ChartElementType.DataPoint)
            {
                int pointIndex = result.PointIndex;
                var canteens = await new BLL.DataManageBLL().GetAllCanteensAsync();
                if (pointIndex >= 0 && pointIndex < canteens.Count)
                {
                    _drillCanteenId = canteens[pointIndex].Id;
                    _btnBack.Visible = true;
                    LoadStats();
                }
            }
        }
    }
}
