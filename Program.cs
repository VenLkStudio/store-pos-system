using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RetailShop.Database;
using RetailShop.Forms;
using RetailShop.Models;

namespace RetailShop
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!DB.TestConnection())
            {
                using (var dlg = new ConnectDialog())
                    if (dlg.ShowDialog() != DialogResult.OK) return;

                if (!DB.TestConnection())
                {
                    MessageBox.Show(
                        "Не удалось подключиться к базе данных.\n" +
                        "Проверьте настройки и убедитесь, что SQL Server запущен.",
                        "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            Application.Run(new LoginForm());
        }
    }

    // ─── Connection settings dialog — Win11 style ─────────────────────────────
    public class ConnectDialog : Form
    {
        public ConnectDialog()
        {
            Text            = "Подключение к базе данных";
            Size            = new Size(460, 340);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            BackColor       = Clr.BgApp;

            var title = UI.MakeTitle("Настройка подключения");
            title.Location = new Point(16, 16);

            var warn = UI.MakeLabel("Не удалось подключиться. Проверьте настройки SQL Server.");
            warn.ForeColor = Clr.StatusRed;
            warn.Location  = new Point(16, 52);

            int y = 84;
            void Row(string lbl, Control ctrl)
            {
                Controls.Add(UI.MakeLabel(lbl, true).Also(l => l.Location = new Point(16, y + 4)));
                ctrl.Location = new Point(150, y);
                if (ctrl is Win11Field wf) { wf.Width = 280; wf.Location = new Point(150, y); }
                else { ctrl.Width = 280; }
                Controls.Add(ctrl);
                y += 42;
            }

            var txtServer = UI.MakeField(280); txtServer.Text = "localhost\\SQLEXPRESS";
            var txtDb     = UI.MakeField(280); txtDb.Text     = "RetailShop";
            var chkWin    = new CheckBox
            {
                Text     = "Windows Authentication",
                Checked  = true,
                Location = new Point(150, y),
                AutoSize = true,
                Font     = new Font("Segoe UI", 9f),
                ForeColor = Clr.TextPrimary,
            };
            y += 34;
            var txtUser = UI.MakeField(280); txtUser.Enabled = false;
            var txtPass = UI.MakeField(280); txtPass.PasswordChar = '●'; txtPass.Enabled = false;

            chkWin.CheckedChanged += (s, e) => {
                txtUser.Enabled = !chkWin.Checked;
                txtPass.Enabled = !chkWin.Checked;
            };

            Row("Сервер:",         txtServer);
            Row("База данных:",    txtDb);
            Controls.Add(chkWin);
            Row("Пользователь:",   txtUser);
            Row("Пароль:",         txtPass);

            void Apply()
            {
                DB.ConnectionString = chkWin.Checked
                    ? $"Server={txtServer.Text};Database={txtDb.Text};Integrated Security=True;"
                    : $"Server={txtServer.Text};Database={txtDb.Text};User Id={txtUser.Text};Password={txtPass.Text};";
            }

            var btnTest   = UI.MakeBtnOutline("Проверить", 110, 32); btnTest.Location = new Point(16, y + 8);
            btnTest.Click += (s, e) =>
            {
                Apply();
                MessageBox.Show(DB.TestConnection() ? "Подключение успешно!" : "Не удалось подключиться.",
                    "Проверка", MessageBoxButtons.OK,
                    DB.TestConnection() ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            };

            var btnOk = UI.MakeBtn("OK", 90, 32); btnOk.Location = new Point(258, y + 8);
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Click       += (s, e) => Apply();

            var btnCancel = UI.MakeBtnOutline("Отмена", 80, 32); btnCancel.Location = new Point(354, y + 8);
            btnCancel.DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] { title, warn, chkWin, btnTest, btnOk, btnCancel });
        }
    }
}
