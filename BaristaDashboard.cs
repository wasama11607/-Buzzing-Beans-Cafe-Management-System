using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace Buzzing_Beans
{
    public partial class BaristaDashboard : Form
    {
        private Panel pnlButtonContainer;
        private Button btnViewMenu;
        private Button btnTakeOrder;
        private Button btnLogout;

        private Image imgBanner;
        private Image imgViewMenu;
        private Image imgTakeOrder;
        private Image imgBackground;

        // Store logged-in barista info
        private string loggedInBaristaId = "";
        private string loggedInBaristaName = "";

        private const int BANNER_HEIGHT = 260;
        private const int BANNER_TOP_MARGIN = 25;
        private const int BANNER_SIDE_MARGIN = 35;

        // Constructor with barista info (called from LoginForm)
        public BaristaDashboard(string baristaId, string baristaName)
        {
            loggedInBaristaId = baristaId;
            loggedInBaristaName = baristaName;
            InitializeComponents();
            SetupForm();
        }

        // Default constructor (for backward compatibility)
        public BaristaDashboard() : this("E001", "Barista")
        {
        }

        private void SetupForm()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.FormClosed += (s, e) => Application.Exit();
            this.Resize += (s, e) => LayoutButtons();

            // Set form title with barista name
            this.Text = $"Barista Dashboard - Welcome {loggedInBaristaName}";
        }

        private void InitializeComponents()
        {
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1024, 720);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.FromArgb(245, 230, 211);
            this.DoubleBuffered = true;
            this.Paint += BaristaDashboardForm_Paint;

            pnlButtonContainer = new Panel { Size = new Size(540, 260), BackColor = Color.Transparent };

            btnViewMenu = new Button
            {
                Size = new Size(220, 220),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(20, 20),
                Tag = "ViewMenu"
            };
            btnViewMenu.FlatAppearance.BorderSize = 0;
            btnViewMenu.Click += BtnViewMenu_Click;
            btnViewMenu.Paint += BtnViewMenu_Paint;
            btnViewMenu.MouseEnter += Btn_MouseEnter;
            btnViewMenu.MouseLeave += Btn_MouseLeave;

            btnTakeOrder = new Button
            {
                Size = new Size(220, 220),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(300, 20),
                Tag = "TakeOrder"
            };
            btnTakeOrder.FlatAppearance.BorderSize = 0;
            btnTakeOrder.Click += BtnTakeOrder_Click;
            btnTakeOrder.Paint += BtnTakeOrder_Paint;
            btnTakeOrder.MouseEnter += Btn_MouseEnter;
            btnTakeOrder.MouseLeave += Btn_MouseLeave;

            btnLogout = new Button
            {
                Text = "Log Out",
                Size = new Size(150, 48),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(62, 39, 35),
                ForeColor = Color.FromArgb(245, 230, 211),
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += BtnLogout_Click;
            btnLogout.MouseEnter += Logout_MouseEnter;
            btnLogout.MouseLeave += Logout_MouseLeave;
            btnLogout.Paint += (s, e) => RoundButton(btnLogout, e, 25, null);

            pnlButtonContainer.Controls.Add(btnViewMenu);
            pnlButtonContainer.Controls.Add(btnTakeOrder);

            this.Controls.Add(btnLogout);
            this.Controls.Add(pnlButtonContainer);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadImagesSafely();
            LayoutButtons();
        }

        private void LayoutButtons()
        {
            int totalBannerBottom = BANNER_TOP_MARGIN + BANNER_HEIGHT;
            int centerX = (this.ClientSize.Width - pnlButtonContainer.Width) / 2;
            int remainingHeight = this.ClientSize.Height - totalBannerBottom;
            int centerY = totalBannerBottom + ((remainingHeight - pnlButtonContainer.Height) / 2) - 20;
            pnlButtonContainer.Location = new Point(centerX, centerY);
            btnLogout.Location = new Point((this.ClientSize.Width - btnLogout.Width) / 2, this.ClientSize.Height - 85);

            this.Invalidate();
        }

        private void LoadImagesSafely()
        {
            string path = Path.Combine(Application.StartupPath, "Images");
            imgBanner = LoadImageNoLock(Path.Combine(path, "BD.png"));
            imgViewMenu = LoadImageNoLock(Path.Combine(path, "VM.png"));
            imgTakeOrder = LoadImageNoLock(Path.Combine(path, "TO.png"));
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

        private void BaristaDashboardForm_Paint(object sender, PaintEventArgs e)
        {
            if (imgBackground != null)
            {
                e.Graphics.DrawImage(imgBackground, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            }

            if (imgBanner != null)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                int bannerWidth = this.ClientSize.Width - (BANNER_SIDE_MARGIN * 2);
                Rectangle bannerRect = new Rectangle(BANNER_SIDE_MARGIN, BANNER_TOP_MARGIN, bannerWidth, BANNER_HEIGHT);

                using (GraphicsPath path = new GraphicsPath())
                {
                    int radius = 25;
                    path.AddArc(bannerRect.X, bannerRect.Y, radius, radius, 180, 90);
                    path.AddArc(bannerRect.Right - radius, bannerRect.Y, radius, radius, 270, 90);
                    path.AddArc(bannerRect.Right - radius, bannerRect.Bottom - radius, radius, radius, 0, 90);
                    path.AddArc(bannerRect.X, bannerRect.Bottom - radius, radius, radius, 90, 90);
                    path.CloseFigure();

                    using (Region reg = new Region(path))
                    {
                        Region oldClip = e.Graphics.Clip;
                        e.Graphics.Clip = reg;
                        e.Graphics.DrawImage(imgBanner, bannerRect);
                        e.Graphics.Clip = oldClip;
                    }
                }
            }
        }

        private void RoundButton(Button btn, PaintEventArgs e, int radius, Image img)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, btn.Width, btn.Height);
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();
                btn.Region = new Region(path);

                if (img != null)
                    e.Graphics.DrawImage(img, 0, 0, btn.Width, btn.Height);
            }
        }

        private void BtnViewMenu_Paint(object sender, PaintEventArgs e) => RoundButton(btnViewMenu, e, 25, imgViewMenu);
        private void BtnTakeOrder_Paint(object sender, PaintEventArgs e) => RoundButton(btnTakeOrder, e, 25, imgTakeOrder);

        private void BtnViewMenu_Click(object sender, EventArgs e)
        {
            BaristaMenuForm menuForm = new BaristaMenuForm();
            this.Hide();
            menuForm.FormClosed += (s, args) => this.Show();
            menuForm.Show();
        }

        private void BtnTakeOrder_Click(object sender, EventArgs e)
        {
            TakeOrder takeOrderForm = new TakeOrder();
            this.Hide();
            takeOrderForm.FormClosed += (s, args) => this.Show();
            takeOrderForm.Show();
        }


        // Add this method:
        private void BtnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Open login form
                LoginForm login = new LoginForm();
                login.Show();

                // Close this dashboard
                this.Hide();
            }
        }

        // Add this method to save pending orders before logout
        //private void SavePendingOrders()
        //{
        //    try
        //    {
        //        string pendingFilePath = Path.Combine(Application.StartupPath, "Data", "pending_orders.txt");
        //        var lines = new List<string> { "OrderId,OrderDetails,Status,TotalBill,EstimatedSeconds,OrderTime" };

        //        foreach (var order in incomingOrders.Where(o => o.Status != "Completed"))
        //        {
        //            lines.Add($"{order.OrderId},{order.OrderDetails},{order.Status},{order.TotalBill},{order.EstimatedSeconds},{order.OrderTime:yyyy-MM-dd HH:mm:ss}");
        //        }

        //        File.WriteAllLines(pendingFilePath, lines);
        //    }
        //    catch { }
        //}

        private void Logout_MouseEnter(object sender, EventArgs e)
        {
            btnLogout.BackColor = Color.FromArgb(85, 55, 50);
            btnLogout.Size = new Size(160, 52);
            btnLogout.Location = new Point((this.ClientSize.Width - btnLogout.Width) / 2, this.ClientSize.Height - 87);
        }

        private void Logout_MouseLeave(object sender, EventArgs e)
        {
            btnLogout.BackColor = Color.FromArgb(62, 39, 35);
            btnLogout.Size = new Size(150, 48);
            btnLogout.Location = new Point((this.ClientSize.Width - btnLogout.Width) / 2, this.ClientSize.Height - 85);
        }

        private void Btn_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Button b)
            {
                b.SetBounds(b.Left - 5, b.Top - 5, 230, 230);
            }
        }

        private void Btn_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Button b)
            {
                b.SetBounds(b.Left + 5, b.Top + 5, 220, 220);
            }
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