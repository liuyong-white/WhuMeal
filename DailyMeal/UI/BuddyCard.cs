using System;
using System.Drawing;
using System.Windows.Forms;
using DailyMeal.Model;

namespace DailyMeal.UI
{
    public partial class BuddyCard : UserControl
    {
        private DinnerBuddy _buddy;
        private PictureBox _pic;
        private Label _lblName;
        private Button _btnEdit, _btnDelete;
        private Label _lblSystem;

        public event Action<DinnerBuddy> EditClicked;
        public event Action<DinnerBuddy> DeleteClicked;

        public BuddyCard(DinnerBuddy buddy, bool isSelf)
        {
            _buddy = buddy;
            InitializeComponent(isSelf);
        }

        private void InitializeComponent(bool isSelf)
        {
            this.Size = new Size(150, 180);
            this.BackColor = Color.FromArgb(0xFF, 0xF5, 0xE1);
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Margin = new Padding(5);

            _pic = new PictureBox
            {
                Size = new Size(80, 80),
                Location = new Point(35, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.LightGray
            };
            var img = Helper.ImageHelper.LoadImage(_buddy.Photo);
            if (img != null) _pic.Image = img;

            _lblName = new Label
            {
                Text = _buddy.Name,
                Font = new Font("微软雅黑", 10f),
                Location = new Point(10, 95),
                Width = 130,
                TextAlign = ContentAlignment.MiddleCenter
            };

            if (isSelf)
            {
                _lblSystem = new Label
                {
                    Text = "[系统]",
                    ForeColor = Color.Gray,
                    Font = new Font("微软雅黑", 8f),
                    Location = new Point(55, 115),
                    AutoSize = true
                };
                this.Controls.Add(_lblSystem);
            }
            else
            {
                _btnEdit = new Button { Text = "编辑", Location = new Point(15, 140), Size = new Size(55, 25), FlatStyle = FlatStyle.Flat };
                _btnDelete = new Button { Text = "删除", Location = new Point(80, 140), Size = new Size(55, 25), FlatStyle = FlatStyle.Flat, ForeColor = Color.Red };
                _btnEdit.Click += (s, e) => EditClicked?.Invoke(_buddy);
                _btnDelete.Click += (s, e) => DeleteClicked?.Invoke(_buddy);
                this.Controls.Add(_btnEdit);
                this.Controls.Add(_btnDelete);
            }

            this.Controls.Add(_pic);
            this.Controls.Add(_lblName);
        }

        public DinnerBuddy Buddy => _buddy;
    }
}
