using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICtrlConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            ICtrlDLL.ICtrl iCtrl = ICtrlDLL.ICtrl.Instance;
            iCtrl.Run();

            while (true)
            {
                Console.ReadKey();

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
