using AutoTester.DockableForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace AutoTester
{
    public partial class MainForm : Form
    {
        private List<TestingBrowserForm> _browsers = new List<TestingBrowserForm>();

        public MainForm()
        {
            InitializeComponent();
        }

        private void RunAllTests()
        {
            if (_browsers.Count == 0)
            {
                MessageBox.Show("Please open or create a test to run", "No test to run", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"Are you sure you want to run {_browsers.Count} test(s)?", "Run all", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (var browser in _browsers)
                {
                    _ = browser.Run();
                }
            }
        }

        private async void RunActiveTest()
        {
            var activeBrowserForm = dockPanel1.ActivePane?.ActiveContent as TestingBrowserForm;

            if (activeBrowserForm == null)
            {
                MessageBox.Show("Please open or create a test to run", "No test to run", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await activeBrowserForm.Run();
        }

        private string SaveAs(string code, string filepath = null)
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.FileName = !string.IsNullOrEmpty(filepath) ? Path.GetFileName(filepath) : null;
                saveFileDialog.Filter = "AutoTester Script | *.ats";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog.FileName, code);
                    return saveFileDialog.FileName;
                }
            }

            return string.Empty;
        }


        #region Event Handlers

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var browserForm = new TestingBrowserForm() { TabText = "New Test" };
            browserForm.Show(dockPanel1, DockState.Document);
            _browsers.Add(browserForm);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var activeBrowserForm = dockPanel1.ActivePane?.ActiveContent as TestingBrowserForm;

            if (activeBrowserForm == null) return;

            var filepath = activeBrowserForm.Tag as string;

            if (string.IsNullOrEmpty(filepath))
            {
                var newFilename = SaveAs(activeBrowserForm.Code);
                if (!string.IsNullOrEmpty(newFilename))
                {
                    activeBrowserForm.Tag = newFilename;
                    activeBrowserForm.TabText = newFilename;
                }
            }
            else
                File.WriteAllText(filepath, activeBrowserForm.Code);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var activeBrowserForm = dockPanel1.ActivePane?.ActiveContent as TestingBrowserForm;

            if (activeBrowserForm == null) return;

            var newFilename = SaveAs(activeBrowserForm.Code, activeBrowserForm.Tag as string);

            if (!string.IsNullOrEmpty(newFilename))
            {
                activeBrowserForm.Tag = newFilename;
                activeBrowserForm.TabText = newFilename;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "AutoTester Script | *.ats";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach(var file in openFileDialog.FileNames)
                    {
                        var browserForm = new TestingBrowserForm() { Code = File.ReadAllText(file), TabText = file };
                        browserForm.Show(dockPanel1, DockState.Document);
                        _browsers.Add(browserForm);
                    }
                }
            }

        }

        private void runAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RunAllTests();
        }

        private void dockPanel1_ContentRemoved(object sender, DockContentEventArgs e)
        {
            _browsers.Remove(e.Content as TestingBrowserForm);
        }

        private void runActiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RunActiveTest();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void puppeteerSharpAPIDocumentationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.puppeteersharp.com/api/index.html");
        }

        #endregion


    }
}
