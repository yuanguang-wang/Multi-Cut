using System;
using System.Collections.Generic;

using Eto.Drawing;
using Eto.Forms;

using Rhino.UI;
using Rhino;


namespace MultiCut
{
    internal static class SettingKey
    {
        public const string SplitCheck = "SplitCheck";
        public const string PredictionLineEnabledCheck = "PredictionLineEnabledCheck";
    }
    
    public class MultiCutPreference 
    {
        #region ATTR

        private PersistentSettings PlugInSettings => MultiCutPlugin.Instance.Settings;
        public bool IsSplitEnabled { get; set; }
        public bool IsProphetEnabled { get; set; }
        public bool IsPriorityEnabled { get; set; }


        #endregion
        #region CTOR

        public static MultiCutPreference Instance { get; } = new MultiCutPreference();
        private MultiCutPreference() { }

        #endregion
        #region MTHD
        
        private void LoadBoolSetting(string keyword, PersistentSettings plugInSettingCollection, bool defaultValue)
        {
            bool isSettingExist = plugInSettingCollection.TryGetBool(keyword, out bool value);
            this.PlugInSettings.SetBool(keyword, isSettingExist ? value : defaultValue);
        }

        public void LoadSettingBundle()
        {
            this.LoadBoolSetting(SettingKey.SplitCheck, PlugInSettings, false);
            this.LoadBoolSetting(SettingKey.PredictionLineEnabledCheck, PlugInSettings, true);
        }
        
        #endregion
    }

    public class PreferenceForm : Form
    {
        private GeneralBox General => GeneralBox.Instance;
        private PredictionLineBox PredictionLine => PredictionLineBox.Instance;
        private DynamicLayout PreferenceLayout { get; set; }


        public PreferenceForm()
        {
            this.Owner = RhinoEtoApp.MainWindow;
            this.Title = "Multi-Cut Preference";
            this.Maximizable = false;
            this.Minimizable = false;
            this.ShowInTaskbar = false;
            this.Resizable = false;
            
            this.SetLayout();
            this.Content = PreferenceLayout;

        }

        private void SetLayout()
        {
            this.PreferenceLayout = new DynamicLayout();

            this.PreferenceLayout.Spacing = new Size(10,10);
            IEnumerable<GroupBox> controls = new GroupBox[] { this.General, this.PredictionLine };
            this.PreferenceLayout.AddSeparateColumn(new Padding(10), 10, false, false, controls);

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
        public MultiCutPreference McPref => MultiCutPreference.Instance;
        public MultiCutPlugin McPlugin => MultiCutPlugin.Instance;
        public DynamicLayout GroupBoxLayout { get; set; }
        public static GeneralBox Instance { get; } = new GeneralBox();
        private CheckBox SplitCheck { get; set; }

        private GeneralBox()
        {
            this.Text = "General";
            this.Padding = new Padding(10);
            
            this.SetSplitCheck();
            this.SetLayout();

            this.Content = this.GroupBoxLayout;

        }

        private void SetSplitCheck()
        {
            this.SplitCheck = new CheckBox(){Text = "Split if possible", ThreeState = false};
            this.SplitCheck.Load += (sender, args) =>
            {
                bool isSplitted = this.McPlugin.Settings.GetBool(SettingKey.SplitCheck);
                this.SplitCheck.Checked = isSplitted;
                this.McPref.IsSplitEnabled = isSplitted;
            };
            this.SplitCheck.CheckedChanged += (sender, args) =>
            {
                // ReSharper disable once PossibleInvalidOperationException
                bool isChecked = (bool)this.SplitCheck.Checked;
                this.McPref.IsSplitEnabled = isChecked;
                this.McPlugin.Settings.SetBool(SettingKey.SplitCheck, isChecked);
                this.McPlugin.SaveSettings();
            };
        }

        public void SetLayout()
        {
            this.GroupBoxLayout = new DynamicLayout();
            this.GroupBoxLayout.AddRow(this.SplitCheck);
        }

    }

    public class PredictionLineBox : GroupBox, IGroupCommon
    {
        public MultiCutPreference McPref => MultiCutPreference.Instance;
        public MultiCutPlugin McPlugin => MultiCutPlugin.Instance;
        public DynamicLayout GroupBoxLayout { get; set; }
        public static PredictionLineBox Instance { get; } = new PredictionLineBox();

        private PredictionLineBox()
        {
            this.Text = "Prediction Line";

            this.SetEnableCheck();
            this.SetPriorityCheck();
            this.SetColorCheck();
            this.SetColorPick();
            this.SetWidthCheck();
            this.SetWidthSlide();
            this.SetLayout();

            this.Content = this.GroupBoxLayout;
        }

        #region Enable

        private CheckBox EnableCheck { get; set; }
        private void SetEnableCheck()
        {
            this.EnableCheck = new CheckBox(){ Text = "Enable", ThreeState = false };
            this.EnableCheck.Load += OnEnableCheckLoad;
            this.EnableCheck.CheckedChanged += OnEnableCheckChanged;
        }
        private void OnEnableCheckLoad(object sender, EventArgs e)
        {
            // get DB //
            bool isChecked = this.McPlugin.Settings.GetBool(SettingKey.PredictionLineEnabledCheck);
            // set CheckBox //
            this.EnableCheck.Checked = isChecked;
            // set mcp ATTR //
            this.SetMcpAttr(isChecked);
            // distribute sub setting //
            this.EnableSubSetting(isChecked);

        }
        private void OnEnableCheckChanged(object sender, EventArgs e)
        {
            // ReSharper disable once PossibleInvalidOperationException //
            bool isChecked = (bool)this.EnableCheck.Checked;
            // set DB //
            this.McPlugin.Settings.SetBool(SettingKey.PredictionLineEnabledCheck, isChecked);
            //set mcp ATTR //
            this.SetMcpAttr(isChecked);
            // distribute sub setting //
            this.EnableSubSetting(isChecked);
            
        }
        private void SetMcpAttr(bool isChecked)
        {
            this.McPref.IsProphetEnabled = isChecked;
        }

        private void EnableSubSetting(bool isChecked)
        {
            this.PriorityCheck.Enabled = isChecked;
            this.ColorCheck.Enabled = isChecked;
            this.WidthCheck.Enabled = isChecked;
        }
        
        #endregion
        #region Priority

        private CheckBox PriorityCheck { get; set; }
        private void SetPriorityCheck()
        {
            this.PriorityCheck = new CheckBox(){ Text = "Prioritize", ThreeState = false };
            this.PriorityCheck.Load += this.OnPriorityChecked;
            this.PriorityCheck.CheckedChanged += this.OnPriorityChecked;
        }

        private void OnPriorityChecked(object sender, EventArgs e)
        {
            bool doubleCheck = MethodBasic.DoubleCheck(this.EnableCheck.Checked, 
                                                       this.PriorityCheck.Checked);
            McPref.IsPriorityEnabled = doubleCheck;
        }

        #endregion
        #region Color

        private CheckBox ColorCheck { get; set; }
        private void SetColorCheck()
        {
            this.ColorCheck = new CheckBox(){Text = "Customize Color", ThreeState = false };
            this.ColorCheck.Load += OnColorChecked;
            this.ColorCheck.CheckedChanged += OnColorChecked;
        }
        private void OnColorChecked(object sender, EventArgs e)
        {
            bool doubleCheck = MethodBasic.DoubleCheck(this.EnableCheck.Checked, this.ColorCheck.Checked);
            this.ColorPick.Enabled = doubleCheck;
        }

        private ColorPicker ColorPick { get; set; }
        private void SetColorPick()
        {
            this.ColorPick = new ColorPicker() { Value = Colors.LimeGreen };
        }

        #endregion
        #region LineWidth

        private CheckBox WidthCheck { get; set; }
        private void SetWidthCheck()
        {
            this.WidthCheck = new CheckBox() { Text = "Customize Linewidth", ThreeState = false };
            this.WidthCheck.Load += OnWidthChecked;
            this.WidthCheck.CheckedChanged += OnWidthChecked;
        }
        private void OnWidthChecked(object sender, EventArgs e)
        {
            bool doubleCheck = MethodBasic.DoubleCheck(this.EnableCheck.Checked, this.WidthCheck.Checked);
            this.WidthSlide.Enabled = doubleCheck;
        }

        private Slider WidthSlide { get; set; }
        private void SetWidthSlide()
        {
            this.WidthSlide = new Slider()
            {
                MaxValue = 9,
                MinValue = 1,
                Value = 3,
                TickFrequency = 1,
                SnapToTick = true
            };
        }

        #endregion
        #region Layout

        public void SetLayout()
        {
            this.GroupBoxLayout = new DynamicLayout();
            IEnumerable<Control> predictionControls = new Control[]
            {
                this.EnableCheck,
                this.PriorityCheck,
                this.ColorCheck,
                this.ColorPick,
                this.WidthCheck,
                this.WidthSlide
            };
            this.GroupBoxLayout.AddSeparateColumn(new Padding(10), 10, false, false, predictionControls);
        }
        
        #endregion
    }

    public interface IGroupCommon
    {
        MultiCutPreference McPref { get; }
        MultiCutPlugin McPlugin { get; }
        DynamicLayout GroupBoxLayout { get; set; }
        void SetLayout();
    }

}