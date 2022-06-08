using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Display;
using Rhino.Input.Custom;

namespace MultiCut
{
    internal class MethodAssembly
    {
        // delegate placeholder
    }

    internal static class MethodBasic
    {
        public static bool ObjectCollecter(string commandPrompt, 
                                           out Rhino.Geometry.Brep brep2BPassed, 
                                           out Rhino.Geometry.Collections.BrepEdgeList bEdgeList2Bpassed)
        {
            brep2BPassed = null;
            bEdgeList2Bpassed = null;
            Rhino.Input.Custom.GetObject getObject = new Rhino.Input.Custom.GetObject
            {
                GeometryFilter = Rhino.DocObjects.ObjectType.Brep 
            };
            getObject.SetCommandPrompt(commandPrompt);
            getObject.Get();
            if (getObject.CommandResult() != Rhino.Commands.Result.Success)
            {
                return false;
            }
            Rhino.DocObjects.ObjRef objRef = getObject.Object(0);
            brep2BPassed = objRef.Brep();
            bEdgeList2Bpassed = brep2BPassed.Edges;
            return brep2BPassed != null & bEdgeList2Bpassed != null;
        }
    }

    internal class Core
    {
        #region FIELD

        private readonly Rhino.RhinoDoc currentDoc;
        private readonly Rhino.Geometry.Brep brepSource;
        private readonly Rhino.Geometry.Collections.BrepEdgeList bEdgeList;

        #endregion
        
        #region ATTR
        public Rhino.Geometry.Curve[] CutterCrvs { get; private set; }
        private Rhino.Geometry.Plane PlaneCutter { get; set; }
        public Rhino.Geometry.Point3d CurrentPt { get; set; }
        private Rhino.Geometry.BrepEdge EdgeLocated { get; set; }
        public bool IsCmdKeyDown { get; set; }
        public bool IsShiftKeyDown { get; set; }
        
        #endregion

        public Core(Rhino.RhinoDoc doc)
        {
            this.currentDoc = doc;
            Failsafe.IsBrepCollected = MethodBasic.ObjectCollecter("Select Brep to be Cut.", out this.brepSource, out this.bEdgeList);
        }

        

        public bool PointsCollector()
        {
            Rhino.Input.Custom.GetPoint gp = new Rhino.Input.Custom.GetPoint();
            gp.Get();
            return true;
        }

        private bool InitiationEdgeFinder()
        {
            foreach (Rhino.Geometry.BrepEdge bEdge in bEdgeList)
            {
                bEdge.ClosestPoint(this.CurrentPt, out double t);
                Rhino.Geometry.Point3d ptProjected = bEdge.PointAt(t);
                // bEdgeTarget Finder //
                if (!(ptProjected.DistanceTo(this.CurrentPt) < this.currentDoc.ModelAbsoluteTolerance)) 
                {
                    continue;
                }
                this.EdgeLocated = bEdge;
                break;

            }
            return true;
        }
        public bool CutterPlane()
        {

            InitiationEdgeFinder();
            if (EdgeLocated != null)
            {
                EdgeLocated.ClosestPoint(CurrentPt, out double p);
                Rhino.Geometry.Vector3d axis = EdgeLocated.TangentAt(p);
                PlaneCutter = new Rhino.Geometry.Plane(CurrentPt, axis);
            }
            else
            {
                // Placeholder //
            }
            


            Rhino.Geometry.Intersect.Intersection.BrepPlane(this.brepSource,
                                                            this.PlaneCutter,
                                                            this.currentDoc.ModelAbsoluteTolerance,
                                                            out Rhino.Geometry.Curve[] intersecrtionCrvs,
                                                            out Rhino.Geometry.Point3d[] intersectionPts);
            CutterCrvs = intersecrtionCrvs;
            return true;
        }
    }
    

    internal class GetPointTemplate : Rhino.Input.Custom.GetPoint
    {
        private readonly Core coreObj;

        public GetPointTemplate(Core coreobjPassed)
        {
            coreObj = coreobjPassed;
        }

        protected override void OnMouseMove(GetPointMouseEventArgs e)
        {
            coreObj.CurrentPt = e.Point;
            coreObj.CutterPlane();
            coreObj.IsCmdKeyDown = e.ControlKeyDown;
            coreObj.IsShiftKeyDown = e.ShiftKeyDown;
            base.OnMouseMove(e);
        }

        protected override void OnDynamicDraw(GetPointDrawEventArgs e)
        {
            if (coreObj.CutterCrvs != null)
            {
                foreach (Rhino.Geometry.Curve crv in coreObj.CutterCrvs)
                {
                    e.Display.DrawCurve(crv, System.Drawing.Color.Blue, 4);
                }
            }

            if (coreObj.IsCmdKeyDown)
            {
                e.Display.DrawPoint(e.CurrentPoint, System.Drawing.Color.Chartreuse);
            }

            if (coreObj.IsShiftKeyDown)
            {
                e.Display.DrawPoint(e.CurrentPoint, System.Drawing.Color.Crimson);
            }
            base.OnDynamicDraw(e);
        }
    }

    internal static class Failsafe
    {
        public static bool IsBrepCollected { get; set; }
        public static Rhino.Commands.Result Interruption(bool result)
        {
            if (!result)
            {
                return Rhino.Commands.Result.Failure;
            }
            else
            {
                return Rhino.Commands.Result.Nothing;
            }
        }
    }
}
