using System;
using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;
using Rhino.UI;


namespace MultiCut
{
    public class MultiCutPreference
    {
        #region ATTR
        
        public bool IsBrepSplitted { get; set; }


        #endregion
        #region CTOR

        public static MultiCutPreference Instance { get; } = new MultiCutPreference();

        private MultiCutPreference()
        {
            this.IsBrepSplitted = false;
        }

        #endregion
        #region MTHD
        
        

        

        #endregion

    }

    public class PreferenceForm : Form
    {
        private MultiCutPreference McPreference => MultiCutPreference.Instance;
        private MultiCutPlugin McPlugin => MultiCutPlugin.Instance;
        private IEnumerable<object> SplitOptBoolList => new object[] { true, false };
        private Label SplitOptLabel => new Label(){Text = "Split if possible:"};
        private RadioButtonList SplitOptRBList { get; set; }
        private DynamicLayout PreferenceLayout { get; set; }


        public PreferenceForm()
        {
            this.Padding = new Padding(10);
            this.Owner = RhinoEtoApp.MainWindow;
            this.Title = "Multi-Cut Preference";
            this.Maximizable = false;
            this.Minimizable = false;
            this.ShowInTaskbar = false;
            
            this.SetSplitRBList();
            this.SetLayout();
            this.Content = PreferenceLayout;

        }

        private void SetLayout()
        {
            this.PreferenceLayout = new DynamicLayout();

            this.PreferenceLayout.Spacing = new Size(10,10);
            this.PreferenceLayout.AddRow(this.SplitOptLabel, this.SplitOptRBList);
            
        }

        private void SetSplitRBList()
        {
            this.SplitOptRBList = new RadioButtonList()
            {
                Orientation = Orientation.Horizontal,
                DataStore = SplitOptBoolList,
                Spacing = new Size(5,5)
            
            };
            this.SplitOptRBList.Load += (sender, args) =>
            {
                // Read
                bool isBrepSplitted = McPlugin.Settings.GetBool("SplitOption");
                if (isBrepSplitted)
                {
                    this.SplitOptRBList.SelectedIndex = 0;
                }
                else if (!isBrepSplitted)
                {
                    this.SplitOptRBList.SelectedIndex = 1;
                }
                
            };
            this.SplitOptRBList.SelectedIndexChanged += (sender, args) =>
            {
                // Write
                if (this.SplitOptRBList.SelectedIndex == 0)
                {
                    McPlugin.Settings.SetBool("SplitOption", true);
                }
                else if (this.SplitOptRBList.SelectedIndex == 1)
                {
                    McPlugin.Settings.SetBool("SplitOption", false);
                }
                McPlugin.SaveSettings();
                McPreference.IsBrepSplitted = McPlugin.Settings.GetBool("SplitOption");
            };
        }

        protected override void OnLoad(EventArgs e)
        {
            this.Location = new Point(Mouse.Position);
            base.OnLoad(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            this.Dispose();
        }
    }
}