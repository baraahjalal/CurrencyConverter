using CurrencyConverter.Forms;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CurrencyConverter
{
    public partial class FrmMain : Form
    {
        
        // Sidebar Logic Variables
        private bool isExpanded = false; // متغير لتتبع حالة الشريط الجانبي (موسع أم لا)
        private Timer sidebarTimer; // مؤقت للتحكم في سلاسة حركة التوسيع والانكماش

        // Original Size Variables for Icon Animation
        private Size originalNavSize; // حجم أيقونة التنقل الأصلي
        private Point originalNavLoc; // موقع أيقونة التنقل الأصلي

        // Sidebar Animation Settings
        private const int MinWidth = 70; // الحد الأدنى لعرض الشريط الجانبي (عندما يكون منكمشاً)
        private const int MaxWidth = 235; // الحد الأقصى لعرض الشريط الجانبي (عندما يكون موسعاً)
        private const int AnimSpeed = 20; // سرعة حركة التوسع والانكماش
        
        
        public FrmMain()
        {
            InitializeComponent(); // استدعاء مكونات التصميم (Designer)

            // 1. Setup Navigation Icon Animation
            originalNavSize = picNav.Size; // حفظ حجم الأيقونة
            originalNavLoc = picNav.Location; // حفظ موقع الأيقونة

            // 2. Setup Sidebar Timer
            sidebarTimer = new Timer(); // إنشاء المؤقت
            sidebarTimer.Interval = 10; // ضبط سرعة المؤقت
            sidebarTimer.Tick += SidebarTimer_Tick; // ربط المؤقت بدالة التحكم بالحركة

            // 3. Initial State
            pnlSideBar.Width = MinWidth; // تعيين العرض الأولي (منكمش)
            isExpanded = false; // تعيين الحالة الأولية

            // 4. Hover Effects for Nav Icon (تأثيرات عند مرور الماوس على أيقونة التنقل)
            picNav.MouseEnter += (s, e) =>
            {
                picNav.Size = new Size(originalNavSize.Width + 10, originalNavSize.Height + 10); // تكبير الأيقونة قليلاً
                picNav.Location = new Point(originalNavLoc.X - 5, originalNavLoc.Y - 5); // تغيير موقع الأيقونة للمحافظة على التمركز
            };

            picNav.MouseLeave += (s, e) =>
            {
                picNav.Size = originalNavSize; // استعادة الحجم الأصلي
                picNav.Location = originalNavLoc; // استعادة الموقع الأصلي
            };
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            // إعدادات لوحة عرض أسعار الصرف (FlowLayoutPanel) لضمان التمرير والترتيب
            flowLayoutPanelRates.AutoScroll = true;        // تفعيل شريط التمرير التلقائي
            flowLayoutPanelRates.WrapContents = true;      // السماح للبطاقات بالنزول لسطر جديد عند الامتلاء
            flowLayoutPanelRates.FlowDirection = FlowDirection.LeftToRight; // الترتيب من اليسار لليمين

            // تحميل البيانات وعرضها
            CurrencyManager.LoadCurrencies(); // تحميل البيانات
            RefreshRatesAndTrends(); // تحديث واجهة المستخدم بالأسعار والاتجاهات
        }

        private void UpdateDateTime()
        {
            lblTime.Text = DateTime.Now.ToString("HH:mm:ss"); // تحديث عرض الوقت
            lblDate.Text = DateTime.Now.ToString("dddd, MMMM dd yyyy"); // تحديث عرض التاريخ
        }
        public void RefreshRatesAndTrends()
        {
            UpdateDateTime(); 

            // تحقق سريع لمنع الأخطاء إذا كانت البيانات غير محملة
            if (CurrencyManager.Currencies == null || !CurrencyManager.Currencies.Any())
            {
                flowLayoutPanelRates.Controls.Clear();
                return;
            }

            // 1. مسح البطاقات القديمة قبل إضافة الجديدة
            flowLayoutPanelRates.Controls.Clear();
            flowLayoutPanelRates.SuspendLayout(); // تعليق مؤقت للرسم لتسريع الأداء

            try
            {
                // 2. الحصول على قائمة العملات وفرزها أبجدياً حسب الرمز، باستثناء العملة الأساسية LYD
                var ratesToDisplay = CurrencyManager.Currencies
                    .Where(kvp => kvp.Key.ToUpper() != "LYD")
                    .OrderBy(kvp => kvp.Key) // الفرز لضمان ترتيب ثابت
                    .ToList();

                foreach (var rateKvp in ratesToDisplay)
                {
                    Currency currency = rateKvp.Value;

                    // 3. إنشاء بطاقة جديدة  CurrencyCard)
                    CurrencyCard newCard = new CurrencyCard();

                    // 4. تعبئة البيانات في البطاقة
                    newCard.CurrencyCode = currency.Code;
                    newCard.ExchangeRate = currency.Rate;
                    newCard.Trend = currency.Trend;
                    newCard.CurrencyName = currency.Name;

                    // 5. إضافة البطاقة إلى لوحة العرض
                    flowLayoutPanelRates.Controls.Add(newCard);
                }
            }
            catch (Exception ex)
            {
                // عرض رسالة خطأ في حال فشل عرض البيانات
                MessageBox.Show($"خطأ في عرض بيانات العملات: {ex.Message}", "خطأ في الواجهة", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                flowLayoutPanelRates.ResumeLayout(); // استئناف الرسم
            }
        }



        // --- NAVIGATION EVENTS --- (أحداث الشريط الجانبي)

        private void picNav_Click(object sender, EventArgs e)
        {
            sidebarTimer.Start(); // بدء المؤقت لتحريك الشريط الجانبي
        }

        private void SidebarTimer_Tick(object sender, EventArgs e)
        {
            if (isExpanded)
            {
                pnlSideBar.Width -= AnimSpeed; // تقليص العرض
                if (pnlSideBar.Width <= MinWidth) // التحقق من الوصول للحد الأدنى
                {
                    pnlSideBar.Width = MinWidth; // تثبيت العرض
                    isExpanded = false; // تحديث الحالة
                    sidebarTimer.Stop(); // إيقاف المؤقت
                }
            }
            else
            {
                pnlSideBar.Width += AnimSpeed; // توسيع العرض
                if (pnlSideBar.Width >= MaxWidth) // التحقق من الوصول للحد الأقصى
                {
                    pnlSideBar.Width = MaxWidth; // تثبيت العرض
                    isExpanded = true; // تحديث الحالة
                    sidebarTimer.Stop(); // إيقاف المؤقت
                }
            }
        }

        private void btnConverter_Click(object sender, EventArgs e)
        {
            FrmConverter converterForm = new FrmConverter(); // إنشاء نموذج المحول
            converterForm.Show(); // إظهار النموذج
            this.Hide(); // إخفاء النموذج الحالي
            converterForm.FormClosed += (s, args) => this.Show(); // عند إغلاق نموذج المحول، أعد إظهار هذا النموذج
        }

        private void btnFavs_Click(object sender, EventArgs e)
        {
            FrmFavourites frmFavourites = new FrmFavourites(); // إنشاء نموذج المفضلة
            frmFavourites.Show(); // إظهار النموذج
            this.Hide(); // إخفاء النموذج الحالي
            frmFavourites.FormClosed += (s, args) => this.Show(); // عند إغلاق نموذج المفضلة، أعد إظهار هذا النموذج
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            FrmHistory frmHistory = new FrmHistory(); // إنشاء نموذج السجل
            frmHistory.Show(); // إظهار النموذج
            this.Hide(); // إخفاء النموذج الحالي
            frmHistory.FormClosed += (s, args) => this.Show(); // عند إغلاق نموذج السجل، أعد إظهار هذا النموذج
        }

        // --- CLOCK LOGIC --- (منطق الساعة)

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateDateTime(); // دالة تتولى تحديث الوقت والتاريخ كل ثانية
        }

      

        private void btnRates_Click(object sender, EventArgs e)
        {
            // 💡 تحديث: استخدام نموذج إدارة العملة الموحد (CurrencyManagement) بدلاً من FrmRatesAdmin
            CurrencyManagement frmManagement = new CurrencyManagement(); // إنشاء نموذج إدارة العملات
            frmManagement.Show(); // إظهار النموذج
            this.Hide(); // إخفاء النموذج الحالي
            frmManagement.Owner = this; // تعيين النموذج الحالي كمالك (اختياري)
            // 💡 تحديث: إعادة تحميل البيانات من الملف أولاً ثم تحديث الواجهة عند الإغلاق
            frmManagement.FormClosed += (s, args) => { CurrencyManager.LoadCurrencies(); this.Show(); RefreshRatesAndTrends(); };
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // عرض رسالة تأكيد احترافية للمستخدم
            DialogResult result = MessageBox.Show(
                "هل أنت متأكد من رغبتك في إغلاق النظام؟", // نص الرسالة
                "تأكيد الإغلاق", // عنوان النافذة
                MessageBoxButtons.YesNo, // أزرار نعم/لا
                MessageBoxIcon.Question // أيقونة السؤال
            );

            // التحقق من استجابة المستخدم
            if (result == DialogResult.Yes)
            {
                // إذا اختار المستخدم "نعم"، نغلق التطبيق بالكامل.
                Environment.Exit(0);
            }
            else
            {
                // إذا اختار المستخدم "لا"، نلغي عملية الإغلاق.
                e.Cancel = true;
            }
        }



        // --- FILE OPERATIONS --- (عمليات الملفات)

        private void btnFileRates_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            // تعيين مرشح JSON
            sfd.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
            sfd.FileName = "Exchange_Rates.json";
            sfd.Title = "تصدير أسعار الصرف الحالية";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                   
                    CurrencyManager.ExportCurrenciesToJson(sfd.FileName);
                    MessageBox.Show($"تم حفظ العملات بنجاح في: {sfd.FileName}", "تم التصدير", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل في حفظ الملف: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void btnImportRates_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog(); // إنشاء مربع حوار فتح الملفات
                                                       // تحديد الفلتر لاستيراد ملفات JSON فقط
            ofd.Filter = "JSON Files (*.json)|*.json";
            ofd.Title = "استيراد أسعار الصرف";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // استدعاء دالة الاستيراد الجديدة
                    CurrencyManager.ImportCurrenciesFromJson(ofd.FileName);

                    // 💡 تحديث الواجهة بعد الاستيراد بنجاح
                    RefreshRatesAndTrends();

                    MessageBox.Show($"تم استيراد العملات بنجاح. تم تحديث {CurrencyManager.Currencies.Count} عملة.", "تم الاستيراد", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل في استيراد الملف. تأكد من أن الملف بصيغة JSON صحيحة. الخطأ: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnAddCurrency_Click(object sender, EventArgs e)
        {
            // 💡 تحديث: استخدام نموذج إدارة العملة الموحد (CurrencyManagement) بدلاً من FrmAddCurrency
            CurrencyManagement frmManagement = new CurrencyManagement(); // إنشاء نموذج إدارة العملات
            frmManagement.Show(); // إظهار النموذج
            this.Hide(); // إخفاء النموذج الحالي
            // 💡 تحديث: إعادة تحميل البيانات من الملف أولاً ثم تحديث البطاقات عند الإغلاق
            frmManagement.FormClosed += (s, args) => { CurrencyManager.LoadCurrencies(); this.Show(); RefreshRatesAndTrends(); };
        }
    }
}