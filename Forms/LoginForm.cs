using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using RetailShop.Database;
using RetailShop.Models;

namespace RetailShop.Forms
{
    // ═══════════════════════════════════════════════════════════════════════
    //  Экран 01 – Авторизация
    //  Wireframe: белый центрированный блок, поля Логин / Пароль, кнопка Войти
    // ═══════════════════════════════════════════════════════════════════════
    public class LoginForm : Form
    {
        private TextBox _txtLogin, _txtPass;
        private Label   _lblErr;

        public LoginForm()
        {
            Text            = "Розничный магазин – Вход";
            Size            = new Size(420, 460);
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
            // ─── Top dark header bar ─────────────────────────────────────────
            var header = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 48,
                BackColor = Clr.Accent,
            };
            var lblApp = new Label
            {
                Text      = "Розничный магазин",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(16, 14),
            };
            header.Controls.Add(lblApp);

            // ─── White card ───────────────────────────────────────────────────
            var card = new Panel
            {
                Size      = new Size(320, 290),
                BackColor = Clr.BgWhite,
                Left      = (420 - 320) / 2,
                Top       = 80,
            };
            card.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(Clr.Border), 0, 0, card.Width - 1, card.Height - 1);

            int y = 24;
            void Row(string label, Control ctrl)
            {
                var lbl = UI.MakeLabel(label, true);
                lbl.Location = new Point(20, y);
                ctrl.Location = new Point(20, y + 20);
                ctrl.Width = 280;
                card.Controls.Add(lbl);
                card.Controls.Add(ctrl);
                y += 72;
            }

            _txtLogin = UI.MakeField(280);
            _txtPass  = UI.MakeField(280);
            _txtPass.PasswordChar = '●';

            Row("Логин",  _txtLogin);
            Row("Пароль", _txtPass);

            _lblErr = new Label
            {
                Text      = "",
                ForeColor = Color.FromArgb(180, 40, 40),
                Font      = new Font("Segoe UI", 8.5f),
                Location  = new Point(20, y),
                Size      = new Size(280, 18),
            };
            card.Controls.Add(_lblErr);

            y += 24;
            var btnLogin = UI.MakeBtn("Войти", 280, 36);
            btnLogin.Location = new Point(20, y);
            btnLogin.Font     = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnLogin.Click   += Login_Click;
            card.Controls.Add(btnLogin);

            // hint
            var hint = new Label
            {
                Text      = "operator / admin / tovaroved / kassir  •  пароль: 1234",
                Font      = new Font("Segoe UI", 7.5f),
                ForeColor = Clr.TextHint,
                AutoSize  = true,
                Location  = new Point((420 - 300) / 2, 388),
            };

            Controls.AddRange(new Control[] { header, card, hint });

            _txtPass.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Login_Click(null, null); };
        }

        private void Login_Click(object sender, EventArgs e)
        {
            string login = _txtLogin.Text.Trim();
            string pass  = _txtPass.Text;
            if (login == "" || pass == "") { _lblErr.Text = "Заполните все поля."; return; }

            var dt = DB.Query(
                "SELECT id,ФИО,роль FROM Сотрудники WHERE логин=@l AND пароль=@p",
                DB.P("@l", login), DB.P("@p", pass));

            if (dt.Rows.Count == 0) { _lblErr.Text = "Неверный логин или пароль."; _txtPass.Clear(); return; }

            var r = dt.Rows[0];
            Session.UserId = (int)r["id"];
            Session.FIO    = r["ФИО"].ToString();
            Session.Role   = r["роль"].ToString();
            Session.Login  = login;

            Hide();
            new MainForm().ShowDialog();
            Session.Clear();
            Show();
            _txtPass.Clear();
            _lblErr.Text = "";
        }
    }
}