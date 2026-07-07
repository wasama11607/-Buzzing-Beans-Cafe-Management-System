using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace Buzzing_Beans
{
    public partial class MemberRegistrationPage : Form
    {
        private readonly Panel mainPanel;
        private readonly Panel signupContainerStack;

        // Form Controls
        private readonly Button btnBack;
        private readonly Label lblTitle;
        private readonly Label lblSubtitle;

        private readonly Label lblName;
        private readonly TextBox txtName;
        private readonly Label lblEmail;
        private readonly TextBox txtEmail;
        private readonly Label lblPassword;
        private readonly TextBox txtPassword;
        private readonly Label lblConfirmPassword;
        private readonly TextBox txtConfirmPassword;

        private readonly Button btnRegister;
        private readonly Label lblTerms;

        // Visual Panels for Rounded Input Fields
        private readonly Panel txtNameContainer;
        private readonly Panel txtEmailContainer;
        private readonly Panel txtPasswordContainer;
        private readonly Panel txtConfirmPasswordContainer;

        // Background Image Reference
        private Image mainBgImage;

        // Custom Mouse Dragging Variables for Window Movement
        private bool isDragging = false;
        private Point dragStartPoint = new Point(0, 0);

        // ========== AGGRESSIVE FLICKER FIX: Pre-render regions ==========
        private GraphicsPath formRoundedPath;
        private GraphicsPath nameRoundedPath;
        private GraphicsPath emailRoundedPath;
        private GraphicsPath passwordRoundedPath;
        private GraphicsPath confirmPasswordRoundedPath;
        private GraphicsPath registerButtonRoundedPath;

        public MemberRegistrationPage()
        {
            // ========== FLICKER FIX 1: Enable maximum double buffering ==========
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                  ControlStyles.UserPaint |
                  ControlStyles.OptimizedDoubleBuffer |
                  ControlStyles.DoubleBuffer, true);
            this.UpdateStyles();

            this.SuspendLayout();

            // ========== 1. MATCHING BORDERLESS WINDOW FRAME SYSTEM ==========
            this.Text = "Join the Club";
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Matches the exact same static footprint aspect ratios as MemberLoginPage
            this.Size = new Size(440, 580);
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            this.BackColor = Color.FromArgb(245, 245, 220);

            // ========== FLICKER FIX 2: Pre-calculate all rounded paths ==========
            RecalculatePaths();

            // Load Background Asset Safely
            try
            {
                mainBgImage = Image.FromFile(@"D:\Kisfa Javed\SDA\Menu\background.png");
            }
            catch { /* Fallback background handled gracefully */ }

            // ========== 2. MAIN CONTAINER PANEL & SEAMLESS DRAG INTERACTION ==========
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            EnableDoubleBuffering(mainPanel);

            if (mainBgImage != null)
            {
                mainPanel.BackgroundImage = mainBgImage;
                mainPanel.BackgroundImageLayout = ImageLayout.Stretch;
            }

            // Bind mouse events directly to the panel background for uniform dragging mechanics
            mainPanel.MouseDown += Window_MouseDown;
            mainPanel.MouseMove += Window_MouseMove;
            mainPanel.MouseUp += Window_MouseUp;
            this.Controls.Add(mainPanel);

            // ========== 3. TOP NAVIGATION (BACK TO LOGIN) ==========
            btnBack = new Button
            {
                Text = "← Back to Login",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 54, 33),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(25, 20),
                AutoSize = true
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnBack.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnBack.Click += BtnBack_Click;
            mainPanel.Controls.Add(btnBack);

            // ========== 4. STRUCTURED LAYOUT STRUCT CONTAINER ==========
            signupContainerStack = new Panel
            {
                Size = new Size(360, 480),
                Location = new Point(40, 65),
                BackColor = Color.Transparent
            };
            EnableDoubleBuffering(signupContainerStack);
            mainPanel.Controls.Add(signupContainerStack);

            // --- A. Screen Main Heading ---
            lblTitle = new Label
            {
                Text = "BECOME A MEMBER",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(62, 39, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(360, 40),
                Location = new Point(0, 0)
            };
            signupContainerStack.Controls.Add(lblTitle);

            // --- B. Screen Subtitle ---
            lblSubtitle = new Label
            {
                Text = "Sign up to start earning rewards",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(141, 110, 99),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(360, 20),
                Location = new Point(0, lblTitle.Bottom)
            };
            signupContainerStack.Controls.Add(lblSubtitle);

            // --- C. Full Name Field ---
            lblName = new Label
            {
                Text = "Full Name",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 54, 33),
                Size = new Size(350, 20),
                Location = new Point(5, lblSubtitle.Bottom + 12)
            };
            signupContainerStack.Controls.Add(lblName);

            txtNameContainer = new Panel { Size = new Size(350, 36), Location = new Point(5, lblName.Bottom + 2), BackColor = Color.White };
            EnableDoubleBuffering(txtNameContainer);
            txtName = new TextBox { BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 11), Width = 330, Location = new Point(10, 8), BackColor = Color.White };
            txtNameContainer.Controls.Add(txtName);
            ConfigureInputPanel(txtNameContainer, 0);
            signupContainerStack.Controls.Add(txtNameContainer);

            // --- D. Email Field ---
            lblEmail = new Label
            {
                Text = "Email Address",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 54, 33),
                Size = new Size(350, 20),
                Location = new Point(5, txtNameContainer.Bottom + 8)
            };
            signupContainerStack.Controls.Add(lblEmail);

            txtEmailContainer = new Panel { Size = new Size(350, 36), Location = new Point(5, lblEmail.Bottom + 2), BackColor = Color.White };
            EnableDoubleBuffering(txtEmailContainer);
            txtEmail = new TextBox { BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 11), Width = 330, Location = new Point(10, 8), BackColor = Color.White };

            // ========== EMAIL VALIDATION ==========
            txtEmail.TextChanged += (s, e) =>
            {
                ValidateEmailFormat(txtEmail);
            };

            txtEmailContainer.Controls.Add(txtEmail);
            ConfigureInputPanel(txtEmailContainer, 1);
            signupContainerStack.Controls.Add(txtEmailContainer);


            // --- E. Password Field ---
            lblPassword = new Label
            {
                Text = "Create Password",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 54, 33),
                Size = new Size(350, 20),
                Location = new Point(5, txtEmailContainer.Bottom + 8)
            };
            signupContainerStack.Controls.Add(lblPassword);

            txtPasswordContainer = new Panel { Size = new Size(350, 36), Location = new Point(5, lblPassword.Bottom + 2), BackColor = Color.White };
            EnableDoubleBuffering(txtPasswordContainer);
            txtPassword = new TextBox { BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 11), Width = 330, Location = new Point(10, 8), UseSystemPasswordChar = true, BackColor = Color.White };
            txtPasswordContainer.Controls.Add(txtPassword);
            ConfigureInputPanel(txtPasswordContainer, 2);
            signupContainerStack.Controls.Add(txtPasswordContainer);

            // --- F. Confirm Password Field ---
            lblConfirmPassword = new Label
            {
                Text = "Confirm Password",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 54, 33),
                Size = new Size(350, 20),
                Location = new Point(5, txtPasswordContainer.Bottom + 8)
            };
            signupContainerStack.Controls.Add(lblConfirmPassword);

            txtConfirmPasswordContainer = new Panel { Size = new Size(350, 36), Location = new Point(5, lblConfirmPassword.Bottom + 2), BackColor = Color.White };
            EnableDoubleBuffering(txtConfirmPasswordContainer);
            txtConfirmPassword = new TextBox { BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 11), Width = 330, Location = new Point(10, 8), UseSystemPasswordChar = true, BackColor = Color.White };
            txtConfirmPasswordContainer.Controls.Add(txtConfirmPassword);
            ConfigureInputPanel(txtConfirmPasswordContainer, 3);
            signupContainerStack.Controls.Add(txtConfirmPasswordContainer);

            // --- G. Custom Sign-Up Submission Button ---
            btnRegister = new Button
            {
                Size = new Size(350, 45),
                Location = new Point(5, txtConfirmPasswordContainer.Bottom + 20),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(62, 39, 35),
                ForeColor = Color.FromArgb(245, 230, 211)
            };
            EnableDoubleBuffering(btnRegister);
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;
            ApplyButtonStyling(btnRegister, "CREATE ACCOUNT");
            signupContainerStack.Controls.Add(btnRegister);

            // --- H. Footer Terms Legal Disclaimer ---
            lblTerms = new Label
            {
                Text = "By signing up, you agree to our Café Terms.",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(161, 136, 127),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(350, 25),
                Location = new Point(5, btnRegister.Bottom + 10)
            };
            signupContainerStack.Controls.Add(lblTerms);

            // Apply sharp espresso frame layout structure bounding paths
            this.Paint += MemberRegistrationPage_Paint;

            this.ResumeLayout(false);
        }

        // ========== HELPER: Enable double buffering via reflection ==========
        private void EnableDoubleBuffering(Control control)
        {
            var type = control.GetType();
            var property = type.GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (property != null && property.CanWrite)
            {
                property.SetValue(control, true);
            }
        }

        // ========== FLICKER FIX 3: Pre-calculate all GraphicsPaths once ==========
        private void RecalculatePaths()
        {
            Rectangle formRect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            Rectangle inputRect = new Rectangle(0, 0, 350 - 1, 36 - 1);
            Rectangle buttonRect = new Rectangle(0, 0, 350 - 1, 45 - 1);

            formRoundedPath = GetRoundedRectPath(formRect, 30);
            nameRoundedPath = GetRoundedRectPath(inputRect, 10);
            emailRoundedPath = GetRoundedRectPath(inputRect, 10);
            passwordRoundedPath = GetRoundedRectPath(inputRect, 10);
            confirmPasswordRoundedPath = GetRoundedRectPath(inputRect, 10);
            registerButtonRoundedPath = GetRoundedRectPath(buttonRect, 15);
        }

        // ========== FLICKER FIX 4: Use pre-calculated paths, don't recreate in Paint ==========
        private void MemberRegistrationPage_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            if (formRoundedPath != null)
            {
                this.Region = new Region(formRoundedPath);

                Color coffeeBorderColor = Color.FromArgb(62, 39, 35);
                using (Pen borderPen = new Pen(coffeeBorderColor, 3))
                {
                    e.Graphics.DrawPath(borderPen, formRoundedPath);
                }
            }
        }

        // ========== Custom OS Floating Frame Shadows Overlay ==========
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED - Forces hardware composition
                const int CS_DROPSHADOW = 0x20000;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        // ========== Form Dragging Execution Control Core Hooks ==========
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

        // ========== FLICKER FIX 5: Minimal paint operations in input panels ==========
        private void ConfigureInputPanel(Panel panel, int panelIndex)
        {
            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

                GraphicsPath path;
                if (panelIndex == 0)
                    path = nameRoundedPath;
                else if (panelIndex == 1)
                    path = emailRoundedPath;
                else if (panelIndex == 2)
                    path = passwordRoundedPath;
                else if (panelIndex == 3)
                    path = confirmPasswordRoundedPath;
                else
                    path = nameRoundedPath;

                if (path != null)
                {
                    panel.Region = new Region(path);
                    using (Pen borderPen = new Pen(Color.FromArgb(215, 204, 200), 1))
                    {
                        e.Graphics.DrawPath(borderPen, path);
                    }
                }
            };
        }
        // ========== EMAIL FORMAT VALIDATION ==========
        private void ValidateEmailFormat(TextBox emailBox)
        {
            string email = emailBox.Text.Trim();

            // Allow empty input (user is still typing)
            if (string.IsNullOrEmpty(email))
            {
                emailBox.BackColor = Color.White;
                return;
            }

            // Check if email matches correct format: example@domain.com
            if (IsValidEmail(email))
            {
                emailBox.BackColor = Color.White; // Valid - white background
            }
            else
            {
                emailBox.BackColor = Color.FromArgb(255, 220, 220); // Invalid - light red background
            }
        }

        // ========== EMAIL FORMAT CHECKER ==========
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        // ========== FLICKER FIX 6: Minimal button painting ==========
        private void ApplyButtonStyling(Button btn, string labelText)
        {
            bool isHovered = false;

            btn.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

                if (registerButtonRoundedPath != null)
                {
                    btn.Region = new Region(registerButtonRoundedPath);

                    Color currentBg = isHovered ? Color.FromArgb(93, 64, 55) : Color.FromArgb(62, 39, 35);

                    using (SolidBrush bgBrush = new SolidBrush(currentBg))
                    {
                        e.Graphics.FillPath(bgBrush, registerButtonRoundedPath);
                    }

                    TextRenderer.DrawText(e.Graphics, labelText, new Font("Segoe UI", 11, FontStyle.Bold),
                        new Rectangle(0, 0, btn.Width, btn.Height), btn.ForeColor,
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

        // ========== APPLICATION USER TRANSITION FLOW HANDLING ==========
        private void BtnBack_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtEmail.Text.Trim();
            string pass = txtPassword.Text.Trim();
            string confirmPass = txtConfirmPassword.Text.Trim();

            // Validation
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(confirmPass))
            {
                MessageBox.Show("Please fill out all registration entry fields.", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (pass != confirmPass)
            {
                MessageBox.Show("Password fields do not match! Please check details again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if member already exists
            if (FileHelper.MemberExists(name, email))
            {
                MessageBox.Show("A member with this name or email already exists!\nPlease use different credentials.",
                    "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Save member to file
            string memberId = FileHelper.AddMember(name, phone, pass);

            MessageBox.Show($"Account created successfully!\n\nYour Member ID: {memberId}\nPlease save this ID for future logins.\n\nYou will now be redirected to the payment page.",
                "Registration Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Hide the registration form
            this.Hide();

            // Launch payment page
            using (MembershipPaymentPage paymentPage = new MembershipPaymentPage(name, phone))
            {
                var result = paymentPage.ShowDialog(this);

                if (result == DialogResult.OK)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    // If they cancel payment, bring them back to registration
                    this.Show();
                }
            }
        }
    }
}