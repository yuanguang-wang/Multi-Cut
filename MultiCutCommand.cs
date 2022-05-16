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
            MultiCut.Core core = new MultiCut.Core(); 
            bool result = core.ObjectCollecter("Select the brep to be cut.");

            

            Rhino.Input.Custom.GetPoint getFirstPoint = new Rhino.Input.Custom.GetPoint();
            getFirstPoint.SetCommandPrompt("Pick the first point");
            getFirstPoint.DynamicDraw += null;
            //getFirstPoint.Tag = obj;
            getFirstPoint.Get();

            if (getFirstPoint.CommandResult() != Rhino.Commands.Result.Success)
            {
                return getFirstPoint.CommandResult();
            }  
            
            Rhino.Geometry.Point3d firstPoint = getFirstPoint.Point();
            Rhino.Geometry.BrepEdge startEdge;
            Rhino.Geometry.Vector3d planeZAxis;
            Rhino.Geometry.Plane plane = doc.Views.ActiveView.ActiveViewport.ConstructionPlane();

            bool astralstep = true;
            foreach (Rhino.Geometry.BrepEdge bEdge in bEdges_List)
            {
                bEdge.ClosestPoint(firstPoint, out double t);
                Rhino.Geometry.Point3d projectedPoint = bEdge.PointAt(t);
                double distance = projectedPoint.DistanceTo(firstPoint);
                if (distance <= doc.ModelAbsoluteTolerance)
                {
                    astralstep = false;
                    startEdge = bEdge;
                    planeZAxis = bEdge.TangentAt(t);
                    plane = new Rhino.Geometry.Plane(projectedPoint, planeZAxis);
                    break;
                }
            }

            if (astralstep)
            {
                RhinoApp.WriteLine("[Placeholder] astral step");
            }
            else
            {
                RhinoApp.WriteLine("on the edge");                
            }


            doc.Views.Redraw();
            RhinoApp.WriteLine("The {0} command added one line to the document.", EnglishName);

            return Rhino.Commands.Result.Success;
        }
    }
}
