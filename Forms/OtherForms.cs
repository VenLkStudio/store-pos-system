using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using RetailShop.Database;
using RetailShop.Models;

namespace RetailShop.Forms
{
    // ── Возврат товара поставщику ─────────────────────────────────────────────
    public class VozvratForm : Form
    {
        private DataGridView dgv;

        public VozvratForm()
        {
            this.Text = "Возврат поставщику";
            this.BackColor = Color.FromArgb(245, 245, 248);

            var lblTitle = new Label
            {
                Text = "↩  Возврат товара поставщику",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 35, 50),
                AutoSize = true, Location = new Point(0, 0)
            };

            var btnAdd = new Button
            {
                Text = "+ Оформить возврат",
                Location = new Point(0, 42), Size = new Size(170, 32),
                BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => ShowAddDialog();

            dgv = CreateGrid();
            dgv.Location = new Point(0, 85);
            dgv.Size = new Size(900, 500);
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            this.Controls.AddRange(new Control[] { lblTitle, btnAdd, dgv });
            LoadData();
        }

        private void LoadData()
        {
            var sql = @"SELECT в.id, п.датаПоступления as [Партия от],
                        пост.название as [Поставщик], т.название as [Товар],
                        в.количество as [Количество], в.причинаВозврата as [Причина],
                        сотр.ФИО as [Товаровед], в.дата as [Дата возврата], в.статус as [Статус]
                        FROM ВозвратныеНакладные в
                        JOIN ПартииТовара п ON в.партияId=п.id
                        JOIN Поставщики пост ON п.поставщикId=пост.id
                        JOIN Товары т ON в.товарId=т.id
                        JOIN Сотрудники сотр ON в.товаровЕдId=сотр.id
                        ORDER BY в.дата DESC";
            dgv.DataSource = DbHelper.ExecuteQuery(sql);
            if (dgv.Columns.Contains("id")) dgv.Columns["id"].Visible = false;
        }

        private void ShowAddDialog()
        {
            var dlg = new Form
            {
                Text = "Оформить возврат",
                Size = new Size(400, 310),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };

            int y = 20;
            void Row(string label, Control ctrl)
            {
                dlg.Controls.Add(new Label { Text = label, Location = new Point(15, y + 2), AutoSize = true });
                ctrl.Location = new Point(140, y); ctrl.Width = 220;
                dlg.Controls.Add(ctrl);
                y += 36;
            }

            var dtPartii = DbHelper.ExecuteQuery("SELECT id, CAST(датаПоступления AS DATE) as [дата] FROM ПартииТовара ORDER BY датаПоступления DESC");
            var cmbPartia = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPartia.DataSource = dtPartii;
            cmbPartia.DisplayMember = "дата";
            cmbPartia.ValueMember = "id";

            var dtTovary = DbHelper.ExecuteQuery("SELECT id, название FROM Товары");
            var cmbTovar = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTovar.DataSource = dtTovary;
            cmbTovar.DisplayMember = "название";
            cmbTovar.ValueMember = "id";

            var numKol = new NumericUpDown { Minimum = 1, Maximum = 10000, Value = 1 };
            var txtPrichina = new TextBox { Multiline = true, Height = 60 };

            Row("Партия:", cmbPartia);
            Row("Товар:", cmbTovar);
            Row("Количество:", numKol);
            dlg.Controls.Add(new Label { Text = "Причина:", Location = new Point(15, y + 2), AutoSize = true });
            txtPrichina.Location = new Point(140, y); txtPrichina.Width = 220;
            dlg.Controls.Add(txtPrichina);
            y += 70;

            var btnOk = new Button
            {
                Text = "Оформить", Location = new Point(140, y), Size = new Size(120, 34),
                BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) =>
            {
                if (cmbPartia.SelectedValue == null || cmbTovar.SelectedValue == null) return;

                // Reduce warehouse stock
                DbHelper.ExecuteNonQuery(
                    "UPDATE Склад SET количество=количество-@k WHERE товарId=@t AND количество>=@k",
                    new[] { new SqlParameter("@k", (int)numKol.Value), new SqlParameter("@t", cmbTovar.SelectedValue) });

                DbHelper.ExecuteNonQuery(
                    "INSERT INTO ВозвратныеНакладные(партияId,товаровЕдId,причинаВозврата,количество,товарId) VALUES(@p,@s,@pr,@k,@t)",
                    new[] {
                        new SqlParameter("@p", cmbPartia.SelectedValue),
                        new SqlParameter("@s", Session.UserId),
                        new SqlParameter("@pr", txtPrichina.Text),
                        new SqlParameter("@k", (int)numKol.Value),
                        new SqlParameter("@t", cmbTovar.SelectedValue)
                    });
                LoadData();
                MessageBox.Show("✅ Возвратная накладная оформлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            dlg.Controls.Add(btnOk);
            dlg.ShowDialog(this);
        }

        private DataGridView CreateGrid()
        {
            var dgv = new DataGridView
            {
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false, BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10),
                GridColor = Color.FromArgb(230, 230, 230)
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(244, 67, 54);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(255, 248, 248);
            return dgv;
        }
    }

    // ── Кассы ─────────────────────────────────────────────────────────────────
    public class KassyForm : Form
    {
        private DataGridView dgv;

        public KassyForm()
        {
            this.Text = "Управление кассами";
            this.BackColor = Color.FromArgb(245, 245, 248);

            var lblTitle = new Label
            {
                Text = "🖥  Кассы",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 35, 50),
                AutoSize = true, Location = new Point(0, 0)
            };

            var btnOpen = new Button
            {
                Text = "▶ Открыть смену",
                Location = new Point(0, 42), Size = new Size(150, 32),
                BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnOpen.FlatAppearance.BorderSize = 0;
            btnOpen.Click += (s, e) => SetKassaStatus("Открыта");

            var btnClose = new Button
            {
                Text = "⏹ Закрыть смену",
                Location = new Point(160, 42), Size = new Size(150, 32),
                BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => SetKassaStatus("Закрыта");

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
            dgv.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex >= 0 && dgv.Columns[e.ColumnIndex].HeaderText == "Статус")
                {
                    if (e.Value?.ToString() == "Открыта") e.CellStyle.ForeColor = Color.Green;
                    else e.CellStyle.ForeColor = Color.Red;
                }
            };

            this.Controls.AddRange(new Control[] { lblTitle, btnOpen, btnClose, dgv });
            LoadData();
        }

        private void LoadData()
        {
            dgv.DataSource = DbHelper.ExecuteQuery("SELECT id, номер as [Касса], статус as [Статус] FROM Кассы");
            if (dgv.Columns.Contains("id")) dgv.Columns["id"].Visible = false;
        }

        private void SetKassaStatus(string status)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Выберите кассу!"); return; }
            int id = (int)dgv.SelectedRows[0].Cells["id"].Value;
            DbHelper.ExecuteNonQuery("UPDATE Кассы SET статус=@s WHERE id=@id",
                new[] { new SqlParameter("@s", status), new SqlParameter("@id", id) });
            LoadData();
        }
    }

    // ── Реализация товаров ────────────────────────────────────────────────────
    public class RealizaciyaForm : Form
    {
        private DataGridView dgvCart;
        private DataTable cartTable;
        private Label lblTotal;
        private ComboBox cmbKassa;

        public RealizaciyaForm()
        {
            this.Text = "Реализация товаров";
            this.BackColor = Color.FromArgb(245, 245, 248);
            cartTable = new DataTable();
            cartTable.Columns.Add("ТоварId", typeof(int));
            cartTable.Columns.Add("Товар", typeof(string));
            cartTable.Columns.Add("Количество", typeof(int));
            cartTable.Columns.Add("Цена", typeof(decimal));
            cartTable.Columns.Add("Сумма", typeof(decimal));
            InitializeUI();
        }

        private void InitializeUI()
        {
            var lblTitle = new Label
            {
                Text = "🛒  Оформление продажи",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 35, 50),
                AutoSize = true, Location = new Point(0, 0)
            };

            // Left panel - catalog
            var pnlCatalog = new Panel { Location = new Point(0, 50), Size = new Size(350, 520), BorderStyle = BorderStyle.FixedSingle };
            var lblCatalog = new Label
            {
                Text = "Каталог товаров",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(5, 5), AutoSize = true
            };
            var dgvCatalog = new DataGridView
            {
                Location = new Point(0, 30), Size = new Size(348, 490),
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false, BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 9)
            };
            dgvCatalog.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(33, 150, 243);
            dgvCatalog.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCatalog.EnableHeadersVisualStyles = false;

            var dtCatalog = DbHelper.ExecuteQuery(
                "SELECT т.id, т.название as [Товар], рц.розничнаяЦена as [Цена], ISNULL(с.количество,0) as [Остаток] FROM Товары т LEFT JOIN РозничныеЦены рц ON т.id=рц.товарId LEFT JOIN Склад с ON т.id=с.товарId ORDER BY т.название");
            dgvCatalog.DataSource = dtCatalog;
            if (dgvCatalog.Columns.Contains("id")) dgvCatalog.Columns["id"].Visible = false;

            pnlCatalog.Controls.Add(lblCatalog);
            pnlCatalog.Controls.Add(dgvCatalog);

            // Right panel - cart
            var pnlCart = new Panel { Location = new Point(365, 50), Size = new Size(580, 520), BorderStyle = BorderStyle.FixedSingle };
            var lblCart = new Label
            {
                Text = "Корзина",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(5, 5), AutoSize = true
            };

            var lblKassaL = new Label { Text = "Касса:", Location = new Point(300, 5), AutoSize = true };
            cmbKassa = new ComboBox { Location = new Point(350, 2), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            var dtKassy = DbHelper.ExecuteQuery("SELECT id, номер FROM Кассы WHERE статус='Открыта'");
            cmbKassa.DataSource = dtKassy;
            cmbKassa.DisplayMember = "номер";
            cmbKassa.ValueMember = "id";

            dgvCart = new DataGridView
            {
                Location = new Point(0, 30), Size = new Size(578, 360),
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false, BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 9),
                DataSource = cartTable
            };
            dgvCart.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(76, 175, 80);
            dgvCart.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCart.EnableHeadersVisualStyles = false;
            if (dgvCart.Columns.Contains("ТоварId")) dgvCart.Columns["ТоварId"].Visible = false;

            lblTotal = new Label
            {
                Text = "ИТОГО: 0.00 руб.",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 150, 243),
                Location = new Point(5, 400), AutoSize = true
            };

            var btnAddToCart = new Button
            {
                Text = "+ Добавить",
                Location = new Point(5, 440), Size = new Size(130, 36),
                BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnAddToCart.FlatAppearance.BorderSize = 0;
            btnAddToCart.Click += (s, e) =>
            {
                if (dgvCatalog.SelectedRows.Count == 0) return;
                var row = dgvCatalog.SelectedRows[0];
                int tovarId = (int)dtCatalog.Rows[row.Index]["id"];
                string name = row.Cells["Товар"].Value?.ToString();
                decimal price = Convert.ToDecimal(row.Cells["Цена"].Value ?? 0);
                cartTable.Rows.Add(tovarId, name, 1, price, price);
                UpdateTotal();
            };

            var btnRemove = new Button
            {
                Text = "✕ Удалить",
                Location = new Point(145, 440), Size = new Size(130, 36),
                BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Click += (s, e) =>
            {
                if (dgvCart.SelectedRows.Count == 0) return;
                cartTable.Rows.RemoveAt(dgvCart.SelectedRows[0].Index);
                UpdateTotal();
            };

            var btnCheckout = new Button
            {
                Text = "💳 Оформить чек",
                Location = new Point(285, 440), Size = new Size(170, 36),
                BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnCheckout.FlatAppearance.BorderSize = 0;
            btnCheckout.Click += (s, e) => Checkout();

            pnlCart.Controls.AddRange(new Control[] { lblCart, lblKassaL, cmbKassa, dgvCart, lblTotal, btnAddToCart, btnRemove, btnCheckout });

            this.Controls.AddRange(new Control[] { lblTitle, pnlCatalog, pnlCart });
        }

        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (DataRow r in cartTable.Rows)
                total += Convert.ToDecimal(r["Сумма"]);
            lblTotal.Text = $"ИТОГО: {total:F2} руб.";
        }

        private void Checkout()
        {
            if (cartTable.Rows.Count == 0) { MessageBox.Show("Корзина пуста!"); return; }
            if (cmbKassa.SelectedValue == null) { MessageBox.Show("Нет открытых касс!"); return; }

            decimal total = 0;
            foreach (DataRow r in cartTable.Rows)
                total += Convert.ToDecimal(r["Сумма"]);

            int kassaId = (int)cmbKassa.SelectedValue;

            // Create check
            var чекId = DbHelper.ExecuteScalar(
                "INSERT INTO Чеки(сумма,итого,кассаId) VALUES(@s,@i,@k); SELECT SCOPE_IDENTITY()",
                new[] {
                    new SqlParameter("@s", total),
                    new SqlParameter("@i", total),
                    new SqlParameter("@k", kassaId)
                });

            int чек = Convert.ToInt32(чекId);

            foreach (DataRow r in cartTable.Rows)
            {
                int tovarId = (int)r["ТоварId"];
                int kol = (int)r["Количество"];
                decimal cena = Convert.ToDecimal(r["Цена"]);

                DbHelper.ExecuteNonQuery(
                    "INSERT INTO СтрокиЧека(чекId,товарId,количество,цена) VALUES(@c,@t,@k,@p)",
                    new[] { new SqlParameter("@c", чек), new SqlParameter("@t", tovarId), new SqlParameter("@k", kol), new SqlParameter("@p", cena) });

                // Reduce stock
                DbHelper.ExecuteNonQuery("UPDATE Склад SET количество=количество-@k WHERE товарId=@t",
                    new[] { new SqlParameter("@k", kol), new SqlParameter("@t", tovarId) });
            }

            cartTable.Rows.Clear();
            UpdateTotal();
            MessageBox.Show($"✅ Чек №{чек} оформлен!\nСумма: {total:F2} руб.", "Продажа завершена", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    // ── Кассовые отчёты ───────────────────────────────────────────────────────
    public class KassovyeOtchety : Form
    {
        private DataGridView dgv;

        public KassovyeOtchety()
        {
            this.Text = "Кассовые отчёты";
            this.BackColor = Color.FromArgb(245, 245, 248);

            var lblTitle = new Label
            {
                Text = "📊  Кассовые отчёты",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 35, 50),
                AutoSize = true, Location = new Point(0, 0)
            };

            var btnForm = new Button
            {
                Text = "+ Сформировать отчёт",
                Location = new Point(0, 42), Size = new Size(180, 32),
                BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnForm.FlatAppearance.BorderSize = 0;
            btnForm.Click += (s, e) => FormOtchet();

            var lblSummary = new Label { Location = new Point(200, 50), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(33, 150, 243) };

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

            this.Controls.AddRange(new Control[] { lblTitle, btnForm, lblSummary, dgv });
            LoadData(lblSummary);
        }

        private void LoadData(Label lblSummary)
        {
            var sql = @"SELECT о.id, к.номер as [Касса], о.дата as [Дата],
                        о.итоговаяСумма as [Итоговая сумма], о.количествоЧеков as [Чеков],
                        сотр.ФИО as [Кассир]
                        FROM КассовыеОтчеты о
                        JOIN Кассы к ON о.кассаId=к.id
                        JOIN Сотрудники сотр ON о.старшийКассирId=сотр.id
                        ORDER BY о.дата DESC";
            dgv.DataSource = DbHelper.ExecuteQuery(sql);
            if (dgv.Columns.Contains("id")) dgv.Columns["id"].Visible = false;

            // Today's totals
            var today = DbHelper.ExecuteScalar("SELECT ISNULL(SUM(итого),0) FROM Чеки WHERE CAST(дата AS DATE)=CAST(GETDATE() AS DATE)");
            lblSummary.Text = $"Выручка сегодня: {Convert.ToDecimal(today ?? 0):F2} руб.";
        }

        private void FormOtchet()
        {
            var dtKassy = DbHelper.ExecuteQuery("SELECT id, номер FROM Кассы");
            var dlg = new Form
            {
                Text = "Сформировать кассовый отчёт",
                Size = new Size(340, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };

            var cmbKassa = new ComboBox { Location = new Point(120, 20), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbKassa.DataSource = dtKassy;
            cmbKassa.DisplayMember = "номер";
            cmbKassa.ValueMember = "id";

            var dtpDate = new DateTimePicker { Location = new Point(120, 60), Width = 180, Format = DateTimePickerFormat.Short, Value = DateTime.Today };

            dlg.Controls.Add(new Label { Text = "Касса:", Location = new Point(15, 23), AutoSize = true });
            dlg.Controls.Add(cmbKassa);
            dlg.Controls.Add(new Label { Text = "Дата:", Location = new Point(15, 63), AutoSize = true });
            dlg.Controls.Add(dtpDate);

            var btnOk = new Button
            {
                Text = "Сформировать", Location = new Point(80, 110), Size = new Size(150, 34),
                BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) =>
            {
                if (cmbKassa.SelectedValue == null) return;
                int kassaId = (int)cmbKassa.SelectedValue;
                DateTime date = dtpDate.Value.Date;

                var statsObj = DbHelper.ExecuteQuery(
                    "SELECT COUNT(*) as cnt, ISNULL(SUM(итого),0) as summa FROM Чеки WHERE кассаId=@k AND CAST(дата AS DATE)=@d",
                    new[] { new SqlParameter("@k", kassaId), new SqlParameter("@d", date) });

                int cnt = Convert.ToInt32(statsObj.Rows[0]["cnt"]);
                decimal summa = Convert.ToDecimal(statsObj.Rows[0]["summa"]);

                DbHelper.ExecuteNonQuery(
                    "INSERT INTO КассовыеОтчеты(кассаId,старшийКассирId,дата,итоговаяСумма,количествоЧеков) VALUES(@k,@s,@d,@i,@c)",
                    new[] {
                        new SqlParameter("@k", kassaId),
                        new SqlParameter("@s", Session.UserId),
                        new SqlParameter("@d", date),
                        new SqlParameter("@i", summa),
                        new SqlParameter("@c", cnt)
                    });

                MessageBox.Show($"✅ Отчёт сформирован!\nЧеков: {cnt}\nСумма: {summa:F2} руб.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData(new Label());
            };
            dlg.Controls.Add(btnOk);
            dlg.ShowDialog(this);
        }
    }

    // ── Инвентаризация ────────────────────────────────────────────────────────
    public class InventarizaciyaForm : Form
    {
        private DataGridView dgvSessions, dgvRows;
        private int selectedInvId = -1;

        public InventarizaciyaForm()
        {
            this.Text = "Инвентаризация";
            this.BackColor = Color.FromArgb(245, 245, 248);
            InitializeUI();
        }

        private void InitializeUI()
        {
            var lblTitle = new Label
            {
                Text = "📝  Инвентаризация",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 35, 50),
                AutoSize = true, Location = new Point(0, 0)
            };

            var btnNew = new Button
            {
                Text = "+ Начать инвентаризацию",
                Location = new Point(0, 42), Size = new Size(200, 32),
                BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnNew.FlatAppearance.BorderSize = 0;
            btnNew.Click += (s, e) => StartNew();

            var btnComplete = new Button
            {
                Text = "✔ Завершить",
                Location = new Point(210, 42), Size = new Size(120, 32),
                BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnComplete.FlatAppearance.BorderSize = 0;
            btnComplete.Click += (s, e) =>
            {
                if (selectedInvId < 0) return;
                DbHelper.ExecuteNonQuery("UPDATE Инвентаризация SET статус='Завершена' WHERE id=@id",
                    new[] { new SqlParameter("@id", selectedInvId) });
                LoadSessions();
            };

            // Top grid - sessions
            dgvSessions = CreateGrid(Color.FromArgb(33, 150, 243));
            dgvSessions.Location = new Point(0, 85);
            dgvSessions.Size = new Size(950, 180);
            dgvSessions.SelectionChanged += (s, e) =>
            {
                if (dgvSessions.SelectedRows.Count == 0) return;
                selectedInvId = (int)dgvSessions.SelectedRows[0].Cells["id"].Value;
                LoadRows(selectedInvId);
            };

            var lblRows = new Label
            {
                Text = "Строки инвентаризации (сверка остатков):",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(0, 275), AutoSize = true
            };

            // Bottom grid - rows
            dgvRows = CreateGrid(Color.FromArgb(156, 39, 176));
            dgvRows.Location = new Point(0, 300);
            dgvRows.Size = new Size(950, 280);

            var btnAddRow = new Button
            {
                Text = "+ Добавить строку",
                Location = new Point(0, 590), Size = new Size(160, 32),
                BackColor = Color.FromArgb(156, 39, 176), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnAddRow.FlatAppearance.BorderSize = 0;
            btnAddRow.Click += (s, e) => AddRow();

            this.Controls.AddRange(new Control[] { lblTitle, btnNew, btnComplete, dgvSessions, lblRows, dgvRows, btnAddRow });
            LoadSessions();
        }

        private void LoadSessions()
        {
            var sql = "SELECT id, дата as [Дата], ответственный as [Ответственный], статус as [Статус] FROM Инвентаризация ORDER BY дата DESC";
            dgvSessions.DataSource = DbHelper.ExecuteQuery(sql);
            if (dgvSessions.Columns.Contains("id")) dgvSessions.Columns["id"].Visible = false;
        }

        private void LoadRows(int invId)
        {
            var sql = @"SELECT т.название as [Товар], си.базаОстаток as [По базе],
                        си.фактОстаток as [Факт], (си.фактОстаток-си.базаОстаток) as [Расхождение]
                        FROM СтрокиИнвентаризации си JOIN Товары т ON си.товарId=т.id
                        WHERE си.инвентаризацияId=@id";
            dgvRows.DataSource = DbHelper.ExecuteQuery(sql, new[] { new SqlParameter("@id", invId) });
        }

        private void StartNew()
        {
            DbHelper.ExecuteNonQuery("INSERT INTO Инвентаризация(ответственный,статус) VALUES(@r,'В процессе')",
                new[] { new SqlParameter("@r", Session.UserFIO) });
            LoadSessions();
        }

        private void AddRow()
        {
            if (selectedInvId < 0) { MessageBox.Show("Выберите инвентаризацию!"); return; }

            var dlg = new Form
            {
                Text = "Добавить строку",
                Size = new Size(360, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };

            var dtTovary = DbHelper.ExecuteQuery("SELECT т.id, т.название, ISNULL(с.количество,0) as остаток FROM Товары т LEFT JOIN Склад с ON т.id=с.товарId");
            var cmbTovar = new ComboBox { Location = new Point(140, 20), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTovar.DataSource = dtTovary;
            cmbTovar.DisplayMember = "название";
            cmbTovar.ValueMember = "id";

            var numFact = new NumericUpDown { Location = new Point(140, 60), Width = 100, Minimum = 0, Maximum = 999999, Value = 0 };

            dlg.Controls.Add(new Label { Text = "Товар:", Location = new Point(15, 23), AutoSize = true });
            dlg.Controls.Add(cmbTovar);
            dlg.Controls.Add(new Label { Text = "Факт. остаток:", Location = new Point(15, 63), AutoSize = true });
            dlg.Controls.Add(numFact);

            var btnOk = new Button
            {
                Text = "Добавить", Location = new Point(140, 110), Size = new Size(110, 34),
                BackColor = Color.FromArgb(156, 39, 176), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) =>
            {
                if (cmbTovar.SelectedValue == null) return;
                int tovarId = (int)cmbTovar.SelectedValue;

                // Get base stock from DB
                var baseStock = DbHelper.ExecuteScalar("SELECT ISNULL(количество,0) FROM Склад WHERE товарId=@t",
                    new[] { new SqlParameter("@t", tovarId) });
                int baza = Convert.ToInt32(baseStock ?? 0);

                DbHelper.ExecuteNonQuery(
                    "INSERT INTO СтрокиИнвентаризации(инвентаризацияId,товарId,фактОстаток,базаОстаток) VALUES(@i,@t,@f,@b)",
                    new[] {
                        new SqlParameter("@i", selectedInvId),
                        new SqlParameter("@t", tovarId),
                        new SqlParameter("@f", (int)numFact.Value),
                        new SqlParameter("@b", baza)
                    });
                LoadRows(selectedInvId);
            };
            dlg.Controls.Add(btnOk);
            dlg.ShowDialog(this);
        }

        private DataGridView CreateGrid(Color headerColor)
        {
            var dgv = new DataGridView
            {
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false, BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10),
                GridColor = Color.FromArgb(230, 230, 230)
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = headerColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 255);
            return dgv;
        }
    }
}
