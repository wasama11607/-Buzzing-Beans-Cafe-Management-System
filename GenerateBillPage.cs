using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Buzzing_Beans
{
    public partial class GenerateBillPage : Form
    {
        // Properties to receive data from previous form
        public decimal Subtotal { get; set; }
        public bool IsMember { get; set; }
        public string ItemsList { get; set; }
        public string SelectedTable { get; set; } = "None";

        private Label lblTotalAmount;
        private Label lblSubtotalAmount;
        private Label lblDiscountAmount;
        private Panel MembershipDiscountRow;
        private FlowLayoutPanel BillItemsPanel;
        private Label lblTableInfo;

        // ========== PERFORMANCE FIX: Cache regions and paths ==========
        private Region cachedFormRegion;
        private Region cachedPanelRegion;
        private Region cachedButtonRegion;
        private GraphicsPath formPath;
        private GraphicsPath panelPath;
        private GraphicsPath buttonPath;
        private bool isFormRegionDirty = true;

        public GenerateBillPage()
        {
            // ========== PERFORMANCE FIX 1: Enable double buffering ==========
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                  ControlStyles.UserPaint |
                  ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            InitializeForm();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // ========== PERFORMANCE FIX 2: Lazy load bill data after form is visible ==========
            isFormRegionDirty = true;
            LoadBillData();
        }

        private void InitializeForm()
        {
            this.Text = "Final Bill";
            this.Size = new Size(400, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;
            this.AllowTransparency = true;

            // Main Border (Rounded Corner Container)
            Panel mainBorder = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(2)
            };
            // ========== PERFORMANCE FIX 3: Single paint event, cache region ==========
            mainBorder.Paint += (s, e) =>
            {
                if (cachedPanelRegion == null)
                {
                    panelPath = GetRoundedRectanglePath(new Rectangle(0, 0, mainBorder.Width - 1, mainBorder.Height - 1), 25);
                    cachedPanelRegion = new Region(panelPath);
                    mainBorder.Region = cachedPanelRegion;
                }
                
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(Color.FromArgb(215, 204, 200), 2))
                {
                    e.Graphics.DrawPath(pen, panelPath);
                }
            };

            // Load Background Image (Async-style loading)
            string bgPath = GetImagePath("backgroundwhite.png");
            if (File.Exists(bgPath))
            {
                try
                {
                    mainBorder.BackgroundImage = Image.FromFile(bgPath);
                    mainBorder.BackgroundImageLayout = ImageLayout.Stretch;
                }
                catch { mainBorder.BackColor = Color.White; }
            }
            else
            {
                mainBorder.BackColor = Color.White;
            }

            // Main Grid Container (Margin 30)
            Panel container = new Panel
            {
                Margin = new Padding(30),
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            // ========== HEADER SECTION ==========
            Panel headerPanel = new Panel
            {
                Height = 120,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent
            };

            Label lblTitle = new Label
            {
                Text = "BUZZING BEANS",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(62, 39, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 45
            };

            Label lblSubtitle = new Label
            {
                Text = "E-Receipt",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(141, 110, 99),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 25
            };

            Label lblSeparator = new Label
            {
                Text = "--------------------------",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(215, 204, 200),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 30
            };

            headerPanel.Controls.Add(lblSeparator);
            headerPanel.Controls.Add(lblSubtitle);
            headerPanel.Controls.Add(lblTitle);

            // ========== TABLE INFO ==========
            lblTableInfo = new Label
            {
                Text = $"Table: {SelectedTable}",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(141, 110, 99),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            headerPanel.Controls.Add(lblTableInfo);

            // ========== PRINT BUTTON ==========
            Button btnFinish = new Button
            {
                Text = "PRINT & FINISH",
                Height = 50,
                Dock = DockStyle.Bottom,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 10, 0, 0)
            };
            btnFinish.FlatAppearance.BorderSize = 0;
            btnFinish.Click += BtnFinish_Click;

            // ========== PERFORMANCE FIX 4: Cache button region ==========
            bool buttonHovered = false;
            btnFinish.Paint += (s, e) =>
            {
                if (cachedButtonRegion == null)
                {
                    buttonPath = GetRoundedRectanglePath(new Rectangle(0, 0, btnFinish.Width - 1, btnFinish.Height - 1), 15);
                    cachedButtonRegion = new Region(buttonPath);
                    btnFinish.Region = cachedButtonRegion;
                }

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color bgColor = buttonHovered ? Color.FromArgb(93, 64, 55) : Color.FromArgb(62, 39, 35);
                using (SolidBrush brush = new SolidBrush(bgColor))
                {
                    e.Graphics.FillPath(brush, buttonPath);
                }
                TextRenderer.DrawText(e.Graphics, btnFinish.Text, new Font("Segoe UI", 12, FontStyle.Bold),
                    new Rectangle(0, 0, btnFinish.Width, btnFinish.Height), Color.FromArgb(245, 230, 211),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            btnFinish.MouseEnter += (s, e) => { buttonHovered = true; btnFinish.Invalidate(); };
            btnFinish.MouseLeave += (s, e) => { buttonHovered = false; btnFinish.Invalidate(); };

            // ========== BOTTOM SUMMARY SECTION ==========
            Panel summaryPanel = new Panel
            {
                Height = 150,
                Dock = DockStyle.Bottom,
                BackColor = Color.Transparent
            };

            // Subtotal Row
            Panel subtotalRow = CreateBillRow("Subtotal", "Rs. ", Color.FromArgb(141, 110, 99), false);
            subtotalRow.Location = new Point(0, 0);
            lblSubtotalAmount = (Label)subtotalRow.Controls[1];

            // Member Discount Row
            MembershipDiscountRow = CreateBillRow("Member Discount (10%)", "- Rs. ", Color.FromArgb(46, 125, 50), true);
            MembershipDiscountRow.Location = new Point(0, 30);
            lblDiscountAmount = (Label)MembershipDiscountRow.Controls[1];

            // Total Row
            Panel totalRow = new Panel
            {
                Height = 45,
                Width = 340,
                Location = new Point(0, 70)
            };

            Label lblTotalText = new Label
            {
                Text = "TOTAL BILL",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(62, 39, 35),
                AutoSize = true,
                Location = new Point(0, 5)
            };

            lblTotalAmount = new Label
            {
                Text = "Rs. 0",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(62, 39, 35),
                AutoSize = true,
                Location = new Point(340 - 105, 5),
                TextAlign = ContentAlignment.MiddleRight
            };

            totalRow.Controls.Add(lblTotalText);
            totalRow.Controls.Add(lblTotalAmount);

            summaryPanel.Controls.Add(totalRow);
            summaryPanel.Controls.Add(MembershipDiscountRow);
            summaryPanel.Controls.Add(subtotalRow);

            // ========== BILL ITEMS SECTION ==========
            Panel scrollContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            // ========== PERFORMANCE FIX 5: Optimize FlowLayoutPanel ==========
            BillItemsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 5, 0, 5)
            };

            scrollContainer.Controls.Add(BillItemsPanel);

            // Add controls in correct order
            container.Controls.Add(scrollContainer);
            container.Controls.Add(summaryPanel);
            container.Controls.Add(btnFinish);
            container.Controls.Add(headerPanel);

            mainBorder.Controls.Add(container);
            this.Controls.Add(mainBorder);

            // ========== PERFORMANCE FIX 6: Cache form region ==========
            this.Paint += (s, e) =>
            {
                if (isFormRegionDirty || cachedFormRegion == null)
                {
                    formPath = GetRoundedRectanglePath(new Rectangle(0, 0, this.Width - 1, this.Height - 1), 25);
                    cachedFormRegion = new Region(formPath);
                    this.Region = cachedFormRegion;
                    isFormRegionDirty = false;
                }
            };
        }

        private Panel CreateBillRow(string labelText, string prefix, Color textColor, bool isBold)
        {
            Panel row = new Panel
            {
                Height = 25,
                Width = 340,
                BackColor = Color.Transparent
            };

            Label lblLabel = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 12, isBold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = textColor,
                AutoSize = true,
                Location = new Point(0, 2)
            };

            Label lblValue = new Label
            {
                Text = prefix + "0",
                Font = new Font("Segoe UI", 12, isBold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = textColor,
                AutoSize = true,
                Location = new Point(340 - 80, 2),
                TextAlign = ContentAlignment.MiddleRight
            };

            row.Controls.Add(lblLabel);
            row.Controls.Add(lblValue);

            return row;
        }

        private void LoadBillData()
        {
            // ========== PERFORMANCE FIX 7: Batch UI updates ==========
            if (lblTableInfo != null)
            {
                lblTableInfo.Text = $"Table: {SelectedTable}";
            }

            BillItemsPanel.SuspendLayout();
            BillItemsPanel.Controls.Clear();

            if (!string.IsNullOrEmpty(ItemsList))
            {
                char[] splitDelimiters = { '\n', '\r', ',', ';' };
                string[] items = ItemsList.Split(splitDelimiters, StringSplitOptions.RemoveEmptyEntries);

                foreach (string item in items)
                {
                    string cleanedItem = item.Trim();
                    if (!string.IsNullOrEmpty(cleanedItem))
                    {
                        Panel itemRow = new Panel
                        {
                            Height = 30,
                            Width = BillItemsPanel.Width - 25,
                            BackColor = Color.Transparent,
                            Margin = new Padding(0, 2, 0, 2)
                        };

                        Label lblItemName = new Label
                        {
                            Text = cleanedItem,
                            Font = new Font("Segoe UI", 11, FontStyle.Regular),
                            ForeColor = Color.FromArgb(62, 39, 35),
                            AutoSize = true,
                            Location = new Point(5, 5)
                        };

                        itemRow.Controls.Add(lblItemName);
                        BillItemsPanel.Controls.Add(itemRow);
                    }
                }
            }

            BillItemsPanel.ResumeLayout(false);

            // Update amounts
            decimal discount = IsMember ? Math.Round(Subtotal * 0.10m, 2) : 0;
            decimal total = Math.Round(Subtotal - discount, 2);

            lblSubtotalAmount.Text = $"Rs. {Subtotal:F0}";

            if (IsMember)
            {
                lblDiscountAmount.Text = $"- Rs. {discount:F0}";
                MembershipDiscountRow.Visible = true;
            }
            else
            {
                MembershipDiscountRow.Visible = false;
            }

            lblTotalAmount.Text = $"Rs. {total:F0}";
        }

        private void BtnFinish_Click(object sender, EventArgs e)
        {
            try
            {
               // string orderId = $"#BB-{DateTime.Now:yyyyMMddHHmmss}";
                //string itemsForFile = ItemsList?.Replace("\n", ", ") ?? "";
                //FileHelper.AddSaleRecord(orderId, itemsForFile, lblTotalAmount?.Text?.Replace("Rs. ", "") ?? "0");

                MessageBox.Show($"Order completed successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        // ========== DRAWING METHODS ==========

        private GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private string GetImagePath(string fileName)
        {
            string[] possiblePaths = {
                Path.Combine(Application.StartupPath, "Resources", fileName),
                Path.Combine(Application.StartupPath, "Images", fileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName),
                @"D:\Kisfa Javed\SDA\Menu\" + fileName
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                    return path;
            }
            return "";
        }
    }
}