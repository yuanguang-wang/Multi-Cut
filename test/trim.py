import Rhino
import scriptcontext as sc
import System
import rhinoscriptsyntax as rs

def test_command():
    srf = rs.coercesurface(rs.GetObject("srf"))
    pt1 = rs.coerce3dpoint(rs.GetObject("pt1"))
    pt2 = rs.coerce3dpoint(rs.GetObject("pt2"))
    ptlist = []
    ptlist.append(pt1)
    ptlist.append(pt2)
    linecurve = Rhino.Geometry.LineCurve(pt1, pt2)
    
    crv = srf.Pullback(linecurve, 1.00)
    sc.doc.Objects.Add(crv)
    sc.doc.Views.Redraw()

if __name__ == "__main__":
    test_command() # Call the function defined above