using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using RetailShop.Database;
using RetailShop.Forms;

namespace RetailShop
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Show connection settings dialog if needed
            if (!DbHelper.TestConnection())
            {
                using (var connDlg = new ConnectionDialog())
                {
                    if (connDlg.ShowDialog() != DialogResult.OK)
                        return;
                }

                if (!DbHelper.TestConnection())
                {
                    MessageBox.Show("Не удалось подключиться к базе данных.\nПроверьте настройки подключения и убедитесь, что SQL Server запущен.",
                        "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            Application.Run(new LoginForm());
        }
    }

    // ─── Connection Settings Dialog ──────────────────────────────────────────
    public class ConnectionDialog : Form
    {
        public ConnectionDialog()
        {
            this.Text = "Настройка подключения к БД";
            this.Size = new Size(460, 320);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var lblTitle = new Label
            {
                Text = "Настройки подключения SQL Server",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            var lblInfo = new Label
            {
                Text = "Не удалось подключиться к базе данных.\nНастройте параметры подключения:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(200, 50, 50),
                Location = new Point(20, 50),
                AutoSize = true
            };

            int y = 95;
            void AddRow(string label, Control ctrl)
            {
                this.Controls.Add(new Label { Text = label, Location = new Point(20, y + 2), AutoSize = true });
                ctrl.Location = new Point(130, y);
                ctrl.Width = 290;
                this.Controls.Add(ctrl);
                y += 38;
            }

            var txtServer = new TextBox { Font = new Font("Segoe UI", 10), Text = "localhost\\SQLEXPRESS" };
            var txtDatabase = new TextBox { Font = new Font("Segoe UI", 10), Text = "RetailShop" };
            var chkWindowsAuth = new CheckBox { Text = "Windows Authentication", Checked = true, Width = 290 };
            var txtUser = new TextBox { Font = new Font("Segoe UI", 10), Enabled = false };
            var txtPass = new TextBox { Font = new Font("Segoe UI", 10), PasswordChar = '●', Enabled = false };

            chkWindowsAuth.CheckedChanged += (s, e) =>
            {
                txtUser.Enabled = !chkWindowsAuth.Checked;
                txtPass.Enabled = !chkWindowsAuth.Checked;
            };

            AddRow("Сервер:", txtServer);
            AddRow("База данных:", txtDatabase);
            this.Controls.Add(new Label { Text = "Аутентификация:", Location = new Point(20, y + 2), AutoSize = true });
            chkWindowsAuth.Location = new Point(130, y);
            this.Controls.Add(chkWindowsAuth);
            y += 38;
            AddRow("Логин:", txtUser);
            AddRow("Пароль:", txtPass);

            var btnTest = new Button
            {
                Text = "Проверить подключение",
                Location = new Point(20, y + 10), Size = new Size(190, 34),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(240, 240, 240)
            };
            btnTest.FlatAppearance.BorderSize = 0;
            btnTest.Click += (s, e) =>
            {
                UpdateConnectionString(txtServer.Text, txtDatabase.Text, chkWindowsAuth.Checked, txtUser.Text, txtPass.Text);
                if (DbHelper.TestConnection())
                    MessageBox.Show("✅ Подключение успешно!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("❌ Не удалось подключиться!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            var btnOk = new Button
            {
                Text = "OK", DialogResult = DialogResult.OK,
                Location = new Point(220, y + 10), Size = new Size(110, 34),
                BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) =>
                UpdateConnectionString(txtServer.Text, txtDatabase.Text, chkWindowsAuth.Checked, txtUser.Text, txtPass.Text);

            var btnCancel = new Button
            {
                Text = "Отмена", DialogResult = DialogResult.Cancel,
                Location = new Point(340, y + 10), Size = new Size(90, 34),
                FlatStyle = FlatStyle.Flat
            };

            this.Controls.AddRange(new Control[] { lblTitle, lblInfo, btnTest, btnOk, btnCancel });
        }

        private void UpdateConnectionString(string server, string db, bool winAuth, string user, string pass)
        {
            string cs;
            if (winAuth)
                cs = $"Server={server};Database={db};Integrated Security=True;";
            else
                cs = $"Server={server};Database={db};User Id={user};Password={pass};";
            DbHelper.ConnectionString = cs;
        }
    }
}
