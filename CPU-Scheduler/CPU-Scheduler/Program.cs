using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPU_Scheduler;

namespace CPU_Scheduler
{
    class Program
    {
        public static void SetUpForSJF(Queue<Process> Q)
        {
            Process P1 = new Process();
            Process P2 = new Process();
            Process P3 = new Process();
            Process P4 = new Process();
            Process P5 = new Process();
            Process P6 = new Process();
            Process P7 = new Process();
            Process P8 = new Process();
            Process P9 = new Process();

            Q.Enqueue(P1);
            Q.Enqueue(P2);
            Q.Enqueue(P3);
            Q.Enqueue(P4);
            Q.Enqueue(P5);
            Q.Enqueue(P6);
            Q.Enqueue(P7);
            Q.Enqueue(P8);
            Q.Enqueue(P9);
        }
        static void Main(string[] args)
        {
            Queue<Process> ReadyQ = new Queue<Process>();
            SetUpForSJF(ReadyQ);

            
            // SJF Algorithm
            
            
            // MLFQ Algorithm     

        }
    }
}
