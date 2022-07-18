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

            while ((from bool boolean in core.OnLoopList where boolean == false select boolean).Count() < 2)
            {
                GetNextPoint getNextPoint = new GetNextPoint(core);
                getNextPoint.Get();
                if (getNextPoint.CommandResult() != Rhino.Commands.Result.Success)
                {
                    return getNextPoint.CommandResult();
                }
                core.LastPtCandidate = getNextPoint.Point();
            }
            
            


            
            doc.Views.Redraw();
            return Rhino.Commands.Result.Success;
        }
    }
}
