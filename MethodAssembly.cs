using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;


namespace MultiCut
{
    internal class MethodAssembly
    {
        // delegate placeholder
    }

    internal static class MethodBasic
    {
        public static bool ObjectCollecter(string commandPrompt, 
                                           out Brep brep2BPassed, 
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
        public static bool PointOnCrvDetector(Curve crv, Point3d pt, Rhino.RhinoDoc doc)
        {
            crv.ClosestPoint(pt, out double t);
            Point3d ptProjected = crv.PointAt(t);
            return ptProjected.DistanceTo(pt) < doc.ModelAbsoluteTolerance;
        }

        public static bool PointOverlapeDetector(Curve crv, Point3d pt, Rhino.RhinoDoc doc, out List<Point3d> ptList)
        {
            ptList = new List<Point3d>();
            double disStart = pt.DistanceTo(crv.PointAtStart);
            if (disStart > doc.ModelAbsoluteTolerance)
            {
                ptList.Add(crv.PointAtStart);
            }

            double disEnd = pt.DistanceTo(crv.PointAtEnd);
            if (disEnd > doc.ModelAbsoluteTolerance)
            {
                ptList.Add(crv.PointAtEnd);
            }
            return true;
        }
    }

    internal class Core
    {
        #region FIELD

        private readonly Rhino.RhinoDoc currentDoc;
        private readonly Brep brepSource;
        private readonly Rhino.Geometry.Collections.BrepEdgeList bEdgeList;

        #endregion
        
        #region ATTR
        public Curve[] CutterCrvs { get; private set; }
        private Plane PlaneCutter { get; set; }
        public Point3d CurrentPt { get; set; }
        private BrepEdge EdgeFound { get; set; }
        public List<BrepFace> FaceFoundList { get; set; }
        public bool IsShiftKeyDown { get; set; }
        public Rhino.DocObjects.ConstructionPlane CutPlane { get; set; }
        public List<Curve> IsocrvList { get; set; }
        public List<Curve> CplcrvList { get; set; }
        public List<Curve> OctopusArmList { get; set; }
        public List<List<Point3d>> OctopusEndList { get; set; }
        public List<string> OctopusNameList { get; set; }
        public Point3d[] CptList { get; private set; }

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
            foreach (BrepEdge bEdge in bEdgeList)
            {
                bool isPointOnEdge = MethodBasic.PointOnCrvDetector(bEdge, this.CurrentPt, this.currentDoc);
                bEdge.ClosestPoint(this.CurrentPt, out double t);
                Point3d ptProjected = bEdge.PointAt(t);
                // bEdgeTarget Finder //
                if (isPointOnEdge) 
                {
                    this.EdgeFound = bEdge;
                    break;
                }
            }
            return true;
        }
        private bool FaceFinder()
        {
            if (this.EdgeFound == null)
            {
                return false;
            }
            this.FaceFoundList = new List<BrepFace>();
            int[] faceIndex = this.EdgeFound.AdjacentFaces();
            foreach (int index in faceIndex)
            {
                this.FaceFoundList.Add(this.brepSource.Faces[index]);
            }

            return this.FaceFoundList.Count != 0;
        }
        private bool OctopusCollector(IEnumerable<Curve> intersectionCrvArray, ref int i, OctopusType[] plType)
        {
            foreach (Curve crv in intersectionCrvArray)
            {
                bool isPointOnCrv = MethodBasic.PointOnCrvDetector(crv, this.CurrentPt, this.currentDoc);
                if (isPointOnCrv)
                {
                    MethodBasic.PointOverlapeDetector(crv, this.CurrentPt, this.currentDoc, out List<Point3d> ptList);
                    foreach (Point3d pt2BAdd in ptList)
                    {
                        int j = 0;
                        List<List<Point3d>> octopusEndListCopy = this.OctopusEndList.ToList();
                        foreach (List<Point3d> endList in octopusEndListCopy)
                        {
                            foreach (Point3d ptInEnd in endList)
                            {
                                if (pt2BAdd.DistanceTo(ptInEnd) <= this.currentDoc.ModelAbsoluteTolerance)
                                {
                                    string newname = this.OctopusNameList[j];
                                    newname = newname + ",";
                                    newname = newname + plType[i];
                                    this.OctopusNameList[j] = newname;
                                }
                                else
                                {
                                    this.OctopusArmList.Add(crv);
                                    this.OctopusEndList.Add(ptList);
                                    this.OctopusNameList.Add(plType[i].ToString());
                                }
                            }
                            j++;
                        }
                    }
                }
            }
            i++;
            return true;
        }
        public bool CptGenerator()
        {
            if (this.EdgeFound == null)
            {
                return false;
            }
            this.EdgeFound.DivideByCount(9, true, out Point3d[] cptTemp);
            this.CptList = cptTemp;
            return true;
        }
        public bool PlaneGenerator()
        {
            this.EdgeFinder();
            if (EdgeFound != null)
            {
                EdgeFound.ClosestPoint(CurrentPt, out double p);
                Vector3d axis = EdgeFound.TangentAt(p);
                PlaneCutter = new Plane(CurrentPt, axis);
            }
            else
            {
                // Placeholder //
            }
            


            Rhino.Geometry.Intersect.Intersection.BrepPlane(this.brepSource,
                                                            this.PlaneCutter,
                                                            this.currentDoc.ModelAbsoluteTolerance,
                                                            out Curve[] intersecrtionCrvs,
                                                            out Point3d[] intersectionPts);
            CutterCrvs = intersecrtionCrvs;
            return true;
        }
        public bool CutPlaneGenerator()
        {
            if (this.PlaneCutter == null | this.CutterCrvs == null)
            {
                return false;
            }

            this.CutPlane = new Rhino.DocObjects.ConstructionPlane();
            List<Point3d> cutterCrvVtxList = new List<Point3d>();
            foreach (Curve crv in this.CutterCrvs)
            {
                cutterCrvVtxList.Add(crv.PointAtStart);
                cutterCrvVtxList.Add(crv.PointAtEnd);
            }
            BoundingBox cutterBox = new BoundingBox(cutterCrvVtxList);
            double spacing = cutterBox.Diagonal.Length;
            if (this.PlaneCutter == null)
            {
                Rhino.RhinoApp.WriteLine("plane cutter not exist");
                return false;
            }
            this.CutPlane.Plane = this.PlaneCutter;
            this.CutPlane.ShowGrid = true;
            this.CutPlane.GridSpacing = spacing / 1;
            this.CutPlane.GridLineCount = 1;
            this.CutPlane.DepthBuffered = true;
            this.CutPlane.ShowAxes = false;
            this.CutPlane.ShowZAxis = false;

            return true;
        }
        public bool IsocrvGenerator()
        {
            this.FaceFinder();
            if (this.FaceFoundList == null)
            {
                return false;
            }
            this.IsocrvList = new List<Curve>();
            foreach (BrepFace bFace in this.FaceFoundList)
            {
                bFace.ClosestPoint(this.CurrentPt, out double u, out double v);
                Curve[] isou = bFace.TrimAwareIsoCurve(1, u);
                Curve[] isov = bFace.TrimAwareIsoCurve(0, v);
                this.IsocrvList.AddRange(isou);
                this.IsocrvList.AddRange(isov);
                foreach (Curve iso in isou)
                {
                    bool isPointOnIso = MethodBasic.PointOnCrvDetector(iso, this.CurrentPt, this.currentDoc);
                    if (isPointOnIso)
                    {
                        this.OctopusArmList.Add(iso);
                        this.OctopusNameList.Add(OctopusType._ISOU.ToString());
                        MethodBasic.PointOverlapeDetector(iso,
                                                          this.CurrentPt, 
                                                          this.currentDoc, 
                                                          out List < Point3d > ptList);
                        this.OctopusEndList.Add(ptList);
                    }
                }
                foreach (Curve iso in isov)
                {
                    bool isPointOnIso = MethodBasic.PointOnCrvDetector(iso, this.CurrentPt, this.currentDoc);
                    if (isPointOnIso)
                    {
                        this.OctopusArmList.Add(iso);
                        this.OctopusNameList.Add(OctopusType._ISOV.ToString());
                        MethodBasic.PointOverlapeDetector(iso,
                            this.CurrentPt, 
                            this.currentDoc, 
                            out List <Point3d> ptList);
                        this.OctopusEndList.Add(ptList);
                    }
                }
            }
            return true;
        }
        public bool CplcrvGenerator()
        {
            this.CplcrvList = new List<Curve>();
            Plane currentCpl =
                this.currentDoc.Views.ActiveView.ActiveViewport.ConstructionPlane();
            if (currentCpl == Plane.Unset)
            {
                return false;
            }
            Vector3d[] cplAxis = new[] { currentCpl.XAxis, currentCpl.YAxis, currentCpl.ZAxis };
            if (this.FaceFoundList == null || this.CurrentPt == null)
            {
                return false;
            }
            Plane[] cplPlanes = new[]
            {
                new Plane(this.CurrentPt, cplAxis[0]),
                new Plane(this.CurrentPt, cplAxis[1]),
                new Plane(this.CurrentPt, cplAxis[2])
            };
            OctopusType[] cplType = new[] { OctopusType._CPLX, OctopusType._CPLY, OctopusType._CPLZ };
            foreach (BrepFace bFace in this.FaceFoundList)
            {
                int i = 0;
                foreach (Plane cpl in cplPlanes)
                {
                    Rhino.Geometry.Intersect.Intersection.BrepPlane(bFace.DuplicateFace(false),
                        cpl,
                        this.currentDoc.ModelAbsoluteTolerance,
                        out Curve[] intersecrtionCrvs,
                        out Point3d[] intersectionPts);
                    if (intersecrtionCrvs != null)
                    {
                        this.OctopusCollector(intersecrtionCrvs, ref i, cplType);
                    }
                }
            }
            return true;
        }
        public bool WplcrvGenerator()
        {
            Plane[] wplPlanes = new[]
            {
                Plane.WorldXY, //z
                Plane.WorldYZ, //x
                Plane.WorldZX  //y
            };
            OctopusType[] wplType = new[] { OctopusType._WPLZ, OctopusType._WPLX, OctopusType._WPLY };
            foreach (BrepFace bFace in this.FaceFoundList)
            {
                int i = 0;
                foreach (Plane cpl in wplPlanes)
                {
                    Rhino.Geometry.Intersect.Intersection.BrepPlane(bFace.DuplicateFace(false),
                        cpl,
                        this.currentDoc.ModelAbsoluteTolerance,
                        out Curve[] intersecrtionCrvs,
                        out Point3d[] intersectionPts);
                    if (intersecrtionCrvs != null)
                    {
                        this.OctopusCollector(intersecrtionCrvs, ref i, wplType);
                    }
                }
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
            coreObj.OctopusArmList = new List<Curve>();
            coreObj.OctopusEndList = new List<List<Point3d>>();
            coreObj.OctopusNameList = new List<string>();
            coreObj.IsShiftKeyDown = e.ShiftKeyDown;
            coreObj.CurrentPt = e.Point;
            coreObj.PlaneGenerator();
            coreObj.CutPlaneGenerator();
            coreObj.IsocrvGenerator();
            coreObj.CplcrvGenerator();
            coreObj.WplcrvGenerator();
            
            base.OnMouseMove(e);
        }

        protected override void OnDynamicDraw(Rhino.Input.Custom.GetPointDrawEventArgs e)
        {
            Rhino.Display.DisplayMaterial mtl = new Rhino.Display.DisplayMaterial(Rhino.ApplicationSettings.AppearanceSettings.SelectedObjectColor, 0.5);
            
            e.Display.DrawConstructionPlane(coreObj.CutPlane);
            
            foreach (Curve crv in coreObj.CutterCrvs)
            {
                e.Display.DrawCurve(crv, System.Drawing.Color.Chartreuse, 3);
            }
            
            foreach (Curve crv in coreObj.OctopusArmList)
            {
                e.Display.DrawCurve(crv, System.Drawing.Color.Blue, 3);
            }

            int i = 0;
            foreach (List<Point3d> endList in coreObj.OctopusEndList)
            {
                foreach (Point3d pt in endList)
                {
                    e.Display.DrawPoint(pt, Rhino.Display.PointStyle.RoundControlPoint, 5, System.Drawing.Color.Blue);
                    e.Display.Draw2dText(coreObj.OctopusNameList[i],System.Drawing.Color.Blue,pt,false,16);
                }
                i++;
            }
            
            foreach (BrepFace bFace in coreObj.FaceFoundList)
            {
                e.Display.DrawBrepShaded(bFace.DuplicateFace(false),mtl);
            }


            if (coreObj.IsShiftKeyDown)
            {
                coreObj.CptGenerator();
                if (coreObj.CptList != null)
                {
                    this.AddConstructionPoints(coreObj.CptList);
                    e.Display.DrawPoints(coreObj.CptList, Rhino.Display.PointStyle.RoundControlPoint, 5, System.Drawing.Color.Red);
                }
            }
            else
            {
                this.ClearConstructionPoints();
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

    internal enum OctopusType
    {
        _ISOU, _ISOV, _CPLX, _CPLY, _CPLZ, _WPLX, _WPLY, _WPLZ
    }
}
