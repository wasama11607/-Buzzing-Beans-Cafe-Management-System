using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Buzzing_Beans
{
    public class CustomerOrderModel
    {
        public string OrderId { get; set; }
        public string OrderDetails { get; set; }
        public string Status { get; set; }
        public double TotalBill { get; set; }
        public int EstimatedSeconds { get; set; }
        public DateTime OrderTime { get; set; }
    }

    public partial class TakeOrder : Form
    {
        private List<CustomerOrderModel> incomingOrders;
        private string imagesFolderPath;
        private System.Windows.Forms.Timer refreshTimer;

        private Panel leftBrownPanel;
        private Panel whiteBoxPanel;
        private FlowLayoutPanel queueCardsFlowPanel;
        private Label lblActiveOrdersCount;
        private Label lblCompletedToday;

        public TakeOrder()
        {
            SetCorrectImagePath();
            InitializeFormLayout();

            // Fallback initialization if helper isn't called externally
            string dataDir = Path.Combine(Application.StartupPath, "Data");
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

            LoadOrdersFromFile();
            PopulateOrdersGrid();

            // Unified App Timer (Handles all countdown updates gracefully)
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 1000;
            refreshTimer.Tick += RefreshTimers;
            refreshTimer.Start();
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

        private Image LoadImageSafely(string fileName)
        {
            try
            {
                string path = Path.Combine(imagesFolderPath, fileName);
                return File.Exists(path) ? Image.FromFile(path) : null;
            }
            catch { return null; }
        }

        private void LoadOrdersFromFile()
        {
            incomingOrders = new List<CustomerOrderModel>();
            var existingOrderIds = new HashSet<string>();

            string dataFolder = Path.Combine(Application.StartupPath, "Data");
            string newOrdersPath = Path.Combine(dataFolder, "new_orders.txt");
            string pendingFilePath = Path.Combine(dataFolder, "pending_orders.txt");

            // 1. First read existing ongoing operations from pending_orders.txt
            if (File.Exists(pendingFilePath))
            {
                var lines = File.ReadAllLines(pendingFilePath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("OrderId")) continue;

                    var parts = line.Split(',');
                    if (parts.Length >= 6)
                    {
                        string orderId = parts[0].Trim();
                        if (!existingOrderIds.Contains(orderId))
                        {
                            string orderDetails = parts[1].Trim().Replace("[NL]", "\n");
                            string status = parts[2].Trim();
                            double.TryParse(parts[3], out double totalBill);
                            int.TryParse(parts[4], out int estimatedSeconds);
                            DateTime.TryParse(parts[5], out DateTime orderTime);

                            if (status != "Completed" && totalBill > 0)
                            {
                                incomingOrders.Add(new CustomerOrderModel
                                {
                                    OrderId = orderId,
                                    OrderDetails = orderDetails,
                                    Status = status,
                                    TotalBill = totalBill,
                                    EstimatedSeconds = estimatedSeconds,
                                    OrderTime = orderTime == DateTime.MinValue ? DateTime.Now : orderTime
                                });
                                existingOrderIds.Add(orderId);
                            }
                        }
                    }
                }
            }

            // 2. Read brand new incoming items from external checkout terminals
            if (File.Exists(newOrdersPath))
            {
                var lines = File.ReadAllLines(newOrdersPath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(',');
                    if (parts.Length >= 3)
                    {
                        string orderId = parts[0].Trim();
                        string orderDetails = parts[1].Trim().Replace("[NL]", "\n");
                        double.TryParse(parts[2], out double totalBill);

                        if (!existingOrderIds.Contains(orderId) && totalBill > 0)
                        {
                            incomingOrders.Add(new CustomerOrderModel
                            {
                                OrderId = orderId,
                                OrderDetails = orderDetails,
                                Status = "Pending",
                                TotalBill = totalBill,
                                EstimatedSeconds = CalculateEstimatedTime(orderDetails),
                                OrderTime = DateTime.Now
                            });
                            existingOrderIds.Add(orderId);
                        }
                    }
                }
            }

            // 3. Fallback to Sample placeholder if environment is empty
            if (incomingOrders.Count == 0)
            {
                incomingOrders.Add(new CustomerOrderModel
                {
                    OrderId = "TEST-001",
                    OrderDetails = "2x Caffe Latte\n1x Chocolate Cake",
                    Status = "Pending",
                    TotalBill = 1250,
                    EstimatedSeconds = 180,
                    OrderTime = DateTime.Now
                });
            }

            // Flush structures safely back to disk state
            SavePendingOrders();
            ClearNewOrdersFile();
        }

        private int CalculateEstimatedTime(string orderDetails)
        {
            int estimatedSeconds = 90;
            if (string.IsNullOrEmpty(orderDetails)) return estimatedSeconds;

            if (orderDetails.Contains("Latte") || orderDetails.Contains("Espresso") ||
                orderDetails.Contains("Coffee") || orderDetails.Contains("Cappuccino") ||
                orderDetails.Contains("Americano") || orderDetails.Contains("Mocha"))
            {
                estimatedSeconds += 120;
            }
            if (orderDetails.Contains("Tea") || orderDetails.Contains("Chai"))
            {
                estimatedSeconds += 90;
            }
            if (orderDetails.Contains("Cake") || orderDetails.Contains("Muffin") ||
                orderDetails.Contains("Macaron") || orderDetails.Contains("Cheese"))
            {
                estimatedSeconds += 60;
            }
            return estimatedSeconds;
        }

        private void SavePendingOrders()
        {
            try
            {
                string pendingFilePath = Path.Combine(Application.StartupPath, "Data", "pending_orders.txt");
                var lines = new List<string> { "OrderId,OrderDetails,Status,TotalBill,EstimatedSeconds,OrderTime" };

                foreach (var order in incomingOrders.Where(o => o.Status != "Completed"))
                {
                    // Clean multi-line strings for inline safe structural file saving
                    string escapedDetails = order.OrderDetails.Replace("\r", "").Replace("\n", "[NL]");
                    lines.Add($"{order.OrderId},{escapedDetails},{order.Status},{order.TotalBill},{order.EstimatedSeconds},{order.OrderTime:yyyy-MM-dd HH:mm:ss}");
                }
                File.WriteAllLines(pendingFilePath, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving database files: {ex.Message}");
            }
        }

        private void ClearNewOrdersFile()
        {
            try
            {
                string newOrdersPath = Path.Combine(Application.StartupPath, "Data", "new_orders.txt");
                if (File.Exists(newOrdersPath)) File.WriteAllText(newOrdersPath, string.Empty);
            }
            catch { }
        }

        private void RefreshTimers(object sender, EventArgs e)
        {
            if (queueCardsFlowPanel == null) return;

            bool structuralChangesNeeded = false;

            foreach (Control card in queueCardsFlowPanel.Controls)
            {
                if (card.Tag is TimerInfo ti)
                {
                    if (ti.Order.Status == "Cooking")
                    {
                        ti.Order.EstimatedSeconds--;
                        if (ti.Order.EstimatedSeconds <= 0)
                        {
                            ti.Order.EstimatedSeconds = 0;
                            ti.Order.Status = "Ready";
                            structuralChangesNeeded = true;
                        }
                        else
                        {
                            ti.UpdateDisplay();
                        }
                    }
                }
            }

            // Re-render display layout cleanly if any working items finished preparation naturally
            if (structuralChangesNeeded)
            {
                SavePendingOrders();
                PopulateOrdersGrid();
            }
        }

        private void InitializeFormLayout()
        {
            this.Text = "Take Order - Cafe Management System";
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true;
            this.BackgroundImage = LoadImageSafely("background.png");
            this.BackgroundImageLayout = ImageLayout.Stretch;

            leftBrownPanel = new Panel { Dock = DockStyle.Left, Width = 220, BackgroundImage = LoadImageSafely("Brown Background.png"), BackgroundImageLayout = ImageLayout.Stretch };
            this.Controls.Add(leftBrownPanel);

            Button btnBack = new Button { Text = "←", Size = new Size(80, 40), Location = new Point(20, 20), BackColor = Color.Transparent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 20, FontStyle.Bold), Cursor = Cursors.Hand };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) => {
                refreshTimer.Stop();
                SavePendingOrders();
                this.Close();
            };
            leftBrownPanel.Controls.Add(btnBack);

            Panel statsPanel = new Panel { Location = new Point(10, 500), Size = new Size(200, 150), BackColor = Color.Transparent };
            lblActiveOrdersCount = new Label { Text = "Active: 0", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 20), AutoSize = true };
            lblCompletedToday = new Label { Text = "Completed: 0", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 60), AutoSize = true };
            statsPanel.Controls.AddRange(new Control[] { lblActiveOrdersCount, lblCompletedToday });
            leftBrownPanel.Controls.Add(statsPanel);

            whiteBoxPanel = new Panel { Location = new Point(250, 60), Size = new Size(900, 700), BackColor = Color.White };
            whiteBoxPanel.Paint += (s, e) => DrawRoundedRectangle(whiteBoxPanel, e.Graphics, 20);
            this.Controls.Add(whiteBoxPanel);

            Label lblTitle = new Label { Text = "Live Kitchen Queue", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Color.FromArgb(62, 39, 35), Location = new Point(30, 20), AutoSize = true };
            whiteBoxPanel.Controls.Add(lblTitle);

            Button btnRefresh = new Button { Text = "⟳ REFRESH", Size = new Size(100, 35), Location = new Point(750, 25), BackColor = Color.FromArgb(62, 39, 35), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => { LoadOrdersFromFile(); PopulateOrdersGrid(); };
            whiteBoxPanel.Controls.Add(btnRefresh);

            Panel statsRow = new Panel { Location = new Point(30, 60), Size = new Size(840, 40) };
            Label lblPendingCount = new Label { Text = "Pending Orders", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(141, 110, 99), Location = new Point(0, 10), AutoSize = true };
            statsRow.Controls.Add(lblPendingCount);
            whiteBoxPanel.Controls.Add(statsRow);

            queueCardsFlowPanel = new FlowLayoutPanel { Location = new Point(30, 110), Size = new Size(840, 560), AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(10) };
            whiteBoxPanel.Controls.Add(queueCardsFlowPanel);
        }

        private void DrawRoundedRectangle(Panel panel, Graphics g, int radius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                Rectangle rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                int diameter = radius * 2;
                path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
                panel.Region = new Region(path);
            }
        }

        private void PopulateOrdersGrid()
        {
            if (queueCardsFlowPanel == null) return;
            queueCardsFlowPanel.Controls.Clear();

            Color darkBrown = Color.FromArgb(62, 39, 35);
            Color pendingColor = Color.FromArgb(210, 180, 140);
            Color cookingColor = Color.FromArgb(255, 193, 7);
            Color readyColor = Color.FromArgb(76, 175, 80);

            var activeOrders = incomingOrders.Where(o => o.Status != "Completed" && o.TotalBill > 0).ToList();

            if (!activeOrders.Any())
            {
                Label noOrdersMsg = new Label
                {
                    Text = "No active orders.\n\nIncoming terminal items appear automatically here.",
                    Font = new Font("Segoe UI", 14, FontStyle.Regular),
                    ForeColor = Color.Gray,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(800, 100)
                };
                queueCardsFlowPanel.Controls.Add(noOrdersMsg);
                UpdateStats();
                return;
            }

            foreach (var order in activeOrders)
            {
                Color cardBgColor = pendingColor;
                if (order.Status == "Cooking") cardBgColor = cookingColor;
                else if (order.Status == "Ready") cardBgColor = readyColor;

                Panel card = new Panel { Size = new Size(250, 230), BackColor = cardBgColor, Margin = new Padding(12) };

                Label lblId = new Label { Text = order.OrderId, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(12, 12), AutoSize = true, ForeColor = darkBrown };

                Label lblStatus = new Label
                {
                    Text = order.Status,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Location = new Point(160, 14),
                    Size = new Size(80, 25),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = order.Status == "Pending" ? Color.Orange : (order.Status == "Cooking" ? Color.Gold : Color.Green),
                    ForeColor = (order.Status == "Cooking") ? darkBrown : Color.White
                };

                Label lblDetails = new Label { Text = order.OrderDetails, Location = new Point(12, 55), Size = new Size(225, 65), ForeColor = darkBrown, Font = new Font("Segoe UI", 9, FontStyle.Regular) };
                Label lblTimer = new Label { Location = new Point(12, 130), Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.DarkRed, AutoSize = true };
                Label lblTotal = new Label { Text = $"Rs. {order.TotalBill:F0}", Location = new Point(12, 160), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = darkBrown, AutoSize = true };

                Button btnAction = new Button
                {
                    Text = order.Status == "Pending" ? "START" : (order.Status == "Cooking" ? "READY" : "COMPLETE"),
                    Size = new Size(100, 35),
                    Location = new Point(130, 170),
                    BackColor = order.Status == "Ready" ? Color.FromArgb(33, 150, 243) : Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btnAction.FlatAppearance.BorderSize = 0;

                // Load accurate initial contextual timer text strings
                if (order.Status == "Cooking")
                    lblTimer.Text = $"Est: {order.EstimatedSeconds / 60}:{order.EstimatedSeconds % 60:D2}";
                else if (order.Status == "Ready")
                    lblTimer.Text = "✓ READY!";
                else
                    lblTimer.Text = "Awaiting Prep";

                btnAction.Click += (s, e) =>
                {
                    if (order.Status == "Pending")
                    {
                        order.Status = "Cooking";
                        SavePendingOrders();
                        PopulateOrdersGrid();
                    }
                    else if (order.Status == "Cooking")
                    {
                        order.EstimatedSeconds = 0;
                        order.Status = "Ready";
                        SavePendingOrders();
                        PopulateOrdersGrid();
                    }
                    else if (order.Status == "Ready")
                    {
                        order.Status = "Completed";
                        SavePendingOrders();
                        PopulateOrdersGrid();
                    }
                };

                card.Controls.AddRange(new Control[] { lblId, lblStatus, lblDetails, lblTimer, lblTotal, btnAction });
                queueCardsFlowPanel.Controls.Add(card);

                // Assign unified runtime context information directly onto UI containers
                card.Tag = new TimerInfo { Label = lblTimer, Order = order };
            }

            UpdateStats();
        }

        private void UpdateStats()
        {
            int activeCount = incomingOrders.Count(o => o.Status != "Completed" && o.TotalBill > 0);
            int completedCount = incomingOrders.Count(o => o.Status == "Completed");

            lblActiveOrdersCount.Text = $"Active: {activeCount}";
            lblCompletedToday.Text = $"Completed: {completedCount}";
        }

        private class TimerInfo
        {
            public Label Label { get; set; }
            public CustomerOrderModel Order { get; set; }

            public void UpdateDisplay()
            {
                if (Order.Status == "Cooking" && Order.EstimatedSeconds > 0)
                {
                    Label.Text = $"Est: {Order.EstimatedSeconds / 60}:{Order.EstimatedSeconds % 60:D2}";
                }
            }
        }
    }
}