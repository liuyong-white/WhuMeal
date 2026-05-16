using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using DailyMeal.BLL;
using DailyMeal.Model;
using DailyMeal.UI.Theme;

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
            var grpPeriod = new Panel { Location = new Point(80, 2), Size = new Size(260, 36), BackColor = Color.Transparent };
            _rbWeek = new RadioButton { Text = "周", Location = new Point(0, 8), Size = new Size(50, 24), Checked = true };
            _rbMonth = new RadioButton { Text = "月", Location = new Point(55, 8), Size = new Size(50, 24) };
            _rbYear = new RadioButton { Text = "年", Location = new Point(110, 8), Size = new Size(50, 24) };
            _rbSemester = new RadioButton { Text = "学期", Location = new Point(165, 8), Size = new Size(70, 24) };
            _rbWeek.CheckedChanged += Period_Changed;
            _rbMonth.CheckedChanged += Period_Changed;
            _rbYear.CheckedChanged += Period_Changed;
            _rbSemester.CheckedChanged += Period_Changed;
            grpPeriod.Controls.AddRange(new Control[] { _rbWeek, _rbMonth, _rbYear, _rbSemester });
            topPanel.Controls.AddRange(new Control[] { lbl, grpPeriod });

            var leftPanel = new Panel { Dock = DockStyle.Left, Width = 400, BackColor = AppTheme.Surface };
            _chart = new Chart { Dock = DockStyle.Fill, BackColor = AppTheme.Surface };
            var chartArea = new ChartArea("Main");
            chartArea.BackColor = AppTheme.Surface;
            chartArea.Area3DStyle.Enable3D = true;
            chartArea.Area3DStyle.Inclination = 15;
            _chart.ChartAreas.Add(chartArea);
            var series = new Series("Default");
            series.ChartType = SeriesChartType.Pie;
            series["PieLabelStyle"] = "Outside";
            series.Font = AppTheme.CaptionFont;
            series.BorderColor = Color.White;
            series.BorderWidth = 2;
            _chart.Series.Add(series);
            var legend = new Legend
            {
                Docking = Docking.Bottom,
                Font = AppTheme.CaptionFont,
                BackColor = AppTheme.Surface,
                LegendStyle = LegendStyle.Row
            };
            _chart.Legends.Add(legend);
            leftPanel.Controls.Add(_chart);

            _gvDetail = new DataGridView { Dock = DockStyle.Fill };
            DataGridViewStyler.ApplyStyle(_gvDetail);
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
            LoadStats();
        }

        private async void LoadStats()
        {
            try
            {
                var canteenResult = await _bll.CalculateCanteenStatsAsync(_currentPeriod, null, null);
                UpdateChart(canteenResult.CanteenStats.Select(c => (c.CanteenName, c.Count, c.Percentage)).ToList());

                var stallResult = await _bll.CalculateAllStallStatsAsync(_currentPeriod, null, null);
                _gvDetail.DataSource = stallResult.StallStats.Select(s => new { 食堂 = s.CanteenName, 档口 = s.StallName, 次数 = s.Count, 消费 = s.TotalExpense.ToString("F2"), 占比 = s.Percentage.ToString("F1") + "%" }).ToList();

                Program.SoundBLL.PlayAsync(SoundType.Interact);
            }
            catch { }
        }

        private void UpdateChart(List<(string name, int count, double pct)> data)
        {
            _chart.Series["Default"].Points.Clear();
            var colors = AppTheme.ChartColors;
            for (int i = 0; i < data.Count; i++)
            {
                _chart.Series["Default"].Points.Add(data[i].count);
                _chart.Series["Default"].Points[i].LegendText = data[i].name;
                _chart.Series["Default"].Points[i].Label = $"{data[i].name}\n{data[i].pct:F1}%";
                _chart.Series["Default"].Points[i].Color = colors[i % colors.Length];
            }
        }
    }
}
