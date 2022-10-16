using System;
using System.Collections.Generic;

using Eto.Drawing;
using Eto.Forms;

using Rhino.UI;
using Rhino;
using Rhino.Runtime;

// ReSharper disable VirtualMemberCallInConstructor

namespace MultiCut
{
    internal static class SettingKey
    {
        public const string General_SplitCheck = "GeneralSplitCheck";
        public const string PredictionLine_EnableCheck = "PredictionLineEnabledCheck";
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
        public const string AssistantPoint_EnableCheck = "AssistantPointEnableCheck";
        public const string AssistantPoint_PointNumber = "AssistantPointNumber";
        public const string AssistantPoint_ColorCheck = "AssistantPointColorCheck";
        public const string AssistantPoint_ColorPick = "AssistantPointColorPick";
        public const string AssistantPoint_SizeCheck = "AssistantPointSizeCheck";
        public const string AssistantPoint_SizePick = "AssistantPointSizePick";
    }
    
    public class MultiCutPreference 
    {
        #region ATTR
        
        public readonly Color defaultProphetColor = Colors.LimeGreen;
        public readonly Color defaultOctopusColor = Colors.Blue;
        public readonly Color defaultPointColor = Colors.Red;
        public int defaultPointSize => (int) MethodCollection.CurrentDoc.Views.ActiveView.DisplayPipeline.DisplayPipelineAttributes.PointRadius;
        public int defaultLineWidth => MethodCollection.CurrentDoc.Views.ActiveView.DisplayPipeline.DisplayPipelineAttributes.CurveThickness;
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
        public bool IsPointEnabled { get; set; }
        public int PointNumber { get; set; }
        public System.Drawing.Color PointColor { get; set; }
        public int PointSize { get; set; }


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
            
            this.IsProphetChecked = this.LoadBoolSetting(SettingKey.PredictionLine_EnableCheck, PlugInSettings, true);
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
            
            this.IsPointEnabled = this.LoadBoolSetting(SettingKey.AssistantPoint_EnableCheck, PlugInSettings, false);
            this.PointNumber = this.LoadIntSetting(SettingKey.AssistantPoint_PointNumber, PlugInSettings, 3);
            this.LoadBoolSetting(SettingKey.AssistantPoint_ColorCheck, PlugInSettings, false);
            this.PointColor = this.LoadColorSetting(SettingKey.AssistantPoint_ColorPick, PlugInSettings,
                this.defaultPointColor.ToSystemDrawing());
            this.LoadBoolSetting(SettingKey.AssistantPoint_SizeCheck, PlugInSettings, false);
            this.PointSize = this.LoadIntSetting(SettingKey.AssistantPoint_SizePick, PlugInSettings, 2);
        }
        
        #endregion
    }

    public class PreferenceForm : Form
    {
        private GeneralBox General => GeneralBox.Instance;
        private AboutBox About => AboutBox.Instance;
        private PredictionLineBox PredictionLine => PredictionLineBox.Instance;
        private OctopusLineBox OctopusLine => OctopusLineBox.Instance;
        private AssistantPointBox AssistantPoint => AssistantPointBox.Instance;
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
            
            IEnumerable<GroupBox> controls_1 = new GroupBox[] { this.General, this.About, this.OctopusLine };
            IEnumerable<GroupBox> controls_2 = new GroupBox[] { this.AssistantPoint, };//this.PredictionLine };
            
            this.PreferenceLayout.BeginHorizontal();
            this.PreferenceLayout.AddSeparateColumn(new Padding(10,10,5,10), 10, false, false, null);
            this.PreferenceLayout.AddSeparateColumn(new Padding(5,10,10,10), 10, false, false, controls_2);
            this.PreferenceLayout.EndHorizontal();

        }
        

        protected override void OnLoad(EventArgs e)
        {
            this.Location = new Point(Mouse.Position);
            base.OnLoad(e);
        }

    }

    public class GeneralBox : GroupBox, IGroupCommon
    {
        public RhinoDoc CurrentDoc => MethodCollection.CurrentDoc;
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
        private LinkButton GithubLink { get; set; }
        private LinkButton WebsiteLink { get; set; }

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
            // Git Link //
            this.GithubLink = new LinkButton();
            this.GithubLink.Text = "Documentation on Github";
            this.GithubLink.Click += (sender, args) =>
            {
                PythonScript ps = PythonScript.Create();
                ps.ExecuteScript("import webbrowser; webbrowser.open('https://github.com/yuanguang-wang/Multi-Cut')");
            };
            
            // Web Link //
            this.WebsiteLink = new LinkButton();
            this.WebsiteLink.Text = "Find more Utilities";
            this.WebsiteLink.Click += (sender, args) =>
            {
                PythonScript ps = PythonScript.Create();
                ps.ExecuteScript("import webbrowser; webbrowser.open('https://elderaven.com')");
            }; 
        }
        private void SetGroupLayout()
        {
            this.AboutBoxLayout = new DynamicLayout();
            IEnumerable<Control> controls = new Control[]
            {
                this.GithubLink,
                this.WebsiteLink
            };
            this.AboutBoxLayout.AddSeparateColumn(AboutBoxLayout.DefaultPadding,13, false, false, controls);
        }
    }

    public class PredictionLineBox : GroupBox, IGroupCommon
    {
        public RhinoDoc CurrentDoc => MethodCollection.CurrentDoc;
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
            bool isChecked = this.McPlugin.Settings.GetBool(SettingKey.PredictionLine_EnableCheck);
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
            this.McPlugin.Settings.SetBool(SettingKey.PredictionLine_EnableCheck, isChecked);
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
            bool doubleCheck = MethodCollection.DoubleCheck(this.EnableCheck.Checked, 
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
            int slideNumChanged = MethodCollection.RemapInt(slideNum);
            this.WidthSlide.Value = slideNumChanged;
        }
        private void OnWidthSlideEnabled(object sender, EventArgs e)
        {
            bool doubleCheck = MethodCollection.DoubleCheck(this.EnableCheck.Checked, this.WidthCheck.Checked);
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
            int slideNumRemap = MethodCollection.RemapInt(slideNum);
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
        public RhinoDoc CurrentDoc => MethodCollection.CurrentDoc;
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
                McPlugin.Settings.SetBool(SettingKey.OctopusLine_EnableCheck, MethodCollection.SafeCast(EnableCheck.Checked)); // set DB //
                McPref.IsOctopusChecked = MethodCollection.SafeCast(EnableCheck.Checked); // Notify MCT //
            };
            this.EnableCheck.CheckedChanged += this.EnableSubOptions;
        }
        private void EnableSubOptions(object sender, EventArgs e)
        {
            bool value = MethodCollection.SafeCast(this.EnableCheck.Checked);
            this.ISOCheck.Enabled = value;
            this.WPLCheck.Enabled = value;
            this.CPLCheck.Enabled = value;
            this.ColorCheck.Enabled = value;
            this.WidthCheck.Enabled = value;
            bool isColorPickEnabled = MethodCollection.DoubleCheck(value, this.ColorCheck.Checked);
            this.ColorPick.Enabled = isColorPickEnabled;
            bool isWidthSlideEnabled = MethodCollection.DoubleCheck(value, this.WidthCheck.Checked);
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
                bool value = MethodCollection.SafeCast(this.ISOCheck.Checked);
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
               bool value = MethodCollection.SafeCast(this.CPLCheck.Checked);
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
                bool value = MethodCollection.SafeCast(this.WPLCheck.Checked);
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
                bool value = MethodCollection.SafeCast(this.ColorCheck.Checked);
                this.McPlugin.Settings.SetBool(SettingKey.OctopusLine_ColorCheck, value); // set DB //
               
                // Control the behavior of ColorPick //
                bool doubleValue = MethodCollection.DoubleCheck(value, this.EnableCheck.Checked);
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
                bool value = MethodCollection.SafeCast(this.WidthCheck.Checked);
                this.McPlugin.Settings.SetBool(SettingKey.OctopusLine_WidthCheck, value); // set DB //
                
                // Control Behavior //
                bool doubleCheck = MethodCollection.DoubleCheck(value, this.EnableCheck.Checked);
                this.WidthSlide.Enabled = doubleCheck;
                this.McPref.OctopusWidth = doubleCheck ? this.WidthSlide.Value : this.DisplayThickness;
            };
        }
        private int DisplayThickness => MethodCollection.CurrentDoc.Views.ActiveView.DisplayPipeline.DisplayPipelineAttributes.CurveThickness;
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
    
    public class AssistantPointBox : GroupBox, IGroupCommon
    {
        public DynamicLayout GroupBoxLayout { get; set; }
        public static AssistantPointBox Instance { get; } = new AssistantPointBox();
        private AssistantPointBox()
        {
            this.Text = "Assistant Point";
            
            this.SetGroupLayout();
            this.Content = this.GroupBoxLayout;
        }
        public void SetGroupLayout()
        {
            this.GroupBoxLayout = new DynamicLayout();
            IEnumerable<Control> controls = new Control[]
            {
                APEnableCheck.Instance, 
                APDropDown.Instance,
                APColorCheck.Instance, 
                APColorPicker.Instance,
                APWidthCheck.Instance,
            };
            this.GroupBoxLayout.AddSeparateColumn(new Padding(10), 10, false, false, controls);
        }
    }

    #region MCP Control Templates 
    
    public abstract class EnableCheckBox : CheckBox
    {
        protected abstract string Key { get; }
        protected abstract Control[] ControlArray { get; }
        protected abstract CheckBox ColorCheck { get; }
        protected abstract ColorPicker ColorPick { get; }
        protected abstract CheckBox WidthCheck { get; }
        protected abstract Slider SizeSlide { get; }
        private MultiCutPlugin McPlugin => MultiCutPlugin.Instance;
        protected MultiCutPreference McPref => MultiCutPreference.Instance;

        protected EnableCheckBox()
        {
            this.ThreeState = false;
            this.Text = "Enable";
        }

        private void EnableSubCheckBox()
        {
            bool value = MethodCollection.SafeCast(this.Checked);
            this.ColorCheck.Enabled = value;
            this.WidthCheck.Enabled = value;
            if (this.ControlArray != null)
            {
                foreach (Control control in ControlArray)
                {
                    control.Enabled = value;
                } 
            }
        }

        protected virtual void EnableSubSettings()
        {
            // UI Setting //
            this.ColorPick.Enabled = MethodCollection.DoubleCheck(this.Checked, this.ColorCheck.Checked);
            this.SizeSlide.Enabled = MethodCollection.DoubleCheck(this.Checked, this.WidthCheck.Checked);
        }

        protected override void OnLoad(EventArgs e)
        {
            this.Checked = McPlugin.Settings.GetBool(this.Key); // get DB //
            this.EnableSubCheckBox(); // Enable Sub Setting //
            base.OnLoad(e);
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            bool value = MethodCollection.SafeCast(this.Checked);
            McPlugin.Settings.SetBool(this.Key, value); // set DB //
            McPlugin.SaveSettings();
            this.EnableSubCheckBox(); // Enable Sub Setting //
            base.OnCheckedChanged(e);
        }
    }
     public abstract class CommonCheckBox : CheckBox
    {
        protected abstract string Key { get; }
        private MultiCutPlugin McPlugin => MultiCutPlugin.Instance;
        protected MultiCutPreference McPref => MultiCutPreference.Instance;

        protected CommonCheckBox()
        {
            this.ThreeState = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            this.Checked = McPlugin.Settings.GetBool(this.Key); // get DB //
            base.OnLoad(e);
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            McPlugin.Settings.SetBool(this.Key, MethodCollection.SafeCast(this.Checked)); // set DB //
            McPlugin.SaveSettings();
            base.OnCheckedChanged(e);
        }
    }

     public abstract class CommonDropDown : DropDown
     {
         protected abstract string Key { get; }
         private MultiCutPlugin McPlugin => MultiCutPlugin.Instance;
         protected MultiCutPreference McPref => MultiCutPreference.Instance;

         protected CommonDropDown()
         { 
             IEnumerable<object> PointNumbers = new List<object>() {2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20};
             this.DataStore = PointNumbers;
         }

         protected override void OnLoad(EventArgs e)
         {
             this.SelectedIndex = McPlugin.Settings.GetInteger(this.Key); // get DB //
             base.OnLoad(e);
         }

         protected override void OnSelectedIndexChanged(EventArgs e)
         {
             McPlugin.Settings.SetInteger(this.Key,this.SelectedIndex); // set DB //
             McPlugin.SaveSettings();
             base.OnSelectedIndexChanged(e);
         }
     }

     public abstract class ColorCheckBox : CheckBox
    {
        protected abstract string Key { get; }
        protected abstract CheckBox UpperCheck { get; }
        protected abstract ColorPicker ColorPick { get; }
        protected abstract string ColorKey { get; }
        protected abstract Color DefaultColor { get; }
        protected MultiCutPlugin McPlugin => MultiCutPlugin.Instance;
        protected MultiCutPreference McPref => MultiCutPreference.Instance;

        protected ColorCheckBox()
        { 
            this.ThreeState = false;
            this.Text = "Customize Color";
        }

        protected virtual void EnableSubSetting()
        {
            // UI Setting //
            this.ColorPick.Enabled = MethodCollection.DoubleCheck(this.Checked, this.UpperCheck.Checked);
            
        }

        protected override void OnLoad(EventArgs e)
        {
            this.Checked = McPlugin.Settings.GetBool(this.Key); // get DB //
            base.OnLoad(e);
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            bool value = MethodCollection.SafeCast(this.Checked);
            McPlugin.Settings.SetBool(this.Key, value); // set DB //
            McPlugin.SaveSettings();
            base.OnCheckedChanged(e);
        }
    }
    
    public abstract class WidthCheckBox : CheckBox
    {
        protected abstract string Key { get; }
        protected abstract CheckBox UpperCheck { get; }
        protected abstract Slider WidthSlider { get; }
        protected abstract string WidthKey { get; }
        protected abstract int DefaultWidth { get; }
        protected MultiCutPlugin McPlugin => MultiCutPlugin.Instance;
        protected MultiCutPreference McPref => MultiCutPreference.Instance;

        protected WidthCheckBox()
        {
            this.ThreeState = false;
            RhinoApp.WriteLine(WidthSlider.Value + ": base WidthCheckBox.ctor(), UI");
            RhinoApp.WriteLine(McPlugin.Settings.GetInteger(this.WidthKey) + ": base WidthCheckBox.ctor(), DB");
        }

        protected virtual void EnableSubSetting()
        {  
            // UI Setting //
            RhinoApp.WriteLine(McPlugin.Settings.GetInteger(this.WidthKey) + ": base WidthCheckBox.EnableSubSetting(), DB");
            RhinoApp.WriteLine(this.WidthSlider.Value + ": base WidthCheckBox.EnableSubSetting(), UI");
            this.WidthSlider.Enabled = MethodCollection.DoubleCheck(this.Checked, this.UpperCheck.Checked);
            this.WidthSlider.Value = MethodCollection.SafeCast(this.Checked)
                ? McPlugin.Settings.GetInteger(this.WidthKey)
                : this.DefaultWidth;
        }
        
        protected override void OnLoad(EventArgs e)
        {
            this.Checked = McPlugin.Settings.GetBool(this.Key); // get DB //
            base.OnLoad(e);
        }
        
        protected override void OnCheckedChanged(EventArgs e)
        {
            McPlugin.Settings.SetBool(this.Key, MethodCollection.SafeCast(this.Checked)); // set DB //
            McPlugin.SaveSettings();
            base.OnCheckedChanged(e);
        }
    }
    
    public abstract class CommonColorPicker : ColorPicker
    {
        protected abstract string Key { get; }
        protected abstract Color DefaultColor { get; }
        protected abstract CheckBox UpperCheck { get; }
        private MultiCutPlugin McPlugin => MultiCutPlugin.Instance;
        protected MultiCutPreference McPref => MultiCutPreference.Instance;

        protected override void OnLoad(EventArgs e)
        { 
            // get DB //
            this.Value = MethodCollection.SafeCast(UpperCheck.Checked)
                ? McPlugin.Settings.GetColor(this.Key).ToEto()
                : this.DefaultColor;
            
            base.OnLoad(e);
        }

        protected override void OnColorChanged(EventArgs e)
        {
            if (this.Value != this.DefaultColor)
            {
                McPlugin.Settings.SetColor(this.Key, this.Value.ToSystemDrawing()); // set DB //
                McPlugin.SaveSettings();
            }
            base.OnColorChanged(e);
        }
    }

    public abstract class CommonWidthSlider : Slider
    {
        protected abstract string Key { get; }
        protected abstract int DefaultWidth { get; }
        protected MultiCutPlugin McPlugin => MultiCutPlugin.Instance;
        protected MultiCutPreference McPref => MultiCutPreference.Instance;

        protected CommonWidthSlider()
        {
            this.MaxValue = 9;
            //this.MinValue = 1;
            this.TickFrequency = 1;
            this.SnapToTick = true;
            this.MinValue = McPlugin.Settings.GetInteger(this.Key);
            RhinoApp.WriteLine(McPlugin.Settings.GetInteger(this.Key) + ": base Slider.ctor(), DB");
            RhinoApp.WriteLine(this.Value + ": base Slider.ctor(), UI");
        }

        protected override void OnValueChanged(EventArgs e)
        {
            if (this.Value != this.DefaultWidth)
            {
                RhinoApp.WriteLine(this.Value + ": base Slider.OnValueChanged(e), UI");
                RhinoApp.WriteLine(McPlugin.Settings.GetInteger(this.Key) + ": base Slider.OnValueChanged(e), DB");
                McPlugin.Settings.SetInteger(this.Key, this.Value); // set DB //
                McPlugin.SaveSettings();
            }
            base.OnValueChanged(e);
        }
    }
    
    #endregion
    #region Prediction Line UI Group
    
    public sealed class PLEnableCheck : EnableCheckBox
    {
        protected override string Key => SettingKey.PredictionLine_EnableCheck;
        protected override Control[] ControlArray => null;
        protected override CheckBox ColorCheck => null;
        protected override ColorPicker ColorPick => null;
        protected override CheckBox WidthCheck => null;
        protected override Slider SizeSlide => null;

    }

    #endregion
    #region Assitant Point UI Group
    
    public sealed class APEnableCheck : EnableCheckBox
    {
        protected override string Key => SettingKey.AssistantPoint_EnableCheck;
        protected override Control[] ControlArray => new Control[]{APDropDown.Instance};
        protected override CheckBox ColorCheck => APColorCheck.Instance;
        protected override ColorPicker ColorPick => APColorPicker.Instance;
        protected override CheckBox WidthCheck => APWidthCheck.Instance;
        protected override Slider SizeSlide => null;
        public static APEnableCheck Instance { get; } = new APEnableCheck();
        private APEnableCheck(){}

        protected override void EnableSubSettings()
        {
            bool value = MethodCollection.SafeCast(this.Checked);
            // MCT Setting //
            McPref.IsPointEnabled = value;
            
            base.EnableSubSettings();
        }

        protected override void OnLoad(EventArgs e)
        {
            this.EnableSubSettings();
            base.OnLoad(e);
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            this.EnableSubSettings();
            base.OnCheckedChanged(e);
        }
    }

    public sealed class APDropDown : CommonDropDown
    {
        protected override string Key => SettingKey.AssistantPoint_PointNumber;
        public static APDropDown Instance { get; } = new APDropDown();
        private APDropDown() { }
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            McPref.PointNumber = this.SelectedIndex + 2;
            base.OnSelectedIndexChanged(e);
        }
    }

    public sealed class APColorCheck : ColorCheckBox
    {
        protected override string Key => SettingKey.AssistantPoint_ColorCheck;
        protected override CheckBox UpperCheck => APEnableCheck.Instance;
        protected override ColorPicker ColorPick => APColorPicker.Instance;
        protected override string ColorKey => SettingKey.AssistantPoint_ColorPick;
        protected override Color DefaultColor => McPref.defaultPointColor;
        public static APColorCheck Instance { get; } = new APColorCheck();
        private APColorCheck() { }
        
        protected override void EnableSubSetting()
        {
            // MCT Setting //
            McPref.PointColor = MethodCollection.SetMCTUIColor(
                this.Checked,
                this.UpperCheck.Checked,
                McPlugin.Settings.GetColor(this.ColorKey),
                this.DefaultColor.ToSystemDrawing()
            );
            this.ColorPick.Value = McPref.PointColor.ToEto();
            
            base.EnableSubSetting();
        }

        protected override void OnLoad(EventArgs e)
        {
            this.EnableSubSetting();
            base.OnLoad(e);
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            this.EnableSubSetting();
            base.OnCheckedChanged(e);
        }
    }

    
    public sealed class APColorPicker : CommonColorPicker
    {
        protected override string Key => SettingKey.AssistantPoint_ColorPick;
        protected override Color DefaultColor => McPref.defaultPointColor;
        protected override CheckBox UpperCheck => APColorCheck.Instance;
        public static APColorPicker Instance { get; } = new APColorPicker();

        private APColorPicker()
        {
            this.ValueChanged += (sender, args) =>
            {
                McPref.PointColor = this.Value.ToSystemDrawing();
            };
        }
    }
    
    public sealed class APWidthCheck : WidthCheckBox
    {
        protected override string Key => SettingKey.AssistantPoint_SizeCheck;
        protected override CheckBox UpperCheck => APEnableCheck.Instance;
        protected override Slider WidthSlider => null;
        protected override string WidthKey => SettingKey.AssistantPoint_SizePick;
        protected override int DefaultWidth => McPref.defaultPointSize;
        public static APWidthCheck Instance { get; } = new APWidthCheck();

        private APWidthCheck()
        {
            this.Text = "Customize PointSize";
            RhinoApp.WriteLine(this.WidthSlider.Value + ": this Check.ctor(), UI");
            RhinoApp.WriteLine(McPlugin.Settings.GetInteger(this.WidthKey) + ": this Check.ctor(), DB");
        }

        protected override void EnableSubSetting()
        {
            // MCT Setting //
            base.EnableSubSetting();
            McPref.PointSize = MethodCollection.SafeCast(this.Checked)
                ? McPlugin.Settings.GetInteger(this.WidthKey)
                : this.DefaultWidth;
            RhinoApp.WriteLine(this.WidthSlider.Value.ToString() + ": this Check.EnableSubSetting(), UI");
            RhinoApp.WriteLine(McPlugin.Settings.GetInteger(this.WidthKey).ToString() + ": this Check.EnableSubSetting(), DB");
        }

        protected override void OnLoad(EventArgs e)
        {
            this.EnableSubSetting();
            RhinoApp.WriteLine("this Check.OnLoad");
            base.OnLoad(e);
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            this.EnableSubSetting();
            RhinoApp.WriteLine("this Check.OnValueChanged");
            base.OnCheckedChanged(e);
        }
    }

 
    
    #endregion

    public interface IGroupCommon
    {
        DynamicLayout GroupBoxLayout { get; set; }
        void SetGroupLayout();
    }

}   