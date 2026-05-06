using System;
using System.Drawing;
using System.Windows.Forms;
using DailyMeal.BLL;
using DailyMeal.DAL;
using DailyMeal.Model;

namespace DailyMeal.UI
{
    public partial class SettingsForm : UserControl
    {
        private MainForm _mainForm;
        private CheckBox _chkSound, _chkInteract;
        private TextBox _txtExportPath;
        private DateTimePicker _dtpSemesterStart, _dtpSemesterEnd;
        private ConfigRepository _configRepo = new ConfigRepository();
        private FileOperateBLL _fileBll = new FileOperateBLL();
        private AppSetting _settings;
        private bool _loading = true;

        public SettingsForm(MainForm mainForm)
        {
            _mainForm = mainForm;
            _settings = _configRepo.LoadSettings();
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(0xFF, 0xF8, 0xE7);
            this.AutoScroll = true;

            int y = 20;
            var x = 20;

            var lblSound = new Label { Text = "音效设置", Font = new Font("微软雅黑", 11, FontStyle.Bold), ForeColor = Color.FromArgb(0x1A, 0x6B, 0x3C), Location = new Point(x, y), AutoSize = true };
            y += 35;
            _chkSound = new CheckBox { Text = "音效总开关", Location = new Point(x + 10, y), AutoSize = true };
            y += 30;
            _chkInteract = new CheckBox { Text = "交互音效开关", Location = new Point(x + 10, y), AutoSize = true };
            y += 40;
            _chkSound.CheckedChanged += SaveSettings;
            _chkInteract.CheckedChanged += SaveSettings;

            var lblPath = new Label { Text = "路径设置", Font = new Font("微软雅黑", 11, FontStyle.Bold), ForeColor = Color.FromArgb(0x1A, 0x6B, 0x3C), Location = new Point(x, y), AutoSize = true };
            y += 35;
            var lblExport = new Label { Text = "默认导出路径:", Location = new Point(x + 10, y + 5), AutoSize = true };
            _txtExportPath = new TextBox { Location = new Point(x + 120, y), Width = 250 };
            var btnBrowse = new Button { Text = "浏览", Location = new Point(x + 380, y), Size = new Size(60, 25), FlatStyle = FlatStyle.Flat };
            btnBrowse.Click += (s, e) =>
            {
                using (var dlg = new FolderBrowserDialog())
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        _txtExportPath.Text = dlg.SelectedPath;
                        SaveSettings(s, e);
                    }
                }
            };
            _txtExportPath.Leave += SaveSettings;
            y += 40;

            var lblSemester = new Label { Text = "学期设置", Font = new Font("微软雅黑", 11, FontStyle.Bold), ForeColor = Color.FromArgb(0x1A, 0x6B, 0x3C), Location = new Point(x, y), AutoSize = true };
            y += 35;
            var lblStart = new Label { Text = "学期开始:", Location = new Point(x + 10, y + 5), AutoSize = true };
            _dtpSemesterStart = new DateTimePicker { Location = new Point(x + 90, y), Width = 150, Format = DateTimePickerFormat.Short };
            y += 30;
            var lblEnd = new Label { Text = "学期结束:", Location = new Point(x + 10, y + 5), AutoSize = true };
            _dtpSemesterEnd = new DateTimePicker { Location = new Point(x + 90, y), Width = 150, Format = DateTimePickerFormat.Short };
            _dtpSemesterStart.ValueChanged += SaveSettings;
            _dtpSemesterEnd.ValueChanged += SaveSettings;
            y += 45;

            var lblData = new Label { Text = "数据维护", Font = new Font("微软雅黑", 11, FontStyle.Bold), ForeColor = Color.FromArgb(0x1A, 0x6B, 0x3C), Location = new Point(x, y), AutoSize = true };
            y += 35;
            var btnBackup = new Button { Text = "数据备份", Location = new Point(x + 10, y), Size = new Size(100, 35), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0x1A, 0x6B, 0x3C), ForeColor = Color.White };
            var btnRestore = new Button { Text = "数据恢复", Location = new Point(x + 130, y), Size = new Size(100, 35), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0xF5, 0xA6, 0x23), ForeColor = Color.White };
            btnBackup.Click += BtnBackup_Click;
            btnRestore.Click += BtnRestore_Click;

            this.Controls.AddRange(new Control[] { lblSound, _chkSound, _chkInteract, lblPath, lblExport, _txtExportPath, btnBrowse, lblSemester, lblStart, _dtpSemesterStart, lblEnd, _dtpSemesterEnd, lblData, btnBackup, btnRestore });
        }

        private void LoadSettings()
        {
            _chkSound.Checked = _settings.SoundEnabled;
            _chkInteract.Checked = _settings.InteractiveSoundEnabled;
            _txtExportPath.Text = _settings.ExportPath;
            if (_settings.SemesterStartDate.HasValue) _dtpSemesterStart.Value = _settings.SemesterStartDate.Value;
            if (_settings.SemesterEndDate.HasValue) _dtpSemesterEnd.Value = _settings.SemesterEndDate.Value;
            _loading = false;
        }

        private void SaveSettings(object sender, EventArgs e)
        {
            if (_loading) return;
            _settings.SoundEnabled = _chkSound.Checked;
            _settings.InteractiveSoundEnabled = _chkInteract.Checked;
            _settings.ExportPath = _txtExportPath.Text;
            _settings.SemesterStartDate = _dtpSemesterStart.Value.Date;
            _settings.SemesterEndDate = _dtpSemesterEnd.Value.Date;
            _configRepo.SaveSettings(_settings);
            Program.SoundBLL.UpdateSettings(_settings.SoundEnabled, _settings.InteractiveSoundEnabled);
        }

        private async void BtnBackup_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "选择备份保存路径";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        await _fileBll.BackupDatabaseAsync(dlg.SelectedPath, new Progress<int>(p => { }));
                        Program.SoundBLL.PlayAsync(SoundType.Success);
                        MessageBox.Show("备份成功！");
                    }
                    catch (Exception ex) { Program.SoundBLL.PlayAsync(SoundType.Error); MessageBox.Show($"备份失败：{ex.Message}"); }
                }
            }
        }

        private async void BtnRestore_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "数据库文件|*.db";
                dlg.Title = "选择备份文件";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if (MessageBox.Show("恢复将覆盖当前所有数据，确认继续？", "恢复确认", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        try
                        {
                            await _fileBll.RestoreDatabaseAsync(dlg.FileName);
                            Program.SoundBLL.PlayAsync(SoundType.Success);
                            MessageBox.Show("恢复成功！部分数据需重启程序后生效。");
                        }
                        catch (Exception ex) { Program.SoundBLL.PlayAsync(SoundType.Error); MessageBox.Show($"恢复失败：{ex.Message}"); }
                    }
                }
            }
        }
    }
}
