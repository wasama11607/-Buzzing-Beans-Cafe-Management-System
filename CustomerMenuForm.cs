using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Label = System.Windows.Forms.Label;

namespace Buzzing_Beans
{
    public class MenuItem
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public string Category { get; set; }
        public string ImagePath { get; set; }
        public string ItemId { get; set; }  // Added for file reference
    }

    public class CartItem
    {
        public MenuItem Item { get; set; }
        public int Quantity { get; set; }
    }

    public partial class CustomerMenuForm : Form
    {
        private FlowLayoutPanel mainFlowPanel;
        private Panel headerPanel;
        private Panel cartPanel;
        private ListBox lstCartDisplay;
        private Label lblTotalAmount;
        private Button btnCheckout;

        private string imagesFolderPath;
        private List<CartItem> customerCart = new List<CartItem>();
        private double orderTotal = 0.0;

        // Member tracking fields
        private bool isMember = false;
        private string memberId = "";

        // FIXED CARD SIZE FOR BEAUTIFUL RATIO (No stretching)
        private const int CARD_WIDTH = 165;
        private const int CARD_HEIGHT = 165;

        // Default constructor for guest users (no member)
        public CustomerMenuForm() : this(false, "")
        {
        }

        // Constructor for member users
        public CustomerMenuForm(bool isMemberCustomer, string customerMemberId)
        {
            isMember = isMemberCustomer;
            memberId = customerMemberId;

            SetCorrectImagePath();

            // Ensure data folder exists
            FileHelper.EnsureDataFolder();

            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1024, 720);
            this.BackColor = Color.FromArgb(245, 230, 211);
            this.Text = "Buzzing Beans - Customer Self-Ordering Menu";

            this.BackgroundImage = LoadImageSafely("background.png");
            this.BackgroundImageLayout = ImageLayout.Stretch;

            InitializeCustomComponents();

            // Load menu from file instead of hardcoded data
            LoadMenuFromFile();

            this.SizeChanged += CustomerMenuForm_SizeChanged;
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

                    items.Add(new MenuItem
                    {
                        ItemId = item[0],
                        Name = item[1],
                        Category = item[2].ToUpper(),
                        Price = price,
                        ImagePath = item.Length > 5 ? item[5] : ""
                    });
                }
            }

            // Group by category and display
            var groupedItems = items.GroupBy(i => i.Category);

            foreach (var group in groupedItems)
            {
                string currentCategory = group.Key;

                // Add category wave header
                Panel pnlWave = new Panel
                {
                    Size = new Size(mainFlowPanel.Width - 40, 50),
                    Margin = new Padding(0, 20, 0, 10),
                    BackColor = Color.Transparent,
                    Tag = "CategoryWave"
                };

                pnlWave.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    Brush brush = new SolidBrush(Color.FromArgb(141, 110, 99));
                    System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
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
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.BottomCenter,
                    BackColor = Color.Transparent,
                    Padding = new Padding(0, 0, 0, 6)
                };

                pnlWave.Controls.Add(lblCat);
                mainFlowPanel.Controls.Add(pnlWave);

                // Add items in this category
                foreach (var item in group)
                {
                    mainFlowPanel.Controls.Add(CreateCustomerMenuCard(item));
                }
            }

            UpdateResponsiveLayout();
        }

        private void InitializeCustomComponents()
        {
            this.DoubleBuffered = true;

            // 1. Header Banner Panel
            headerPanel = new Panel
            {
                Height = 220,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(141, 110, 99)
            };

            PictureBox pbLogo = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent,
                Image = LoadImageSafely("BuzzingBeans(logo).png")
            };
            headerPanel.Controls.Add(pbLogo);

            Button btnBack = new Button
            {
                Size = new Size(45, 45),
                Location = new Point(15, 15),
                Text = "←",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 0, 0, 0),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) =>
            {
                CustomerStartingPage mainPage = new CustomerStartingPage();
                mainPage.Show();
                this.Hide();
            };
            headerPanel.Controls.Add(btnBack);
            btnBack.BringToFront();

            this.Controls.Add(headerPanel);

            // 2. Right Side - Basket Panel
            cartPanel = new Panel
            {
                Width = 320,
                BackColor = Color.FromArgb(141, 110, 99),
                Padding = new Padding(15)
            };

            Label lblCartTitle = new Label
            {
                Text = "YOUR BASKET",
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            cartPanel.Controls.Add(lblCartTitle);

            btnCheckout = new Button
            {
                Text = "PLACE ORDER",
                Dock = DockStyle.Bottom,
                Height = 55,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(193, 154, 107),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCheckout.FlatAppearance.BorderSize = 0;
            btnCheckout.Click += BtnCheckout_Click;
            cartPanel.Controls.Add(btnCheckout);

            lblTotalAmount = new Label
            {
                Text = "Total: Rs. 0.00",
                Dock = DockStyle.Bottom,
                Height = 45,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(245, 230, 211),
                TextAlign = ContentAlignment.MiddleRight
            };
            cartPanel.Controls.Add(lblTotalAmount);

            lstCartDisplay = new ListBox
            {
                Dock = DockStyle.None,
                BackColor = Color.FromArgb(245, 230, 211),
                ForeColor = Color.FromArgb(62, 39, 35),
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                BorderStyle = BorderStyle.None,
                DrawMode = DrawMode.OwnerDrawFixed,
                IntegralHeight = false,
                ItemHeight = 32
            };

            lstCartDisplay.DrawItem += LstCartDisplay_DrawItem;
            cartPanel.Controls.Add(lstCartDisplay);

            this.Controls.Add(cartPanel);

            // 3. Left Side - Menu Flow Layout Panel
            mainFlowPanel = new FlowLayoutPanel
            {
                AutoScroll = true,
                WrapContents = true,
                BackColor = Color.Transparent
            };

            this.Controls.Add(mainFlowPanel);

            UpdateResponsiveLayout();
        }

        private void CustomerMenuForm_SizeChanged(object sender, EventArgs e)
        {
            UpdateResponsiveLayout();
        }

        private void UpdateResponsiveLayout()
        {
            if (cartPanel == null || mainFlowPanel == null || lstCartDisplay == null) return;

            int headerHeight = headerPanel.Height;
            int workingHeight = this.ClientSize.Height - headerHeight;

            cartPanel.Location = new Point(this.ClientSize.Width - cartPanel.Width - 20, headerHeight + 20);
            cartPanel.Height = workingHeight - 40;

            lstCartDisplay.Location = new Point(15, 55);
            lstCartDisplay.Width = cartPanel.Width - 30;
            lstCartDisplay.Height = cartPanel.Height - lblTotalAmount.Height - btnCheckout.Height - 85;

            int totalCardsWidth = (CARD_WIDTH + 16) * 4;
            int usableWidth = mainFlowPanel.Width - SystemInformation.VerticalScrollBarWidth;
            int sidePadding = (usableWidth - totalCardsWidth) / 2;
            if (sidePadding < 15) sidePadding = 15;

            mainFlowPanel.Location = new Point(15, headerHeight + 20);
            mainFlowPanel.Width = cartPanel.Left - 25;
            mainFlowPanel.Height = workingHeight - 30;
            mainFlowPanel.Padding = new Padding(sidePadding, 15, sidePadding, 30);

            foreach (Control ctrl in mainFlowPanel.Controls)
            {
                if (ctrl is Panel && ctrl.Tag != null && ctrl.Tag.ToString() == "CategoryWave")
                {
                    ctrl.Width = mainFlowPanel.Width - (sidePadding * 2) - 10;
                }
            }
        }

        private Panel CreateCustomerMenuCard(MenuItem item)
        {
            Panel card = new Panel
            {
                Width = CARD_WIDTH,
                Height = CARD_HEIGHT,
                Margin = new Padding(8),
                BackColor = Color.FromArgb(193, 154, 107),
                Tag = item.ItemId
            };
            card.Cursor = Cursors.Hand;

            PictureBox pic = new PictureBox
            {
                Height = 130,
                Dock = DockStyle.Top,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(193, 154, 107),
                Image = LoadImageSafely(item.ImagePath)
            };

            Panel infoPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6, 2, 6, 2) };

            Label lblName = new Label
            {
                Text = item.Name,
                Location = new Point(2, 6),
                Width = 100,
                Height = 22,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label lblPrice = new Label
            {
                Text = $"Rs. {item.Price:F0}",
                Location = new Point(106, 6),
                Width = 52,
                Height = 22,
                ForeColor = Color.FromArgb(245, 230, 211),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };

            infoPanel.Controls.Add(lblName);
            infoPanel.Controls.Add(lblPrice);
            card.Controls.Add(infoPanel);
            card.Controls.Add(pic);

            Action<object, EventArgs> addToCartAction = (s, e) => AddItemToCart(item);
            card.Click += (s, e) => addToCartAction(s, e);
            pic.Click += (s, e) => addToCartAction(s, e);
            lblName.Click += (s, e) => addToCartAction(s, e);
            lblPrice.Click += (s, e) => addToCartAction(s, e);

            return card;
        }

        private void AddItemToCart(MenuItem item)
        {
            CartItem existingItem = customerCart.Find(x => x.Item.Name == item.Name);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                customerCart.Add(new CartItem { Item = item, Quantity = 1 });
            }

            UpdateCartUI();
        }

        private void UpdateCartUI()
        {
            lstCartDisplay.Items.Clear();
            orderTotal = 0.0;

            foreach (var cartItem in customerCart)
            {
                double itemSubtotal = cartItem.Item.Price * cartItem.Quantity;
                orderTotal += itemSubtotal;

                lstCartDisplay.Items.Add($"{cartItem.Quantity}x {cartItem.Item.Name}|Rs. {itemSubtotal:F0}");
            }

            lblTotalAmount.Text = $"Total: Rs. {orderTotal:F0}";
            lstCartDisplay.Refresh();
        }

        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (customerCart.Count == 0)
            {
                MessageBox.Show("Your basket is empty!", "Basket Empty",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (OrderReviewPage reviewForm = new OrderReviewPage(customerCart, orderTotal, isMember, memberId))
            {
                if (reviewForm.ShowDialog(this) == DialogResult.OK)
                {
                    customerCart.Clear();
                    UpdateCartUI();
                }
            }
            // Do NOT close this form — keep it open
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

        private void LstCartDisplay_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 32;
        }

        private void LstCartDisplay_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= lstCartDisplay.Items.Count) return;

            e.DrawBackground();
            Graphics g = e.Graphics;
            Rectangle bounds = e.Bounds;

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            using (SolidBrush bgBrush = new SolidBrush(isSelected ? Color.FromArgb(193, 154, 107) : lstCartDisplay.BackColor))
            {
                g.FillRectangle(bgBrush, bounds);
            }

            string rawItemString = lstCartDisplay.Items[e.Index].ToString();
            string[] components = rawItemString.Split('|');
            string leftText = components[0];
            string rightText = components.Length > 1 ? components[1] : "";

            Color textCol = isSelected ? Color.White : lstCartDisplay.ForeColor;

            TextFormatFlags leftFlags = TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
            TextFormatFlags rightFlags = TextFormatFlags.VerticalCenter | TextFormatFlags.Right;

            Rectangle leftBounds = new Rectangle(bounds.Left + 10, bounds.Top, bounds.Width / 2 + 20, bounds.Height);
            Rectangle rightBounds = new Rectangle(bounds.Right - 90, bounds.Top, 80, bounds.Height);

            TextRenderer.DrawText(g, leftText, lstCartDisplay.Font, leftBounds, textCol, leftFlags);
            if (!string.IsNullOrEmpty(rightText))
            {
                TextRenderer.DrawText(g, rightText, lstCartDisplay.Font, rightBounds, textCol, rightFlags);
            }

            e.DrawFocusRectangle();
        }
    }
}