using System;
using System.Drawing;
using System.Windows.Forms;
using DailyMeal.UI.Theme;

namespace DailyMeal.UI.Theme
{
    public static class DataGridViewStyler
    {
        public static void ApplyStyle(DataGridView grid)
        {
            grid.BackgroundColor = AppTheme.Surface;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.EnableHeadersVisualStyles = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowHeadersVisible = false;
            grid.GridColor = AppTheme.Divider;

            grid.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.Primary;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 10f, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            grid.ColumnHeadersHeight = 36;

            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = AppTheme.TextPrimary;
            grid.DefaultCellStyle.Font = AppTheme.BodyFont;
            grid.DefaultCellStyle.SelectionBackColor = AppTheme.PrimaryLight;
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.DefaultCellStyle.Padding = new Padding(5);
            grid.RowTemplate.Height = 32;

            grid.AlternatingRowsDefaultCellStyle.BackColor = AppTheme.Surface;

            grid.DataError += (s, e) => e.ThrowException = false;
        }
    }
}