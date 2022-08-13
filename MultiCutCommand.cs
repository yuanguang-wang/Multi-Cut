using Rhino;
using Rhino.Commands;
using Rhino.Input;

namespace MultiCut
{
    public class MultiCutCommand : Command
    {
        #region ATTR
        public MultiCutCommand()
        {
            Instance = this;
        }
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static MultiCutCommand Instance { get; private set; }
        public override string EnglishName => "mct";
        
        #endregion

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Core coreObj = new Core(doc);
            MethodBasic.CurrentDoc = doc;
            if (coreObj.CollectionResult == false)
            {
                return Result.Cancel;
            }
            
            GetFirstPoint getFirstPoint = new GetFirstPoint(coreObj);
            GetResult rsFpt = getFirstPoint.Get();
            if (rsFpt != GetResult.Point) // !Mouse Down
            {
                if (rsFpt == GetResult.Nothing) // Press Enter
                {
                    coreObj.CutOperation();
                    doc.Views.Redraw();
                    return Result.Success;
                } 
                else // Press ESC
                {
                    RhinoApp.WriteLine("Command EXIT");
                    return Result.Cancel;
                }
            }
            
            while (true)
            {
                int ptCollectedCount = coreObj.OnLoopList.Count;
                if (ptCollectedCount >= 2)
                {
                    if (coreObj.OnLoopList.Contains(false))
                    {
                        break;
                    }
                }
                
                GetNextPoint getNextPoint = new GetNextPoint(coreObj);
                GetResult rsNextPt = getNextPoint.Get();
                if (rsNextPt != GetResult.Point) // !Mouse Down
                {
                    if (rsNextPt == GetResult.Nothing) // Press Enter
                    {
                        break;
                    } 
                    else // Press ESC
                    {
                        RhinoApp.WriteLine("Command EXIT");
                        return Result.Cancel;
                    }
                }

            }

            // not need to get but trigger ctor
            GetNextPoint _ = new GetNextPoint(coreObj);

            coreObj.CutOperation();
            
            doc.Views.Redraw();
            return Result.Success;
        }
    }

    public class PreferenceCommand : Command
    {
        #region ATTR
        public PreferenceCommand()
        {
            Instance = this;
        }
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static PreferenceCommand Instance { get; private set; }
        public override string EnglishName => "mcp";
        
        #endregion

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            MethodBasic.CurrentDoc = doc;
            PreferenceForm formObj = new PreferenceForm();
            
            formObj.Show();
            
            return Result.Success;
        }
    }
}
