using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;
using Rhino.Input.Custom;

using Eto.Forms;
using Rhino.UI;


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
            GetObject getObject = new GetObject
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

        private static bool PolylineFilter(Curve crv)
        {
            Curve[] segArray = crv.DuplicateSegments();
            return segArray.Length == 1;
        }

        private static bool OnEdgeFilter(Curve crv, Point3d pt, RhinoDoc doc)
        {
            crv.ClosestPoint(pt, out double t);
            Point3d ptProjected = crv.PointAt(t);
            return ptProjected.DistanceTo(pt) < doc.ModelAbsoluteTolerance;
        }

        public static bool OnLoopFilter(Brep brep, int faceIndex, Point3d startpt, Point3d endpt, RhinoDoc doc)
        {
            Curve outerLoop = brep.Faces[faceIndex].OuterLoop.To3dCurve();
            bool isStartPtOnLoop = OnEdgeFilter(outerLoop, startpt, doc);
            bool isEndPtOnLoop = OnEdgeFilter(outerLoop, endpt, doc);
            return isStartPtOnLoop & isEndPtOnLoop;
        }

        private static bool EdgeDupFilter(Curve crv, Brep brep)
        {
            return brep.Edges.All(bEdge => !GeometryBase.GeometryEquals(crv, bEdge));
        }

        private static bool MidPtFilter(Curve crv, Point3d pt, RhinoDoc doc, out List<Curve> crvSplitted)
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
        
        private static void FlipFilter(Curve crv, Point3d pt, RhinoDoc doc)
        {
            double disStart = pt.DistanceTo(crv.PointAtStart);
            if (disStart > doc.ModelAbsoluteTolerance)
            {
                crv.Reverse();
            }
        }

        public static void OctopusBundleCollector(Curve crv,
            Point3d pt,
            Brep brep,
            RhinoDoc doc,
            out Dictionary<Curve, OctopusType> octopusRawDic,
            OctopusType type)
        {
            octopusRawDic = new Dictionary<Curve, OctopusType>();
            bool isCrvSingle = PolylineFilter(crv);
            bool isPtOnCrv = OnEdgeFilter(crv, pt, doc);
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
                        FlipFilter(crvSplitted, pt, doc);
                        octopusRawDic.Add(crvSplitted, type);
                    }
                }
            }
        }

        public static List<BrepEdge> EdgeFinder(Point3d pt, Brep brep, RhinoDoc doc)
        {
            List<BrepEdge> bEdgeList = new List<BrepEdge>();
            foreach (BrepEdge bEdge in brep.Edges)
            {
                bEdge.ClosestPoint(pt, out double t);
                Point3d ptProjected = bEdge.PointAt(t);
                double distance = ptProjected.DistanceTo(pt);
                if (distance < doc.ModelAbsoluteTolerance) 
                {
                    bEdgeList.Add(bEdge);
                }
            }
            return bEdgeList;
        }

        public static List<int> FaceFinder(IEnumerable<BrepEdge> bEdgeList)
        {
            List<int> faceIndexList = new List<int>();
            foreach (int faceindex in 
                     from bEdge in bEdgeList 
                     select bEdge.AdjacentFaces() into faceIndexArray 
                     from faceindex in faceIndexArray 
                     where !faceIndexList.Contains(faceindex) 
                     select faceindex)
            {
                faceIndexList.Add(faceindex);
            }
            return faceIndexList;
        }

    }

    internal class Core
    { 
        #region FIELD

        private readonly RhinoDoc currentDoc;
        private readonly Brep currentBrep;

        #endregion
        
        #region ATTR
        public Curve[] ProphetCrvs { get; private set; }
        private Plane ProphetPlane { get; set; }
        public Point3d CurrentPt { get; set; }
        private Point3d LastPt { get; set; }
        public Point3d LastPtCandidate { get; set; }
        private List<BrepEdge> CurrentEdgeFoundList { get; set; }
        private List<BrepEdge> LastEdgeFoundList { get; set; }
        private List<int> CurrentFaceFoundIndexList { get; set; }
        private List<int> LastFaceFoundIndexList { get; set; }
        public List<BrepFace> DrawFaceFoundList { get; private set; }
        public bool IsAssistKeyDown { get; set; }
        public Point3d[] AssistPtList { get; private set; }
        private Dictionary<Curve, OctopusType> OctopusRaw { get; set; }
        private Dictionary<Curve, int> OctopusBaseRaw { get; set; }
        public Dictionary<Curve, string> OctopusCascade { get; private set; }
        private Dictionary<Curve, int> OctopusBaseCascade { get; set; }
        public List<Point3d> OctopusPtStocker { get; }
        public Dictionary<int, List<Curve>> OctopusArmStocker { get; }
        public KeyValuePair<Curve, int> OctopusCustom { get; set; }

        #endregion

        #region CTOR
        public Core(RhinoDoc doc)
        {
            this.currentDoc = doc;
            MethodBasic.ObjectCollecter(out this.currentBrep);

            this.OctopusArmStocker = new Dictionary<int, List<Curve>>();
            this.OctopusPtStocker = new List<Point3d>();

            foreach (BrepFace bFace in this.currentBrep.Faces)
            {
                int faceIndex = bFace.FaceIndex;
                List<Curve> crvListHolder = new List<Curve>();
                this.OctopusArmStocker.Add(faceIndex, crvListHolder);
            }
            
        }
        #endregion

        #region MTHD
        
        public void OctopusOverlapDispatcher()
        {
            if (this.OctopusPtStocker.Count == 0)
            {
                this.OctopusPtStocker.Add(this.LastPtCandidate);
            }
            else
            {
                int cascadeIndex = 1;
            
                Dialog<int> dispatchDialog = new Dialog<int>(){WindowStyle = WindowStyle.None};
                DynamicLayout dispatchLayout = new DynamicLayout();
                List<Button> candidateButtonList = new List<Button>();

                List<string> textList = new List<string>();
                List<int> indexList = new List<int>();
                textList.Add("_ITPL");
                indexList.Add(0);

                List<Curve> keyList = this.OctopusCascade.Keys.ToList();

                foreach (KeyValuePair<Curve, string> element in this.OctopusCascade)
                {
                    double distance = this.LastPtCandidate.DistanceTo(element.Key.PointAtEnd);
                    if (distance <= 100 * this.currentDoc.ModelAbsoluteTolerance)
                    {
                        textList.Add(element.Value);
                        indexList.Add(cascadeIndex);
                    }
                    cascadeIndex++;
                }
                for (int i = 0; i < indexList.Count; i++)
                {
                    Button candidateButton = new Button() { Text = textList[i], BackgroundColor = Eto.Drawing.Colors.White};
                    int j = i;
                    candidateButton.Click += (sender, e) => dispatchDialog.Close(indexList[j]);
                    candidateButtonList.Add(candidateButton);
                }
                // ReSharper disable once CoVariantArrayConversion
                dispatchLayout.AddColumn(candidateButtonList.ToArray());
                dispatchDialog.Content = dispatchLayout;

                if (indexList.Count == 1)
                {
                    this.OctopusPtStocker.Add(this.LastPtCandidate);
                }
                else
                {
                    RhinoApp.WriteLine("Overlapping detected, pick one curve to continue.");
                    int indexSelcted = dispatchDialog.ShowModal(RhinoEtoApp.MainWindow);
                    RhinoApp.WriteLine(indexSelcted.ToString());
                    this.OctopusPtStocker.Add(indexSelcted == 0
                        ? this.LastPtCandidate
                        : keyList[indexSelcted - 1].PointAtEnd);
                }
            }
            
            this.LastPt = this.OctopusPtStocker.Last();
        }
        
        public int CurrentEdgeFinder()
        {
            this.CurrentEdgeFoundList = new List<BrepEdge>();
            CurrentEdgeFoundList = MethodBasic.EdgeFinder(this.CurrentPt, this.currentBrep, this.currentDoc);
            return this.CurrentEdgeFoundList.Count;
        }
        private void CurrentFaceFinder()
        {
            this.CurrentFaceFoundIndexList = new List<int>();
            CurrentFaceFoundIndexList = MethodBasic.FaceFinder(this.CurrentEdgeFoundList);
        }

        public int LastEdgeFinder()
        {
            this.LastEdgeFoundList = new List<BrepEdge>();
            LastEdgeFoundList = MethodBasic.EdgeFinder(this.LastPt, this.currentBrep, this.currentDoc);
            return this.LastEdgeFoundList.Count;
        }

        private void LastFaceFinder()
        {
            this.LastFaceFoundIndexList = new List<int>();
            LastFaceFoundIndexList = MethodBasic.FaceFinder(this.LastEdgeFoundList);
        }

        private void DrawFaceGenerator()
        {
            this.DrawFaceFoundList = new List<BrepFace>();
            List<int> drawFaceFoundIndexList = this.LastFaceFoundIndexList == null 
                                             ? this.CurrentFaceFoundIndexList 
                                             : this.CurrentFaceFoundIndexList.Union(this.LastFaceFoundIndexList).ToList();
            foreach (int index in drawFaceFoundIndexList)
            {
                this.DrawFaceFoundList.Add(this.currentBrep.Faces[index]);
            }
        }

        public void AssistPtGenerator()
        {
            if (this.CurrentEdgeFoundList.Count == 1)
            {
                this.CurrentEdgeFoundList[0].DivideByCount(9, true, out Point3d[] cptTemp);
                this.AssistPtList = cptTemp;
            }
        }

        private void ProphetGenerator()
        {
            int currentPtStockedNumber = this.OctopusPtStocker.Count;
            if (currentPtStockedNumber == 0)
            {
                if (this.CurrentEdgeFoundList.Count != 1)
                {
                    return;
                }
                this.CurrentEdgeFoundList[0].ClosestPoint(this.CurrentPt, out double p);
                Vector3d axis = CurrentEdgeFoundList[0].TangentAt(p);
                this.ProphetPlane = new Plane(this.CurrentPt, axis);
            }
            else if (currentPtStockedNumber == 1)
            {
                Vector3d vec = this.CurrentPt - this.OctopusPtStocker[0];
                this.ProphetPlane = new Plane(this.CurrentPt, vec, Vector3d.XAxis);
                
            }
            else if (currentPtStockedNumber == 2)
            {
                this.ProphetPlane = new Plane(this.CurrentPt, this.OctopusPtStocker[0], this.OctopusPtStocker[1]);
            }

            Rhino.Geometry.Intersect.Intersection.BrepPlane(this.currentBrep,
                                                            this.ProphetPlane,
                                                            this.currentDoc.ModelAbsoluteTolerance,
                                                            out Curve[] intersecrtionCrvs,
                                                            out _);
            this.ProphetCrvs = intersecrtionCrvs;
        }

        private void ISOCrvGenerator()
        {
            foreach (BrepFace bFace in 
                     this.LastFaceFoundIndexList.Select
                         (bFaceIndex => this.currentBrep.Faces[bFaceIndex]))
            {
                bFace.ClosestPoint(this.LastPt, out double u, out double v);
                Curve[] isou = bFace.TrimAwareIsoCurve(1, u);
                Curve[] isov = bFace.TrimAwareIsoCurve(0, v);
                foreach (Curve iso in isou)
                {
                    MethodBasic.OctopusBundleCollector(iso, this.LastPt, this.currentBrep, this.currentDoc,
                        out Dictionary<Curve, OctopusType> octopusRawDic, 
                        OctopusType._ISOU);
                    foreach (KeyValuePair<Curve, OctopusType> element in octopusRawDic)
                    {
                        this.OctopusRaw.Add(element.Key, element.Value);
                        this.OctopusBaseRaw.Add(element.Key, bFace.FaceIndex);
                    }
                }
                foreach (Curve iso in isov)
                {
                    MethodBasic.OctopusBundleCollector(iso, this.LastPt, this.currentBrep, this.currentDoc,
                        out Dictionary<Curve, OctopusType> octopusRawDic, 
                        OctopusType._ISOV);
                    foreach (KeyValuePair<Curve, OctopusType> element in octopusRawDic)
                    {
                        this.OctopusRaw.Add(element.Key, element.Value);
                        this.OctopusBaseRaw.Add(element.Key, bFace.FaceIndex);
                    }
                }
            }
        }

        private void CPLCrvGenerator()
        {
            Plane currentCpl =
                this.currentDoc.Views.ActiveView.ActiveViewport.ConstructionPlane();
            if (currentCpl == Plane.Unset)
            {
                return;
            }
            Vector3d[] cplAxis = new[] { currentCpl.XAxis, currentCpl.YAxis, currentCpl.ZAxis };
            OctopusType[] cplType = new[] { OctopusType._CPLX, OctopusType._CPLY, OctopusType._CPLZ };

            Plane[] cplPlanes = new[]
            {
                new Plane(this.LastPt, cplAxis[0]),
                new Plane(this.LastPt, cplAxis[1]),
                new Plane(this.LastPt, cplAxis[2])
            };
            foreach (int bFaceIndex in this.LastFaceFoundIndexList)
            {
                int i = 0;
                BrepFace bFace = this.currentBrep.Faces[bFaceIndex];
                foreach (Plane cpl in cplPlanes)
                {
                    Rhino.Geometry.Intersect.Intersection.BrepPlane(bFace.DuplicateFace(false),
                                                                    cpl,
                                                                    this.currentDoc.ModelAbsoluteTolerance,
                                                                    out Curve[] intersecrtionCrvs,
                                                                    out _);
                    if (intersecrtionCrvs != null)
                    {
                        foreach (Curve crv in intersecrtionCrvs)
                        {
                            MethodBasic.OctopusBundleCollector(crv, this.LastPt, this.currentBrep, this.currentDoc,
                                                               out Dictionary<Curve, OctopusType> octopusRawDic, 
                                                               cplType[i]);
                            foreach (KeyValuePair<Curve, OctopusType> element in octopusRawDic)
                            {
                                this.OctopusRaw.Add(element.Key, element.Value);
                                this.OctopusBaseRaw.Add(element.Key, bFace.FaceIndex);
                            }
                        }
                    }
                    i++;
                }
            }
        }

        private void WPLCrvGenerator()
        {
            Plane[] wplPlanes = new[]
            {
                new Plane(this.LastPt, Vector3d.ZAxis), //z
                new Plane(this.LastPt, Vector3d.XAxis), //x
                new Plane(this.LastPt, Vector3d.YAxis)  //y
            };
            OctopusType[] wplType = new[] { OctopusType._WPLZ, OctopusType._WPLX, OctopusType._WPLY };
            foreach (int bFaceIndex in this.LastFaceFoundIndexList)
            {
                int i = 0;
                BrepFace bFace = this.currentBrep.Faces[bFaceIndex];
                foreach (Plane wpl in wplPlanes)
                {
                    Rhino.Geometry.Intersect.Intersection.BrepPlane(bFace.DuplicateFace(false),
                                                                    wpl,
                                                                    this.currentDoc.ModelAbsoluteTolerance,
                                                                    out Curve[] intersecrtionCrvs,
                                                                    out _);
                    if (intersecrtionCrvs != null)
                    {
                        foreach (Curve crv in intersecrtionCrvs)
                        {
                            MethodBasic.OctopusBundleCollector(crv, this.LastPt, this.currentBrep, this.currentDoc,
                                                               out Dictionary<Curve, OctopusType> octopusRawDic, 
                                                               wplType[i]);
                            foreach (KeyValuePair<Curve, OctopusType> element in octopusRawDic)
                            {
                                this.OctopusRaw.Add(element.Key, element.Value);
                                this.OctopusBaseRaw.Add(element.Key, bFace.FaceIndex);
                            }
                        }
                    }
                    i++;
                }
            }
        }

        private void OctopusRawGenerator()
        {
            this.OctopusRaw = new Dictionary<Curve, OctopusType>();
            this.OctopusBaseRaw = new Dictionary<Curve, int>();
        }

        private void OctopusCascader()
        {
            this.OctopusCascade = new Dictionary<Curve, string>();
            this.OctopusBaseCascade = new Dictionary<Curve, int>();
            
            List<Curve> octopusCrvList = this.OctopusRaw.Keys.ToList();
            List<OctopusType> octopusTypeList = this.OctopusRaw.Values.ToList();
            List<int> octopusBaseList = this.OctopusBaseRaw.Values.ToList();
            
            List<string> octopusTypeStrList = new List<string>();
            
            List<int> criminalIndex = new List<int>();
            List<int> allIndex = new List<int>();

            List<Curve> dicKeyList = this.OctopusRaw.Keys.ToList();
            List<OctopusType> dicValueList = this.OctopusRaw.Values.ToList();
            
            for (int i = 0; i < octopusCrvList.Count; i++)
            {
                allIndex.Add(i);
            }
            for (int i = 0; i < this.OctopusRaw.Count; i++)
            {
                string typeOriginal = octopusTypeList[i].ToString();
                string typeModifeid = "";
                typeModifeid += typeOriginal;
                for (int j = i + 1; j < this.OctopusRaw.Count; j++)
                {
                    bool isCrvIdentical = GeometryBase.GeometryEquals(dicKeyList[j], octopusCrvList[i]);
                    if (isCrvIdentical)
                    {
                        if (octopusTypeList[i] != dicValueList[j])
                        {
                            if (!typeModifeid.Contains(dicValueList[j].ToString()))
                            {
                                typeModifeid += dicValueList[j].ToString();
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
            foreach (int index in clearIndexList)
            {
                this.OctopusCascade.Add(octopusCrvList[index], octopusTypeStrList[index]);
                this.OctopusBaseCascade.Add(octopusCrvList[index], octopusBaseList[index]);
                
            }
        }

        public void OnMouseMoveTemplateBundle()
        {
            this.CurrentFaceFinder();
            this.DrawFaceGenerator();
            this.ProphetGenerator();

        }

        public void OctopusDrawBundle()
        {
            this.LastFaceFinder();
            this.OctopusRawGenerator();
            this.ISOCrvGenerator();
            this.CPLCrvGenerator();
            this.WPLCrvGenerator();
            this.OctopusCascader();
        }

        public void OctopusCustomGenerator()
        {
            foreach (int i in CurrentFaceFoundIndexList)
            {
                bool isPtOnSrf = MethodBasic.OnLoopFilter(this.currentBrep, i, this.CurrentPt, this.LastPt,
                                 this.currentDoc);
                if (isPtOnSrf)
                {
                    List<Point3d> ptList = new List<Point3d>(){this.LastPt, this.CurrentPt};
                    Curve octopusArmCustom = this.currentBrep.Faces[i].
                                             InterpolatedCurveOnSurface(ptList, this.currentDoc.ModelAbsoluteTolerance);
                    
                    this.OctopusCustom = new KeyValuePair<Curve, int>(octopusArmCustom, i);
                    break;
                }
            }
            
        }
        
        public void OctopusArmCollector(KeyValuePair<Curve, int> pair)
        {
            
        }

        #endregion

    }

    internal class GetPointTemplate : GetPoint
    {
        protected Core coreObj;
        protected GetPointTemplate(Core coreobjPassed)
        {
            coreObj = coreobjPassed;
            this.PermitElevatorMode(0);
        }

        protected override void OnMouseMove(GetPointMouseEventArgs e)
        {
            coreObj.IsAssistKeyDown = e.ShiftKeyDown & e.ControlKeyDown;
            coreObj.CurrentPt = e.Point;
            
            int isPtOnEdge = coreObj.CurrentEdgeFinder();
            
            if (isPtOnEdge > 0)
            {
                coreObj.OnMouseMoveTemplateBundle();
            }
            
            base.OnMouseMove(e);
        }

        protected override void OnDynamicDraw(GetPointDrawEventArgs e)
        {
            Rhino.Display.DisplayMaterial mtl = new Rhino.Display.DisplayMaterial(
                                                Rhino.ApplicationSettings.AppearanceSettings.SelectedObjectColor, 
                                                0.5);
            if (coreObj.ProphetCrvs != null)
            {
                foreach (Curve crv in coreObj.ProphetCrvs)
                {
                    e.Display.DrawCurve(crv, System.Drawing.Color.Chartreuse, 3);
                }
            }
            
            foreach (BrepFace bFace in coreObj.DrawFaceFoundList)
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
            
            e.Display.DrawPoint(coreObj.CurrentPt, Rhino.Display.PointStyle.RoundControlPoint, 5, System.Drawing.Color.Black);
            
            base.OnDynamicDraw(e);
        }
    }

    internal class GetFirstPoint : GetPointTemplate
    {
        public GetFirstPoint(Core coreobjPassed) : base(coreobjPassed)
        {
            coreObj = coreobjPassed;
        }
    }

    internal class GetNextPoint : GetPointTemplate
    {
        public GetNextPoint(Core coreobjPassed) : base(coreobjPassed)
        {
            coreObj = coreobjPassed;
            coreObj.OctopusOverlapDispatcher();

            int isLastPtOnCrv = coreObj.LastEdgeFinder();
            if (isLastPtOnCrv > 0)
            {
                coreObj.OctopusDrawBundle();
            }
            
        }

        protected override void OnMouseMove(GetPointMouseEventArgs e)
        {
            coreObj.OctopusCustomGenerator();
            base.OnMouseMove(e);
        }

        protected override void OnDynamicDraw(GetPointDrawEventArgs e)
        {
            foreach (Curve crv in coreObj.OctopusCascade.Keys)
            {
                e.Display.DrawCurve(crv, System.Drawing.Color.Blue, 3);
            }

            foreach (KeyValuePair<Curve, string> element in coreObj.OctopusCascade)
            {
                e.Display.Draw2dText(element.Value, System.Drawing.Color.Blue, element.Key.PointAtEnd, false, 14);
                e.Display.DrawPoint(element.Key.PointAtEnd, Rhino.Display.PointStyle.RoundControlPoint, 5, System.Drawing.Color.Blue);
                this.AddConstructionPoint(element.Key.PointAtEnd);
            }

            if (coreObj.OctopusCustom.Key != null)
            {
                e.Display.DrawCurve(coreObj.OctopusCustom.Key, System.Drawing.Color.Blue, 3);
            }

            base.OnDynamicDraw(e);
        }
    }

    internal static class Watchdog
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

    internal enum OctopusType
    {
        _ISOU, _ISOV, _CPLX, _CPLY, _CPLZ, _WPLX, _WPLY, _WPLZ
    }
}
