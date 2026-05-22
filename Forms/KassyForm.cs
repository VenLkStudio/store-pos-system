using System;
using System.Drawing;
using System.Windows.Forms;
using RetailShop.Database;
using RetailShop.Models;

namespace RetailShop.Forms
{
    // ═══════════════════════════════════════════════════════════════════════
    //  Экран 10 – Управление кассами (открытие/закрытие смен)
    //  Wireframe: таблица касс + кнопки Открыть смену / Закрыть смену
    // ═══════════════════════════════════════════════════════════════════════
    public class KassyForm : Form
    {
        private DataGridView _dgv;
        private Label        _lblStatus;

        public KassyForm() { Build(); Load_(); }

        private void Build()
        {
            BackColor = Clr.BgApp;

            var title = UI.MakeTitle("Управление кассами");
            title.Location = new Point(0, 0);

            var sub = UI.MakeLabel("Открытие и закрытие смен");
            sub.Location = new Point(0, 30);

            // toolbar
            var btnOpen = UI.MakeBtn("▶  Открыть смену", 150, 30);
            btnOpen.Location  = new Point(0, 56);
            btnOpen.BackColor = System.Drawing.Color.FromArgb(40, 120, 40);
            btnOpen.Click    += (s, e) => SetStatus("Открыта");

            var btnClose = UI.MakeBtn("■  Закрыть смену", 150, 30);
            btnClose.Location  = new Point(160, 56);
            btnClose.BackColor = System.Drawing.Color.FromArgb(140, 40, 40);
            btnClose.Click    += (s, e) => SetStatus("Закрыта");

            var btnAdd = UI.MakeBtnOutline("+ Добавить кассу", 150, 30);
            btnAdd.Location = new Point(320, 56);
            btnAdd.Click   += (s, e) => AddKassa();

            var btnRefresh = UI.MakeBtnOutline("Обновить", 90, 30);
            btnRefresh.Location = new Point(480, 56);
            btnRefresh.Click   += (s, e) => Load_();

            _dgv = UI.MakeGrid();
            _dgv.Location = new Point(0, 96);
            _dgv.Size     = new Size(920, 480);
            _dgv.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _dgv.CellFormatting += (s, e) =>
            {
                if (_dgv.Columns.Contains("Статус") && e.ColumnIndex == _dgv.Columns["Статус"].Index && e.Value != null)
                {
                    e.CellStyle.ForeColor = e.Value.ToString() == "Открыта"
                        ? System.Drawing.Color.FromArgb(30, 120, 30)
                        : System.Drawing.Color.FromArgb(140, 40, 40);
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
            };

            _lblStatus = new Label
            {
                Dock = DockStyle.Bottom, Height = 22,
                Font = new Font("Segoe UI", 8.5f), ForeColor = Clr.TextSecond,
                BackColor = Clr.BgApp, TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0)
            };

            Controls.AddRange(new Control[] { title, sub, btnOpen, btnClose, btnAdd, btnRefresh, _dgv, _lblStatus });
        }

        private void Load_()
        {
            var dt = DB.Query(@"
                SELECT к.id,
                       к.номер   AS [Касса],
                       к.статус  AS [Статус],
                       (SELECT COUNT(*) FROM Чеки ч
                        WHERE ч.кассаId=к.id
                          AND CAST(ч.дата AS DATE)=CAST(GETDATE() AS DATE)) AS [Чеков сегодня],
                       (SELECT ISNULL(SUM(итого),0) FROM Чеки ч
                        WHERE ч.кассаId=к.id
                          AND CAST(ч.дата AS DATE)=CAST(GETDATE() AS DATE)) AS [Выручка сегодня, ₽]
                FROM Кассы к
                ORDER BY к.номер");
            _dgv.DataSource = dt;
            UI.HideCols(_dgv, "id");
        }

        private void SetStatus(string status)
        {
            if (_dgv.SelectedRows.Count == 0) { _lblStatus.Text = "Выберите кассу"; return; }
            int id = (int)_dgv.SelectedRows[0].Cells["id"].Value;
            DB.Exec("UPDATE Кассы SET статус=@s WHERE id=@id", DB.P("@s", status), DB.P("@id", id));
            Load_();
            _lblStatus.Text = $"Касса {_dgv.SelectedRows[0].Cells["Касса"]?.Value}: статус изменён → {status}";
        }

        private void AddKassa()
        {
            // Simple inline input dialog
            string name = "";
            var idlg = new Form { Text = "Добавить кассу", Size = new Size(300, 130),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false,
                BackColor = Clr.BgWhite };
            var txt = UI.MakeField(230); txt.Location = new Point(16, 16); txt.Text = "Касса №";
            var bOk = UI.MakeBtn("Добавить", 100, 30); bOk.Location = new Point(16, 54); bOk.DialogResult = DialogResult.OK;
            bOk.Click += (bs, be) => { name = txt.Text; };
            var bCnl = UI.MakeBtnOutline("Отмена", 80, 30); bCnl.Location = new Point(124, 54); bCnl.DialogResult = DialogResult.Cancel;
            idlg.Controls.AddRange(new Control[] { txt, bOk, bCnl });
            if (idlg.ShowDialog() != DialogResult.OK) return;
            if (string.IsNullOrWhiteSpace(name)) return;
            DB.Exec("INSERT INTO Кассы(номер,статус) VALUES(@n,'Закрыта')", DB.P("@n", name));
            Load_();
        }
    }
}
