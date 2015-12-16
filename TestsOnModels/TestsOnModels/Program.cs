using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsOnModels
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Debugger.Launch();
            //################## We need to select the ready-made freeform, the part file is included in the project package ####################
            //SelectFreeform selectFreeform = new SelectFreeform();
            //selectFreeform.Show();

            //################## We need to select the ready-made hump, the part file is included in the project package ####################
            //SelectHump selectHump = new SelectHump();
            //selectHump.Show();

            //################## We dont't need to select anything, NX will create the test models for us. ####################
            //TestOnTorus testOnTorus = new TestOnTorus(Snap.Position.Origin, Snap.Vector.AxisZ, 30, 10, 2);
            TestOnSphere testOnSphere = new TestOnSphere(Snap.Position.Origin, 50, 2); 
        }

        public static int GetUnloadOption(string dummy)
        {
            return (int)Snap.UnloadOption.Immediately;
        }
    }
}
