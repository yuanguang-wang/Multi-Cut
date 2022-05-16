import Rhino
import scriptcontext as sc
import System
import rhinoscriptsyntax as rs

def test_command():
    gp = Rhino.Input.Custom.GetPoint()
    gp.EnableCurveSnapArrow(True,False)
    gp.Get()
    
    sc.doc.Views.Redraw()


if __name__ == "__main__":
    test_command() # Call the function defined above