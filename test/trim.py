import Rhino
import scriptcontext as sc
import System
import rhinoscriptsyntax as rs

def test_command():
    brep = []
    objid = rs.GetObjects()
    for i in range(len(objid)):
        brep.append(rs.coercebrep(objid[i]))
        print(brep[i])

    #brep[0].Trim(brep[1],0.1)
    
    sc.doc.Views.Redraw()


if __name__ == "__main__":
    test_command() # Call the function defined above