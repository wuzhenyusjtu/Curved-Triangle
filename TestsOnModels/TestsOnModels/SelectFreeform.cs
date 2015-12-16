using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsOnModels
{
    class SelectFreeform : Snap.UI.BlockForm
    {
        private Snap.UI.Block.SelectObject selectFreeform;

        public SelectFreeform()
        {
            Title = "Select Freeform";
            string cue = " Please Select the Freeform";
            string label = "Select Freeform";

            selectFreeform = new Snap.UI.Block.SelectObject(cue, label);
            Snap.NX.ObjectTypes.SubType[] types = { Snap.NX.ObjectTypes.SubType.FaceBsurface };
            selectFreeform.SetFaceFilter(types);
            selectFreeform.AllowMultiple = false;
            selectFreeform.StepStatus = Snap.UI.Block.StepStatus.Required;

            AddBlocks(selectFreeform);
        }

        public override void OnApply()
        {
            Snap.NX.NXObject[] bodies = selectFreeform.SelectedObjects;
            TestOnFreeform testOnFreeform = new TestOnFreeform((Snap.NX.Face)bodies[0], 2);
            base.OnApply();
        }
    }
}
