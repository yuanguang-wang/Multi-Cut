using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
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
        private TryGetSetting<bool> TryGetBool { get; set; }

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
            

            return base.OnLoad(ref errorMessage);
        }

        private void GetPlugInSetting<T>(string keyword, PersistentSettings plugInSettingCollection, T defaultValue)
        {
            if (defaultValue is bool isBoolValue)
            {
                TryGetSetting<bool> tryGetSetting = plugInSettingCollection.TryGetBool;
            }
            else if (defaultValue is int isIntValue)
            {
                TryGetSetting<int> tryGetSetting = plugInSettingCollection.TryGetInteger;
            }
            else if (defaultValue is Color isColorValue)
            {
                TryGetSetting<Color> tryGetSetting = plugInSettingCollection.TryGetColor;
            }
            
            this.TryGetBool = plugInSettingCollection.TryGetBool;
            
            
            
        }

        #endregion
        
        

        
        
    }
}