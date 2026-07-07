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
    public partial class BaristaMenuForm : Form
    {
        private FlowLayoutPanel mainFlowPanel;
        private string imagesFolderPath;
        private Image imgLogo;

        private const int CARD_WIDTH = 260;
        private const int CARD_HEIGHT = 270;
        private const int CARD_MARGIN = 12;

        private const int BANNER_HEIGHT = 200;
        private const int BANNER_TOP_MARGIN = 20;
        private const int BANNER_SIDE_MARGIN = 35;

        public BaristaMenuForm()
        {
            SetCorrectImagePath();

            // Ensure data folder exists
            FileHelper.EnsureDataFolder();

            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1024, 720);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 230, 211);
            this.Text = "Buzzing Beans - Barista Menu";
            this.BackgroundImage = LoadImageSafely("background.png");
            this.BackgroundImageLayout = ImageLayout.Stretch;

            InitializeCustomComponents();

            // Load menu from file instead of hardcoded data
            LoadMenuFromFile();

            this.Resize += (s, e) => UpdateFlowPanelLayout();
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

        private void LoadMenuFromFile()
        {
            // Clear existing controls (if any)
            mainFlowPanel?.Controls.Clear();

            var menuItems = FileHelper.ReadMenu();

            if (menuItems.Count == 0)
            {
                MessageBox.Show("No menu items found. Please contact the manager to add items to the menu.",
                    "Menu Empty", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Convert file data to MenuItem objects
            List<MenuItem> items = new List<MenuItem>();
            foreach (var item in menuItems)
            {
                if (item.Length >= 4)
                {
                    double price;
                    double.TryParse(item[3], out price);

                    // Convert category to uppercase for consistency
                    string category = item[2].ToUpper();

                    items.Add(new MenuItem
                    {
                        Name = item[1],
                        Category = category,
                        Price = price,
                        ImagePath = item.Length > 5 ? item[5] : ""
                    });
                }
            }

            // Group by category
            var groupedItems = items.GroupBy(i => i.Category);

            foreach (var group in groupedItems)
            {
                string currentCategory = group.Key;

                // Add category wave header
                Panel pnlWave = new Panel
                {
                    Width = (CARD_WIDTH + (CARD_MARGIN * 2)) * 4,
                    Height = 65,
                    Margin = new Padding(0, 25, 0, 15),
                    BackColor = Color.Transparent,
                    Tag = "WaveCategory"
                };

                pnlWave.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    Brush brush = new SolidBrush(Color.FromArgb(141, 110, 99));
                    GraphicsPath path = new GraphicsPath();
                    int w = pnlWave.Width;
                    int h = pnlWave.Height;
                    path.StartFigure();
                    path.AddLine(0, 15, 0, h);
                    path.AddLine(0, h, w, h);
                    path.AddLine(w, h, w, 15);
                    path.AddBezier(w, 15, w * 0.75f, -5, w * 0.25f, 35, 0, 15);
                    path.CloseFigure();
                    e.Graphics.FillPath(brush, path);
                };

                Label lblCat = new Label
                {
                    Text = currentCategory,
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 15, FontStyle.Bold),
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    Padding = new Padding(0, 5, 0, 0)
                };

                pnlWave.Controls.Add(lblCat);
                mainFlowPanel.Controls.Add(pnlWave);

                // Add items in this category
                foreach (var item in group)
                {
                    mainFlowPanel.Controls.Add(CreateMenuCard(item));
                }
            }
        }

        private void InitializeCustomComponents()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);

            imgLogo = LoadImageSafely("buzzingbeansmenu(logo).png");
            this.Paint += BaristaMenuForm_Paint;

            Button btnBack = new Button
            {
                Size = new Size(45, 45),
                Location = new Point(50, BANNER_TOP_MARGIN + 15),
                Text = "←",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) => { this.Close(); };
            this.Controls.Add(btnBack);
            btnBack.BringToFront();

            mainFlowPanel = new FlowLayoutPanel
            {
                Top = BANNER_HEIGHT + BANNER_TOP_MARGIN + 15,
                AutoScroll = true,
                WrapContents = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(mainFlowPanel);
            UpdateFlowPanelLayout();
        }

        private void BaristaMenuForm_Paint(object sender, PaintEventArgs e)
        {
            if (imgLogo != null)
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
                        e.Graphics.DrawImage(imgLogo, bannerRect);
                        e.Graphics.Clip = oldClip;
                    }
                }
            }
        }

        private void UpdateFlowPanelLayout()
        {
            if (mainFlowPanel == null) return;

            int headerBottom = BANNER_HEIGHT + BANNER_TOP_MARGIN + 15;
            mainFlowPanel.Top = headerBottom;
            mainFlowPanel.Height = this.ClientSize.Height - headerBottom - 15;
            mainFlowPanel.Width = this.ClientSize.Width;
            mainFlowPanel.Left = 0;

            int strictFourColumnsWidth = (CARD_WIDTH + (CARD_MARGIN * 2)) * 4 + 10;
            int leftPaddingOffset = Math.Max(20, (mainFlowPanel.Width - strictFourColumnsWidth - 25) / 2);

            mainFlowPanel.Padding = new Padding(leftPaddingOffset, 10, 10, 30);
            foreach (Control ctrl in mainFlowPanel.Controls)
            {
                if (ctrl is Panel && ctrl.Tag != null && ctrl.Tag.ToString() == "WaveCategory")
                {
                    ctrl.Width = strictFourColumnsWidth;
                }
            }
        }

        private Panel CreateMenuCard(MenuItem item)
        {
            Color normalColor = Color.FromArgb(193, 154, 107);
            Color hoverColor = Color.FromArgb(141, 110, 99);

            Panel cardContainer = new Panel
            {
                Width = CARD_WIDTH + (CARD_MARGIN * 2),
                Height = CARD_HEIGHT + (CARD_MARGIN * 2),
                BackColor = Color.Transparent
            };

            Panel card = new Panel
            {
                Width = CARD_WIDTH,
                Height = CARD_HEIGHT,
                Location = new Point(CARD_MARGIN, CARD_MARGIN),
                BackColor = normalColor
            };

            int originalPicWidth = CARD_WIDTH;
            int originalPicHeight = CARD_HEIGHT - 50;

            Panel picContainer = new Panel
            {
                Size = new Size(originalPicWidth, originalPicHeight),
                Location = new Point(0, 0),
                BackColor = Color.Transparent
            };

            PictureBox pic = new PictureBox
            {
                Size = new Size(originalPicWidth, originalPicHeight),
                Location = new Point(0, 0),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = LoadImageSafely(item.ImagePath)
            };
            picContainer.Controls.Add(pic);

            Panel infoPanel = new Panel
            {
                Size = new Size(CARD_WIDTH, 50),
                Location = new Point(0, originalPicHeight),
                Padding = new Padding(5)
            };

            Label lblName = new Label
            {
                Text = item.Name,
                Location = new Point(8, 12),
                Width = CARD_WIDTH - 95,
                AutoSize = false,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label lblPrice = new Label
            {
                Text = $"Rs. {item.Price:F0}",
                Location = new Point(CARD_WIDTH - 85, 12),
                Width = 75,
                AutoSize = false,
                ForeColor = Color.FromArgb(245, 230, 211),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };

            infoPanel.Controls.Add(lblName);
            infoPanel.Controls.Add(lblPrice);

            card.Controls.Add(infoPanel);
            card.Controls.Add(picContainer);
            cardContainer.Controls.Add(card);

            Control[] interactiveControls = { card, pic, infoPanel, lblName, lblPrice };
            foreach (var control in interactiveControls)
            {
                control.MouseEnter += (s, e) =>
                {
                    card.BackColor = hoverColor;
                    card.Location = new Point(CARD_MARGIN, CARD_MARGIN - 6);
                    int zoomOffset = 15;
                    pic.Size = new Size(originalPicWidth + zoomOffset, originalPicHeight + zoomOffset);
                    pic.Location = new Point(-zoomOffset / 2, -zoomOffset / 2);
                    card.Cursor = Cursors.Hand;
                };
                control.MouseLeave += (s, e) =>
                {
                    if (!cardContainer.ClientRectangle.Contains(cardContainer.PointToClient(Cursor.Position)))
                    {
                        card.BackColor = normalColor;
                        card.Location = new Point(CARD_MARGIN, CARD_MARGIN);
                        pic.Size = new Size(originalPicWidth, originalPicHeight);
                        pic.Location = new Point(0, 0);
                    }
                };
            }

            card.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(0, 0, card.Width, card.Height);
                using (GraphicsPath path = new GraphicsPath())
                {
                    int radius = 20;
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                    path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                    path.CloseFigure();
                    card.Region = new Region(path);
                }
            };
            return cardContainer;
        }

        private Image LoadImageSafely(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;

            try
            {
                string fullPath = Path.Combine(imagesFolderPath, fileName);
                if (File.Exists(fullPath))
                {
                    using (FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                    {
                        return Image.FromStream(fs);
                    }
                }
                return null;
            }
            catch { return null; }
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