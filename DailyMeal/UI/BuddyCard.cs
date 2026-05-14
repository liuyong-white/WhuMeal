using System;
using System.Drawing;
using System.Windows.Forms;
using DailyMeal.Model;
using DailyMeal.UI.Theme;

namespace DailyMeal.UI
{
    public partial class BuddyCard : UserControl
    {
        private DinnerBuddy _buddy;
        private PictureBox _pic;
        private Label _lblName;
        private Button _btnEdit, _btnDelete;
        private Label _lblSystem;
        private Panel _cardPanel;
        private Color _normalBackColor = AppTheme.Surface;
        private Color _hoverBackColor = Color.White;

        public event Action<DinnerBuddy> EditClicked;
        public event Action<DinnerBuddy> DeleteClicked;

        public BuddyCard(DinnerBuddy buddy, bool isSelf)
        {
            _buddy = buddy;
            InitializeComponent(isSelf);
        }

        private void InitializeComponent(bool isSelf)
        {
            this.Size = new Size(160, 200);
            this.BackColor = AppTheme.Background;
            this.Margin = new Padding(8);

            _cardPanel = new Panel
            {
                Size = new Size(150, 190),
                Location = new Point(5, 5),
                BackColor = _normalBackColor,
                BorderStyle = BorderStyle.None
            };
            DrawCardBorder(_cardPanel);

            _pic = new PictureBox
            {
                Size = new Size(80, 80),
                Location = new Point(35, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = AppTheme.SurfaceDark,
                BorderStyle = BorderStyle.FixedSingle
            };
            var img = Helper.ImageHelper.LoadImage(_buddy.Photo);
            if (img != null) _pic.Image = img;

            _lblName = new Label
            {
                Text = _buddy.Name,
                Font = AppTheme.BodyFont,
                ForeColor = AppTheme.TextPrimary,
                Location = new Point(5, 95),
                Width = 140,
                TextAlign = ContentAlignment.MiddleCenter
            };

            if (isSelf)
            {
                _lblSystem = new Label
                {
                    Text = "[系统]",
                    ForeColor = AppTheme.TextMuted,
                    Font = AppTheme.SmallFont,
                    Location = new Point(55, 115),
                    AutoSize = true
                };
                _cardPanel.Controls.Add(_lblSystem);
            }
            else
            {
                _btnEdit = new Button
                {
                    Text = "编辑",
                    Location = new Point(15, 145),
                    Size = new Size(55, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = AppTheme.Primary,
                    ForeColor = Color.White,
                    Font = AppTheme.CaptionFont,
                    Cursor = Cursors.Hand
                };
                _btnEdit.FlatAppearance.BorderSize = 0;
                _btnDelete = new Button
                {
                    Text = "删除",
                    Location = new Point(80, 145),
                    Size = new Size(55, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = AppTheme.Danger,
                    ForeColor = Color.White,
                    Font = AppTheme.CaptionFont,
                    Cursor = Cursors.Hand
                };
                _btnDelete.FlatAppearance.BorderSize = 0;
                _btnEdit.Click += (s, e) => EditClicked?.Invoke(_buddy);
                _btnDelete.Click += (s, e) => DeleteClicked?.Invoke(_buddy);
                _cardPanel.Controls.Add(_btnEdit);
                _cardPanel.Controls.Add(_btnDelete);
            }

            _cardPanel.Controls.Add(_pic);
            _cardPanel.Controls.Add(_lblName);
            this.Controls.Add(_cardPanel);

            _cardPanel.MouseEnter += Card_MouseEnter;
            _cardPanel.MouseLeave += Card_MouseLeave;
            _pic.MouseEnter += Card_MouseEnter;
            _pic.MouseLeave += Card_MouseLeave;
            _lblName.MouseEnter += Card_MouseEnter;
            _lblName.MouseLeave += Card_MouseLeave;
        }

        private void Card_MouseEnter(object sender, EventArgs e)
        {
            _cardPanel.BackColor = _hoverBackColor;
            _cardPanel.Invalidate();
        }

        private void Card_MouseLeave(object sender, EventArgs e)
        {
            _cardPanel.BackColor = _normalBackColor;
            _cardPanel.Invalidate();
        }

        private void DrawCardBorder(Panel panel)
        {
            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                g.DrawRectangle(new Pen(AppTheme.Border, 1), rect);
            };
        }

        public DinnerBuddy Buddy => _buddy;
    }
}