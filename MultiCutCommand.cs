using Rhino;

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
            
            GetFirstPoint getFirstPoint = new GetFirstPoint(core);
            getFirstPoint.SetCommandPrompt("Pick the first point");
            getFirstPoint.Get();
            if (getFirstPoint.CommandResult() != Rhino.Commands.Result.Success)
            {
                return getFirstPoint.CommandResult();
            }
            core.LastPt = getFirstPoint.Point();
            
            GetNextPoint getNextPoint = new GetNextPoint(core, 0);
            getNextPoint.SetCommandPrompt("Pick next point");
            getNextPoint.Get();
            if (getNextPoint.CommandResult() != Rhino.Commands.Result.Success)
            {
                return getNextPoint.CommandResult();
            }
            
            GetNextPoint getNextPoint2 = new GetNextPoint(core, 1);
            getNextPoint2.SetCommandPrompt("Pick next point");
            getNextPoint2.Get();
            if (getNextPoint2.CommandResult() != Rhino.Commands.Result.Success)
            {
                return getNextPoint2.CommandResult();
            }
            
            doc.Views.Redraw();
            return Rhino.Commands.Result.Success;
        }
    }
}
