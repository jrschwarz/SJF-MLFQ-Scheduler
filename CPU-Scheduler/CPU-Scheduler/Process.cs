using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU_Scheduler
{
    class Process
    {
        private int[] _processes;

        public Process()
        {
            _processes = new int[] { }; // initialize with no processes
        }
        public void AddProcesses(int[] inputProcesses)
        {
            _processes = inputProcesses;
        }

    }
}
