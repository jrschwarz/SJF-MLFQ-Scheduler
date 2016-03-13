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
        // Array of cpu time and I/O time
        private int[] _processes;

        // Keeps track of current position in _processes array
        private int _index;

        // Name of the Process
        public string Name;

        // Corresponding times for a process. Calculated at end of run.
        public int? WaitTime { get; set; }
        public int? TurnTime { get; set; }
        public int? ResponseTime { get; set; }
        public int FinishTime { get; set; }

        // The value of _processes[_index]
        public int? CurCPUTime;

        // Keeps track of the current state of a process
        public Status State { get; set; }

        // Defines the states in which a process can be in
        public enum Status
        {
            READY,
            RUNNING,
            BLOCKED,
            FINISHED,
            DOWNGRADED
        };

        // Priority types of a process (1,2,3)
        public int? PriorityType { get; set; }

        /// <summary>
        /// Constructor. Initializes all properties.
        /// </summary>
        /// <param name="name">Name of process</param>
        public Process(string name)
        {
            _processes = new int[] { }; // initialize with no processes
            Name = name;
            WaitTime = null;
            TurnTime = null;
            ResponseTime = null;
            State = Status.READY;
            _index = 0;
            CurCPUTime = null;
            PriorityType = null;

        }

        /// <summary>
        /// Adds a list of CPU time and I/O time to a process.
        /// Also sets CurCPUTime to first value of the array.
        /// </summary>
        /// <param name="inputProcesses">Array of cpu time and I/O time</param>
        public void AddProcesses(int[] inputProcesses)
        {
            _processes = inputProcesses;
            CurCPUTime = _processes[_index];
        }

        /// <summary>
        /// Updates that status of a process based on the current position of the _index property
        /// </summary>
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

        /// <summary>
        /// Returns total of CPU time and I/O of a process.
        /// </summary>
        /// <returns>Total I/O and CPU time</returns>
        public int GetTotal()
        {
            return _processes.Sum();
        }

        /// <summary>
        /// Returns the total CPU time of a process.
        /// </summary>
        /// <returns>Total CPU time</returns>
        public int GetTotalCPUTime()
        {
            int sum = 0;

            for (int i = 0; i < _processes.Length; i++)
            {
                if (i%2 == 0)
                {
                    sum += _processes[i];
                }
            }

            return sum;
        }

        /// <summary>
        /// Implementation of the IComparer Interface. Used to find the min of a process
        /// based on its CurCPUTime.
        /// </summary>
        /// <param name="p">A process</param>
        /// <returns>1 or -1</returns>
        public int CompareTo(Process p)
        {
            return (this.CurCPUTime < p.CurCPUTime) ? -1 : 1;
        }

        /// <summary>
        /// Resets all properties of the process.
        /// </summary>
        public void Reset()
        {
            WaitTime = null;
            TurnTime = null;
            ResponseTime = null;
            State = Status.READY;
            _index = 0;
            CurCPUTime = null;
        }

        /// <summary>
        /// Determine the way a process object is output to a string for 
        /// displaying to the console.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Name}({CurCPUTime})";
        }
    }
}
