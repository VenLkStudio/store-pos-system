using System;
using System.Drawing;
using System.Windows.Forms;
using RetailShop.Models;

namespace RetailShop.Forms
{
    // ═══════════════════════════════════════════════════════════════════════
    //  Главная оболочка
    //  Wireframe: тёмный хедер + левый sidebar + контентная область
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
            var header = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Clr.Accent };

            var lblName = new Label
            {
                Text      = "Розничный магазин",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(16, 14),
            };

            var lblUser = new Label
            {
                Text      = Session.FIO,
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(200, 200, 200),
                AutoSize  = true,
                Location  = new Point(860, 10),
            };
            var lblRole = new Label
            {
                Text      = Session.Role,
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(150, 150, 150),
                AutoSize  = true,
                Location  = new Point(860, 28),
            };
            var btnExit = UI.MakeBtnOutline("Выйти", 80, 28);
            btnExit.ForeColor = Color.White;
            btnExit.BackColor = Color.Transparent;
            btnExit.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            btnExit.Location  = new Point(1070, 10);
            btnExit.Click    += (s, e) => Close();

            header.Controls.AddRange(new Control[] { lblName, lblUser, lblRole, btnExit });
            header.Resize += (s, e) => {
                lblUser.Location = new Point(header.Width - 240, 10);
                lblRole.Location = new Point(header.Width - 240, 28);
                btnExit.Location = new Point(header.Width - 96, 10);
            };

            // ─── Sidebar ─────────────────────────────────────────────────────
            _sidebar = new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 200,
                BackColor = Clr.Sidebar,
            };

            // ─── Content ─────────────────────────────────────────────────────
            _pnlContent = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Clr.BgApp,
                Padding   = new Padding(20, 16, 20, 16),
            };

            Controls.Add(_pnlContent);
            Controls.Add(_sidebar);
            Controls.Add(header);

            BuildSidebar();
        }

        private void BuildSidebar()
        {
            _sidebar.Controls.Clear();
            int y = 12;

            void Section(string title)
            {
                var lbl = new Label
                {
                    Text      = title.ToUpper(),
                    Font      = new Font("Segoe UI", 7f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(90, 90, 90),
                    Location  = new Point(12, y + 6),
                    AutoSize  = true,
                };
                _sidebar.Controls.Add(lbl);
                y += 26;
            }

            void Item(string text, Action onClick)
            {
                var btn = new Button
                {
                    Text      = "  " + text,
                    Location  = new Point(0, y),
                    Size      = new Size(200, 36),
                    Font      = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.FromArgb(200, 200, 200),
                    BackColor = Color.Transparent,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor    = Cursors.Hand,
                };
                btn.FlatAppearance.BorderSize           = 0;
                btn.FlatAppearance.MouseOverBackColor   = Color.FromArgb(50, 50, 50);
                btn.FlatAppearance.MouseDownBackColor   = Color.FromArgb(60, 60, 60);
                btn.Click += (s, e) =>
                {
                    if (_activeBtn != null)
                    { _activeBtn.BackColor = Color.Transparent; _activeBtn.ForeColor = Color.FromArgb(200, 200, 200); }
                    btn.BackColor = Color.FromArgb(55, 55, 55);
                    btn.ForeColor = Color.White;
                    _activeBtn = btn;
                    onClick();
                };
                _sidebar.Controls.Add(btn);
                y += 36;
            }

            Section("Главная");
            Item("Главная",          ShowHome);

            if (Session.IsOperator || Session.IsAdmin || Session.IsTovaroved)
            {
                Section("Склад и товары");
                if (Session.IsOperator || Session.IsAdmin)
                    Item("Приём партии товара", () => ShowPage(new PartiaTovara()));
                Item("Товары",               () => ShowPage(new TovaryForm()));
                Item("Остатки склада",        () => ShowPage(new SkladForm()));
            }

            if (Session.IsAdmin)
            {
                Section("Администратор");
                Item("Розничные цены",    () => ShowPage(new CenyForm()));
                Item("Заявки в зал",      () => ShowPage(new ZayavkiForm()));
            }

            if (Session.IsTovaroved)
            {
                Section("Товаровед");
                Item("Приём партии товара", () => ShowPage(new PartiaTovara()));
                Item("Возврат поставщику",  () => ShowPage(new VozvratForm()));
            }

            if (Session.IsKassir)
            {
                Section("Касса");
                Item("Кассы (смены)",       () => ShowPage(new KassyForm()));
                Item("Реализация товаров",  () => ShowPage(new RealizaciyaForm()));
                Item("Кассовые отчёты",     () => ShowPage(new OtchyotyForm()));
                Section("Инвентаризация");
                Item("Инвентаризация",      () => ShowPage(new InventarizaciyaForm()));
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
            sub.Location = new Point(0, 32);

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
                Controls.Add(MakeCard(lbl, val, x, 72));
                x += 185;
            }
        }

        private Panel MakeCard(string title, string value, int x, int y)
        {
            var card = UI.MakeCard(170, 90);
            card.Location = new Point(x, y);

            var v = new Label { Text = value, Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                                ForeColor = Clr.TextPrimary, AutoSize = true, Location = new Point(12, 12) };
            var t = new Label { Text = title, Font = new Font("Segoe UI", 8.5f),
                                ForeColor = Clr.TextSecond, AutoSize = true, Location = new Point(12, 54) };
            card.Controls.Add(v);
            card.Controls.Add(t);
            return card;
        }
    }
}