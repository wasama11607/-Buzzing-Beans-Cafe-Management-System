using Buzzing_Beans;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Buzzing_Beans
{
    public partial class MemeberLoginPage : Form
    {
        private readonly Panel mainPanel;
        private readonly Panel loginContainerStack;

        private readonly Button btnBack;
        private readonly PictureBox picMemberIcon;
        private readonly Label lblTitle;
        private readonly Label lblSubtitle;
        private readonly Label lblMemberId;
        private readonly TextBox txtUsername;
        private readonly Label lblPassword;
        private readonly TextBox txtPassword;
        private readonly Button btnLogin;
        private readonly Button btnRegister;

        private readonly Panel txtUsernameContainer;
        private readonly Panel txtPasswordContainer;

        private Image mainBgImage;

        private bool isDragging = false;
        private Point dragStartPoint = new Point(0, 0);

        // ========== AGGRESSIVE FLICKER FIX: Pre-render regions ==========
        private GraphicsPath formRoundedPath;
        private GraphicsPath usernameRoundedPath;
        private GraphicsPath passwordRoundedPath;
        private GraphicsPath loginButtonRoundedPath;

        public string LoggedInMemberId { get; private set; } = "";

        public MemeberLoginPage()
        {
            // ========== FLICKER FIX 1: Enable maximum double buffering ==========
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                  ControlStyles.UserPaint |
                  ControlStyles.OptimizedDoubleBuffer |
                  ControlStyles.DoubleBuffer, true);
            this.UpdateStyles();

            this.SuspendLayout();

            this.Text = "Member Login";
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(440, 580);
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            this.BackColor = Color.FromArgb(245, 245, 220);

            // ========== FLICKER FIX 2: Pre-calculate all rounded paths ==========
            RecalculatePaths();

            try
            {
                mainBgImage = Image.FromFile(@"D:\Kisfa Javed\SDA\Menu\background.png");
            }
            catch { }

            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            // Enable double buffering via reflection for child controls
            EnableDoubleBuffering(mainPanel);

            if (mainBgImage != null)
            {
                mainPanel.BackgroundImage = mainBgImage;
                mainPanel.BackgroundImageLayout = ImageLayout.Stretch;
            }

            mainPanel.MouseDown += Window_MouseDown;
            mainPanel.MouseMove += Window_MouseMove;
            mainPanel.MouseUp += Window_MouseUp;
            this.Controls.Add(mainPanel);

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
            btnBack.Click += BtnBack_Click;
            mainPanel.Controls.Add(btnBack);

            loginContainerStack = new Panel
            {
                Size = new Size(360, 450),
                Location = new Point(40, 85),
                BackColor = Color.Transparent
            };
            EnableDoubleBuffering(loginContainerStack);
            mainPanel.Controls.Add(loginContainerStack);

            picMemberIcon = new PictureBox
            {
                Size = new Size(360, 70),
                Location = new Point(0, 0),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            try { picMemberIcon.Image = Image.FromFile(@"D:\Kisfa Javed\SDA\Menu\Member.png"); } catch { }
            loginContainerStack.Controls.Add(picMemberIcon);

            lblTitle = new Label
            {
                Text = "MEMBER LOGIN",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(62, 39, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(360, 45),
                Location = new Point(0, picMemberIcon.Bottom + 5)
            };
            loginContainerStack.Controls.Add(lblTitle);

            lblSubtitle = new Label
            {
                Text = "Access your exclusive discounts",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(141, 110, 99),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(360, 25),
                Location = new Point(0, lblTitle.Bottom)
            };
            loginContainerStack.Controls.Add(lblSubtitle);

            lblMemberId = new Label
            {
                Text = "Member ID",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 54, 33),
                Size = new Size(350, 25),
                Location = new Point(5, lblSubtitle.Bottom + 20)
            };
            loginContainerStack.Controls.Add(lblMemberId);

            txtUsernameContainer = new Panel
            {
                Size = new Size(350, 40),
                Location = new Point(5, lblMemberId.Bottom + 2),
                BackColor = Color.White
            };
            EnableDoubleBuffering(txtUsernameContainer);
            txtUsername = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 12),
                Width = 330,
                Location = new Point(10, 10),
                BackColor = Color.White,
            };
            txtUsernameContainer.Controls.Add(txtUsername);
            ConfigureInputPanel(txtUsernameContainer, true);
            loginContainerStack.Controls.Add(txtUsernameContainer);

            lblPassword = new Label
            {
                Text = "Password",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 54, 33),
                Size = new Size(350, 25),
                Location = new Point(5, txtUsernameContainer.Bottom + 12)
            };
            loginContainerStack.Controls.Add(lblPassword);

            txtPasswordContainer = new Panel
            {
                Size = new Size(350, 40),
                Location = new Point(5, lblPassword.Bottom + 2),
                BackColor = Color.White
            };
            EnableDoubleBuffering(txtPasswordContainer);
            txtPassword = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 12),
                Width = 330,
                Location = new Point(10, 10),
                UseSystemPasswordChar = true,
                BackColor = Color.White,
            };
            txtPasswordContainer.Controls.Add(txtPassword);
            ConfigureInputPanel(txtPasswordContainer, false);
            loginContainerStack.Controls.Add(txtPasswordContainer);

            btnLogin = new Button
            {
                Size = new Size(350, 45),
                Location = new Point(5, txtPasswordContainer.Bottom + 25),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(62, 39, 35),
                ForeColor = Color.FromArgb(245, 230, 211)
            };
            EnableDoubleBuffering(btnLogin);
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            ConfigureLoginButton(btnLogin, "SIGN IN");
            loginContainerStack.Controls.Add(btnLogin);

            btnRegister = new Button
            {
                Text = "Not a member? Join now!",
                Font = new Font("Segoe UI", 11, FontStyle.Underline),
                ForeColor = Color.FromArgb(139, 69, 19),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Size = new Size(350, 30),
                Location = new Point(5, btnLogin.Bottom + 12),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnRegister.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnRegister.Click += BtnRegister_Click;
            loginContainerStack.Controls.Add(btnRegister);

            this.Load += (s, e) => { };

            this.Paint += MemberLoginPage_Paint;

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

        // ========== FLICKER FIX 5: Pre-calculate all GraphicsPaths once ==========
        private void RecalculatePaths()
        {
            Rectangle formRect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            Rectangle usernameRect = new Rectangle(0, 0, 350 - 1, 40 - 1);
            Rectangle passwordRect = new Rectangle(0, 0, 350 - 1, 40 - 1);
            Rectangle buttonRect = new Rectangle(0, 0, 350 - 1, 45 - 1);

            formRoundedPath = GetRoundedRectPath(formRect, 30);
            usernameRoundedPath = GetRoundedRectPath(usernameRect, 10);
            passwordRoundedPath = GetRoundedRectPath(passwordRect, 10);
            loginButtonRoundedPath = GetRoundedRectPath(buttonRect, 15);
        }

        // ========== FLICKER FIX 6: Use pre-calculated paths, don't recreate in Paint ==========
        private void MemberLoginPage_Paint(object sender, PaintEventArgs e)
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

        // ========== FLICKER FIX 7: Use WS_EX_COMPOSITED ==========
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

        // ========== FLICKER FIX 8: Minimal paint operations in input panels ==========
        private void ConfigureInputPanel(Panel panel, bool isUsername)
        {
            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

                GraphicsPath path = isUsername ? usernameRoundedPath : passwordRoundedPath;

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

        // ========== FLICKER FIX 9: Minimal button painting ==========
        private void ConfigureLoginButton(Button btn, string labelText)
        {
            bool isHovered = false;

            btn.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

                if (loginButtonRoundedPath != null)
                {
                    btn.Region = new Region(loginButtonRoundedPath);

                    Color currentBg = isHovered ? Color.FromArgb(93, 64, 55) : Color.FromArgb(62, 39, 35);

                    using (SolidBrush bgBrush = new SolidBrush(currentBg))
                    {
                        e.Graphics.FillPath(bgBrush, loginButtonRoundedPath);
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

        private void BtnBack_Click(object sender, EventArgs e)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is CustomerStartingPage)
                {
                    form.Show();
                    break;
                }
            }
            this.Close();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string memberId = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(memberId) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both your Member ID and Password.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (FileHelper.ValidateMember(memberId, password))
            {
                LoggedInMemberId = memberId;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid Member ID or Password!\nPlease check your credentials or register as a new member.",
                    "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            this.Hide();

            using (MemberRegistrationPage registrationPage = new MemberRegistrationPage())
            {
                registrationPage.ShowDialog(this);
            }

            this.Show();
        }
    }
}