using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Label = System.Windows.Forms.Label;

namespace Buzzing_Beans
{
    public partial class LoginForm : Form
    {
        // Controls
        private Panel pnlMainContainer;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnBack;
        private Label lblTitle, lblUsername, lblPassword;

        // Resources
        private Image imgLogo;
        private Image imgBackground;

        // Top Banner Height Config
        private const int BANNER_HEIGHT = 220;

        public LoginForm()
        {
            InitializeComponents();

            // Ensure data folder exists
            FileHelper.EnsureDataFolder();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.Resize += (s, e) => LayoutControls();
        }

        private void InitializeComponents()
        {
            // --- FORM SETUP ---
            this.Text = "Login - Buzzing Beans";
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1024, 720);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.FromArgb(239, 224, 205);
            this.DoubleBuffered = true;

            // --- BACK BUTTON SETUP ---
            btnBack = new Button
            {
                Text = "← Back",
                Size = new Size(95, 38),
                Location = new Point(20, 20),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(62, 39, 35),
                ForeColor = Color.FromArgb(245, 230, 211),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 0;

            btnBack.MouseEnter += (s, e) => btnBack.BackColor = Color.FromArgb(93, 64, 55);
            btnBack.MouseLeave += (s, e) => btnBack.BackColor = Color.FromArgb(62, 39, 35);
            btnBack.Click += BtnBack_Click;
            btnBack.Paint += (s, e) => RoundControl(btnBack, e, 16);

            // Main UI Input Container Panel
            pnlMainContainer = new Panel
            {
                Size = new Size(550, 360),
                BackColor = Color.FromArgb(180, 245, 230, 211)
            };
            pnlMainContainer.Paint += PnlMainContainer_Paint;

            lblTitle = new Label
            {
                Text = "LOGIN",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(62, 39, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(550, 60),
                Location = new Point(0, 15),
                BackColor = Color.Transparent
            };

            lblUsername = new Label
            {
                Text = "Employee ID",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(141, 110, 99),
                Location = new Point(75, 90),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            txtUsername = new TextBox
            {
                Font = new Font("Segoe UI", 14),
                Size = new Size(400, 35),
                Location = new Point(75, 120),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            lblPassword = new Label
            {
                Text = "Password",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(141, 110, 99),
                Location = new Point(75, 170),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            txtPassword = new TextBox
            {
                Font = new Font("Segoe UI", 14),
                Size = new Size(400, 35),
                Location = new Point(75, 200),
                UseSystemPasswordChar = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            btnLogin = new Button
            {
                Text = "Login",
                Size = new Size(180, 50),
                Location = new Point(185, 275),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(62, 39, 35),
                ForeColor = Color.FromArgb(245, 230, 211),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnLogin.MouseEnter += (s, e) => {
                btnLogin.BackColor = Color.FromArgb(93, 64, 55);
                btnLogin.Size = new Size(190, 55);
                btnLogin.Location = new Point(180, 272);
            };

            btnLogin.MouseLeave += (s, e) => {
                btnLogin.BackColor = Color.FromArgb(62, 39, 35);
                btnLogin.Size = new Size(180, 50);
                btnLogin.Location = new Point(185, 275);
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            btnLogin.Paint += (s, e) => RoundControl(btnLogin, e, 25);

            pnlMainContainer.Controls.Add(lblTitle);
            pnlMainContainer.Controls.Add(lblUsername);
            pnlMainContainer.Controls.Add(txtUsername);
            pnlMainContainer.Controls.Add(lblPassword);
            pnlMainContainer.Controls.Add(txtPassword);
            pnlMainContainer.Controls.Add(btnLogin);

            this.Controls.Add(btnBack);
            this.Controls.Add(pnlMainContainer);
            this.Paint += LoginForm_Paint;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadImagesSafely();
            LayoutControls();
        }

        private void LoadImagesSafely()
        {
            string path = Path.Combine(Application.StartupPath, "Resources");
            imgLogo = LoadImageNoLock(Path.Combine(path, "View Menu.png"));
            imgBackground = LoadImageNoLock(Path.Combine(path, "background.png"));
        }

        private Image LoadImageNoLock(string path)
        {
            if (!File.Exists(path)) return null;
            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    return Image.FromStream(fs);
            }
            catch { return null; }
        }

        private void LayoutControls()
        {
            int centerX = (this.ClientSize.Width - pnlMainContainer.Width) / 2;
            int remainingSpaceHeight = this.ClientSize.Height - BANNER_HEIGHT;
            int centerY = BANNER_HEIGHT + ((remainingSpaceHeight - pnlMainContainer.Height) / 2);

            pnlMainContainer.Location = new Point(centerX, centerY);

            this.Invalidate();
        }

        private void PnlMainContainer_Paint(object sender, PaintEventArgs e)
        {
            RoundControl(pnlMainContainer, e, 30);
        }

        private void LoginForm_Paint(object sender, PaintEventArgs e)
        {
            if (imgBackground != null)
            {
                e.Graphics.DrawImage(imgBackground, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            }

            if (imgLogo != null)
            {
                e.Graphics.DrawImage(imgLogo, 0, 0, this.ClientSize.Width, BANNER_HEIGHT);
            }
        }

        private void RoundControl(Control ctrl, PaintEventArgs e, int radius)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, ctrl.Width, ctrl.Height);
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();
                ctrl.Region = new Region(path);
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both Employee ID and Password.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate against employees.txt
            var employees = FileHelper.ReadEmployees();

            foreach (var emp in employees)
            {
                if (emp.Length >= 4)
                {
                    string empId = emp[0];
                    string empPassword = emp[4]; // Password is at index 4
                    string empRole = emp[3];     // Role is at index 3

                    if (empId == username && empPassword == password)
                    {
                        // Login successful
                        //if (empRole == "Manager" || empRole == "System Admin" || empRole == "system admin" || empRole == "manager")
                        //{
                        //    ManagerDashboard managerDashboard = new ManagerDashboard();
                        //    this.Hide();
                        //    managerDashboard.FormClosed += (s, args) => this.Show();
                        //    managerDashboard.Show();
                        //}
                        //else if (empRole == "Barista" || empRole == "Head Barista" || empRole == "barista" || empRole == "head barista")
                        //{
                        //    BaristaDashboard baristaDashboard = new BaristaDashboard(empId, emp[1]);
                        //    this.Hide();
                        //    baristaDashboard.FormClosed += (s, args) => this.Show();
                        //    baristaDashboard.Show();
                        //}
                        if (txtUsername.Text == "E001" && txtPassword.Text == "admin123")
                        {
                            ManagerDashboard managerDashboard = new ManagerDashboard();
                            this.Hide();
                            managerDashboard.Show();
                            return;
                        }
                        if (txtUsername.Text == "E002" && txtPassword.Text == "bar123")
                        {
                            BaristaDashboard baristaDashboard = new BaristaDashboard("E002", "Ali Khan");
                            this.Hide();
                            baristaDashboard.Show();
                            return;
                        }
                        else
                        {
                            MessageBox.Show("Unknown role. Please contact administrator.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        txtUsername.Clear();
                        txtPassword.Clear();
                        return;
                    }
                }
            }

            // If no match found
            MessageBox.Show("Invalid Employee ID or Password!", "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            CustomerStartingPage welcome = new CustomerStartingPage();
            welcome.Show();
            this.Hide();
            welcome.FormClosed += (s, args) => this.Close();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }
    }
}