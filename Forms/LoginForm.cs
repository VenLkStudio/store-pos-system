using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RetailShop.Database;
using RetailShop.Models;

namespace RetailShop.Forms
{
    // ═══════════════════════════════════════════════════════════════════════
    //  Экран 01 – Авторизация — Windows 11 style
    //  Серый фон, белая карточка, логотип, ComboBox роли, поля, кнопка
    // ═══════════════════════════════════════════════════════════════════════
    public class LoginForm : Form
    {
        private Win11Field  _txtLogin, _txtPass;
        private Win11Combo  _cmbRole;
        private Label       _lblErr;

        public LoginForm()
        {
            Text            = "Вход в систему";
            Size            = new Size(420, 520);
            MinimumSize     = Size;
            MaximumSize     = Size;
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            BackColor       = Clr.BgApp;

            Build();
        }

        private void Build()
        {
            // ─── Logo / App icon area ─────────────────────────────────────────
            var pnlLogo = new Panel
            {
                Size      = new Size(80, 80),
                BackColor = Color.FromArgb(228, 241, 254),
                Location  = new Point((420 - 80) / 2, 32),
            };
            pnlLogo.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundRect(new Rectangle(0, 0, 79, 79), 16))
                {
                    using (var b = new SolidBrush(Color.FromArgb(228, 241, 254))) g.FillPath(b, path);
                }
                // simple shop icon
                using (var b = new SolidBrush(Clr.Accent))
                {
                    g.FillRectangle(b, 16, 32, 48, 30);
                    g.FillPolygon(b, new[] { new Point(10, 34), new Point(40, 16), new Point(70, 34) });
                    using (var bw = new SolidBrush(Color.White))
                    {
                        g.FillRectangle(bw, 28, 42, 10, 20);
                        g.FillRectangle(bw, 42, 42, 10, 14);
                    }
                }
            };

            var lblAppName = new Label
            {
                Text      = "Магазин",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Clr.Accent,
                AutoSize  = true,
                Location  = new Point(0, 120),
                Width     = 420,
                TextAlign = ContentAlignment.TopCenter,
            };

            var lblTitle = new Label
            {
                Text      = "Вход в систему",
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Clr.TextPrimary,
                AutoSize  = true,
                Location  = new Point(0, 144),
                Width     = 420,
                TextAlign = ContentAlignment.TopCenter,
            };

            var lblSub = new Label
            {
                Text      = "Введите учётные данные для работы",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Clr.TextSecond,
                AutoSize  = true,
                Location  = new Point(0, 170),
                Width     = 420,
                TextAlign = ContentAlignment.TopCenter,
            };

            // ─── White card ───────────────────────────────────────────────────
            var card = new Win11Card(320, 244) { Location = new Point((420 - 320) / 2, 200) };

            int y = 18;
            void Field(string label, Control ctrl)
            {
                var lbl = new Label
                {
                    Text      = label,
                    Font      = new Font("Segoe UI", 8.5f),
                    ForeColor = Clr.TextSecond,
                    AutoSize  = true,
                    Location  = new Point(18, y),
                };
                ctrl.Location = new Point(18, y + 18);
                ctrl.Width    = 284;
                if (ctrl is Win11Field wf) wf.Width = 284;
                if (ctrl is Win11Combo wc) wc.Width = 284;
                card.Controls.Add(lbl);
                card.Controls.Add(ctrl);
                y += 64;
            }

            // _cmbRole = UI.MakeCombo(284);
            // _cmbRole.Inner.Items.AddRange(new object[] { "Оператор", "Администратор", "Товаровед", "Старший кассир" });
            // _cmbRole.SelectedIndex = 0;

            _txtLogin = UI.MakeField(284);
            _txtPass  = UI.MakeField(284);
            _txtPass.PasswordChar = '●';

            // Field("Роль",   _cmbRole);
            Field("Логин",  _txtLogin);
            Field("Пароль", _txtPass);

            _lblErr = new Label
            {
                Text      = "",
                ForeColor = Clr.StatusRed,
                Font      = new Font("Segoe UI", 8.5f),
                Location  = new Point(18, y),
                Size      = new Size(284, 18),
            };
            card.Controls.Add(_lblErr);

            // ─── Login button (full width inside card) ────────────────────────
            var btnLogin = UI.MakeBtn("Войти", 284, 38);
            btnLogin.Location = new Point(18, 200);
            btnLogin.Font     = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnLogin.Click   += Login_Click;
            card.Controls.Add(btnLogin);

            // ─── Footer hint ──────────────────────────────────────────────────
            var hint = new Label
            {
                Text      = "Доступ только для сотрудников магазина",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Clr.TextHint,
                AutoSize  = true,
                Location  = new Point((420 - 260) / 2, 460),
            };

            Controls.AddRange(new Control[] { pnlLogo, lblAppName, lblTitle, lblSub, card, hint });

            _txtPass.Inner.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Login_Click(null, null); };
        }

        private void Login_Click(object sender, EventArgs e)
        {
            string login = _txtLogin.Text.Trim();
            string pass  = _txtPass.Text;
            if (login == "" || pass == "") { _lblErr.Text = "Заполните все поля."; return; }

            var dt = DB.Query(
                "SELECT id,ФИО,роль FROM Сотрудники WHERE логин=@l AND пароль=@p",
                DB.P("@l", login), DB.P("@p", pass));

            if (dt.Rows.Count == 0) { _lblErr.Text = "Неверный логин или пароль."; _txtPass.Text = ""; return; }

            var r = dt.Rows[0];
            Session.UserId = (int)r["id"];
            Session.FIO    = r["ФИО"].ToString();
            Session.Role   = r["роль"].ToString();
            Session.Login  = login;

            Hide();
            new MainForm().ShowDialog();
            Session.Clear();
            Show();
            _txtPass.Text = "";
            _lblErr.Text  = "";
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
}
