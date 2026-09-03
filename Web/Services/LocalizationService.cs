using System.Globalization;

namespace Web.Services
{
    public interface ILocalizationService
    {
        string GetString(string key);
        string GetString(string key, bool isArabic);
        bool IsArabic { get; }
    }

    public class LocalizationService : ILocalizationService
    {
        private static readonly Dictionary<string, (string En, string Ar)> _translations = CreateTranslations();

        private static Dictionary<string, (string En, string Ar)> CreateTranslations()
        {
            var dict = new Dictionary<string, (string En, string Ar)>(StringComparer.OrdinalIgnoreCase);

            void Add(string key, string en, string ar)
            {
                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, (en, ar));
                }
            }

            // Navigation Sidebar Links
            Add("Dashboard", "Dashboard", "لوحة التحكم الرئيسية");
            Add("Rental Fleet Catalog", "Rental Fleet Catalog", "كتالوج أسطول الإيجارات");
            Add("Sales Showroom", "Sales Showroom", "معرض بيع السيارات");
            Add("Rental Contracts", "Rental Contracts", "عقود الإيجار");
            Add("Car Sales Ledger", "Car Sales Ledger", "سجل مبيعات السيارات");
            Add("Customers", "Customers", "إدارة العملاء");
            Add("Payment Ledger", "Payment Ledger", "سجل المقبوضات والمدفوعات");
            Add("Tax Invoices", "Tax Invoices", "الفواتير الضريبية");
            Add("Maintenance", "Maintenance", "صيانة وإصلاح الأسطول");
            Add("GPS Live Tracking", "GPS Live Tracking", "التتبع المباشر عبر GPS");
            Add("Audit Monitor", "Audit Monitor", "سجلات ومراقبة النظام");
            Add("Roles & Permissions", "Roles & Permissions", "إدارة الصلاحيات والأدوار");

            Add("Enterprise Portal", "Enterprise Portal", "بوابة إدارة المؤسسة");
            Add("System Operational Alerts", "System Operational Alerts", "تنبيهات التشغيل والعمليات");
            Add("Overdue Rental Contracts", "Overdue Rental Contracts", "عقود إيجار متأخرة التسليم");
            Add("Vehicles in Maintenance", "Vehicles in Maintenance", "سيارات داخل ورش الصيانة");
            Add("Profile", "Profile", "الملف الشخصي");
            Add("Sign Out", "Sign Out", "تسجيل الخروج");
            Add("Light Mode", "Light Mode", "الثيم الأبيض");
            Add("Dark Mode", "Dark Mode", "الثيم الداكن");
            Add("Add New Vehicle", "Add New Vehicle", "إضافة سيارة جديدة");
            Add("Sell Vehicle", "Sell Vehicle", "بيع سيارة (عقد جديد)");
            Add("Total Sales Value", "Total Sales Value", "إجمالي قيمة المبيعات");
            Add("Net Gross Profit", "Net Gross Profit", "صافي أرباح المعرض");
            Add("Installments Receivable", "Installments Receivable", "الأقساط المستحقة القادمة");
            Add("Cash / Financing Ratio", "Cash / Financing Ratio", "نسبة الكاش / التقسيط");
            Add("Fleet Utilization", "Fleet Utilization", "نسبة تشغيل الأسطول");
            Add("Total Rentals", "Total Rentals", "إجمالي عقود الإيجار");
            Add("Active Rentals", "Active Rentals", "العقود النشطة حالياً");
            Add("Total Revenue", "Total Revenue", "إجمالي إيرادات النظام");
            Add("Net Revenue", "Net Revenue", "صافي الدخل الربحي");
            Add("All Fleet Vehicles", "All Fleet Vehicles", "جميع سيارات الأسطول");
            Add("Rental Fleet", "Rental Fleet", "أسطول الإيجارات فقط");
            Add("Search...", "Search...", "بحث سريع...");
            Add("Save", "Save", "حفظ البيانات");
            Add("Cancel", "Cancel", "إلغاء");
            Add("Details", "Details", "عرض التفاصيل");
            Add("Print Agreement", "Print Agreement", "طباعة العقد الرسمي");
            Add("Collect Payment", "Collect Payment", "تحصيل الدفعة");
            Add("Available", "Available", "متاحة للإيجار");
            Add("Rented", "Rented", "مؤجرة حالياً");
            Add("Maintenance Status", "Maintenance", "تحت الصيانة");
            Add("OutOfService", "OutOfService", "خارج الخدمة / مباعة");
            Add("ForSale", "For Sale", "معروضة للبيع");
            Add("Reserved", "Reserved", "محجوزة بالدفعة المقدمة");
            Add("Sold", "Sold", "تم البيع ونقل الملكية");
            Add("Plate Number", "Plate Number", "رقم اللوحة");
            Add("Vehicle Model", "Vehicle Model", "موديل السيارة");
            Add("Daily Rate", "Daily Rate", "السعر اليومي");
            Add("Asking Price", "Asking Price", "السعر المعلن للبيع");
            Add("Floor Price", "Min Floor Price", "أدنى سعر للتفاوض");
            Add("Total Cost Basis", "Total Cost Basis", "إجمالي التكلفة الشاملة");
            Add("Est Gross Profit", "Est Gross Profit", "الربح التقديري المتوقع");
            Add("Customer Name", "Customer Name", "اسم العميل");
            Add("Phone Number", "Phone Number", "رقم الهاتف");
            Add("Driving License", "Driving License", "رقم رخصة القيادة");
            Add("Sale Date", "Sale Date", "تاريخ البيع");
            Add("Start Date", "Start Date", "تاريخ بداية الإيجار");
            Add("End Date", "End Date", "تاريخ نهاية الإيجار");
            Add("Status", "Status", "حالة العملية");
            Add("Actions", "Actions", "الإجراءات");
            Add("Cash Sale", "Cash Sale", "بيع كاش");
            Add("Installments Sale", "Installment Sale", "بيع تقسيط");
            Add("Paid Amount", "Paid Amount", "المبلغ المدفوع");
            Add("Remaining Balance", "Remaining Balance", "المبلغ المتبقي");
            Add("Overview & Fleet Performance", "Overview & Fleet Performance", "نظرة عامة وأداء الأسطول");
            Add("Recent Payments Ledger", "Recent Payments Ledger", "أحدث العمليات المباشرة");
            Add("Upcoming Returns", "Upcoming Returns", "المواعيد القادمة لتسليم السيارات");
            Add("Top Performing Vehicles", "Top Performing Vehicles", "السيارات الأكثر ربحية وتأجيراً");
            Add("Dealership Showroom Inventory", "Dealership Showroom Inventory", "معرض بيع وشراء السيارات");
            Add("Open Vehicle Sale Contract", "Open Vehicle Sale Contract", "إجراء عقد بيع سيارة");
            Add("Process Vehicle Sale", "Process Vehicle Sale", "بدء عقد بيع السيارة");
            Add("Smart Deal Negotiator Assistant", "Smart Deal Negotiator Assistant", "المساعد الذكي للتفاوض");
            Add("Floor Price Breach Alert!", "Floor Price Breach Alert!", "تحذير: تجاوز أدنى سعر للتفاوض!");
            Add("Dashboard Overview", "Dashboard Overview", "نظرة عامة على لوحة التحكم");
            Add("Quick Actions:", "Quick Actions:", "إجراءات سريعة:");
            Add("New Contract", "New Contract", "عقد إيجار جديد");
            Add("Add Vehicle", "Add Vehicle", "إضافة سيارة جديدة");
            Add("Record Payment", "Record Payment", "تسجيل دفعة إيجار");
            Add("Add Customer", "Add Customer", "إضافة عميل جديد");
            Add("Schedule Service", "Schedule Service", "جدولة صيانة");
            Add("GPS Live Map", "GPS Live Map", "خريطة التتبع المباشر");
            Add("Overdue Returns", "Overdue Returns", "سيارات متأخرة التسليم");
            Add("Under Maintenance", "Under Maintenance", "سيارات قيد الصيانة");
            Add("Net Operating Revenue", "Net Operating Revenue", "صافي أرباح التشغيل");
            Add("Maintenance Expenses", "Maintenance Expenses", "مصاريف صيانة الأسطول");
            Add("Outstanding Receivables", "Outstanding Receivables", "المبالغ المتبقية للتحصيل");
            Add("Active Fleet Rentals", "Active Fleet Rentals", "عقود الإيجار النشطة");
            Add("Available Fleet", "Available Fleet", "السيارات المتاحة للإيجار");
            Add("Contract Velocity", "Contract Velocity", "معدل عقود الإيجار الشهرية");
            Add("Cash Flow Revenue", "Cash Flow Revenue", "التدفقات النقدية للإيرادات");
            Add("Fleet Distribution", "Fleet Distribution", "توزيع حالة الأسطول");
            Add("Upcoming & Overdue Returns", "Upcoming & Overdue Returns", "مواعيد التسليم القادمة والمتأخرة");
            Add("Recent Payment Transactions", "Recent Payment Transactions", "أحدث المعاملات المالية المباشرة");
            Add("Rank", "Rank", "الترتيب");
            Add("Vehicle", "Vehicle", "السيارة");
            Add("Category", "Category", "الفئة");
            Add("Trips", "Trips", "عدد الرحلات");
            Add("Revenue", "Revenue", "الإيرادات");
            Add("Contract", "Contract", "رقم العقد");
            Add("Customer", "Customer", "العميل");
            Add("Return Date", "Return Date", "تاريخ التسليم");
            Add("Payment ID", "Payment ID", "رقم العملية");
            Add("Amount", "Amount", "المبلغ");
            Add("Purpose", "Purpose", "السبب / الغرض");
            Add("Method", "Method", "طريقة الدفع");
            Add("Date", "Date", "التاريخ والوقت");
            Add("Receipt", "Receipt", "سند القبض");
            Add("View All", "View All", "عرض الكل");
            Add("View Fleet", "View Fleet", "عرض الأسطول");
            Add("Contract ID", "Contract ID", "رقم العقد");
            Add("Period", "Period", "فترة الإيجار");
            Add("Paid / Balance", "Paid / Balance", "المسدد / المتبقي");
            Add("Fully Paid", "Fully Paid", "خالص السداد بالكامل");
            Add("Active", "Active", "عقد نشط");
            Add("Closed", "Closed", "عقد مغلق");
            Add("Cancelled", "Cancelled", "عقد ملغى");
            Add("Customer Profile", "Customer Profile", "ملف العميل");
            Add("Total Contracts", "Total Contracts", "إجمالي العقود");
            Add("Address", "Address", "العنوان السكني");
            Add("National ID", "National ID", "الرقم القومي");
            Add("Reference", "Reference", "المرجع / السند");
            Add("Module", "Module", "الوحدة / القسم");
            Add("Employee", "Employee", "الموظف المنفذ");
            Add("IP Address", "IP Address", "عنوان IP");
            Add("Service Type", "Service Type", "نوع خدمة الصيانة");
            Add("Cost", "Cost", "التكلفة الإجمالية");
            Add("Odometer", "Odometer", "قراءة العداد");
            Add("Performed By", "Performed By", "المنفذ / المركز");
            Add("Invoice Number", "Invoice Number", "رقم الفاتورة الضريبية");
            Add("Subtotal", "Subtotal", "الإجمالي قبل الضريبة");
            Add("Tax Amount", "Tax Amount", "مبلغ الضريبة 14%");
            Add("Total Amount", "Total Amount", "الإجمالي الصافي الشامل");
            Add("All Contracts", "All Contracts", "جميع العقود");

            return dict;
        }

        public bool IsArabic => CultureInfo.CurrentUICulture.Name.StartsWith("ar", StringComparison.OrdinalIgnoreCase);

        public string GetString(string key)
        {
            return GetString(key, IsArabic);
        }

        public string GetString(string key, bool isArabic)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (_translations.TryGetValue(key, out var val))
            {
                return isArabic ? val.Ar : val.En;
            }
            return key;
        }
    }
}
