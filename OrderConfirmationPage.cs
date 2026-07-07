using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Buzzing_Beans
{
    public partial class OrderConfirmationPage : Form
    {
        private System.Windows.Forms.Timer countdownTimer;
        private TimeSpan timeRemaining = TimeSpan.FromMinutes(5);

        // Core Containers
        private Panel overlayBackgroundPanel;
        private Panel centeredConfirmationCard;
        private TableLayoutPanel tableGrid;

        // Controls
        private Label lblTimer;
        private Button btnProceed;
        private Button selectedTableButton;

        // Data fields passed down from Main Order Menu Form
        private readonly decimal finalSubtotal;
        private readonly bool isCustomerMember;
        private readonly string finalItemsList;
        private readonly Form parentOrderReviewForm;  // ========== NEW: Keep reference to parent ==========

        public string SelectedTableNumber { get; set; } = "None";

        /// <summary>
        /// Updated constructor accepting business data from your main order transaction page
        /// </summary>
        public OrderConfirmationPage(Form parentForm, decimal subtotal, bool isMember, string itemsList)
        {
            this.SuspendLayout();

            // ========== NEW: Store parent form reference ==========
            this.parentOrderReviewForm = parentForm;

            // Assign incoming data values directly to clean readonly backing fields
            this.finalSubtotal = subtotal;
            this.isCustomerMember = isMember;
            this.finalItemsList = itemsList;

            // Calculate dynamic wait time based on order
            CalculateWaitTimeBasedOnOrder();

            // ========== 1. MATCH THE FULL SIZE OF THE APPLICATION ==========
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = parentForm.Size;
            this.Location = parentForm.Location;
            this.BackColor = Color.FromArgb(163, 153, 140);

            // ========== FIX: DON'T hide the parent form here, handle it in btnProceed ==========

            InitializeOverlayHierarchy();
            StartCountdownTimer();

            this.ResumeLayout(false);
        }

        private void CalculateWaitTimeBasedOnOrder()
        {
            // Estimate time based on order items
            int estimatedSeconds = 90; // Base 1.5 minutes

            if (!string.IsNullOrEmpty(finalItemsList))
            {
                // Coffees take about 2 minutes each
                if (finalItemsList.Contains("Latte") || finalItemsList.Contains("Espresso") ||
                    finalItemsList.Contains("Coffee") || finalItemsList.Contains("Cappuccino") ||
                    finalItemsList.Contains("Americano") || finalItemsList.Contains("Mocha"))
                {
                    estimatedSeconds += 120;
                }

                // Teas take about 1.5 minutes
                if (finalItemsList.Contains("Tea") || finalItemsList.Contains("Chai"))
                {
                    estimatedSeconds += 90;
                }

                // Desserts/Cakes take about 1 minute (usually ready to serve)
                if (finalItemsList.Contains("Cake") || finalItemsList.Contains("Muffin") ||
                    finalItemsList.Contains("Macaron") || finalItemsList.Contains("Cheese"))
                {
                    estimatedSeconds += 60;
                }
            }

            // Member gets 30 seconds less wait time (priority service)
            if (isCustomerMember)
                estimatedSeconds = Math.Max(30, estimatedSeconds - 30);

            timeRemaining = TimeSpan.FromSeconds(estimatedSeconds);
        }

        private void InitializeOverlayHierarchy()
        {
            // ========== 2. FULL WINDOW SEMI-TRANSPARENT TINT COVER ==========
            overlayBackgroundPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Cornsilk
            };
            this.Controls.Add(overlayBackgroundPanel);

            overlayBackgroundPanel.Paint += (s, e) =>
            {
                using (SolidBrush alphaBrush = new SolidBrush(Color.FromArgb(150, 30, 20, 15)))
                {
                    e.Graphics.FillRectangle(alphaBrush, overlayBackgroundPanel.ClientRectangle);
                }
            };

            // ========== 3. CENTERED PREMIUM CONFIRMATION CARD ==========
            centeredConfirmationCard = new Panel
            {
                Size = new Size(430, 570),
                BackColor = Color.FromArgb(248, 241, 231)
            };

            centeredConfirmationCard.Location = new Point(
                (this.ClientSize.Width - centeredConfirmationCard.Width) / 2,
                (this.ClientSize.Height - centeredConfirmationCard.Height) / 2
            );

            centeredConfirmationCard.HandleCreated += (s, e) => ApplyControlRounding(centeredConfirmationCard, 25);
            overlayBackgroundPanel.Controls.Add(centeredConfirmationCard);

            // --- A. Top Status Header ---
            FlowLayoutPanel headerStack = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Size = new Size(360, 45),
                Location = new Point(30, 35),
                BackColor = Color.Transparent,
                WrapContents = false
            };

            Panel checkCircle = new Panel { Size = new Size(36, 36), Margin = new Padding(3, 0, 12, 0), BackColor = Color.FromArgb(62, 39, 35) };
            checkCircle.HandleCreated += (s, e) => ApplyControlRounding(checkCircle, 18);

            Label lblCheck = new Label
            {
                Text = "✔",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(245, 230, 211),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            checkCircle.Controls.Add(lblCheck);

            Label lblConfirmedText = new Label
            {
                Text = "CONFIRMED!",
                Font = new Font("Segoe UI", 22f, FontStyle.Bold),
                ForeColor = Color.FromArgb(62, 39, 35),
                AutoSize = true
            };

            headerStack.Controls.Add(checkCircle);
            headerStack.Controls.Add(lblConfirmedText);
            centeredConfirmationCard.Controls.Add(headerStack);

            // --- B. Selection Guide Subtitle ---
            Label lblInstruction = new Label
            {
                Text = "SELECT YOUR TABLE",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(141, 110, 99),
                Location = new Point(30, 95),
                Size = new Size(360, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            centeredConfirmationCard.Controls.Add(lblInstruction);

            // --- C. White Clean Container Grid for Circular Table Buttons ---
            Panel gridContainer = new Panel
            {
                Location = new Point(30, 135),
                Size = new Size(360, 155),
                BackColor = Color.White,
                Padding = new Padding(12)
            };
            gridContainer.HandleCreated += (s, e) => ApplyControlRounding(gridContainer, 15);

            tableGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                BackColor = Color.White
            };

            for (int i = 0; i < 4; i++) tableGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            for (int i = 0; i < 2; i++) tableGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            int tableCounter = 1;
            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    Button btnTable = new Button
                    {
                        Text = $"T{tableCounter}",
                        Tag = tableCounter.ToString(),
                        Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                        Size = new Size(52, 52),
                        Margin = new Padding(3),
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand,
                        Anchor = AnchorStyles.None
                    };
                    btnTable.FlatAppearance.BorderSize = 0;

                    if (tableCounter == 3 || tableCounter == 7)
                    {
                        btnTable.Enabled = false;
                        btnTable.BackColor = Color.FromArgb(229, 115, 115);
                        btnTable.ForeColor = Color.White;
                        btnTable.Cursor = Cursors.No;
                    }
                    else
                    {
                        btnTable.BackColor = Color.FromArgb(215, 204, 200);
                        btnTable.ForeColor = Color.FromArgb(62, 39, 35);
                        btnTable.Click += TableButton_Click;
                    }

                    btnTable.HandleCreated += (s, ev) => ApplyControlRounding((Control)s, 26);

                    tableGrid.Controls.Add(btnTable, col, row);
                    tableCounter++;
                }
            }
            gridContainer.Controls.Add(tableGrid);
            centeredConfirmationCard.Controls.Add(gridContainer);

            // --- D. Espresso Wait Time Display Card ---
            Panel timerCard = new Panel
            {
                Location = new Point(30, 310),
                Size = new Size(360, 60),
                BackColor = Color.FromArgb(62, 39, 35)
            };
            timerCard.HandleCreated += (s, e) => ApplyControlRounding(timerCard, 15);

            Label lblWaitTitle = new Label
            {
                Text = "WAIT TIME:",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(251, 235, 214),
                Location = new Point(30, 18),
                Size = new Size(110, 25),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            lblTimer = new Label
            {
                Text = timeRemaining.ToString(@"mm\:ss"),
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.FromArgb(252, 235, 216),
                Location = new Point(145, 12),
                Size = new Size(170, 35),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            timerCard.Controls.Add(lblWaitTitle);
            timerCard.Controls.Add(lblTimer);
            centeredConfirmationCard.Controls.Add(timerCard);

            // --- E. Proceed and Exit Action Button ---
            btnProceed = new Button
            {
                Text = "PROCEED TO BILL",
                Location = new Point(30, 395),
                Size = new Size(360, 50),
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(245, 230, 211),
                FlatStyle = FlatStyle.Flat,
                Enabled = true,
                BackColor = Color.FromArgb(62, 39, 35),
                Cursor = Cursors.Hand
            };
            btnProceed.FlatAppearance.BorderSize = 0;
            btnProceed.Click += btnProceed_Click;

            btnProceed.HandleCreated += (s, e) => ApplyControlRounding(btnProceed, 15);
            centeredConfirmationCard.Controls.Add(btnProceed);

            centeredConfirmationCard.Paint += CenteredConfirmationCard_Paint;
        }

        private void StartCountdownTimer()
        {
            countdownTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (timeRemaining > TimeSpan.Zero)
            {
                timeRemaining = timeRemaining.Add(TimeSpan.FromSeconds(-1));
                lblTimer.Text = timeRemaining.ToString(@"mm\:ss");
            }
            else
            {
                countdownTimer.Stop();
                lblTimer.Text = "READY!";

                // Optional: Change color to green when ready
                lblTimer.ForeColor = Color.FromArgb(76, 175, 80);
            }
        }

        private void TableButton_Click(object sender, EventArgs e)
        {
            if (sender is Button clickedButton)
            {
                if (selectedTableButton == clickedButton)
                {
                    selectedTableButton.BackColor = Color.FromArgb(215, 204, 200);
                    selectedTableButton.ForeColor = Color.FromArgb(62, 39, 35);
                    selectedTableButton = null;
                    SelectedTableNumber = "None";
                    return;
                }

                if (selectedTableButton != null)
                {
                    selectedTableButton.BackColor = Color.FromArgb(215, 204, 200);
                    selectedTableButton.ForeColor = Color.FromArgb(62, 39, 35);
                }

                selectedTableButton = clickedButton;
                selectedTableButton.BackColor = Color.FromArgb(111, 78, 55);
                selectedTableButton.ForeColor = Color.White;

                SelectedTableNumber = selectedTableButton.Tag.ToString();
            }
        }

        private void btnProceed_Click(object sender, EventArgs e)
        {
            countdownTimer?.Stop();

            GenerateBillPage billForm = new GenerateBillPage
            {
                Subtotal = finalSubtotal,
                IsMember = isCustomerMember,
                ItemsList = finalItemsList,
                SelectedTable = SelectedTableNumber
            };

            // ========== FIX: Hide the confirmation and parent pages separately ==========
            this.Hide();
            if (parentOrderReviewForm != null && parentOrderReviewForm.Visible)
            {
                parentOrderReviewForm.Hide();
            }

            if (billForm.ShowDialog() == DialogResult.OK)
            {
                // Check if welcome page already exists
                CustomerStartingPage welcomePage = null;

                foreach (Form form in Application.OpenForms)
                {
                    if (form is CustomerStartingPage)
                    {
                        welcomePage = (CustomerStartingPage)form;
                        welcomePage.BringToFront();
                        welcomePage.Show();  // ========== ENSURE IT'S VISIBLE ==========
                        break;
                    }
                }

                // If no welcome page exists, create a new one
                if (welcomePage == null)
                {
                    welcomePage = new CustomerStartingPage();
                    welcomePage.Show();
                }

                // Close confirmation and review pages
                this.Close();
                if (parentOrderReviewForm != null)
                {
                    parentOrderReviewForm.Close();
                }
            }
            else
            {
                // User cancelled from bill page, show both pages again
                if (parentOrderReviewForm != null)
                {
                    parentOrderReviewForm.Show();
                }
                this.Show();
            }
        }

        private void CenteredConfirmationCard_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            Rectangle rect = new Rectangle(0, 0, centeredConfirmationCard.Width - 1, centeredConfirmationCard.Height - 1);

            using (GraphicsPath path = GetRoundedRectPath(rect, 25))
            {
                Color coffeeBorderColor = Color.FromArgb(62, 39, 35);
                using (Pen borderPen = new Pen(coffeeBorderColor, 3))
                {
                    e.Graphics.DrawPath(borderPen, path);
                }
            }
        }

        private void ApplyControlRounding(Control control, int radius)
        {
            if (control == null || control.Width <= 0 || control.Height <= 0) return;

            using (GraphicsPath path = GetRoundedRectPath(new Rectangle(0, 0, control.Width, control.Height), radius))
            {
                control.Region = new Region(path);
            }
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
    }
}