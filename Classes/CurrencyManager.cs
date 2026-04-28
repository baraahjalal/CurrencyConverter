using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;
using System.Text.Json; // 👈 تم إضافة هذا لاستخدام معالجة JSON
using System.Text.Encodings.Web; // 💡 تم إضافة هذا لدعم الأحرف العربية في JSON

namespace CurrencyConverter
{
    // 💡 تم إعادة هيكلة الكلاس لاستخدام نموذج بيانات شامل (Currency) وإدارة البيانات بملف JSON.
    public static class CurrencyManager
    {

       
        //private static readonly string CurrenciesFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Currencies.json");


        //------------------------------------------------------التخزين في الAPP DATA-------------------------

        private static readonly string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CurrencyConverter_Data");
        private static readonly string CurrenciesFilePath = Path.Combine(AppDataFolder, "Currencies.json");


        private static readonly string HistoryFilePath = Path.Combine(AppDataFolder, "history.txt");



        //---------------------------------------------THE  MAIN DDICTIONARY-----------------------------------------------
        public static Dictionary<string, Currency> Currencies { get; private set; } = new Dictionary<string, Currency>(StringComparer.OrdinalIgnoreCase);



        //-------------------------------------------------FAV CONVERSIONS LIST-------------------------------------------- 
        public static List<string> FavoriteConversions = new List<string>();


        // -------------------------------------------------------------------
        // دوال التحميل والحفظ التلقائي
        // -------------------------------------------------------------------

      
        //------------------------------------------------THE LOAD FUNCTION-------------------------------
        public static void LoadCurrencies()
        {
            //اختبار """"""""""""""المجلد""""""""""""" لحفظ الملفااااات فيه
            if (!Directory.Exists(AppDataFolder))
            {
                try
                {
                    Directory.CreateDirectory(AppDataFolder);
                }
                catch (Exception ex)
                {
                    // التعامل مع أي خطأ قد يحدث أثناء إنشاء المجلد (مثل عدم وجود صلاحيات)
                    MessageBox.Show($"فشل في إنشاء مجلد البيانات: {ex.Message}", "خطأ في النظام", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return; // إيقاف عملية الحفظ
                }
            }

            //اختبار وجود الملف نفسه
            if (File.Exists(CurrenciesFilePath))
            {
                try
                {
                    string jsonString = File.ReadAllText(CurrenciesFilePath);

                    // إعدادات إلغاء التسلسل: تسمح بـ camelCase أو PascalCase كأسماء للخصائص
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        // 💡 هذا هو الإعداد السحري لمنع الهروب (Escaping) للأحرف غير ASCII مثل العربية
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };

                    // إلغاء تسلسل ملف JSON إلى قاموس الكائنات
                    var loadedData = JsonSerializer.Deserialize<Dictionary<string, Currency>>(jsonString, options);

                    if (loadedData != null && loadedData.Any())
                    {
                        // 💡 تحديث القاموس الحالي بالبيانات المحملة مع توحيد المفاتيح إلى الأحرف الكبيرة
                        Currencies = loadedData.ToDictionary(
                            kvp => kvp.Key.ToUpper(),
                            kvp => kvp.Value,
                            StringComparer.OrdinalIgnoreCase
                        );

                        // 💡 تحديث كود العملة داخل كل كائن لضمان التناسق (العملة تعرف رمزها)
                        foreach (var kvp in Currencies)
                        {
                            kvp.Value.Code = kvp.Key;
                        }
                    }
                    else
                    {


                        // إذا كان الملف موجوداً لكن فارغاً أو غير صالح، نعود للقيم الافتراضية
                        InitializeDefaultCurrencies();
                        SaveCurrencies();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في تحميل بيانات العملات من ملف JSON: {ex.Message}", "خطأ في التحميل", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // في حالة الخطأ، إنشاء قيم افتراضية احتياطية
                    InitializeDefaultCurrencies();
                    SaveCurrencies();
                }
            }
            else
            {
                // إذا لم يتم العثور على الملف، إنشاء قيم افتراضية وحفظها
                InitializeDefaultCurrencies();
                SaveCurrencies();


                // 🌟 رسالة توضيحية: الملف تالف/فارغ وتم استخدام الافتراضي 🌟
                MessageBox.Show(
                    "ملف إعدادات العملات موجود لكنه فارغ أو غير صالح. تم تحميل الإعدادات الافتراضية.",
                    "تحذير بيانات",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        private static void InitializeDefaultCurrencies()
        {
            // 💡 تهيئة البيانات الافتراضية الآن ككائنات Currency كاملة
            Currencies = new Dictionary<string, Currency>(StringComparer.OrdinalIgnoreCase)
            {
                // الكود والمعدل هما الأهم، والباقي تفاصيل
                {"USD", new Currency("USD", "الدولار الأمريكي", 4.85, "▲ +1.2%")},
                {"EUR", new Currency("EUR", "اليورو", 5.20, "▼ -0.5%")},
                {"GBP", new Currency("GBP", "الجنيه الاسترليني", 6.10, "▲ +0.1%")},
                {"LYD", new Currency("LYD", "الدينار الليبي", 1.00)} // العملة الأساسية
            };
        }


        /// <summary>
        /// يحفظ بيانات العملات الحالية تلقائياً إلى ملف JSON.
        /// </summary>
        public static void SaveCurrencies()
        {
            try
            {
                // إعدادات التسلسل: تفعيل التنسيق الجميل (Indented) ومنع الهروب للأحرف العربية
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                // تسلسل القاموس إلى سلسلة نصية بتنسيق JSON 
                string jsonString = JsonSerializer.Serialize(Currencies, options);

                File.WriteAllText(CurrenciesFilePath, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ بيانات العملات: {ex.Message}");
            }
        }

        // -------------------------------------------------------------------
        // دوال الاستيراد والتصدير للمستخدم (لتبادل الملفات)
        // -------------------------------------------------------------------

        /// <summary>
        /// تصدير بيانات العملات الحالية إلى ملف JSON يختاره المستخدم.
        /// </summary>
        public static void ExportCurrenciesToJson(string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                // 💡 حفظ القاموس مباشرةً (Currencies)
                string jsonString = JsonSerializer.Serialize(Currencies, options);
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تصدير العملات إلى JSON: {ex.Message}");
            }
        }

        /// <summary>
        /// استيراد بيانات العملات من ملف JSON يختاره المستخدم واستبدال البيانات الحالية.
        /// </summary>
        public static void ImportCurrenciesFromJson(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);

            // إعدادات إلغاء التسلسل: تسمح بـ camelCase أو PascalCase كأسماء للخصائص
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var loadedData = JsonSerializer.Deserialize<Dictionary<string, Currency>>(jsonString, options);

            if (loadedData == null)
            {
                throw new InvalidOperationException("فشل في قراءة ملف العملات. قد يكون الملف فارغاً أو بصيغة خاطئة.");
            }

            // مسح العملات القديمة واستبدالها بالعملات المستوردة
            Currencies.Clear();

            // 💡 التأكد من أن جميع المفاتيح في القاموس المستورد هي بأحرف كبيرة
            Currencies = loadedData.ToDictionary(
                kvp => kvp.Key.ToUpper(),
                kvp => kvp.Value,
                StringComparer.OrdinalIgnoreCase
            );

            // تحديث كود العملة داخل كل كائن
            foreach (var kvp in Currencies)
            {
                kvp.Value.Code = kvp.Key;
            }

            // حفظ البيانات الجديدة في ملف Currencies.json الخاص بالنظام لضمان استمرارية التغيير
            SaveCurrencies();
        }

        // -------------------------------------------------------------------
        // دوال الإدارة الأخرى (تم تحديثها)
        // -------------------------------------------------------------------

        /// <summary>
        /// استرجاع كائن العملة الكامل بناءً على الرمز.
        /// </summary>
        public static Currency GetCurrency(string currencyCode)
        {
            // نستخدم ToUpper لضمان البحث الصحيح بغض النظر عن حالة الأحرف
            Currencies.TryGetValue(currencyCode.ToUpper(), out Currency currency);
            return currency; // قد يعيد null إذا لم يتم العثور على العملة
        }


        /// <summary>
        /// إضافة أو تحديث عملة جديدة بكامل تفاصيلها.
        /// </summary>
        public static void AddOrUpdateCurrency(Currency newCurrency)
        {
            // نتأكد أن المفتاح دائماً بالكود الكبير (مثل USD) لتوحيد المفاتيح
            string code = newCurrency.Code.ToUpper();

            // 💡 التعديل هنا: تحديث تاريخ التحديث دائماً عند الإضافة أو التعديل
            newCurrency.Code = code;
            newCurrency.LastUpdated = DateTime.Now; // تحديث تاريخ آخر تعديل

            Currencies[code] = newCurrency; // القاموس سيضيف جديداً إذا لم يجد أو يعدل الموجود إذا وجد
            SaveCurrencies();
        }

        public static bool DeleteCurrency(string currencyCode)
        {
            string code = currencyCode.ToUpper();

            // 1. لا تسمح بحذف العملة الأساسية (LYD)
            if (code == "LYD")
            {
                MessageBox.Show("لا يمكن حذف العملة الأساسية (LYD).", "عملية مرفوضة", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }

            // 2. محاولة الحذف من القاموس
            if (Currencies.ContainsKey(code))
            {
                Currencies.Remove(code);
                SaveCurrencies(); // 3. حفظ التغييرات في ملف JSON
                return true;
            }
            else
            {
                MessageBox.Show($"العملة ذات الرمز {code} غير موجودة للحذف.", "خطأ في الحذف", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        public static void UpdateRate(string currencyCode, double newRate)
        {
            string code = currencyCode.ToUpper();
            if (Currencies.ContainsKey(code) && code != "LYD")
            {
                // 💡 نحدث فقط خاصية Rate وتاريخ التحديث داخل كائن Currency الحالي
                Currencies[code].Rate = newRate;
                Currencies[code].LastUpdated = DateTime.Now; // تحديث تاريخ التحديث
                // ملاحظة: يُفضل استخدام دالة لحساب وتحديث الـ Trend بناءً على السعر السابق
                SaveCurrencies();
            }
        }

        public static List<string> GetEditableCurrencies()
        {
            // 💡 الآن نأخذ رموز العملات من مفاتيح قاموس Currencies، باستثناء العملة الأساسية (LYD)
            return Currencies.Keys.Where(key => key.ToUpper() != "LYD").ToList();
        }

        // -------------------------------------------------------------------
        // دوال التاريخ (تم الحفاظ عليها)
        // -------------------------------------------------------------------

        public static void SaveTransaction(string amount, string from, string to, string result)
        {
            try
            {
                // تنسيق السطر: التاريخ|المبلغ|من|إلى|النتيجة
                string logLine = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}|{amount}|{from}|{to}|{result}";

                // الكتابة في الملف الافتراضي وإضافة سطر جديد بعد كل عملية
                File.AppendAllText(HistoryFilePath, logLine + Environment.NewLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ التاريخ: {ex.Message}");
            }
        }

        public static List<string[]> LoadHistory()
        {
            // استدعاء دالة التحميل المحددة للمسار الافتراضي
            return LoadHistory(HistoryFilePath);
        }

        public static List<string[]> LoadHistory(string filePath)
        {
            List<string[]> historyData = new List<string[]>();

            try
            {
                if (File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);

                    foreach (string line in lines.Reverse()) // 💡 قراءة الأحدث أولاً
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            string[] parts = line.Split('|');
                            historyData.Add(parts);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل التاريخ من {filePath}: {ex.Message}", "خطأ في التحميل", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return historyData;
        }

        public static void ExportHistory(string savePath, List<string[]> historyData)
        {
            try
            {
                List<string> lines = historyData
                    .Select(parts => string.Join("|", parts))
                    .ToList();

                File.WriteAllLines(savePath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تصدير التاريخ: {ex.Message}");
            }
        }





       
    }
}