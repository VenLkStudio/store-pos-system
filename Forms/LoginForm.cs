using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using RetailShop.Database;
using RetailShop.Models;

namespace RetailShop.Forms
{
    public class LoginForm : Form
    {
        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Panel pnlForm;
        private Label lblLogin;
        private TextBox txtLogin;
        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblStatus;
        private PictureBox picIcon;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Розничный магазин — Вход в систему";
            this.Size = new Size(420, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 245, 245);

            // Header
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 140,
                BackColor = Color.FromArgb(33, 150, 243)
            };

            lblTitle = new Label
            {
                Text = "🏪 Розничный магазин",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 30),
                Width = 420
            };

            lblSubtitle = new Label
            {
                Text = "Система управления",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(200, 230, 255),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 85),
                Width = 420
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);

            // Form panel
            pnlForm = new Panel
            {
                Location = new Point(40, 160),
                Size = new Size(340, 280),
                BackColor = Color.White
            };
            pnlForm.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 220, 220)), 0, 0, pnlForm.Width - 1, pnlForm.Height - 1);
            };

            lblLogin = new Label
            {
                Text = "Логин",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location = new Point(20, 20),
                AutoSize = true
            };

            txtLogin = new TextBox
            {
                Location = new Point(20, 42),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblPassword = new Label
            {
                Text = "Пароль",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location = new Point(20, 90),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new Point(20, 112),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 11),
                PasswordChar = '●',
                BorderStyle = BorderStyle.FixedSingle
            };

            btnLogin = new Button
            {
                Text = "ВОЙТИ",
                Location = new Point(20, 170),
                Size = new Size(300, 42),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            lblStatus = new Label
            {
                Text = "",
                Location = new Point(20, 225),
                Size = new Size(300, 40),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(244, 67, 54),
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlForm.Controls.AddRange(new Control[] { lblLogin, txtLogin, lblPassword, txtPassword, btnLogin, lblStatus });

            // Hint label
            var lblHint = new Label
            {
                Text = "Тестовые аккаунты: operator/1234, admin/1234\ntovaroved/1234, kassir/1234",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Location = new Point(40, 455),
                AutoSize = true
            };

            this.Controls.AddRange(new Control[] { pnlHeader, pnlForm, lblHint });

            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnLogin_Click(null, null); };
            txtLogin.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) txtPassword.Focus(); };
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                lblStatus.Text = "Введите логин и пароль";
                return;
            }

            var sql = "SELECT id, ФИО, роль FROM Сотрудники WHERE логин=@l AND пароль=@p";
            var dt = DbHelper.ExecuteQuery(sql, new[]
            {
                new SqlParameter("@l", login),
                new SqlParameter("@p", password)
            });

            if (dt.Rows.Count == 0)
            {
                lblStatus.Text = "Неверный логин или пароль";
                txtPassword.Clear();
                return;
            }

            var row = dt.Rows[0];
            Session.UserId = (int)row["id"];
            Session.UserFIO = row["ФИО"].ToString();
            Session.UserRole = row["роль"].ToString();
            Session.UserName = login;

            this.Hide();
            new MainForm().ShowDialog();
            this.Show();
            Session.Clear();
            txtPassword.Clear();
            lblStatus.Text = "";
        }
    }
}
