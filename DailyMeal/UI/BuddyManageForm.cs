using System;
using System.Drawing;
using System.Windows.Forms;
using DailyMeal.BLL;
using DailyMeal.Helper;
using DailyMeal.Model;
using DailyMeal.UI.Theme;

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
            this.BackColor = AppTheme.Background;

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = AppTheme.Surface };
            var lblName = new Label { Text = "姓名:", Location = new Point(10, 15), AutoSize = true };
            _txtName = new TextBox { Location = new Point(55, 12), Width = 120 };
            var lblPhoto = new Label { Text = "照片:", Location = new Point(190, 15), AutoSize = true };
            var txtPhoto = new TextBox { Location = new Point(230, 12), Width = 180, ReadOnly = true, BackColor = Color.White };
            var btnBrowse = new Button { Text = "浏览", Location = new Point(415, 10), Size = new Size(55, 28) };
            ButtonStyler.ApplyPrimary(btnBrowse);
            btnBrowse.Click += (s, e) =>
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                    dlg.Title = "选择照片";
                    if (dlg.ShowDialog() == DialogResult.OK)
                        txtPhoto.Text = dlg.FileName;
                }
            };
            var btnAdd = new Button { Text = "新增", Location = new Point(480, 10), Size = new Size(70, 28) };
            ButtonStyler.ApplyAccent(btnAdd);
            btnAdd.Click += (s, e) => BtnAdd_Click(s, e, txtPhoto.Text);
            topPanel.Controls.AddRange(new Control[] { lblName, _txtName, lblPhoto, txtPhoto, btnBrowse, btnAdd });

            _cardPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = AppTheme.Background, Padding = new Padding(10), AutoScroll = true };

            this.Controls.Add(_cardPanel);
            this.Controls.Add(topPanel);
        }

        private async void BtnAdd_Click(object sender, EventArgs e, string photoPath)
        {
            var (valid, msg) = RegexHelper.ValidateBuddyName(_txtName.Text);
            if (!valid) { Program.SoundBLL.PlayAsync(SoundType.Error); MessageBox.Show(msg); return; }
            try
            {
                await _bll.AddBuddyAsync(_txtName.Text, photoPath);
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
            var selfBuddy = buddies.Find(b => b.Name == "老己" && b.IsSystem);
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
                    dlg.Size = new Size(350, 220);
                    dlg.StartPosition = FormStartPosition.CenterParent;
                    int y = 15;
                    dlg.Controls.Add(new Label { Text = "姓名:", Location = new Point(20, y + 5), AutoSize = true });
                    var txt = new TextBox { Text = b.Name, Location = new Point(70, y), Width = 200 };
                    dlg.Controls.Add(txt);
                    y += 35;
                    dlg.Controls.Add(new Label { Text = "照片:", Location = new Point(20, y + 5), AutoSize = true });
                    var txtPhoto = new TextBox { Text = b.Photo, Location = new Point(70, y), Width = 200, ReadOnly = true, BackColor = Color.White };
                    dlg.Controls.Add(txtPhoto);
                    var btnBrowse = new Button { Text = "浏览", Location = new Point(275, y - 2), Size = new Size(50, 26) };
                    ButtonStyler.ApplyPrimary(btnBrowse);
                    btnBrowse.Click += (s2, e2) =>
                    {
                        using (var ofd = new OpenFileDialog())
                        {
                            ofd.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                            ofd.Title = "选择照片";
                            if (ofd.ShowDialog() == DialogResult.OK)
                                txtPhoto.Text = ofd.FileName;
                        }
                    };
                    dlg.Controls.Add(btnBrowse);
                    y += 45;
                    var btnOk = new Button { Text = "确定", Location = new Point(70, y), Size = new Size(80, 30) };
                    ButtonStyler.ApplyPrimary(btnOk);
                    var btnCancel = new Button { Text = "取消", Location = new Point(160, y), Size = new Size(80, 30), FlatStyle = FlatStyle.Flat };
                    btnCancel.Click += (s2, e2) => dlg.Close();
                    btnOk.Click += async (s2, e2) =>
                    {
                        var (valid, msg) = RegexHelper.ValidateBuddyName(txt.Text);
                        if (!valid) { MessageBox.Show(msg); return; }
                        try
                        {
                            b.Name = txt.Text;
                            if (!string.IsNullOrWhiteSpace(txtPhoto.Text) && txtPhoto.Text != b.Photo)
                            {
                                if (!string.IsNullOrWhiteSpace(b.Photo))
                                    ImageHelper.DeleteLocalImage(b.Photo);
                                b.Photo = ImageHelper.CopyToLocalStorage(txtPhoto.Text, "Buddy", b.Id);
                            }
                            await _bll.UpdateBuddyAsync(b);
                            Program.SoundBLL.PlayAsync(SoundType.Success);
                            dlg.Close();
                            LoadBuddies();
                        }
                        catch (Exception ex) { Program.SoundBLL.PlayAsync(SoundType.Error); MessageBox.Show(ex.Message); }
                    };
                    dlg.Controls.AddRange(new Control[] { btnOk, btnCancel });
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
