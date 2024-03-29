﻿using System.Collections.Generic;
using System.Linq;

using Rhino;
using Rhino.Geometry;
using Rhino.Input.Custom;

using Eto.Forms;
using Rhino.DocObjects;
using Rhino.UI;

namespace MultiCut
{
    internal static class MethodCollection
    {

        #region Core Method
        
        public static bool ObjectCollecter(out Brep brep2BPassed, out ObjRef brepRef2BPassed)
        {
            brep2BPassed = null;
            brepRef2BPassed = null;
            GetObject getObject = new GetObject
            {
                GeometryFilter = ObjectType.Brep 
            };
            getObject.SetCommandPrompt("Select Brep to be Cut.");
            getObject.Get();
            if (getObject.CommandResult() != Rhino.Commands.Result.Success)
            {
                RhinoApp.WriteLine("Command EXIT");
                return false;
            }
            brepRef2BPassed = getObject.Object(0);
            brep2BPassed = brepRef2BPassed.Brep();

            return !(brep2BPassed == null | brepRef2BPassed == null);
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

        public static System.Drawing.Color ColorDesaturate(System.Drawing.Color color)
        {
            byte R = color.R;
            byte G = color.G;
            byte B = color.B;
            color.GetSaturation();
            System.Drawing.Color newColor = System.Drawing.Color.FromArgb(ByteConvert(R), ByteConvert(G), ByteConvert(B));
            return newColor;
        }

        private static int ByteConvert(byte original)
        {
            int value = original + 100;
            if (value > 255)
            {
                value = 255;
            }
            return value;
        }

        #endregion
        #region Preference Method

        public static bool DoubleCheck(bool? upperCheck, bool? localCheck)
        {
            // ReSharper disable once PossibleInvalidOperationException
            bool upperCast = (bool)upperCheck;
            // ReSharper disable once PossibleInvalidOperationException
            bool localCast = (bool)localCheck;
            return upperCast && localCast;
        }

        public static bool SafeCast(bool? boolPassed)
        {
            // ReSharper disable once PossibleInvalidOperationException
            return (bool)boolPassed;
        }

        public static System.Drawing.Color SetMCTUIColor(
            bool? thisChecked,
            bool? colorChecked,
            System.Drawing.Color dbColor,
            System.Drawing.Color defaultColor
        )
        {
            bool colorDoubleCheck = DoubleCheck(thisChecked,colorChecked);
            System.Drawing.Color mctColor = colorDoubleCheck ? dbColor : defaultColor;
            return mctColor;
        }

        public static int SetMCTUIWidth(
            bool? thisChecked,
            bool? widthChecked,
            int dbWidth,
            int defaultWidth
        )
        {
            bool widthDoubleCheck = DoubleCheck(thisChecked, widthChecked);
            int mctWidth = widthDoubleCheck ? dbWidth : defaultWidth;
            return mctWidth;
        }

        #endregion
        #region Command Method

        public static void SyncCommandOpt(GetPointTemplate gp)
        {
            bool splitValue = gp.SplitOpt.CurrentValue;
            SplitCheck.Instance.Checked = splitValue;
            MultiCutPreference.Instance.IsSplitEnabled = splitValue;
            MultiCutPlugin.Instance.Settings.SetBool(SettingKey.General_SplitCheck,splitValue);

            bool enableValue = gp.APOpt.CurrentValue;
            APEnableCheck.Instance.Checked = enableValue;
            MultiCutPreference.Instance.IsPointEnabled = enableValue;
            MultiCutPlugin.Instance.Settings.SetBool(SettingKey.AssistantPoint_EnableCheck, enableValue);

            int selectedIndex = gp.APInt.CurrentValue - 2;
            APDropDown.Instance.SelectedIndex = selectedIndex;
            MultiCutPreference.Instance.PointNumber = selectedIndex + 2;
            MultiCutPlugin.Instance.Settings.SetInteger(SettingKey.AssistantPoint_PointNumber, selectedIndex);
        }

        #endregion
    }

    public class Core
    { 
        #region FIELD

        private readonly RhinoDoc currentDoc;
        private readonly Brep baseBrep;
        private readonly ObjRef baseBrepRef;
        private readonly MultiCutPreference McPref = MultiCutPreference.Instance;

        #endregion
        #region ATTR

        public bool CollectionResult { get; }
        public Curve[] ProphetCrvs { get; private set; }
        private Plane ProphetPlane { get; set; }
        public Point3d CurrentPt { get; set; }
        private Point3d LastPt { get; set; }
        private List<BrepEdge> CurrentEdgeFoundList { get; set; }
        private List<BrepEdge> LastEdgeFoundList { get; set; }
        private List<int> CurrentFaceFoundIndexList { get; set; }
        private List<int> LastFaceFoundIndexList { get; set; }
        public Point3d[] AssistPtList { get; private set; }
        private Dictionary<Curve, OctopusType> OctopusRaw { get; set; }
        public Dictionary<Curve, string> OctopusCascade { get; private set; }
        private List<Point3d> OctopusPtStocker { get; }
        public List<Curve> OctopusArmStocker { get; }
        public Curve OctopusCustom { get; private set; }
        public List<bool> OnLoopList { get; }

        #endregion
        #region CTOR
        
        public Core(RhinoDoc currentDoc)
        {
            this.currentDoc = currentDoc;
            this.CollectionResult = MethodCollection.ObjectCollecter(out this.baseBrep, out this.baseBrepRef);

            this.OctopusArmStocker = new List<Curve>();
            this.OctopusPtStocker = new List<Point3d>();
            this.OnLoopList = new List<bool>();

        }
        
        #endregion
        #region DISPATCH
        
        private void OctopusOverlapDispatcher()
        {
            if (this.OctopusPtStocker.Count == 0)
            {
                this.OctopusPtStocker.Add(this.CurrentPt);
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
                    double distance = this.CurrentPt.DistanceTo(element.Key.PointAtEnd);
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
                    this.OctopusPtStocker.Add(this.CurrentPt);
                    this.OctopusArmCollector(this.OctopusCustom);
                }
                else
                {
                    RhinoApp.WriteLine("Overlapping detected, pick one curve to continue.");
                    dispatchDialog.Load += (sender, args) => dispatchDialog.Location = new Eto.Drawing.Point(Mouse.Position);
                    int indexSelcted = dispatchDialog.ShowModal(RhinoEtoApp.MainWindow);

                    Curve targetCrv = keyList[indexSelcted - 1];
                    
                    this.OctopusPtStocker.Add(indexSelcted == 0
                        ? this.CurrentPt
                        : targetCrv.PointAtEnd);
                    this.OctopusArmCollector(targetCrv);
                }
            }
            
            this.LastPt = this.OctopusPtStocker.Last();
        }
        
        private void IsPtOnLoopDispatcher()
        {
            // decide to stop the new gp generation when press enter (3 pt above).
            foreach (bool isPtOnSrf in 
                     this.CurrentFaceFoundIndexList.Select
                        (faceIndex => MethodCollection.OnLoopFilter
                            (this.baseBrep, faceIndex, this.CurrentPt, this.LastPt, this.currentDoc)
                        )
                     )
            {
                this.OnLoopList.Add(isPtOnSrf);
                break;
            }
        }

        
        #endregion
        #region FIND
        
        public int CurrentEdgeFinder()
        {
            this.CurrentEdgeFoundList = new List<BrepEdge>();
            CurrentEdgeFoundList = MethodCollection.EdgeFinder(this.CurrentPt, this.baseBrep, this.currentDoc);
            return this.CurrentEdgeFoundList.Count;
        }
        private void CurrentFaceFinder()
        {
            this.CurrentFaceFoundIndexList = new List<int>();
            CurrentFaceFoundIndexList = MethodCollection.FaceFinder(this.CurrentEdgeFoundList);
        }

        private int LastEdgeFinder()
        {
            this.LastEdgeFoundList = new List<BrepEdge>();
            LastEdgeFoundList = MethodCollection.EdgeFinder(this.LastPt, this.baseBrep, this.currentDoc);
            return this.LastEdgeFoundList.Count;
        }

        private void LastFaceFinder()
        {
            this.LastFaceFoundIndexList = new List<int>();
            LastFaceFoundIndexList = MethodCollection.FaceFinder(this.LastEdgeFoundList);
        }
        
        #endregion
        #region GENERATE

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

            Rhino.Geometry.Intersect.Intersection.BrepPlane(this.baseBrep,
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
                         (bFaceIndex => this.baseBrep.Faces[bFaceIndex]))
            {
                bFace.ClosestPoint(this.LastPt, out double u, out double v);
                Curve[] isou = bFace.TrimAwareIsoCurve(1, u);
                Curve[] isov = bFace.TrimAwareIsoCurve(0, v);
                foreach (Curve iso in isou)
                {
                    MethodCollection.OctopusBundleCollector(iso, this.LastPt, this.baseBrep, this.currentDoc,
                        out Dictionary<Curve, OctopusType> octopusRawDic, 
                        OctopusType._ISOU);
                    foreach (KeyValuePair<Curve, OctopusType> element in octopusRawDic)
                    {
                        this.OctopusRaw.Add(element.Key, element.Value);
                    }
                }
                foreach (Curve iso in isov)
                {
                    MethodCollection.OctopusBundleCollector(iso, this.LastPt, this.baseBrep, this.currentDoc,
                        out Dictionary<Curve, OctopusType> octopusRawDic, 
                        OctopusType._ISOV);
                    foreach (KeyValuePair<Curve, OctopusType> element in octopusRawDic)
                    {
                        this.OctopusRaw.Add(element.Key, element.Value);
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
                BrepFace bFace = this.baseBrep.Faces[bFaceIndex];
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
                            MethodCollection.OctopusBundleCollector(crv, this.LastPt, this.baseBrep, this.currentDoc,
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
                BrepFace bFace = this.baseBrep.Faces[bFaceIndex];
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
                            MethodCollection.OctopusBundleCollector(crv, this.LastPt, this.baseBrep, this.currentDoc,
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
        }
        
        private void OctopusCustomGenerator()
        {
            foreach (int i in CurrentFaceFoundIndexList)
            {
                bool isPtOnSrf = MethodCollection.OnLoopFilter(this.baseBrep, i, this.CurrentPt, this.LastPt,
                    this.currentDoc);
                if (isPtOnSrf)
                {
                    List<Point3d> ptList = new List<Point3d>(){this.LastPt, this.CurrentPt};
                    Curve octopusArmCustom = this.baseBrep.Faces[i].
                        InterpolatedCurveOnSurface(ptList, this.currentDoc.ModelAbsoluteTolerance);
                    
                    this.OctopusCustom = octopusArmCustom;
                    break;
                }
            }
        }
        
        public void AssistPtGenerator()
        {
            if (!McPref.IsPointEnabled)
            {
                return;
            }
            if (this.CurrentEdgeFoundList.Count == 1)
            {
                this.CurrentEdgeFoundList[0].DivideByCount(McPref.PointNumber, true, out Point3d[] cptTemp);
                this.AssistPtList = cptTemp;
            }
        }
        
        #endregion
        #region COLLECT

        private void OctopusRawGenerator()
        {
            this.OctopusRaw = new Dictionary<Curve, OctopusType>();
        }

        private void OctopusCascader()
        {
            this.OctopusCascade = new Dictionary<Curve, string>();
            
            List<Curve> octopusCrvList = this.OctopusRaw.Keys.ToList();
            List<OctopusType> octopusTypeList = this.OctopusRaw.Values.ToList();
            
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
            }
        }

        private void OctopusArmCollector(Curve crvCandidate)
        {
            bool isCrvExist = false;
            if (this.OctopusArmStocker.Count > 0)
            {
                foreach (var crv in this.OctopusArmStocker)
                {
                    isCrvExist |= GeometryBase.GeometryEquals(crv, crvCandidate);
                }
            }
            if (isCrvExist == false)
            {
                this.OctopusArmStocker.Add(crvCandidate);
            }
        }
        
        #endregion
        #region BUNDLE
        
        public void MouseMoveBaseBundle()
        {
            this.CurrentFaceFinder();
            if (this.McPref.IsProphetEnabled)
            {
                this.ProphetGenerator();
            }
            
        }

        public void MouseMoveNextBundle()
        {
            this.OctopusCustomGenerator();

        }

        public void OctopusRunBundle()
        {
            int isLastPtOnCrv = this.LastEdgeFinder();
            if (!(isLastPtOnCrv > 0))
            {
                return;
            }
            
            bool trinityBool = !McPref.IsIsoChecked & !McPref.IsCplChecked & !McPref.IsWplChecked;
            if (!McPref.IsOctopusEnabled | trinityBool)
            {
                this.OctopusCascade = new Dictionary<Curve, string>();
                return;
            }
            this.LastFaceFinder();
            this.OctopusRawGenerator();
            if (McPref.IsIsoChecked)
            {
                this.ISOCrvGenerator(); 
            }
            if (McPref.IsCplChecked)
            {
                this.CPLCrvGenerator(); 
            }
            if (McPref.IsWplChecked)
            {
                this.WPLCrvGenerator(); 
            } 
            this.OctopusCascader();
        }

        public void GetPtDispatchBundle()
        {
            this.OctopusOverlapDispatcher();
            this.IsPtOnLoopDispatcher();
        }
        
        #endregion
        #region CUT
        
        private void CutByCrvList(IEnumerable<Curve> crvCollection)
        {
            List<Curve> crvList = crvCollection.ToList();
            if (crvList.Count == 0)
            {
                RhinoApp.WriteLine("Cut curves collection is empty");
                return;
            }
            
            Brep[] newBrepArray = this.baseBrep.Split(crvList, this.currentDoc.ModelAbsoluteTolerance);
            
            if (newBrepArray.Length == 0)
            {
                RhinoApp.WriteLine("Brep cannot be cut");
                foreach (Curve crv in crvList)
                {
                    this.currentDoc.Objects.AddCurve(crv);
                }
            }
            else if (newBrepArray.Length == 1)
            {
                this.currentDoc.Objects.Replace(this.baseBrepRef, newBrepArray[0]);
            }
            else
            {
                if (this.McPref.IsSplitEnabled)
                {
                    RhinoApp.WriteLine("-------------");
                    RhinoApp.WriteLine("Brep Splitted");
                    RhinoApp.WriteLine("-------------");
                    foreach (Brep brep in newBrepArray)
                    {
                        RhinoObject brepObj = this.baseBrepRef.Object();
                        ObjectAttributes BrepAttr = brepObj.Attributes;
                        this.currentDoc.Objects.AddBrep(brep, BrepAttr);
                    }
                    this.currentDoc.Objects.Delete(this.baseBrepRef.Object());
                }
                else
                {
                    Brep[] tryJoinBrepArray = Brep.JoinBreps(newBrepArray,this.currentDoc.ModelAbsoluteTolerance);
                    if (tryJoinBrepArray.Length == 0)
                    {
                        RhinoApp.WriteLine("Try join brep failed");
                    }
                    else if (tryJoinBrepArray.Length == 1)
                    {
                        this.currentDoc.Objects.Replace(this.baseBrepRef, tryJoinBrepArray[0]);
                    }
                    else
                    {
                        RhinoApp.WriteLine("More than one brep is generated");
                        foreach (Brep brep in tryJoinBrepArray)
                        {
                            RhinoObject brepObj = this.baseBrepRef.Object();
                            ObjectAttributes BrepAttr = brepObj.Attributes;
                            this.currentDoc.Objects.AddBrep(brep, BrepAttr);
                        }
                        this.currentDoc.Objects.Delete(this.baseBrepRef.Object());
                    }
                }
            }
        }
        
        private void CutByProphet()
        {
            if (this.ProphetCrvs != null)
            {
                this.CutByCrvList(this.ProphetCrvs);
            }
            else
            {
                RhinoApp.WriteLine("No Prediction Line detected.");
            }
        }
        
        private void CutByOctopus()
        {
            if (this.OctopusArmStocker != null)
            {
                this.CutByCrvList(this.OctopusArmStocker);
            }
            else
            {
                RhinoApp.WriteLine("No Assistent Line detected.");
            }
        }

        public void CutOperation()
        {
            if (McPref.IsPriorityEnabled & McPref.IsProphetEnabled)
            {
                this.CutByProphet();
            }
            else
            {
                this.CutByOctopus();
            }
        }

        #endregion
    }

    internal class GetPointTemplate : GetPoint
    {
        protected Core coreObj;
        protected MultiCutPreference McPref => MultiCutPreference.Instance;
        private MultiCutPlugin McPlugin => MultiCutPlugin.Instance;
        public OptionToggle SplitOpt { get; }
        public OptionToggle APOpt { get; }
        public OptionInteger APInt { get; }

        protected GetPointTemplate(Core coreobjPassed)
        {
            coreObj = coreobjPassed;
            
            this.PermitElevatorMode(0);
            this.AcceptNothing(true);
            
            this.SplitOpt = new OptionToggle(McPlugin.Settings.GetBool(SettingKey.General_SplitCheck), 
                "KeepBrepJoined",
                "SplitWhenPossible");
            OptionToggle splitOptTemp = this.SplitOpt;
            this.AddOptionToggle("Split", ref splitOptTemp);
            this.SplitOpt = splitOptTemp;
            
            this.APOpt = new OptionToggle(McPlugin.Settings.GetBool(SettingKey.AssistantPoint_EnableCheck), 
                "Disable",
                "Enable");
            OptionToggle apEnableTemp = this.APOpt;
            this.AddOptionToggle("AssistantPoint", ref apEnableTemp);
            this.APOpt = apEnableTemp;
            
            this.APInt = new OptionInteger(McPlugin.Settings.GetInteger(SettingKey.AssistantPoint_PointNumber) + 2, 
                2,
                20);
            OptionInteger apIntTemp = this.APInt;
            this.AddOptionInteger("Division", ref apIntTemp);
            this.APInt = apIntTemp;
            
        }

        protected override void OnMouseMove(GetPointMouseEventArgs e)
        {
            coreObj.CurrentPt = e.Point;
            
            int isPtOnEdge = coreObj.CurrentEdgeFinder();
            if (isPtOnEdge > 0)
            {
                coreObj.MouseMoveBaseBundle();
            }
            
            base.OnMouseMove(e);
        }

        protected override void OnDynamicDraw(GetPointDrawEventArgs e)
        {
            base.OnDynamicDraw(e);
            
            if (coreObj.ProphetCrvs != null)
            {
                foreach (Curve crv in coreObj.ProphetCrvs)
                {
                    e.Display.DrawCurve(crv, this.McPref.ProphetColor, this.McPref.ProphetWidth);
                }
            }
            
            coreObj.AssistPtGenerator();
            if (coreObj.AssistPtList != null)
            {
                this.AddConstructionPoints(coreObj.AssistPtList);
                e.Display.DrawPoints(coreObj.AssistPtList, Rhino.Display.PointStyle.RoundControlPoint, McPref.PointSize, McPref.PointColor);
                int i = 0;
                foreach (Point3d pt in coreObj.AssistPtList)
                {
                    string serialNum = i + "/" + (coreObj.AssistPtList.Length - 1);
                    e.Display.Draw2dText(serialNum, McPref.PointColor,pt, false,16);
                    i++;
                }
            }
            
            e.Display.DrawPoint(coreObj.CurrentPt, Rhino.Display.PointStyle.RoundControlPoint, McPref.PointSize, System.Drawing.Color.Black);
            
        }
    }

    internal class GetFirstPoint : GetPointTemplate
    {
        public GetFirstPoint(Core coreobjPassed) : base(coreobjPassed)
        {
            coreObj = coreobjPassed;
            this.SetCommandPrompt("Pick the first point, or press ENTER to finish cut");

        }
    }

    internal class GetNextPoint : GetPointTemplate
    {
        public GetNextPoint(Core coreobjPassed) : base(coreobjPassed)
        {
            coreObj = coreobjPassed;
            coreObj.GetPtDispatchBundle();
            coreObj.OctopusRunBundle();
            
            this.SetCommandPrompt("Pick the next point, or press ENTER to finish cut");

        }

        protected override void OnMouseMove(GetPointMouseEventArgs e)
        {
            coreObj.MouseMoveNextBundle();
            base.OnMouseMove(e);
        }

        protected override void OnDynamicDraw(GetPointDrawEventArgs e)
        {
            base.OnDynamicDraw(e);
            
            if (coreObj.OctopusCascade != null)
            {
                foreach (Curve crv in coreObj.OctopusCascade.Keys)
                {
                    e.Display.DrawCurve(crv, this.McPref.OctopusColor, this.McPref.OctopusWidth);
                }
                foreach (KeyValuePair<Curve, string> element in coreObj.OctopusCascade)
                {
                    e.Display.Draw2dText(element.Value, this.McPref.OctopusColor, element.Key.PointAtEnd, false, 16);
                    e.Display.DrawPoint(element.Key.PointAtEnd, Rhino.Display.PointStyle.RoundControlPoint, McPref.PointSize, this.McPref.OctopusColor);
                    this.AddConstructionPoint(element.Key.PointAtEnd);
                }
            }
            
            if (coreObj.OctopusCustom != null)
            {
                e.Display.DrawCurve(coreObj.OctopusCustom, this.McPref.OctopusColor,  this.McPref.OctopusWidth);
            }
            
            foreach (Curve crv in coreObj.OctopusArmStocker)
            {
                e.Display.DrawCurve(crv, MethodCollection.ColorDesaturate(this.McPref.OctopusColor),  this.McPref.OctopusWidth);
            }

        }
    }

    internal enum OctopusType
    {
        _ISOU, _ISOV, _CPLX, _CPLY, _CPLZ, _WPLX, _WPLY, _WPLZ
    }

}
