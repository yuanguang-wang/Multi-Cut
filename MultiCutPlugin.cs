using Rhino.PlugIns;

namespace MultiCut
{
    public class MultiCutPlugin : PlugIn
    {
        #region ATTR
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static MultiCutPlugin Instance { get; private set; }
        private MultiCutPreference McPref => MultiCutPreference.Instance;

        #endregion
        #region CTOR
        public MultiCutPlugin()
        {
            Instance = this;
        }
        #endregion
        #region MTHD

        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            this.McPref.LoadSettingBundle();
            return base.OnLoad(ref errorMessage);
        }

        #endregion
    }
}