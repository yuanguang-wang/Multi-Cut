﻿using Rhino;
using Rhino.Commands;
using Rhino.Input;

namespace MultiCut
{
    public class MultiCutCommand : Command
    {
        #region ATTR
        public MultiCutCommand()
        {
            Instance = this;
        }
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static MultiCutCommand Instance { get; private set; }
        public override string EnglishName => "mct";
        
        #endregion

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Core coreObj = new Core(doc);
            if (coreObj.CollectionResult == false)
            {
                return Result.Cancel;
            }
            
            GetFirstPoint getFirstPoint = new GetFirstPoint(coreObj);
            GetResult rsFpt = getFirstPoint.Get();
            if (rsFpt != GetResult.Point) // !Mouse Down
            {
                if (rsFpt == GetResult.Nothing) // Press Enter
                {
                    coreObj.CutOperation();
                    doc.Views.Redraw();
                    return Result.Success;
                } 
                else // Press ESC
                {
                    RhinoApp.WriteLine("Command EXIT");
                    return Result.Cancel;
                }
            }
            
            coreObj.LastPtCandidate = getFirstPoint.Point();
            while (true)
            {
                int ptCollectedCount = coreObj.OnLoopList.Count;
                if (ptCollectedCount >= 2)
                {
                    if (coreObj.OnLoopList.Contains(false))
                    {
                        break;
                    }
                }
                
                GetNextPoint getNextPoint = new GetNextPoint(coreObj);
                GetResult rsNpt = getNextPoint.Get();
                if (rsNpt != GetResult.Point) // !Mouse Down
                {
                    if (rsNpt == GetResult.Nothing) // Press Enter
                    {
                        break;
                    } 
                    else // Press ESC
                    {
                        RhinoApp.WriteLine("Command EXIT");
                        return Result.Cancel;
                    }
                }
                coreObj.LastPtCandidate = getNextPoint.Point();

            }
            coreObj.CutOperation();
            
            doc.Views.Redraw();
            return Result.Success;
        }
    }

    public class PreferenceCommand : Command
    {
        #region ATTR
        public PreferenceCommand()
        {
            Instance = this;
        }
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static PreferenceCommand Instance { get; private set; }
        public override string EnglishName => "mcp";
        
        #endregion

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            MultiCutPreference mcpObj = MultiCutPreference.Instance;
            PreferenceForm formObj = new PreferenceForm();
            
            formObj.Show();
            
            mcpObj.IsBrepSplitted = true;
            
            return Result.Success;
        }
    }
}
