﻿using System;
using Eto.Forms;
using Eto.Drawing;
using DWSIM.UI.Forms;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using DWSIM.UI.Forms.Forms;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.IO;

namespace DWSIM.UI
{
    partial class MainForm : Form
    {

        void InitializeComponent()
        {
            string imgprefix = "DWSIM.UI.Forms.Resources.Icons.";

            Title = "DWSIMLauncher".Localize();
            ClientSize = new Size(490, 210);
            Icon = Eto.Drawing.Icon.FromResource(imgprefix + "DWSIM_ico.ico");

            var bgcolor = new Color(0.051f, 0.447f, 0.651f);

            Eto.Style.Add<Button>("main", button => { button.BackgroundColor = bgcolor;
                                                      button.Font = new Font(FontFamilies.Sans, 12f, FontStyle.None);
                                                      button.TextColor = Colors.White;
                                                      button.ImagePosition = ButtonImagePosition.Left;
                                                      button.Width = 230;
                                                    });

            var btn1 = new Button(){ Style = "main", Text = "OpenSavedFile".Localize(), Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "OpenFolder_100px.png"), 40, 40, ImageInterpolation.Default)};
            var btn2 = new Button(){ Style = "main", Text = "NewSimulation".Localize(), Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "Workflow_100px.png"), 40, 40, ImageInterpolation.Default)};
            var btn3 = new Button(){ Style = "main", Text = "NewCompound".Localize(), Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "Peptide_100px.png"), 40, 40, ImageInterpolation.Default)};
            var btn4 =  new Button(){ Style = "main", Text = "NewDataRegression".Localize(), Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "AreaChart_100px.png"), 40, 40, ImageInterpolation.Default)};
            var btn5 = new Button() { Style = "main", Text = "OpenSamples".Localize(), Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "OpenBook_100px.png"), 40, 40, ImageInterpolation.Default) };
            var btn6 = new Button(){ Style = "main", Text = "Help".Localize(), Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "Help_100px.png"), 40, 40, ImageInterpolation.Default)};
            var btn7 = new Button() { Style = "main", Text = "About".Localize(), Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "Info_100px.png"), 40, 40, ImageInterpolation.Default) };
            var btn8 = new Button() { Style = "main", Text = "Donate".Localize(), Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "Donate_100px.png"), 40, 40, ImageInterpolation.Default) };

            btn5.Click += (sender, e) => {
                var dialog = new OpenFileDialog();
                dialog.Title = "OpenSamples".Localize();
                dialog.Filters.Add(new FileDialogFilter("XML Simulation File".Localize(), new[] { ".dwxml", ".dwxmz" }));
                dialog.MultiSelect = false;
                dialog.Directory = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "samples"));
                dialog.CurrentFilterIndex = 0;
                if (dialog.ShowDialog(this) == DialogResult.Ok)
                {
                    LoadSimulation(dialog.FileName);
                }
            };

            btn6.Click += (sender, e) =>
            {
                Process.Start("http://dwsim.inforside.com.br/docs/mobile/help/");
            };

            btn1.Click += (sender, e) => {
                var dialog = new OpenFileDialog();
                dialog.Title = "Open File".Localize();
                dialog.Filters.Add(new FileDialogFilter("XML Simulation File".Localize(), new[] { ".dwxml", ".dwxmz" }));
                dialog.MultiSelect = false;
                dialog.CurrentFilterIndex = 0;
                if (dialog.ShowDialog(this) == DialogResult.Ok)
                {
                    LoadSimulation(dialog.FileName);
                }
                
            };

            btn2.Click += (sender, e) =>
            {
                new Forms.Flowsheet().Show();
            };

            btn7.Click += (sender, e) => new About().Show();
            btn8.Click += (sender, e) => Process.Start("http://sourceforge.net/p/dwsim/donate/");

            Content = new TableLayout
            {
                Padding = 10,
                Spacing = new Size(5, 5),
                Rows =
                {
                    new TableRow(btn1, btn2, null),
                    //new TableRow(btn3, btn4, null),
                    new TableRow(btn5, btn6, null),
                    new TableRow(btn7, btn8, null),
                    null
                },
                BackgroundColor = bgcolor,
            };

            var quitCommand = new Command { MenuText = "Quit".Localize(), Shortcut = Application.Instance.CommonModifier | Keys.Q };
            quitCommand.Executed += (sender, e) => Application.Instance.Quit();

            var aboutCommand = new Command { MenuText = "About".Localize() };
            aboutCommand.Executed += (sender, e) => new About().Show();

            var aitem1 = new ButtonMenuItem { Text = "Preferences".Localize() };
            aitem1.Click += (sender, e) =>
            {
                new Forms.Forms.GeneralSettings().GetForm().Show();
            };

            var hitem1 = new ButtonMenuItem { Text = "Help".Localize() };
            hitem1.Click += (sender, e) =>
            {
                Process.Start("http://dwsim.inforside.com.br/docs/mobile/help/");
            };

            // create menu
            Menu = new MenuBar
            {
                ApplicationItems = {aitem1},
                QuitItem = quitCommand,
                HelpItems = {hitem1},
                AboutItem = aboutCommand
            };

            Shown += MainForm_Shown;

            Closing += MainForm_Closing;
                        
        }

        void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show(this, "ConfirmAppExit".Localize(), "AppExit".Localize(), MessageBoxButtons.YesNo, MessageBoxType.Question, MessageBoxDefaultButton.No) == DialogResult.No)
            {
                e.Cancel = true; 
            }
        }

        void MainForm_Shown(object sender, EventArgs e)
        {
            Application.Instance.Invoke(() =>
            {
                var splash = new SplashScreen();
                splash.Show();
            });
        }

        void LoadSimulation(string path)
        {

            var form = new Forms.Flowsheet();

            var loadingdialog = new LoadingData();
            loadingdialog.Show();

            Task.Factory.StartNew(() =>
            {
                if (System.IO.Path.GetExtension(path).ToLower() == ".dwxmz")
                {
                    form.FlowsheetObject.LoadZippedXML(path);
                }
                else if (System.IO.Path.GetExtension(path).ToLower() == ".dwxml")
                {
                    form.FlowsheetObject.LoadFromXML(XDocument.Load(path));
                }
            }).ContinueWith((t) =>
            {
                Application.Instance.Invoke(() =>
                {
                    loadingdialog.Close();
                    var surface = (DWSIM.Drawing.SkiaSharp.GraphicsSurface)form.FlowsheetObject.GetSurface();
                    surface.ZoomAll(ClientSize.Width, ClientSize.Height);
                    surface.ZoomAll(ClientSize.Width, ClientSize.Height);
                    form.Title = form.FlowsheetObject.Options.SimulationName + " [" + form.FlowsheetObject.Options.FilePath + "]";
                    form.Show();
                });
            });
        }

    }
}