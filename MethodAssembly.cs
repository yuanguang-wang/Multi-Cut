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

    class Core
    {
        #region ATTR
        public Rhino.Geometry.Curve[] CutterCrvs { get; set; }
        public Rhino.Geometry.Brep BrepSource { get; set; }
        public List<Rhino.Geometry.Point3d> Pts_List { get; set; }
        public Rhino.Geometry.Collections.BrepEdgeList BEdges_List { get; set; }
    
        #endregion

        public bool ObjectCollecter(string commandPrompt)
        {
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
            Rhino.Geometry.Brep brep2bPassed = objRef.Brep();
            if (brep2bPassed != null)
            {
                this.BrepSource = brep2bPassed;
                this.BEdges_List = brep2bPassed.Edges;
                return true;
            }
            else
            {
                return false;
            }
            
        }

    }

    static class Failsafe
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
}
