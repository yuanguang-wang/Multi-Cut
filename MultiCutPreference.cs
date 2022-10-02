using System;
using System.Collections.Generic;

using Eto.Drawing;
using Eto.Forms;

using Rhino.UI;
using Rhino;
using Rhino.Runtime;

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
        public const string OctopusLine_EnableCheck = "OctopusLineEnableCheck";
        public const string OctopusLine_ISOCheck = "OctopusLineIsoCheck";
        public const string OctopusLine_CPLCheck = "OctopusLineCLPCheck";
        public const string OctopusLine_WPLCheck = "OctopusLineWLPCheck";
        public const string OctopusLine_ColorCheck = "OctopusLineColorCheck";
        public const string OctopusLine_ColorCustom = "OctopusLineColorCustom";
        public const string OctopusLine_WidthCheck = "OctopusLineWidthCheck";
        public const string OctopusLine_WidthSlide = "OctopusLineWidthSlide";
    }
    
    public class MultiCutPreference 
    {
        #region ATTR
        
        public readonly Color defaultProphetColor = Colors.LimeGreen;
        public readonly Color defaultOctopusColor = Colors.Blue;
        private PersistentSettings PlugInSettings => MultiCutPlugin.Instance.Settings;
        public bool IsSplitChecked { get; set; }
        public bool IsProphetChecked { get; set; }
        public bool IsPriorityChecked { get; set; }
        public System.Drawing.Color ProphetColor { get; set; }
        public int ProphetWidth { get; set; }
        public bool IsOctopusChecked { get; set; }
        public bool IsIsoChecked { get; set; }
        public bool IsCplChecked { get; set; }
        public bool IsWplChecked { get; set; }
        public System.Drawing.Color OctopusColor { get; set; }
        public int OctopusWidth { get; set; }


        #endregion
        #region CTOR

        public static MultiCutPreference Instance { get; } = new MultiCutPreference();
        private MultiCutPreference() { }

        #endregion
        #region MTHD
        
        private bool LoadBoolSetting(string keyword, PersistentSettings plugInSettingCollection, bool defaultValue)
        {
            bool isSettingExist = plugInSettingCollection.TryGetBool(keyword, out bool value);
            bool initialBool = isSettingExist ? value : defaultValue;
            this.PlugInSettings.SetBool(keyword, initialBool);
            return initialBool;
        }

        private System.Drawing.Color LoadColorSetting(string keyword, PersistentSettings plugInSettingCollection, System.Drawing.Color defaultValue)
        {
            bool isSettingExist = plugInSettingCollection.TryGetColor(keyword, out System.Drawing.Color value);
            System.Drawing.Color initialColor = isSettingExist ? value : defaultValue;
            this.PlugInSettings.SetColor(keyword, initialColor);
            return initialColor;
        }

        private int LoadIntSetting(string keyword, PersistentSettings plugInSettingCollection, int defaultValue)
        {
            bool isSettingExist = plugInSettingCollection.TryGetInteger(keyword, out int value);
            int initialValue = isSettingExist ? value : defaultValue;
            this.PlugInSettings.SetInteger(keyword, initialValue);
            return initialValue; 
        }

        /// <summary>
        /// Set DataBase !!!
        /// </summary>
        public void LoadSettingBundle()
        {
            this.IsSplitChecked = this.LoadBoolSetting(SettingKey.General_SplitCheck, PlugInSettings, false);
            this.IsProphetChecked = this.LoadBoolSetting(SettingKey.PredictionLine_EnabledCheck, PlugInSettings, true);
            this.IsPriorityChecked = this.LoadBoolSetting(SettingKey.PredictionLine_PriorityCheck, PlugInSettings, true);
            this.LoadBoolSetting(SettingKey.PredictionLine_ColorCheck, PlugInSettings, false);
            this.ProphetColor = this.LoadColorSetting(SettingKey.PredictionLine_ColorCustom, PlugInSettings, 
                this.defaultProphetColor.ToSystemDrawing());
            this.LoadBoolSetting(SettingKey.PredictionLine_WidthCheck, PlugInSettings, false);
            this.ProphetWidth = this.LoadIntSetting(SettingKey.PredictionLine_WidthSlide, PlugInSettings, 2);
            
            this.IsOctopusChecked = this.LoadBoolSetting(SettingKey.OctopusLine_EnableCheck, PlugInSettings, true);
            this.IsIsoChecked = this.LoadBoolSetting(SettingKey.OctopusLine_ISOCheck, PlugInSettings, true);
            this.IsCplChecked = this.LoadBoolSetting(SettingKey.OctopusLine_CPLCheck, PlugInSettings, true);
            this.IsWplChecked = this.LoadBoolSetting(SettingKey.OctopusLine_WPLCheck, PlugInSettings, true);
            this.LoadBoolSetting(SettingKey.OctopusLine_ColorCheck, PlugInSettings, false);
            this.OctopusColor = this.LoadColorSetting(SettingKey.OctopusLine_ColorCustom, PlugInSettings,
                this.defaultOctopusColor.ToSystemDrawing());
            this.LoadBoolSetting(SettingKey.OctopusLine_WidthCheck, PlugInSettings, false);
            this.OctopusWidth = this.LoadIntSetting(SettingKey.OctopusLine_WidthSlide, PlugInSettings, 2);

        }
        
        #endregion
    }

    public class PreferenceForm : Form
    {
        private GeneralBox General => GeneralBox.Instance;
        private AboutBox About => AboutBox.Instance;
        private PredictionLineBox PredictionLine => PredictionLineBox.Instance;
        private OctopusLineBox OctopusLine => OctopusLineBox.Instance;
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
            
            IEnumerable<GroupBox> controls_1 = new GroupBox[] { this.General, this.About, this.OctopusLine};
            IEnumerable<GroupBox> controls_2 = new GroupBox[] { this.PredictionLine };
            
            this.PreferenceLayout.BeginHorizontal();
            this.PreferenceLayout.AddSeparateColumn(new Padding(10,10,5,10), 10, false, false, controls_1);
            this.PreferenceLayout.AddSeparateColumn(new Padding(5,10,10,10), 10, false, false, controls_2);
            this.PreferenceLayout.EndHorizontal();

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

    public class AboutBox : GroupBox
    {
        public static AboutBox Instance { get; } = new AboutBox();
        private DynamicLayout AboutBoxLayout { get; set; }
        private LinkButton Link { get; set; }
        private AboutBox()
        {
            this.Text = "About";
            this.Padding = new Padding(10);

            this.SetLinkButton();
            this.SetGroupLayout();
            this.Content = this.AboutBoxLayout;
        }
        private void SetLinkButton()
        {
            this.Link = new LinkButton();
            this.Link.Text = "Documentation on Github";
            this.Link.Click += (sender, args) =>
            {
                PythonScript ps = PythonScript.Create();
                ps.ExecuteScript("import webbrowser; webbrowser.open('https://github.com/yuanguang-wang/Multi-Cut')");
            };
        }
        private void SetGroupLayout()
        {
            this.AboutBoxLayout = new DynamicLayout();
            this.AboutBoxLayout.AddRow(this.Link);
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
                        : this.DefaultProphetColor;
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
        private Color DefaultProphetColor => this.McPref.defaultProphetColor;
        private void SetColorPick()
        {
            this.ColorPick = new ColorPicker(){ Value = this.DefaultProphetColor };
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
            if (color != this.DefaultProphetColor)
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
        public static OctopusLineBox Instance { get; } = new OctopusLineBox();

        private OctopusLineBox()
        {
            this.Text = "Assistant Line";
            
            this.SetGroupLayout();
            this.Content = this.GroupBoxLayout;
        }

        #region EnableCheck

        private CheckBox EnableCheck { get; set; }
        private void SetEnableCheck()
        {
            this.EnableCheck = new CheckBox(){ Text = "Enable", ThreeState = false };
            this.EnableCheck.Load += (sender, args) =>
            {
                this.EnableCheck.Checked = McPlugin.Settings.GetBool(SettingKey.OctopusLine_EnableCheck); // get DB //
            };
            this.EnableCheck.CheckedChanged += (sender, args) =>
            {
                McPlugin.Settings.SetBool(SettingKey.OctopusLine_EnableCheck, MethodBasic.SafeCast(EnableCheck.Checked)); // set DB //
                McPref.IsOctopusChecked = MethodBasic.SafeCast(EnableCheck.Checked); // Notify MCT //
            };
            this.EnableCheck.CheckedChanged += this.EnableSubOptions;
        }
        private void EnableSubOptions(object sender, EventArgs e)
        {
            bool value = MethodBasic.SafeCast(this.EnableCheck.Checked);
            this.ISOCheck.Enabled = value;
            this.WPLCheck.Enabled = value;
            this.CPLCheck.Enabled = value;
            this.ColorCheck.Enabled = value;
            this.WidthCheck.Enabled = value;
            bool isColorPickEnabled = MethodBasic.DoubleCheck(value, this.ColorCheck.Checked);
            this.ColorPick.Enabled = isColorPickEnabled;
            bool isWidthSlideEnabled = MethodBasic.DoubleCheck(value, this.WidthCheck.Checked);
            this.WidthSlide.Enabled = isWidthSlideEnabled;
        }

        #endregion
        #region SubCheck

        private CheckBox ISOCheck { get; set; }
        private void SetIsoCheck()
        {
            this.ISOCheck = new CheckBox(){ Text = "Isocurve Intersect", ThreeState = false };
            this.ISOCheck.Load += (sender, args) =>
            {
                this.ISOCheck.Checked = McPlugin.Settings.GetBool(SettingKey.OctopusLine_ISOCheck); // Get DB //
            };
            this.ISOCheck.CheckedChanged += (sender, args) =>
            {
                bool value = MethodBasic.SafeCast(this.ISOCheck.Checked);
                McPlugin.Settings.SetBool(SettingKey.OctopusLine_ISOCheck, value); // Set DB //
                McPref.IsIsoChecked = value; // Notify MCT//
            };
        }

        private CheckBox CPLCheck { get; set; }
        private void SetCplChecked()
        {
           this.CPLCheck = new CheckBox(){ Text = "CPlane Intersect", ThreeState = false };
           this.CPLCheck.Load += (sender, args) =>
           {
               this.CPLCheck.Checked = McPlugin.Settings.GetBool(SettingKey.OctopusLine_CPLCheck); // get DB //
           };
           this.CPLCheck.Load += (sender, args) =>
           {
               bool value = MethodBasic.SafeCast(this.CPLCheck.Checked);
               McPlugin.Settings.SetBool(SettingKey.OctopusLine_CPLCheck, value); // Set DB //
               McPref.IsCplChecked = value; // Notify MCT //
           };
        }

        private CheckBox WPLCheck { get; set; }
        private void SetWplChecked()
        {
            this.WPLCheck = new CheckBox(){ Text = "WPlane Intersect", ThreeState = false };
            this.WPLCheck.Load += (sender, args) =>
            {
                this.WPLCheck.Checked = McPlugin.Settings.GetBool(SettingKey.OctopusLine_WPLCheck); // get DB //
            };
            this.WPLCheck.CheckedChanged += (sender, args) =>
            {
                bool value = MethodBasic.SafeCast(this.WPLCheck.Checked);
                this.McPlugin.Settings.SetBool(SettingKey.OctopusLine_WPLCheck, value); // Set DB //
                this.McPref.IsWplChecked = value; // Notify MCT //
            };
        }

        #endregion
        #region ColorSet

        private CheckBox ColorCheck { get; set; }
        private void SetColorCheck()
        {
            this.ColorCheck = new CheckBox(){ Text = "Customize Color", ThreeState = false };
            this.ColorCheck.Load += (sender, args) =>
            {
                this.ColorCheck.Checked = this.McPlugin.Settings.GetBool(SettingKey.OctopusLine_ColorCheck); // get DB //
            };
            this.ColorCheck.CheckedChanged += (sender, args) =>
            {
                bool value = MethodBasic.SafeCast(this.ColorCheck.Checked);
                this.McPlugin.Settings.SetBool(SettingKey.OctopusLine_ColorCheck, value); // set DB //
               
                // Control the behavior of ColorPick //
                bool doubleValue = MethodBasic.DoubleCheck(value, this.EnableCheck.Checked);
                Color color = value
                    ? this.McPlugin.Settings.GetColor(SettingKey.OctopusLine_ColorCustom).ToEto() // dispatch Color //
                    : this.DefaultOctopusColor;
                this.ColorPick.Enabled = doubleValue;
                this.ColorPick.Value = color;
                this.McPref.OctopusColor = color.ToSystemDrawing();
            };
        }

        private Color DefaultOctopusColor => this.McPref.defaultOctopusColor;
        private ColorPicker ColorPick { get; set; }
        private void SetColorPick()
        {
            this.ColorPick = new ColorPicker() { Value = this.DefaultOctopusColor };
            this.ColorPick.Load += (sender, args) =>
            {
                this.ColorPick.Value = this.McPlugin.Settings.GetColor(SettingKey.OctopusLine_ColorCustom).ToEto();
            };
            this.ColorPick.ValueChanged += (sender, args) =>
            {
                Color color = this.ColorPick.Value;
                if (color != this.DefaultOctopusColor)
                {
                    this.McPlugin.Settings.SetColor(SettingKey.OctopusLine_ColorCustom,color.ToSystemDrawing());
                    this.McPref.OctopusColor = color.ToSystemDrawing();
                }
            };
        }


        #endregion
        #region WidthSet
        private CheckBox WidthCheck { get; set; }
        private void SetWidthCheck()
        {
            this.WidthCheck = new CheckBox() { Text = "Customize LineWidth", ThreeState = false };
            this.WidthCheck.Load += (sender, args) =>
            {
                this.WidthCheck.Checked = this.McPlugin.Settings.GetBool(SettingKey.OctopusLine_WidthCheck); // get DB //
            };
            this.WidthCheck.CheckedChanged += (sender, args) =>
            {
                bool value = MethodBasic.SafeCast(this.WidthCheck.Checked);
                this.McPlugin.Settings.SetBool(SettingKey.OctopusLine_WidthCheck, value); // set DB //
                
                // Control Behavior //
                bool doubleCheck = MethodBasic.DoubleCheck(value, this.EnableCheck.Checked);
                this.WidthSlide.Enabled = doubleCheck;
                this.McPref.OctopusWidth = doubleCheck ? this.WidthSlide.Value : this.DisplayThickness;
            };
        }
        private int DisplayThickness => MethodBasic.CurrentDoc.Views.ActiveView.DisplayPipeline.DisplayPipelineAttributes.CurveThickness;
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
            this.WidthSlide.Load += (sender, args) =>
            {
                this.WidthSlide.Value = this.McPlugin.Settings.GetInteger(SettingKey.OctopusLine_WidthSlide); // get DB //
            };
            this.WidthSlide.ValueChanged += (sender, args) =>
            {
                this.McPlugin.Settings.SetInteger(SettingKey.OctopusLine_WidthSlide, this.WidthSlide.Value); // set DB //
                this.McPref.OctopusWidth = this.WidthSlide.Value; // Notify MCT //
            };
        }

        #endregion
        
        public void SetGroupLayout()
        {
            this.SetEnableCheck();
            this.SetIsoCheck(); 
            this.SetCplChecked();
            this.SetWplChecked();
            this.SetColorCheck();
            this.SetColorPick();
            this.SetWidthCheck();
            this.SetWidthSlide();
            
            this.GroupBoxLayout = new DynamicLayout();
            IEnumerable<Control> predictionControls = new Control[]
            {
                this.EnableCheck,
                this.ISOCheck,
                this.CPLCheck,
                this.WPLCheck,
                this.ColorCheck,
                this.ColorPick,
                this.WidthCheck,
                this.WidthSlide
            };
            this.GroupBoxLayout.AddSeparateColumn(new Padding(10), 10, false, false, predictionControls);
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