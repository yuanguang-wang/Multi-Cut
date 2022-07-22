using Rhino;
using Rhino.Input;

namespace MultiCut
{
    public class MultiCutCommand : Rhino.Commands.Command
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

        protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
        {
            Core core = new Core(doc);
            if (core.CollectionResult == false)
            {
                return Rhino.Commands.Result.Cancel;
            }
            
            GetFirstPoint getFirstPoint = new GetFirstPoint(core);
            GetResult rsFpt = getFirstPoint.Get();
            if (rsFpt != GetResult.Point) // !Mouse Down
            {
                if (rsFpt == GetResult.Nothing) // Press Enter
                {
                    core.CutOperation();
                    doc.Views.Redraw();
                    return Rhino.Commands.Result.Success;
                } 
                else // Press ESC
                {
                    RhinoApp.WriteLine("Command Exit");
                    return Rhino.Commands.Result.Cancel;
                }
            }
            
            core.LastPtCandidate = getFirstPoint.Point();
            while (true)
            {
                int ptCollectedCount = core.OnLoopList.Count;
                if (ptCollectedCount >= 2)
                {
                    if (core.OnLoopList.Contains(false))
                    {
                        break;
                    }
                }
                
                GetNextPoint getNextPoint = new GetNextPoint(core);
                GetResult rsNpt = getNextPoint.Get();
                if (rsNpt != GetResult.Point) // !Mouse Down
                {
                    if (rsNpt == GetResult.Nothing) // Press Enter
                    {
                        break;
                    } 
                    else // Press ESC
                    {
                        RhinoApp.WriteLine("Command Exit");
                        return Rhino.Commands.Result.Cancel;
                    }
                }
                core.LastPtCandidate = getNextPoint.Point();

            }
            core.CutOperation();
            
            doc.Views.Redraw();
            return Rhino.Commands.Result.Success;
        }
    }
}
