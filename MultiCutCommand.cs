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
            if (coreObj.CollectionResult == false)
            {
                return Result.Cancel;
            }
            
            GetFirstPoint getFirstPoint = new GetFirstPoint(coreObj);
            
            while (true)
            {
                GetResult rsFpt = getFirstPoint.Get();

                if (rsFpt == GetResult.Nothing) // Press Enter
                {
                    coreObj.CutOperation();
                    doc.Views.Redraw();
                    return Result.Success;
                }
                if (rsFpt == GetResult.Point)
                {
                    break;
                }
                if (rsFpt == GetResult.Option)
                {
                    MethodCollection.SyncCommandOpt(getFirstPoint);
                }
                else // Press ESC
                {
                    RhinoApp.WriteLine("Command EXIT");
                    return Result.Cancel;
                }
            }

            bool breakLoop = true;
            while (breakLoop)
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
                while (true)
                {
                    GetResult rsNextPt = getNextPoint.Get();
                    if (rsNextPt == GetResult.Point) 
                    {
                        break;
                    }
                    if (rsNextPt == GetResult.Nothing) // Press Enter
                    {
                        breakLoop = false;
                        break;
                    }
                    if (rsNextPt == GetResult.Option)
                    {
                        MethodCollection.SyncCommandOpt(getNextPoint);
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
        public PreferenceForm FormObj { get; set; }

        #endregion

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            if (this.FormObj == null)
            {
                this.FormObj = new PreferenceForm();
                this.FormObj.Closed += (sender, args) =>
                {
                    this.FormObj.Dispose();
                    this.FormObj = null;
                };
                this.FormObj.Show();
            }
            else
            {
                RhinoApp.WriteLine("Multi-Cut Preference Window has already opened.");
            }

            return Result.Success;
        }
    }
}
