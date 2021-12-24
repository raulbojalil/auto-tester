using CefSharp;
using CefSharp.Puppeteer;
using Microsoft.CSharp;
using ScintillaNET;
using System;
using System.CodeDom.Compiler;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace AutoTester.DockableForms
{
    public partial class TestingBrowserForm : DockContent
    {
        public string Code
        {
            set { scintilla1.Text = value; }
            get { return scintilla1.Text; }
        }

        private Page _page;

        public TestingBrowserForm()
        {        
            InitializeComponent();
        }

        public async Task ExecuteScript(string sourceText)
        {
            if (string.IsNullOrEmpty(sourceText)) return;

            var codeProvider = new CSharpCodeProvider();
            ICodeCompiler compiler = codeProvider.CreateCompiler();
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true;
            parameters.OutputAssembly = "CS-Script-Tmp-Junk";
            parameters.MainClass = "CScript.Main";
            parameters.IncludeDebugInformation = false;

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    parameters.ReferencedAssemblies.Add(asm.Location);
                }
                catch
                {

                }
            }

            CompilerResults results = compiler.CompileAssemblyFromSource(parameters, $@"

using CefSharp;
using CefSharp.Puppeteer;
using System;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoTester.Extensions;

class CScript {{
    public async Task<string> Execute(Page page) {{

        try {{
            {sourceText}
        }}
        catch(Exception ex) {{ 
            return ex.Message + "" "" + ex.StackTrace;  
        }}

        return string.Empty;
    }}
}}
");

            if (results.Errors.Count > 0)
            {
                var errors = "Compilation failed:\n";
                foreach (var err in results.Errors)
                {
                    errors += err.ToString() + "\n";
                }
                MessageBox.Show(errors, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var o = results.CompiledAssembly.CreateInstance("CScript");
                var type = o.GetType();
                var m = type.GetMethod("Execute");
                var task = (Task<string>)m.Invoke(o, new object[] { (object)_page });

                var res = await task;

                if (!string.IsNullOrEmpty(res))
                {
                    MessageBox.Show(res, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (File.Exists("CS-Script-Tmp-Junk")) { File.Delete("CS-Script-Tmp-Junk"); }
            }

        }

        private async void TestingBrowserForm_Load(object sender, EventArgs e)
        {
            chromiumWebBrowser1.MenuHandler = new SearchContextMenuHandler();
            chromiumWebBrowser1.LoadUrl("chrome://version");

            await chromiumWebBrowser1.WaitForInitialLoadAsync();

            _page = await chromiumWebBrowser1.GetPuppeteerPageAsync();

            scintilla1.Lexer = Lexer.Cpp;

            scintilla1.SetProperty("fold", "1");
            scintilla1.SetProperty("fold.compact", "1");

            scintilla1.Margins[2].Type = MarginType.Symbol;
            scintilla1.Margins[2].Mask = Marker.MaskFolders;
            scintilla1.Margins[2].Sensitive = true;
            scintilla1.Margins[2].Width = 20;

            for (int i = 25; i <= 31; i++)
            {
                scintilla1.Markers[i].SetForeColor(SystemColors.ControlLightLight);
                scintilla1.Markers[i].SetBackColor(SystemColors.ControlDark);
            }

            scintilla1.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
            scintilla1.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
            scintilla1.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
            scintilla1.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            scintilla1.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
            scintilla1.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            scintilla1.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            scintilla1.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);

            scintilla1.StyleResetDefault();
            scintilla1.Styles[Style.Default].Font = "Consolas";
            scintilla1.Styles[Style.Default].Size = 10;
            scintilla1.StyleClearAll();

            scintilla1.Styles[Style.Cpp.Default].ForeColor = Color.Silver;
            scintilla1.Styles[Style.Cpp.Comment].ForeColor = Color.FromArgb(0, 128, 0); // Green
            scintilla1.Styles[Style.Cpp.CommentLine].ForeColor = Color.FromArgb(0, 128, 0); // Green
            scintilla1.Styles[Style.Cpp.CommentLineDoc].ForeColor = Color.FromArgb(128, 128, 128); // Gray
            scintilla1.Styles[Style.Cpp.Number].ForeColor = Color.Olive;
            scintilla1.Styles[Style.Cpp.Word].ForeColor = Color.Blue;
            scintilla1.Styles[Style.Cpp.Word2].ForeColor = Color.Blue;
            scintilla1.Styles[Style.Cpp.String].ForeColor = Color.FromArgb(163, 21, 21); // Red
            scintilla1.Styles[Style.Cpp.Character].ForeColor = Color.FromArgb(163, 21, 21); // Red
            scintilla1.Styles[Style.Cpp.Verbatim].ForeColor = Color.FromArgb(163, 21, 21); // Red
            scintilla1.Styles[Style.Cpp.StringEol].BackColor = Color.Pink;
            scintilla1.Styles[Style.Cpp.Operator].ForeColor = Color.Purple;
            scintilla1.Styles[Style.Cpp.Preprocessor].ForeColor = Color.Maroon;
        }

        public async Task Run()
        {
            await ExecuteScript(scintilla1.Text);
        }

        private async void btnRun_Click(object sender, EventArgs e)
        {
            await Run();
        }
    }

    public class SearchContextMenuHandler : IContextMenuHandler
    {
        public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            model.Clear();
            model.AddItem(CefMenuCommand.Back, "Go Back");
            model.AddItem(CefMenuCommand.Reload, "Reload");
            model.AddItem(CefMenuCommand.ReloadNoCache, "Reload (No Cache)");
            model.AddItem(CefMenuCommand.Print, "Inspect element");
        }

        public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters,
            CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            if (commandId == CefMenuCommand.Reload)
            {
                browser.Reload();
                return true;
            }
            if (commandId == CefMenuCommand.ReloadNoCache)
            {
                browser.Reload(true);
                return true;
            }
            if (commandId == CefMenuCommand.Print)
            {
                CefSharp.WebBrowserExtensions.ShowDevTools(browser, null, parameters.XCoord, parameters.YCoord);
                return true;
            }
            if (commandId == CefMenuCommand.Back)
            {
                browser.GoBack();
                return true;
            }

            return false;
        }

        public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {
        }

        public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            return false;
        }
    }
}
