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
    //  Экран 11 – Реализация товаров (касса)
    //  Wireframe: левая панель – каталог товаров, правая – корзина/чек,
    //             снизу – итого + кнопка «Оформить чек»
    // ═══════════════════════════════════════════════════════════════════════
    public class RealizaciyaForm : Form
    {
        private DataGridView _dgvCatalog, _dgvCart;
        private DataTable    _cartDt;
        private Label        _lblTotal;
        private Win11Combo   _cmbKassa;
        private Win11Field   _txtSearch;

        public RealizaciyaForm() { Build(); LoadCatalog(""); }

        private void Build()
        {
            BackColor = Clr.BgApp;

            var title = UI.MakeTitle("Реализация товаров");
            title.Location = new Point(0, 0);

            // ── Касса picker ──────────────────────────────────────────────────
            var lblK = UI.MakeLabel("Касса:", true); lblK.Location = new Point(0, 38);
            _cmbKassa = UI.MakeCombo(220); _cmbKassa.Location = new Point(56, 34);

            var dtK = DB.Query("SELECT id, номер FROM Кассы WHERE статус='Открыта'");
            if (dtK.Rows.Count == 0)
            {
                // show all if none open
                dtK = DB.Query("SELECT id, номер FROM Кассы");
            }
            _cmbKassa.DataSource    = dtK;
            _cmbKassa.DisplayMember = "номер";
            _cmbKassa.ValueMember   = "id";

            // ── LEFT: catalogue ───────────────────────────────────────────────
            var pnlLeft = new Panel
            {
                Location  = new Point(0, 70),
                Size      = new Size(430, 550),
                BackColor = Clr.BgWhite,
            };
            pnlLeft.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(Clr.Border), 0, 0, pnlLeft.Width - 1, pnlLeft.Height - 1);

            var lblCat = UI.MakeLabel("Каталог товаров", true);
            lblCat.Location = new Point(8, 8);

            _txtSearch = UI.MakeField(200, "Поиск...");
            _txtSearch.Location     = new Point(8, 32);
            _txtSearch.TextChanged += (s, e) =>
            {
                string v = _txtSearch.Text == "Поиск..." ? "" : _txtSearch.Text;
                LoadCatalog(v);
            };

            _dgvCatalog = UI.MakeGrid();
            _dgvCatalog.Location = new Point(0, 68);
            _dgvCatalog.Size     = new Size(430, 440);
            _dgvCatalog.Dock     = DockStyle.None;

            var btnAddToCart = UI.MakeBtn("→ В корзину", 420, 34);
            btnAddToCart.Location = new Point(0, 510);
            btnAddToCart.Click   += AddToCart;

            pnlLeft.Controls.AddRange(new Control[] { lblCat, _txtSearch, _dgvCatalog, btnAddToCart });

            // ── RIGHT: cart ───────────────────────────────────────────────────
            var pnlRight = new Panel
            {
                Location  = new Point(444, 70),
                Size      = new Size(500, 550),
                BackColor = Clr.BgWhite,
            };
            pnlRight.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(Clr.Border), 0, 0, pnlRight.Width - 1, pnlRight.Height - 1);

            var lblCart = UI.MakeLabel("Корзина", true);
            lblCart.Location = new Point(8, 8);

            _cartDt = new DataTable();
            _cartDt.Columns.Add("ТоварId",    typeof(int));
            _cartDt.Columns.Add("Товар",      typeof(string));
            _cartDt.Columns.Add("Кол-во",     typeof(int));
            _cartDt.Columns.Add("Цена, ₽",   typeof(decimal));
            _cartDt.Columns.Add("Сумма, ₽",  typeof(decimal));

            _dgvCart = UI.MakeGrid();
            _dgvCart.Location   = new Point(0, 32);
            _dgvCart.Size       = new Size(500, 390);
            _dgvCart.DataSource = _cartDt;

            var btnDel = UI.MakeBtnOutline("✕ Удалить строку", 180, 30);
            btnDel.Location  = new Point(8, 430);
            btnDel.ForeColor = System.Drawing.Color.FromArgb(140, 40, 40);
            btnDel.Click    += (s, e) =>
            {
                if (_dgvCart.SelectedRows.Count == 0) return;
                _cartDt.Rows.RemoveAt(_dgvCart.SelectedRows[0].Index);
                UpdateTotal();
            };

            var btnClear = UI.MakeBtnOutline("Очистить", 100, 30);
            btnClear.Location = new Point(196, 430);
            btnClear.Click   += (s, e) => { _cartDt.Rows.Clear(); UpdateTotal(); };

            _lblTotal = new Label
            {
                Text      = "ИТОГО:  0.00 ₽",
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Clr.TextPrimary,
                Location  = new Point(8, 470),
                AutoSize  = true,
            };

            var btnCheckout = UI.MakeBtn("💳  Оформить чек", 490, 40);
            btnCheckout.Location = new Point(0, 504);
            btnCheckout.Font     = new Font("Segoe UI", 11f, FontStyle.Bold);
            btnCheckout.Click   += Checkout;

            pnlRight.Controls.AddRange(new Control[]
            {
                lblCart, _dgvCart, btnDel, btnClear, _lblTotal, btnCheckout
            });

            Controls.AddRange(new Control[] { title, lblK, _cmbKassa, pnlLeft, pnlRight });

            // hide internal id column after grid is populated
            _dgvCart.DataBindingComplete += (s, e) => UI.HideCols(_dgvCart, "ТоварId");
        }

        // ── Data ──────────────────────────────────────────────────────────────
        private void LoadCatalog(string q)
        {
            var dt = DB.Query(@"
                SELECT т.id,
                       т.название            AS [Товар],
                       т.единицаИзмерения    AS [Ед],
                       ISNULL(рц.розничнаяЦена,0) AS [Цена, ₽],
                       ISNULL(с.количество,0)      AS [Остаток]
                FROM Товары т
                LEFT JOIN РозничныеЦены рц ON рц.товарId=т.id
                LEFT JOIN Склад с           ON с.товарId=т.id
                WHERE т.название LIKE @q
                ORDER BY т.название",
                DB.P("@q", "%" + q + "%"));
            _dgvCatalog.DataSource = dt;
            UI.HideCols(_dgvCatalog, "id");
        }

        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (DataRow r in _cartDt.Rows)
                total += Convert.ToDecimal(r["Сумма, ₽"]);
            _lblTotal.Text = $"ИТОГО:  {total:N2} ₽";
        }

        // ── Actions ───────────────────────────────────────────────────────────
        private void AddToCart(object s, EventArgs e)
        {
            if (_dgvCatalog.SelectedRows.Count == 0) return;
            var row = _dgvCatalog.SelectedRows[0];

            // find underlying DataTable row to get id
            var src = ((DataTable)_dgvCatalog.DataSource);
            int idx = _dgvCatalog.SelectedRows[0].Index;
            int tovarId = (int)src.Rows[idx]["id"];
            string name  = row.Cells["Товар"].Value?.ToString();
            decimal cena = Convert.ToDecimal(row.Cells["Цена, ₽"].Value ?? 0);
            int stock    = Convert.ToInt32(row.Cells["Остаток"].Value ?? 0);

            if (stock <= 0) { MessageBox.Show("Товар отсутствует на складе.", "Недостаточно товара", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            // check already in cart
            foreach (DataRow cr in _cartDt.Rows)
            {
                if ((int)cr["ТоварId"] == tovarId)
                {
                    cr["Кол-во"]    = (int)cr["Кол-во"] + 1;
                    cr["Сумма, ₽"] = (int)cr["Кол-во"] * (decimal)cr["Цена, ₽"];
                    UpdateTotal();
                    return;
                }
            }

            _cartDt.Rows.Add(tovarId, name, 1, cena, cena);
            UpdateTotal();
        }

        private void Checkout(object s, EventArgs e)
        {
            if (_cartDt.Rows.Count == 0) { MessageBox.Show("Корзина пуста.", "Пустой чек"); return; }
            if (_cmbKassa.SelectedValue == null) { MessageBox.Show("Нет доступных касс.", "Ошибка"); return; }

            decimal итого = 0;
            foreach (DataRow r in _cartDt.Rows)
                итого += Convert.ToDecimal(r["Сумма, ₽"]);

            int кассаId = (int)_cmbKassa.SelectedValue;

            // Create check
            var чекIdObj = DB.Scalar(
                "INSERT INTO Чеки(кассаId,итого) VALUES(@k,@i); SELECT SCOPE_IDENTITY()",
                DB.P("@k", кассаId), DB.P("@i", итого));
            int чекId = Convert.ToInt32(чекIdObj);

            foreach (DataRow r in _cartDt.Rows)
            {
                int tid  = (int)r["ТоварId"];
                int kol  = (int)r["Кол-во"];
                decimal цена = Convert.ToDecimal(r["Цена, ₽"]);

                DB.Exec("INSERT INTO СтрокиЧека(чекId,товарId,количество,цена) VALUES(@c,@t,@k,@p)",
                    DB.P("@c", чекId), DB.P("@t", tid), DB.P("@k", kol), DB.P("@p", цена));

                // reduce stock
                DB.Exec("UPDATE Склад SET количество=CASE WHEN количество>@k THEN количество-@k ELSE 0 END WHERE товарId=@t",
                    DB.P("@k", kol), DB.P("@t", tid));
            }

            _cartDt.Rows.Clear();
            UpdateTotal();
            LoadCatalog("");

            MessageBox.Show(
                $"Чек №{чекId} оформлен!\nСумма: {итого:N2} ₽",
                "Продажа завершена", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
