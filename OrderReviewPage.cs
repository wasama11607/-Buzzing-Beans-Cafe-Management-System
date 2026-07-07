using Buzzing_Beans;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Label = System.Windows.Forms.Label;

namespace Buzzing_Beans
{
    public partial class OrderReviewPage : Form
    {
        private readonly Panel mainPanel;
        private readonly Panel reviewContainerStack;

        // Form Controls
        private readonly Button btnBack;
        private readonly Label lblTitle;
        private readonly Panel pnlSeparator;
        private readonly Panel pnlCartContainer;
        private readonly Button btnConfirmBill;

        // Data Context & Paths
        private string imagesFolderPath;
        private List<CartItem> structuralCartReference;
        private double conceptualTotal = 0.0;
        private Image mainBgImage;
        private bool isMember = false; // Track if customer is a member
        private string currentMemberId = ""; // Track member ID

        // Window Draggable State Variables
        private bool isDragging = false;
        private Point dragStartPoint = new Point(0, 0);

        public OrderReviewPage(List<CartItem> currentCart, double totalAmount)
            : this(currentCart, totalAmount, false, "")
        {
        }

        public OrderReviewPage(List<CartItem> currentCart, double totalAmount, bool isMemberCustomer, string memberId)
        {
            this.structuralCartReference = currentCart;
            this.conceptualTotal = totalAmount;
            this.isMember = isMemberCustomer;
            this.currentMemberId = memberId;

            this.SuspendLayout();

            // ========== 1. WINDOW STRUCTURE AND BOUNDS SETUP ==========
            this.Text = "Review Your Order";
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Strict dimension boundaries matching your login card profile
            this.Size = new Size(440, 580);
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            this.BackColor = Color.FromArgb(245, 245, 220);

            SetCorrectImagePath();

            // Load Background Asset Safely
            try
            {
                string bgPath = Path.Combine(imagesFolderPath, "background.png");
                if (File.Exists(bgPath)) mainBgImage = Image.FromFile(bgPath);
            }
            catch
            {
                try { mainBgImage = Image.FromFile(@"D:\Kisfa Javed\SDA\Menu\background.png"); } catch { }
            }

            // ========== 2. LAYOUT ENGINE BACKGROUND LAYER ==========
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            if (mainBgImage != null)
            {
                mainPanel.BackgroundImage = mainBgImage;
                mainPanel.BackgroundImageLayout = ImageLayout.Stretch;
            }

            // Mouse hook pass-through to guarantee window dragging actions function perfectly
            mainPanel.MouseDown += Window_MouseDown;
            mainPanel.MouseMove += Window_MouseMove;
            mainPanel.MouseUp += Window_MouseUp;
            this.Controls.Add(mainPanel);

            // ========== 3. NAVIGATION HEADER CAP (Back Button) ==========
            btnBack = new Button
            {
                Text = "← Back",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 54, 33),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(25, 25),
                AutoSize = true
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnBack.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnBack.Click += BtnBackToMenu_Click;
            mainPanel.Controls.Add(btnBack);

            // ========== 4. CORE CONTROLS STACK GRID BLOCK ==========
            reviewContainerStack = new Panel
            {
                Size = new Size(360, 440),
                Location = new Point(40, 95),
                BackColor = Color.FromArgb(245, 245, 220)
            };
            mainPanel.Controls.Add(reviewContainerStack);

            // Bind drag actions to the stack panel surface
            reviewContainerStack.MouseDown += Window_MouseDown;
            reviewContainerStack.MouseMove += Window_MouseMove;
            reviewContainerStack.MouseUp += Window_MouseUp;

            // --- A. Section Title Text Block ---
            lblTitle = new Label
            {
                Text = "YOUR SELECTIONS",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(62, 39, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(360, 40),
                Location = new Point(0, 0),
                BackColor = Color.Transparent
            };
            reviewContainerStack.Controls.Add(lblTitle);

            // --- B. Horizontal Separator Bar ---
            pnlSeparator = new Panel
            {
                Size = new Size(200, 2),
                Location = new Point(80, lblTitle.Bottom + 5),
                BackColor = Color.FromArgb(215, 204, 200)
            };
            reviewContainerStack.Controls.Add(pnlSeparator);

            // --- C. White Selection Panel ---
            pnlCartContainer = new Panel
            {
                Location = new Point(5, pnlSeparator.Bottom + 15),
                Size = new Size(350, 290),
                BackColor = Color.White,
                AutoScroll = true,
                Padding = new Padding(12)
            };
            ApplyContainerRounding(pnlCartContainer, 15);
            reviewContainerStack.Controls.Add(pnlCartContainer);

            // --- D. Confirm Button ---
            string discountText = isMember ? " (10% Member Discount Applied)" : "";
            btnConfirmBill = new Button
            {
                Size = new Size(350, 45),
                Location = new Point(5, pnlCartContainer.Bottom + 20),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(62, 39, 35),
                ForeColor = Color.FromArgb(245, 230, 211)
            };
            btnConfirmBill.FlatAppearance.BorderSize = 0;
            btnConfirmBill.Click += BtnConfirmBill_Click;

            decimal finalTotal = isMember ? (decimal)conceptualTotal * 0.9m : (decimal)conceptualTotal;
            ApplyButtonStyling(btnConfirmBill, $"CONFIRM & GENERATE BILL (Rs. {finalTotal:F0}){discountText}");
            reviewContainerStack.Controls.Add(btnConfirmBill);

            // Custom Frame Paint handler execution
            this.Paint += OrderReviewPage_Paint;

            this.ResumeLayout(false);
            PopulateCartSelections();
        }

        private void SetCorrectImagePath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            imagesFolderPath = Path.Combine(baseDir, "Images");

            if (!Directory.Exists(imagesFolderPath))
            {
                string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\"));
                imagesFolderPath = Path.Combine(projectRoot, "Images");
            }
        }

        private void PopulateCartSelections()
        {
            pnlCartContainer.Controls.Clear();
            int verticalOffsetPosition = 12;
            int rowWidth = pnlCartContainer.Width - 25;

            foreach (var item in structuralCartReference)
            {
                Panel rowPanel = new Panel
                {
                    Width = rowWidth,
                    Height = 30,
                    Location = new Point(5, verticalOffsetPosition),
                    BackColor = Color.White
                };

                Label lblSelectionDetails = new Label
                {
                    Text = $"{item.Quantity}x {item.Item.Name}",
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(62, 39, 35),
                    Location = new Point(5, 5),
                    Size = new Size(180, 22),
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.White
                };

                double lineItemCost = item.Item.Price * item.Quantity;

                Label lblSelectionPrice = new Label
                {
                    Text = $"Rs. {lineItemCost:F0}",
                    Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                    ForeColor = Color.FromArgb(141, 110, 99),
                    Size = new Size(80, 22),
                    Location = new Point(rowPanel.Width - 85, 5),
                    TextAlign = ContentAlignment.MiddleRight,
                    BackColor = Color.White
                };

                rowPanel.Controls.Add(lblSelectionDetails);
                rowPanel.Controls.Add(lblSelectionPrice);
                pnlCartContainer.Controls.Add(rowPanel);
                verticalOffsetPosition += 35;
            }

            // Add discount row if member
            if (isMember)
            {
                Panel discountRow = new Panel
                {
                    Width = rowWidth,
                    Height = 30,
                    Location = new Point(5, verticalOffsetPosition),
                    BackColor = Color.White
                };

                Label lblDiscountText = new Label
                {
                    Text = "Member Discount (10%)",
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(46, 125, 50),
                    Location = new Point(5, 5),
                    Size = new Size(180, 22),
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.White
                };

                double discountAmount = conceptualTotal * 0.10;
                Label lblDiscountPrice = new Label
                {
                    Text = $"- Rs. {discountAmount:F0}",
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(46, 125, 50),
                    Size = new Size(80, 22),
                    Location = new Point(rowWidth - 85, 5),
                    TextAlign = ContentAlignment.MiddleRight,
                    BackColor = Color.White
                };

                discountRow.Controls.Add(lblDiscountText);
                discountRow.Controls.Add(lblDiscountPrice);
                pnlCartContainer.Controls.Add(discountRow);
                verticalOffsetPosition += 35;
            }
        }

        private string GenerateOrderId()
        {
            var sales = FileHelper.ReadSales();
            int maxId = 0;

            foreach (var sale in sales)
            {
                if (sale.Length >= 2 && sale[1].StartsWith("#BB-"))
                {
                    string numPart = sale[1].Substring(4);
                    if (int.TryParse(numPart, out int num) && num > maxId)
                    {
                        maxId = num;
                    }
                }
            }

            int newId = maxId + 1;
            return $"#BB-{newId:D3}";
        }

        private string GetOrderItemsString()
        {
            List<string> items = new List<string>();
            foreach (var cartItem in structuralCartReference)
            {
                items.Add($"{cartItem.Quantity}x {cartItem.Item.Name}");
            }
            // CHANGED: Use a semicolon instead of \n
            return string.Join("; ", items);
        }

        // ========== SYSTEM CARD DRAW RENDERING ==========
        private void OrderReviewPage_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

            using (GraphicsPath path = GetRoundedRectPath(rect, 30))
            {
                this.Region = new Region(path);
                Color coffeeBorderColor = Color.FromArgb(62, 39, 35);
                using (Pen borderPen = new Pen(coffeeBorderColor, 3))
                {
                    e.Graphics.DrawPath(borderPen, path);
                }
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                const int CS_DROPSHADOW = 0x20000;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        // ========== CORE WINDOW MOTION HOOK DRAGGERS ==========
        private void Window_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = new Point(e.X, e.Y);
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point p = PointToScreen(e.Location);
                this.Location = new Point(p.X - this.dragStartPoint.X, p.Y - this.dragStartPoint.Y);
            }
        }

        private void Window_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        // ========== INTERACTIVE RENDER AND CORNER CURVE HELPERS ==========
        private void ApplyContainerRounding(Panel panel, int radius)
        {
            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.Clear(panel.BackColor);
                Rectangle rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                using (GraphicsPath path = GetRoundedRectPath(rect, radius))
                {
                    panel.Region = new Region(path);
                    using (Pen borderPen = new Pen(Color.FromArgb(215, 204, 200), 1))
                    {
                        e.Graphics.DrawPath(borderPen, path);
                    }
                }
            };
        }

        private void ApplyButtonStyling(Button btn, string labelText)
        {
            bool isHovered = false;

            btn.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                Rectangle rect = new Rectangle(0, 0, btn.Width, btn.Height);

                using (GraphicsPath path = GetRoundedRectPath(rect, 15))
                {
                    btn.Region = new Region(path);
                    Color currentBg = isHovered ? Color.FromArgb(93, 64, 55) : Color.FromArgb(62, 39, 35);

                    using (SolidBrush bgBrush = new SolidBrush(currentBg))
                    {
                        e.Graphics.FillPath(bgBrush, path);
                    }

                    TextRenderer.DrawText(e.Graphics, labelText, new Font("Segoe UI", 7, FontStyle.Bold), rect, btn.ForeColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            };

            btn.MouseEnter += (s, e) => { isHovered = true; btn.Invalidate(); };
            btn.MouseLeave += (s, e) => { isHovered = false; btn.Invalidate(); };
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
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

        // ========== APPLICATION FLOW DIALOG DISPATCH ACTION EVENTS ==========
        private void BtnBackToMenu_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void BtnConfirmBill_Click(object sender, EventArgs e)
        {
            // Calculate final total with discount
            decimal finalTotal = isMember ? (decimal)conceptualTotal * 0.9m : (decimal)conceptualTotal;
            string orderId = GenerateOrderId();
            string itemsString = GetOrderItemsString();
            string memberIdForOrder = isMember ? currentMemberId : "GUEST";

            // Save to sales file (for manager reports)
            FileHelper.AddSaleRecord(orderId, itemsString, finalTotal.ToString("F0"));

            // ========== CRITICAL: Save to new_orders.txt for barista ==========
            string newOrdersPath = Path.Combine(Application.StartupPath, "Data", "new_orders.txt");

            // Create the Data folder if it doesn't exist
            string dataFolder = Path.Combine(Application.StartupPath, "Data");
            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }

            string newOrderLine = $"{orderId},{itemsString.Trim()},{finalTotal.ToString("F0")}";

            // Append to file safely
            File.AppendAllText(newOrdersPath, newOrderLine + Environment.NewLine);

            // Show success message
            string discountMessage = isMember ? $"\n\nMember Discount Applied: 10%\nYou saved: Rs. {(decimal)conceptualTotal * 0.1m:F0}" : "";
            MessageBox.Show($"Order placed successfully!\nOrder ID: {orderId}\nTotal Amount: Rs. {finalTotal:F0}{discountMessage}\n\nPlease choose your table on the next screen. Thank you! ☕✨",
                            "Buzzing Beans Order", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ========== FIX: Don't close this form, just hide it ==========
            this.Hide();

            using (OrderConfirmationPage confirmationScreen = new OrderConfirmationPage(this, finalTotal, isMember, itemsString))
            {
                if (confirmationScreen.ShowDialog() == DialogResult.OK)
                {
                    string selectedTable = confirmationScreen.SelectedTableNumber;
                    this.DialogResult = DialogResult.OK;
                    this.Close();  // Close only after everything is complete
                }
                else
                {
                    // User cancelled from confirmation page, show this form again
                    this.Show();
                }
            }
        }

        // Add this helper method to calculate estimated time
        private int CalculateEstimatedTime(string orderDetails)
        {
            int estimatedSeconds = 90; // Base 1.5 minutes

            if (!string.IsNullOrEmpty(orderDetails))
            {
                if (orderDetails.Contains("Latte") || orderDetails.Contains("Espresso") ||
                    orderDetails.Contains("Coffee") || orderDetails.Contains("Cappuccino") ||
                    orderDetails.Contains("Americano") || orderDetails.Contains("Mocha"))
                {
                    estimatedSeconds += 120;
                }

                if (orderDetails.Contains("Tea") || orderDetails.Contains("Chai"))
                {
                    estimatedSeconds += 90;
                }

                if (orderDetails.Contains("Cake") || orderDetails.Contains("Muffin") ||
                    orderDetails.Contains("Macaron") || orderDetails.Contains("Cheese"))
                {
                    estimatedSeconds += 60;
                }
            }

            return estimatedSeconds;
        }
    }
}