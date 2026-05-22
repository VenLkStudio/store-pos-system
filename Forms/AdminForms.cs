using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using RetailShop.Database;
using RetailShop.Models;

namespace RetailShop.Forms
{
    // ═══════════════════════════════════════════════════════════════════════
    //  Экран 07 – Расчёт розничных цен
    //  Wireframe: таблица товаров + поле наценки + кнопка Установить цену
    // ═══════════════════════════════════════════════════════════════════════
    public class CenyForm : Form
    {
        private DataGridView _dgv;
        private NumericUpDown _numZakup, _numNazenka;
        private Label _lblResult;

        public CenyForm() { Build(); Load_(); }

        private void Build()
        {
            BackColor = Clr.BgApp;

            var title = UI.MakeTitle("Розничные цены");
            title.Location = new Point(0, 0);

            // ── Left: table ───────────────────────────────────────────────────
            _dgv = UI.MakeGrid();
            _dgv.Location          = new Point(0, 40);
            _dgv.Size              = new Size(600, 520);
            _dgv.Anchor            = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            _dgv.SelectionChanged += (s, e) => OnSelectRow();

            // ── Right: calc panel ─────────────────────────────────────────────
            var card = UI.MakeCard(280, 260);
            card.Location = new Point(620, 40);

            int cy = 16;
            void CRow(string lbl, Control ctrl)
            {
                var l = UI.MakeLabel(lbl, true); l.Location = new Point(12, cy + 2);
                ctrl.Location = new Point(12, cy + 18); ctrl.Width = 256;
                card.Controls.Add(l); card.Controls.Add(ctrl); cy += 54;
            }

            _numZakup   = new NumericUpDown { Minimum = 0, Maximum = 999999, DecimalPlaces = 2, Font = new Font("Segoe UI", 9f) };
            _numNazenka = new NumericUpDown { Minimum = 0, Maximum = 1000,   DecimalPlaces = 2, Font = new Font("Segoe UI", 9f), Value = 30 };

            _numZakup.ValueChanged   += CalcPrice;
            _numNazenka.ValueChanged += CalcPrice;

            CRow("Закупочная цена, ₽:", _numZakup);
            CRow("Наценка, %:", _numNazenka);

            _lblResult = new Label
            {
                Text      = "Розничная цена: 0.00 ₽",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Clr.TextPrimary,
                Location  = new Point(12, cy),
                AutoSize  = true,
            };
            card.Controls.Add(_lblResult);
            cy += 36;

            var sep = UI.MakeSep(256); sep.Location = new Point(12, cy); card.Controls.Add(sep); cy += 12;

            var btnSet = UI.MakeBtn("Установить цену", 256, 34);
            btnSet.Location = new Point(12, cy);
            btnSet.Click   += SetPrice;
            card.Controls.Add(btnSet);

            Controls.AddRange(new Control[] { title, _dgv, card });
        }

        private void Load_()
        {
            var dt = DB.Query(@"
                SELECT т.id, т.название AS [Товар],
                       ISNULL(рц.закупочнаяЦена,0) AS [Закупочная, ₽],
                       ISNULL(рц.наценка,0) AS [Наценка, %],
                       ISNULL(рц.розничнаяЦена,0) AS [Розничная, ₽],
                       FORMAT(рц.дата,'dd.MM.yyyy') AS [Дата]
                FROM Товары т
                LEFT JOIN РозничныеЦены рц ON рц.товарId=т.id
                ORDER BY т.название");
            _dgv.DataSource = dt;
            UI.HideCols(_dgv, "id");
        }

        private void OnSelectRow()
        {
            if (_dgv.SelectedRows.Count == 0) return;
            var r = _dgv.SelectedRows[0];
            _numZakup.Value   = Convert.ToDecimal(r.Cells["Закупочная, ₽"].Value ?? 0);
            _numNazenka.Value = Convert.ToDecimal(r.Cells["Наценка, %"].Value ?? 0);
            CalcPrice(null, null);
        }

        private void CalcPrice(object s, EventArgs e)
        {
            decimal roz = _numZakup.Value * (1 + _numNazenka.Value / 100m);
            _lblResult.Text = $"Розничная цена: {roz:F2} ₽";
        }

        private void SetPrice(object s, EventArgs e)
        {
            if (_dgv.SelectedRows.Count == 0) { MessageBox.Show("Выберите товар"); return; }
            int id  = (int)_dgv.SelectedRows[0].Cells["id"].Value;
            decimal roz = _numZakup.Value * (1 + _numNazenka.Value / 100m);

            var ex = Convert.ToInt32(DB.Scalar("SELECT COUNT(*) FROM РозничныеЦены WHERE товарId=@id", DB.P("@id", id)));
            if (ex > 0)
                DB.Exec("UPDATE РозничныеЦены SET закупочнаяЦена=@z,наценка=@n,розничнаяЦена=@r,дата=GETDATE() WHERE товарId=@id",
                    DB.P("@z", _numZakup.Value), DB.P("@n", _numNazenka.Value), DB.P("@r", roz), DB.P("@id", id));
            else
                DB.Exec("INSERT INTO РозничныеЦены(товарId,закупочнаяЦена,наценка,розничнаяЦена) VALUES(@id,@z,@n,@r)",
                    DB.P("@id", id), DB.P("@z", _numZakup.Value), DB.P("@n", _numNazenka.Value), DB.P("@r", roz));
            Load_();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Экран 08 – Заявки в торговый зал
    //  Wireframe: таблица заявок + кнопки Создать / Выполнить
    // ═══════════════════════════════════════════════════════════════════════
    public class ZayavkiForm : Form
    {
        private DataGridView _dgv;

        public ZayavkiForm() { Build(); Load_(); }

        private void Build()
        {
            BackColor = Clr.BgApp;

            var title = UI.MakeTitle("Заявки в торговый зал");
            title.Location = new Point(0, 0);

            var btnNew = UI.MakeBtn("+ Создать заявку", 140, 30);
            btnNew.Location = new Point(0, 38);
            btnNew.Click   += (s, e) => OpenNew();

            var btnDone = UI.MakeBtnOutline("Выполнена", 110, 30);
            btnDone.Location = new Point(150, 38);
            btnDone.Click   += (s, e) =>
            {
                if (_dgv.SelectedRows.Count == 0) return;
                int id = (int)_dgv.SelectedRows[0].Cells["id"].Value;
                DB.Exec("UPDATE ЗаявкиВЗал SET статус='Выполнена' WHERE id=@id", DB.P("@id", id));
                Load_();
            };

            _dgv = UI.MakeGrid();
            _dgv.Location = new Point(0, 78);
            _dgv.Size     = new Size(920, 520);
            _dgv.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            Controls.AddRange(new Control[] { title, btnNew, btnDone, _dgv });
        }

        private void Load_()
        {
            var dt = DB.Query(@"
                SELECT з.id, т.название AS [Товар], з.количество AS [Кол-во],
                       с.ФИО AS [Администратор],
                       FORMAT(з.дата,'dd.MM.yyyy HH:mm') AS [Дата],
                       з.статус AS [Статус]
                FROM ЗаявкиВЗал з
                JOIN Товары т     ON т.id=з.товарId
                JOIN Сотрудники с ON с.id=з.администраторId
                ORDER BY з.дата DESC");
            _dgv.DataSource = dt;
            UI.HideCols(_dgv, "id");
        }

        private void OpenNew()
        {
            var dlg = new Form
            {
                Text = "Новая заявка в зал", Size = new Size(340, 180),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, BackColor = Clr.BgWhite
            };

            var cmbT = UI.MakeCombo(200); cmbT.Location = new Point(110, 16);
            var dt = DB.Query("SELECT id, название FROM Товары");
            cmbT.DataSource = dt; cmbT.DisplayMember = "название"; cmbT.ValueMember = "id";
            var numK = new NumericUpDown { Location = new Point(110, 56), Width = 100, Minimum = 1, Maximum = 9999, Font = new Font("Segoe UI", 9f) };

            dlg.Controls.Add(UI.MakeLabel("Товар:",     true).Also(l => l.Location = new Point(16, 19)));
            dlg.Controls.Add(UI.MakeLabel("Кол-во:",    true).Also(l => l.Location = new Point(16, 59)));
            dlg.Controls.Add(cmbT); dlg.Controls.Add(numK);

            var btnOk = UI.MakeBtn("Создать", 110, 32); btnOk.Location = new Point(110, 100); btnOk.DialogResult = DialogResult.OK;
            btnOk.Click += (s, e) =>
            {
                if (cmbT.SelectedValue == null) return;
                DB.Exec("INSERT INTO ЗаявкиВЗал(товарId,количество,администраторId) VALUES(@t,@k,@a)",
                    DB.P("@t", cmbT.SelectedValue), DB.P("@k", (int)numK.Value), DB.P("@a", Session.UserId));
                Load_();
            };
            var btnCnl = UI.MakeBtnOutline("Отмена", 90, 32); btnCnl.Location = new Point(228, 100); btnCnl.DialogResult = DialogResult.Cancel;
            dlg.Controls.Add(btnOk); dlg.Controls.Add(btnCnl);
            dlg.ShowDialog(this);
        }
    }
}
