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

        public override string EnglishName => "MultiCut";
        #endregion

        protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
        {
            Rhino.Input.Custom.GetObject getObject = new Rhino.Input.Custom.GetObject
            {
                GeometryFilter = Rhino.DocObjects.ObjectType.EdgeFilter
            };
            getObject.SetCommandPrompt("Pick a edge to start");
            getObject.Get();
            Rhino.DocObjects.ObjRef objRef = getObject.Object(0);
            Rhino.Geometry.BrepEdge edge = objRef.Edge();
            Rhino.Geometry.Brep brep = objRef.Brep();

            if (brep == null | edge == null)
            {
                RhinoApp.WriteLine("Brep is invalid");
                return Rhino.Commands.Result.Failure;
            }

            Rhino.Input.Custom.GetPoint getPoint = new Rhino.Input.Custom.GetPoint();
            getPoint.DynamicDraw += null;

            doc.Objects.Add(brep);


            doc.Views.Redraw();
            RhinoApp.WriteLine("The {0} command added one line to the document.", EnglishName);

            return Rhino.Commands.Result.Success;
        }
    }
}
