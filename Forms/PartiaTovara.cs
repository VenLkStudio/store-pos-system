using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using RetailShop.Database;
using RetailShop.Models;

namespace RetailShop.Forms
{
    public class PartiaTovara : Form
    {
        private TabControl tabs;
        private DataGridView dgvPartii, dgvStroki, dgvDocs;
        private Button btnAdd, btnRefresh, btnSend, btnAddDoc;
        private ComboBox cmbPostavshik;
        private int selectedPartiyaId = -1;

        public PartiaTovara()
        {
            InitializeComponent();
            LoadPartii();
        }

        private void InitializeComponent()
        {
            this.Text = "Приём партии товара";
            this.BackColor = Color.FromArgb(245, 245, 248);

            var lblTitle = new Label
            {
                Text = "📦  Приём партии товара",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 35, 50),
                AutoSize = true,
                Location = new Point(0, 0)
            };

            // Toolbar
            var toolbar = new Panel { Location = new Point(0, 40), Height = 44, Width = 900 };

            var lblPostavshik = new Label { Text = "Поставщик:", Location = new Point(0, 12), AutoSize = true };
            cmbPostavshik = new ComboBox
            {
                Location = new Point(90, 8),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };

            btnAdd = new Button
            {
                Text = "+ Создать партию",
                Location = new Point(325, 6),
                Size = new Size(150, 32),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            btnRefresh = new Button
            {
                Text = "↻ Обновить",
                Location = new Point(485, 6),
                Size = new Size(110, 32),
                BackColor = Color.FromArgb(240, 240, 240),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadPartii();

            toolbar.Controls.AddRange(new Control[] { lblPostavshik, cmbPostavshik, btnAdd, btnRefresh });

            // Tabs
            tabs = new TabControl
            {
                Location = new Point(0, 95),
                Size = new Size(950, 550),
                Font = new Font("Segoe UI", 10)
            };

            var tabPartii = new TabPage("Партии товара");
            var tabStroki = new TabPage("Состав партии");
            var tabDocs = new TabPage("Документы");

            // ── Tab 1: Партии ──────────────────────────────────────────────────
            dgvPartii = CreateGrid();
            dgvPartii.Dock = DockStyle.Fill;
            dgvPartii.SelectionChanged += DgvPartii_SelectionChanged;

            btnSend = new Button
            {
                Text = "📤 Отправить на склад",
                Dock = DockStyle.Bottom,
                Height = 36,
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += BtnSend_Click;

            tabPartii.Controls.Add(dgvPartii);
            tabPartii.Controls.Add(btnSend);

            // ── Tab 2: Строки ──────────────────────────────────────────────────
            var panelStroki = new Panel { Dock = DockStyle.Fill };
            dgvStroki = CreateGrid();
            dgvStroki.Dock = DockStyle.Fill;

            var panelAddStroka = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(250, 250, 250) };
            var cmbTovar = new ComboBox { Location = new Point(5, 12), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            var numKol = new NumericUpDown { Location = new Point(215, 12), Width = 80, Minimum = 1, Maximum = 10000, Value = 1 };
            var numCena = new NumericUpDown { Location = new Point(305, 12), Width = 100, Minimum = 0, Maximum = 999999, DecimalPlaces = 2, Value = 0 };
            var btnAddStroka = new Button
            {
                Text = "+ Добавить",
                Location = new Point(415, 9),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAddStroka.FlatAppearance.BorderSize = 0;
            btnAddStroka.Click += (s, e) =>
            {
                if (selectedPartiyaId < 0) { MessageBox.Show("Выберите партию!"); return; }
                if (cmbTovar.SelectedValue == null) { MessageBox.Show("Выберите товар!"); return; }
                DbHelper.ExecuteNonQuery(
                    "INSERT INTO СтрокиПартии(партияId,товарId,количество,цена) VALUES(@p,@t,@k,@c)",
                    new[] {
                        new SqlParameter("@p", selectedPartiyaId),
                        new SqlParameter("@t", cmbTovar.SelectedValue),
                        new SqlParameter("@k", (int)numKol.Value),
                        new SqlParameter("@c", numCena.Value)
                    });
                LoadStroki(selectedPartiyaId);
            };

            panelAddStroka.Controls.AddRange(new Control[] {
                new Label { Text = "Товар:", Location = new Point(5, -2), AutoSize = true },
                cmbTovar,
                new Label { Text = "Кол-во:", Location = new Point(215, -2), AutoSize = true },
                numKol,
                new Label { Text = "Цена:", Location = new Point(305, -2), AutoSize = true },
                numCena,
                btnAddStroka
            });

            panelStroki.Controls.Add(dgvStroki);
            panelStroki.Controls.Add(panelAddStroka);
            tabStroki.Controls.Add(panelStroki);

            LoadComboBoxes(cmbTovar);

            // ── Tab 3: Документы ───────────────────────────────────────────────
            var panelDocs = new Panel { Dock = DockStyle.Fill };
            dgvDocs = CreateGrid();
            dgvDocs.Dock = DockStyle.Fill;

            btnAddDoc = new Button
            {
                Text = "+ Добавить документ",
                Dock = DockStyle.Bottom,
                Height = 36,
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnAddDoc.FlatAppearance.BorderSize = 0;
            btnAddDoc.Click += BtnAddDoc_Click;

            panelDocs.Controls.Add(dgvDocs);
            panelDocs.Controls.Add(btnAddDoc);
            tabDocs.Controls.Add(panelDocs);

            tabs.TabPages.AddRange(new[] { tabPartii, tabStroki, tabDocs });

            this.Controls.AddRange(new Control[] { lblTitle, toolbar, tabs });

            LoadPostavshikiCombo();
        }

        private DataGridView CreateGrid()
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

        private void LoadPostavshikiCombo()
        {
            var dt = DbHelper.ExecuteQuery("SELECT id, название FROM Поставщики");
            cmbPostavshik.DataSource = dt;
            cmbPostavshik.DisplayMember = "название";
            cmbPostavshik.ValueMember = "id";
        }

        private void LoadComboBoxes(ComboBox cmbTovar)
        {
            var dt = DbHelper.ExecuteQuery("SELECT id, название FROM Товары");
            cmbTovar.DataSource = dt;
            cmbTovar.DisplayMember = "название";
            cmbTovar.ValueMember = "id";
        }

        private void LoadPartii()
        {
            var sql = @"SELECT п.id, п.датаПоступления as [Дата], пост.название as [Поставщик], 
                        сотр.ФИО as [Оператор], п.статус as [Статус]
                        FROM ПартииТовара п
                        JOIN Поставщики пост ON п.поставщикId=пост.id
                        JOIN Сотрудники сотр ON п.операторId=сотр.id
                        ORDER BY п.датаПоступления DESC";
            dgvPartii.DataSource = DbHelper.ExecuteQuery(sql);
        }

        private void LoadStroki(int партияId)
        {
            var sql = @"SELECT т.название as [Товар], сп.количество as [Количество], 
                        сп.цена as [Цена], (сп.количество*сп.цена) as [Сумма]
                        FROM СтрокиПартии сп JOIN Товары т ON сп.товарId=т.id
                        WHERE сп.партияId=@id";
            dgvStroki.DataSource = DbHelper.ExecuteQuery(sql, new[] { new SqlParameter("@id", партияId) });
        }

        private void LoadDocs(int партияId)
        {
            var sql = @"SELECT тип as [Тип], номер as [Номер], дата as [Дата], 
                        сумма as [Сумма], CASE WHEN проверен=1 THEN 'Да' ELSE 'Нет' END as [Проверен]
                        FROM Документы WHERE партияId=@id";
            dgvDocs.DataSource = DbHelper.ExecuteQuery(sql, new[] { new SqlParameter("@id", партияId) });
        }

        private void DgvPartii_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPartii.SelectedRows.Count == 0) return;
            selectedPartiyaId = (int)dgvPartii.SelectedRows[0].Cells["id"].Value;
            LoadStroki(selectedPartiyaId);
            LoadDocs(selectedPartiyaId);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (cmbPostavshik.SelectedValue == null) { MessageBox.Show("Выберите поставщика!"); return; }
            DbHelper.ExecuteNonQuery(
                "INSERT INTO ПартииТовара(поставщикId,операторId) VALUES(@p,@o)",
                new[] {
                    new SqlParameter("@p", cmbPostavshik.SelectedValue),
                    new SqlParameter("@o", Session.UserId)
                });
            LoadPartii();
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            if (selectedPartiyaId < 0) { MessageBox.Show("Выберите партию!"); return; }

            // Check that all 3 document types exist
            var docsCount = DbHelper.ExecuteScalar(
                "SELECT COUNT(DISTINCT тип) FROM Документы WHERE партияId=@id",
                new[] { new SqlParameter("@id", selectedPartiyaId) });

            if (Convert.ToInt32(docsCount) < 3)
            {
                MessageBox.Show("Необходимо добавить все 3 документа (Накладная, СчетФактура, СертификатКачества) перед отправкой на склад!",
                    "Проверка документов", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Update warehouse quantities
            var stroki = DbHelper.ExecuteQuery(
                "SELECT товарId, количество FROM СтрокиПартии WHERE партияId=@id",
                new[] { new SqlParameter("@id", selectedPartiyaId) });

            foreach (DataRow row in stroki.Rows)
            {
                int товарId = (int)row["товарId"];
                int кол = (int)row["количество"];

                var exists = DbHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Склад WHERE товарId=@t",
                    new[] { new SqlParameter("@t", товарId) });

                if (Convert.ToInt32(exists) > 0)
                    DbHelper.ExecuteNonQuery(
                        "UPDATE Склад SET количество=количество+@k WHERE товарId=@t",
                        new[] { new SqlParameter("@k", кол), new SqlParameter("@t", товарId) });
                else
                    DbHelper.ExecuteNonQuery(
                        "INSERT INTO Склад(товарId,количество) VALUES(@t,@k)",
                        new[] { new SqlParameter("@t", товарId), new SqlParameter("@k", кол) });
            }

            DbHelper.ExecuteNonQuery(
                "UPDATE ПартииТовара SET статус='Принята' WHERE id=@id",
                new[] { new SqlParameter("@id", selectedPartiyaId) });

            MessageBox.Show("✅ Партия успешно отправлена на склад!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadPartii();
        }

        private void BtnAddDoc_Click(object sender, EventArgs e)
        {
            if (selectedPartiyaId < 0) { MessageBox.Show("Выберите партию!"); return; }

            var dlg = new AddDocForm(selectedPartiyaId);
            if (dlg.ShowDialog() == DialogResult.OK)
                LoadDocs(selectedPartiyaId);
        }
    }

    // ─── Add Document Dialog ─────────────────────────────────────────────────
    public class AddDocForm : Form
    {
        private int партияId;
        public AddDocForm(int partId)
        {
            партияId = partId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Добавить документ";
            this.Size = new Size(380, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            int y = 20;
            void AddRow(string label, Control ctrl)
            {
                this.Controls.Add(new Label { Text = label, Location = new Point(20, y), AutoSize = true });
                ctrl.Location = new Point(120, y - 3);
                ctrl.Width = 220;
                this.Controls.Add(ctrl);
                y += 36;
            }

            var cmbTip = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTip.Items.AddRange(new[] { "Накладная", "СчетФактура", "СертификатКачества" });
            cmbTip.SelectedIndex = 0;

            var txtNomer = new TextBox();
            var dtpDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            var numSumma = new NumericUpDown { Minimum = 0, Maximum = 9999999, DecimalPlaces = 2 };

            AddRow("Тип:", cmbTip);
            AddRow("Номер:", txtNomer);
            AddRow("Дата:", dtpDate);
            AddRow("Сумма:", numSumma);

            var btnOk = new Button
            {
                Text = "Сохранить", DialogResult = DialogResult.OK,
                Location = new Point(120, y), Size = new Size(110, 34),
                BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, ev) =>
            {
                DbHelper.ExecuteNonQuery(
                    "INSERT INTO Документы(партияId,тип,номер,дата,сумма,проверен) VALUES(@p,@t,@n,@d,@s,1)",
                    new[] {
                        new SqlParameter("@p", партияId),
                        new SqlParameter("@t", cmbTip.SelectedItem),
                        new SqlParameter("@n", txtNomer.Text),
                        new SqlParameter("@d", dtpDate.Value.Date),
                        new SqlParameter("@s", numSumma.Value)
                    });
            };

            var btnCancel = new Button
            {
                Text = "Отмена", DialogResult = DialogResult.Cancel,
                Location = new Point(240, y), Size = new Size(100, 34),
                FlatStyle = FlatStyle.Flat
            };

            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);
        }
    }
}
