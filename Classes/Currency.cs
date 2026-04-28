using System;
using System.Text.Json.Serialization;

namespace CurrencyConverter
{
    /// <summary>
    /// يمثل نموذج بيانات كامل لعملة واحدة.
    /// هذا الكلاس يسمح بتخزين أكثر من مجرد رمز العملة وقيمتها.
    /// </summary>
    public class Currency
    {
        // نستخدم [JsonPropertyName] للتأكد من أن أسماء الخصائص في ملف JSON تكون صغيرة لسهولة القراءة
        // وفي نفس الوقت نحافظ على نمط C# القياسي (PascalCase) في الكود.

        [JsonPropertyName("code")]
        public string Code { get; set; } // رمز العملة (مثل USD)

        [JsonPropertyName("name")]
        public string Name { get; set; } // الاسم الكامل للعملة (مثل الدولار الأمريكي)

        [JsonPropertyName("rate")]
        public double Rate { get; set; } // سعر الصرف مقابل العملة الأساسية (LYD)

        [JsonPropertyName("trend")]
        public string Trend { get; set; } = ""; // اتجاه السعر (مثل ▲ +1.2%)

        [JsonPropertyName("lastUpdate")]
        public DateTime LastUpdated { get; set; } = DateTime.Now; // آخر تاريخ ووقت للتحديث

        /// <summary>
        /// مُنشئ لإنشاء كائن عملة جديد.
        /// </summary>
        public Currency(string code, string name, double rate, string trend = "", DateTime? lastUpdated = null)
        {
            Code = code.ToUpper();
            Name = name;
            Rate = rate;
            Trend = trend;
            LastUpdated = lastUpdated ?? DateTime.Now;
        }

        // منشئ فارغ ضروري لعملية إلغاء تسلسل (Deserialization) JSON
        public Currency() { }
    }
}
