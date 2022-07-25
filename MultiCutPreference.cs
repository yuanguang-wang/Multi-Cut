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
        private GeneralBox General => GeneralBox.Instance;
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
            
            this.SetLayout();
            this.Content = PreferenceLayout;

        }

        private void SetLayout()
        {
            this.PreferenceLayout = new DynamicLayout();

            this.PreferenceLayout.Spacing = new Size(10,10);
            this.PreferenceLayout.AddRow(this.General);
            
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

    public class GeneralBox : GroupBox, IGroupCommon
    {
        public MultiCutPlugin McPlugin => MultiCutPlugin.Instance;
        public DynamicLayout GroupLayout { get; set; }
        public static GeneralBox Instance { get; } = new GeneralBox();
        private CheckBox SplitCheck { get; set; }

        private GeneralBox()
        {
            this.Text = "General";
            this.Padding = new Padding(10);
            
            this.SetSplitCheck();
            this.SetLayout();

            this.Content = this.GroupLayout;

        }

        private void SetSplitCheck()
        {
            this.SplitCheck = new CheckBox(){Text = "Split Is Possible", ThreeState = false};
            this.SplitCheck.Load += (sender, args) =>
            {
                bool isSplitted = this.McPlugin.Settings.GetBool("SplitCheck");
                this.SplitCheck.Checked = isSplitted;
            };
            this.SplitCheck.CheckedChanged += (sender, args) =>
            {
                // ReSharper disable once PossibleInvalidOperationException
                bool isChecked = (bool)this.SplitCheck.Checked;
                this.McPlugin.Settings.SetBool("SplitCheck", isChecked);
                this.McPlugin.SaveSettings();
            };
        }

        public void SetLayout()
        {
            this.GroupLayout = new DynamicLayout();
            this.GroupLayout.AddRow(this.SplitCheck);
        }

    }

    public interface IGroupCommon
    {
        MultiCutPlugin McPlugin { get; }
        DynamicLayout GroupLayout { get; set; }
        void SetLayout();
    }

}