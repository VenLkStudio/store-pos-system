using System;
using System.Drawing;
using System.Windows.Forms;
using RetailShop.Database;
using RetailShop.Models;

namespace RetailShop.Forms
{
    // ═══════════════════════════════════════════════════════════════════════
    //  Экран 09 – Возврат товара поставщику
    //  Wireframe: список возвратов + форма оформления справа
    // ═══════════════════════════════════════════════════════════════════════
    public class VozvratForm : Form
    {
        private DataGridView _dgv;

        public VozvratForm() { Build(); Load_(); }

        private void Build()
        {
            BackColor = Clr.BgApp;

            var title = UI.MakeTitle("Возврат товара поставщику");
            title.Location = new Point(0, 0);

            var btnNew = UI.MakeBtn("+ Оформить возврат", 160, 30);
            btnNew.Location = new Point(0, 38);
            btnNew.Click   += (s, e) => OpenNew();

            _dgv = UI.MakeGrid();
            _dgv.Location = new Point(0, 78);
            _dgv.Size     = new Size(920, 520);
            _dgv.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            Controls.AddRange(new Control[] { title, btnNew, _dgv });
        }

        private void Load_()
        {
            var dt = DB.Query(@"
                SELECT в.id,
                       т.название                                   AS [Товар],
                       в.количество                                  AS [Кол-во],
                       пс.название                                   AS [Поставщик],
                       в.причина                                     AS [Причина],
                       с.ФИО                                         AS [Товаровед],
                       FORMAT(в.дата,'dd.MM.yyyy HH:mm')             AS [Дата]
                FROM ВозвратыПоставщику в
                JOIN Товары т          ON т.id=в.товарId
                JOIN ПартииТовара п    ON п.id=в.партияId
                JOIN Поставщики пс     ON пс.id=п.поставщикId
                JOIN Сотрудники с      ON с.id=в.товаровЕдId
                ORDER BY в.дата DESC");
            _dgv.DataSource = dt;
            UI.HideCols(_dgv, "id");
        }

        private void OpenNew()
        {
            var dlg = new Form
            {
                Text            = "Оформить возврат поставщику",
                Size            = new Size(400, 310),
                StartPosition   = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false,
                BackColor       = Clr.BgWhite,
            };

            int y = 16;
            void Row(string lbl, Control ctrl)
            {
                dlg.Controls.Add(UI.MakeLabel(lbl, true).Also(l => l.Location = new Point(16, y + 2)));
                ctrl.Location = new Point(130, y); ctrl.Width = 240;
                dlg.Controls.Add(ctrl); y += 40;
            }

            var cmbPartia = UI.MakeCombo(240);
            var dtP = DB.Query("SELECT п.id, FORMAT(датаПоступления,'dd.MM.yy')+' — '+пс.название AS [название] FROM ПартииТовара п JOIN Поставщики пс ON пс.id=п.поставщикId ORDER BY датаПоступления DESC");
            cmbPartia.DataSource = dtP; cmbPartia.DisplayMember = "название"; cmbPartia.ValueMember = "id";

            var cmbTovar = UI.MakeCombo(240);
            var dtT = DB.Query("SELECT id, название FROM Товары");
            cmbTovar.DataSource = dtT; cmbTovar.DisplayMember = "название"; cmbTovar.ValueMember = "id";

            var numKol = new NumericUpDown { Minimum = 1, Maximum = 9999, Font = new Font("Segoe UI", 9f) };
            var txtPrichina = new TextBox  { Multiline = true, Height = 56, Font = new Font("Segoe UI", 9f), BorderStyle = BorderStyle.FixedSingle };

            Row("Партия:",    cmbPartia);
            Row("Товар:",     cmbTovar);
            Row("Кол-во:",    numKol);
            dlg.Controls.Add(UI.MakeLabel("Причина:", true).Also(l => l.Location = new Point(16, y + 2)));
            txtPrichina.Location = new Point(130, y); txtPrichina.Width = 240; dlg.Controls.Add(txtPrichina); y += 68;

            var btnOk  = UI.MakeBtn("Оформить", 120, 32); btnOk.Location = new Point(130, y); btnOk.DialogResult = DialogResult.OK;
            btnOk.Click += (s, e) =>
            {
                if (cmbPartia.SelectedValue == null || cmbTovar.SelectedValue == null) return;
                int tid = (int)cmbTovar.SelectedValue, kol = (int)numKol.Value;
                // reduce stock
                DB.Exec("UPDATE Склад SET количество=количество-@k WHERE товарId=@t AND количество>=@k",
                    DB.P("@k", kol), DB.P("@t", tid));
                DB.Exec("INSERT INTO ВозвратыПоставщику(партияId,товарId,товаровЕдId,количество,причина) VALUES(@p,@t,@s,@k,@pr)",
                    DB.P("@p", cmbPartia.SelectedValue), DB.P("@t", tid),
                    DB.P("@s", Session.UserId), DB.P("@k", kol), DB.P("@pr", txtPrichina.Text));
                Load_();
                MessageBox.Show("Возврат оформлен", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            var btnCnl = UI.MakeBtnOutline("Отмена", 100, 32); btnCnl.Location = new Point(258, y); btnCnl.DialogResult = DialogResult.Cancel;
            dlg.Controls.Add(btnOk); dlg.Controls.Add(btnCnl);
            dlg.ShowDialog(this);
        }
    }
}
