using System;
using System.Drawing;
using System.Windows.Forms;
using RetailShop.Models;

namespace RetailShop.Forms
{
    public class MainForm : Form
    {
        private Panel pnlSidebar;
        private Panel pnlHeader;
        private Panel pnlContent;
        private Label lblUserInfo;
        private Label lblRole;
        private Button btnActive;

        public MainForm()
        {
            InitializeComponent();
            LoadMenuForRole();
        }

        private void InitializeComponent()
        {
            this.Text = "Розничный магазин";
            this.Size = new Size(1200, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 650);
            this.BackColor = Color.FromArgb(245, 245, 248);

            // Header
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.FromArgb(33, 150, 243)
            };

            var lblAppName = new Label
            {
                Text = "🏪  Розничный магазин",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(16, 14)
            };

            lblUserInfo = new Label
            {
                Text = Session.UserFIO,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(900, 10)
            };

            lblRole = new Label
            {
                Text = GetRoleDisplay(Session.UserRole),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(180, 220, 255),
                AutoSize = true,
                Location = new Point(900, 32)
            };

            var btnLogout = new Button
            {
                Text = "Выйти",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(25, 118, 210),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 32),
                Location = new Point(1090, 12),
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => this.Close();

            pnlHeader.Controls.AddRange(new Control[] { lblAppName, lblUserInfo, lblRole, btnLogout });
            pnlHeader.Resize += (s, e) =>
            {
                lblUserInfo.Location = new Point(pnlHeader.Width - 240, 10);
                lblRole.Location = new Point(pnlHeader.Width - 240, 32);
                btnLogout.Location = new Point(pnlHeader.Width - 100, 12);
            };

            // Sidebar
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 210,
                BackColor = Color.FromArgb(30, 35, 50),
                Padding = new Padding(0, 10, 0, 0)
            };

            // Content
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 245, 248),
                Padding = new Padding(20)
            };

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(pnlHeader);
        }

        private void LoadMenuForRole()
        {
            pnlSidebar.Controls.Clear();
            int y = 10;

            void AddSection(string title)
            {
                var lbl = new Label
                {
                    Text = title.ToUpper(),
                    Font = new Font("Segoe UI", 7, FontStyle.Bold),
                    ForeColor = Color.FromArgb(100, 110, 130),
                    Location = new Point(14, y + 8),
                    AutoSize = true
                };
                pnlSidebar.Controls.Add(lbl);
                y += 28;
            }

            void AddBtn(string text, string icon, EventHandler handler)
            {
                var btn = new Button
                {
                    Text = $"  {icon}  {text}",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.FromArgb(200, 210, 230),
                    BackColor = Color.Transparent,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Size = new Size(210, 40),
                    Location = new Point(0, y),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 60, 80);
                btn.Click += (s, e) =>
                {
                    if (btnActive != null)
                    {
                        btnActive.BackColor = Color.Transparent;
                        btnActive.ForeColor = Color.FromArgb(200, 210, 230);
                    }
                    btn.BackColor = Color.FromArgb(33, 150, 243);
                    btn.ForeColor = Color.White;
                    btnActive = btn;
                    handler(s, e);
                };
                pnlSidebar.Controls.Add(btn);
                y += 40;
            }

            // Common
            AddSection("Главная");
            AddBtn("Главная", "🏠", (s, e) => ShowDashboard());

            // Operator
            if (Session.IsOperator || Session.IsAdmin)
            {
                AddSection("Товары");
                AddBtn("Приём партии", "📦", (s, e) => ShowForm(new PartiaTovara()));
                AddBtn("Товары", "🗃", (s, e) => ShowForm(new TovaryForm()));
                AddBtn("Остатки склада", "🏭", (s, e) => ShowForm(new SkladForm()));
            }

            if (Session.IsAdmin)
            {
                AddSection("Администратор");
                AddBtn("Розничные цены", "💰", (s, e) => ShowForm(new RoznichnyCenyForm()));
                AddBtn("Заявки в зал", "📋", (s, e) => ShowForm(new ZayavkiZalForm()));
            }

            if (Session.IsTovaroved)
            {
                AddSection("Товаровед");
                AddBtn("Приём партии", "📦", (s, e) => ShowForm(new PartiaTovara()));
                AddBtn("Товары", "🗃", (s, e) => ShowForm(new TovaryForm()));
                AddBtn("Остатки склада", "🏭", (s, e) => ShowForm(new SkladForm()));
                AddBtn("Возвраты поставщику", "↩", (s, e) => ShowForm(new VozvratForm()));
            }

            if (Session.IsKassir)
            {
                AddSection("Касса");
                AddBtn("Реализация товаров", "🛒", (s, e) => ShowForm(new RealizaciyaForm()));
                AddBtn("Кассы", "🖥", (s, e) => ShowForm(new KassyForm()));
                AddBtn("Кассовые отчёты", "📊", (s, e) => ShowForm(new KassovyeOtchety()));
                AddSection("Склад");
                AddBtn("Инвентаризация", "📝", (s, e) => ShowForm(new InventarizaciyaForm()));
            }

            ShowDashboard();
        }

        private void ShowForm(Form form)
        {
            pnlContent.Controls.Clear();
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(form);
            form.Show();
        }

        private void ShowDashboard()
        {
            pnlContent.Controls.Clear();
            var dash = new DashboardPanel(Session.UserFIO, Session.UserRole);
            dash.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(dash);
        }

        private string GetRoleDisplay(string role)
        {
            switch (role)
            {
                case "Оператор": return "Оператор";
                case "Администратор": return "Администратор";
                case "Товаровед": return "Товаровед";
                case "СтаршийКассир": return "Старший кассир";
                default: return role;
            }
        }
    }

    // ─── Dashboard Panel ────────────────────────────────────────────────────────
    public class DashboardPanel : Panel
    {
        public DashboardPanel(string fio, string role)
        {
            this.BackColor = Color.FromArgb(245, 245, 248);
            BuildUI(fio, role);
        }

        private void BuildUI(string fio, string role)
        {
            var lbl = new Label
            {
                Text = $"Добро пожаловать, {fio}!",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 35, 50),
                AutoSize = true,
                Location = new Point(0, 20)
            };

            var lbl2 = new Label
            {
                Text = $"Роль: {role}  •  {DateTime.Now:dddd, d MMMM yyyy}",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 58)
            };

            this.Controls.Add(lbl);
            this.Controls.Add(lbl2);

            // Quick stats
            int y = 110;
            var stats = GetStats();
            int x = 0;
            foreach (var stat in stats)
            {
                var card = CreateStatCard(stat.Item1, stat.Item2, stat.Item3);
                card.Location = new Point(x, y);
                this.Controls.Add(card);
                x += 195;
            }
        }

        private Panel CreateStatCard(string title, string value, Color color)
        {
            var card = new Panel
            {
                Size = new Size(180, 100),
                BackColor = Color.White
            };
            card.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 220, 220)), 0, 0, card.Width - 1, card.Height - 1);
                e.Graphics.FillRectangle(new SolidBrush(color), 0, 0, 5, 100);
            };

            var lblVal = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = color,
                AutoSize = true,
                Location = new Point(16, 18)
            };
            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(16, 60)
            };
            card.Controls.Add(lblVal);
            card.Controls.Add(lblTitle);
            return card;
        }

        private (string, string, Color)[] GetStats()
        {
            string товаров = "—", остаток = "—", чеков = "—", сотрудников = "—";
            try
            {
                var r1 = Database.DbHelper.ExecuteScalar("SELECT COUNT(*) FROM Товары");
                if (r1 != null) товаров = r1.ToString();
                var r2 = Database.DbHelper.ExecuteScalar("SELECT ISNULL(SUM(количество),0) FROM Склад");
                if (r2 != null) остаток = r2.ToString();
                var r3 = Database.DbHelper.ExecuteScalar("SELECT COUNT(*) FROM Чеки WHERE CAST(дата AS DATE)=CAST(GETDATE() AS DATE)");
                if (r3 != null) чеков = r3.ToString();
                var r4 = Database.DbHelper.ExecuteScalar("SELECT COUNT(*) FROM Сотрудники");
                if (r4 != null) сотрудников = r4.ToString();
            }
            catch { }
            return new[]
            {
                ("Товаров в базе", товаров, Color.FromArgb(33,150,243)),
                ("Единиц на складе", остаток, Color.FromArgb(76,175,80)),
                ("Чеков сегодня", чеков, Color.FromArgb(255,152,0)),
                ("Сотрудников", сотрудников, Color.FromArgb(156,39,176))
            };
        }
    }
}
