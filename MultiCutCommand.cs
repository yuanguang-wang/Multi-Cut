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
            MultiCut.Core core = new MultiCut.Core(doc); 
            MultiCut.EventModerator em = new MultiCut.EventModerator { CoreObj = core };
            bool result = core.ObjectCollecter("Select the brep to be cut.");

            

            Rhino.Input.Custom.GetPoint getFirstPoint = new Rhino.Input.Custom.GetPoint();
            getFirstPoint.SetCommandPrompt("Pick the first point");
            getFirstPoint.MouseMove += em.Test;
            getFirstPoint.DynamicDraw += em.CutByOnePtEventMod;
            getFirstPoint.Get();

            

            if (getFirstPoint.CommandResult() != Rhino.Commands.Result.Success)
            {
                return getFirstPoint.CommandResult();
            }  

            doc.Views.Redraw();
            RhinoApp.WriteLine("The {0} command added one line to the document.", EnglishName);

            return Rhino.Commands.Result.Success;
        }
    }
}
