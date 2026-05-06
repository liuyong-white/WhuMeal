using System;
using System.Drawing;
using System.Windows.Forms;
using DailyMeal.BLL;
using DailyMeal.Helper;
using DailyMeal.Model;

namespace DailyMeal.UI
{
    public partial class BuddyManageForm : UserControl
    {
        private MainForm _mainForm;
        private DataManageBLL _bll = new DataManageBLL();
        private TextBox _txtName;
        private FlowLayoutPanel _cardPanel;

        public BuddyManageForm(MainForm mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();
            LoadBuddies();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(0xFF, 0xF8, 0xE7);

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(0xFF, 0xF5, 0xE1) };
            var lblName = new Label { Text = "姓名:", Location = new Point(10, 15), AutoSize = true };
            _txtName = new TextBox { Location = new Point(55, 12), Width = 150 };
            var btnAdd = new Button { Text = "新增", Location = new Point(220, 10), Size = new Size(70, 28), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0x1A, 0x6B, 0x3C), ForeColor = Color.FromArgb(0xFF, 0xF5, 0xE1) };
            btnAdd.Click += BtnAdd_Click;
            topPanel.Controls.AddRange(new Control[] { lblName, _txtName, btnAdd });

            _cardPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(0xFF, 0xF8, 0xE7), Padding = new Padding(10), AutoScroll = true };

            this.Controls.Add(_cardPanel);
            this.Controls.Add(topPanel);
        }

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            var (valid, msg) = RegexHelper.ValidateBuddyName(_txtName.Text);
            if (!valid) { Program.SoundBLL.PlayAsync(SoundType.Error); MessageBox.Show(msg); return; }
            try
            {
                await _bll.AddBuddyAsync(_txtName.Text, "");
                Program.SoundBLL.PlayAsync(SoundType.Success);
                _txtName.Text = "";
                LoadBuddies();
            }
            catch (Exception ex) { Program.SoundBLL.PlayAsync(SoundType.Error); MessageBox.Show($"新增失败：{ex.Message}"); }
        }

        private async void LoadBuddies()
        {
            _cardPanel.Controls.Clear();
            var buddies = new DAL.DinnerBuddyDAL().GetAll();
            var selfBuddy = buddies.Find(b => b.Name == "自己" && b.IsSystem);
            if (selfBuddy != null)
            {
                _cardPanel.Controls.Add(CreateCard(selfBuddy, true));
                buddies.Remove(selfBuddy);
            }
            foreach (var b in buddies)
                _cardPanel.Controls.Add(CreateCard(b, false));
        }

        private BuddyCard CreateCard(DinnerBuddy buddy, bool isSelf)
        {
            var card = new BuddyCard(buddy, isSelf);
            card.EditClicked += async (b) =>
            {
                using (var dlg = new Form())
                {
                    dlg.Text = "编辑饭搭子";
                    dlg.Size = new Size(300, 150);
                    var txt = new TextBox { Text = b.Name, Location = new Point(20, 20), Width = 200 };
                    var btnOk = new Button { Text = "确定", Location = new Point(20, 60), Size = new Size(80, 30) };
                    btnOk.Click += async (s, e2) =>
                    {
                        var (valid, msg) = RegexHelper.ValidateBuddyName(txt.Text);
                        if (!valid) { MessageBox.Show(msg); return; }
                        try { b.Name = txt.Text; await _bll.UpdateBuddyAsync(b); Program.SoundBLL.PlayAsync(SoundType.Success); dlg.Close(); LoadBuddies(); }
                        catch (Exception ex) { Program.SoundBLL.PlayAsync(SoundType.Error); MessageBox.Show(ex.Message); }
                    };
                    dlg.Controls.AddRange(new Control[] { txt, btnOk });
                    dlg.ShowDialog();
                }
            };
            card.DeleteClicked += async (b) =>
            {
                if (MessageBox.Show($"确认删除 {b.Name}？", "删除确认", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try { await _bll.DeleteBuddyAsync(b.Id); Program.SoundBLL.PlayAsync(SoundType.Success); LoadBuddies(); }
                    catch (Exception ex) { Program.SoundBLL.PlayAsync(SoundType.Error); MessageBox.Show(ex.Message); }
                }
            };
            return card;
        }
    }
}
