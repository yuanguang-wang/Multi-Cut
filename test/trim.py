import Rhino
import scriptcontext as sc
import System
import rhinoscriptsyntax as rs

def test_command():
    list = ["_ISOV","_CPLZ","_WPLX"]
    test = Rhino.UI.Dialogs.ShowMultiListBox("title","select the point", list)
    print(test)
    crv = rs.coercecurve(rs.GetObject())
    mp = crv.PointAtLength(1400.0)
    gp = Rhino.Input.Custom.GetPoint()
    gp.AddConstructionPoint(mp)
    #gp.EnableCurveSnapArrow(True,False)
    gp.Get()
    sc.doc.Objects.AddPoint(mp)
    sc.doc.Views.Redraw()

if __name__ == "__main__":
    test_command() # Call the function defined above