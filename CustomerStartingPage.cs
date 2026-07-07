using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Buzzing_Beans
{
    public partial class CustomerStartingPage : Form
    {
        private Panel mainPanel;
        private Panel topBannerPanel;
        private Panel cardsContainer;
        private Button btnGuestOrder;
        private Button btnMembership;
        private Button btnStaffEntrance;

        private Image mainBgImage;
        private Image topBannerImage;
        private Image guestOrderImage;
        private Image membershipImage;

        private string loggedInMemberId = "";
        private bool isMemberLoggedIn = false;

        // ========== FIX 1: Cache regions to prevent recreation ==========
        private Region cachedTopBannerRegion;
        private bool isLayoutDirty = true;
        private Size lastFormSize = Size.Empty;

        public CustomerStartingPage()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                  ControlStyles.UserPaint |
                  ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            this.SuspendLayout();

            this.Text = "Welcome to Our Café";
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.StartPosition = FormStartPosition.CenterScreen;

            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1024, 720);
            this.Size = new Size(685, 750);
            this.BackColor = Color.FromArgb(245, 245, 220);

            LoadImages();

            // ========== MAIN CONTAINER PANEL ==========
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
            this.Controls.Add(mainPanel);

            // ========== RESPONSIVE TOP BANNER HEADER ==========
            topBannerPanel = new Panel
            {
                Location = new Point(0, 0),
                Height = 200,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            topBannerPanel.Width = mainPanel.Width;

            topBannerPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                Rectangle rect = new Rectangle(0, 0, topBannerPanel.Width, topBannerPanel.Height);

                // ========== FIX 2: Only recreate region if size changed ==========
                if (cachedTopBannerRegion == null || isLayoutDirty)
                {
                    cachedTopBannerRegion?.Dispose();
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        int rBot = 60;
                        path.AddLine(0, 0, rect.Width, 0);
                        path.AddLine(rect.Width, 0, rect.Width, rect.Height - rBot);
                        path.AddArc(rect.Width - (rBot * 2), rect.Height - (rBot * 2), rBot * 2, rBot * 2, 0, 90);
                        path.AddArc(0, rect.Height - (rBot * 2), rBot * 2, rBot * 2, 90, 90);
                        path.CloseFigure();
                        cachedTopBannerRegion = new Region(path);
                    }
                    topBannerPanel.Region = cachedTopBannerRegion;
                }

                if (topBannerImage != null)
                {
                    e.Graphics.DrawImage(topBannerImage, rect);
                }
                else
                {
                    using (SolidBrush fallback = new SolidBrush(Color.FromArgb(141, 110, 99)))
                    {
                        e.Graphics.FillRectangle(fallback, rect);
                    }
                }
            };
            mainPanel.Controls.Add(topBannerPanel);

            // ========== RESPONSIVE CARDS CONTAINER ==========
            cardsContainer = new Panel
            {
                Size = new Size(540, 270),
                BackColor = Color.Transparent
            };
            mainPanel.Controls.Add(cardsContainer);

            // GUEST ORDER BUTTON CARD
            btnGuestOrder = new Button
            {
                Size = new Size(220, 250),
                Location = new Point(20, 10),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Text = guestOrderImage == null ? "Guest Order" : "",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(141, 110, 99)
            };
            btnGuestOrder.FlatAppearance.BorderSize = 0;
            btnGuestOrder.Click += BtnGuestOrder_Click;
            ConfigureCardRendering(btnGuestOrder, guestOrderImage, "Guest Order");
            cardsContainer.Controls.Add(btnGuestOrder);

            // MEMBERSHIP BUTTON CARD
            btnMembership = new Button
            {
                Size = new Size(220, 250),
                Location = new Point(300, 10),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Text = membershipImage == null ? "Membership" : "",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(141, 110, 99)
            };
            btnMembership.FlatAppearance.BorderSize = 0;
            btnMembership.Click += BtnMembership_Click;
            ConfigureCardRendering(btnMembership, membershipImage, "Membership");
            cardsContainer.Controls.Add(btnMembership);

            // STAFF LOGIN FOOTER BUTTON
            btnStaffEntrance = new Button
            {
                Text = "Staff Login",
                Font = new Font("Segoe UI", 22, FontStyle.Bold | FontStyle.Underline),
                ForeColor = Color.FromArgb(141, 110, 99),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            btnStaffEntrance.FlatAppearance.BorderSize = 0;
            btnStaffEntrance.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnStaffEntrance.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnStaffEntrance.Click += BtnStaffEntrance_Click;
            mainPanel.Controls.Add(btnStaffEntrance);

            // ========== FIX 3: Throttle layout events - only trigger when needed ==========
            this.Load += (s, e) =>
            {
                lastFormSize = this.Size;
                isLayoutDirty = true;
                RealignUIElements();
                isLayoutDirty = false;
            };

            this.Resize += (s, e) =>
            {
                if (this.Size != lastFormSize)
                {
                    lastFormSize = this.Size;
                    isLayoutDirty = true;
                    RealignUIElements();
                    isLayoutDirty = false;
                }
            };

            this.ResumeLayout(false);
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

        private void RealignUIElements()
        {
            if (mainPanel == null || cardsContainer == null || btnStaffEntrance == null || topBannerPanel == null) return;

            topBannerPanel.Width = mainPanel.Width;

            int cardsTop = topBannerPanel.Bottom + 45;
            cardsContainer.Location = new Point((mainPanel.Width - cardsContainer.Width) / 2, cardsTop);

            int staffTop = cardsContainer.Bottom + 40;
            btnStaffEntrance.Location = new Point((mainPanel.Width - btnStaffEntrance.Width) / 2, staffTop);

            topBannerPanel.Invalidate();
        }

        private void LoadImages()
        {
            try { mainBgImage = Image.FromFile(@"D:\Kisfa Javed\SDA\Menu\background.png"); } catch { }
            try { topBannerImage = Image.FromFile(@"D:\Kisfa Javed\SDA\Menu\Customer.jpg"); } catch { }
            try { guestOrderImage = Image.FromFile(@"D:\Kisfa Javed\SDA\Menu\Guest Order.jpg"); } catch { }
            try { membershipImage = Image.FromFile(@"D:\Downloads\_0fb298ad-2ba2-4036-bd51-6e231b9224d4.jfif"); } catch { }
        }

        private void ConfigureCardRendering(Button btn, Image img, string fallbackText)
        {
            int baseWidth = 220;
            int baseHeight = 250;
            bool isHovered = false;
            Region cachedButtonRegion = null;

            btn.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                Rectangle rect = new Rectangle(0, 0, btn.Width, btn.Height);

                // ========== FIX 4: Cache button region ==========
                if (cachedButtonRegion == null)
                {
                    using (GraphicsPath path = GetRoundedRectanglePath(rect, 25))
                    {
                        cachedButtonRegion = new Region(path);
                    }
                    btn.Region = cachedButtonRegion;
                }

                Color bgColor = isHovered ? Color.FromArgb(253, 245, 230) : Color.White;
                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                {
                    e.Graphics.FillRectangle(bgBrush, rect);  // ✅ FIXED: Brush FIRST, then Rectangle
                }

                if (img != null)
                {
                    e.Graphics.DrawImage(img, rect);
                }
                else
                {
                    TextRenderer.DrawText(e.Graphics, fallbackText, btn.Font, rect, btn.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                Color borderColor = isHovered ? Color.FromArgb(193, 154, 107) : Color.FromArgb(215, 204, 200);
                using (Pen borderPen = new Pen(borderColor, 2))
                using (GraphicsPath path = GetRoundedRectanglePath(rect, 25))
                {
                    e.Graphics.DrawPath(borderPen, path);
                }
            };

            btn.MouseEnter += (s, e) =>
            {
                isHovered = true;
                btn.Size = new Size((int)(baseWidth * 1.05), (int)(baseHeight * 1.05));
                btn.Location = new Point(btn.Location.X - 5, btn.Location.Y - 6);
                btn.Invalidate();
            };

            btn.MouseLeave += (s, e) =>
            {
                isHovered = false;
                btn.Size = new Size(baseWidth, baseHeight);
                btn.Location = new Point(btn.Location.X + 5, btn.Location.Y + 6);
                btn.Invalidate();
            };
        }

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

        private void BtnGuestOrder_Click(object sender, EventArgs e)
        {
            CustomerMenuForm menuForm = new CustomerMenuForm(false, "");
            menuForm.Show();
            this.Hide();
        }

        private void BtnStaffEntrance_Click(object sender, EventArgs e)
        {
            LoginForm dash = new LoginForm();
            dash.Show();
            this.Hide();
        }

        private void BtnMembership_Click(object sender, EventArgs e)
        {
            Panel dimOverlay = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Cornsilk
            };

            dimOverlay.Paint += (s, ev) => {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(90, 45, 30, 20)))
                {
                    ev.Graphics.FillRectangle(brush, dimOverlay.ClientRectangle);
                }
            };

            this.Controls.Add(dimOverlay);
            dimOverlay.BringToFront();

            string memberId = "";
            bool loginSuccessful = false;

            using (MemeberLoginPage loginPage = new MemeberLoginPage())
            {
                if (loginPage.ShowDialog(this) == DialogResult.OK)
                {
                    loginSuccessful = true;
                    memberId = loginPage.LoggedInMemberId;
                }
            }

            this.Controls.Remove(dimOverlay);
            dimOverlay.Dispose();

            if (loginSuccessful)
            {
                CustomerMenuForm menuForm = new CustomerMenuForm(true, memberId);
                menuForm.Show();
                this.Hide();
            }
        }
    }
}