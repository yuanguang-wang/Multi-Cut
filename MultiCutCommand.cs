using System.Linq;
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
            getFirstPoint.Get();
            if (getFirstPoint.CommandResult() != Rhino.Commands.Result.Success)
            {
                return getFirstPoint.CommandResult();
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
                getNextPoint.Get();
                if (getNextPoint.CommandResult() != Rhino.Commands.Result.Success)
                {
                    return getNextPoint.CommandResult();
                }
                core.LastPtCandidate = getNextPoint.Point();

            }
            
            core.CutbyOctopus();
            
            doc.Views.Redraw();
            return Rhino.Commands.Result.Success;
        }
    }
}
