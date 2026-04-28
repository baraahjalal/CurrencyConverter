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
    public partial class FrmFavourites : Form
    {
        public FrmFavourites()
        {
            InitializeComponent();
        }

        private void FrmFavourites_Load(object sender, EventArgs e)
        {
            RefreshList();
        }


        // دالة مساعدة لتحديث القائمة
        private void RefreshList()
        {
            lstFavs.Items.Clear(); // تنظيف القائمة القديمة

            // التحقق هل هناك مفضلات أصلاً؟
            if (CurrencyManager.FavoriteConversions.Count > 0)
            {
                // إضافة كل العناصر من الكلاس إلى الليست بوكس
                foreach (string item in CurrencyManager.FavoriteConversions)
                {
                    lstFavs.Items.Add(item);
                }
            }
            else
            {
                lstFavs.Items.Add("No favorites added yet.");
            }

        }

        private void btnRemove_Click(object sender, EventArgs e)
        {

            // التأكد من أن المستخدم اختار عنصراً للحذف
            if (lstFavs.SelectedIndex != -1)
            {
                // الحصول على النص المختار
                string selectedItem = lstFavs.SelectedItem.ToString();

                // التحقق من أنها ليست رسالة "No favorites"
                if (selectedItem == "No favorites added yet.") return;

                // الحذف من الذاكرة (الداتا)
                CurrencyManager.FavoriteConversions.Remove(selectedItem);

                // الحذف من الشاشة (الليست بوكس)
                lstFavs.Items.RemoveAt(lstFavs.SelectedIndex);

                MessageBox.Show("Item removed successfully.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // إعادة التحديث للتأكد من القائمة فارغة أم لا
                if (CurrencyManager.FavoriteConversions.Count == 0)
                {
                    RefreshList();
                }
            }
            else
            {
                MessageBox.Show("Please select an item to remove first.", "Select Item", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
