using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiCut
{
    class MethodAssembly
    {
    }

    class CoreBase
    {
        #region ATTR
        protected Rhino.RhinoDoc CurrentDoc { get; set; }
        protected Rhino.Geometry.Brep BrepSource { get; set; }
        protected Rhino.Geometry.Collections.BrepEdgeList BEdges_List { get; set; }
        public Rhino.Geometry.Point3d CurrentPt { get; set; }
        protected Rhino.Geometry.BrepEdge EdgeLocated { get; set; }

        #endregion

        protected bool InitiationEdgeFinder()
        {
            foreach (Rhino.Geometry.BrepEdge bEdge in BEdges_List)
            {
                bEdge.ClosestPoint(this.CurrentPt, out double t);
                Rhino.Geometry.Point3d ptProjected = bEdge.PointAt(t);
                // bEdgeTarget Finder //
                if (ptProjected.DistanceTo(this.CurrentPt) < this.CurrentDoc.ModelAbsoluteTolerance)
                {
                    this.EdgeLocated = bEdge;
                    break;
                }

            }
            return true;
        }
    }
    class Core : CoreBase
    {
        #region ATTR
        public Rhino.Geometry.Curve[] CutterCrvs { get; set; }        
        public Rhino.Geometry.Plane PlaneCutter { get; set; }
        public List<Rhino.Geometry.Point3d> Pt_List { get; set; }
        public bool CmdKey { get; set; }
        #endregion

        public Core(Rhino.RhinoDoc doc)
        {
            this.Pt_List = new List<Rhino.Geometry.Point3d>();
            this.CurrentDoc = doc;
            
        }

        public bool ObjectCollecter(string commandPrompt)
        {
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
            Rhino.Geometry.Brep brep2bPassed = objRef.Brep();
            if (brep2bPassed != null)
            {
                this.BrepSource = brep2bPassed;
                this.BEdges_List = brep2bPassed.Edges;
                return true;
            }
            else
            {
                return false;
            }
            
        }

        public bool PointsCollector()
        {
            Rhino.Input.Custom.GetPoint gp = new Rhino.Input.Custom.GetPoint();
            gp.Get();
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Curve: Intersected</returns>
        public bool CutterPlane()
        {


            Rhino.Geometry.Vector3d axis;
            InitiationEdgeFinder();
            if (EdgeLocated != null)
            {
                EdgeLocated.ClosestPoint(CurrentPt, out double p);
                axis = EdgeLocated.TangentAt(p);
                PlaneCutter = new Rhino.Geometry.Plane(CurrentPt, axis);
            }
            else
            {
                // Placeholder //
            }
            


            Rhino.Geometry.Intersect.Intersection.BrepPlane(BrepSource,
                                                            PlaneCutter,
                                                            CurrentDoc.ModelAbsoluteTolerance,
                                                            out Rhino.Geometry.Curve[] intersecrtionCrvs,
                                                            out Rhino.Geometry.Point3d[] intersectionPts);
            CutterCrvs = intersecrtionCrvs;
            return true;
        }
    }

    class EventModerator
    {
        public Core CoreObj { get; set; }

        public void Test(object sender, Rhino.Input.Custom.GetPointMouseEventArgs e)
        {
            CoreObj.CurrentPt = e.Point;
            CoreObj.CutterPlane();
            if (e.ControlKeyDown)
            {
                CoreObj.CmdKey = true;
            }
            else
            {
                CoreObj.CmdKey = false;
            }
        }

        public void CutByOnePtEventMod(object sender, Rhino.Input.Custom.GetPointDrawEventArgs e)
        {
            //CoreObj.CurrentPt = e.CurrentPoint;
            //CoreObj.CutterPlane();

            if (CoreObj.CmdKey == true)
            {
                e.Display.DrawPoint(e.CurrentPoint, System.Drawing.Color.Red);
            }
            if (CoreObj.CutterCrvs != null)
            {
                foreach (Rhino.Geometry.Curve crv in CoreObj.CutterCrvs)
                {
                    e.Display.DrawCurve(crv, System.Drawing.Color.Blue, 4);
                }

            }

        }

        private void CutOutside(object sender, Rhino.Input.Custom.GetPointDrawEventArgs e)
        {
            
        }
    }

    class GetFirstPoint : Rhino.Input.Custom.GetPoint
    {
        public Core CoreObj { get; set; }

        public GetFirstPoint(Core coreobjpassed)
        {
            CoreObj = coreobjpassed;
        }

        //protected override void OnDynamicDraw(Rhino.Input.Custom.GetPointDrawEventArgs e)
        //{
        //    CoreObj.CurrentPt = e.CurrentPoint;
        //    CoreObj.CutterPlane();

        //    if (CoreObj.CutterCrvs != null)
        //    {
        //        Rhino.RhinoApp.WriteLine("triggered");
        //        foreach (Rhino.Geometry.Curve crv in CoreObj.CutterCrvs)
        //        {
        //            e.Display.DrawCurve(crv, System.Drawing.Color.Blue, 4);
        //        }

        //    }
        //    else
        //    {
        //        Rhino.RhinoApp.WriteLine("not triggered");
        //    }
        //    base.OnDynamicDraw(e);

        //}
    }

    static class Failsafe
    {
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
