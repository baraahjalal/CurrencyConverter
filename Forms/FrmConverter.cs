using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CurrencyConverter
{
    public partial class FrmConverter : Form
    {

        public FrmConverter()
        {
            InitializeComponent();
        }

        private void FrmConverter_Load(object sender, EventArgs e)
        {
            // جلب أسماء العملات من الكلاس ووضعها في القوائم
            //  تم تحديث المصدر للحصول على رموز العملات من مفاتيح قاموس Currencies
            var currencies = CurrencyManager.Currencies.Keys.ToArray();

            cmbFrom.Items.AddRange(currencies);
            cmbTo.Items.AddRange(currencies);

            // اختيارات افتراضية: يفضل اختيار عملة موجودة و LYD إذا كانت لا تزال موجودة
            if (currencies.Length > 0)
            {
                cmbFrom.SelectedItem = "USD";
                cmbTo.SelectedItem = "LYD";
            }
            txtAmount.Text = "1";

            // حساب النتيجة المبدئية
            CalculateConversion();
        }

        
        private void CalculateConversion()
        {
            // التأكد من أن المستخدم اختار العملات وكتب رقماً
            if (cmbFrom.SelectedItem == null || cmbTo.SelectedItem == null || string.IsNullOrWhiteSpace(txtAmount.Text))
                return;

            // محاولة تحويل النص إلى رقم
            if (double.TryParse(txtAmount.Text, out double amount))
            {
                string fromCode = cmbFrom.SelectedItem.ToString();
                string toCode = cmbTo.SelectedItem.ToString();

              
                if (!CurrencyManager.Currencies.ContainsKey(fromCode) || !CurrencyManager.Currencies.ContainsKey(toCode))
                {
                    lblResult.Text = "خطأ: العملة غير موجودة.";
                    lblResultSub.Text = "";
                    return;
                }

                double fromRate = CurrencyManager.Currencies[fromCode].Rate; // استخراج سعر الصرف
                double toRate = CurrencyManager.Currencies[toCode].Rate;     // استخراج سعر الصرف

                // معادلة التحويل
                // إذا كانت العملات المحفوظة بالنسبة لـ USD (مثلاً)، فإن التحويل يكون:
                // amount * (Rate of Base / Rate of Target)
                double result = amount * (fromRate / toRate);

                // عرض النتيجة النهائية
                lblResult.Text = result.ToString("N2") + " " + toCode;

                // (حركة إضافية) عرض سعر الصرف للوحدة الواحدة في الأسفل
                // مثلاً: 1 USD = 4.85 LYD
                double unitRate = (1 * fromRate) / toRate;
                lblResultSub.Text = $"1 {fromCode} = {unitRate.ToString("N4")} {toCode}"; // استخدام N4 لدقة أكبر في الوحدة الواحدة
            }
            else
            {
                lblResult.Text = "---";
                lblResultSub.Text = "";
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            CalculateConversion();

            // 2. الحفظ في السجل
            // نتأكد أن هناك نتيجة ظاهرة وليست "---"
            if (lblResult.Text != "---" && !string.IsNullOrEmpty(txtAmount.Text) && cmbFrom.SelectedItem != null && cmbTo.SelectedItem != null)
            {
                // نأخذ القيم الحالية
                string amount = txtAmount.Text;
                string from = cmbFrom.SelectedItem.ToString();
                string to = cmbTo.SelectedItem.ToString();
                string result = lblResult.Text;

                // نستدعي دالة الحفظ
                // 💡 نفترض أن دالة SaveTransaction لا تزال موجودة بنفس التوقيع
                CurrencyManager.SaveTransaction(amount, from, to, result);
            }
        }

        private void btnSwitch_Click(object sender, EventArgs e)
        {
            // تبديل الأماكن
            int tempIndex = cmbFrom.SelectedIndex;
            cmbFrom.SelectedIndex = cmbTo.SelectedIndex;
            cmbTo.SelectedIndex = tempIndex;

            CalculateConversion(); // إعادة الحساب فوراً
        }

        private void btnFav_Click(object sender, EventArgs e)
        {

            if (cmbFrom.SelectedItem == null || cmbTo.SelectedItem == null) return;

            string fromCurr = cmbFrom.SelectedItem.ToString();
            string toCurr = cmbTo.SelectedItem.ToString();

            // التنسيق الذي سنحفظه: "USD >> LYD"
            string favItem = $"{fromCurr} >> {toCurr}";

            // التحقق مما إذا كانت موجودة مسبقاً لمنع التكرار
            // 💡 نفترض أن FavoriteConversions لا تزال قائمة سلاسل نصية (List<string>)
            if (!CurrencyManager.FavoriteConversions.Contains(favItem))
            {
                CurrencyManager.FavoriteConversions.Add(favItem);
                MessageBox.Show("Conversion added to Favorites!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // 💡 يجب استدعاء دالة حفظ المفضلة هنا إذا كانت موجودة في CurrencyManager
               // CurrencyManager.SaveFavorites();
            }
            else
            {
                MessageBox.Show("This conversion is already in your favorites.", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtAmount_TextChanged(object sender, EventArgs e)
        {
            CalculateConversion();
        }

        private void txtAmount_KeyPress(object sender, KeyPressEventArgs e)
        {
            // يسمح فقط بالأرقام وزر الحذف والنقطة
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            // يسمح بإدخال نقطة واحدة فقط
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
    }
}