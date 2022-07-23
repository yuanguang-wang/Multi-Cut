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
            RhinoApp.WriteLine("Loaded");
            return base.OnLoad(ref errorMessage);
        }

        #endregion
        
        

        
        
    }
}