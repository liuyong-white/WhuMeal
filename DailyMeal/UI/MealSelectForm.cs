using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DailyMeal.BLL;
using DailyMeal.Helper;
using DailyMeal.Model;

namespace DailyMeal.UI
{
    public partial class MealSelectForm : UserControl
    {
        private MainForm _mainForm;
        private MealSelectBLL _selectBll = new MealSelectBLL();
        private DataManageBLL _manageBll = new DataManageBLL();
        private CheckedListBox _candidateList;
        private PictureBox _rollImage;
        private Label _rollName;
        private ComboBox _groupCombo;
        private Button _btnStart;
        private Button _btnConfirm;
        private Button _btnDirect;
        private CancellationTokenSource _cts;
        private List<Stall> _allStalls = new List<Stall>();
        private Stall _selectedStall;
        private Panel _rollPanel;
        private Panel _directPanel;

        public MealSelectForm(MainForm mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();
            _ = LoadDataAsync();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(0xFF, 0xF8, 0xE7);

            var leftPanel = new Panel { Dock = DockStyle.Left, Width = 200, BackColor = Color.FromArgb(0xFF, 0xF5, 0xE1), Padding = new Padding(5) };
            var lblCandidate = new Label { Text = "候选档口", Font = new Font("微软雅黑", 10, FontStyle.Bold), Dock = DockStyle.Top, Height = 30, TextAlign = ContentAlignment.MiddleCenter };
            _candidateList = new CheckedListBox { Dock = DockStyle.Fill, Font = new Font("微软雅黑", 9f), CheckOnClick = true };
            leftPanel.Controls.Add(_candidateList);
            leftPanel.Controls.Add(lblCandidate);

            _rollPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(0xFF, 0xF5, 0xE1) };
            _rollImage = new PictureBox { Size = new Size(200, 200), Location = new Point(150, 50), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.LightGray };
            _rollName = new Label { Font = new Font("微软雅黑", 14, FontStyle.Bold), Location = new Point(100, 270), AutoSize = true, ForeColor = Color.FromArgb(0x1A, 0x6B, 0x3C) };
            _rollPanel.Controls.Add(_rollImage);
            _rollPanel.Controls.Add(_rollName);

            var rightPanel = new Panel { Dock = DockStyle.Right, Width = 150, BackColor = Color.FromArgb(0xFF, 0xF5, 0xE1), Padding = new Padding(5) };
            var lblGroup = new Label { Text = "选餐分组", Font = new Font("微软雅黑", 9, FontStyle.Bold), Dock = DockStyle.Top, Height = 25 };
            _groupCombo = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
            _groupCombo.SelectedIndexChanged += GroupCombo_SelectedIndexChanged;
            var btnAddGroup = new Button { Text = "管理分组", Dock = DockStyle.Top, Height = 30, FlatStyle = FlatStyle.Flat };
            btnAddGroup.Click += BtnAddGroup_Click;
            rightPanel.Controls.Add(btnAddGroup);
            rightPanel.Controls.Add(_groupCombo);
            rightPanel.Controls.Add(lblGroup);

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(0xFF, 0xF5, 0xE1) };
            _btnStart = new Button { Text = "开始选餐", Size = new Size(100, 35), Location = new Point(50, 8), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0x1A, 0x6B, 0x3C), ForeColor = Color.FromArgb(0xFF, 0xF5, 0xE1) };
            _btnConfirm = new Button { Text = "确认选餐", Size = new Size(100, 35), Location = new Point(170, 8), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0xF5, 0xA6, 0x23), ForeColor = Color.FromArgb(0xFF, 0xF5, 0xE1), Enabled = false };
            _btnDirect = new Button { Text = "直接录入", Size = new Size(100, 35), Location = new Point(290, 8), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0x1A, 0x6B, 0x3C), ForeColor = Color.FromArgb(0xFF, 0xF5, 0xE1) };
            _btnStart.Click += BtnStart_Click;
            _btnConfirm.Click += BtnConfirm_Click;
            _btnDirect.Click += BtnDirect_Click;
            bottomPanel.Controls.AddRange(new Control[] { _btnStart, _btnConfirm, _btnDirect });

            _directPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(0xFF, 0xF5, 0xE1), Visible = false };

            this.Controls.Add(_rollPanel);
            this.Controls.Add(_directPanel);
            this.Controls.Add(leftPanel);
            this.Controls.Add(rightPanel);
            this.Controls.Add(bottomPanel);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var canteens = await _manageBll.GetAllCanteensAsync();
                var stalls = new List<Stall>();
                foreach (var c in canteens)
                {
                    var s = await _manageBll.GetStallsByCanteenAsync(c.Id);
                    stalls.AddRange(s);
                }
                _allStalls = stalls;

                _candidateList.Items.Clear();
                foreach (var stall in _allStalls)
                    _candidateList.Items.Add($"{stall.CanteenName}-{stall.StallName}", false);

                RefreshGroupCombo();
            }
            catch { }
        }

        private void RefreshGroupCombo()
        {
            var prevSelected = _groupCombo.SelectedItem as SelectionGroup;
            _groupCombo.Items.Clear();
            _groupCombo.Items.Add("(全部)");
            var groups = _selectBll.GetSelectionGroups();
            foreach (var g in groups)
                _groupCombo.Items.Add(g);
            _groupCombo.DisplayMember = "GroupName";
            if (prevSelected != null)
            {
                for (int i = 1; i < _groupCombo.Items.Count; i++)
                {
                    if ((_groupCombo.Items[i] as SelectionGroup)?.Id == prevSelected.Id)
                    { _groupCombo.SelectedIndex = i; return; }
                }
            }
            if (_groupCombo.Items.Count > 0) _groupCombo.SelectedIndex = 0;
        }

        private async void GroupCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_groupCombo.SelectedIndex <= 0) return;
            var group = _groupCombo.SelectedItem as SelectionGroup;
            if (group == null) return;
            try
            {
                var stalls = await _selectBll.GetGroupStallsAsync(group.Id);
                for (int i = 0; i < _candidateList.Items.Count; i++)
                    _candidateList.SetItemChecked(i, false);
                for (int i = 0; i < _allStalls.Count; i++)
                {
                    if (stalls.Any(s => s.Id == _allStalls[i].Id))
                        _candidateList.SetItemChecked(i, true);
                }
            }
            catch { }
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            var checkedStalls = new List<Stall>();
            for (int i = 0; i < _candidateList.Items.Count; i++)
            {
                if (_candidateList.GetItemChecked(i) && i < _allStalls.Count)
                    checkedStalls.Add(_allStalls[i]);
            }
            if (checkedStalls.Count == 0)
            {
                _ = Program.SoundBLL.PlayAsync(SoundType.Error);
                MessageBox.Show("请先添加候选档口");
                return;
            }

            _btnStart.Enabled = false;
            _cts = new CancellationTokenSource();

            try
            {
                var random = new Random();
                int targetIndex = random.Next(checkedStalls.Count);
                var progress = new Progress<RollProgressInfo>(info =>
                {
                    _rollName.Text = info.CurrentName ?? "";
                    try
                    {
                        if (!string.IsNullOrEmpty(info.CurrentPhoto))
                            _rollImage.Image = Helper.ImageHelper.LoadImage(info.CurrentPhoto);
                    }
                    catch { }
                });

                await _selectBll.AnimateRollAsync(checkedStalls, targetIndex, progress, _cts.Token);
                _selectedStall = checkedStalls[targetIndex];
                _btnConfirm.Enabled = true;
                _ = Program.SoundBLL.PlayAsync(SoundType.Interact);
            }
            catch (OperationCanceledException) { }
            catch { }
            finally { _btnStart.Enabled = true; }
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (_selectedStall == null) return;

            using (var dlg = new Form())
            {
                dlg.Text = "确认选餐";
                dlg.Size = new Size(350, 300);
                dlg.StartPosition = FormStartPosition.CenterParent;

                int y = 20;
                dlg.Controls.Add(new Label { Text = "档口: " + _selectedStall.StallName, Location = new Point(20, y), AutoSize = true });
                y += 30;
                var txtPrice = new TextBox { Location = new Point(100, y), Width = 150 };
                dlg.Controls.Add(new Label { Text = "价格:", Location = new Point(20, y + 5), AutoSize = true });
                dlg.Controls.Add(txtPrice);
                y += 35;
                var txtCalorie = new TextBox { Location = new Point(100, y), Width = 150 };
                dlg.Controls.Add(new Label { Text = "热量:", Location = new Point(20, y + 5), AutoSize = true });
                dlg.Controls.Add(txtCalorie);
                y += 35;
                var txtRemark = new TextBox { Location = new Point(100, y), Width = 150 };
                dlg.Controls.Add(new Label { Text = "备注:", Location = new Point(20, y + 5), AutoSize = true });
                dlg.Controls.Add(txtRemark);
                y += 45;

                bool saved = false;
                var btnOk = new Button { Text = "确认", Location = new Point(100, y), Size = new Size(80, 30) };
                var btnCancel = new Button { Text = "取消", Location = new Point(200, y), Size = new Size(80, 30) };
                btnCancel.Click += (s2, e2) => dlg.Close();
                btnOk.Click += async (s2, e2) =>
                {
                    var pv = RegexHelper.ValidatePrice(txtPrice.Text);
                    var cv = RegexHelper.ValidateCalorie(txtCalorie.Text);
                    if (!pv.isValid || !cv.isValid) { MessageBox.Show(pv.isValid ? cv.message : pv.message); return; }
                    try
                    {
                        await _selectBll.ConfirmSelectionAsync(_selectedStall.Id, null, decimal.Parse(txtPrice.Text), decimal.Parse(txtCalorie.Text), txtRemark.Text, new List<int> { 1 });
                        _ = Program.SoundBLL.PlayAsync(SoundType.Success);
                        MessageBox.Show("记录保存成功！");
                        _mainForm.LoadTodayOverview();
                        saved = true;
                        dlg.Close();
                    }
                    catch (Exception ex) { _ = Program.SoundBLL.PlayAsync(SoundType.Error); MessageBox.Show($"保存失败：{ex.Message}"); }
                };

                dlg.Controls.AddRange(new Control[] { btnOk, btnCancel });
                dlg.ShowDialog();

                if (saved)
                {
                    _btnConfirm.Enabled = false;
                    _selectedStall = null;
                }
            }
        }

        private void BtnDirect_Click(object sender, EventArgs e)
        {
            _rollPanel.Visible = false;
            _directPanel.Visible = true;
            BuildDirectPanel();
        }

        private void BuildDirectPanel()
        {
            _directPanel.Controls.Clear();
            int y = 20;
            _directPanel.Controls.Add(new Label { Text = "直接录入就餐记录", Font = new Font("微软雅黑", 12, FontStyle.Bold), Location = new Point(20, y), AutoSize = true });
            y += 40;

            var cmbCanteen = new ComboBox { Location = new Point(80, y), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            _directPanel.Controls.Add(new Label { Text = "食堂:", Location = new Point(20, y + 5), AutoSize = true });
            _directPanel.Controls.Add(cmbCanteen);
            y += 35;
            var cmbStall = new ComboBox { Location = new Point(80, y), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            _directPanel.Controls.Add(new Label { Text = "档口:", Location = new Point(20, y + 5), AutoSize = true });
            _directPanel.Controls.Add(cmbStall);
            y += 35;
            var txtPrice = new TextBox { Location = new Point(80, y), Width = 200 };
            _directPanel.Controls.Add(new Label { Text = "价格:", Location = new Point(20, y + 5), AutoSize = true });
            _directPanel.Controls.Add(txtPrice);
            y += 35;
            var txtCalorie = new TextBox { Location = new Point(80, y), Width = 200 };
            _directPanel.Controls.Add(new Label { Text = "热量:", Location = new Point(20, y + 5), AutoSize = true });
            _directPanel.Controls.Add(txtCalorie);
            y += 35;
            var txtRemark = new TextBox { Location = new Point(80, y), Width = 200 };
            _directPanel.Controls.Add(new Label { Text = "备注:", Location = new Point(20, y + 5), AutoSize = true });
            _directPanel.Controls.Add(txtRemark);
            y += 45;

            var btnSave = new Button { Text = "保存记录", Location = new Point(80, y), Size = new Size(100, 35), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0x1A, 0x6B, 0x3C), ForeColor = Color.FromArgb(0xFF, 0xF5, 0xE1) };
            var btnCancel = new Button { Text = "取消", Location = new Point(200, y), Size = new Size(80, 35), FlatStyle = FlatStyle.Flat };
            btnCancel.Click += (s, e2) => { _directPanel.Visible = false; _rollPanel.Visible = true; };
            _directPanel.Controls.Add(btnSave);
            _directPanel.Controls.Add(btnCancel);

            cmbCanteen.SelectedIndexChanged += async (s, e2) =>
            {
                var canteen = cmbCanteen.SelectedItem as Canteen;
                if (canteen != null)
                {
                    var stalls = await _manageBll.GetStallsByCanteenAsync(canteen.Id);
                    cmbStall.DataSource = stalls;
                    cmbStall.DisplayMember = "StallName";
                }
            };

            btnSave.Click += async (s, e2) =>
            {
                var stall = cmbStall.SelectedItem as Stall;
                if (stall == null) { MessageBox.Show("请选择档口"); return; }
                var priceVal = RegexHelper.ValidatePrice(txtPrice.Text);
                var calorieVal = RegexHelper.ValidateCalorie(txtCalorie.Text);
                if (!priceVal.isValid) { MessageBox.Show(priceVal.message); return; }
                if (!calorieVal.isValid) { MessageBox.Show(calorieVal.message); return; }
                try
                {
                    await _selectBll.ConfirmSelectionAsync(stall.Id, null, decimal.Parse(txtPrice.Text), decimal.Parse(txtCalorie.Text), txtRemark.Text, new List<int> { 1 });
                    _ = Program.SoundBLL.PlayAsync(SoundType.Success);
                    MessageBox.Show("录入成功！");
                    _mainForm.LoadTodayOverview();
                    _directPanel.Visible = false; _rollPanel.Visible = true;
                }
                catch (Exception ex) { _ = Program.SoundBLL.PlayAsync(SoundType.Error); MessageBox.Show($"录入失败：{ex.Message}"); }
            };

            _ = LoadCanteensAsync(cmbCanteen);
        }

        private async Task LoadCanteensAsync(ComboBox cmb)
        {
            try
            {
                var canteens = await _manageBll.GetAllCanteensAsync();
                cmb.DataSource = canteens;
                cmb.DisplayMember = "CanteenName";
            }
            catch { }
        }

        private void BtnAddGroup_Click(object sender, EventArgs e)
        {
            using (var dlg = new Form())
            {
                dlg.Text = "管理选餐分组";
                dlg.Size = new Size(420, 420);
                dlg.StartPosition = FormStartPosition.CenterParent;

                var lblName = new Label { Text = "分组名称:", Location = new Point(20, 20), AutoSize = true };
                var txtName = new TextBox { Location = new Point(100, 17), Width = 180 };
                var btnSave = new Button { Text = "新增", Location = new Point(290, 15), Size = new Size(60, 25), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0x1A, 0x6B, 0x3C), ForeColor = Color.FromArgb(0xFF, 0xF5, 0xE1) };
                var listBox = new ListBox { Location = new Point(20, 50), Size = new Size(360, 260) };

                LoadGroupList(listBox);

                btnSave.Click += async (s2, e2) =>
                {
                    if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("请输入分组名称"); return; }
                    var (valid, msg) = RegexHelper.ValidateGroupName(txtName.Text);
                    if (!valid) { MessageBox.Show(msg); return; }
                    var checkedIds = new List<int>();
                    for (int i = 0; i < _candidateList.Items.Count; i++)
                        if (_candidateList.GetItemChecked(i) && i < _allStalls.Count) checkedIds.Add(_allStalls[i].Id);
                    try
                    {
                        await _selectBll.SaveSelectionGroupAsync(new SelectionGroup { GroupName = txtName.Text, IsSystem = false }, checkedIds);
                        Program.SoundBLL.PlayAsync(SoundType.Success);
                        txtName.Text = "";
                        LoadGroupList(listBox);
                        RefreshGroupCombo();
                    }
                    catch (Exception ex) { Program.SoundBLL.PlayAsync(SoundType.Error); MessageBox.Show($"保存失败：{ex.Message}"); }
                };

                var btnDelete = new Button { Text = "删除选中", Location = new Point(20, 320), Size = new Size(80, 28), FlatStyle = FlatStyle.Flat };
                btnDelete.Click += async (s2, e2) =>
                {
                    var group = listBox.SelectedItem as SelectionGroup;
                    if (group == null) { MessageBox.Show("请先选择一个分组"); return; }
                    if (group.IsSystem) { MessageBox.Show("内置分组不可删除"); return; }
                    if (MessageBox.Show($"确认删除分组\"{group.GroupName}\"？", "删除确认", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        try
                        {
                            await _selectBll.DeleteSelectionGroupAsync(group.Id);
                            Program.SoundBLL.PlayAsync(SoundType.Success);
                            LoadGroupList(listBox);
                            RefreshGroupCombo();
                        }
                        catch (Exception ex) { Program.SoundBLL.PlayAsync(SoundType.Error); MessageBox.Show($"删除失败：{ex.Message}"); }
                    }
                };

                dlg.Controls.AddRange(new Control[] { lblName, txtName, btnSave, listBox, btnDelete });
                dlg.ShowDialog();
            }
        }

        private void LoadGroupList(ListBox listBox)
        {
            listBox.Items.Clear();
            var groups = _selectBll.GetSelectionGroups();
            foreach (var g in groups)
                listBox.Items.Add(g);
            listBox.DisplayMember = "GroupName";
        }
    }
}
