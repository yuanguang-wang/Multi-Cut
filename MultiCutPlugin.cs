using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.PlugIns;

namespace MultiCut
{
    public class MultiCutPlugin : PlugIn
    {
        #region ATTR
        
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static MultiCutPlugin Instance { get; private set; }
        //private Core CoreObj { get; }
        
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
            PersistentSettings settingCollection = GetPluginSettings(this.Id, true);
            bool isSettingExist = settingCollection.TryGetBool("SplitOption", out bool value);
            if (!isSettingExist)
            {
                this.Settings.SetBool("SplitOption", false);
            }
            else
            {
                this.Settings.SetBool("SplitOption", value);
            }
            RhinoApp.WriteLine("Loaded");
            return base.OnLoad(ref errorMessage);
        }

        protected override void OnShutdown()
        {
            SavePluginSettings(this.Id);
            base.OnShutdown();
        }

        #endregion
        
        

        
        
    }
}