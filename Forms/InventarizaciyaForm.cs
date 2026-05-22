using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using RetailShop.Database;
using RetailShop.Models;

namespace RetailShop.Forms
{
    // ═══════════════════════════════════════════════════════════════════════
    //  Экраны 14-15 – Инвентаризация
    //  Wireframe: список сессий инвентаризации (верх) +
    //             строки сверки остатков (низ) + итоговая сводка расхождений
    // ═══════════════════════════════════════════════════════════════════════
    public class InventarizaciyaForm : Form
    {
        private DataGridView _dgvSessions, _dgvRows;
        private Label        _lblInfo;
        private int          _selInvId = -1;

        public InventarizaciyaForm() { Build(); LoadSessions(); }

        private void Build()
        {
            BackColor = Clr.BgApp;

            var title = UI.MakeTitle("Инвентаризация");
            title.Location = new Point(0, 0);

            var sub = UI.MakeLabel("Сверка фактических остатков со складом и залом");
            sub.Location = new Point(0, 30);

            // ── toolbar ───────────────────────────────────────────────────────
            var btnNew = UI.MakeBtn("+ Начать инвентаризацию", 200, 30);
            btnNew.Location = new Point(0, 56);
            btnNew.Click   += (s, e) => NewSession();

            var btnFinish = UI.MakeBtnOutline("✓ Завершить", 120, 30);
            btnFinish.Location = new Point(210, 56);
            btnFinish.Click   += (s, e) => FinishSession();

            var btnRefresh = UI.MakeBtnOutline("Обновить", 90, 30);
            btnRefresh.Location = new Point(340, 56);
            btnRefresh.Click   += (s, e) => { LoadSessions(); if (_selInvId > 0) LoadRows(); };

            // ── Sessions grid ─────────────────────────────────────────────────
            var lblSess = UI.MakeLabel("Сессии инвентаризации", true);
            lblSess.Location = new Point(0, 96);

            _dgvSessions = UI.MakeGrid();
            _dgvSessions.Location         = new Point(0, 116);
            _dgvSessions.Size             = new Size(940, 180);
            _dgvSessions.Anchor           = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _dgvSessions.SelectionChanged += (s, e) => OnSelectSession();
            _dgvSessions.CellFormatting  += (s, e) =>
            {
                if (_dgvSessions.Columns.Contains("Статус") &&
                    e.ColumnIndex == _dgvSessions.Columns["Статус"].Index && e.Value != null)
                {
                    e.CellStyle.ForeColor = e.Value.ToString() == "Завершена"
                        ? System.Drawing.Color.FromArgb(30, 120, 30)
                        : System.Drawing.Color.FromArgb(180, 120, 0);
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
            };

            // ── Rows grid ─────────────────────────────────────────────────────
            var lblRows = UI.MakeLabel("Строки сверки (выберите инвентаризацию выше)", true);
            lblRows.Location = new Point(0, 308);

            _dgvRows = UI.MakeGrid();
            _dgvRows.Location = new Point(0, 328);
            _dgvRows.Size     = new Size(940, 220);
            _dgvRows.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _dgvRows.CellFormatting += (s, e) =>
            {
                if (_dgvRows.Columns.Contains("Расхождение") &&
                    e.ColumnIndex == _dgvRows.Columns["Расхождение"].Index && e.Value != null)
                {
                    int diff = Convert.ToInt32(e.Value);
                    e.CellStyle.ForeColor = diff < 0
                        ? System.Drawing.Color.FromArgb(160, 30, 30)
                        : diff > 0
                            ? System.Drawing.Color.FromArgb(30, 120, 30)
                            : Clr.TextPrimary;
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
            };

            // ── Add row bar ───────────────────────────────────────────────────
            var rowBar = new Panel { Location = new Point(0, 556), Height = 40, BackColor = Clr.BgApp };

            var cmbT = UI.MakeCombo(220); cmbT.Location = new Point(0, 5);
            var dtT  = DB.Query("SELECT id, название FROM Товары");
            cmbT.DataSource = dtT; cmbT.DisplayMember = "название"; cmbT.ValueMember = "id";

            var numFact = new NumericUpDown { Location = new Point(232, 5), Width = 80, Minimum = 0, Maximum = 99999, Font = new Font("Segoe UI", 9f) };
            var lblF    = UI.MakeLabel("Факт. кол-во:"); lblF.Location = new Point(228, -1);

            var btnAddRow = UI.MakeBtn("+ Добавить строку", 150, 30);
            btnAddRow.Location = new Point(325, 5);
            btnAddRow.Click   += (s, e) => AddRow(cmbT, numFact);

            rowBar.Controls.AddRange(new Control[]
            {
                UI.MakeLabel("Товар:").Also(l => l.Location = new Point(0, -1)),
                cmbT, lblF, numFact, btnAddRow
            });

            // ── Info strip ────────────────────────────────────────────────────
            _lblInfo = new Label
            {
                Dock = DockStyle.Bottom, Height = 22,
                Font = new Font("Segoe UI", 8.5f), ForeColor = Clr.TextSecond,
                BackColor = Clr.BgApp, TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0)
            };

            Controls.AddRange(new Control[]
            {
                title, sub, btnNew, btnFinish, btnRefresh,
                lblSess, _dgvSessions,
                lblRows, _dgvRows,
                rowBar, _lblInfo
            });
        }

        // ── Load ──────────────────────────────────────────────────────────────
        private void LoadSessions()
        {
            var dt = DB.Query(@"
                SELECT и.id,
                       FORMAT(и.дата,'dd.MM.yyyy HH:mm') AS [Дата],
                       и.ответственный                   AS [Ответственный],
                       и.статус                          AS [Статус],
                       (SELECT COUNT(*) FROM СтрокиИнвентаризации
                        WHERE инвентаризацияId=и.id)     AS [Строк],
                       (SELECT COUNT(*) FROM СтрокиИнвентаризации
                        WHERE инвентаризацияId=и.id
                          AND фактическийОстаток<>остатокПоБазе) AS [Расхождений]
                FROM Инвентаризация и
                ORDER BY и.дата DESC");
            _dgvSessions.DataSource = dt;
            UI.HideCols(_dgvSessions, "id");
        }

        private void LoadRows()
        {
            if (_selInvId < 0) return;
            var dt = DB.Query(@"
                SELECT т.название                                   AS [Товар],
                       т.единицаИзмерения                          AS [Ед],
                       си.остатокПоБазе                            AS [По базе],
                       си.фактическийОстаток                       AS [Факт],
                       (си.фактическийОстаток - си.остатокПоБазе) AS [Расхождение]
                FROM СтрокиИнвентаризации си
                JOIN Товары т ON т.id = си.товарId
                WHERE си.инвентаризацияId = @id
                ORDER BY т.название",
                DB.P("@id", _selInvId));
            _dgvRows.DataSource = dt;

            // Summary
            int minus = 0, plus = 0;
            foreach (DataRow r in dt.Rows)
            {
                int d = Convert.ToInt32(r["Расхождение"]);
                if (d < 0) minus++; else if (d > 0) plus++;
            }
            _lblInfo.Text = $"Инвентаризация №{_selInvId} | " +
                            $"Строк: {dt.Rows.Count} | " +
                            $"Недостача: {minus} | Излишек: {plus}";
        }

        // ── Actions ───────────────────────────────────────────────────────────
        private void OnSelectSession()
        {
            if (_dgvSessions.SelectedRows.Count == 0) return;
            _selInvId = (int)_dgvSessions.SelectedRows[0].Cells["id"].Value;
            LoadRows();
        }

        private void NewSession()
        {
            DB.Exec("INSERT INTO Инвентаризация(ответственный,статус) VALUES(@r,'В процессе')",
                DB.P("@r", Session.FIO));
            LoadSessions();
            _lblInfo.Text = "Новая инвентаризация создана. Добавьте строки сверки.";
        }

        private void FinishSession()
        {
            if (_selInvId < 0) { _lblInfo.Text = "Выберите инвентаризацию"; return; }
            if (MessageBox.Show("Завершить инвентаризацию?", "Подтверждение",
                MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            DB.Exec("UPDATE Инвентаризация SET статус='Завершена' WHERE id=@id", DB.P("@id", _selInvId));
            LoadSessions(); LoadRows();
        }

        private void AddRow(Win11Combo cmbT, NumericUpDown numFact)
        {
            if (_selInvId < 0) { _lblInfo.Text = "Сначала выберите инвентаризацию"; return; }
            if (cmbT.SelectedValue == null) return;

            int tovarId = (int)cmbT.SelectedValue;
            int fact    = (int)numFact.Value;

            // get base stock from warehouse
            var bObj = DB.Scalar("SELECT ISNULL(количество,0) FROM Склад WHERE товарId=@t", DB.P("@t", tovarId));
            int base_ = Convert.ToInt32(bObj ?? 0);

            // check duplicate
            var ex = Convert.ToInt32(DB.Scalar(
                "SELECT COUNT(*) FROM СтрокиИнвентаризации WHERE инвентаризацияId=@i AND товарId=@t",
                DB.P("@i", _selInvId), DB.P("@t", tovarId)));
            if (ex > 0)
            {
                DB.Exec(@"UPDATE СтрокиИнвентаризации
                          SET фактическийОстаток=@f, остатокПоБазе=@b
                          WHERE инвентаризацияId=@i AND товарId=@t",
                    DB.P("@f", fact), DB.P("@b", base_),
                    DB.P("@i", _selInvId), DB.P("@t", tovarId));
            }
            else
            {
                DB.Exec(@"INSERT INTO СтрокиИнвентаризации
                          (инвентаризацияId,товарId,остатокПоБазе,фактическийОстаток)
                          VALUES(@i,@t,@b,@f)",
                    DB.P("@i", _selInvId), DB.P("@t", tovarId),
                    DB.P("@b", base_), DB.P("@f", fact));
            }
            LoadRows();
        }
    }
}
