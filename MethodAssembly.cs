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

    static class Core
    {
        public static Rhino.Geometry.Curve[] IntersectionedCurves { get; set; }
        public static bool DrawAutomatic(Rhino.Geometry.Brep brep, Rhino.Geometry.Plane plane)
        {
            Rhino.Geometry.Intersect.Intersection.BrepPlane(brep, plane, 0.01, out Rhino.Geometry.Curve[] intersectionCurves, out Rhino.Geometry.Point3d[] intersectionPoints);
            IntersectionedCurves = intersectionCurves;
            return true;
        }

        public static void DrawAutomaticPublisher(object sender, Rhino.Input.Custom.GetPointDrawEventArgs e)
        {
            foreach (Rhino.Geometry.Curve crv in IntersectionedCurves)
            {
                e.Display.DrawCurve(crv, System.Drawing.Color.Red, 5);
            }
            
        }
    }
}
