using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Display;


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
        public List<Rhino.Geometry.BrepFace> FaceLocatedList { get; set; }
        public bool IsCmdKeyDown { get; set; }
        public bool IsShiftKeyDown { get; set; }
        public Rhino.DocObjects.ConstructionPlane CPlane { get; set; }
        public List<Rhino.Geometry.Curve> IsocrvList { get; set; }

        #endregion

        #region CTOR
        public Core(Rhino.RhinoDoc doc)
        {
            this.currentDoc = doc;
            Failsafe.IsBrepCollected = MethodBasic.ObjectCollecter("Select Brep to be Cut.", out this.brepSource, out this.bEdgeList);
        }
        #endregion

        #region MTHD

        private bool EdgeFinder()
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

        private bool FaceFinder()
        {
            if (this.EdgeLocated == null)
            {
                return false;
            }
            this.FaceLocatedList = new List<Rhino.Geometry.BrepFace>();
            int[] faceIndex = this.EdgeLocated.AdjacentFaces();
            foreach (int index in faceIndex)
            {
                this.FaceLocatedList.Add(this.brepSource.Faces[index]);
            }

            return this.FaceLocatedList.Count != 0;
        }

        public bool PlaneGenerator()
        {
            this.EdgeFinder();
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
        
        public bool CPlaneGenerator()
        {
            if (this.PlaneCutter == null | this.CutterCrvs == null)
            {
                return false;
            }

            this.CPlane = new Rhino.DocObjects.ConstructionPlane();
            List<Rhino.Geometry.Point3d> cutterCrvVtxList = new List<Rhino.Geometry.Point3d>();
            foreach (Rhino.Geometry.Curve crv in this.CutterCrvs)
            {
                cutterCrvVtxList.Add(crv.PointAtStart);
                cutterCrvVtxList.Add(crv.PointAtEnd);
            }
            Rhino.Geometry.BoundingBox cutterBox = new Rhino.Geometry.BoundingBox(cutterCrvVtxList);
            double spacing = cutterBox.Diagonal.Length;
            if (this.PlaneCutter == null)
            {
                Rhino.RhinoApp.WriteLine("plane cutter not exist");
                return false;
            }
            this.CPlane.Plane = this.PlaneCutter;
            this.CPlane.ShowGrid = true;
            this.CPlane.GridSpacing = spacing / 10;
            this.CPlane.GridLineCount = 10;
            this.CPlane.DepthBuffered = true;
            return true;
        }

        public bool IsocrvGenerator()
        {
            this.FaceFinder();
            if (this.FaceLocatedList == null)
            {
                return false;
            }
            this.IsocrvList = new List<Rhino.Geometry.Curve>();
            foreach (Rhino.Geometry.BrepFace bFace in this.FaceLocatedList)
            {
                bFace.ClosestPoint(this.CurrentPt, out double u, out double v);
                this.IsocrvList.AddRange(bFace.TrimAwareIsoCurve(1,u));
                this.IsocrvList.AddRange(bFace.TrimAwareIsoCurve(0,v));
                
            }
            return true;
        }
        
        #endregion

        
    }

    internal class GetPointTemplate : Rhino.Input.Custom.GetPoint
    {
        private readonly Core coreObj;

        public GetPointTemplate(Core coreobjPassed)
        {
            coreObj = coreobjPassed;
        }

        protected override void OnMouseMove(Rhino.Input.Custom.GetPointMouseEventArgs e)
        {
            coreObj.CurrentPt = e.Point;
            coreObj.PlaneGenerator();
            coreObj.CPlaneGenerator();
            coreObj.IsocrvGenerator();
            coreObj.IsCmdKeyDown = e.ControlKeyDown;
            coreObj.IsShiftKeyDown = e.ShiftKeyDown;
            base.OnMouseMove(e);
        }

        protected override void OnDynamicDraw(Rhino.Input.Custom.GetPointDrawEventArgs e)
        {
            Rhino.Display.DisplayMaterial mtl = new Rhino.Display.DisplayMaterial(Rhino.ApplicationSettings.AppearanceSettings.SelectedObjectColor, 0.5);
            if (coreObj.CutterCrvs != null)
            {
                foreach (Rhino.Geometry.Curve crv in coreObj.CutterCrvs)
                {
                    e.Display.DrawCurve(crv, System.Drawing.Color.Black, 2);
                    
                }
                e.Display.DrawConstructionPlane(coreObj.CPlane);
                foreach (Rhino.Geometry.Curve crv in coreObj.IsocrvList)
                {
                    e.Display.DrawCurve(crv, System.Drawing.Color.Blue, 2);
                }
                foreach (Rhino.Geometry.BrepFace bFace in coreObj.FaceLocatedList)
                {
                    e.Display.DrawBrepShaded(bFace.DuplicateFace(false),mtl);
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
