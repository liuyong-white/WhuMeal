using System;
using System.Drawing;
using System.Windows.Forms;
using DailyMeal.BLL;
using DailyMeal.Model;
using DailyMeal.UI.Theme;

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
        private Button[] _navButtons;
        private int _currentNavIndex = -1;

        private readonly string[] _navNames = { "随机摇号", "食堂档案", "吃货统计", "学期回顾", "饭搭子", "系统设置" };

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
            this.BackColor = AppTheme.Background;

            _navPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 160,
                BackColor = AppTheme.Primary,
                Padding = new Padding(10, 20, 10, 10)
            };

            var titleLbl = new Label
            {
                Text = "珞珈食记",
                ForeColor = Color.White,
                Font = AppTheme.TitleFont,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            _navPanel.Controls.Add(titleLbl);

            var rightPanel = new Panel { Dock = DockStyle.Fill };

            _overviewPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = AppTheme.Surface,
                Padding = new Padding(15)
            };

            var overviewTitle = new Label
            {
                Text = "今日概览",
                Font = AppTheme.SubtitleFont,
                ForeColor = AppTheme.Primary,
                Location = new Point(15, 10),
                AutoSize = true
            };

            _lblMealCount = new Label { Text = "就餐次数: 0", Font = AppTheme.CaptionFont, ForeColor = AppTheme.TextSecondary, Location = new Point(15, 40), AutoSize = true };
            _lblExpense = new Label { Text = "消费: ¥0.00", Font = AppTheme.CaptionFont, ForeColor = AppTheme.TextSecondary, Location = new Point(150, 40), AutoSize = true };
            _lblCalorie = new Label { Text = "摄入热量: 0千卡", Font = AppTheme.CaptionFont, ForeColor = AppTheme.TextSecondary, Location = new Point(300, 40), AutoSize = true };

            _overviewPanel.Controls.AddRange(new Control[] { overviewTitle, _lblMealCount, _lblExpense, _lblCalorie });

            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Background,
                Padding = new Padding(10)
            };

            rightPanel.Controls.Add(_contentPanel);
            rightPanel.Controls.Add(_overviewPanel);

            this.Controls.Add(rightPanel);
            this.Controls.Add(_navPanel);
        }

        private void SetupNavigation()
        {
            _navButtons = new Button[_navNames.Length];
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
                    Font = AppTheme.BodyFont,
                    Tag = i,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.MouseOverBackColor = AppTheme.PrimaryLight;
                btn.FlatAppearance.MouseDownBackColor = AppTheme.PrimaryDark;
                btn.Click += NavButton_Click;
                _navPanel.Controls.Add(btn);
                _navButtons[i] = btn;
                y += 50;
            }
        }

        private void NavButton_Click(object sender, EventArgs e)
        {
            int index = (int)((Button)sender).Tag;
            SwitchToForm(index);
        }

        public void SwitchToForm(int index)
        {
            if (_currentNavIndex >= 0 && _currentNavIndex < _navButtons.Length)
            {
                _navButtons[_currentNavIndex].BackColor = Color.Transparent;
                _navButtons[_currentNavIndex].Font = AppTheme.BodyFont;
            }

            _currentNavIndex = index;
            _navButtons[index].BackColor = AppTheme.Accent;
            _navButtons[index].Font = new Font("微软雅黑", 10f, FontStyle.Bold);

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