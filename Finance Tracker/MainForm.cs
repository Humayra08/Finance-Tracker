using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Finance_Tracker
{
    public partial class MainForm : Form
    {
        // ---- In-memory data store ----
        private System.Collections.Generic.List<Transaction> transactions
            = new System.Collections.Generic.List<Transaction>();

        // ---- Palette ----
        private readonly Color ColorGreenBg = Color.FromArgb(220, 252, 231);
        private readonly Color ColorGreenText = Color.FromArgb(22, 163, 74);
        private readonly Color ColorRedBg = Color.FromArgb(254, 226, 226);
        private readonly Color ColorRedText = Color.FromArgb(220, 38, 38);
        private readonly Color ColorBlueBg = Color.FromArgb(219, 234, 254);
        private readonly Color ColorBlueText = Color.FromArgb(37, 99, 235);
        private readonly Color ColorPageBg = Color.White;
        private readonly Color ColorBorder = Color.FromArgb(229, 231, 235);
        private readonly Color ColorMuted = Color.FromArgb(107, 114, 128);
        private readonly Color ColorHeading = Color.FromArgb(17, 24, 39);

        // ---- Controls referenced across methods ----
        private Label lblTotalIncome, lblTotalExpenses, lblNetBalance;
        private TextBox txtAmount, txtNotes, txtSearch;
        private ComboBox cboCategory;
        private RadioButton rbIncome, rbExpense;
        private DateTimePicker dtpDate;
        private DataGridView dgvTransactions;
        private RoundedButton btnAddTransaction, btnDeleteSelected;

        public MainForm()
        {
            InitializeComponent();
            SetupForm();
            BuildHeader();
            BuildSummaryCards();
            BuildEntryPanel();
            BuildTransactionsPanel();
            UpdateSummary();
        }

        // =========================================================
        // FORM SETUP
        // =========================================================
        private void SetupForm()
        {
            Text = "Personal Finance Tracker";
            ClientSize = new Size(900, 600);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ColorPageBg;
            Font = new Font("Segoe UI", 9F);
        }

        // =========================================================
        // HEADER (title, subtitle, date)
        // =========================================================
        private void BuildHeader()
        {
            Label lblTitle = new Label
            {
                Text = "Personal Finance Tracker",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = ColorHeading,
                AutoSize = true,
                Location = new Point(24, 16)
            };

            Label lblSubtitle = new Label
            {
                Text = "Track your income, expenses and manage your finances",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = ColorMuted,
                AutoSize = true,
                Location = new Point(26, 54)
            };

            Label lblDate = new Label
            {
                Text = "\U0001F4C5 " + DateTime.Now.ToString("dddd, dd MMMM yyyy"),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = ColorMuted,
                AutoSize = true
            };
            lblDate.Location = new Point(ClientSize.Width - 260, 20);

            Controls.Add(lblTitle);
            Controls.Add(lblSubtitle);
            Controls.Add(lblDate);
        }

        // =========================================================
        // SUMMARY CARDS (Total Income / Total Expenses / Net Balance)
        // =========================================================
        private void BuildSummaryCards()
        {
            int cardWidth = 278, cardHeight = 100, top = 82, gap = 15, left = 24;

            RoundedPanel cardIncome = CreateSummaryCard(
                new Point(left, top), new Size(cardWidth, cardHeight),
                "Total Income", "From all income sources",
                ColorGreenBg, ColorGreenText, -90f, true, out lblTotalIncome);

            RoundedPanel cardExpenses = CreateSummaryCard(
                new Point(left + cardWidth + gap, top), new Size(cardWidth, cardHeight),
                "Total Expenses", "From all expense items",
                ColorRedBg, ColorRedText, 90f, false, out lblTotalExpenses);

            RoundedPanel cardBalance = CreateSummaryCard(
                new Point(left + (cardWidth + gap) * 2, top), new Size(cardWidth, cardHeight),
                "Net Balance", "Income - Expenses",
                ColorBlueBg, ColorBlueText, -45f, true, out lblNetBalance);

            Controls.Add(cardIncome);
            Controls.Add(cardExpenses);
            Controls.Add(cardBalance);
        }

        private RoundedPanel CreateSummaryCard(Point location, Size size, string title,
            string caption, Color accentBg, Color accentText, float arrowAngle, bool uptrend, out Label valueLabel)
        {
            RoundedPanel card = new RoundedPanel
            {
                Location = location,
                Size = size,
                BorderColor = ColorBorder,
                BackColor = accentBg
            };

            Panel iconCircle = new Panel
            {
                Size = new Size(36, 36),
                Location = new Point(size.Width - 52, 14),
                BackColor = accentBg
            };
            iconCircle.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath ellipsePath = new GraphicsPath())
                {
                    ellipsePath.AddEllipse(0, 0, iconCircle.Width - 1, iconCircle.Height - 1);
                    iconCircle.Region = new Region(ellipsePath);
                    using (Brush b = new SolidBrush(accentBg))
                        e.Graphics.FillEllipse(b, 0, 0, iconCircle.Width - 1, iconCircle.Height - 1);
                }
                DrawArrowIcon(e.Graphics, iconCircle.ClientRectangle, accentText, arrowAngle);
            };

            Label lblTitleText = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorHeading,
                AutoSize = true,
                Location = new Point(16, 14)
            };

            valueLabel = new Label
            {
                Text = "৳ 0.00",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = accentText,
                AutoSize = true,
                Location = new Point(16, 38)
            };

            Label lblCaption = new Label
            {
                Text = caption,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = ColorMuted,
                AutoSize = true,
                Location = new Point(16, 70)
            };

            card.Controls.Add(iconCircle);
            card.Controls.Add(lblTitleText);
            card.Controls.Add(valueLabel);
            card.Controls.Add(lblCaption);

            return card;
        }

        /// <summary>
        /// Hand-draws a bold directional arrow (rather than relying on a text
        /// glyph) so its size and stroke thickness can be controlled directly.
        /// </summary>
        private void DrawArrowIcon(Graphics g, Rectangle bounds, Color color, float angleDegrees)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            PointF center = new PointF(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
            float len = Math.Min(bounds.Width, bounds.Height) * 0.32f;
            double rad = angleDegrees * Math.PI / 180.0;
            float dx = (float)Math.Cos(rad);
            float dy = (float)Math.Sin(rad);

            PointF start = new PointF(center.X - dx * len, center.Y - dy * len);
            PointF end = new PointF(center.X + dx * len, center.Y + dy * len);

            using (Pen pen = new Pen(color, 2.6f))
            {
                pen.StartCap = LineCap.Round;
                pen.CustomEndCap = new AdjustableArrowCap(4f, 5f, true);
                g.DrawLine(pen, start, end);
            }
        }

        // =========================================================
        // ENTRY PANEL (left side: Add New Transaction)
        // =========================================================
        private void BuildEntryPanel()
        {
            RoundedPanel panel = new RoundedPanel
            {
                Location = new Point(24, 198),
                Size = new Size(340, 380),
                CornerRadius = 14,
                BorderColor = ColorBorder,
                BackColor = ColorPageBg
            };

            // Header: outlined circle "+" icon followed by the bold blue title
            Panel headerIcon = new Panel { Size = new Size(22, 22), Location = new Point(16, 11), BackColor = Color.Transparent };
            headerIcon.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(ColorBlueText, 1.6f))
                {
                    e.Graphics.DrawEllipse(pen, 1, 1, headerIcon.Width - 3, headerIcon.Height - 3);
                    int cx = headerIcon.Width / 2, cy = headerIcon.Height / 2, r = 5;
                    e.Graphics.DrawLine(pen, cx - r, cy, cx + r, cy);
                    e.Graphics.DrawLine(pen, cx, cy - r, cx, cy + r);
                }
            };

            Label lblHeader = new Label
            {
                Text = "Add New Transaction",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ColorBlueText,
                AutoSize = true,
                Location = new Point(44, 11)
            };

            // Amount
            Label lblAmount = MakeFieldLabel("Amount (৳)", new Point(16, 44));
            txtAmount = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9.5F),
                PlaceholderText = "Enter amount"
            };
            txtAmount.KeyPress += TxtAmount_KeyPress;
            RoundedPanel amountWrap = WrapInputControl(new Point(16, 64), new Size(110, 32), txtAmount);

            // Type (radio buttons)
            Label lblType = MakeFieldLabel("Type", new Point(140, 44));
            rbIncome = new RadioButton
            {
                Text = "Income",
                Location = new Point(140, 70),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = ColorHeading
            };
            rbExpense = new RadioButton
            {
                Text = "Expense",
                Location = new Point(232, 70),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = ColorHeading
            };

            // Category
            Label lblCategory = MakeFieldLabel("Category", new Point(16, 104));
            cboCategory = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F),
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 20
            };
            cboCategory.Items.AddRange(new object[]
            { "Select category", "Salary", "Freelance", "Food", "Transport", "Utilities", "Other" });
            cboCategory.SelectedIndex = 0;
            cboCategory.DrawItem += CboCategory_DrawItem;
            RoundedPanel categoryWrap = WrapInputControl(new Point(16, 124), new Size(300, 32), cboCategory);

            // Date
            Label lblDateField = MakeFieldLabel("Date", new Point(16, 164));
            dtpDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Font = new Font("Segoe UI", 9.5F),
                CalendarForeColor = ColorHeading,
                CalendarMonthBackground = Color.White,
                Location = new Point(16, 184),
                Size = new Size(300, 32)
            };

            // Notes
            Label lblNotes = MakeFieldLabel("Notes (optional)", new Point(16, 224));
            txtNotes = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9.5F),
                Multiline = true,
                PlaceholderText = "Enter notes"
            };
            RoundedPanel notesWrap = WrapInputControl(new Point(16, 244), new Size(300, 50), txtNotes);

            // Add Transaction button
            btnAddTransaction = new RoundedButton
            {
                Text = "+   Add Transaction",
                Location = new Point(16, 308),
                Size = new Size(300, 38),
                CornerRadius = 10,
                BackColor = ColorBlueText,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnAddTransaction.Click += BtnAddTransaction_Click;

            panel.Controls.AddRange(new Control[]
            {
                headerIcon, lblHeader, lblAmount, amountWrap, lblType, rbIncome, rbExpense,
                lblCategory, categoryWrap, lblDateField, dtpDate,
                lblNotes, notesWrap, btnAddTransaction
            });

            Controls.Add(panel);
        }

        /// <summary>
        /// Wraps a native control in a RoundedPanel so it reads as a soft,
        /// rounded-border input box like the reference design, since WinForms
        /// text/combo/date controls don't support rounded corners natively.
        /// </summary>
        private RoundedPanel WrapInputControl(Point location, Size size, Control inner)
        {
            RoundedPanel wrap = new RoundedPanel
            {
                Location = location,
                Size = size,
                CornerRadius = 8,
                BorderColor = ColorBorder,
                BackColor = Color.White
            };

            bool isMultiline = inner is TextBox mtb && mtb.Multiline;
            inner.Width = size.Width - 20;
            if (isMultiline)
                inner.Height = size.Height - 16;
            inner.Location = new Point(10, isMultiline ? 8 : (size.Height - inner.Height) / 2);

            wrap.Controls.Add(inner);
            return wrap;
        }

        private void CboCategory_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0) return;

            string text = cboCategory.Items[e.Index].ToString() ?? "";
            Color color = e.Index == 0 ? ColorMuted : ColorHeading;

            Rectangle bounds = e.Bounds;
            bounds.X += 2;
            TextRenderer.DrawText(e.Graphics, text, cboCategory.Font, bounds, color,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            e.DrawFocusRectangle();
        }

        private Label MakeFieldLabel(string text, Point location) => new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = ColorHeading,
            AutoSize = true,
            Location = location
        };

        private void TxtAmount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                e.Handled = true;
        }

        // =========================================================
        // TRANSACTIONS PANEL (right side: grid + search + delete)
        // =========================================================
        private void BuildTransactionsPanel()
        {
            RoundedPanel panel = new RoundedPanel
            {
                Location = new Point(380, 198),
                Size = new Size(496, 380),
                CornerRadius = 14,
                BorderColor = ColorBorder,
                BackColor = ColorPageBg
            };

            Label lblHeader = new Label
            {
                Text = "☰ Transactions",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ColorBlueText,
                AutoSize = true,
                Location = new Point(16, 14)
            };

            // Rounded search box with a magnifying-glass icon on the right
            RoundedPanel searchWrap = new RoundedPanel
            {
                Location = new Point(316, 12),
                Size = new Size(164, 32),
                CornerRadius = 10,
                BorderColor = ColorBorder,
                BackColor = Color.White
            };

            txtSearch = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9.5F),
                PlaceholderText = "Search transactions..."
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            txtSearch.Width = searchWrap.Width - 24;
            txtSearch.Location = new Point(12, (searchWrap.Height - txtSearch.Height) / 2);

            searchWrap.Controls.Add(txtSearch);

            dgvTransactions = new DataGridView
            {
                Location = new Point(16, 56),
                Size = new Size(464, 262),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(237, 239, 242),
                ColumnHeadersDefaultCellStyle =
                {
                    BackColor = Color.FromArgb(238, 242, 255),
                    ForeColor = Color.FromArgb(71, 85, 105),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    SelectionBackColor = Color.FromArgb(238, 242, 255)
                },
                DefaultCellStyle =
                {
                    SelectionBackColor = Color.FromArgb(243, 244, 246),
                    SelectionForeColor = ColorHeading,
                    Padding = new Padding(2, 0, 0, 0)
                },
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 38,
                RowTemplate = { Height = 44 },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgvTransactions.Columns.Add("Date", "Date");
            dgvTransactions.Columns.Add("Type", "Type");
            dgvTransactions.Columns.Add("Category", "Category");
            dgvTransactions.Columns.Add("Amount", "Amount");
            dgvTransactions.Columns.Add("Notes", "Notes");
            dgvTransactions.CellPainting += DgvTransactions_CellPainting;

            btnDeleteSelected = new RoundedButton
            {
                Text = "Delete Selected",
                Location = new Point(320, 330),
                Size = new Size(160, 34),
                CornerRadius = 8,
                BackColor = ColorRedBg,
                ForeColor = ColorRedText,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnDeleteSelected.Click += BtnDeleteSelected_Click;

            panel.Controls.AddRange(new Control[]
            { lblHeader, searchWrap, dgvTransactions, btnDeleteSelected });

            Controls.Add(panel);
        }

        private void DgvTransactions_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != dgvTransactions.Columns["Type"].Index) return;

            e.PaintBackground(e.CellBounds, true);

            string value = e.Value?.ToString() ?? "";
            if (string.IsNullOrEmpty(value)) { e.Handled = true; return; }

            bool isIncome = value == "Income";
            Color bg = isIncome ? ColorGreenBg : ColorRedBg;
            Color fg = isIncome ? ColorGreenText : ColorRedText;

            Rectangle pillRect = new Rectangle(
                e.CellBounds.X + 8, e.CellBounds.Y + 6, 72, e.CellBounds.Height - 12);

            using (Brush b = new SolidBrush(bg))
            using (var path = RoundedRectPath(pillRect, 10))
                e.Graphics.FillPath(b, path);

            TextRenderer.DrawText(e.Graphics, value, new Font("Segoe UI", 8.5F, FontStyle.Bold),
                pillRect, fg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            e.Handled = true;
        }

        private System.Drawing.Drawing2D.GraphicsPath RoundedRectPath(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string filter = txtSearch.Text.Trim().ToLower();
            dgvTransactions.Rows.Clear();

            var filtered = string.IsNullOrEmpty(filter)
                ? transactions
                : transactions.Where(t =>
                    t.Category.ToLower().Contains(filter) ||
                    t.Notes.ToLower().Contains(filter) ||
                    t.Type.ToLower().Contains(filter)).ToList();

            foreach (var t in filtered) AddRowToGrid(t);
        }

        // =========================================================
        // ADD TRANSACTION
        // =========================================================
        private void BtnAddTransaction_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid positive amount.", "Invalid Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboCategory.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select a category.", "Invalid Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var txn = new Transaction
            {
                Date = dtpDate.Value.Date,
                Type = rbIncome.Checked ? "Income" : "Expense",
                Category = cboCategory.SelectedItem.ToString(),
                Amount = amount,
                Notes = txtNotes.Text.Trim()
            };

            transactions.Add(txn);
            AddRowToGrid(txn);
            UpdateSummary();
            ClearInputs();
        }

        private void AddRowToGrid(Transaction txn)
        {
            int rowIndex = dgvTransactions.Rows.Add(
                txn.Date.ToString("dd/MM/yyyy"),
                txn.Type,
                txn.Category,
                (txn.Type == "Income" ? "+ " : "- ") + txn.Amount.ToString("N2"),
                txn.Notes
            );

            Color amountColor = txn.Type == "Income" ? ColorGreenText : ColorRedText;
            dgvTransactions.Rows[rowIndex].Cells["Amount"].Style.ForeColor = amountColor;
            dgvTransactions.Rows[rowIndex].Cells["Amount"].Style.Font =
                new Font("Segoe UI", 9, FontStyle.Bold);
        }

        private void ClearInputs()
        {
            txtAmount.Clear();
            txtNotes.Clear();
            cboCategory.SelectedIndex = 0;
            rbIncome.Checked = true;
            dtpDate.Value = DateTime.Now;
        }

        // =========================================================
        // DELETE SELECTED
        // =========================================================
        private void BtnDeleteSelected_Click(object sender, EventArgs e)
        {
            if (dgvTransactions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int rowIndex = dgvTransactions.SelectedRows[0].Index;

            string dateText = dgvTransactions.Rows[rowIndex].Cells["Date"].Value.ToString();
            string category = dgvTransactions.Rows[rowIndex].Cells["Category"].Value.ToString();
            string notes = dgvTransactions.Rows[rowIndex].Cells["Notes"].Value?.ToString() ?? "";

            var match = transactions.FirstOrDefault(t =>
                t.Date.ToString("dd/MM/yyyy") == dateText &&
                t.Category == category &&
                t.Notes == notes);

            if (match != null) transactions.Remove(match);

            dgvTransactions.Rows.RemoveAt(rowIndex);
            UpdateSummary();
        }

        // =========================================================
        // SUMMARY CALCULATION
        // =========================================================
        private void UpdateSummary()
        {
            decimal totalIncome = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            decimal totalExpenses = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            decimal netBalance = totalIncome - totalExpenses;

            lblTotalIncome.Text = "৳ " + totalIncome.ToString("N2");
            lblTotalExpenses.Text = "৳ " + totalExpenses.ToString("N2");
            lblNetBalance.Text = "৳ " + netBalance.ToString("N2");

            lblNetBalance.ForeColor = netBalance < 0 ? ColorRedText : ColorBlueText;
        }
    }
}
