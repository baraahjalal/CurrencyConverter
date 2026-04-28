using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CurrencyConverter.Forms
{
    public partial class FrmHistory : Form
    {
        public FrmHistory()
        {
            InitializeComponent();
        }

        private void FrmHistory_Load(object sender, EventArgs e)
        {
            LoadHistoryData();

            // Ensure no default selection when loading to keep it clean
            dgvHistory.ClearSelection();
        }

        private void LoadHistoryData()
        {
            // 1. Clean existing rows
            dgvHistory.Rows.Clear();

            // 2. Fetch data (Assuming CurrencyManager is available in your project)
            List<string[]> historyList = CurrencyManager.LoadHistory();

            // 3. Reverse to show newest first
            historyList.Reverse();

            foreach (string[] row in historyList)
            {
                // Validate row length
                if (row.Length == 5)
                {
                    dgvHistory.Rows.Add(row[0], row[1], row[2], row[3], row[4]);
                }
            }
        }

        private void btnClearHistory_Click(object sender, EventArgs e)
        {
            // Modernized the message box caption to match the new look
            if (MessageBox.Show("Are you sure you want to delete all history?", "Delete Records", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    // Clear the file
                    System.IO.File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "history.txt"), string.Empty);

                    // Refresh Grid
                    LoadHistoryData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error clearing history: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

 
            private void btnFileHistory_Click(object sender, EventArgs e)
        {
            // نستخدم saveFileDialog1 الذي ذكر المستخدم أنه مهيأ مسبقاً.
            // 1. تهيئة مربع حوار الحفظ (إذا لم يكن مهيئاً في المصمم)
            // سنفترض وجود saveFileDialog1 كعضو في الكلاس (Private field)
            SaveFileDialog sfd = saveFileDialog1 ?? new SaveFileDialog();

            sfd.Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
            sfd.FileName = "Conversion_History.txt";
            sfd.Title = "Export Conversion History";

            // 2. عرض مربع الحوار والتحقق من أن المستخدم ضغط على 'Save'
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string selectedPath = sfd.FileName;

                    // 3. الحصول على البيانات من CurrencyManager مجدداً للتأكد من أنها الأحدث،
                    // الأفضل هو الحصول عليها من دالة LoadHistory لضمان توافق التنسيق
                    List<string[]> historyListToExport = CurrencyManager.LoadHistory();

                    // 4. استدعاء دالة التصدير في CurrencyManager
                    CurrencyManager.ExportHistory(selectedPath, historyListToExport);

                    MessageBox.Show($"History exported successfully to:\n{selectedPath}", "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export history: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnImportHistory_Click(object sender, EventArgs e)
        {
            // نستخدم openFileDialog1 (الذي يجب أن يكون مهيئاً في المصمم)
            OpenFileDialog ofd = openFileDialog1 ?? new OpenFileDialog();

            ofd.Filter = "History Files (*.txt)|*.txt|All Files (*.*)|*.*";
            ofd.Title = "Select History File to Import";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string selectedPath = ofd.FileName;

                    // 1. استدعاء دالة التحميل الجديدة التي تقبل المسار
                    List<string[]> importedHistory = CurrencyManager.LoadHistory(selectedPath);

                    // 2. تحديث شبكة البيانات بالعناصر الجديدة (بدون مسح البيانات الحالية في الملف الافتراضي)
                    // ملاحظة: هذا الإجراء يقوم فقط بعرض الملف المستورد ولكنه لا يغير الملف الافتراضي للتطبيق.

                    dgvHistory.Rows.Clear();
                    importedHistory.Reverse();

                    foreach (string[] row in importedHistory)
                    {
                        if (row.Length == 5)
                        {
                            dgvHistory.Rows.Add(row[0], row[1], row[2], row[3], row[4]);
                        }
                    }

                    MessageBox.Show($"History loaded successfully from:\n{selectedPath}", "Import Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load history: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}