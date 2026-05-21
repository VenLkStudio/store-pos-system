using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using RetailShop.Database;

namespace RetailShop.Forms
{
    // ── Товары ───────────────────────────────────────────────────────────────
    public class TovaryForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSearch;

        public TovaryForm()
        {
            this.Text = "Товары";
            this.BackColor = Color.FromArgb(245, 245, 248);

            var lblTitle = new Label
            {
                Text = "🗃  Справочник товаров",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 35, 50),
                AutoSize = true,
                Location = new Point(0, 0)
            };

            txtSearch = new TextBox
            {
                Text = "🔍 Поиск...",
                Location = new Point(0, 44),
                Width = 300,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (s, e) => LoadData(txtSearch.Text);

            var btnAdd = new Button
            {
                Text = "+ Добавить",
                Location = new Point(310, 41),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => ShowEditDialog(-1);

            var btnEdit = new Button
            {
                Text = "✏ Изменить",
                Location = new Point(430, 41),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += (s, e) =>
            {
                if (dgv.SelectedRows.Count == 0) return;
                ShowEditDialog((int)dgv.SelectedRows[0].Cells["id"].Value);
            };

            dgv = CreateStyledGrid();
            dgv.Location = new Point(0, 85);
            dgv.Size = new Size(900, 500);
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            this.Controls.AddRange(new Control[] { lblTitle, txtSearch, btnAdd, btnEdit, dgv });
            LoadData("");
        }

        private void LoadData(string search)
        {
            var sql = @"SELECT т.id, т.название as [Название], т.ашС as [Штрихкод], 
                        т.единицаИзмерения as [Ед.изм],
                        ISNULL(с.количество,0) as [На складе],
                        ISNULL(рц.розничнаяЦена,0) as [Цена]
                        FROM Товары т
                        LEFT JOIN Склад с ON т.id=с.товарId
                        LEFT JOIN РозничныеЦены рц ON т.id=рц.товарId
                        WHERE т.название LIKE @s OR т.ашС LIKE @s
                        ORDER BY т.название";
            dgv.DataSource = DbHelper.ExecuteQuery(sql,
                new[] { new SqlParameter("@s", "%" + search + "%") });
            if (dgv.Columns.Contains("id")) dgv.Columns["id"].Visible = false;
        }

        private void ShowEditDialog(int id)
        {
            var dlg = new Form
            {
                Text = id < 0 ? "Новый товар" : "Изменить товар",
                Size = new Size(360, 230),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };

            var txtNaz = new TextBox { Location = new Point(120, 20), Width = 200, Font = new Font("Segoe UI", 10) };
            var txtBarcode = new TextBox { Location = new Point(120, 60), Width = 200, Font = new Font("Segoe UI", 10) };
            var cmbEd = new ComboBox { Location = new Point(120, 100), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbEd.Items.AddRange(new[] { "шт", "кг", "л", "упак", "м" });
            cmbEd.SelectedIndex = 0;

            if (id > 0)
            {
                var dt = DbHelper.ExecuteQuery("SELECT * FROM Товары WHERE id=@id",
                    new[] { new SqlParameter("@id", id) });
                if (dt.Rows.Count > 0)
                {
                    txtNaz.Text = dt.Rows[0]["название"].ToString();
                    txtBarcode.Text = dt.Rows[0]["ашС"].ToString();
                    var ed = dt.Rows[0]["единицаИзмерения"].ToString();
                    if (cmbEd.Items.Contains(ed)) cmbEd.SelectedItem = ed;
                }
            }

            dlg.Controls.Add(new Label { Text = "Название:", Location = new Point(15, 23), AutoSize = true });
            dlg.Controls.Add(txtNaz);
            dlg.Controls.Add(new Label { Text = "Штрихкод:", Location = new Point(15, 63), AutoSize = true });
            dlg.Controls.Add(txtBarcode);
            dlg.Controls.Add(new Label { Text = "Ед. изм.:", Location = new Point(15, 103), AutoSize = true });
            dlg.Controls.Add(cmbEd);

            var btnOk = new Button
            {
                Text = "Сохранить", Location = new Point(120, 150), Size = new Size(110, 34),
                BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtNaz.Text)) return;
                if (id < 0)
                    DbHelper.ExecuteNonQuery("INSERT INTO Товары(название,ашС,единицаИзмерения) VALUES(@n,@b,@e)",
                        new[] { new SqlParameter("@n", txtNaz.Text), new SqlParameter("@b", txtBarcode.Text), new SqlParameter("@e", cmbEd.SelectedItem) });
                else
                    DbHelper.ExecuteNonQuery("UPDATE Товары SET название=@n,ашС=@b,единицаИзмерения=@e WHERE id=@id",
                        new[] { new SqlParameter("@n", txtNaz.Text), new SqlParameter("@b", txtBarcode.Text), new SqlParameter("@e", cmbEd.SelectedItem), new SqlParameter("@id", id) });
                LoadData(txtSearch.Text);
            };
            dlg.Controls.Add(btnOk);
            dlg.ShowDialog(this);
        }

        private DataGridView CreateStyledGrid()
        {
            var dgv = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10),
                GridColor = Color.FromArgb(230, 230, 230)
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(33, 150, 243);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 255);
            return dgv;
        }
    }

    // ── Склад ─────────────────────────────────────────────────────────────────
    public class SkladForm : Form
    {
        private DataGridView dgv;

        public SkladForm()
        {
            this.Text = "Остатки склада";
            this.BackColor = Color.FromArgb(245, 245, 248);

            var lblTitle = new Label
            {
                Text = "🏭  Остатки на складе",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 35, 50),
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var btnRefresh = new Button
            {
                Text = "↻ Обновить",
                Location = new Point(0, 42),
                Size = new Size(110, 30),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(240, 240, 240)
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadData();

            dgv = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10),
                GridColor = Color.FromArgb(230, 230, 230),
                Location = new Point(0, 85),
                Size = new Size(900, 500),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(33, 150, 243);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 255);
            dgv.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex >= 0 && dgv.Columns[e.ColumnIndex].HeaderText == "На складе")
                {
                    if (e.Value != null && Convert.ToInt32(e.Value) < 10)
                        e.CellStyle.ForeColor = Color.FromArgb(244, 67, 54);
                }
            };

            this.Controls.AddRange(new Control[] { lblTitle, btnRefresh, dgv });
            LoadData();
        }

        private void LoadData()
        {
            var sql = @"SELECT т.название as [Товар], т.единицаИзмерения as [Ед.изм],
                        ISNULL(с.количество,0) as [На складе], с.адрес as [Секция],
                        ISNULL(рц.розничнаяЦена,0) as [Розн. цена]
                        FROM Товары т
                        LEFT JOIN Склад с ON т.id=с.товарId
                        LEFT JOIN РозничныеЦены рц ON т.id=рц.товарId
                        ORDER BY т.название";
            dgv.DataSource = DbHelper.ExecuteQuery(sql);
        }
    }

    // ── Розничные цены ────────────────────────────────────────────────────────
    public class RoznichnyCenyForm : Form
    {
        private DataGridView dgv;

        public RoznichnyCenyForm()
        {
            this.Text = "Розничные цены";
            this.BackColor = Color.FromArgb(245, 245, 248);

            var lblTitle = new Label
            {
                Text = "💰  Расчёт розничных цен",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 35, 50),
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var btnSet = new Button
            {
                Text = "✏ Установить цену",
                Location = new Point(0, 42),
                Size = new Size(160, 32),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSet.FlatAppearance.BorderSize = 0;
            btnSet.Click += (s, e) => ShowSetPriceDialog();

            dgv = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10),
                GridColor = Color.FromArgb(230, 230, 230),
                Location = new Point(0, 85),
                Size = new Size(900, 500),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(33, 150, 243);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 255);

            this.Controls.AddRange(new Control[] { lblTitle, btnSet, dgv });
            LoadData();
        }

        private void LoadData()
        {
            var sql = @"SELECT т.id, т.название as [Товар], рц.закупочнаяЦена as [Закупочная],
                        рц.розничнаяЦена as [Розничная], рц.наценка as [Наценка %],
                        рц.датаУстановки as [Дата]
                        FROM Товары т LEFT JOIN РозничныеЦены рц ON т.id=рц.товарId
                        ORDER BY т.название";
            dgv.DataSource = DbHelper.ExecuteQuery(sql);
            if (dgv.Columns.Contains("id")) dgv.Columns["id"].Visible = false;
        }

        private void ShowSetPriceDialog()
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Выберите товар!"); return; }
            int товарId = (int)dgv.SelectedRows[0].Cells["id"].Value;

            var dlg = new Form
            {
                Text = "Установить цену",
                Size = new Size(320, 220),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };

            var numZakup = new NumericUpDown { Location = new Point(140, 20), Width = 140, DecimalPlaces = 2, Minimum = 0, Maximum = 999999, Value = 0 };
            var numNazenka = new NumericUpDown { Location = new Point(140, 60), Width = 140, DecimalPlaces = 2, Minimum = 0, Maximum = 1000, Value = 30 };
            var lblResult = new Label { Location = new Point(15, 100), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            void CalcPrice()
            {
                decimal roz = numZakup.Value * (1 + numNazenka.Value / 100);
                lblResult.Text = $"Розничная цена: {roz:F2} руб.";
            }
            numZakup.ValueChanged += (s, e) => CalcPrice();
            numNazenka.ValueChanged += (s, e) => CalcPrice();
            CalcPrice();

            dlg.Controls.Add(new Label { Text = "Закупочная цена:", Location = new Point(15, 23), AutoSize = true });
            dlg.Controls.Add(numZakup);
            dlg.Controls.Add(new Label { Text = "Наценка (%):", Location = new Point(15, 63), AutoSize = true });
            dlg.Controls.Add(numNazenka);
            dlg.Controls.Add(lblResult);

            var btnSave = new Button
            {
                Text = "Сохранить", Location = new Point(80, 140), Size = new Size(110, 34),
                BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) =>
            {
                decimal roz = numZakup.Value * (1 + numNazenka.Value / 100);
                var exists = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM РозничныеЦены WHERE товарId=@id",
                    new[] { new SqlParameter("@id", товарId) });
                if (Convert.ToInt32(exists) > 0)
                    DbHelper.ExecuteNonQuery(
                        "UPDATE РозничныеЦены SET закупочнаяЦена=@z,розничнаяЦена=@r,наценка=@n,датаУстановки=GETDATE() WHERE товарId=@id",
                        new[] { new SqlParameter("@z", numZakup.Value), new SqlParameter("@r", roz), new SqlParameter("@n", numNazenka.Value), new SqlParameter("@id", товарId) });
                else
                    DbHelper.ExecuteNonQuery(
                        "INSERT INTO РозничныеЦены(товарId,закупочнаяЦена,розничнаяЦена,наценка) VALUES(@id,@z,@r,@n)",
                        new[] { new SqlParameter("@id", товарId), new SqlParameter("@z", numZakup.Value), new SqlParameter("@r", roz), new SqlParameter("@n", numNazenka.Value) });
                LoadData();
            };
            dlg.Controls.Add(btnSave);
            dlg.ShowDialog(this);
        }
    }

    // ── Заявки в торговый зал ─────────────────────────────────────────────────
    public class ZayavkiZalForm : Form
    {
        private DataGridView dgv;

        public ZayavkiZalForm()
        {
            this.Text = "Заявки в торговый зал";
            this.BackColor = Color.FromArgb(245, 245, 248);

            var lblTitle = new Label
            {
                Text = "📋  Заявки в торговый зал",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 35, 50),
                AutoSize = true, Location = new Point(0, 0)
            };

            var btnAdd = new Button
            {
                Text = "+ Создать заявку",
                Location = new Point(0, 42), Size = new Size(150, 32),
                BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => ShowAddDialog();

            var btnComplete = new Button
            {
                Text = "✔ Выполнить",
                Location = new Point(160, 42), Size = new Size(120, 32),
                BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnComplete.FlatAppearance.BorderSize = 0;
            btnComplete.Click += (s, e) =>
            {
                if (dgv.SelectedRows.Count == 0) return;
                int id = (int)dgv.SelectedRows[0].Cells["id"].Value;
                DbHelper.ExecuteNonQuery("UPDATE ЗаявкиВТорговыйЗал SET статус='Выполнена' WHERE id=@id",
                    new[] { new SqlParameter("@id", id) });
                LoadData();
            };

            dgv = new DataGridView
            {
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false, BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10),
                GridColor = Color.FromArgb(230, 230, 230),
                Location = new Point(0, 85), Size = new Size(900, 500),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(33, 150, 243);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 255);

            this.Controls.AddRange(new Control[] { lblTitle, btnAdd, btnComplete, dgv });
            LoadData();
        }

        private void LoadData()
        {
            var sql = @"SELECT з.id, т.название as [Товар], з.количество as [Количество],
                        сотр.ФИО as [Администратор], з.датаЗаявки as [Дата], з.статус as [Статус]
                        FROM ЗаявкиВТорговыйЗал з
                        JOIN Товары т ON з.товарId=т.id
                        JOIN Сотрудники сотр ON з.администраторId=сотр.id
                        ORDER BY з.датаЗаявки DESC";
            dgv.DataSource = DbHelper.ExecuteQuery(sql);
            if (dgv.Columns.Contains("id")) dgv.Columns["id"].Visible = false;
        }

        private void ShowAddDialog()
        {
            var dlg = new Form
            {
                Text = "Новая заявка",
                Size = new Size(360, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };
            var cmbTovar = new ComboBox { Location = new Point(120, 20), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            var dt = DbHelper.ExecuteQuery("SELECT id, название FROM Товары");
            cmbTovar.DataSource = dt;
            cmbTovar.DisplayMember = "название";
            cmbTovar.ValueMember = "id";

            var numKol = new NumericUpDown { Location = new Point(120, 60), Width = 100, Minimum = 1, Maximum = 10000, Value = 1 };

            dlg.Controls.Add(new Label { Text = "Товар:", Location = new Point(15, 23), AutoSize = true });
            dlg.Controls.Add(cmbTovar);
            dlg.Controls.Add(new Label { Text = "Количество:", Location = new Point(15, 63), AutoSize = true });
            dlg.Controls.Add(numKol);

            var btnOk = new Button
            {
                Text = "Создать", Location = new Point(120, 110), Size = new Size(110, 34),
                BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) =>
            {
                DbHelper.ExecuteNonQuery(
                    "INSERT INTO ЗаявкиВТорговыйЗал(товарId,количество,администраторId) VALUES(@t,@k,@a)",
                    new[] {
                        new SqlParameter("@t", cmbTovar.SelectedValue),
                        new SqlParameter("@k", (int)numKol.Value),
                        new SqlParameter("@a", Models.Session.UserId)
                    });
                LoadData();
            };
            dlg.Controls.Add(btnOk);
            dlg.ShowDialog(this);
        }
    }
}
