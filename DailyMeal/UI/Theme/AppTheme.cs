using System;
using System.Drawing;
using System.Windows.Forms;

namespace DailyMeal.UI.Theme
{
    public static class AppTheme
    {
        public static Color Primary => Color.FromArgb(0x1A, 0x6B, 0x3C);
        public static Color PrimaryLight => Color.FromArgb(0x2E, 0x8B, 0x57);
        public static Color PrimaryDark => Color.FromArgb(0x10, 0x48, 0x28);
        public static Color Accent => Color.FromArgb(0xF5, 0xA6, 0x23);
        public static Color AccentLight => Color.FromArgb(0xF7, 0xB7, 0x3A);
        public static Color Background => Color.FromArgb(0xFF, 0xF8, 0xE7);
        public static Color Surface => Color.FromArgb(0xFF, 0xF5, 0xE1);
        public static Color SurfaceDark => Color.FromArgb(0xF5, 0xED, 0xD8);
        public static Color TextPrimary => Color.FromArgb(0x33, 0x33, 0x33);
        public static Color TextSecondary => Color.FromArgb(0x66, 0x66, 0x66);
        public static Color TextMuted => Color.FromArgb(0x99, 0x99, 0x99);
        public static Color TextOnPrimary => Color.White;
        public static Color TextOnAccent => Color.White;
        public static Color Success => Color.FromArgb(0x28, 0xA7, 0x45);
        public static Color Danger => Color.FromArgb(0xDC, 0x35, 0x45);
        public static Color Warning => Color.FromArgb(0xFF, 0xC1, 0x07);
        public static Color Info => Color.FromArgb(0x17, 0xA2, 0xB8);
        public static Color Border => Color.FromArgb(0xDD, 0xDD, 0xDD);
        public static Color Divider => Color.FromArgb(0xEE, 0xEE, 0xEE);

        public static Font TitleFont => new Font("微软雅黑", 16, FontStyle.Bold);
        public static Font SubtitleFont => new Font("微软雅黑", 12, FontStyle.Bold);
        public static Font HeadingFont => new Font("微软雅黑", 11, FontStyle.Bold);
        public static Font BodyFont => new Font("微软雅黑", 10f);
        public static Font CaptionFont => new Font("微软雅黑", 9f);
        public static Font SmallFont => new Font("微软雅黑", 8f);

        public static Color[] ChartColors => new Color[]
        {
            Primary,
            Accent,
            Color.FromArgb(0x7B, 0xC0, 0x4A),
            Color.FromArgb(0xE7, 0x4C, 0x3C),
            Color.FromArgb(0x9B, 0x59, 0xB6),
            Color.FromArgb(0x1A, 0xBC, 0x9C),
            Color.FromArgb(0x2E, 0x8B, 0x57),
            Color.FromArgb(0xE6, 0x7E, 0x22),
            Color.FromArgb(0x3A, 0x3A, 0x3A),
            Color.FromArgb(0x5B, 0xC0, 0xDE)
        };

        public static ButtonStyle PrimaryButton => new ButtonStyle
        {
            BackColor = Primary,
            ForeColor = TextOnPrimary,
            HoverColor = PrimaryLight,
            PressColor = PrimaryDark
        };

        public static ButtonStyle AccentButton => new ButtonStyle
        {
            BackColor = Accent,
            ForeColor = TextOnAccent,
            HoverColor = AccentLight,
            PressColor = Accent
        };

        public static ButtonStyle DangerButton => new ButtonStyle
        {
            BackColor = Danger,
            ForeColor = Color.White,
            HoverColor = Color.FromArgb(0xC8, 0x23, 0x33),
            PressColor = Color.FromArgb(0xBD, 0x21, 0x30)
        };
    }

    public class ButtonStyle
    {
        public Color BackColor { get; set; }
        public Color ForeColor { get; set; }
        public Color HoverColor { get; set; }
        public Color PressColor { get; set; }
    }

    public static class ButtonStyler
    {
        public static void ApplyPrimary(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = AppTheme.Primary;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = AppTheme.PrimaryLight;
            btn.FlatAppearance.MouseDownBackColor = AppTheme.PrimaryDark;
            btn.Cursor = Cursors.Hand;
        }

        public static void ApplyAccent(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = AppTheme.Accent;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = AppTheme.AccentLight;
            btn.FlatAppearance.MouseDownBackColor = AppTheme.Accent;
            btn.Cursor = Cursors.Hand;
        }

        public static void ApplyDanger(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = AppTheme.Danger;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(0xC8, 0x23, 0x33);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(0xBD, 0x21, 0x30);
            btn.Cursor = Cursors.Hand;
        }
    }
}
