// يتطلب هذا الملف وجود كلاسات Currency و CurrencyManager
// وكذا الملف التصميمي (designer) الخاص به
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CurrencyConverter.Forms
{
    public partial class CurrencyManagement : Form
    {
        public Currency CurrentCurrency { get; set; }

        // 💡 متغير داخلي لتتبع ما إذا كنا نضيف عملة جديدة أم نعدل عملة موجودة
        private bool IsAddingNew = false;

        // 💡 متغير لتخزين السعر الأصلي للعملة للمقارنة وحساب الاتجاه
        private double _originalRate = 0.00;

        public CurrencyManagement()
        {
            InitializeComponent();
        }

        private void CurrencyManagement_Load(object sender, EventArgs e)
        {
            if (CurrentCurrency != null)
            {
                // وضع التعديل
                IsAddingNew = false;

                // 💡 تخزين السعر الأصلي للمقارنة عند التعديل
                _originalRate = CurrentCurrency.Rate;

                txtCode.Text = CurrentCurrency.Code;
                txtName.Text = CurrentCurrency.Name;
                txtRate.Text = CurrentCurrency.Rate.ToString("0.00");

                // عرض الاتجاه التاريخي المخزن وتطبيق لونه
                lblTrend.Text = CurrentCurrency.Trend ?? "— 0.00%";
                SetTrendColor(lblTrend.Text);

                // منع التعديل على النص مباشرة حتى يضغط المستخدم على "تعديل"
                txtCode.ReadOnly = true;
                txtName.ReadOnly = true;
                txtRate.ReadOnly = true;

                // منع تعديل العملة الأساسية (LYD)
                if (CurrentCurrency.Code.ToUpper() == "LYD")
                {
                    btnEdit.Enabled = false;
                    btnDelete.Enabled = false;
                    btnSave.Enabled = false;
                }
                else
                {
                    btnEdit.Enabled = true;
                    btnDelete.Enabled = true;
                    btnSave.Enabled = false; // يتم تفعيله فقط عند الضغط على "تعديل"
                }
            }
            else
            {
                // إذا لم يتم تمرير عملة، ننتقل لوضع الإضافة فوراً
                btnNew_Click(null, EventArgs.Empty);
            }
        }

        private void txtCode_TextChanged(object sender, EventArgs e)
        {
            lblCode.Text = txtCode.Text;
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            lblName.Text = txtName.Text;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            IsAddingNew = true;
            CurrentCurrency = null; // إشارة إلى أننا نضيف عملة جديدة

            // 💡 تصحيح المنطق: عند إضافة عملة جديدة، لا يوجد سعر مرجعي سابق.
            // نضع القيمة 0.0 للإشارة إلى عدم وجود تاريخ للمقارنة.
            _originalRate = 0.0;

            // تفعيل الإدخال
            txtCode.ReadOnly = false;
            txtName.ReadOnly = false;
            txtRate.ReadOnly = false;

            // مسح الحقول
            txtCode.Text = "";
            txtName.Text = "";
            txtRate.Text = "1.00"; // قيمة افتراضية مناسبة كبداية

            // إعادة تعيين مؤشر الاتجاه للافتراضي
            lblTrend.Text = "— 0.00%";
            SetTrendColor(lblTrend.Text);

            // تفعيل زر الحفظ وإلغاء أزرار التعديل والحذف
            btnSave.Enabled = true;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
        }

        private void txtRate_TextChanged(object sender, EventArgs e)
        {
            lblRate.Text = txtRate.Text;

            // 💡 حساب الاتجاه عند تغيير السعر (فقط إذا كان التعديل متاحاً)
            if (btnSave.Enabled)
            {
                CalculateTrend();
            }
        }

     
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            // قراءة البيانات من الحقول
            string code = txtCode.Text.ToUpper();
            string name = txtName.Text;
            double newRate = double.Parse(txtRate.Text);

            // الحصول على نص الاتجاه المحسوب
            string trend = lblTrend.Text;

            if (IsAddingNew)
            {
                // منطق إضافة عملة جديدة: يجب التحقق من عدم تكرار الرمز
                if (CurrencyManager.GetCurrency(code) != null)
                {
                    MessageBox.Show("رمز العملة هذا موجود بالفعل. استخدم زر التعديل إذا كنت تريد تعديله.", "خطأ في الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 💡 عند الحفظ لأول مرة، الاتجاه سيكون "— 0.00%" لأنها عملة جديدة
                CurrentCurrency = new Currency(code, name, newRate, trend);

                // يتم حفظ الكائن الجديد
                CurrencyManager.AddOrUpdateCurrency(CurrentCurrency);
                MessageBox.Show($"تم إضافة العملة {code} بنجاح.", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else // وضع التعديل
            {
                // منطق تحديث العملة الموجودة
                if (CurrentCurrency != null)
                {
                    // تحديث الخصائص
                    CurrentCurrency.Name = name;
                    CurrentCurrency.Rate = newRate;
                    CurrentCurrency.Trend = trend; // حفظ الاتجاه الجديد المحسوب

                    CurrencyManager.AddOrUpdateCurrency(CurrentCurrency);
                    MessageBox.Show($"تم تحديث العملة {code} بنجاح.", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            // إغلاق النموذج للسماح للنموذج الرئيسي (FrmMain) بتحديث البطاقات
            this.Close();
        }

      
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (CurrentCurrency != null && CurrentCurrency.Code.ToUpper() != "LYD")
            {
                // تفعيل وضع التعديل
                IsAddingNew = false;

                // 💡 حفظ السعر الحالي كسعر مرجعي قبل بدء التعديل
                _originalRate = CurrentCurrency.Rate;

                // تفعيل حقول الإدخال (لا يمكن تعديل الرمز بعد الإضافة)
                txtCode.ReadOnly = true;
                txtName.ReadOnly = false;
                txtRate.ReadOnly = false;

                // تفعيل زر الحفظ
                btnSave.Enabled = true;
                btnEdit.Enabled = false; // تعطيل زر التعديل
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (CurrentCurrency == null) return;

            string code = CurrentCurrency.Code;

            var result = MessageBox.Show($"هل أنت متأكد من حذف العملة {code}؟", "تأكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                if (CurrencyManager.DeleteCurrency(code))
                {
                    MessageBox.Show($"تم حذف العملة {code} بنجاح.", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("الرجاء إدخال رمز واسم العملة.", "خطأ في الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!double.TryParse(txtRate.Text, out double newRate) || newRate <= 0)
            {
                MessageBox.Show("الرجاء إدخال سعر صرف صحيح وموجب.", "خطأ في الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void CurrencyManagement_FormClosing(object sender, FormClosingEventArgs e)
        {
            CurrencyManager.LoadCurrencies();
        }

        private void btnPlus_Click(object sender, EventArgs e)
        {
            UpdateRate(0.01);
        }

        private void btnMinus_Click(object sender, EventArgs e)
        {
            UpdateRate(-0.01);
        }

        private void UpdateRate(double amount)
        {
            if (double.TryParse(txtRate.Text, out double currentRate))
            {
                currentRate += amount;
                if (currentRate < 0) currentRate = 0;
                txtRate.Text = currentRate.ToString("0.00");
                // txtRate_TextChanged سيستدعي CalculateTrend
            }
            else
            {
                txtRate.Text = (amount > 0 ? amount : 0).ToString("0.00");
            }
        }

        private void CalculateTrend()
        {
            if (double.TryParse(txtRate.Text, out double newRate))
            {
                double baselineRate = _originalRate;

                // 💡 FIX: المنطق المصحح
                // إذا كنا نضيف عملة جديدة (IsAddingNew) أو إذا كان السعر المرجعي غير صالح (<= 0)
                // فلا يجب حساب أي نسبة مئوية لأنها ستكون غير منطقية.
                if (IsAddingNew || baselineRate <= 0)
                {
                    lblTrend.Text = "— 0.00%";
                    SetTrendColor(lblTrend.Text);
                    return; // الخروج من الدالة
                }

                // الكود أدناه يتم تنفيذه فقط عند "تعديل" عملة موجودة سابقاً
                double change = newRate - baselineRate;
                double percentageChange = (change / baselineRate) * 100;

                string trendSymbol = "—";
                Color trendColor = SystemColors.ControlText;

                if (Math.Abs(change) < 0.0001)
                {
                    trendSymbol = "—";
                    trendColor = SystemColors.ControlText;
                    percentageChange = 0;
                }
                else if (change > 0)
                {
                    trendSymbol = "↑";
                    trendColor = Color.Green;
                }
                else // change < 0
                {
                    trendSymbol = "↓";
                    trendColor = Color.Red;
                }

                // تنسيق النسبة المئوية
                string format = (Math.Abs(percentageChange) < 1.0 && Math.Abs(percentageChange) > 0) ? "0.0000" : "0.00";
                string trendText = $"{trendSymbol} {Math.Abs(percentageChange).ToString(format)}%";

                lblTrend.Text = trendText;
                lblTrend.ForeColor = trendColor;
            }
            else
            {
                // إدخال غير صالح
                lblTrend.Text = "— 0.00%";
                SetTrendColor(lblTrend.Text);
            }
        }

        private void SetTrendColor(string trendText)
        {
            if (trendText.Contains("↑") || trendText.Contains("+"))
            {
                lblTrend.ForeColor = Color.Green;
            }
            else if (trendText.Contains("↓") || trendText.Contains("-"))
            {
                lblTrend.ForeColor = Color.Red;
            }
            else
            {
                lblTrend.ForeColor = SystemColors.ControlText;
            }
        }

        private void txtCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtName.Select();
            }
        }

        private void txtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtRate.Select();
            }
        }
    }
}