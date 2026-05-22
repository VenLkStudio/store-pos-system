using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RetailShop.Models;

namespace RetailShop.Forms
{
    // ═══════════════════════════════════════════════════════════════════════
    //  Главная оболочка — Windows 11 style
    //  Белый хедер + белый sidebar с синей активной полосой + контент
    // ═══════════════════════════════════════════════════════════════════════
    public class MainForm : Form
    {
        private Panel   _pnlContent;
        private Panel   _sidebar;
        private Button  _activeBtn;

        public MainForm()
        {
            Text            = "Розничный магазин";
            Size            = new Size(1180, 740);
            MinimumSize     = new Size(1000, 640);
            StartPosition   = FormStartPosition.CenterScreen;
            BackColor       = Clr.BgApp;
            Build();
            ShowHome();
        }

        private void Build()
        {
            // ─── Header ──────────────────────────────────────────────────────
            var header = new HeaderPanel { Dock = DockStyle.Top, Height = 48 };

            var lblName = new Label
            {
                Text      = "Розничный магазин",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Clr.TextPrimary,
                AutoSize  = true,
                Location  = new Point(220, 14),
            };

            var lblUser = new Label
            {
                Text      = Session.FIO,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Clr.TextPrimary,
                AutoSize  = true,
                Location  = new Point(860, 10),
            };
            var lblRole = new Label
            {
                Text      = Session.Role,
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Clr.TextSecond,
                AutoSize  = true,
                Location  = new Point(860, 28),
            };

            var btnExit = new RoundButton
            {
                Text       = "Выйти",
                Size       = new Size(80, 28),
                IsOutline  = true,
                Font       = new Font("Segoe UI", 8.5f),
                Location   = new Point(1070, 10),
                OutlineColor = Clr.Border,
            };
            btnExit.ForeColor = Clr.TextSecond;
            btnExit.Click    += (s, e) => Close();

            header.Controls.AddRange(new Control[] { lblName, lblUser, lblRole, btnExit });
            header.Resize += (s, e) => {
                lblUser.Location = new Point(header.Width - 240, 10);
                lblRole.Location = new Point(header.Width - 240, 28);
                btnExit.Location = new Point(header.Width - 96, 10);
            };

            // ─── Sidebar ─────────────────────────────────────────────────────
            _sidebar = new SidebarPanel
            {
                Dock  = DockStyle.Left,
                Width = 200,
            };

            // ─── Content ─────────────────────────────────────────────────────
            _pnlContent = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Clr.BgApp,
                Padding   = new Padding(24, 20, 24, 20),
            };

            Controls.Add(_pnlContent);
            Controls.Add(_sidebar);
            Controls.Add(header);

            BuildSidebar();
        }

        private void BuildSidebar()
        {
            _sidebar.Controls.Clear();
            int y = 16;

            void Section(string title)
            {
                var lbl = new Label
                {
                    Text      = title.ToUpper(),
                    Font      = new Font("Segoe UI", 7f, FontStyle.Bold),
                    ForeColor = Clr.TextHint,
                    Location  = new Point(16, y + 4),
                    AutoSize  = true,
                };
                _sidebar.Controls.Add(lbl);
                y += 24;
            }

            void Item(string icon, string text, Action onClick)
            {
                var pnl = new SidebarItem
                {
                    Location = new Point(8, y),
                    Size     = new Size(184, 36),
                };

                var lblIcon = new Label
                {
                    Text      = icon,
                    Font      = new Font("Segoe UI", 11f),
                    ForeColor = Clr.TextSecond,
                    AutoSize  = false,
                    Size      = new Size(30, 36),
                    Location  = new Point(8, 0),
                    TextAlign = ContentAlignment.MiddleCenter,
                };
                var lblText = new Label
                {
                    Text      = text,
                    Font      = new Font("Segoe UI", 9.5f),
                    ForeColor = Clr.TextPrimary,
                    AutoSize  = false,
                    Size      = new Size(140, 36),
                    Location  = new Point(38, 0),
                    TextAlign = ContentAlignment.MiddleLeft,
                };

                pnl.Controls.Add(lblIcon);
                pnl.Controls.Add(lblText);

                Action activate = () =>
                {
                    if (_activeBtn != null && _activeBtn.Tag is SidebarItem prev)
                    {
                        prev.IsActive = false;
                        prev.Invalidate();
                        foreach (Control c in prev.Controls)
                        {
                            c.ForeColor = c is Label lbl2 && lbl2.Location.X < 32
                                ? Clr.TextSecond : Clr.TextPrimary;
                            c.Font = new Font("Segoe UI",
                                c.Location.X < 32 ? 11f : 9.5f);
                        }
                    }

                    pnl.IsActive = true;
                    pnl.Invalidate();
                    lblIcon.ForeColor = Clr.Accent;
                    lblText.ForeColor = Clr.Accent;
                    lblText.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

                    _activeBtn = new Button { Tag = pnl };
                    onClick();
                };

                pnl.Click     += (s, e) => activate();
                lblIcon.Click += (s, e) => activate();
                lblText.Click += (s, e) => activate();

                // cursor
                pnl.Cursor = lblIcon.Cursor = lblText.Cursor = Cursors.Hand;

                _sidebar.Controls.Add(pnl);
                y += 38;
            }

            Section("Главная");
            Item("⊞", "Главная",          ShowHome);

            if (Session.IsOperator || Session.IsAdmin || Session.IsTovaroved)
            {
                Section("Склад и товары");
                if (Session.IsOperator || Session.IsAdmin)
                    Item("📦", "Приём партии товара", () => ShowPage(new PartiaTovara()));
                Item("🏷", "Товары",               () => ShowPage(new TovaryForm()));
                Item("📊", "Остатки склада",        () => ShowPage(new SkladForm()));
            }

            if (Session.IsAdmin)
            {
                Section("Администратор");
                Item("💲", "Розничные цены",    () => ShowPage(new CenyForm()));
                Item("📋", "Заявки в зал",      () => ShowPage(new ZayavkiForm()));
            }

            if (Session.IsTovaroved)
            {
                Section("Товаровед");
                Item("📦", "Приём партии товара", () => ShowPage(new PartiaTovara()));
                Item("↩", "Возврат поставщику",  () => ShowPage(new VozvratForm()));
            }

            if (Session.IsKassir)
            {
                Section("Касса");
                Item("🖥", "Кассы (смены)",       () => ShowPage(new KassyForm()));
                Item("🧾", "Реализация товаров",  () => ShowPage(new RealizaciyaForm()));
                Item("📄", "Кассовые отчёты",     () => ShowPage(new OtchyotyForm()));
                Section("Инвентаризация");
                Item("📦", "Инвентаризация",      () => ShowPage(new InventarizaciyaForm()));
            }
        }

        private void ShowPage(Form f)
        {
            _pnlContent.Controls.Clear();
            f.TopLevel          = false;
            f.FormBorderStyle   = FormBorderStyle.None;
            f.Dock              = DockStyle.Fill;
            f.BackColor         = Clr.BgApp;
            _pnlContent.Controls.Add(f);
            f.Show();
        }

        private void ShowHome()
        {
            _pnlContent.Controls.Clear();
            _pnlContent.Controls.Add(new HomePanel());
        }
    }

    // ─── Custom panels ────────────────────────────────────────────────────────
    public class HeaderPanel : Panel
    {
        public HeaderPanel() { BackColor = Clr.Header; }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(Clr.HeaderBorder))
                e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);
        }
    }

    public class SidebarPanel : Panel
    {
        public SidebarPanel() { BackColor = Clr.Sidebar; }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(Clr.SidebarBorder))
                e.Graphics.DrawLine(pen, Width - 1, 0, Width - 1, Height);
        }
    }

    public class SidebarItem : Panel
    {
        public bool IsActive { get; set; } = false;

        public SidebarItem()
        {
            BackColor = Color.Transparent;
            MouseEnter += (s, e) => Invalidate();
            MouseLeave += (s, e) => Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (IsActive)
            {
                using (var brush = new SolidBrush(Clr.AccentLight))
                    g.FillRectangle(brush, 0, 0, Width, Height);
                // left accent bar
                using (var brush = new SolidBrush(Clr.Accent))
                    g.FillRectangle(brush, 0, 4, 3, Height - 8);
            }
            else if (ClientRectangle.Contains(PointToClient(MousePosition)))
            {
                using (var brush = new SolidBrush(Color.FromArgb(245, 245, 245)))
                    g.FillRectangle(brush, 0, 0, Width, Height);
            }

            base.OnPaint(e);
        }
    }

    // ─── Home dashboard ───────────────────────────────────────────────────────
    public class HomePanel : Panel
    {
        public HomePanel()
        {
            Dock      = DockStyle.Fill;
            BackColor = Clr.BgApp;

            var title = UI.MakeTitle("Добро пожаловать, " + Session.FIO + "!");
            title.Location = new Point(0, 0);

            var sub = UI.MakeLabel(Session.Role + "  •  " + DateTime.Now.ToString("dd MMMM yyyy"));
            sub.Location = new Point(0, 36);

            Controls.Add(title);
            Controls.Add(sub);

            // Stat cards
            int x = 0;
            foreach (var (lbl, sql) in new[] {
                ("Товаров в базе",    "SELECT COUNT(*) FROM Товары"),
                ("Единиц на складе", "SELECT ISNULL(SUM(количество),0) FROM Склад"),
                ("Чеков сегодня",    "SELECT COUNT(*) FROM Чеки WHERE CAST(дата AS DATE)=CAST(GETDATE() AS DATE)"),
                ("Сотрудников",      "SELECT COUNT(*) FROM Сотрудники"),
            })
            {
                string val = "—";
                try { var r = Database.DB.Scalar(sql); if (r != null) val = r.ToString(); } catch { }
                Controls.Add(MakeStatCard(lbl, val, x, 78));
                x += 185;
            }
        }

        private Panel MakeStatCard(string title, string value, int x, int y)
        {
            var card = UI.MakeCard(170, 96);
            card.Location = new Point(x, y);
            card.Padding  = new Padding(14);

            var v = new Label
            {
                Text      = value,
                Font      = new Font("Segoe UI", 22f, FontStyle.Bold),
                ForeColor = Clr.Accent,
                AutoSize  = true,
                Location  = new Point(14, 14),
            };
            var t = new Label
            {
                Text      = title,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Clr.TextSecond,
                AutoSize  = true,
                Location  = new Point(14, 56),
            };
            card.Controls.Add(v);
            card.Controls.Add(t);
            return card;
        }
    }
}
