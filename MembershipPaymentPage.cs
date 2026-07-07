using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace Buzzing_Beans
{
    public partial class MembershipPaymentPage : Form
    {
        private  Panel mainPanel;
        private  Panel paymentContainerStack;

        // Form Controls
        private Button btnCancel;
        private Label lblTitle;
        private Label lblSubtitle;

        // Premium Info Box
        private Panel pnlPremiumCard;
        private Label lblCardTitle;
        private Label lblFeature1;
        private Label lblFeature2;
        private Label lblFeeLabel;
        private Label lblFeeAmount;

        // QR Presentation Elements
        private Label lblScanQrText;
        private Panel picQrContainer;
        private PictureBox picQR;
        private Label lblPaymentMethods;
        private Button btnActivate;

        // Background Image Reference
        private Image mainBgImage;
        private string pendingMemberName = "";
        private string pendingMemberPhone = "";

        // Custom Mouse Dragging Variables for Window Movement
        private bool isDragging = false;
        private Point dragStartPoint = new Point(0, 0);

        // Constructor with member info (called from MemberRegistrationPage)
        public MembershipPaymentPage(string memberName, string memberPhone)
        {
            pendingMemberName = memberName;
            pendingMemberPhone = memberPhone;
            InitializePaymentPage();
        }

        // Default constructor (for backward compatibility)
        public MembershipPaymentPage() : this("", "")
        {
        }

        private void InitializePaymentPage()
        {
            this.SuspendLayout();

            // ========== 1. UNIFIED BORDERLESS WINDOW SETUP ==========
            this.Text = "Become a Member";
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Explicitly shares the identical dimensions of previous steps
            this.Size = new Size(440, 580);
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            this.BackColor = Color.FromArgb(245, 245, 220);

            // Load Background Asset Safely
            LoadBackgroundImage();

            // ========== 2. DYNAMIC MAIN BASE & SURFACE DRAGGING ==========
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

            mainPanel.MouseDown += Window_MouseDown;
            mainPanel.MouseMove += Window_MouseMove;
            mainPanel.MouseUp += Window_MouseUp;
            this.Controls.Add(mainPanel);

            // ========== 3. TOP ACTION CONTROLS (CANCEL BUTTON) ==========
            btnCancel = new Button
            {
                Text = "← Cancel",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 54, 33),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(25, 20),
                AutoSize = true
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnCancel.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnCancel.Click += BtnCancel_Click;
            mainPanel.Controls.Add(btnCancel);

            // ========== 4. CENTRAL DISPLAY STACK OBJECTS ==========
            paymentContainerStack = new Panel
            {
                Size = new Size(360, 490),
                Location = new Point(40, 60),
                BackColor = Color.Transparent
            };
            mainPanel.Controls.Add(paymentContainerStack);

            // --- A. Main Screen Header ---
            lblTitle = new Label
            {
                Text = "JOIN THE ELITE CLUB",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(62, 39, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(360, 35),
                Location = new Point(0, 0)
            };
            paymentContainerStack.Controls.Add(lblTitle);

            // --- B. Screen Subheading ---
            lblSubtitle = new Label
            {
                Text = "Scan to activate your membership",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(141, 110, 99),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(360, 20),
                Location = new Point(0, lblTitle.Bottom + 2)
            };
            paymentContainerStack.Controls.Add(lblSubtitle);

            // --- C. Espresso Premium Membership Info Card Container ---
            pnlPremiumCard = new Panel
            {
                Size = new Size(350, 140),
                Location = new Point(5, lblSubtitle.Bottom + 12),
                BackColor = Color.FromArgb(62, 39, 35)
            };
            ApplyPremiumCardRounding(pnlPremiumCard);
            paymentContainerStack.Controls.Add(pnlPremiumCard);

            lblCardTitle = new Label
            {
                Text = "Premium Membership",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(245, 230, 211),
                Location = new Point(15, 12),
                Size = new Size(320, 25),
                BackColor = Color.Transparent
            };
            pnlPremiumCard.Controls.Add(lblCardTitle);

            // Bullet Point Benefit 1
            lblFeature1 = new Label
            {
                Text = "•  Flat 10% Discount on every order",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(215, 204, 200),
                Location = new Point(20, lblCardTitle.Bottom + 12),
                Size = new Size(310, 20),
                BackColor = Color.Transparent
            };
            pnlPremiumCard.Controls.Add(lblFeature1);

            // Bullet Point Benefit 2
            lblFeature2 = new Label
            {
                Text = "•  Exclusive access to seasonal items",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(215, 204, 200),
                Location = new Point(20, lblFeature1.Bottom + 4),
                Size = new Size(310, 20),
                BackColor = Color.Transparent
            };
            pnlPremiumCard.Controls.Add(lblFeature2);

            // One-Time Fee Meta Tags
            lblFeeLabel = new Label
            {
                Text = "ONE-TIME FEE",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(245, 230, 211),
                Location = new Point(15, lblFeature2.Bottom + 16),
                Size = new Size(150, 20),
                BackColor = Color.Transparent
            };
            pnlPremiumCard.Controls.Add(lblFeeLabel);

            lblFeeAmount = new Label
            {
                Text = "RS. 500",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(245, 230, 211),
                Location = new Point(200, lblFeature2.Bottom + 13),
                Size = new Size(135, 25),
                TextAlign = ContentAlignment.TopRight,
                BackColor = Color.Transparent
            };
            pnlPremiumCard.Controls.Add(lblFeeAmount);

            // --- D. Scan Title Target ---
            lblScanQrText = new Label
            {
                Text = "SCAN QR CODE",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(62, 39, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(360, 20),
                Location = new Point(0, pnlPremiumCard.Bottom + 15)
            };
            paymentContainerStack.Controls.Add(lblScanQrText);

            // --- E. White Rounded Container for QR Image ---
            picQrContainer = new Panel
            {
                Size = new Size(150, 150),
                Location = new Point(105, lblScanQrText.Bottom + 5),
                BackColor = Color.White
            };
            ApplyInputBoxRounding(picQrContainer);

            picQR = new PictureBox
            {
                Size = new Size(130, 130),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };

            // Load QR image from multiple possible locations
            LoadQRImage();

            picQrContainer.Controls.Add(picQR);
            paymentContainerStack.Controls.Add(picQrContainer);

            // --- F. Accepted Methods Subscript ---
            lblPaymentMethods = new Label
            {
                Text = "Pay via EasyPaisa or JazzCash",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(141, 110, 99),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(360, 20),
                Location = new Point(0, picQrContainer.Bottom + 8)
            };
            paymentContainerStack.Controls.Add(lblPaymentMethods);

            // --- G. Submit Verification Confirmation Activation Button ---
            btnActivate = new Button
            {
                Size = new Size(350, 45),
                Location = new Point(5, lblPaymentMethods.Bottom + 10),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(62, 39, 35),
                ForeColor = Color.FromArgb(245, 230, 211)
            };
            btnActivate.FlatAppearance.BorderSize = 0;
            btnActivate.Click += BtnActivate_Click;
            ApplyButtonStyling(btnActivate, "I HAVE PAID - ACTIVATE NOW");
            paymentContainerStack.Controls.Add(btnActivate);

            // Hook up clean espresso outer form frame paint logic
            this.Paint += MembershipPaymentPage_Paint;

            this.ResumeLayout(false);
        }

        private void LoadBackgroundImage()
        {
            string[] possiblePaths = {
                @"D:\Kisfa Javed\SDA\Menu\background.png",
                Path.Combine(Application.StartupPath, "Images", "background.png"),
                Path.Combine(Application.StartupPath, "Resources", "background.png")
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    try { mainBgImage = Image.FromFile(path); break; }
                    catch { }
                }
            }
        }

        private void LoadQRImage()
        {
            string[] possiblePaths = {
                @"D:\Kisfa Javed\SDA\Menu\QR.jpeg",
                Path.Combine(Application.StartupPath, "Images", "QR.png"),
                Path.Combine(Application.StartupPath, "Resources", "QR.png")
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    try { picQR.Image = Image.FromFile(path); break; }
                    catch { }
                }
            }
        }

        // ========== Custom Outer Canvas High Contrast Separation Painter ==========
        private void MembershipPaymentPage_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (GraphicsPath path = GetRoundedRectPath(rect, 30))
            {
                this.Region = new Region(path);
                using (Pen borderPen = new Pen(Color.FromArgb(62, 39, 35), 3))
                {
                    e.Graphics.DrawPath(borderPen, path);
                }
            }
        }

        // ========== Native Win32 Drop Shadow Rendering Context ==========
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

        // ========== Seamless Click and Drag Window Control Logic Handlers ==========
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

        // ========== Subcomponent Interface Corner Rounding Utility Extensions ==========
        private void ApplyPremiumCardRounding(Panel panel)
        {
            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                Rectangle rect = new Rectangle(0, 0, panel.Width, panel.Height);
                using (GraphicsPath path = GetRoundedRectPath(rect, 20))
                {
                    panel.Region = new Region(path);

                    // Draw clean crisp separator line horizontally across card layout
                    using (Pen sepPen = new Pen(Color.FromArgb(50, 215, 204, 200), 1))
                    {
                        e.Graphics.DrawLine(sepPen, 15, 42, panel.Width - 15, 42);
                        e.Graphics.DrawLine(sepPen, 15, 100, panel.Width - 15, 100);
                    }
                }
            };
        }

        private void ApplyInputBoxRounding(Panel panel)
        {
            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                Rectangle rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                using (GraphicsPath path = GetRoundedRectPath(rect, 15))
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

                    TextRenderer.DrawText(e.Graphics, labelText, new Font("Segoe UI", 10, FontStyle.Bold), rect, btn.ForeColor,
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

        // ========== VERIFICATION FLOW EVENT HANDLING ROUTINES ==========
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void BtnActivate_Click(object sender, EventArgs e)
        {
            // If we have pending member info from registration, update points
            if (!string.IsNullOrEmpty(pendingMemberName) && !string.IsNullOrEmpty(pendingMemberPhone))
            {
                // Member is already saved in MemberRegistrationPage
                // Just show success message
                MessageBox.Show($"Payment Verified! Welcome {pendingMemberName} to the Elite Club!\n\nYour 10% member discount is now active.\n\nYou can now log in using your Member ID.",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Payment Verified! Welcome to the Elite Club.\n\nYour 10% member discount is now active.\n\nPlease complete your registration to get your Member ID.",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}