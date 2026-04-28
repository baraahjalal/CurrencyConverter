using CurrencyConverter.Forms;
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
    // يمثل عنصر تحكم مستخدم
    // (UserControl)
    // يستخدم لعرض بيانات عملة واحدة (الرمز، الاسم، السعر، الاتجاه)
    //
    //
    //ضمن قائمة أسعار الصرف الرئيسية.
    public partial class CurrencyCard : UserControl
    {
        public CurrencyCard()
        {
            InitializeComponent();
        }

        // -------------------------------------------------------------------
        // خصائص لضبط البيانات من النموذج الرئيسي (FrmMain)
        // -------------------------------------------------------------------

        public string CurrencyCode
        {
            get => lblCode.Text;
            set => lblCode.Text = value; // تعيين رمز العملة
        }

        public string CurrencyName
        {
            get => lblName.Text;
            set => lblName.Text = value; // تعيين الاسم الكامل للعملة
        }

        public double ExchangeRate
        {
            // عند تعيين القيمة، يتم تنسيقها مباشرة للعرض
            set => lblRate.Text = value.ToString("0.00") + " LYD"; // تنسيق الرقم بـ 0.00 وإضافة العملة الأساسية
        }

        public string Trend
        {
            set
            {
                lblTrend.Text = value; // تعيين نص الاتجاه مثل ▲ +1.2
                StyleTrendLabel(lblTrend); // تطبيق التنسيق واللون بناءً على الاتجاه
            }
        }

        // -------------------------------------------------------------------
        // منطق التنسيق (Styling Logic)
        // -------------------------------------------------------------------

  
        //- تغير لون خلفية ونص عنصر التحكم بناءً على إشارة الاتجاه (+ أو
        public void StyleTrendLabel(Label lbl)
        {
            // تحديد الاتجاه إما باستخدام علامة الجمع (+) أو رمز المثلث الصاعد (▲)
            if (lbl.Text.Contains("+") || lbl.Text.Contains("↑"))
            {
                lbl.ForeColor = Color.FromArgb(0, 184, 148); // Green (لون النص)
                lbl.BackColor = Color.FromArgb(223, 249, 237); // Light Green (لون الخلفية)
            }
            else // إذا كانت تحتوي على علامة الطرح (-) أو رمز المثلث الهابط (▼)
            {
                lbl.ForeColor = Color.FromArgb(214, 48, 49); // Red (لون النص)
                lbl.BackColor = Color.FromArgb(255, 230, 230); // Light Red (لون الخلفية)
            }
        }



        // -------------------------------------------------------------------
        // الأحداث والتفاعل (Events and Interaction)
        // -------------------------------------------------------------------



        private void lblSettings_Click(object sender, EventArgs e)
        {

            {
                // 1. جلب كائن العملة الكامل باستخدام رمز العملة الخاص بهذه البطاقة
                Currency currencyToEdit = CurrencyManager.GetCurrency(this.CurrencyCode);

                if (currencyToEdit == null)
                {
                    MessageBox.Show($"فشل في العثور على بيانات العملة: {this.CurrencyCode}.", "خطأ في البيانات", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 2. إنشاء نموذج الإدارة وتعيين كائن العملة للخاصية العامة
                CurrencyManagement currencyManager = new CurrencyManagement
                {
                    CurrentCurrency = currencyToEdit // 👈 تمرير البيانات الكاملة!
                };

                // 3. إدارة إظهار وإخفاء النماذج
                var mainForm = Application.OpenForms.OfType<FrmMain>().FirstOrDefault();

                if (mainForm != null)
                {
                    mainForm.Hide();
                    currencyManager.Owner = mainForm;

                    currencyManager.FormClosed += (s, args) =>
                    {
                        // 1. إظهار النموذج الرئيسي أولاً
                        mainForm.Show();

                        // 2. إعادة تحميل البيانات من الملف (لأن نموذج الإدارة قام بتغييرها)
                        CurrencyManager.LoadCurrencies();

                        // 3. طلب تحديث الواجهة عبر BeginInvoke لضمان أن التحديث يحدث على الواجهة الرسومية الرئيسية 
                        // بعد أن يتم عرضها بالكامل.
                        mainForm.BeginInvoke((MethodInvoker)delegate
                        {
                            ((FrmMain)mainForm).RefreshRatesAndTrends();
                        });
                    };
                }

                currencyManager.Show();
            }
        }


        private void lblEmoji_MouseEnter(object sender, EventArgs e)
        {
            Label lbl = sender as Label;
            // تكبير حجم الخط عند مرور الماوس
            lbl.Font = new Font(lbl.Font.FontFamily, lbl.Font.Size + 4, lbl.Font.Style);
        }

        private void lblEmoji_MouseLeave(object sender, EventArgs e)
        {
            Label lbl = sender as Label;
            // استعادة حجم الخط الأصلي عند مغادرة الماوس
            lbl.Font = new Font(lbl.Font.FontFamily, lbl.Font.Size - 4, lbl.Font.Style);
        }

        private void pnlCard_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}