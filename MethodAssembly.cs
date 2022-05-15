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
        public Rhino.Geometry.Curve[] IntersectionedCurves { get; set; }

        public bool ObjectCollecter(string commandPrompt, out Rhino.Geometry.Brep brep)
        {
            Rhino.Input.Custom.GetObject getObject = new Rhino.Input.Custom.GetObject
            {
                GeometryFilter = Rhino.DocObjects.ObjectType.Brep 
            };
            getObject.SetCommandPrompt(commandPrompt);
            getObject.Get();
            if (getObject.CommandResult() != Rhino.Commands.Result.Success)
            {
                brep = null;
                return false;
            }
            Rhino.DocObjects.ObjRef objRef = getObject.Object(0);
            Rhino.Geometry.Brep brep2bpassed = objRef.Brep();
            if (brep2bpassed != null)
            {
                brep = brep2bpassed;
                return true;
            }
            else
            {
                brep = null;
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
