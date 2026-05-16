using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DailyMeal.BLL;
using DailyMeal.Helper;
using DailyMeal.Model;
using DailyMeal.UI.Theme;

namespace DailyMeal.UI
{
    public partial class DataManageForm : UserControl
    {
        private MainForm _mainForm;
        private DataManageBLL _bll = new DataManageBLL();
        private TabControl _tabControl;
        private DataGridView _gvCanteen, _gvStall;
        private ComboBox _cmbStallCanteen;
        private ErrorProvider _errorProvider = new ErrorProvider();

        public DataManageForm(MainForm mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();
            LoadAllData();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(0xFF, 0xF8, 0xE7);
            _tabControl = new TabControl { Dock = DockStyle.Fill };
            var tabCanteen = new TabPage("食堂管理");
            var tabStall = new TabPage("档口管理");

            BuildCanteenTab(tabCanteen);
            BuildStallTab(tabStall);

            _tabControl.TabPages.AddRange(new TabPage[] { tabCanteen, tabStall });
            this.Controls.Add(_tabControl);
        }

        private void BuildCanteenTab(TabPage tab)
        {
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(0xFF, 0xF5, 0xE1) };
            var lblName = new Label { Text = "名称:", Location = new Point(10, 15), AutoSize = true };
            var txtName = new TextBox { Name = "txtCanteenName", Location = new Point(55, 12), Width = 200 };
            var btnSave = new Button { Text = "保存", Location = new Point(270, 10), Size = new Size(70, 28) };
            ButtonStyler.ApplyPrimary(btnSave);
            var btnCancel = new Button { Text = "取消", Location = new Point(350, 10), Size = new Size(70, 28), FlatStyle = FlatStyle.Flat };
            topPanel.Controls.AddRange(new Control[] { lblName, txtName, btnSave, btnCancel });

            _gvCanteen = new DataGridView { Dock = DockStyle.Fill };
            DataGridViewStyler.ApplyStyle(_gvCanteen);

            tab.Controls.Add(_gvCanteen);
            tab.Controls.Add(topPanel);

            int editingId = 0;
            btnSave.Click += async (s, e) =>
            {
                var (valid, msg) = RegexHelper.ValidateEntityName(txtName.Text);
                if (!valid) { _errorProvider.SetError(txtName, msg); Program.SoundBLL.PlayAsync(SoundType.Error); return; }
                _errorProvider.SetError(txtName, "");
                try
                {
                    if (editingId > 0) { var c = await _bll.GetAllCanteensAsync(); var item = c.Find(x => x.Id == editingId); if (item != null) { item.CanteenName = txtName.Text; await _bll.UpdateCanteenAsync(item); } }
                    else { await _bll.AddCanteenAsync(txtName.Text); }
                    Program.SoundBLL.PlayAsync(SoundType.Success);
                    editingId = 0; txtName.Text = "";
                    await RefreshCanteens();
                }
                catch (Exception ex) { Program.SoundBLL.PlayAsync(SoundType.Error); MessageBox.Show($"保存失败：{ex.Message}"); }
            };
            btnCancel.Click += (s, e) => { editingId = 0; txtName.Text = ""; _errorProvider.Clear(); };
            _gvCanteen.CellContentClick += async (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var row = _gvCanteen.Rows[e.RowIndex];
                var id = (int)row.Cells["Id"].Value;
                if (_gvCanteen.Columns[e.ColumnIndex].Name == "Edit")
                {
                    txtName.Text = row.Cells["CanteenName"].Value.ToString();
                    editingId = id;
                }
                else if (_gvCanteen.Columns[e.ColumnIndex].Name == "Delete")
                {
                    var impact = _bll.CalculateCascadeImpact("Canteen", id);
                    var desc = impact.GetDescription();
                    if (MessageBox.Show($"确认删除？{desc}", "删除确认", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        try { await _bll.DeleteCanteenAsync(id); Program.SoundBLL.PlayAsync(SoundType.Success); await RefreshCanteens(); }
                        catch (Exception ex) { Program.SoundBLL.PlayAsync(SoundType.Error); MessageBox.Show($"删除失败：{ex.Message}"); }
                    }
                }
            };
        }

        private void BuildStallTab(TabPage tab)
        {
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(0xFF, 0xF5, 0xE1) };
            var lblName = new Label { Text = "名称:", Location = new Point(10, 15), AutoSize = true };
            var txtName = new TextBox { Location = new Point(55, 12), Width = 120 };
            var lblCanteen = new Label { Text = "食堂:", Location = new Point(190, 15), AutoSize = true };
            var cmbCanteen = new ComboBox { Name = "cmbStallCanteen", Location = new Point(230, 12), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "CanteenName", ValueMember = "Id" };
            _cmbStallCanteen = cmbCanteen;
            var btnSave = new Button { Text = "保存", Location = new Point(370, 10), Size = new Size(70, 28) };
            ButtonStyler.ApplyPrimary(btnSave);
            topPanel.Controls.AddRange(new Control[] { lblName, txtName, lblCanteen, cmbCanteen, btnSave });

            _gvStall = new DataGridView { Dock = DockStyle.Fill };
            DataGridViewStyler.ApplyStyle(_gvStall);

            tab.Controls.Add(_gvStall);
            tab.Controls.Add(topPanel);

            int editingId = 0;
            btnSave.Click += async (s, e) =>
            {
                var (valid, msg) = RegexHelper.ValidateEntityName(txtName.Text);
                if (!valid) { MessageBox.Show(msg); Program.SoundBLL.PlayAsync(SoundType.Error); return; }
                var canteen = cmbCanteen.SelectedItem as Canteen;
                if (canteen == null) { MessageBox.Show("请选择食堂"); return; }
                try
                {
                    if (editingId > 0)
                    {
                        var stalls = await _bll.GetStallsByCanteenAsync(canteen.Id);
                        var item = stalls.Find(x => x.Id == editingId);
                        if (item != null) { item.StallName = txtName.Text; item.CanteenId = canteen.Id; await _bll.UpdateStallAsync(item); }
                    }
                    else { await _bll.AddStallAsync(txtName.Text, canteen.Id); }
                    Program.SoundBLL.PlayAsync(SoundType.Success);
                    editingId = 0; txtName.Text = "";
                    await RefreshStalls();
                }
                catch (Exception ex) { Program.SoundBLL.PlayAsync(SoundType.Error); MessageBox.Show($"保存失败：{ex.Message}"); }
            };

            _gvStall.CellContentClick += async (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var row = _gvStall.Rows[e.RowIndex];
                var id = (int)row.Cells["Id"].Value;
                if (_gvStall.Columns[e.ColumnIndex].Name == "Delete")
                {
                    var impact = _bll.CalculateCascadeImpact("Stall", id);
                    var desc = impact.GetDescription();
                    if (MessageBox.Show($"确认删除？{desc}", "删除确认", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        try { await _bll.DeleteStallAsync(id); Program.SoundBLL.PlayAsync(SoundType.Success); await RefreshStalls(); }
                        catch (Exception ex) { Program.SoundBLL.PlayAsync(SoundType.Error); MessageBox.Show($"删除失败：{ex.Message}"); }
                    }
                }
            };
        }

        private async void LoadAllData() { await RefreshCanteens(); await RefreshStalls(); }

        private async Task RefreshCanteens()
        {
            var data = await _bll.GetAllCanteensAsync();
            _gvCanteen.DataSource = data.Select(c => new { c.Id, c.CanteenName, 来源 = c.IsSystem ? "内置" : "自定义", Edit = "编辑", Delete = "删除" }).ToList();
            var prev = _cmbStallCanteen.SelectedValue;
            _cmbStallCanteen.DataSource = data;
            if (prev != null && data.Any(c => c.Id == (int)prev)) _cmbStallCanteen.SelectedValue = prev;
        }

        private async Task RefreshStalls()
        {
            var canteens = await _bll.GetAllCanteensAsync();
            var stalls = new List<Stall>();
            foreach (var c in canteens) stalls.AddRange(await _bll.GetStallsByCanteenAsync(c.Id));
            _gvStall.DataSource = stalls.Select(s => new { s.Id, s.StallName, 食堂 = s.CanteenName, 来源 = s.IsSystem ? "内置" : "自定义", Delete = "删除" }).ToList();
        }
    }
}
