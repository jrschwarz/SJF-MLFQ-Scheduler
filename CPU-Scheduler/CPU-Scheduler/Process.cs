using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CPU_Scheduler
{
    class Process
    {
        private int[] _processes;
        private int _index;


        public int? curCPUTime;
        public int ArrivalTime { get; set; }
        public int? ResponseTime { get; set; }
        public bool IsFinished { get; set; }
        public Status State { get; set; }
        public enum Status
        {
            READY,
            RUNNING,
            BLOCKED
        }; 

        public Process()
        {
            _processes = new int[] { }; // initialize with no processes
            ArrivalTime = 0;
            ResponseTime = null;
            IsFinished = false;
            State = Status.READY;
            _index = 0;
            curCPUTime = null;

        }
        public void AddProcesses(int[] inputProcesses)
        {
            _processes = inputProcesses;
            curCPUTime = _processes[_index];
        }

    }
}
