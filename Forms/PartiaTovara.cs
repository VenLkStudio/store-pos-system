using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using RetailShop.Database;
using RetailShop.Models;

namespace RetailShop.Forms
{
    // ═══════════════════════════════════════════════════════════════════════
    //  Экраны 02-04 – Приём партии товара
    //  Wireframe: список партий | таблица состава | документы | кнопки
    // ═══════════════════════════════════════════════════════════════════════
    public class PartiaTovara : Form
    {
        private DataGridView _dgvPartii, _dgvStroki, _dgvDocs;
        private Label        _lblStatus;
        private int          _selId = -1;

        public PartiaTovara() { Build(); Load_(); }

        private void Build()
        {
            BackColor = Clr.BgApp;

            // ── Заголовок ────────────────────────────────────────────────────
            var title = UI.MakeTitle("Приём партии товара");
            title.Location = new Point(0, 0);

            // ── Toolbar ──────────────────────────────────────────────────────
            var tbPanel = new Panel { Location = new Point(0, 36), Height = 38, BackColor = Clr.BgApp };

            var lblPst  = UI.MakeLabel("Поставщик:");
            lblPst.Location = new Point(0, 9);

            var cmbPst = UI.MakeCombo(200);
            cmbPst.Location = new Point(80, 4);
            LoadCombo(cmbPst, "SELECT id, название FROM Поставщики");

            var btnNew = UI.MakeBtn("+ Новая партия", 130, 30);
            btnNew.Location = new Point(292, 4);
            btnNew.Click   += (s, e) => NewPartia(cmbPst);

            var btnRefresh = UI.MakeBtnOutline("Обновить", 90, 30);
            btnRefresh.Location = new Point(430, 4);
            btnRefresh.Click   += (s, e) => Load_();

            tbPanel.Controls.AddRange(new Control[] { lblPst, cmbPst, btnNew, btnRefresh });

            // ── TabControl ───────────────────────────────────────────────────
            var tabs = new TabControl
            {
                Location = new Point(0, 82),
                Size     = new Size(940, 540),
                Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Font     = new Font("Segoe UI", 9.5f),
            };

            // Tab 1 – Партии
            var tpPartii = new TabPage("Партии товара") { BackColor = Clr.BgApp };
            _dgvPartii = UI.MakeGrid();
            _dgvPartii.Dock               = DockStyle.Fill;
            _dgvPartii.SelectionChanged  += (s, e) => OnSelectPartia();

            var barPartii = BuildBottomBar(tpPartii, new[]
            {
                ("Отправить на склад", (Action)SendToSklad),
                ("Отклонить",          (Action)RejectPartia),
            });

            tpPartii.Controls.Add(_dgvPartii);
            tpPartii.Controls.Add(barPartii);

            // Tab 2 – Состав
            var tpStroki = new TabPage("Состав партии") { BackColor = Clr.BgApp };
            _dgvStroki = UI.MakeGrid();
            _dgvStroki.Dock = DockStyle.Fill;

            var addStrokaRow = BuildAddStrokaRow(tpStroki);
            tpStroki.Controls.Add(_dgvStroki);
            tpStroki.Controls.Add(addStrokaRow);

            // Tab 3 – Документы
            var tpDocs = new TabPage("Документы") { BackColor = Clr.BgApp };
            _dgvDocs = UI.MakeGrid();
            _dgvDocs.Dock = DockStyle.Fill;

            var barDocs = BuildBottomBar(tpDocs, new[]
            {
                ("+ Добавить документ", (Action)AddDoc),
                ("Отметить проверен",   (Action)MarkChecked),
            });

            tpDocs.Controls.Add(_dgvDocs);
            tpDocs.Controls.Add(barDocs);

            tabs.TabPages.AddRange(new[] { tpPartii, tpStroki, tpDocs });

            // Status strip
            _lblStatus = new Label
            {
                Dock      = DockStyle.Bottom,
                Height    = 22,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Clr.TextSecond,
                BackColor = Clr.BgApp,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(4, 0, 0, 0),
            };

            Controls.AddRange(new Control[] { title, tbPanel, tabs, _lblStatus });
        }

        private Panel BuildBottomBar(TabPage tp, (string text, Action act)[] buttons)
        {
            var bar = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 40,
                BackColor = Clr.BgApp,
            };
            int bx = 0;
            foreach (var (t, a) in buttons)
            {
                var b = UI.MakeBtn(t, 160, 30);
                b.Location = new Point(bx, 5);
                var captured = a;
                b.Click += (s, e) => captured();
                bar.Controls.Add(b);
                bx += 170;
            }
            return bar;
        }

        private Panel BuildAddStrokaRow(TabPage tp)
        {
            var bar = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Clr.BgApp };

            var cmbT = UI.MakeCombo(200); cmbT.Location = new Point(0, 7);
            LoadCombo(cmbT, "SELECT id, название FROM Товары");

            var numK = new NumericUpDown { Location = new Point(210, 7), Width = 80, Minimum = 1, Maximum = 9999, Font = new Font("Segoe UI", 9f) };
            var numC = new NumericUpDown { Location = new Point(300, 7), Width = 100, Minimum = 0, Maximum = 999999, DecimalPlaces = 2, Font = new Font("Segoe UI", 9f) };

            var lblK = UI.MakeLabel("Кол-во:"); lblK.Location = new Point(207, -2);
            var lblC = UI.MakeLabel("Цена:");   lblC.Location = new Point(297, -2);

            var btnAdd = UI.MakeBtn("+ Строка", 100, 30);
            btnAdd.Location = new Point(410, 7);
            btnAdd.Click += (s, e) =>
            {
                if (_selId < 0) { Status("Выберите партию на вкладке «Партии»"); return; }
                if (cmbT.SelectedValue == null) { Status("Выберите товар"); return; }
                DB.Exec("INSERT INTO СтрокиПартии(партияId,товарId,количество,цена) VALUES(@p,@t,@k,@c)",
                    DB.P("@p", _selId), DB.P("@t", cmbT.SelectedValue),
                    DB.P("@k", (int)numK.Value), DB.P("@c", numC.Value));
                LoadStroki();
                Status("Строка добавлена");
            };
            bar.Controls.AddRange(new Control[] { UI.MakeLabel("Товар:"), cmbT, lblK, numK, lblC, numC, btnAdd });
            return bar;
        }

        // ── Load ──────────────────────────────────────────────────────────────
        private void Load_()
        {
            var dt = DB.Query(@"
                SELECT п.id, FORMAT(п.датаПоступления,'dd.MM.yyyy HH:mm') AS [Дата],
                       пс.название AS [Поставщик],
                       с.ФИО       AS [Оператор],
                       п.статус    AS [Статус],
                       (SELECT ISNULL(SUM(количество*цена),0)
                        FROM СтрокиПартии WHERE партияId=п.id) AS [Сумма, ₽]
                FROM ПартииТовара п
                JOIN Поставщики пс ON пс.id=п.поставщикId
                JOIN Сотрудники с  ON с.id=п.операторId
                ORDER BY п.датаПоступления DESC");
            _dgvPartii.DataSource = dt;
            UI.HideCols(_dgvPartii, "id");
        }

        private void LoadStroki()
        {
            if (_selId < 0) return;
            var dt = DB.Query(@"
                SELECT т.название AS [Товар], т.единицаИзмерения AS [Ед],
                       сп.количество AS [Кол-во], сп.цена AS [Цена, ₽],
                       сп.количество*сп.цена AS [Сумма, ₽]
                FROM СтрокиПартии сп
                JOIN Товары т ON т.id=сп.товарId
                WHERE сп.партияId=@id", DB.P("@id", _selId));
            _dgvStroki.DataSource = dt;
        }

        private void LoadDocs()
        {
            if (_selId < 0) return;
            var dt = DB.Query(@"
                SELECT тип AS [Тип], номер AS [Номер], дата AS [Дата],
                       сумма AS [Сумма, ₽],
                       CASE WHEN проверен=1 THEN '✓ Проверен' ELSE '— Ожидает' END AS [Статус]
                FROM Документы WHERE партияId=@id",
                DB.P("@id", _selId));
            _dgvDocs.DataSource = dt;
        }

        private void OnSelectPartia()
        {
            if (_dgvPartii.SelectedRows.Count == 0) return;
            _selId = (int)_dgvPartii.SelectedRows[0].Cells["id"].Value;
            LoadStroki();
            LoadDocs();
            Status($"Партия №{_selId} выбрана");
        }

        // ── Actions ───────────────────────────────────────────────────────────
        private void NewPartia(Win11Combo cmbPst)
        {
            if (cmbPst.SelectedValue == null) { Status("Выберите поставщика"); return; }
            DB.Exec("INSERT INTO ПартииТовара(поставщикId,операторId) VALUES(@p,@o)",
                DB.P("@p", cmbPst.SelectedValue), DB.P("@o", Session.UserId));
            Load_();
            Status("Новая партия создана");
        }

        private void SendToSklad()
        {
            if (_selId < 0) { Status("Выберите партию"); return; }

            // Check 3 document types present
            var cnt = Convert.ToInt32(DB.Scalar(
                "SELECT COUNT(DISTINCT тип) FROM Документы WHERE партияId=@id AND проверен=1",
                DB.P("@id", _selId)));

            if (cnt < 3)
            {
                MessageBox.Show(
                    "Необходимо добавить и отметить проверенными все три документа:\n" +
                    "• Накладная\n• СчётФактуры\n• СертификатКачества",
                    "Недостаточно документов", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Add qty to warehouse
            var rows = DB.Query("SELECT товарId, количество FROM СтрокиПартии WHERE партияId=@id", DB.P("@id", _selId));
            foreach (DataRow r in rows.Rows)
            {
                int tid = (int)r["товарId"], qty = (int)r["количество"];
                var ex = Convert.ToInt32(DB.Scalar("SELECT COUNT(*) FROM Склад WHERE товарId=@t", DB.P("@t", tid)));
                if (ex > 0) DB.Exec("UPDATE Склад SET количество=количество+@q WHERE товарId=@t", DB.P("@q", qty), DB.P("@t", tid));
                else        DB.Exec("INSERT INTO Склад(товарId,количество) VALUES(@t,@q)", DB.P("@t", tid), DB.P("@q", qty));
            }

            DB.Exec("UPDATE ПартииТовара SET статус='ОтправленаНаСклад' WHERE id=@id", DB.P("@id", _selId));
            Load_();
            Status("Партия отправлена на склад");
        }

        private void RejectPartia()
        {
            if (_selId < 0) return;
            if (MessageBox.Show("Отклонить партию?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            DB.Exec("UPDATE ПартииТовара SET статус='Отклонена' WHERE id=@id", DB.P("@id", _selId));
            Load_();
            Status("Партия отклонена");
        }

        private void AddDoc()
        {
            if (_selId < 0) { Status("Выберите партию"); return; }
            var dlg = new AddDocDialog(_selId);
            if (dlg.ShowDialog() == DialogResult.OK) LoadDocs();
        }

        private void MarkChecked()
        {
            if (_dgvDocs.SelectedRows.Count == 0 || _selId < 0) return;
            string тип = _dgvDocs.SelectedRows[0].Cells["Тип"].Value?.ToString();
            DB.Exec("UPDATE Документы SET проверен=1 WHERE партияId=@id AND тип=@t",
                DB.P("@id", _selId), DB.P("@t", тип));
            LoadDocs();
        }

        // helpers
        private void LoadCombo(Win11Combo c, string sql)
        {
            var dt = DB.Query(sql);
            c.DataSource    = dt;
            c.DisplayMember = dt.Columns[1].ColumnName;
            c.ValueMember   = dt.Columns[0].ColumnName;
        }

        private void Status(string msg) { _lblStatus.Text = msg; }
    }

    // ─── Диалог добавления документа ─────────────────────────────────────────
    public class AddDocDialog : Form
    {
        private readonly int _partiaId;
        public AddDocDialog(int partiaId)
        {
            _partiaId       = partiaId;
            Text            = "Добавить документ";
            Size            = new Size(360, 260);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            BackColor       = Clr.BgWhite;
            Build();
        }

        private void Build()
        {
            int y = 16;
            void Row(string lbl, Control ctrl)
            {
                Controls.Add(UI.MakeLabel(lbl, true) .Also(l => l.Location = new Point(16, y + 2)));
                ctrl.Location = new Point(130, y); ctrl.Width = 200;
                Controls.Add(ctrl); y += 36;
            }

            var cmbTip = UI.MakeCombo(200);
            cmbTip.Items.AddRange(new[] { "Накладная", "СчётФактуры", "СертификатКачества" });
            cmbTip.SelectedIndex = 0;

            var txtNom = UI.MakeField(200);
            var dtp    = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today, Width = 200, Font = new Font("Segoe UI", 9f) };
            var numSum = new NumericUpDown  { Minimum = 0, Maximum = 9999999, DecimalPlaces = 2, Width = 200, Font = new Font("Segoe UI", 9f) };

            Row("Тип:",    cmbTip);
            Row("Номер:",  txtNom);
            Row("Дата:",   dtp);
            Row("Сумма:",  numSum);

            var btnOk = UI.MakeBtn("Сохранить", 110, 32);
            btnOk.Location     = new Point(130, y);
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Click += (s, e) =>
            {
                DB.Exec("INSERT INTO Документы(партияId,тип,номер,дата,сумма) VALUES(@p,@t,@n,@d,@s)",
                    DB.P("@p", _partiaId), DB.P("@t", cmbTip.SelectedItem),
                    DB.P("@n", txtNom.Text), DB.P("@d", dtp.Value.Date), DB.P("@s", numSum.Value));
            };
            var btnCancel = UI.MakeBtnOutline("Отмена", 90, 32);
            btnCancel.Location     = new Point(248, y);
            btnCancel.DialogResult = DialogResult.Cancel;
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
        }
    }

}
