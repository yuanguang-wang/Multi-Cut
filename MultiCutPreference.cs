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
        public const string General_SplitCheck = "GeneralSplitCheck";
        public const string PredictionLine_EnabledCheck = "PredictionLineEnabledCheck";
        public const string PredictionLine_PriorityCheck = "PredictionLinePriorityCheck";
        public const string PredictionLine_ColorCheck = "PredictionLineColorCheck";
        public const string PredictionLine_ColorCustom = "PredictionLineColorCustom";
        public const string PredictionLine_WidthCheck = "PredictionLineWidthCheck";
        public const string PredictionLine_WidthSlide = "PredictionLineWidthSlide";
    }
    
    public class MultiCutPreference 
    {
        #region ATTR
        
        public readonly Color DefaultColor = Colors.LimeGreen;
        private PersistentSettings PlugInSettings => MultiCutPlugin.Instance.Settings;
        public bool IsSplitChecked { get; set; }
        public bool IsProphetChecked { get; set; }
        public bool IsPriorityChecked { get; set; }
        public System.Drawing.Color ProphetColor { get; set; }
        public int ProphetWidth { get; set; }


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

        private void LoadColorSetting(string keyword, PersistentSettings plugInSettingCollection, System.Drawing.Color defaultValue)
        {
            bool isSettingExist = plugInSettingCollection.TryGetColor(keyword, out System.Drawing.Color value);
            this.PlugInSettings.SetColor(keyword, isSettingExist ? value : defaultValue);
        }

        private void LoadIntSetting(string keyword, PersistentSettings plugInSettingCollection, int defaultValue)
        {
            bool isSettingExist = plugInSettingCollection.TryGetInteger(keyword, out int value);
            this.PlugInSettings.SetInteger(keyword, isSettingExist ? value : defaultValue);
        }

        public void LoadSettingBundle()
        {
            this.LoadBoolSetting(SettingKey.General_SplitCheck, PlugInSettings, false);
            this.LoadBoolSetting(SettingKey.PredictionLine_EnabledCheck, PlugInSettings, true);
            this.LoadBoolSetting(SettingKey.PredictionLine_PriorityCheck, PlugInSettings, true);
            this.LoadBoolSetting(SettingKey.PredictionLine_ColorCheck, PlugInSettings, false);
            this.LoadColorSetting(SettingKey.PredictionLine_ColorCustom, PlugInSettings, this.DefaultColor.ToSystemDrawing());
            this.LoadBoolSetting(SettingKey.PredictionLine_WidthCheck, PlugInSettings, false);
            this.LoadIntSetting(SettingKey.PredictionLine_WidthSlide, PlugInSettings, 1);
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
        public RhinoDoc CurrentDoc => MethodBasic.CurrentDoc;
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
            this.SetGroupLayout();

            this.Content = this.GroupBoxLayout;

        }

        private void SetSplitCheck()
        {
            this.SplitCheck = new CheckBox(){Text = "Split if possible", ThreeState = false};
            this.SplitCheck.Load += (sender, args) =>
            {
                bool isSplitted = this.McPlugin.Settings.GetBool(SettingKey.General_SplitCheck);
                this.SplitCheck.Checked = isSplitted;
                this.McPref.IsSplitChecked = isSplitted;
            };
            this.SplitCheck.CheckedChanged += (sender, args) =>
            {
                // ReSharper disable once PossibleInvalidOperationException
                bool isChecked = (bool)this.SplitCheck.Checked;
                this.McPref.IsSplitChecked = isChecked;
                this.McPlugin.Settings.SetBool(SettingKey.General_SplitCheck, isChecked);
                this.McPlugin.SaveSettings();
            };
        }

        public void SetGroupLayout()
        {
            this.GroupBoxLayout = new DynamicLayout();
            this.GroupBoxLayout.AddRow(this.SplitCheck);
        }

    }

    public class PredictionLineBox : GroupBox, IGroupCommon
    {
        public RhinoDoc CurrentDoc => MethodBasic.CurrentDoc;
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
            this.SetGroupLayout();

            this.Content = this.GroupBoxLayout;
        }

        #region EnableCheck

        private CheckBox EnableCheck { get; set; }
        private void SetEnableCheck()
        {
            this.EnableCheck = new CheckBox(){ Text = "Enable", ThreeState = false };
            this.EnableCheck.Load += OnEnableCheckLoad;
            this.EnableCheck.Load += OnColorPickEnabled;
            this.EnableCheck.CheckedChanged += OnEnableCheckChanged;
            this.EnableCheck.CheckedChanged += OnColorPickEnabled;
        }
        private void OnEnableCheckLoad(object sender, EventArgs e)
        {
            // get DB //
            bool isChecked = this.McPlugin.Settings.GetBool(SettingKey.PredictionLine_EnabledCheck);
            // set CheckBox //
            this.EnableCheck.Checked = isChecked;
            // set mcp ATTR //
            this.SetMcPrefProphet(isChecked);
            // distribute sub setting //
            this.EnableSubSetting(isChecked);
        }
        private void OnEnableCheckChanged(object sender, EventArgs e)
        {
            // ReSharper disable once PossibleInvalidOperationException //
            bool isChecked = (bool)this.EnableCheck.Checked;
            // set DB //
            this.McPlugin.Settings.SetBool(SettingKey.PredictionLine_EnabledCheck, isChecked);
            this.McPlugin.SaveSettings();
            //set mcp ATTR //
            this.SetMcPrefProphet(isChecked);
            // distribute sub setting //
            this.EnableSubSetting(isChecked);
        }
        private void SetMcPrefProphet(bool isChecked)
        {
            this.McPref.IsProphetChecked = isChecked;
        }
        private void EnableSubSetting(bool isChecked)
        {
            this.PriorityCheck.Enabled = isChecked;
            this.ColorCheck.Enabled = isChecked;
            this.WidthCheck.Enabled = isChecked;
        }
        
        #endregion
        #region PriorityCheck

        private CheckBox PriorityCheck { get; set; }
        private void SetPriorityCheck()
        {
            this.PriorityCheck = new CheckBox(){ Text = "Prioritize", ThreeState = false };
            this.PriorityCheck.Load += this.OnPriorityCheckLoad;
            this.PriorityCheck.CheckedChanged += this.OnPriorityCheckChanged;
        }
        private void OnPriorityCheckLoad(object sender, EventArgs e)
        {
            bool isChecked = this.McPlugin.Settings.GetBool(SettingKey.PredictionLine_PriorityCheck);
            this.PriorityCheck.Checked = isChecked;
            this.SetMcPrefPriority(isChecked);
        }
        private void OnPriorityCheckChanged(object sender, EventArgs e)
        {
            // ReSharper disable once PossibleInvalidOperationException
            bool isChecked = (bool)this.PriorityCheck.Checked;
            this.McPlugin.Settings.SetBool(SettingKey.PredictionLine_PriorityCheck, isChecked);
            this.McPlugin.SaveSettings();
            this.SetMcPrefPriority(isChecked);
        }
        private void SetMcPrefPriority(bool isChecked)
        {
            this.McPref.IsPriorityChecked = isChecked;
        }

        #endregion
        #region ColorCheck

        private CheckBox ColorCheck { get; set; }
        private void SetColorCheck()
        {
            this.ColorCheck = new CheckBox(){Text = "Customize Color", ThreeState = false };
            this.ColorCheck.Load += OnColorCheckLoad;
            this.ColorCheck.Load += OnColorPickEnabled;
            this.ColorCheck.CheckedChanged += OnColorCheckChanged;
            this.ColorCheck.CheckedChanged += OnColorPickEnabled;
        }
        private void OnColorCheckLoad(object sender, EventArgs e)
        {
            bool isChecked = this.McPlugin.Settings.GetBool(SettingKey.PredictionLine_ColorCheck);
            this.ColorCheck.Checked = isChecked;
        }
        private void OnColorCheckChanged(object sender, EventArgs e)
        {
            // ReSharper disable once PossibleInvalidOperationException
            bool isChecked = (bool)this.ColorCheck.Checked;
            this.McPlugin.Settings.SetBool(SettingKey.PredictionLine_ColorCheck, isChecked);
            this.McPlugin.SaveSettings();
            // Set Default Color of ColorPick
            Color color = isChecked 
                        ? this.McPlugin.Settings.GetColor(SettingKey.PredictionLine_ColorCustom).ToEto() 
                        : this.DefaultColor;
            this.ColorPick.Value = color;
            this.SetMcPrefColor(color);
        }
        private void OnColorPickEnabled(object sender, EventArgs e)
        {
            bool doubleCheck = MethodBasic.DoubleCheck(this.EnableCheck.Checked, 
                                                        this.ColorCheck.Checked);
            this.ColorPick.Enabled = doubleCheck;
        }

        #endregion
        #region ColorPick
        
        private ColorPicker ColorPick { get; set; }
        private Color DefaultColor => this.McPref.DefaultColor;
        private void SetColorPick()
        {
            this.ColorPick = new ColorPicker(){ Value = this.DefaultColor };
            this.ColorPick.Load += OnColorPickLoad;
            this.ColorPick.ValueChanged += OnColorPickChanged;
        }
        private void OnColorPickLoad(object sender, EventArgs e)
        {
            Color color = this.McPlugin.Settings.GetColor(SettingKey.PredictionLine_ColorCustom).ToEto(); 
            this.ColorPick.Value = color;
            this.SetMcPrefColor(color);
        }
        private void OnColorPickChanged(object sender, EventArgs e)
        {
            Color color = this.ColorPick.Value;
            if (color != this.DefaultColor)
            {
                this.McPlugin.Settings.SetColor(SettingKey.PredictionLine_ColorCustom, color.ToSystemDrawing());
                this.SetMcPrefColor(color);
                this.McPlugin.SaveSettings();
            }
        }
        private void SetMcPrefColor(Color color)
        {
            this.McPref.ProphetColor = color.ToSystemDrawing();
        }

        #endregion
        #region WidthCheck

        private CheckBox WidthCheck { get; set; }
        private void SetWidthCheck()
        {
            this.WidthCheck = new CheckBox() { Text = "Customize Linewidth", ThreeState = false };
            this.WidthCheck.Load += OnWidthCheckLoad;
            this.WidthCheck.Load += OnWidthSlideEnabled;
            this.WidthCheck.CheckedChanged += OnWidthCheckChanged;
            this.WidthCheck.CheckedChanged += OnWidthSlideEnabled;
        }
        private void OnWidthCheckLoad(object sender, EventArgs e)
        {
            bool isChecked = this.McPlugin.Settings.GetBool(SettingKey.PredictionLine_WidthCheck);
            this.WidthCheck.Checked = isChecked;
        }
        private void OnWidthCheckChanged(object sender, EventArgs e)
        {
            // ReSharper disable once PossibleInvalidOperationException
            bool isChecked = (bool)this.WidthCheck.Checked;
            this.McPlugin.Settings.SetBool(SettingKey.PredictionLine_WidthCheck, isChecked);
            // Set Slide Number
            int slideNum = !isChecked 
                         ? this.displayThickness
                         : this.McPlugin.Settings.GetInteger(SettingKey.PredictionLine_WidthSlide);
            int slideNumChanged = MethodBasic.RemapInt(slideNum);
            this.WidthSlide.Value = slideNumChanged;
        }
        private void OnWidthSlideEnabled(object sender, EventArgs e)
        {
            bool doubleCheck = MethodBasic.DoubleCheck(this.EnableCheck.Checked, this.WidthCheck.Checked);
            this.WidthSlide.Enabled = doubleCheck;
        }

        #endregion
        #region WidthSlide

        private int displayThickness => this.CurrentDoc.Views.ActiveView.DisplayPipeline.DisplayPipelineAttributes.CurveThickness;
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
            this.WidthSlide.Load += OnWidthCheckChanged;
            this.WidthSlide.ValueChanged += OnWidthSlideChanged;
        }
        private void OnWidthSlideChanged(object sender, EventArgs e)
        {
            int slideNum = this.WidthSlide.Value;
            int slideNumRemap = MethodBasic.RemapInt(slideNum);
            int displayNum = this.displayThickness;
            if (displayNum != slideNumRemap)
            { 
                this.McPlugin.Settings.SetInteger(SettingKey.PredictionLine_WidthSlide, slideNumRemap);
            }
            this.SetMcPrefInt(slideNumRemap);
        }
        private void SetMcPrefInt(int slideNum)
        {
            this.McPref.ProphetWidth = slideNum;
        }

        #endregion
        #region Layout

        public void SetGroupLayout()
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

    public class OctopusLineBox : GroupBox, IGroupCommon
    {
        public RhinoDoc CurrentDoc => MethodBasic.CurrentDoc;
        public MultiCutPreference McPref => MultiCutPreference.Instance;
        public MultiCutPlugin McPlugin => MultiCutPlugin.Instance;
        public DynamicLayout GroupBoxLayout { get; set; }
        public void SetGroupLayout()
        {
        }
    }

    public interface IGroupCommon
    {
        RhinoDoc CurrentDoc { get; }
        MultiCutPreference McPref { get; }
        MultiCutPlugin McPlugin { get; }
        DynamicLayout GroupBoxLayout { get; set; }
        void SetGroupLayout();
    }

}