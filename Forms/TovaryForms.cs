using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using RetailShop.Database;

namespace RetailShop.Forms
{
    // ═══════════════════════════════════════════════════════════════════════
    //  Экран 05 – Справочник товаров
    //  Wireframe: поиск + таблица + кнопки Добавить / Изменить / Удалить
    // ═══════════════════════════════════════════════════════════════════════
    public class TovaryForm : Form
    {
        private DataGridView _dgv;
        private TextBox      _search;

        public TovaryForm() { Build(); Load_(""); }

        private void Build()
        {
            BackColor = Clr.BgApp;

            var title = UI.MakeTitle("Справочник товаров");
            title.Location = new Point(0, 0);

            // toolbar
            _search = UI.MakeField(220, "Поиск по названию...");
            _search.Location     = new Point(0, 38);
            _search.TextChanged += (s, e) =>
            {
                string v = _search.Text;
                if (v == "Поиск по названию...") v = "";
                Load_(v);
            };

            var btnAdd = UI.MakeBtn("+ Добавить", 110, 30);
            btnAdd.Location = new Point(230, 38);
            btnAdd.Click   += (s, e) => OpenEdit(-1);

            var btnEdit = UI.MakeBtnOutline("Изменить", 100, 30);
            btnEdit.Location = new Point(348, 38);
            btnEdit.Click   += (s, e) =>
            {
                if (_dgv.SelectedRows.Count == 0) return;
                OpenEdit((int)_dgv.SelectedRows[0].Cells["id"].Value);
            };

            var btnDel = UI.MakeBtnOutline("Удалить", 90, 30);
            btnDel.Location = new Point(456, 38);
            btnDel.ForeColor = Models.Clr.TextSecond;
            btnDel.Click += (s, e) =>
            {
                if (_dgv.SelectedRows.Count == 0) return;
                if (MessageBox.Show("Удалить товар?", "Подтверждение",
                    MessageBoxButtons.YesNo) != DialogResult.Yes) return;
                int id = (int)_dgv.SelectedRows[0].Cells["id"].Value;
                DB.Exec("DELETE FROM Товары WHERE id=@id", DB.P("@id", id));
                Load_("");
            };

            _dgv = UI.MakeGrid();
            _dgv.Location = new Point(0, 78);
            _dgv.Size     = new Size(920, 520);
            _dgv.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            Controls.AddRange(new Control[] { title, _search, btnAdd, btnEdit, btnDel, _dgv });
        }

        private void Load_(string q)
        {
            var dt = DB.Query(@"
                SELECT т.id, т.название AS [Название], т.штрихкод AS [Штрихкод],
                       т.единицаИзмерения AS [Ед.изм],
                       ISNULL(с.количество,0) AS [На складе],
                       ISNULL(рц.розничнаяЦена,0) AS [Розн. цена, ₽]
                FROM Товары т
                LEFT JOIN Склад с         ON с.товарId=т.id
                LEFT JOIN РозничныеЦены рц ON рц.товарId=т.id
                WHERE т.название LIKE @q OR т.штрихкод LIKE @q
                ORDER BY т.название",
                DB.P("@q", "%" + q + "%"));
            _dgv.DataSource = dt;
            UI.HideCols(_dgv, "id");
        }

        private void OpenEdit(int id)
        {
            var dlg = new EditTovarDialog(id);
            if (dlg.ShowDialog() == DialogResult.OK) Load_("");
        }
    }

    // ─── Диалог редактирования товара ────────────────────────────────────────
    public class EditTovarDialog : Form
    {
        private readonly int _id;
        public EditTovarDialog(int id)
        {
            _id             = id;
            Text            = id < 0 ? "Новый товар" : "Изменить товар";
            Size            = new Size(360, 240);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            BackColor       = Models.Clr.BgWhite;
            Build();
        }

        private void Build()
        {
            int y = 16;
            void Row(string lbl, Control ctrl)
            {
                Controls.Add(UI.MakeLabel(lbl, true).Also(l => l.Location = new Point(16, y + 2)));
                ctrl.Location = new Point(120, y); ctrl.Width = 210;
                Controls.Add(ctrl); y += 36;
            }

            var txtName = UI.MakeField(210);
            var txtBar  = UI.MakeField(210);
            var cmbEd   = UI.MakeCombo(210);
            cmbEd.Items.AddRange(new[] { "шт", "кг", "л", "упак", "м" });
            cmbEd.SelectedIndex = 0;

            if (_id > 0)
            {
                var r = DB.Query("SELECT * FROM Товары WHERE id=@id", DB.P("@id", _id));
                if (r.Rows.Count > 0)
                {
                    txtName.Text = r.Rows[0]["название"].ToString();
                    txtBar.Text  = r.Rows[0]["штрихкод"].ToString();
                    string ed    = r.Rows[0]["единицаИзмерения"].ToString();
                    if (cmbEd.Items.Contains(ed)) cmbEd.SelectedItem = ed;
                }
            }

            Row("Название:", txtName);
            Row("Штрихкод:", txtBar);
            Row("Ед. изм.:", cmbEd);

            var btnOk = UI.MakeBtn("Сохранить", 110, 32);
            btnOk.Location     = new Point(120, y);
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text)) return;
                if (_id < 0)
                    DB.Exec("INSERT INTO Товары(название,штрихкод,единицаИзмерения) VALUES(@n,@b,@e)",
                        DB.P("@n", txtName.Text), DB.P("@b", txtBar.Text), DB.P("@e", cmbEd.SelectedItem));
                else
                    DB.Exec("UPDATE Товары SET название=@n,штрихкод=@b,единицаИзмерения=@e WHERE id=@id",
                        DB.P("@n", txtName.Text), DB.P("@b", txtBar.Text), DB.P("@e", cmbEd.SelectedItem), DB.P("@id", _id));
            };
            var btnCnl = UI.MakeBtnOutline("Отмена", 90, 32);
            btnCnl.Location     = new Point(238, y);
            btnCnl.DialogResult = DialogResult.Cancel;
            Controls.Add(btnOk); Controls.Add(btnCnl);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Экран 06 – Остатки склада
    //  Wireframe: таблица Товар | Ед | На складе | Секция | Цена  (read-only)
    // ═══════════════════════════════════════════════════════════════════════
    public class SkladForm : Form
    {
        private DataGridView _dgv;

        public SkladForm() { Build(); Load_(); }

        private void Build()
        {
            BackColor = Clr.BgApp;

            var title = UI.MakeTitle("Остатки склада");
            title.Location = new Point(0, 0);

            var btnRef = UI.MakeBtnOutline("Обновить", 90, 30);
            btnRef.Location = new Point(0, 38);
            btnRef.Click   += (s, e) => Load_();

            _dgv = UI.MakeGrid();
            _dgv.Location = new Point(0, 78);
            _dgv.Size     = new Size(920, 520);
            _dgv.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _dgv.CellFormatting += (s, e) =>
            {
                if (_dgv.Columns.Count > 3 && e.ColumnIndex == _dgv.Columns["На складе"].Index)
                    if (e.Value != null && Convert.ToInt32(e.Value) < 10)
                        e.CellStyle.ForeColor = System.Drawing.Color.FromArgb(180, 40, 40);
            };

            Controls.AddRange(new Control[] { title, btnRef, _dgv });
        }

        private void Load_()
        {
            var dt = DB.Query(@"
                SELECT т.название AS [Товар], т.единицаИзмерения AS [Ед.изм],
                       ISNULL(с.количество,0) AS [На складе], с.секция AS [Секция],
                       ISNULL(рц.розничнаяЦена,0) AS [Розн. цена, ₽]
                FROM Товары т
                LEFT JOIN Склад с          ON с.товарId=т.id
                LEFT JOIN РозничныеЦены рц ON рц.товарId=т.id
                ORDER BY т.название");
            _dgv.DataSource = dt;
        }
    }
}