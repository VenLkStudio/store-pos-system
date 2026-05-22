using System;
using System.Drawing;
using System.Windows.Forms;
using RetailShop.Database;
using RetailShop.Models;

namespace RetailShop.Forms
{
    // ═══════════════════════════════════════════════════════════════════════
    //  Экраны 12-13 – Кассовые отчёты
    //  Wireframe: верхняя панель со сводкой за день +
    //             таблица отчётов + кнопка «Сформировать отчёт»
    // ═══════════════════════════════════════════════════════════════════════
    public class OtchyotyForm : Form
    {
        private DataGridView _dgvOtchyoty, _dgvCheki;
        private Label        _lblSummary;
        private int          _selKassaId = -1;

        public OtchyotyForm() { Build(); LoadOtchyoty(); LoadSummary(); }

        private void Build()
        {
            BackColor = Clr.BgApp;

            var title = UI.MakeTitle("Кассовые отчёты");
            title.Location = new Point(0, 0);

            // ── Summary card ─────────────────────────────────────────────────
            var cardSummary = UI.MakeCard(920, 70);
            cardSummary.Location = new Point(0, 36);

            _lblSummary = new Label
            {
                Text     = "Загрузка...",
                Font     = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor= Clr.TextPrimary,
                Location = new Point(16, 12),
                AutoSize = true,
            };
            var lblDate = new Label
            {
                Text      = "За сегодня  •  " + DateTime.Now.ToString("dd MMMM yyyy"),
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Clr.TextSecond,
                Location  = new Point(16, 38),
                AutoSize  = true,
            };
            cardSummary.Controls.Add(_lblSummary);
            cardSummary.Controls.Add(lblDate);

            // ── Toolbar ───────────────────────────────────────────────────────
            var btnForm = UI.MakeBtn("+ Сформировать отчёт", 180, 30);
            btnForm.Location = new Point(0, 120);
            btnForm.Click   += (s, e) => FormOtchet();

            var btnRefresh = UI.MakeBtnOutline("Обновить", 90, 30);
            btnRefresh.Location = new Point(190, 120);
            btnRefresh.Click   += (s, e) => { LoadOtchyoty(); LoadSummary(); };

            // ── Tabs ──────────────────────────────────────────────────────────
            var tabs = new TabControl
            {
                Location = new Point(0, 162),
                Size     = new Size(940, 440),
                Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Font     = new Font("Segoe UI", 9.5f),
            };

            // Tab 1 – отчёты
            var tpOtch = new TabPage("Отчёты по сменам") { BackColor = Clr.BgApp };
            _dgvOtchyoty = UI.MakeGrid();
            _dgvOtchyoty.Dock = DockStyle.Fill;
            tpOtch.Controls.Add(_dgvOtchyoty);

            // Tab 2 – чеки
            var tpCheki = new TabPage("Чеки") { BackColor = Clr.BgApp };
            _dgvCheki = UI.MakeGrid();
            _dgvCheki.Dock = DockStyle.Fill;

            // date filter for checks
            var barCheki = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Clr.BgApp };
            var dtpFrom = new DateTimePicker { Location = new Point(0, 7), Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Font = new Font("Segoe UI", 9f) };
            var dtpTo   = new DateTimePicker { Location = new Point(150, 7), Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Font = new Font("Segoe UI", 9f) };
            var btnFind = UI.MakeBtn("Показать", 100, 30); btnFind.Location = new Point(300, 5);
            btnFind.Click += (s, e) => LoadCheki(dtpFrom.Value.Date, dtpTo.Value.Date);
            barCheki.Controls.Add(UI.MakeLabel("с:").Also(l => l.Location = new Point(0, 12)));
            barCheki.Controls.Add(dtpFrom);
            barCheki.Controls.Add(UI.MakeLabel("по:").Also(l => l.Location = new Point(135, 12)));
            barCheki.Controls.Add(dtpTo);
            barCheki.Controls.Add(btnFind);

            tpCheki.Controls.Add(_dgvCheki);
            tpCheki.Controls.Add(barCheki);

            tabs.TabPages.AddRange(new[] { tpOtch, tpCheki });

            Controls.AddRange(new Control[] { title, cardSummary, btnForm, btnRefresh, tabs });
        }

        private void LoadSummary()
        {
            var r = DB.Query(@"
                SELECT COUNT(*)           AS чеков,
                       ISNULL(SUM(итого),0) AS сумма
                FROM Чеки
                WHERE CAST(дата AS DATE) = CAST(GETDATE() AS DATE)
                  AND аннулирован = 0");
            if (r.Rows.Count > 0)
                _lblSummary.Text =
                    $"Чеков сегодня: {r.Rows[0]["чеков"]}   •   " +
                    $"Выручка: {Convert.ToDecimal(r.Rows[0]["сумма"]):N2} ₽";
        }

        private void LoadOtchyoty()
        {
            var dt = DB.Query(@"
                SELECT о.id,
                       к.номер                                       AS [Касса],
                       FORMAT(о.дата,'dd.MM.yyyy')                   AS [Дата],
                       о.количествоЧеков                             AS [Кол-во чеков],
                       о.суммаЗаСмену                                AS [Сумма за смену, ₽],
                       с.ФИО                                         AS [Кассир]
                FROM КассовыеОтчёты о
                JOIN Кассы к      ON к.id = о.кассаId
                JOIN Сотрудники с ON с.id = о.кассирId
                ORDER BY о.дата DESC, к.номер");
            _dgvOtchyoty.DataSource = dt;
            UI.HideCols(_dgvOtchyoty, "id");
        }

        private void LoadCheki(DateTime from, DateTime to)
        {
            var dt = DB.Query(@"
                SELECT ч.id                                         AS [№ чека],
                       к.номер                                      AS [Касса],
                       FORMAT(ч.дата,'dd.MM.yyyy HH:mm')            AS [Дата/время],
                       ч.итого                                      AS [Итого, ₽],
                       CASE WHEN ч.аннулирован=1 THEN 'Аннулирован' ELSE 'Проведён' END AS [Статус]
                FROM Чеки ч
                JOIN Кассы к ON к.id=ч.кассаId
                WHERE CAST(ч.дата AS DATE) BETWEEN @f AND @t
                ORDER BY ч.дата DESC",
                DB.P("@f", from), DB.P("@t", to));
            _dgvCheki.DataSource = dt;
        }

        private void FormOtchet()
        {
            var dlg = new Form
            {
                Text = "Сформировать кассовый отчёт", Size = new Size(360, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false,
                BackColor = Clr.BgWhite
            };

            int y = 16;
            void Row(string lbl, Control ctrl)
            {
                dlg.Controls.Add(UI.MakeLabel(lbl, true).Also(l => l.Location = new Point(16, y + 2)));
                ctrl.Location = new Point(110, y); ctrl.Width = 210; dlg.Controls.Add(ctrl); y += 38;
            }

            var cmbK = UI.MakeCombo(210);
            var dtK = DB.Query("SELECT id, номер FROM Кассы");
            cmbK.DataSource = dtK; cmbK.DisplayMember = "номер"; cmbK.ValueMember = "id";

            var dtp = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today, Font = new Font("Segoe UI", 9f) };

            Row("Касса:", cmbK);
            Row("Дата:",  dtp);

            var btnOk = UI.MakeBtn("Сформировать", 130, 32);
            btnOk.Location = new Point(110, y); btnOk.DialogResult = DialogResult.OK;
            btnOk.Click += (s, e) =>
            {
                if (cmbK.SelectedValue == null) return;
                int kassaId = (int)cmbK.SelectedValue;
                DateTime date = dtp.Value.Date;

                var stats = DB.Query(@"
                    SELECT COUNT(*) AS cnt, ISNULL(SUM(итого),0) AS summa
                    FROM Чеки
                    WHERE кассаId=@k AND CAST(дата AS DATE)=@d AND аннулирован=0",
                    DB.P("@k", kassaId), DB.P("@d", date));

                int cnt      = Convert.ToInt32(stats.Rows[0]["cnt"]);
                decimal summa = Convert.ToDecimal(stats.Rows[0]["summa"]);

                DB.Exec(@"INSERT INTO КассовыеОтчёты(кассаId,кассирId,дата,суммаЗаСмену,количествоЧеков)
                          VALUES(@k,@s,@d,@sum,@cnt)",
                    DB.P("@k",   kassaId),
                    DB.P("@s",   Session.UserId),
                    DB.P("@d",   date),
                    DB.P("@sum", summa),
                    DB.P("@cnt", cnt));

                LoadOtchyoty(); LoadSummary();
                MessageBox.Show($"Отчёт сформирован.\nЧеков: {cnt}\nСумма: {summa:N2} ₽",
                    "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            var btnCnl = UI.MakeBtnOutline("Отмена", 80, 32);
            btnCnl.Location = new Point(248, y); btnCnl.DialogResult = DialogResult.Cancel;
            dlg.Controls.Add(btnOk); dlg.Controls.Add(btnCnl);
            dlg.ShowDialog(this);
        }
    }
}
