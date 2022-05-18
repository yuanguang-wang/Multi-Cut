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
        protected Rhino.Geometry.Point3d CurrentPt { get; set; }
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
        public new Rhino.Geometry.BrepEdge EdgeLocated { get; set; }


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
            int currentPtsNumber = this.Pt_List.Count();

            if (currentPtsNumber == 0)
            {
                return false;
            }
            else if (currentPtsNumber == 1)
            {
                Rhino.Geometry.Vector3d axis;
                base.CurrentPt = this.Pt_List[0];
                base.InitiationEdgeFinder();
                if (base.EdgeLocated != null)
                {
                    this.EdgeLocated = base.EdgeLocated;
                    this.EdgeLocated.ClosestPoint(this.Pt_List[0], out double p);
                    axis = this.EdgeLocated.TangentAt(p);
                    this.PlaneCutter = new Rhino.Geometry.Plane(this.Pt_List[0], axis);
                }
                else
                {
                    // Placeholder //
                }

                




            }
            else if (currentPtsNumber == 2)
            {

                
            }
            else if (currentPtsNumber == 3)
            {
                // Placeholder
            }
            else
            {
                // Placeholder
            }

            Rhino.Geometry.Intersect.Intersection.BrepPlane(this.BrepSource,
                                                            this.PlaneCutter,
                                                            this.CurrentDoc.ModelAbsoluteTolerance,
                                                            out Rhino.Geometry.Curve[] intersecrtionCrvs,
                                                            out Rhino.Geometry.Point3d[] intersectionPts);
            this.CutterCrvs = intersecrtionCrvs;
            return true;
        }
    }

    class EventModerator
    {
        public Core CoreObj { get; set; }
        public void CutByOnePtEventMod(object sender, Rhino.Input.Custom.GetPointDrawEventArgs e)
        {
            this.CoreObj.Pt_List.Add(e.CurrentPoint);
            this.CoreObj.CutterPlane();
            if (this.CoreObj.CutterCrvs != null)
            {
                foreach (Rhino.Geometry.Curve crv in this.CoreObj.CutterCrvs)
                {
                    e.Display.DrawCurve(crv, System.Drawing.Color.Blue, 4);
                }
            }
            
            
        }
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
