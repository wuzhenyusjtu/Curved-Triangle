using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsOnModels
{
    class SelectHump :Snap.UI.BlockForm
    {
        private Snap.UI.Block.SelectObject selectHump;

        public SelectHump()
        {
            Title = "Select Hump";
            string cue = " Please Select the Hump";
            string label = "Select Hump";

            selectHump = new Snap.UI.Block.SelectObject(cue, label);
            Snap.NX.ObjectTypes.SubType[] types = { Snap.NX.ObjectTypes.SubType.FaceBsurface };
            selectHump.SetFaceFilter(types);
            selectHump.AllowMultiple = false;
            selectHump.StepStatus = Snap.UI.Block.StepStatus.Required;

            AddBlocks(selectHump);
        }

        public override void OnApply()
        {
            Snap.NX.NXObject[] bodies = selectHump.SelectedObjects;
            TestOnHump testOnHump = new TestOnHump((Snap.NX.Face)bodies[0], 2);
            base.OnApply();
        }
    }
}
