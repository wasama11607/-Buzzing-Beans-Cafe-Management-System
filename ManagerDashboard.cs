using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace Buzzing_Beans
{
    public partial class ManagerDashboard : Form
    {
        // ==================== FIELDS ====================
        private  Panel pnlSidebar;
        private  Panel pnlMain;
        private  Label lblPageTitle;
        private FlowLayoutPanel EmployeeCardContainer;
        private FlowLayoutPanel CoffeeContainer;
        private FlowLayoutPanel TeaContainer;
        private FlowLayoutPanel DessertContainer;
        private Panel CafeMapCanvas;

        private int employeeCount = 1;
        private Panel currentlyUpdatingCard;
        private string selectedImagePath = "";
        private string newProfileImagePath = "";
        private string currentUpdatingEmpId = "";

        private Form employeeModal;
        private Form menuModal;
        private Form profileModal;

        private TextBox txtEmpName, txtEmpRole, txtEmpContact;
        private TextBox txtItemName, txtItemPrice;
        private ComboBox cmbCategory;
        private Label lblImagePath;
        private TextBox txtProfileName, txtProfileRole;
        private PictureBox ProfilePreview;

        private  Label lblManagerName, lblManagerRole;
        private  PictureBox imgManagerPhoto;

        private  List<SaleRecord> MySalesData = new List<SaleRecord>();

        private TextBox txtOpenTime, txtCloseTime;
        private CheckBox chkNightShift, chkAutoLock;

        private Panel ViewHome, ViewEmployees, ViewMenu, ViewTime, ViewPlacement, ViewSales;

        private bool isDraggingTable;
        private Point tableClickOffset;
        private Control selectedTable;
        private int tableCounterPlacement;

        public ManagerDashboard()
        {
            InitializeComponent();

            FileHelper.EnsureDataFolder();

            this.Text = "Manager Dashboard - BUZZING BEANS";
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1024, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 230, 211);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            InitializeSalesData();
            SetupUI();

            LoadEmployeesFromFile();
            LoadMenuFromFile();
            LoadCafeTimeFromFile();
            LoadManagerProfile();
        }

        private void LoadEmployeesFromFile()
        {
            var employees = FileHelper.ReadEmployees();
            if (employees.Count > 0)
            {
                employeeCount = employees.Count;
                foreach (var emp in employees)
                {
                    if (emp.Length >= 4)
                    {
                        AddNewEmployeeCard(emp[1], emp[2], emp[3], emp[0]);
                    }
                }
            }
        }

        private void LoadMenuFromFile()
        {
            var menuItems = FileHelper.ReadMenu();
            foreach (var item in menuItems)
            {
                if (item.Length >= 5)
                {
                    string imagePath = item.Length > 5 ? item[5] : null;
                    AddSampleMenuItem(item[1], item[2], item[3], imagePath);
                }
            }
        }

        private void LoadCafeTimeFromFile()
        {
            var (openTime, closeTime, nightShift, autoLock) = FileHelper.LoadCafeTime();
        }
        private void LoadManagerProfile()
        {
            var (name, role, imagePath) = FileHelper.LoadManagerProfile();

            // Update the labels
            lblManagerName.Text = name;
            lblManagerRole.Text = role;

            // Load the profile picture if exists
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                try
                {
                    imgManagerPhoto.Image = Image.FromFile(imagePath);
                }
                catch { }
            }
        }

        private void InitializeSalesData()
        {
            var sales = FileHelper.ReadSales();
            foreach (var sale in sales)
            {
                if (sale.Length >= 4)
                {
                    MySalesData.Add(new SaleRecord
                    {
                        OrderId = sale[1],
                        Items = sale[2],
                        Total = sale[3]
                    });
                }
            }

            if (MySalesData.Count == 0)
            {
                MySalesData.Add(new SaleRecord { OrderId = "#BB-501", Items = "2x Latte, 1x Brownie", Total = "1,250" });
                MySalesData.Add(new SaleRecord { OrderId = "#BB-502", Items = "1x Espresso", Total = "450" });
            }
        }

        private void AddSampleMenuItem(string name, string category, string price, string imagePath)
        {
            if (CoffeeContainer is null || TeaContainer is null || DessertContainer is null)
                return;

            Panel newCard = CreateMenuItemCard(name, price, imagePath);
            if (category == "Coffee")
                CoffeeContainer.Controls.Add(newCard);
            else if (category == "Tea")
                TeaContainer.Controls.Add(newCard);
            else
                DessertContainer.Controls.Add(newCard);
        }

        private void SetupUI()
        {
            this.Controls.Clear();

            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            pnlSidebar = new Panel { Dock = DockStyle.Fill };

            string sidebarImagePath = @"D:\Kisfa Javed\SDA\Menu\Brown Background.png";
            if (File.Exists(sidebarImagePath))
            {
                pnlSidebar.BackgroundImage = Image.FromFile(sidebarImagePath);
                pnlSidebar.BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                pnlSidebar.BackColor = Color.FromArgb(62, 39, 35);
            }

            FlowLayoutPanel sidebarFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 40, 20, 20),
                BackColor = Color.Transparent
            };

            Label lblTitle = new Label
            {
                Text = "BUZZING BEANS",
                ForeColor = Color.FromArgb(245, 230, 211),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = false,
                Width = 220,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            sidebarFlow.Controls.Add(lblTitle);

            Label lblSubtitle = new Label
            {
                Text = "Manager Portal",
                ForeColor = Color.FromArgb(215, 204, 200),
                Font = new Font("Segoe UI", 12),
                AutoSize = false,
                Width = 220,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 30),
                BackColor = Color.Transparent
            };

            sidebarFlow.Controls.Add(lblSubtitle);
            sidebarFlow.Controls.Add(CreateNavButton("🏠 Home / Profile", BtnHome_Click));
            sidebarFlow.Controls.Add(CreateNavButton("👥 Manage Employees", BtnEmployees_Click));
            sidebarFlow.Controls.Add(CreateNavButton("☕ Manage Menu", BtnMenu_Click));
            sidebarFlow.Controls.Add(CreateNavButton("🕒 Manage Cafe Time", BtnTime_Click));
            sidebarFlow.Controls.Add(CreateNavButton("📍 Manage Placement", BtnPlacement_Click));
            sidebarFlow.Controls.Add(CreateNavButton("📈 View Sales Report", BtnSales_Click));

            pnlSidebar.Controls.Add(sidebarFlow);

            Panel spacer = new Panel { Height = 100 };
            sidebarFlow.Controls.Add(spacer);

            Button btnLogout = new Button
            {
                Text = "Logout",
                BackColor = Color.FromArgb(141, 110, 99),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Height = 45,
                Width = 220,
                FlatStyle = FlatStyle.Flat
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += BtnLogout_Click;
            sidebarFlow.Controls.Add(btnLogout);

            pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            string imagePath = @"D:\Kisfa Javed\SDA\Menu\background.png";
            if (File.Exists(imagePath))
            {
                pnlMain.BackgroundImage = Image.FromFile(imagePath);
                pnlMain.BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                pnlMain.BackColor = Color.FromArgb(245, 230, 211);
            }

            lblPageTitle = new Label
            {
                Text = "Welcome, Shazuryan",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(62, 39, 35),
                Dock = DockStyle.Top,
                Height = 45,
                Padding = new Padding(10, 0, 0, 0),
                BackColor = Color.Transparent
            };

            FlowLayoutPanel statsRow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Height = 120,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 10, 0, 15)
            };
            statsRow.Controls.Add(CreateStatCard("Total Revenue", "Rs. 15,400", Color.FromArgb(62, 39, 35), Color.White));
            statsRow.Controls.Add(CreateStatCard("Active Staff", "2", Color.White, Color.FromArgb(62, 39, 35)));
            statsRow.Controls.Add(CreateStatCard("Cafe Status", "OPEN", Color.White, Color.FromArgb(46, 125, 50)));

            CreateHomeView();
            CreateEmployeesView();
            CreateMenuView();
            CreateTimeView();
            CreatePlacementView();
            CreateSalesView();

            Panel viewsContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                AutoScroll = true
            };

            viewsContainer.Controls.Add(ViewHome);
            viewsContainer.Controls.Add(ViewEmployees);
            viewsContainer.Controls.Add(ViewMenu);
            viewsContainer.Controls.Add(ViewTime);
            viewsContainer.Controls.Add(ViewPlacement);
            viewsContainer.Controls.Add(ViewSales);

            foreach (Control view in viewsContainer.Controls)
            {
                view.Dock = DockStyle.Fill;
            }

            pnlMain.Controls.Add(viewsContainer);
            pnlMain.Controls.Add(statsRow);
            pnlMain.Controls.Add(lblPageTitle);

            mainLayout.Controls.Add(pnlSidebar, 0, 0);
            mainLayout.Controls.Add(pnlMain, 1, 0);
            this.Controls.Add(mainLayout);
        }

        private Button CreateNavButton(string text, EventHandler clickEvent)
        {
            Button btn = new Button
            {
                Text = text,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(245, 230, 211),
                Font = new Font("Segoe UI", 12),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Height = 45,
                Width = 220,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += clickEvent;
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(93, 64, 55);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.Transparent;
            return btn;
        }

        private Panel CreateStatCard(string title, string value, Color bgColor, Color textColor)
        {
            Panel card = new Panel
            {
                Width = 230,
                Height = 90,
                BackColor = bgColor,
                Margin = new Padding(5)
            };
            card.Paint += (sender, e) => MakeRoundedCorners(card, 15);

            Label lblTitle = new Label
            {
                Text = title,
                ForeColor = bgColor == Color.FromArgb(62, 39, 35) ? Color.FromArgb(215, 204, 200) : Color.FromArgb(141, 110, 99),
                Font = new Font("Segoe UI", 12),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 30
            };

            Label lblValue = new Label
            {
                Text = value,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            card.Controls.Add(lblValue);
            card.Controls.Add(lblTitle);
            return card;
        }

        private void MakeRoundedCorners(Panel panel, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            Rectangle rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
            panel.Region = new Region(path);
        }

        // ==================== HOME VIEW ====================
        private void CreateHomeView()
        {
            ViewHome = new Panel
            {
                Visible = true,
                AutoScroll = true,
                Padding = new Padding(10),
                BackColor = Color.Transparent
            };

            TableLayoutPanel profileLayout = new TableLayoutPanel
            {
                Width = 700,
                Height = 250,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White,
                Padding = new Padding(40)
            };
            profileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            profileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            profileLayout.Paint += (sender, e) => MakeRoundedCorners(profileLayout, 25);

            imgManagerPhoto = new PictureBox
            {
                Size = new Size(120, 120),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.FromArgb(62, 39, 35)
            };

            Panel imgPanel = new Panel();
            imgPanel.Controls.Add(imgManagerPhoto);
            imgManagerPhoto.Location = new Point(15, 15);
            imgPanel.Size = new Size(150, 150);

            FlowLayoutPanel infoPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Margin = new Padding(10, 0, 0, 0),
                Width = 300,
                AutoSize = true
            };

            lblManagerName = new Label { Text = "Shazuryan", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(62, 39, 35), AutoSize = true };
            lblManagerRole = new Label { Text = "System Administrator", Font = new Font("Segoe UI", 12), ForeColor = Color.FromArgb(93, 64, 55), AutoSize = true };
            Label lblLocation = new Label { Text = "Location: UET Lahore", Font = new Font("Segoe UI", 12), ForeColor = Color.FromArgb(93, 64, 55), AutoSize = true };

            Button btnUpdateProfile = new Button
            {
                Text = "Update Profile",
                BackColor = Color.FromArgb(62, 39, 35),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(150, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 15, 0, 0),
                Cursor = Cursors.Hand
            };
            btnUpdateProfile.FlatAppearance.BorderSize = 0;
            btnUpdateProfile.Click += BtnUpdateProfile_Click;

            infoPanel.Controls.Add(lblManagerName);
            infoPanel.Controls.Add(lblManagerRole);
            infoPanel.Controls.Add(lblLocation);
            infoPanel.Controls.Add(btnUpdateProfile);

            profileLayout.Controls.Add(imgPanel, 0, 0);
            profileLayout.Controls.Add(infoPanel, 1, 0);
            ViewHome.Controls.Add(profileLayout);
        }

        // ==================== EMPLOYEES VIEW ====================
        private void CreateEmployeesView()
        {
            ViewEmployees = new Panel { Visible = false, BackColor = Color.Transparent };

            FlowLayoutPanel topBar = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Height = 50,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 15)
            };

            Label lblTitle = new Label
            {
                Text = "Staff Management",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(62, 39, 35),
                AutoSize = true,
                Margin = new Padding(0, 5, 20, 0)
            };

            Button btnAddEmployee = new Button
            {
                Text = "+ Add New Employee",
                BackColor = Color.FromArgb(46, 125, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 160,
                Height = 35,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAddEmployee.FlatAppearance.BorderSize = 0;
            btnAddEmployee.Click += BtnAddEmployee_Click;

            topBar.Controls.Add(lblTitle);
            topBar.Controls.Add(btnAddEmployee);

            EmployeeCardContainer = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            ViewEmployees.Controls.Add(EmployeeCardContainer);
            ViewEmployees.Controls.Add(topBar);
        }

        private void AddNewEmployeeCard(string name, string role, string contact, string empId = null)
        {
            if (empId == null)
            {
                employeeCount++;
                empId = $"E{employeeCount:D3}";
            }
            else
            {
                int num = int.Parse(empId.Substring(1));
                if (num > employeeCount) employeeCount = num;
            }

            Panel card = new Panel
            {
                Width = 450,
                Height = 130,
                BackColor = Color.White,
                Margin = new Padding(10),
                Tag = empId
            };

            string crdBackgroundPath = @"D:\Kisfa Javed\SDA\Menu\backgroundwhite.png";
            if (File.Exists(crdBackgroundPath))
            {
                card.BackgroundImage = Image.FromFile(crdBackgroundPath);
                card.BackgroundImageLayout = ImageLayout.Stretch;
            }
            card.Paint += (sender, e) => MakeRoundedCorners(card, 15);

            TableLayoutPanel grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(15)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));

            Panel avatar = new Panel
            {
                BackColor = Color.FromArgb(62, 39, 35),
                Size = new Size(60, 60)
            };
            avatar.Paint += (sender, e) => MakeRoundedCorners(avatar, 30);
            Label avatarText = new Label
            {
                Text = "👤",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 24),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            avatar.Controls.Add(avatarText);

            FlowLayoutPanel info = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Margin = new Padding(10, 0, 0, 0),
                BackColor = Color.Transparent,
            };
            info.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(62, 39, 35), AutoSize = true, BackColor = Color.Transparent });
            info.Controls.Add(new Label { Text = "Role: " + role, ForeColor = Color.FromArgb(141, 110, 99), AutoSize = true, BackColor = Color.Transparent });
            info.Controls.Add(new Label { Text = $"ID: {empId} | Shift: Morning", ForeColor = Color.FromArgb(93, 64, 55), Font = new Font("Segoe UI", 10), AutoSize = true, BackColor = Color.Transparent });
            info.Controls.Add(new Label { Text = "Contact: " + contact, ForeColor = Color.FromArgb(93, 64, 55), Font = new Font("Segoe UI", 10), AutoSize = true, BackColor = Color.Transparent });

            FlowLayoutPanel btnPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Margin = new Padding(10, 20, 0, 0),
            };

            Button upBtn = new Button { Text = "Update", BackColor = Color.FromArgb(141, 110, 99), ForeColor = Color.White, Width = 80, Height = 30, FlatStyle = FlatStyle.Flat };
            upBtn.FlatAppearance.BorderSize = 0;
            upBtn.Click += (s, e) => OpenUpdateForm(card, name, role, contact, empId);

            Button rmBtn = new Button { Text = "Remove", BackColor = Color.FromArgb(211, 47, 47), ForeColor = Color.White, Width = 80, Height = 30, FlatStyle = FlatStyle.Flat };
            rmBtn.FlatAppearance.BorderSize = 0;
            rmBtn.Click += (s, e) =>
            {
                DialogResult result = MessageBox.Show($"Remove {name} from employees?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    FileHelper.DeleteEmployee(empId);
                    EmployeeCardContainer.Controls.Remove(card);
                }
            };

            btnPanel.Controls.Add(upBtn);
            btnPanel.Controls.Add(rmBtn);

            grid.Controls.Add(avatar, 0, 0);
            grid.Controls.Add(info, 1, 0);
            grid.Controls.Add(btnPanel, 2, 0);
            card.Controls.Add(grid);

            EmployeeCardContainer.Controls.Add(card);
        }

        private void OpenUpdateForm(Panel card, string name, string role, string contact, string empId)
        {
            currentlyUpdatingCard = card;
            currentUpdatingEmpId = empId;
            ShowEmployeeModal("Update Employee Info", name, role, contact);
        }

        private void ShowEmployeeModal(string title, string name, string role, string contact)
        {
            if (employeeModal != null && !employeeModal.IsDisposed)
                employeeModal.Close();

            employeeModal = new Form
            {
                Text = title,
                Size = new Size(450, 500),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30),
                RowCount = 6
            };

            layout.Controls.Add(new Label { Text = "Full Name", ForeColor = Color.FromArgb(141, 110, 99), Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true }, 0, 0);
            txtEmpName = new TextBox { Text = name, BackColor = Color.FromArgb(245, 230, 211), Height = 40, Dock = DockStyle.Fill };
            layout.Controls.Add(txtEmpName, 0, 1);

            layout.Controls.Add(new Label { Text = "Role", ForeColor = Color.FromArgb(141, 110, 99), Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 15, 0, 0) }, 0, 2);
            txtEmpRole = new TextBox { Text = role, BackColor = Color.FromArgb(245, 230, 211), Height = 40, Dock = DockStyle.Fill };
            layout.Controls.Add(txtEmpRole, 0, 3);

            layout.Controls.Add(new Label { Text = "Contact", ForeColor = Color.FromArgb(141, 110, 99), Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 15, 0, 0) }, 0, 4);
            txtEmpContact = new TextBox { Text = contact, BackColor = Color.FromArgb(245, 230, 211), Height = 40, Dock = DockStyle.Fill };
            layout.Controls.Add(txtEmpContact, 0, 5);

            FlowLayoutPanel btnPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Height = 50, Dock = DockStyle.Bottom, Margin = new Padding(0, 20, 0, 0) };
            Button btnCancel = new Button { Text = "Cancel", BackColor = Color.Transparent, ForeColor = Color.FromArgb(141, 110, 99), Width = 90, Height = 40, FlatStyle = FlatStyle.Flat };
            btnCancel.Click += (s, ev) => employeeModal.Close();
            Button btnSave = new Button { Text = "Save Info", BackColor = Color.FromArgb(46, 125, 50), ForeColor = Color.White, Width = 130, Height = 40, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnSave.Click += BtnSaveEmployee_Click;
            btnPanel.Controls.Add(btnSave);
            btnPanel.Controls.Add(btnCancel);

            employeeModal.Controls.Add(layout);
            employeeModal.Controls.Add(btnPanel);
            employeeModal.ShowDialog(this);
        }

        private void BtnSaveEmployee_Click(object sender, EventArgs e)
        {
            string name = txtEmpName.Text;
            string role = txtEmpRole.Text;
            string contact = txtEmpContact.Text;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(role))
            {
                MessageBox.Show("Please enter a name and role.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (currentlyUpdatingCard == null)
            {
                FileHelper.AddEmployee(name, role, contact, "default123", "Morning", "30000");
                AddNewEmployeeCard(name, role, contact);
                MessageBox.Show("Employee Added Successfully!");
            }
            else
            {
                FileHelper.UpdateEmployee(currentUpdatingEmpId, name, role, contact, "Morning", "30000");

                if (currentlyUpdatingCard.Controls[0] is TableLayoutPanel tableLayout && tableLayout.Controls[1] is FlowLayoutPanel infoPanel)
                {
                    var labels = infoPanel.Controls.OfType<Label>().ToList();
                    if (labels.Count >= 2)
                    {
                        labels[0].Text = name;
                        labels[1].Text = $"Role: {role}";
                        if (labels.Count >= 4)
                            labels[3].Text = $"Contact: {contact}";
                    }
                }
                MessageBox.Show("Employee Updated!");
            }

            employeeModal?.Close();
            currentlyUpdatingCard = null;
            currentUpdatingEmpId = "";
        }

        private void BtnAddEmployee_Click(object sender, EventArgs e)
        {
            currentlyUpdatingCard = null;
            ShowEmployeeModal("Add New Employee", "", "", "");
        }

        // ==================== MENU VIEW ====================
        private void CreateMenuView()
        {
            ViewMenu = new Panel { Visible = false, BackColor = Color.Transparent };

            FlowLayoutPanel topBar = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Height = 50,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 15)
            };

            Label lblTitle = new Label
            {
                Text = "Menu Management",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(62, 39, 35),
                AutoSize = true,
                Margin = new Padding(0, 5, 20, 0)
            };

            Button btnAddItem = new Button
            {
                Text = "+ Add New Item",
                BackColor = Color.FromArgb(111, 78, 55),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 140,
                Height = 35,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnAddItem.FlatAppearance.BorderSize = 0;
            btnAddItem.Click += BtnAddItem_Click;

            topBar.Controls.Add(lblTitle);
            topBar.Controls.Add(btnAddItem);

            Panel menuScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            Label lblCoffee = new Label { Text = "☕ Coffees", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(141, 110, 99), Dock = DockStyle.Top, Height = 30, Margin = new Padding(0, 10, 0, 5) };
            CoffeeContainer = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Dock = DockStyle.Top, Height = 200, WrapContents = true, AutoScroll = true };

            Label lblTea = new Label { Text = "🍵 Teas", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(141, 110, 99), Dock = DockStyle.Top, Height = 30, Margin = new Padding(0, 20, 0, 5) };
            TeaContainer = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Dock = DockStyle.Top, Height = 200, WrapContents = true, AutoScroll = true };

            Label lblDessert = new Label { Text = "🍰 Desserts", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(141, 110, 99), Dock = DockStyle.Top, Height = 30, Margin = new Padding(0, 20, 0, 5) };
            DessertContainer = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Dock = DockStyle.Top, Height = 200, WrapContents = true, AutoScroll = true };

            menuScroll.Controls.Add(DessertContainer);
            menuScroll.Controls.Add(lblDessert);
            menuScroll.Controls.Add(TeaContainer);
            menuScroll.Controls.Add(lblTea);
            menuScroll.Controls.Add(CoffeeContainer);
            menuScroll.Controls.Add(lblCoffee);

            ViewMenu.Controls.Add(menuScroll);
            ViewMenu.Controls.Add(topBar);
        }

        private Panel CreateMenuItemCard(string name, string price, string imagePath)
        {
            Panel card = new Panel
            {
                Width = 180,
                Height = 260,
                BackColor = Color.White,
                Margin = new Padding(10)
            };
            card.Paint += (sender, e) => MakeRoundedCorners(card, 15);

            Panel imgArea = new Panel
            {
                Height = 110,
                BackColor = Color.FromArgb(62, 39, 35),
                Dock = DockStyle.Top,
                Padding = new Padding(3)
            };
            imgArea.Paint += (sender, e) => MakeRoundedCorners(imgArea, 15);

            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                try
                {
                    PictureBox pb = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.StretchImage, Image = Image.FromFile(imagePath) };
                    imgArea.Controls.Add(pb);
                }
                catch { }
            }
            else
            {
                Label emoji = new Label { Text = "☕", ForeColor = Color.White, Font = new Font("Segoe UI", 36), TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill };
                imgArea.Controls.Add(emoji);
            }

            Button btnRemove = new Button
            {
                Text = "✖ REMOVE",
                BackColor = Color.FromArgb(160, 82, 45),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 25,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 8, FontStyle.Bold)
            };
            btnRemove.FlatAppearance.BorderSize = 0;

            string itemName = name;  // Capture the variable
            btnRemove.Click += (s, e) =>
            {
                DialogResult result = MessageBox.Show($"Remove {itemName} from menu?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    if (card.Parent is FlowLayoutPanel parent)
                    {
                        var menuItems = FileHelper.ReadMenu();
                        foreach (var item in menuItems)
                        {
                            if (item.Length >= 2 && item[1] == itemName)
                            {
                                FileHelper.DeleteMenuItem(item[0]);
                                break;
                            }
                        }
                        parent.Controls.Remove(card);
                    }
                }
            };

            Label lblName = new Label
            {
                Text = name,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(62, 39, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 35
            };

            Label lblPrice = new Label
            {
                Text = $"Rs. {price}",
                ForeColor = Color.FromArgb(141, 110, 99),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 30
            };

            card.Controls.Add(lblPrice);
            card.Controls.Add(lblName);
            card.Controls.Add(btnRemove);
            card.Controls.Add(imgArea);

            return card;
        }

        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            if (menuModal != null && !menuModal.IsDisposed)
                menuModal.Close();

            menuModal = new Form
            {
                Text = "Add New Menu Item",
                Size = new Size(450, 580),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30),
                RowCount = 9
            };

            layout.Controls.Add(new Label { Text = "Item Name", ForeColor = Color.FromArgb(141, 110, 99), Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true }, 0, 0);
            txtItemName = new TextBox { BackColor = Color.FromArgb(245, 230, 211), Height = 35, Dock = DockStyle.Fill };
            layout.Controls.Add(txtItemName, 0, 1);

            layout.Controls.Add(new Label { Text = "Category", ForeColor = Color.FromArgb(141, 110, 99), Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 15, 0, 0) }, 0, 2);
            cmbCategory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Height = 35, Dock = DockStyle.Fill };
            cmbCategory.Items.AddRange(new object[] { "Coffee", "Tea", "Dessert" });
            cmbCategory.SelectedIndex = 0;
            layout.Controls.Add(cmbCategory, 0, 3);

            layout.Controls.Add(new Label { Text = "Price (Rs.)", ForeColor = Color.FromArgb(141, 110, 99), Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 15, 0, 0) }, 0, 4);
            txtItemPrice = new TextBox { BackColor = Color.FromArgb(245, 230, 211), Height = 35, Dock = DockStyle.Fill };
            layout.Controls.Add(txtItemPrice, 0, 5);

            layout.Controls.Add(new Label { Text = "Item Image", ForeColor = Color.FromArgb(141, 110, 99), Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 15, 0, 0) }, 0, 6);
            Button btnSelectImage = new Button { Text = "📸 Select Image", BackColor = Color.FromArgb(215, 204, 200), FlatStyle = FlatStyle.Flat, Height = 35 };
            btnSelectImage.Click += (sender2, e2) =>
            {
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        selectedImagePath = dlg.FileName;
                        lblImagePath.Text = Path.GetFileName(selectedImagePath);
                    }
                }
            };
            layout.Controls.Add(btnSelectImage, 0, 7);

            lblImagePath = new Label { Text = "No image selected", ForeColor = Color.Gray, Font = new Font("Segoe UI", 9), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            layout.Controls.Add(lblImagePath, 0, 8);

            FlowLayoutPanel btnPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Height = 50, Dock = DockStyle.Bottom, Margin = new Padding(0, 20, 0, 0) };
            Button btnCancel = new Button { Text = "Cancel", BackColor = Color.Transparent, ForeColor = Color.FromArgb(141, 110, 99), Width = 90, Height = 40, FlatStyle = FlatStyle.Flat };
            btnCancel.Click += (s, ev) => menuModal.Close();
            Button btnSave = new Button { Text = "Add to Menu", BackColor = Color.FromArgb(46, 125, 50), ForeColor = Color.White, Width = 130, Height = 40, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnSave.Click += (s, ev) =>
            {
                if (string.IsNullOrEmpty(txtItemName.Text) || string.IsNullOrEmpty(txtItemPrice.Text))
                {
                    MessageBox.Show("Please enter a name and price!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Panel newCard = CreateMenuItemCard(txtItemName.Text, txtItemPrice.Text, selectedImagePath);
                string category = cmbCategory.SelectedItem.ToString();

                int prepTime = category == "Coffee" ? 120 : (category == "Tea" ? 90 : 30);
                FileHelper.AddMenuItem(txtItemName.Text, category, txtItemPrice.Text, prepTime, selectedImagePath);

                if (category == "Coffee")
                    CoffeeContainer.Controls.Add(newCard);
                else if (category == "Tea")
                    TeaContainer.Controls.Add(newCard);
                else
                    DessertContainer.Controls.Add(newCard);

                txtItemName.Clear();
                txtItemPrice.Clear();
                cmbCategory.SelectedIndex = 0;
                selectedImagePath = "";
                lblImagePath.Text = "No image selected";
                menuModal.Close();
                MessageBox.Show("Menu item added successfully!");
            };
            btnPanel.Controls.Add(btnSave);
            btnPanel.Controls.Add(btnCancel);

            menuModal.Controls.Add(layout);
            menuModal.Controls.Add(btnPanel);
            menuModal.ShowDialog(this);
        }

        // ==================== TIME VIEW ====================
        private void CreateTimeView()
        {
            ViewTime = new Panel { Visible = false, AutoScroll = true, Padding = new Padding(10), BackColor = Color.Transparent };
            Panel settingsPanel = new Panel
            {
                BackColor = Color.White,
                Width = 700,
                Height = 400,
                Padding = new Padding(30)
            };
            settingsPanel.Paint += (sender, e) => MakeRoundedCorners(settingsPanel, 20);

            Label lblHours = new Label { Text = "Cafe Operating Hours", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(62, 39, 35), Location = new Point(30, 20), AutoSize = true };

            Label lblOpen = new Label { Text = "Opening Time:", Font = new Font("Segoe UI", 12, FontStyle.Regular), ForeColor = Color.FromArgb(141, 110, 99), Location = new Point(30, 70), AutoSize = true };
            txtOpenTime = new TextBox { Text = "08:00 AM", BackColor = Color.FromArgb(245, 230, 211), Width = 150, Height = 35, Location = new Point(150, 65) };

            Label lblClose = new Label { Text = "Closing Time:", Font = new Font("Segoe UI", 12, FontStyle.Regular), ForeColor = Color.FromArgb(141, 110, 99), Location = new Point(30, 115), AutoSize = true };
            txtCloseTime = new TextBox { Text = "11:00 PM", BackColor = Color.FromArgb(245, 230, 211), Width = 150, Height = 35, Location = new Point(150, 110) };

            Label lblShift = new Label { Text = "Shift Management", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(62, 39, 35), Location = new Point(30, 170), AutoSize = true };

            chkNightShift = new CheckBox { Text = "Enable Night Shift (Weekends)", Font = new Font("Segoe UI", 12, FontStyle.Regular), ForeColor = Color.FromArgb(93, 64, 55), Location = new Point(30, 210), AutoSize = true };
            chkAutoLock = new CheckBox { Text = "Auto-Lock POS after Closing", Font = new Font("Segoe UI", 12, FontStyle.Regular), ForeColor = Color.FromArgb(93, 64, 55), Location = new Point(30, 240), AutoSize = true, Checked = true };

            var (openTime, closeTime, nightShift, autoLock) = FileHelper.LoadCafeTime();
            txtOpenTime.Text = openTime;
            txtCloseTime.Text = closeTime;
            chkNightShift.Checked = nightShift;
            chkAutoLock.Checked = autoLock;

            Button btnSave = new Button
            {
                Text = "Save Timing Settings",
                BackColor = Color.FromArgb(62, 39, 35),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 200,
                Height = 40,
                Location = new Point(30, 300),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, ev) =>
            {
                FileHelper.SaveCafeTime(txtOpenTime.Text, txtCloseTime.Text, chkNightShift.Checked, chkAutoLock.Checked);
                MessageBox.Show("Timing settings saved!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            settingsPanel.Controls.Add(lblHours);
            settingsPanel.Controls.Add(lblOpen);
            settingsPanel.Controls.Add(txtOpenTime);
            settingsPanel.Controls.Add(lblClose);
            settingsPanel.Controls.Add(txtCloseTime);
            settingsPanel.Controls.Add(lblShift);
            settingsPanel.Controls.Add(chkNightShift);
            settingsPanel.Controls.Add(chkAutoLock);
            settingsPanel.Controls.Add(btnSave);

            ViewTime.Controls.Add(settingsPanel);
        }

        // ==================== PLACEMENT VIEW ====================
        private void CreatePlacementView()
        {
            ViewPlacement = new Panel
            {
                Visible = false,
                AutoScroll = true,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(245, 230, 211)
            };

            // Header Panel
            Panel headerPanel = new Panel
            {
                Height = 60,
                BackColor = Color.White,
                Width = 730
            };

            Label lblTitle = new Label
            {
                Text = "Table Placement",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(62, 39, 35),
                Location = new Point(15, 15),
                AutoSize = true
            };

            Button btnAddTable = new Button
            {
                Text = "+ Add Table",
                BackColor = Color.FromArgb(46, 125, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 100,
                Height = 30,
                Location = new Point(200, 15),
                Cursor = Cursors.Hand
            };
            btnAddTable.FlatAppearance.BorderSize = 0;
            btnAddTable.Click += BtnAddTablePlacement_Click;

            Button btnRemoveSelected = new Button
            {
                Text = "Remove Selected",
                BackColor = Color.FromArgb(211, 47, 47),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 120,
                Height = 30,
                Location = new Point(310, 15),
                Cursor = Cursors.Hand
            };
            btnRemoveSelected.FlatAppearance.BorderSize = 0;
            btnRemoveSelected.Click += (s, e) =>
            {
                if (selectedTable is null)
                {
                    MessageBox.Show("Click a table first to select it.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string tableId = selectedTable.Tag.ToString();
                CafeMapCanvas.Controls.Remove(selectedTable);
                FileHelper.DeleteTablePlacementRecord(tableId);
                selectedTable = null;
                MessageBox.Show("Table removed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            Button btnSaveLayout = new Button
            {
                Text = "💾 Save Layout",
                BackColor = Color.FromArgb(62, 39, 35),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 100,
                Height = 30,
                Location = new Point(450, 15),
                Cursor = Cursors.Hand
            };
            btnSaveLayout.FlatAppearance.BorderSize = 0;
            btnSaveLayout.Click += BtnSaveTableLayout_Click;

            Label lblHint = new Label
            {
                Text = "(Drag to move | Click to select)",
                ForeColor = Color.FromArgb(141, 110, 99),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                Location = new Point(570, 22),
                AutoSize = true
            };

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(btnAddTable);
            headerPanel.Controls.Add(btnRemoveSelected);
            headerPanel.Controls.Add(btnSaveLayout);
            headerPanel.Controls.Add(lblHint);

            // Canvas for tables with background image
            CafeMapCanvas = new Panel
            {
                BackColor = Color.FromArgb(245, 230, 211),
                Height = 400,
                Width = 730,
                BorderStyle = BorderStyle.FixedSingle
            };

            // ========== ADD CAFE LAYOUT BACKGROUND IMAGE ==========
            string layoutImagePath = @"D:\Kisfa Javed\SDA\Buzzing_Beans\Buzzing_Beans\Resources\BLUEPRINT.png";
            if (File.Exists(layoutImagePath))
            {
                CafeMapCanvas.BackgroundImage = Image.FromFile(layoutImagePath);
                CafeMapCanvas.BackgroundImageLayout = ImageLayout.Zoom;  // Keeps proportions
            }
            // ======================================================

            ViewPlacement.Controls.Add(headerPanel);
            ViewPlacement.Controls.Add(CafeMapCanvas);

            LoadSavedTablePositions();
        }

        private void LoadSavedTablePositions()
        {
            CafeMapCanvas.Controls.Clear();
            var savedTables = FileHelper.LoadSavedTablePlacement();
            tableCounterPlacement = 0;

            foreach (FileHelper.TablePlacementData table in savedTables)
            {
                Panel tablePanel = new Panel
                {
                    Size = new Size(60, 60),
                    BackColor = Color.FromArgb(62, 39, 35),
                    Location = new Point(table.X, table.Y),
                    Tag = table.TableId,
                    Cursor = Cursors.Hand
                };
                tablePanel.Paint += (sender, e) => MakeRoundedCorners(tablePanel, 30);

                Label lblTable = new Label
                {
                    Text = table.TableId,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Enabled = false
                };
                tablePanel.Controls.Add(lblTable);

                AttachTableEventHandlers(tablePanel);
                CafeMapCanvas.Controls.Add(tablePanel);

                if (table.TableId.StartsWith("T") && int.TryParse(table.TableId.Substring(1), out int num))
                {
                    if (num > tableCounterPlacement)
                    {
                        tableCounterPlacement = num;
                    }
                }
            }
        }

        private void AttachTableEventHandlers(Panel table)
        {
            table.MouseDown += (s, ev) =>
            {
                if (ev.Button == MouseButtons.Left)
                {
                    isDraggingTable = true;
                    tableClickOffset = ev.Location;
                    if (selectedTable != null)
                    {
                        selectedTable.BackColor = Color.FromArgb(62, 39, 35);
                    }
                    selectedTable = table;
                    selectedTable.BackColor = Color.FromArgb(230, 126, 34);
                }
            };

            table.MouseMove += (s, ev) =>
            {
                if (isDraggingTable)
                {
                    table.Left += ev.X - tableClickOffset.X;
                    table.Top += ev.Y - tableClickOffset.Y;
                }
            };

            table.MouseUp += (s, ev) => isDraggingTable = false;
        }

        private void BtnAddTablePlacement_Click(object sender, EventArgs e)
        {
            tableCounterPlacement++;

            Panel table = new Panel
            {
                Size = new Size(60, 60),
                BackColor = Color.FromArgb(62, 39, 35),
                Tag = $"T{tableCounterPlacement}",
                Cursor = Cursors.Hand
            };
            table.Paint += (sender2, e2) => MakeRoundedCorners(table, 30);

            Label lblTable = new Label
            {
                Text = $"T{tableCounterPlacement}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Enabled = false
            };
            table.Controls.Add(lblTable);

            int col = (tableCounterPlacement - 1) % 4;
            int row = (tableCounterPlacement - 1) / 4;
            table.Location = new Point(20 + col * 80, 60 + row * 80);

            AttachTableEventHandlers(table);
            CafeMapCanvas.Controls.Add(table);

            SaveTablePositions();
        }

        private void BtnSaveTableLayout_Click(object sender, EventArgs e)
        {
            SaveTablePositions();
            MessageBox.Show("Table layout saved successfully! ✓", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveTablePositions()
        {
            List<FileHelper.TablePlacementData> tables = new List<FileHelper.TablePlacementData>();

            foreach (Control control in CafeMapCanvas.Controls)
            {
                if (control is Panel table)
                {
                    string tableId = table.Tag.ToString();
                    tables.Add(new FileHelper.TablePlacementData
                    {
                        TableId = tableId,
                        X = table.Location.X,
                        Y = table.Location.Y
                    });
                }
            }

            FileHelper.SaveTablePlacement(tables);
        }

        // ==================== SALES VIEW ====================
        private void CreateSalesView()
        {
            ViewSales = new Panel { Visible = false, BackColor = Color.Transparent };

            FlowLayoutPanel topBar = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Height = 40,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 15)
            };

            Label lblTitle = new Label { Text = "Sales History", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(62, 39, 35), AutoSize = true };
            Button btnExport = new Button { Text = "Export to Excel", BackColor = Color.FromArgb(141, 110, 99), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Width = 100, Height = 35, Margin = new Padding(450, 0, 0, 0) };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExportExcel_Click;

            topBar.Controls.Add(lblTitle);
            topBar.Controls.Add(btnExport);

            Panel header = new Panel { Height = 40, BackColor = Color.FromArgb(245, 230, 211), Padding = new Padding(15) };
            Label lblOrderId = new Label { Text = "ORDER ID", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(62, 39, 35), Location = new Point(15, 10), AutoSize = true };
            Label lblItems = new Label { Text = "ITEMS", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(62, 39, 35), Location = new Point(250, 10), AutoSize = true };
            Label lblTotal = new Label { Text = "TOTAL", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(62, 39, 35), Location = new Point(600, 10), AutoSize = true };
            header.Controls.Add(lblOrderId);
            header.Controls.Add(lblItems);
            header.Controls.Add(lblTotal);

            FlowLayoutPanel salesList = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = false
            };

            foreach (var sale in MySalesData)
            {
                Panel row = new Panel { Height = 40, Width = 720, Padding = new Padding(15), BackColor = Color.White };
                Label idLbl = new Label { Text = sale.OrderId, Location = new Point(15, 10), AutoSize = true };
                Label itemsLbl = new Label { Text = sale.Items, Location = new Point(250, 10), AutoSize = true };
                Label totalLbl = new Label { Text = $"Rs. {sale.Total}", Location = new Point(600, 10), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
                row.Controls.Add(idLbl);
                row.Controls.Add(itemsLbl);
                row.Controls.Add(totalLbl);
                salesList.Controls.Add(row);
            }

            ViewSales.Controls.Add(salesList);
            ViewSales.Controls.Add(header);
            ViewSales.Controls.Add(topBar);
        }

        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Sales History");
                    worksheet.Cell(1, 1).Value = "Order ID";
                    worksheet.Cell(1, 2).Value = "Items";
                    worksheet.Cell(1, 3).Value = "Total (Rs.)";

                    var headerRange = worksheet.Range("A1:C1");
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#6F4E37");
                    headerRange.Style.Font.FontColor = XLColor.White;

                    int row = 2;
                    foreach (var item in MySalesData)
                    {
                        worksheet.Cell(row, 1).Value = item.OrderId;
                        worksheet.Cell(row, 2).Value = item.Items;
                        worksheet.Cell(row, 3).Value = item.Total;
                        row++;
                    }

                    using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                    {
                        saveFileDialog.Filter = "Excel Files|*.xlsx";
                        saveFileDialog.Title = "Save Sales Report";
                        saveFileDialog.FileName = $"SalesReport_{DateTime.Now:yyyyMMdd}";
                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            workbook.SaveAs(saveFileDialog.FileName);
                            MessageBox.Show("Report exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== PROFILE MODAL ====================
        private void BtnUpdateProfile_Click(object sender, EventArgs e)
        {
            if (profileModal != null && !profileModal.IsDisposed)
                profileModal.Close();

            profileModal = new Form
            {
                Text = "Edit Profile",
                Size = new Size(400, 500),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            Panel container = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };

            ProfilePreview = new PictureBox
            {
                Size = new Size(100, 100),
                Location = new Point(130, 20),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.FromArgb(245, 230, 211)
            };
            if (imgManagerPhoto.Image != null)
                ProfilePreview.Image = imgManagerPhoto.Image;

            Button btnChangePhoto = new Button
            {
                Text = "Change Photo",
                Size = new Size(120, 30),
                Location = new Point(120, 130),
                BackColor = Color.FromArgb(215, 204, 200),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnChangePhoto.FlatAppearance.BorderSize = 0;
            btnChangePhoto.Click += (s2, e2) =>
            {
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Filter = "Images|*.png;*.jpg;*.jpeg";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        newProfileImagePath = dlg.FileName;
                        ProfilePreview.Image = Image.FromFile(newProfileImagePath);
                    }
                }
            };

            Label lblNameTitle = new Label { Text = "Full Name", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(141, 110, 99), Location = new Point(30, 180), AutoSize = true };
            txtProfileName = new TextBox
            {
                Text = lblManagerName.Text,
                Location = new Point(30, 205),
                Width = 280,
                Height = 35,
                BackColor = Color.FromArgb(245, 230, 211)
            };

            Label lblRoleTitle = new Label { Text = "Role", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(141, 110, 99), Location = new Point(30, 255), AutoSize = true };
            txtProfileRole = new TextBox
            {
                Text = lblManagerRole.Text,
                Location = new Point(30, 280),
                Width = 280,
                Height = 35,
                BackColor = Color.FromArgb(245, 230, 211)
            };

            Button btnSave = new Button
            {
                Text = "Save Changes",
                Size = new Size(130, 40),
                Location = new Point(30, 340),
                BackColor = Color.FromArgb(62, 39, 35),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;

            // ========== MODIFIED SAVE BUTTON CLICK EVENT ==========
            btnSave.Click += (s, ev) =>
            {
                string newName = txtProfileName.Text;
                string newRole = txtProfileRole.Text;

                lblManagerName.Text = newName;
                lblManagerRole.Text = newRole;

                string savedImagePath = "";

                if (!string.IsNullOrEmpty(newProfileImagePath))
                {
                    // Copy image to Data folder for persistence
                    string destPath = Path.Combine(Application.StartupPath, "Data", "manager_photo.png");
                    try
                    {
                        File.Copy(newProfileImagePath, destPath, true);
                        savedImagePath = destPath;
                        imgManagerPhoto.Image = Image.FromFile(destPath);
                    }
                    catch { savedImagePath = newProfileImagePath; }
                }
                else if (imgManagerPhoto.Image != null)
                {
                    string existingPath = Path.Combine(Application.StartupPath, "Data", "manager_photo.png");
                    if (File.Exists(existingPath))
                        savedImagePath = existingPath;
                }

                FileHelper.SaveManagerProfile(newName, newRole, savedImagePath);

                profileModal.Close();
                MessageBox.Show("Profile updated successfully!");
            };
            // ========================================================

            Button btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(100, 40),
                Location = new Point(180, 340),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Gray,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, ev) => profileModal.Close();

            container.Controls.Add(ProfilePreview);
            container.Controls.Add(btnChangePhoto);
            container.Controls.Add(lblNameTitle);
            container.Controls.Add(txtProfileName);
            container.Controls.Add(lblRoleTitle);
            container.Controls.Add(txtProfileRole);
            container.Controls.Add(btnSave);
            container.Controls.Add(btnCancel);

            profileModal.Controls.Add(container);
            profileModal.ShowDialog(this);
        }

        // ==================== NAVIGATION ====================
        private void HideAllViews()
        {
            ViewHome.Visible = false;
            ViewEmployees.Visible = false;
            ViewMenu.Visible = false;
            ViewTime.Visible = false;
            ViewPlacement.Visible = false;
            ViewSales.Visible = false;
        }

        private void BtnHome_Click(object sender, EventArgs e)
        {
            HideAllViews();
            ViewHome.Visible = true;
            lblPageTitle.Text = "Welcome, Shazuryan";
        }

        private void BtnEmployees_Click(object sender, EventArgs e)
        {
            HideAllViews();
            ViewEmployees.Visible = true;
            lblPageTitle.Text = "Manage Employees";
        }

        private void BtnMenu_Click(object sender, EventArgs e)
        {
            HideAllViews();
            ViewMenu.Visible = true;
            lblPageTitle.Text = "Manage Menu";
        }

        private void BtnTime_Click(object sender, EventArgs e)
        {
            HideAllViews();
            ViewTime.Visible = true;
            lblPageTitle.Text = "Cafe Time";
        }

        private void BtnPlacement_Click(object sender, EventArgs e)
        {
            HideAllViews();
            ViewPlacement.Visible = true;
            lblPageTitle.Text = "Table Placement";
            LoadSavedTablePositions();
        }

        private void BtnSales_Click(object sender, EventArgs e)
        {
            HideAllViews();
            ViewSales.Visible = true;
            lblPageTitle.Text = "Sales Reports";
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            LoginForm login = new LoginForm();
            login.Show();
            this.Close();
        }

        // ==================== HELPER CLASS ====================
        public class SaleRecord
        {
            public string OrderId { get; set; }
            public string Items { get; set; }
            public string Total { get; set; }
        }
    }
}