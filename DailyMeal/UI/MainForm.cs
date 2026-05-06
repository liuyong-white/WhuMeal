using System;
using System.Drawing;
using System.Windows.Forms;
using DailyMeal.BLL;
using DailyMeal.Model;

namespace DailyMeal.UI
{
    public partial class MainForm : Form
    {
        private Panel _navPanel;
        private Panel _contentPanel;
        private Panel _overviewPanel;
        private Label _lblMealCount;
        private Label _lblExpense;
        private Label _lblCalorie;
        private UserControl _currentForm;
        private StatisticBLL _statisticBll = new StatisticBLL();

        private readonly string[] _navNames = { "智能选餐", "数据管理", "统计分析", "趣味报告", "饭搭子管理", "设置" };
        private readonly Color PrimaryColor = Color.FromArgb(0x1A, 0x6B, 0x3C);
        private readonly Color BackgroundColor = Color.FromArgb(0xFF, 0xF8, 0xE7);
        private readonly Color AccentColor = Color.FromArgb(0xF5, 0xA6, 0x23);

        public MainForm()
        {
            InitializeComponent();
            SetupNavigation();
            LoadTodayOverview();
        }

        private void InitializeComponent()
        {
            this.Text = "珞珈食记";
            this.Size = new Size(1024, 768);
            this.MinimumSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BackgroundColor;

            _navPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 160,
                BackColor = PrimaryColor,
                Padding = new Padding(10, 20, 10, 10)
            };

            var titleLbl = new Label
            {
                Text = "珞珈食记",
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            _navPanel.Controls.Add(titleLbl);

            var rightPanel = new Panel { Dock = DockStyle.Fill };

            _overviewPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(0xFF, 0xF5, 0xE1),
                Padding = new Padding(15)
            };

            var overviewTitle = new Label
            {
                Text = "今日概览",
                Font = new Font("微软雅黑", 12, FontStyle.Bold),
                ForeColor = PrimaryColor,
                Location = new Point(15, 10),
                AutoSize = true
            };

            _lblMealCount = new Label { Text = "就餐次数: 0", Font = new Font("微软雅黑", 9f), Location = new Point(15, 40), AutoSize = true };
            _lblExpense = new Label { Text = "消费: ¥0.00", Font = new Font("微软雅黑", 9f), Location = new Point(150, 40), AutoSize = true };
            _lblCalorie = new Label { Text = "摄入热量: 0千卡", Font = new Font("微软雅黑", 9f), Location = new Point(300, 40), AutoSize = true };

            _overviewPanel.Controls.AddRange(new Control[] { overviewTitle, _lblMealCount, _lblExpense, _lblCalorie });

            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BackgroundColor,
                Padding = new Padding(10)
            };

            rightPanel.Controls.Add(_contentPanel);
            rightPanel.Controls.Add(_overviewPanel);

            this.Controls.Add(rightPanel);
            this.Controls.Add(_navPanel);
        }

        private void SetupNavigation()
        {
            int y = 70;
            for (int i = 0; i < _navNames.Length; i++)
            {
                var btn = new Button
                {
                    Text = _navNames[i],
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 0 },
                    Size = new Size(140, 40),
                    Location = new Point(10, y),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("微软雅黑", 10f),
                    Tag = i,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(0x15, 0x5A, 0x32);
                btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(0x10, 0x48, 0x28);
                btn.Click += NavButton_Click;
                _navPanel.Controls.Add(btn);
                y += 50;
            }
        }

        private async void NavButton_Click(object sender, EventArgs e)
        {
            int index = (int)((Button)sender).Tag;
            SwitchToForm(index);
        }

        public void SwitchToForm(int index)
        {
            if (_currentForm != null)
            {
                _contentPanel.Controls.Remove(_currentForm);
                _currentForm.Dispose();
            }

            switch (index)
            {
                case 0: _currentForm = new MealSelectForm(this); break;
                case 1: _currentForm = new DataManageForm(this); break;
                case 2: _currentForm = new StatisticForm(this); break;
                case 3: _currentForm = new ReportForm(this); break;
                case 4: _currentForm = new BuddyManageForm(this); break;
                case 5: _currentForm = new SettingsForm(this); break;
            }

            if (_currentForm != null)
            {
                _currentForm.Dock = DockStyle.Fill;
                _contentPanel.Controls.Add(_currentForm);
            }
        }

        public async void LoadTodayOverview()
        {
            try
            {
                var overview = await _statisticBll.GetTodayOverviewAsync();
                _lblMealCount.Text = $"就餐次数: {overview.MealCount}";
                _lblExpense.Text = $"消费: ¥{overview.TotalExpense:F2}";
                _lblCalorie.Text = $"摄入热量: {overview.TotalCalorie:F0}千卡";
            }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }
    }
}
