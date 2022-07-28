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

        public bool IsProphetEnabled { get; set; }
        public bool IsPriorityEnabled { get; set; }


        #endregion
        #region CTOR

        public static MultiCutPreference Instance { get; } = new MultiCutPreference();
        private MultiCutPreference() { }

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
            this.SplitCheck = new CheckBox(){Text = "Split if possible", ThreeState = false};
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

    public class PredictionLineBox : GroupBox, IGroupCommon
    {
        public MultiCutPreference McPref => MultiCutPreference.Instance;
        public MultiCutPlugin McPlugin => MultiCutPlugin.Instance;
        public DynamicLayout GroupLayout { get; set; }
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

            this.Content = this.GroupLayout;
        }

        #region Enable

        private CheckBox EnableCheck { get; set; }
        private void SetEnableCheck()
        {
            this.EnableCheck = new CheckBox(){Text = "Enable", ThreeState = false };
            this.EnableCheck.Load += OnEnableCheck;
            this.EnableCheck.CheckedChanged += OnEnableCheck;
        }
        private void OnEnableCheck(object sender, EventArgs e)
        {
            // ReSharper disable once PossibleInvalidOperationException
            bool isAllChecked = (bool)this.EnableCheck.Checked;
            this.PriorityCheck.Enabled = isAllChecked;
            this.ColorCheck.Enabled = isAllChecked;
            this.WidthCheck.Enabled = isAllChecked;
            
            this.McPref.IsProphetEnabled = isAllChecked;
            
            this.OnColorChecked(sender, e);
            this.OnWidthChecked(sender, e);
            
        }

        #endregion
        #region Priority

        private CheckBox PriorityCheck { get; set; }
        private void SetPriorityCheck()
        {
            this.PriorityCheck = new CheckBox(){Text = "Prioritize", ThreeState = false };
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
            this.ColorCheck = new CheckBox(){Text = "Customize color", ThreeState = false };
            this.ColorCheck.Load += OnColorChecked;
            this.ColorCheck.CheckedChanged += OnColorChecked;
        }
        private void OnColorChecked(object sender, EventArgs e)
        {
            // ReSharper disable once PossibleInvalidOperationException
            bool isSubChecked = (bool)this.ColorCheck.Checked;
            // ReSharper disable once PossibleInvalidOperationException
            bool isAllChecked = (bool)this.EnableCheck.Checked;
            this.ColorPick.Enabled = isSubChecked & isAllChecked;
            if(isSubChecked & isAllChecked)
            {
                this.ColorPick.Value = Colors.LimeGreen;
            }
            else
            {
                this.ColorPick.Value = Colors.LightSlateGray;
            }
            
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
            // ReSharper disable once PossibleInvalidOperationException
            bool isSubChecked = (bool)this.WidthCheck.Checked;
            // ReSharper disable once PossibleInvalidOperationException
            bool isAllChecked = (bool)this.EnableCheck.Checked;
            this.WidthSlide.Enabled = isSubChecked & isAllChecked;
            this.SlideNumber.Visible = isSubChecked & isAllChecked;
            if (isSubChecked & isAllChecked)
            {
                Rhino.RhinoApp.WriteLine("enabled");
                this.SlideNumber.TextColor = Eto.Drawing.Color.FromArgb(0, 0, 0, 255);
            }
            else
            {
                Rhino.RhinoApp.WriteLine("disabled");
                Eto.Drawing.Color grey = Eto.Drawing.Color.FromArgb(128, 128, 128, 255);
                this.SlideNumber.TextColor = grey;
            }
            
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
        private Label SlideNumber => new Label() { Text = " 1   2   3   4   5   6   7   8   9"};

        #endregion
        
        

        public void SetLayout()
        {
            this.GroupLayout = new DynamicLayout();
            IEnumerable<Control> predictionControls = new Control[]
            {
                this.EnableCheck,
                this.PriorityCheck,
                this.ColorCheck,
                this.ColorPick,
                this.WidthCheck,
                this.WidthSlide,
                this.SlideNumber
            };
            this.GroupLayout.AddSeparateColumn(new Padding(10), 10, false, false, predictionControls);
        }
    }

    public interface IGroupCommon
    {
        MultiCutPreference McPref { get; }
        MultiCutPlugin McPlugin { get; }
        DynamicLayout GroupLayout { get; set; }
        void SetLayout();
    }

}