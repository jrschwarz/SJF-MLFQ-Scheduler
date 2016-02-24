using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CPU_Scheduler
{
    class Process : IComparable<Process>
    {
        private int[] _processes;
        private int _index;

        public int? WaitTime;

        public int? TurnTime;

        public int? CurCPUTime;
        public int[] ArrivalTime { get; set; }
        public int? ResponseTime { get; set; }
        public Status State { get; set; }
        public enum Status
        {
            READY,
            RUNNING,
            BLOCKED,
            FINISHED
        }; 

        public Process()
        {
            _processes = new int[] { }; // initialize with no processes
            ArrivalTime = new int[] {0, 0};
            WaitTime = null;
            TurnTime = null;
            ResponseTime = null;
            State = Status.READY;
            _index = 0;
            CurCPUTime = null;

        }
        public void AddProcesses(int[] inputProcesses)
        {
            _processes = inputProcesses;
            CurCPUTime = _processes[_index];
        }
        public void Update()
        {
            if (CurCPUTime == 0)
            {
                _index++;
                State = (_index % 2 == 0) ? Status.READY : Status.BLOCKED;

                if (_index < _processes.Length)
                {
                    CurCPUTime = _processes[_index];
                }
                else
                {
                    CurCPUTime = null;
                    State = Status.FINISHED;
                }
            }

            
        }
        public int GetTotalCPUTime()
        {
            return _processes.Sum();
        }
        public int CompareTo(Process p)
        {
            return (this.CurCPUTime < p.CurCPUTime) ? -1 : 1;
        }
        public override string ToString()
        {
            return CurCPUTime.ToString();
        }
    }
}
