using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using Rhino.Geometry.Collections;


namespace MultiCut
{
    internal class MethodAssembly
    {
        // delegate placeholder
    }

    internal static class MethodBasic
    {
        public static bool ObjectCollecter(out Brep brep2BPassed)
        {
            brep2BPassed = null;
            Rhino.Input.Custom.GetObject getObject = new Rhino.Input.Custom.GetObject
            {
                GeometryFilter = Rhino.DocObjects.ObjectType.Brep 
            };
            getObject.SetCommandPrompt("Select Brep to be Cut.");
            getObject.Get();
            if (getObject.CommandResult() != Rhino.Commands.Result.Success)
            {
                return false;
            }
            Rhino.DocObjects.ObjRef objRef = getObject.Object(0);
            brep2BPassed = objRef.Brep();
            return brep2BPassed != null;
        }

        public static bool EdgeFinder(Brep brep, Point3d pt, Rhino.RhinoDoc doc)
        {
            return brep.Edges.Select(bEdge => FarAwayFilter(bEdge, pt, doc)).FirstOrDefault();
        }

        private static bool PolylineFilter(Curve crv)
        {
            Curve[] segArray = crv.DuplicateSegments();
            return segArray.Length == 1;
        }

        private static bool FarAwayFilter(Curve crv, Point3d pt, Rhino.RhinoDoc doc)
        {
            crv.ClosestPoint(pt, out double t);
            Point3d ptProjected = crv.PointAt(t);
            return ptProjected.DistanceTo(pt) < doc.ModelAbsoluteTolerance;
        }

        private static bool EdgeDupFilter(Curve crv, Brep brep)
        {
            return brep.Edges.All(bEdge => !GeometryBase.GeometryEquals(crv, bEdge));
        }

        private static bool MidPtFilter(Curve crv, Point3d pt, Rhino.RhinoDoc doc, out List<Curve> crvSplitted)
        {
            crvSplitted = new List<Curve>();
            double disStart = pt.DistanceTo(crv.PointAtStart);
            double disEnd = pt.DistanceTo(crv.PointAtEnd);
            if (disStart > doc.ModelAbsoluteTolerance & disEnd > doc.ModelAbsoluteTolerance)
            {
                crv.ClosestPoint(pt, out double t);
                crvSplitted.AddRange(crv.Split(t));
                return false;
            }
            return true;
        }
        
        private static bool FlipFilter(Curve crv, Point3d pt, Rhino.RhinoDoc doc)
        {
            double disStart = pt.DistanceTo(crv.PointAtStart);
            if (disStart > doc.ModelAbsoluteTolerance)
            {
                crv.Reverse();
                return false;
            }
            return true;
        }

        public static bool OctopusBundleCollector(Curve crv,
            Point3d pt,
            Brep brep,
            Rhino.RhinoDoc doc,
            out Dictionary<Curve, OctopusType> octopusRawDic,
            OctopusType type)
        {
            octopusRawDic = new Dictionary<Curve, OctopusType>();
            bool isCrvSingle = PolylineFilter(crv);
            bool isPtOnCrv = FarAwayFilter(crv, pt, doc);
            bool isCrvDuped = EdgeDupFilter(crv, brep);
            if (isCrvSingle & isPtOnCrv & isCrvDuped)
            {
                bool isCrvNeedCut = MidPtFilter(crv, pt, doc, out List<Curve> crvSplittedList);
                if (isCrvNeedCut)
                {
                    FlipFilter(crv, pt, doc);
                    octopusRawDic.Add(crv, type);
                }
                else
                {
                    foreach (Curve crvSplitted in crvSplittedList)
                    {
                        FlipFilter(crv, pt, doc);
                        octopusRawDic.Add(crvSplitted, type);
                    }
                }
            }
            return true;
        }
        
    }

    internal class Core
    { 
        #region FIELD

        public readonly Rhino.RhinoDoc currentDoc;
        public readonly Brep CurrentBrep;

        #endregion
        
        #region ATTR
        public Curve[] CutterCrvs { get; private set; }
        private Plane PlaneCutter { get; set; }
        public Point3d CurrentPt { get; set; }
        private List<BrepEdge> EdgeFoundList { get; set; }
        public List<BrepFace> FaceFoundList { get; set; }
        public bool IsAssistKeyDown { get; set; }
        public Rhino.DocObjects.ConstructionPlane CutPlane { get; set; }
        public Point3d[] AssistPtList { get; private set; }
        public Dictionary<Curve, OctopusType> OctopusRaw { get; set; }
        public Dictionary<Curve, string> OctopusCascade { get; set; }

        #endregion

        #region CTOR
        public Core(Rhino.RhinoDoc doc)
        {
            this.currentDoc = doc;
            Watchdog.IsBrepCollected = MethodBasic.ObjectCollecter(out this.CurrentBrep);
        }
        #endregion

        #region MTHD
        private void EdgeFinder()
        {
            this.EdgeFoundList = new List<BrepEdge>();
            foreach (BrepEdge bEdge in CurrentBrep.Edges)
            {
                bEdge.ClosestPoint(this.CurrentPt, out double t);
                Point3d ptProjected = bEdge.PointAt(t);
                double distance = ptProjected.DistanceTo(this.CurrentPt);
                if (distance < this.currentDoc.ModelAbsoluteTolerance) 
                {
                    this.EdgeFoundList.Add(bEdge);
                }
            }
        }
        private void FaceFinder()
        {
            this.FaceFoundList = new List<BrepFace>();
            List<int> faceIndexList = new List<int>();
            foreach (BrepEdge bEdge in this.EdgeFoundList)
            {
                int[] faceIndexArray = bEdge.AdjacentFaces();
                foreach (int faceindex in faceIndexArray)
                {
                    if (!faceIndexList.Contains(faceindex))
                    {
                        faceIndexList.Add(faceindex);
                    }
                }
            }
            foreach (int index in faceIndexList)
            {
                this.FaceFoundList.Add(this.CurrentBrep.Faces[index]);
            }
        }
        
        public void AssistPtGenerator()
        {
            if (this.EdgeFoundList.Count == 1)
            {
                this.EdgeFoundList[0].DivideByCount(9, true, out Point3d[] cptTemp);
                this.AssistPtList = cptTemp;
            }
        }

        private bool ProphetGenerator()
        {
            if (this.EdgeFoundList.Count != 1)
            {
                return false;
            }

            BrepVertexList bVtxList = this.CurrentBrep.Vertices;
            foreach (BrepVertex bVtx in bVtxList)
            {
                if (this.CurrentPt.DistanceTo(bVtx.Location) < this.currentDoc.ModelAbsoluteTolerance)
                {
                    return false;
                }
            }
            
            EdgeFoundList[0].ClosestPoint(CurrentPt, out double p);
            Vector3d axis = EdgeFoundList[0].TangentAt(p);
            PlaneCutter = new Plane(CurrentPt, axis);



            Rhino.Geometry.Intersect.Intersection.BrepPlane(this.CurrentBrep,
                                                            this.PlaneCutter,
                                                            this.currentDoc.ModelAbsoluteTolerance,
                                                            out Curve[] intersecrtionCrvs,
                                                            out Point3d[] intersectionPts);
            CutterCrvs = intersecrtionCrvs;
            return true;
        }

        private bool CutPlaneGenerator()
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

        private bool ISOCrvGenerator()
        {
            if (this.FaceFoundList == null)
            {
                return false;
            }
            foreach (BrepFace bFace in this.FaceFoundList)
            {
                bFace.ClosestPoint(this.CurrentPt, out double u, out double v);
                Curve[] isou = bFace.TrimAwareIsoCurve(1, u);
                Curve[] isov = bFace.TrimAwareIsoCurve(0, v);
                foreach (Curve iso in isou)
                {
                    MethodBasic.OctopusBundleCollector(iso, this.CurrentPt, this.CurrentBrep, this.currentDoc,
                                                       out Dictionary<Curve, OctopusType> octopusRawDic, 
                                                       OctopusType._ISOU);
                    foreach (KeyValuePair<Curve, OctopusType> element in octopusRawDic)
                    {
                        this.OctopusRaw.Add(element.Key, element.Value);
                    }
                }
                foreach (Curve iso in isov)
                {
                    MethodBasic.OctopusBundleCollector(iso, this.CurrentPt, this.CurrentBrep, this.currentDoc,
                                                       out Dictionary<Curve, OctopusType> octopusRawDic, 
                                                       OctopusType._ISOV);
                    foreach (KeyValuePair<Curve, OctopusType> element in octopusRawDic)
                    {
                        this.OctopusRaw.Add(element.Key, element.Value);
                    }
                }
            }
            return true;
        }

        private bool CPLCrvGenerator()
        {
            Plane currentCpl =
                this.currentDoc.Views.ActiveView.ActiveViewport.ConstructionPlane();
            if (currentCpl == Plane.Unset)
            {
                return false;
            }
            
            Vector3d[] cplAxis = new[] { currentCpl.XAxis, currentCpl.YAxis, currentCpl.ZAxis };
            OctopusType[] cplType = new[] { OctopusType._CPLX, OctopusType._CPLY, OctopusType._CPLZ };
            
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
                        foreach (Curve crv in intersecrtionCrvs)
                        {
                            MethodBasic.OctopusBundleCollector(crv, this.CurrentPt, this.CurrentBrep, this.currentDoc,
                                                               out Dictionary<Curve, OctopusType> octopusRawDic, 
                                                               cplType[i]);
                            foreach (KeyValuePair<Curve, OctopusType> element in octopusRawDic)
                            {
                                this.OctopusRaw.Add(element.Key, element.Value);
                            }
                        }
                    }
                    i++;
                }
            }
            return true;
        }

        private bool WPLCrvGenerator()
        {
            Plane[] wplPlanes = new[]
            {
                new Plane(this.CurrentPt, Vector3d.ZAxis), //z
                new Plane(this.CurrentPt, Vector3d.XAxis), //x
                new Plane(this.CurrentPt, Vector3d.YAxis)  //y
            };
            OctopusType[] wplType = new[] { OctopusType._WPLZ, OctopusType._WPLX, OctopusType._WPLY };
            foreach (BrepFace bFace in this.FaceFoundList)
            {
                int i = 0;
                foreach (Plane wpl in wplPlanes)
                {
                    Rhino.Geometry.Intersect.Intersection.BrepPlane(bFace.DuplicateFace(false),
                                                                    wpl,
                                                                    this.currentDoc.ModelAbsoluteTolerance,
                                                                    out Curve[] intersecrtionCrvs,
                                                                    out Point3d[] intersectionPts);
                    if (intersecrtionCrvs != null)
                    {
                        foreach (Curve crv in intersecrtionCrvs)
                        {

                            MethodBasic.OctopusBundleCollector(crv, this.CurrentPt, this.CurrentBrep, this.currentDoc,
                                                               out Dictionary<Curve, OctopusType> octopusRawDic, 
                                                               wplType[i]);
                            foreach (KeyValuePair<Curve, OctopusType> element in octopusRawDic)
                            {
                                this.OctopusRaw.Add(element.Key, element.Value);
                            }
                        }
                    }
                    i++;
                }
            }
            return true;
        }

        private bool OctopusCascader()
        {
            this.OctopusCascade = new Dictionary<Curve, string>();
            
            List<Curve> octopusCrvList = this.OctopusRaw.Keys.ToList();
            List<OctopusType> octopusTypeList = this.OctopusRaw.Values.ToList();
            List<string> octopusTypeStrList = new List<string>();
            
            List<int> criminalIndex = new List<int>();
            List<int> allIndex = new List<int>();

            List<Curve> keyList = this.OctopusRaw.Keys.ToList();
            List<OctopusType> valueList = this.OctopusRaw.Values.ToList();
            
            for (int i = 0; i < octopusCrvList.Count; i++)
            {
                allIndex.Add(i);
            }
            for (int i = 0; i < this.OctopusRaw.Count - 1; i++)
            {
                string typeOriginal = octopusTypeList[i].ToString();
                string typeModifeid = "";
                typeModifeid += typeOriginal;
                for (int j = i + 1; j < this.OctopusRaw.Count; j++)
                {
                    bool isCrvIdentical = GeometryBase.GeometryEquals(keyList[j], octopusCrvList[i]);
                    if (isCrvIdentical)
                    {
                        if (octopusTypeList[i] != valueList[j])
                        {
                            if (!typeModifeid.Contains(valueList[j].ToString()))
                            {
                                typeModifeid += valueList[j].ToString();
                            }
                            if (!criminalIndex.Contains(j))
                            {
                                criminalIndex.Add(j);
                            }
                        }
                    }
                }
                octopusTypeStrList.Add(typeModifeid);
            }

            IEnumerable<int> clearIndex = allIndex.Except(criminalIndex);
            List<int> clearIndexList = clearIndex.ToList();
            for (int i = 0; i < clearIndexList.Count(); i++)
            {
                this.OctopusCascade.Add(octopusCrvList[clearIndexList[i]], octopusTypeStrList[clearIndexList[i]]);
            }
            return true;
        }

        public void OnMouseMoveBundle()
        {
            this.EdgeFinder();
            this.FaceFinder();
            this.ProphetGenerator();
            this.CutPlaneGenerator();
            this.ISOCrvGenerator();
            this.CPLCrvGenerator();
            this.WPLCrvGenerator();
            this.OctopusCascader();
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
            coreObj.OctopusRaw = new Dictionary<Curve, OctopusType>();
            coreObj.IsAssistKeyDown = e.ShiftKeyDown & e.ControlKeyDown;
            coreObj.CurrentPt = e.Point;
            bool isPtOnEdge = MethodBasic.EdgeFinder(coreObj.CurrentBrep, coreObj.CurrentPt, coreObj.currentDoc);
            if (isPtOnEdge)
            {
                coreObj.OnMouseMoveBundle();
            }
            
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

            foreach (Curve crv in coreObj.OctopusCascade.Keys)
            {
                e.Display.DrawCurve(crv, System.Drawing.Color.Blue, 3);
            }

            foreach (KeyValuePair<Curve, string> element in coreObj.OctopusCascade)
            {
                e.Display.Draw2dText(element.Value, System.Drawing.Color.Blue, element.Key.PointAtEnd, false, 14);
            }
            foreach (BrepFace bFace in coreObj.FaceFoundList)
            {
                e.Display.DrawBrepShaded(bFace.DuplicateFace(false),mtl);
            }


            if (coreObj.IsAssistKeyDown)
            {
                coreObj.AssistPtGenerator();
                if (coreObj.AssistPtList != null)
                {
                    this.AddConstructionPoints(coreObj.AssistPtList);
                    e.Display.DrawPoints(coreObj.AssistPtList, Rhino.Display.PointStyle.RoundControlPoint, 5, System.Drawing.Color.Red);
                }
            }
            else
            {
                this.ClearConstructionPoints();
            }
            
            base.OnDynamicDraw(e);
        }
    }

    internal static class Watchdog
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
