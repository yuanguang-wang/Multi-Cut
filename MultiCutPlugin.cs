using System.Collections;
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
            
            bool isSettingExist = settingCollection.TryGetBool("SplitCheck", out bool value);
            if (!isSettingExist)
            {
                this.Settings.SetBool("SplitCheck", false);
            }
            else
            {
                this.Settings.SetBool("SplitCheck", value);
            }

            ICollection<string> keyCollection = settingCollection.Keys;
            foreach (string key in keyCollection)
            {
                
            }


            return base.OnLoad(ref errorMessage);
        }

        #endregion
        
        

        
        
    }
}