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

    class Core
    {
        #region ATTR
        public Rhino.Geometry.Curve[] CutterCrvs { get; set; }
        public Rhino.Geometry.Brep BrepSource { get; set; }
        public List<Rhino.Geometry.Point3d> Pts_List { get; set; }
        public Rhino.Geometry.Collections.BrepEdgeList BEdges_List { get; set; }
        public Rhino.RhinoDoc CurrentDoc { get; set; }
        public Rhino.Geometry.BrepEdge BEdgeInitiatedTarget { get; set; }
        public Rhino.Geometry.Plane PlaneCutter { get; set; }

        #endregion

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
            ;

            int currentPtsNumber = this.Pts_List.Count();

            if (currentPtsNumber == 0)
            {
                return false;
            }
            else if (currentPtsNumber == 1)
            {
                Rhino.Geometry.Vector3d axis;
                foreach (Rhino.Geometry.BrepEdge bEdge in BEdges_List)
                {
                    bEdge.ClosestPoint(this.Pts_List[0], out double t);
                    Rhino.Geometry.Point3d ptProjected = bEdge.PointAt(t);
                    // bEdgeTarget Finder //
                    if (ptProjected.DistanceTo(this.Pts_List[0]) < this.CurrentDoc.ModelAbsoluteTolerance)
                    {
                        this.BEdgeInitiatedTarget = bEdge;
                        break;
                    }
                    else
                    {
                        return false;
                    }
                }
                this.BEdgeInitiatedTarget.ClosestPoint(this.Pts_List[0], out double p);
                axis = this.BEdgeInitiatedTarget.TangentAt(p);
                this.PlaneCutter = new Rhino.Geometry.Plane(this.Pts_List[0], axis);
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

    static class EventModerator
    {
        static public void CutByOnePtEventMod(object s, Rhino.Input.Custom.GetPointDrawEventArgs e)
        {
            MultiCut.Core core = new Core();
            core.Pts_List = new List<Rhino.Geometry.Point3d>();
            core.Pts_List.Add(e.CurrentPoint);
            core.CutterPlane();
            foreach (Rhino.Geometry.Curve crv in core.CutterCrvs)
            {
                e.Display.DrawCurve(crv, System.Drawing.Color.Blue, 4);
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
