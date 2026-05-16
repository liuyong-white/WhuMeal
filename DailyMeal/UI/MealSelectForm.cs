using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DailyMeal.BLL;
using DailyMeal.DAL;
using DailyMeal.Helper;
using DailyMeal.Model;
using DailyMeal.UI.Theme;

namespace DailyMeal.UI
{
    public partial class MealSelectForm : UserControl
    {
        private MainForm _mainForm;
        private MealSelectBLL _selectBll = new MealSelectBLL();
        private DataManageBLL _manageBll = new DataManageBLL();
        private DinnerBuddyDAL _buddyDal = new DinnerBuddyDAL();
        private CheckedListBox _candidateList;
        private ComboBox _groupCombo;
        private Button _btnStart;
        private Button _btnConfirm;
        private Button _btnDirect;
        private CancellationTokenSource _cts;
        private List<Stall> _allStalls = new List<Stall>();
        private Stall _selectedStall;
        private Panel _directPanel;
        private Panel _slotPanel;
        private Panel _slotViewport;
        private List<Stall> _rollCandidates;
        private bool _isRolling;
        private string _currentDisplayName;
        private float _currentAlpha = 1f;
        private bool _isFinalResult;
        private System.Windows.Forms.Timer _renderTimer;

        public MealSelectForm(MainForm mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();
            _ = LoadDataAsync();
        }

        private void InitializeComponent()
        {
            this.BackColor = AppTheme.Background;

            _slotPanel = new Panel { Dock = DockStyle.Left, Width = 280, BackColor = AppTheme.Surface, Padding = new Padding(10, 10, 10, 10) };
            var lblSlotTitle = new Label { Text = "摇号窗口", Font = AppTheme.SubtitleFont, ForeColor = AppTheme.Primary, Dock = DockStyle.Top, Height = 35, TextAlign = ContentAlignment.MiddleCenter };
            _slotViewport = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            _slotViewport.Paint += SlotViewport_Paint;
            _slotPanel.Controls.Add(_slotViewport);
            _slotPanel.Controls.Add(lblSlotTitle);

            var rightPanel = new Panel { Dock = DockStyle.Right, Width = 200, BackColor = AppTheme.Surface, Padding = new Padding(5) };
            var lblCandidate = new Label { Text = "候选档口", Font = AppTheme.HeadingFont, Dock = DockStyle.Top, Height = 30, TextAlign = ContentAlignment.MiddleCenter };
            _candidateList = new CheckedListBox { Dock = DockStyle.Fill, Font = AppTheme.CaptionFont, CheckOnClick = true };
            rightPanel.Controls.Add(_candidateList);
            rightPanel.Controls.Add(lblCandidate);

            var groupPanel = new Panel { Dock = DockStyle.Right, Width = 160, BackColor = AppTheme.Surface, Padding = new Padding(5) };
            var lblGroup = new Label { Text = "选餐分组", Font = AppTheme.HeadingFont, Dock = DockStyle.Top, Height = 25 };
            _groupCombo = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
            _groupCombo.SelectedIndexChanged += GroupCombo_SelectedIndexChanged;
            var btnAddGroup = new Button { Text = "管理分组", Dock = DockStyle.Top, Height = 30, FlatStyle = FlatStyle.Flat };
            btnAddGroup.Click += BtnAddGroup_Click;
            groupPanel.Controls.Add(btnAddGroup);
            groupPanel.Controls.Add(_groupCombo);
            groupPanel.Controls.Add(lblGroup);

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = AppTheme.Surface };
            _btnStart = new Button { Text = "开始选餐", Size = new Size(100, 35), Location = new Point(50, 8) };
            ButtonStyler.ApplyPrimary(_btnStart);
            _btnConfirm = new Button { Text = "确认选餐", Size = new Size(100, 35), Location = new Point(170, 8), Enabled = false };
            ButtonStyler.ApplyAccent(_btnConfirm);
            _btnDirect = new Button { Text = "直接录入", Size = new Size(100, 35), Location = new Point(290, 8) };
            ButtonStyler.ApplyPrimary(_btnDirect);
            _btnStart.Click += BtnStart_Click;
            _btnConfirm.Click += BtnConfirm_Click;
            _btnDirect.Click += BtnDirect_Click;
            bottomPanel.Controls.AddRange(new Control[] { _btnStart, _btnConfirm, _btnDirect });

            _directPanel = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Surface, Visible = false };

            this.Controls.Add(_directPanel);
            this.Controls.Add(groupPanel);
            this.Controls.Add(rightPanel);
            this.Controls.Add(_slotPanel);
            this.Controls.Add(bottomPanel);

            _renderTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _renderTimer.Tick += (s, e) => { if (_isRolling) _slotViewport.Invalidate(); };
            _renderTimer.Start();
        }

        private void SlotViewport_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            int w = _slotViewport.ClientSize.Width;
            int h = _slotViewport.ClientSize.Height;
            int cx = w / 2;
            int cy = h / 2;

            if (_rollCandidates == null || _rollCandidates.Count == 0)
            {
                using (var font = new Font("微软雅黑", 10, FontStyle.Regular))
                using (var brush = new SolidBrush(AppTheme.TextMuted))
                {
                    var txt = "选择分组后点击开始选餐";
                    var sz = g.MeasureString(txt, font);
                    g.DrawString(txt, font, brush, cx - sz.Width / 2, cy - sz.Height / 2);
                }
                return;
            }

            if (_isFinalResult && !string.IsNullOrEmpty(_currentDisplayName))
            {
                using (var bgBrush = new LinearGradientBrush(new Point(cx - 80, cy - 40), new Point(cx + 80, cy + 40), Color.FromArgb(0x1A, 0x6B, 0x3C), Color.FromArgb(0x2E, 0x8B, 0x57)))
                {
                    g.FillRoundedRectangle(bgBrush, cx - 110, cy - 45, 220, 90, 12);
                }
                using (var borderPen = new Pen(Color.FromArgb(0x10, 0x48, 0x28), 2))
                {
                    g.DrawRoundedRectangle(borderPen, cx - 110, cy - 45, 220, 90, 12);
                }
                using (var font = new Font("微软雅黑", 16, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    var sz = g.MeasureString(_currentDisplayName, font);
                    g.DrawString(_currentDisplayName, font, brush, cx - sz.Width / 2, cy - sz.Height / 2);
                }
                return;
            }

            if (string.IsNullOrEmpty(_currentDisplayName))
            {
                using (var font = new Font("微软雅黑", 10, FontStyle.Regular))
                using (var brush = new SolidBrush(AppTheme.TextMuted))
                {
                    var txt = "点击\"开始选餐\"抽号";
                    var sz = g.MeasureString(txt, font);
                    g.DrawString(txt, font, brush, cx - sz.Width / 2, cy - sz.Height / 2);
                }
                return;
            }

            int alpha = (int)(255 * _currentAlpha);
            alpha = Math.Max(0, Math.Min(255, alpha));

            using (var font = new Font("微软雅黑", 20, FontStyle.Bold))
            {
                var sz = g.MeasureString(_currentDisplayName, font);
                float textX = cx - sz.Width / 2;
                float textY = cy - sz.Height / 2;

                using (var shadowBrush = new SolidBrush(Color.FromArgb(alpha / 4, 0, 0, 0)))
                {
                    g.DrawString(_currentDisplayName, font, shadowBrush, textX + 2, textY + 2);
                }
                using (var textBrush = new SolidBrush(Color.FromArgb(alpha, 0x1A, 0x6B, 0x3C)))
                {
                    g.DrawString(_currentDisplayName, font, textBrush, textX, textY);
                }
            }
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
            if (_groupCombo.SelectedIndex < 0) return;
            try
            {
                List<Stall> stalls;
                if (_groupCombo.SelectedIndex == 0)
                {
                    stalls = _allStalls;
                }
                else
                {
                    var group = _groupCombo.SelectedItem as SelectionGroup;
                    if (group == null) return;
                    stalls = await _selectBll.GetGroupStallsAsync(group.Id);
                }
                for (int i = 0; i < _candidateList.Items.Count; i++)
                    _candidateList.SetItemChecked(i, false);
                for (int i = 0; i < _allStalls.Count; i++)
                {
                    if (stalls.Any(s => s.Id == _allStalls[i].Id))
                        _candidateList.SetItemChecked(i, true);
                }
                _rollCandidates = stalls;
                _currentDisplayName = null;
                _isFinalResult = false;
                _isRolling = false;
                _slotViewport.Invalidate();
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
            if (checkedStalls.Count == 0 && _rollCandidates != null && _rollCandidates.Count > 0)
                checkedStalls = _rollCandidates;
            if (checkedStalls.Count == 0)
            {
                _ = Program.SoundBLL.PlayAsync(SoundType.Error);
                MessageBox.Show("请先添加候选档口");
                return;
            }

            _btnStart.Enabled = false;
            _btnConfirm.Enabled = false;
            _cts = new CancellationTokenSource();
            _rollCandidates = checkedStalls;
            _isRolling = true;
            _isFinalResult = false;

            try
            {
                var random = new Random();
                int targetIndex = random.Next(checkedStalls.Count);

                var progress = new Progress<RollStepInfo>(info =>
                {
                    _currentDisplayName = info.DisplayName;
                    _currentAlpha = info.Alpha;
                    _slotViewport.Invalidate();
                });

                await _selectBll.AnimateFlashRollAsync(checkedStalls, targetIndex, progress, _cts.Token);

                _isRolling = false;
                _isFinalResult = true;
                _currentDisplayName = $"{checkedStalls[targetIndex].CanteenName} - {checkedStalls[targetIndex].StallName}";
                _currentAlpha = 1f;
                _slotViewport.Invalidate();
                _selectedStall = checkedStalls[targetIndex];
                _btnConfirm.Enabled = true;
                _ = Program.SoundBLL.PlayAsync(SoundType.Interact);
            }
            catch (OperationCanceledException) { _isRolling = false; _isFinalResult = false; _currentDisplayName = null; _slotViewport.Invalidate(); }
            catch (Exception ex) { _isRolling = false; _isFinalResult = false; _currentDisplayName = null; _slotViewport.Invalidate(); MessageBox.Show($"选餐异常：{ex.Message}"); }
            finally { _btnStart.Enabled = true; }
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (_selectedStall == null) return;

            using (var dlg = new Form())
            {
                dlg.Text = "确认选餐";
                dlg.Size = new Size(350, 420);
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
                y += 40;

                dlg.Controls.Add(new Label { Text = "饭搭子:", Location = new Point(20, y), AutoSize = true });
                y += 25;
                var chkBuddies = new CheckedListBox { Location = new Point(30, y), Size = new Size(260, 80), CheckOnClick = true };
                var allBuddies = _buddyDal.GetAll();
                foreach (var buddy in allBuddies)
                {
                    int idx = chkBuddies.Items.Add(buddy);
                    if (buddy.Name == "老己") chkBuddies.SetItemChecked(idx, true);
                }
                chkBuddies.DisplayMember = "Name";
                dlg.Controls.Add(chkBuddies);
                y += 90;

                bool saved = false;
                var btnOk = new Button { Text = "确认", Location = new Point(100, y), Size = new Size(80, 30) };
                var btnCancel = new Button { Text = "取消", Location = new Point(200, y), Size = new Size(80, 30) };
                btnCancel.Click += (s2, e2) => dlg.Close();
                btnOk.Click += async (s2, e2) =>
                {
                    var pv = RegexHelper.ValidatePrice(txtPrice.Text);
                    var cv = RegexHelper.ValidateCalorie(txtCalorie.Text);
                    if (!pv.isValid || !cv.isValid) { MessageBox.Show(pv.isValid ? cv.message : pv.message); return; }
                    var buddyIds = new List<int>();
                    for (int i = 0; i < chkBuddies.Items.Count; i++)
                    {
                        if (chkBuddies.GetItemChecked(i) && chkBuddies.Items[i] is DinnerBuddy b)
                            buddyIds.Add(b.Id);
                    }
                    if (buddyIds.Count == 0) { MessageBox.Show("请至少选择一位饭搭子（含老己）"); return; }
                    try
                    {
                        var price = string.IsNullOrWhiteSpace(txtPrice.Text) ? 0m : decimal.Parse(txtPrice.Text);
                        var calorie = string.IsNullOrWhiteSpace(txtCalorie.Text) ? 0m : decimal.Parse(txtCalorie.Text);
                        await _selectBll.ConfirmSelectionAsync(_selectedStall.Id, null, price, calorie, txtRemark.Text, buddyIds);
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
                    _isFinalResult = false;
                    _currentDisplayName = null;
                    _slotViewport.Invalidate();
                }
            }
        }

        private void BtnDirect_Click(object sender, EventArgs e)
        {
            _slotPanel.Visible = false;
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
            y += 40;

            _directPanel.Controls.Add(new Label { Text = "饭搭子:", Location = new Point(20, y), AutoSize = true });
            y += 25;
            var chkBuddies = new CheckedListBox { Location = new Point(30, y), Size = new Size(250, 80), CheckOnClick = true };
            var allBuddies = _buddyDal.GetAll();
            foreach (var buddy in allBuddies)
            {
                int idx = chkBuddies.Items.Add(buddy);
                if (buddy.Name == "自己") chkBuddies.SetItemChecked(idx, true);
            }
            chkBuddies.DisplayMember = "Name";
            _directPanel.Controls.Add(chkBuddies);
            y += 90;

            var btnSave = new Button { Text = "保存记录", Location = new Point(80, y), Size = new Size(100, 35) };
            ButtonStyler.ApplyPrimary(btnSave);
            var btnCancel = new Button { Text = "取消", Location = new Point(200, y), Size = new Size(80, 35), FlatStyle = FlatStyle.Flat };
            btnCancel.Click += (s, e2) => { _directPanel.Visible = false; _slotPanel.Visible = true; };
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
                var buddyIds = new List<int>();
                for (int i = 0; i < chkBuddies.Items.Count; i++)
                {
                    if (chkBuddies.GetItemChecked(i) && chkBuddies.Items[i] is DinnerBuddy b)
                        buddyIds.Add(b.Id);
                }
                if (buddyIds.Count == 0) { MessageBox.Show("请至少选择一位饭搭子（含老己）"); return; }
                try
                {
                    var price = string.IsNullOrWhiteSpace(txtPrice.Text) ? 0m : decimal.Parse(txtPrice.Text);
                    var calorie = string.IsNullOrWhiteSpace(txtCalorie.Text) ? 0m : decimal.Parse(txtCalorie.Text);
                    await _selectBll.ConfirmSelectionAsync(stall.Id, null, price, calorie, txtRemark.Text, buddyIds);
                    _ = Program.SoundBLL.PlayAsync(SoundType.Success);
                    MessageBox.Show("录入成功！");
                    _mainForm.LoadTodayOverview();
                    _directPanel.Visible = false; _slotPanel.Visible = true;
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
                var btnSave = new Button { Text = "新增", Location = new Point(290, 15), Size = new Size(60, 25) };
                ButtonStyler.ApplyPrimary(btnSave);
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
