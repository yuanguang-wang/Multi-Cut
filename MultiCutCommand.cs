using Rhino;
using System.Collections.Generic;

namespace MultiCut
{
    public class MultiCutCommand : Rhino.Commands.Command
    {
        #region ATTR
        public MultiCutCommand()
        {
            Instance = this;
        }

        public static MultiCutCommand Instance { get; private set; }

        public override string EnglishName => "mct";
        #endregion

        protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
        {
            Core core = new Core(doc);
            GetPointTemplate getFirstPoint = new GetPointTemplate(core);
            getFirstPoint.SetCommandPrompt("Pick the first point");
            getFirstPoint.Get();
            if (getFirstPoint.CommandResult() != Rhino.Commands.Result.Success)
            {
                return getFirstPoint.CommandResult();
            }  
            doc.Views.Redraw();
            return Rhino.Commands.Result.Success;
        }
    }
}
