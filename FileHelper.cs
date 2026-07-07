using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Buzzing_Beans
{
    public static class FileHelper
    {
        private static string dataPath = Path.Combine(Application.StartupPath, "Data");
        private static string DataFolder => dataPath;

        // Ensure Data folder exists
        public static void EnsureDataFolder()
        {
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
        }

        // ========== MEMBERS ==========
        public static List<string[]> ReadMembers()
        {
            EnsureDataFolder();
            string filePath = Path.Combine(dataPath, "members.txt");
            List<string[]> members = new List<string[]>();

            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                for (int i = 1; i < lines.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(lines[i]))
                    {
                        members.Add(lines[i].Split(','));
                    }
                }
            }
            return members;
        }

        public static bool MemberExists(string name, string email)
        {
            var members = ReadMembers();
            foreach (var member in members)
            {
                if (member.Length >= 2)
                {
                    if (member[1].Equals(name, StringComparison.OrdinalIgnoreCase))
                        return true;
                    if (member.Length > 2 && member[2].Equals(email, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            return false;
        }

        public static string AddMember(string name, string phone, string password)
        {
            EnsureDataFolder();
            string filePath = Path.Combine(dataPath, "members.txt");
            var members = ReadMembers();
            int newId = members.Count + 1;
            string memberId = $"MBR{newId:D3}";
            string newLine = $"{memberId},{name},{phone},{password},0,{DateTime.Now:yyyy-MM-dd}";

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "MemberID,Name,Phone,Password,Points,JoinDate\n");
            }
            File.AppendAllText(filePath, newLine + "\n");

            return memberId;
        }

        public static bool ValidateMember(string memberId, string password)
        {
            var members = ReadMembers();
            foreach (var member in members)
            {
                if (member.Length >= 4 && member[0] == memberId && member[3] == password)
                {
                    return true;
                }
            }
            return false;
        }

        // ========== EMPLOYEES ==========
        public static List<string[]> ReadEmployees()
        {
            EnsureDataFolder();
            string filePath = Path.Combine(dataPath, "employees.txt");
            List<string[]> employees = new List<string[]>();

            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                for (int i = 1; i < lines.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(lines[i]))
                    {
                        employees.Add(lines[i].Split(','));
                    }
                }
            }
            else
            {
                // Create default employees file
                string header = "EmpID,Name,Role,Contact,Password,Shift,Salary";
                string[] defaultEmployees = {
                    header,
                    "E001,Shazuryan,System Admin,+92 300 1234567,admin123,Morning,50000",
                    "E002,Ali Khan,Head Barista,+92 300 7654321,bar123,Morning,30000"
                };
                File.WriteAllLines(filePath, defaultEmployees);
                for (int i = 1; i < defaultEmployees.Length; i++)
                {
                    employees.Add(defaultEmployees[i].Split(','));
                }
            }
            return employees;
        }

        public static void AddEmployee(string name, string role, string contact, string password, string shift, string salary)
        {
            string filePath = Path.Combine(dataPath, "employees.txt");
            var employees = ReadEmployees();
            int newId = employees.Count + 1;
            string empId = $"E{newId:D3}";
            string newLine = $"{empId},{name},{role},{contact},{password},{shift},{salary}";
            File.AppendAllText(filePath, newLine + "\n");
        }

        public static void UpdateEmployee(string empId, string name, string role, string contact, string shift, string salary)
        {
            string filePath = Path.Combine(dataPath, "employees.txt");
            var lines = File.ReadAllLines(filePath).ToList();
            for (int i = 1; i < lines.Count; i++)
            {
                if (lines[i].StartsWith(empId + ","))
                {
                    var parts = lines[i].Split(',');
                    parts[1] = name;
                    parts[2] = role;
                    parts[3] = contact;
                    parts[5] = shift;
                    parts[6] = salary;
                    lines[i] = string.Join(",", parts);
                    break;
                }
            }
            File.WriteAllLines(filePath, lines);
        }
        // ========== MANAGER PROFILE ==========
        public static void SaveManagerProfile(string name, string role, string imagePath)
        {
            EnsureDataFolder();
            string filePath = Path.Combine(dataPath, "manager_profile.txt");
            string content = $"Name={name}\nRole={role}\nImagePath={imagePath}";
            File.WriteAllText(filePath, content);
        }

        public static (string name, string role, string imagePath) LoadManagerProfile()
        {
            string filePath = Path.Combine(dataPath, "manager_profile.txt");
            string name = "Shazuryan";
            string role = "System Administrator";
            string imagePath = "";

            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    if (line.StartsWith("Name="))
                        name = line.Substring(5);
                    else if (line.StartsWith("Role="))
                        role = line.Substring(5);
                    else if (line.StartsWith("ImagePath="))
                        imagePath = line.Substring(10);
                }
            }
            return (name, role, imagePath);
        }
        public static void DeleteEmployee(string empId)
        {
            string filePath = Path.Combine(dataPath, "employees.txt");
            var lines = File.ReadAllLines(filePath).ToList();
            lines.RemoveAll(line => line.StartsWith(empId + ","));
            File.WriteAllLines(filePath, lines);
        }

        // ========== MENU ==========
        public static List<string[]> ReadMenu()
        {
            EnsureDataFolder();
            string filePath = Path.Combine(dataPath, "menu.txt");
            List<string[]> menu = new List<string[]>();

            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                for (int i = 1; i < lines.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(lines[i]))
                    {
                        menu.Add(lines[i].Split(','));
                    }
                }
            }
            else
            {
                // Create default menu file
                string header = "ItemID,Name,Category,Price,PrepTimeSeconds,ImagePath";
                string[] defaultMenu = {
                    header,
                    "M001,Caramel Latte,Coffee,550,120,",
                    "M002,Espresso,Coffee,450,45,",
                    "M003,Green Tea,Tea,300,90,",
                    "M004,Chocolate Cake,Dessert,400,30,"
                };
                File.WriteAllLines(filePath, defaultMenu);
                for (int i = 1; i < defaultMenu.Length; i++)
                {
                    menu.Add(defaultMenu[i].Split(','));
                }
            }
            return menu;
        }

        public static void AddMenuItem(string name, string category, string price, int prepTime, string imagePath)
        {
            string filePath = Path.Combine(dataPath, "menu.txt");
            var menu = ReadMenu();
            int newId = menu.Count + 1;
            string itemId = $"M{newId:D3}";
            string newLine = $"{itemId},{name},{category},{price},{prepTime},{imagePath}";
            File.AppendAllText(filePath, newLine + "\n");
        }

        public static void DeleteMenuItem(string itemId)
        {
            string filePath = Path.Combine(dataPath, "menu.txt");
            var lines = File.ReadAllLines(filePath).ToList();
            lines.RemoveAll(line => line.StartsWith(itemId + ","));
            File.WriteAllLines(filePath, lines);
        }

        // ========== CAFE TIME ==========
        public static void SaveCafeTime(string openTime, string closeTime, bool nightShift, bool autoLock)
        {
            EnsureDataFolder();
            string filePath = Path.Combine(dataPath, "timing.txt");
            string content = $"OpeningTime={openTime}\nClosingTime={closeTime}\nNightShift={nightShift}\nAutoLock={autoLock}";
            File.WriteAllText(filePath, content);
        }

        public static (string openTime, string closeTime, bool nightShift, bool autoLock) LoadCafeTime()
        {
            string filePath = Path.Combine(dataPath, "timing.txt");
            string openTime = "08:00 AM";
            string closeTime = "11:00 PM";
            bool nightShift = false;
            bool autoLock = true;

            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    if (line.StartsWith("OpeningTime="))
                        openTime = line.Substring(12);
                    else if (line.StartsWith("ClosingTime="))
                        closeTime = line.Substring(12);
                    else if (line.StartsWith("NightShift="))
                        bool.TryParse(line.Substring(11), out nightShift);
                    else if (line.StartsWith("AutoLock="))
                        bool.TryParse(line.Substring(9), out autoLock);
                }
            }
            return (openTime, closeTime, nightShift, autoLock);
        }

        // ========== ORDERS (for Barista Take Order) ==========
        public static void AddNewOrder(string orderId, string orderDetails, double total)
        {
            EnsureDataFolder();
            string filePath = Path.Combine(dataPath, "new_orders.txt");
            string newLine = $"{orderId},{orderDetails},{total}";
            File.AppendAllText(filePath, newLine + "\n");
        }

        // ========== SALES ==========
        public static void AddSaleRecord(string orderId, string items, string total)
        {
            EnsureDataFolder();
            string filePath = Path.Combine(dataPath, "sales.txt");
            string newLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{orderId},{items},{total}";

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "DateTime,OrderId,Items,Total\n");
            }
            File.AppendAllText(filePath, newLine + "\n");
        }

        public static List<string[]> ReadSales()
        {
            EnsureDataFolder();
            string filePath = Path.Combine(dataPath, "sales.txt");
            List<string[]> sales = new List<string[]>();

            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                for (int i = 1; i < lines.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(lines[i]))
                    {
                        sales.Add(lines[i].Split(','));
                    }
                }
            }
            return sales;
        }

        // ========== TABLE PLACEMENT (SINGLE VERSION) ==========
        public static void SaveTablePlacement(List<(int x, int y, string tag, string backColor)> tables)
        {
            EnsureDataFolder();
            string filePath = Path.Combine(dataPath, "placement.txt");
            List<string> lines = new List<string>();
            lines.Add("X,Y,Tag,BackColor");
            foreach (var table in tables)
            {
                lines.Add($"{table.x},{table.y},{table.tag},{table.backColor}");
            }
            File.WriteAllLines(filePath, lines);
        }
        // ========== TABLE PLACEMENT WITH TABLEPLACEMENTDATA CLASS ==========
        public class TablePlacementData
        {
            public string TableId { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
        }

        public static void SaveTablePlacement(List<TablePlacementData> tables)
        {
            try
            {
                EnsureDataFolder();
                string filePath = Path.Combine(dataPath, "table_placement.txt");
                List<string> lines = new List<string>();

                foreach (var table in tables)
                {
                    lines.Add($"{table.TableId}|{table.X}|{table.Y}");
                }

                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving table placement: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static List<TablePlacementData> LoadSavedTablePlacement()
        {
            try
            {
                EnsureDataFolder();
                string filePath = Path.Combine(dataPath, "table_placement.txt");
                List<TablePlacementData> tables = new List<TablePlacementData>();

                if (File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            string[] parts = line.Split('|');
                            if (parts.Length == 3 && int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
                            {
                                tables.Add(new TablePlacementData { TableId = parts[0], X = x, Y = y });
                            }
                        }
                    }
                }

                return tables;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading table placement: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<TablePlacementData>();
            }
        }

        public static void DeleteTablePlacementRecord(string tableId)
        {
            try
            {
                EnsureDataFolder();
                string filePath = Path.Combine(dataPath, "table_placement.txt");
                if (File.Exists(filePath))
                {
                    var tables = LoadSavedTablePlacement();
                    tables = tables.Where(t => t.TableId != tableId).ToList();
                    SaveTablePlacement(tables);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting table: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static List<(int x, int y, string tag, string backColor)> LoadTablePlacement()
        {
            string filePath = Path.Combine(dataPath, "placement.txt");
            List<(int x, int y, string tag, string backColor)> tables = new List<(int, int, string, string)>();

            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                for (int i = 1; i < lines.Length; i++)
                {
                    var parts = lines[i].Split(',');
                    if (parts.Length >= 4)
                    {
                        if (int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                        {
                            tables.Add((x, y, parts[2], parts[3]));
                        }
                    }
                }
            }
            return tables;
        }

        // ========== TABLE PLACEMENT WITH STRING IDs ==========
        public static void SaveTablePlacementData(List<(string tableId, int x, int y)> tables)
        {
            EnsureDataFolder();
            string filePath = Path.Combine(dataPath, "table_placement.txt");
            List<string> lines = new List<string>();

            foreach (var table in tables)
            {
                lines.Add($"{table.tableId}|{table.x}|{table.y}");
            }

            File.WriteAllLines(filePath, lines);
        }

        public static List<(string tableId, int x, int y)> LoadTablePlacementData()
        {
            string filePath = Path.Combine(dataPath, "table_placement.txt");
            List<(string, int, int)> tables = new List<(string, int, int)>();

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        string[] parts = line.Split('|');
                        if (parts.Length == 3)
                        {
                            if (int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
                            {
                                tables.Add((parts[0], x, y));
                            }
                        }
                    }
                }
            }

            return tables;
        }

        public static void DeleteTablePlacement(string tableId)
        {
            try
            {
                string filePath = Path.Combine(dataPath, "table_placement.txt");
                if (File.Exists(filePath))
                {
                    var tables = LoadTablePlacementData();
                    tables = tables.Where(t => t.tableId != tableId).ToList();
                    SaveTablePlacementData(tables);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting table: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}