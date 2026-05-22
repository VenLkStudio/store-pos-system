using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RetailShop.Models;

namespace RetailShop
{
    // ─── Rounded button (Win11 style) ─────────────────────────────────────────
    public class RoundButton : Button
    {
        public int   CornerRadius { get; set; } = 6;
        public bool  IsOutline    { get; set; } = false;
        public Color OutlineColor { get; set; } = Clr.Border;
        private bool _hover, _pressed;

        public RoundButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 9f);
            MouseEnter += (s, e) => { _hover = true;   Invalidate(); };
            MouseLeave += (s, e) => { _hover = false; _pressed = false; Invalidate(); };
            MouseDown  += (s, e) => { _pressed = true;  Invalidate(); };
            MouseUp    += (s, e) => { _pressed = false; Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color bg, fg, border;
            if (IsOutline)
            {
                bg     = _pressed ? Color.FromArgb(235, 235, 235)
                       : _hover   ? Color.FromArgb(245, 245, 245)
                                  : Clr.BgWhite;
                fg     = Clr.TextPrimary;
                border = OutlineColor;
            }
            else
            {
                bg     = _pressed ? Clr.AccentPress
                       : _hover   ? Clr.AccentHover
                                  : Clr.BtnPrimary;
                fg     = Color.White;
                border = bg;
            }

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = RoundRect(rect, CornerRadius))
            {
                using (var brush = new SolidBrush(bg))
                    g.FillPath(brush, path);
                using (var pen = new Pen(border))
                    g.DrawPath(pen, path);
            }

            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using (var brush = new SolidBrush(Enabled ? fg : Clr.TextHint))
                g.DrawString(Text, Font, brush, new RectangleF(0, 0, Width, Height), sf);
        }

        private static GraphicsPath RoundRect(Rectangle r, int cr)
        {
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, cr * 2, cr * 2, 180, 90);
            path.AddArc(r.Right - cr * 2, r.Y, cr * 2, cr * 2, 270, 90);
            path.AddArc(r.Right - cr * 2, r.Bottom - cr * 2, cr * 2, cr * 2, 0, 90);
            path.AddArc(r.X, r.Bottom - cr * 2, cr * 2, cr * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override bool ShowFocusCues => false;
    }

    // ─── Rounded TextBox (Win11 style) ────────────────────────────────────────
    public class Win11Field : Panel
    {
        public TextBox Inner { get; }
        private bool _focused;
        public char PasswordChar { get => Inner.PasswordChar; set => Inner.PasswordChar = value; }
        public override string Text { get => Inner.Text; set => Inner.Text = value; }
        public new bool Enabled { get => Inner.Enabled; set { Inner.Enabled = value; Invalidate(); } }
        public new event EventHandler TextChanged { add => Inner.TextChanged += value; remove => Inner.TextChanged -= value; }

        public Win11Field(int w = 200)
        {
            Size = new Size(w, 34);
            BackColor = Clr.BgWhite;
            Padding = new Padding(1);

            Inner = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font        = new Font("Segoe UI", 9.5f),
                BackColor   = Clr.BgWhite,
                ForeColor   = Clr.TextPrimary,
                Dock        = DockStyle.Fill,
            };
            Inner.GotFocus  += (s, e) => { _focused = true;  Invalidate(); };
            Inner.LostFocus += (s, e) => { _focused = false; Invalidate(); };
            Controls.Add(Inner);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var brush = new SolidBrush(Enabled ? Clr.BgWhite : Color.FromArgb(245, 245, 245)))
                g.FillRectangle(brush, r);
            // bottom border accent (Win11 style)
            using (var pen = new Pen(_focused ? Clr.BorderFocus : Clr.Border, _focused ? 2 : 1))
                g.DrawLine(pen, 0, Height - (_focused ? 2 : 1), Width, Height - (_focused ? 2 : 1));
            using (var pen = new Pen(Clr.Border))
            {
                g.DrawLine(pen, 0, 0, Width, 0);
                g.DrawLine(pen, 0, 0, 0, Height - 1);
                g.DrawLine(pen, Width - 1, 0, Width - 1, Height - 1);
            }
        }

        protected override void OnClick(EventArgs e) { Inner.Focus(); }
    }

    // ─── Rounded ComboBox wrapper ─────────────────────────────────────────────
    public class Win11Combo : Panel
    {
        public ComboBox Inner { get; }
        private bool _focused;
        public int    SelectedIndex  { get => Inner.SelectedIndex;  set => Inner.SelectedIndex  = value; }
        public object SelectedItem   { get => Inner.SelectedItem;   set => Inner.SelectedItem   = value; }
        public object SelectedValue  { get => Inner.SelectedValue;  set => Inner.SelectedValue  = value; }
        public object DataSource     { get => Inner.DataSource;     set => Inner.DataSource     = value; }
        public string DisplayMember  { get => Inner.DisplayMember;  set => Inner.DisplayMember  = value; }
        public string ValueMember    { get => Inner.ValueMember;    set => Inner.ValueMember    = value; }
        public ComboBox.ObjectCollection Items => Inner.Items;
        public event EventHandler SelectedIndexChanged { add => Inner.SelectedIndexChanged += value; remove => Inner.SelectedIndexChanged -= value; }

        public Win11Combo(int w = 200)
        {
            Size = new Size(w, 34);
            BackColor = Clr.BgWhite;

            Inner = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle     = FlatStyle.Flat,
                Font          = new Font("Segoe UI", 9.5f),
                BackColor     = Clr.BgWhite,
                ForeColor     = Clr.TextPrimary,
                Left          = 1,
                Top           = 4,
                Width         = w - 2,
            };
            Inner.GotFocus  += (s, e) => { _focused = true;  Invalidate(); };
            Inner.LostFocus += (s, e) => { _focused = false; Invalidate(); };
            Controls.Add(Inner);
            Resize += (s, e) => { Inner.Width = Width - 2; };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var brush = new SolidBrush(Clr.BgWhite)) g.FillRectangle(brush, r);
            using (var pen = new Pen(_focused ? Clr.BorderFocus : Clr.Border, _focused ? 2 : 1))
                g.DrawLine(pen, 0, Height - (_focused ? 2 : 1), Width, Height - (_focused ? 2 : 1));
            using (var pen = new Pen(Clr.Border))
            {
                g.DrawLine(pen, 0, 0, Width, 0);
                g.DrawLine(pen, 0, 0, 0, Height - 1);
                g.DrawLine(pen, Width - 1, 0, Width - 1, Height - 1);
            }
        }
    }

    // ─── Card panel with shadow-like border ───────────────────────────────────
    public class Win11Card : Panel
    {
        public Win11Card(int w, int h)
        {
            Size = new Size(w, h);
            BackColor = Clr.BgWhite;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(Clr.Border))
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }

    // ─── Tag / badge label ────────────────────────────────────────────────────
    public class StatusBadge : Label
    {
        public Color TagFore { get; set; } = Clr.StatusGreen;
        public Color TagBack { get; set; } = Clr.StatusGreenBg;

        public StatusBadge(string text, Color fore, Color back)
        {
            Text      = text;
            TagFore   = fore;
            TagBack   = back;
            Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            AutoSize  = false;
            Size      = new Size(100, 22);
            TextAlign = ContentAlignment.MiddleCenter;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 4))
            {
                using (var b = new SolidBrush(TagBack)) g.FillPath(b, path);
                using (var p = new Pen(TagFore, 1))     g.DrawPath(p, path);
            }
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using (var b = new SolidBrush(TagFore))
                g.DrawString(Text, Font, b, new RectangleF(0, 0, Width, Height), sf);
        }

        private static GraphicsPath RoundRect(Rectangle r, int cr)
        {
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, cr * 2, cr * 2, 180, 90);
            path.AddArc(r.Right - cr * 2, r.Y, cr * 2, cr * 2, 270, 90);
            path.AddArc(r.Right - cr * 2, r.Bottom - cr * 2, cr * 2, cr * 2, 0, 90);
            path.AddArc(r.X, r.Bottom - cr * 2, cr * 2, cr * 2, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // ─── Static factory helpers ───────────────────────────────────────────────
    public static class UI
    {
        // DataGridView ─────────────────────────────────────────────────────────
        public static DataGridView MakeGrid()
        {
            var g = new DataGridView
            {
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                ReadOnly              = true,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible     = false,
                BackgroundColor       = Clr.BgWhite,
                GridColor             = Clr.Border,
                BorderStyle           = BorderStyle.FixedSingle,
                Font                  = new Font("Segoe UI", 9f),
                ColumnHeadersHeight   = 34,
                RowTemplate           = { Height = 30 },
                EnableHeadersVisualStyles = false,
                CellBorderStyle       = DataGridViewCellBorderStyle.SingleHorizontal,
            };
            g.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor  = Clr.TableHeader,
                ForeColor  = Clr.TextPrimary,
                Font       = new Font("Segoe UI", 9f, FontStyle.Bold),
                Padding    = new Padding(8, 0, 0, 0),
                Alignment  = DataGridViewContentAlignment.MiddleLeft,
            };
            g.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Clr.BgWhite,
                ForeColor = Clr.TextPrimary,
                Padding   = new Padding(8, 0, 0, 0),
                SelectionBackColor = Clr.AccentLight,
                SelectionForeColor = Clr.TextPrimary,
            };
            g.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Clr.BgRow,
                ForeColor = Clr.TextPrimary,
                Padding   = new Padding(8, 0, 0, 0),
                SelectionBackColor = Clr.AccentLight,
                SelectionForeColor = Clr.TextPrimary,
            };
            return g;
        }

        // Buttons ──────────────────────────────────────────────────────────────
        public static RoundButton MakeBtn(string text, int w = 120, int h = 32)
        {
            return new RoundButton
            {
                Text       = text,
                Size       = new Size(w, h),
                Font       = new Font("Segoe UI", 9f),
                IsOutline  = false,
                BackColor  = Clr.BtnPrimary,
                ForeColor  = Color.White,
            };
        }

        public static RoundButton MakeBtnOutline(string text, int w = 120, int h = 32)
        {
            return new RoundButton
            {
                Text        = text,
                Size        = new Size(w, h),
                Font        = new Font("Segoe UI", 9f),
                IsOutline   = true,
                BackColor   = Clr.BgWhite,
                ForeColor   = Clr.TextPrimary,
                OutlineColor = Clr.Border,
            };
        }

        public static RoundButton MakeBtnDanger(string text, int w = 120, int h = 32)
        {
            var b = new RoundButton
            {
                Text      = text,
                Size      = new Size(w, h),
                Font      = new Font("Segoe UI", 9f),
                IsOutline = true,
                OutlineColor = Clr.StatusRed,
            };
            b.ForeColor = Clr.StatusRed;
            return b;
        }

        // Fields ───────────────────────────────────────────────────────────────
        public static Win11Field MakeField(int w = 200, string placeholder = "")
        {
            var f = new Win11Field(w);
            if (!string.IsNullOrEmpty(placeholder))
            {
                f.Inner.ForeColor = Clr.TextHint;
                f.Inner.Text      = placeholder;
                f.Inner.GotFocus  += (s, e) =>
                {
                    if (f.Inner.Text == placeholder) { f.Inner.Text = ""; f.Inner.ForeColor = Clr.TextPrimary; }
                };
                f.Inner.LostFocus += (s, e) =>
                {
                    if (string.IsNullOrEmpty(f.Inner.Text)) { f.Inner.Text = placeholder; f.Inner.ForeColor = Clr.TextHint; }
                };
            }
            return f;
        }

        // ComboBox ─────────────────────────────────────────────────────────────
        public static Win11Combo MakeCombo(int w = 200)
        {
            return new Win11Combo(w);
        }

        // Labels ───────────────────────────────────────────────────────────────
        public static Label MakeTitle(string text)
        {
            return new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Clr.TextPrimary,
                AutoSize  = true,
            };
        }

        public static Label MakeLabel(string text, bool bold = false)
        {
            return new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 9f, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = bold ? Clr.TextPrimary : Clr.TextSecond,
                AutoSize  = true,
            };
        }

        // Card ─────────────────────────────────────────────────────────────────
        public static Win11Card MakeCard(int w, int h)
        {
            return new Win11Card(w, h);
        }

        // Separator ────────────────────────────────────────────────────────────
        public static Panel MakeSep(int w)
        {
            return new Panel { Size = new Size(w, 1), BackColor = Clr.Border };
        }

        // Badge ────────────────────────────────────────────────────────────────
        public static StatusBadge MakeBadge(string text, string type = "green")
        {
            Color fore, back;
            switch (type)
            {
                case "red":   fore = Clr.StatusRed;   back = Clr.StatusRedBg;   break;
                case "blue":  fore = Clr.StatusBlue;  back = Clr.StatusBlueBg;  break;
                default:      fore = Clr.StatusGreen; back = Clr.StatusGreenBg; break;
            }
            return new StatusBadge(text, fore, back);
        }

        // Column helpers ───────────────────────────────────────────────────────
        public static void HideCols(DataGridView g, params string[] names)
        {
            foreach (var n in names)
                if (g.Columns.Contains(n))
                    g.Columns[n].Visible = false;
        }

        // Fluent helper ────────────────────────────────────────────────────────
        public static T Also<T>(this T ctrl, Action<T> action) where T : Control { action(ctrl); return ctrl; }
    }
}
