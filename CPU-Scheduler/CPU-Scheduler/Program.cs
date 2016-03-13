using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPU_Scheduler;

namespace CPU_Scheduler
{
    class Program
    {

        private static int counter = 0;
        private static int startOfRunningP;
        private const int tq1 = 7;
        private const int tq2 = 14;
        private static List<Process> readyQ;
        private static List<Process> blockingList;  
        private static Queue<Process> readyQ1;
        private static Queue<Process> readyQ2;
        private static Queue<Process> readyQ3;


        // ------------ SJF Auxillary Methods -------------------------------------------------------------------------------
        public static Process SJFGetNextProcess(List<Process> processes)
        {
            var p = processes.Min();
            processes.Remove(p);
            p.State = Process.Status.RUNNING;
            return p;
        }
        public static void SJFUpdateQueues(ref Process p)
        {
            if(p?.State == Process.Status.READY) readyQ.Add(p);
            if (p?.State == Process.Status.BLOCKED) blockingList.Add(p);

            List<Process> readyProcesses = blockingList.FindAll(x => x?.State == Process.Status.READY);

            readyQ.AddRange(readyProcesses);

            readyQ.RemoveAll(x => x?.State == Process.Status.BLOCKED || x?.State == Process.Status.FINISHED);
            blockingList.RemoveAll(x => x?.State == Process.Status.READY || x?.State == Process.Status.FINISHED);

            if (p?.CurCPUTime == 0) p.ArrivalTime[1] = counter;

            if (p?.State == Process.Status.BLOCKED) p = null;

            if (p?.State == Process.Status.FINISHED)
            {
                CalcTime(p, counter);
                p = null;
            }
        }
        private static void RunSJF(Process runningP)
        {
            while (true)
            {
                //Console.WriteLine("Current Process: " + runningP);
                //Console.WriteLine("ReadyQ: " + String.Join(",", readyQ));
                //Console.WriteLine("BlockingList: " + String.Join(",", blockingList));
                //Console.WriteLine("Counter: {0}", counter);
                //Console.WriteLine();

                if (readyQ.Count > 0)
                {
                    if (runningP == null) runningP = SJFGetNextProcess(readyQ);
                    if (runningP.ResponseTime == null) runningP.ResponseTime = counter;
                }

                if (runningP != null) runningP.CurCPUTime--;

                foreach (Process p in blockingList)
                {
                    p.CurCPUTime--;
                    p.Update();
                }

                runningP?.Update();

                // Checks to see if all processes have finished
                if (SJFAllFinished(runningP)) break;

                SJFUpdateQueues(ref runningP);
                counter++;
            }
        }
        public static bool SJFAllFinished(Process runningP)
        {
            return readyQ.Count == 0 && blockingList.Count == 0 && runningP == null;
        }




        // ------------ MLFQ Auxillary Methods ------------------------------------------------------------------------------
        public static void MLFQUpdateQueues(ref Process p, Queue<Process> readyQ)
        {
            if (p?.State == Process.Status.READY || p?.State == Process.Status.DOWNGRADED) readyQ.Enqueue(p);
            if (p?.State == Process.Status.BLOCKED) blockingList.Add(p);

            //List<Process> readyProcesses = blockingList.FindAll(x => x?.State == Process.Status.READY);

            //readyProcesses.ForEach(readyQ.Enqueue);

            if (p?.CurCPUTime == 0) p.ArrivalTime[1] = counter;

            if (p?.State == Process.Status.BLOCKED) p = null;


            if (p?.State == Process.Status.DOWNGRADED)
            {
                
                p.State = Process.Status.READY;
                p = null;
            }

            if (p?.State == Process.Status.FINISHED)
            {
                CalcTime(p, counter);
                p = null;
            }
        }
        public static void MLFQUpdateBlockingList(List<Process> blockingList)
        {
            List<Process> tempProcessesToRemove = new List<Process>();

            foreach (Process p in blockingList)
            {
                p.CurCPUTime--;
                p.Update();

                if (p.State == Process.Status.READY)
                {
                    if (p.PriorityType == 1)
                    {
                        readyQ1.Enqueue(p);
                        tempProcessesToRemove.Add(p);
                    }

                    if (p.PriorityType == 2)
                    {
                        readyQ2.Enqueue(p);
                        tempProcessesToRemove.Add(p);
                    }

                    if (p.PriorityType == 3)
                    {
                        readyQ3.Enqueue(p);
                        tempProcessesToRemove.Add(p);
                    }

                    if (p.State == Process.Status.FINISHED) tempProcessesToRemove.Add(p);
                }
            }

            blockingList.RemoveAll(x => tempProcessesToRemove.Contains(x));
        }
        public static void Preempt(ref Process p, Queue<Process> readyQ, int priorityType)
        {
            //RunCurrentProcess(startOfRunningP, ref p);
            if (p != null)
            {
                p.State = Process.Status.READY;

                if (p.PriorityType == 2) readyQ2.Enqueue(p);
                if (p.PriorityType == 3) readyQ3.Enqueue(p);

                p = readyQ.Dequeue();
                p.PriorityType = priorityType;
                p.State = Process.Status.RUNNING;
            }
        }
        private static void RunMLFQ(Process runningP)
        {

            startOfRunningP = 0;

            while (true)
            {
                
                // Get next ready process or preempt a process based on priority queue
                runningP = MLFQGetNext(runningP, ref startOfRunningP);

                // Decrement current running process's CPU time based on priority queue type
                if (!RunCurrentProcess(startOfRunningP, ref runningP)) continue;

                MLFQUpdateBlockingList(blockingList);

                if (runningP != null)
                {
                    if(runningP.PriorityType == 1) MLFQUpdateQueues(ref runningP, readyQ1);
                    else if(runningP.PriorityType == 2) MLFQUpdateQueues(ref runningP, readyQ2);
                    else if(runningP.PriorityType == 3) MLFQUpdateQueues(ref runningP, readyQ3);
                }


                if (MLFQAllFinished(runningP)) break;



                counter++;


                //Console.WriteLine("Counter: {0}", counter);
                //Console.WriteLine("Current Process: " + runningP);
                //Console.WriteLine("ReadyQ1: " + String.Join(",", readyQ1));
                //Console.WriteLine("ReadyQ2: " + String.Join(",", readyQ2));
                //Console.WriteLine("ReadyQ3: " + String.Join(",", readyQ3));
                //Console.WriteLine("BlockingList: " + String.Join(",", blockingList));
                //Console.WriteLine();
            }
        }
        private static Process MLFQGetNext(Process runningP, ref int startOfRunningP)
        {
            if (readyQ1.Count > 0)
            {
                if (runningP == null)
                {
                    runningP = readyQ1.Dequeue();
                    runningP.State = Process.Status.RUNNING;
                    runningP.PriorityType = 1;
                    startOfRunningP = counter;
                }

                if (runningP.ResponseTime == null) runningP.ResponseTime = counter;

                if (runningP.PriorityType > 1)
                {
                    startOfRunningP = counter;
                    Preempt(ref runningP, readyQ1, 1);
                }
            }
            else if (readyQ2.Count > 0)
            {
                if (runningP == null)
                {
                    runningP = readyQ2.Dequeue();
                    runningP.State = Process.Status.RUNNING;
                    runningP.PriorityType = 2;
                    startOfRunningP = counter;
                }

                if (runningP.PriorityType > 2)
                {
                    startOfRunningP = counter;
                    Preempt(ref runningP, readyQ2, 2);
                }
            }
            else if (readyQ3.Count > 0)
            {
                if (runningP == null)
                {
                    runningP = readyQ3.Dequeue();
                    runningP.State = Process.Status.RUNNING;
                    runningP.PriorityType = 3;
                    startOfRunningP = counter;
                }
            }
            return runningP;
        }
        private static bool RunCurrentProcess(int startOfRunningP, ref Process runningP)
        {
            if (runningP != null)
            {
                switch (runningP.PriorityType)
                {
                    case 1:

                        if ((counter - startOfRunningP) < tq1)
                        {
                            runningP.CurCPUTime--;
                            runningP.Update();
                        }
                        else
                        {
                            if (runningP.CurCPUTime != 0)
                            {
                                runningP.State = Process.Status.DOWNGRADED;
                                MLFQUpdateQueues(ref runningP, readyQ2); //Downgrade process to priority 2 queue
                                return false;
                            }
                        }

                        break;
                    case 2:

                        if ((counter - startOfRunningP) < tq2)
                        {
                            runningP.CurCPUTime--;
                            runningP.Update();
                        }
                        else
                        {
                            if (runningP.CurCPUTime != 0)
                            {
                                runningP.State = Process.Status.DOWNGRADED;
                                MLFQUpdateQueues(ref runningP, readyQ3); //Downgrade process to priority 2 queue
                                return false;
                            }
                        }


                        break;
                    case 3:
                        runningP.CurCPUTime--;
                        runningP.Update();

                        break;
                    default:
                        break;
                }
            }
            return true;
        }
        private static bool MLFQAllFinished(Process runningP)
        {
            return (runningP == null &&
                    readyQ1.Count == 0 &&
                    readyQ2.Count == 0 &&
                    readyQ3.Count == 0 &&
                    blockingList.Count == 0);
        }





        // ------------ Generic Auxillary Methods ---------------------------------------------------------------------------
        private static void CalcTime(Process p, int time)
        {
            p.TurnTime = time;
            p.WaitTime = p.TurnTime - p.GetTotalCPUTime();
        }
        public static void SetUpProcesses(Process p1, Process p2, Process p3, Process p4, Process p5, Process p6,
            Process p7, Process p8, Process p9)
        {
            p1.AddProcesses(new int[] { 18, 41, 16, 52, 19, 31, 14, 33, 17, 43, 19, 66, 14, 39, 17 });
            p2.AddProcesses(new int[] { 8, 32, 7, 42, 6, 27, 17, 41, 7, 33, 11, 43, 12, 32, 14 });
            p3.AddProcesses(new int[] { 6, 51, 5, 53, 6, 46, 9, 32, 11, 52, 4, 61, 8 });
            p4.AddProcesses(new int[] { 25, 35, 19, 41, 21, 45, 18, 51, 12, 61, 24, 54, 23, 61, 21 });
            p5.AddProcesses(new int[] { 15, 61, 16, 52, 15, 71, 13, 41, 15, 62, 14, 31, 14, 41, 13, 32, 15 });
            p6.AddProcesses(new int[] { 6, 25, 5, 31, 6, 32, 5, 41, 4, 81, 8, 39, 11, 42, 5 });
            p7.AddProcesses(new int[] { 16, 38, 17, 41, 15, 29, 14, 26, 9, 32, 5, 34, 8, 26, 6, 39, 5 });
            p8.AddProcesses(new int[] { 5, 52, 4, 42, 6, 31, 7, 21, 4, 43, 5, 31, 7, 32, 6, 32, 7, 41, 4 });
            p9.AddProcesses(new int[] { 11, 37, 12, 41, 6, 41, 4, 48, 6, 41, 5, 29, 4, 26, 5, 31, 3 });
        }
        public static void PrintCalculations(params Process[] processes)
        {
            List<int?> responseTimes = new List<int?>();
            List<int?> waitTimes = new List<int?>();
            List<int?> turnTimes = new List<int?>();

            foreach (Process p in processes)
            {
                Console.WriteLine("{0}: ResponseTime: {1}", p.Name, p.ResponseTime);
                Console.WriteLine("    WaitTime: {0}", p.WaitTime);
                Console.WriteLine("    TurnTime: {0}", p.TurnTime);

                responseTimes.Add(p.ResponseTime);
                waitTimes.Add(p.WaitTime);
                turnTimes.Add(p.TurnTime);
            }

            Console.WriteLine("----------------------------");
            Console.WriteLine("AVG: ResponseTime: {0:0.00}", Avg(responseTimes));
            Console.WriteLine("     WaitTime: {0:0.00}", Avg(waitTimes));
            Console.WriteLine("     TurnTime: {0:0.00}", Avg(turnTimes));
        }
        public static double Avg(List<int?> times)
        {
            double result = times.Cast<int>().Aggregate<int, double>(0, (current, item) => current + item);

            result /= times.Count;

            return result;
        }
        public static void ResetAllProcesses(params Process[] processes)
        {
            foreach (Process p in processes)
            {
                p.Reset();
            }
        }




        // ------------- Main Program Entry Point ---------------------------------------------------------------------------
        public static void Main(string[] args)
        {
            //-------------- SJF -----------------

            // Initialize each process with their name as a string
            Process p1 = new Process("P1"),
                    p2 = new Process("P2"),
                    p3 = new Process("P3"),
                    p4 = new Process("P4"),
                    p5 = new Process("P5"),
                    p6 = new Process("P6"),
                    p7 = new Process("P7"),
                    p8 = new Process("P8"),
                    p9 = new Process("P9");

            // Adds an array of CPU bursts and IO time to each process
            SetUpProcesses(p1, p2, p3, p4, p5, p6, p7, p8, p9);

            readyQ = new List<Process>( new Process[] { p1, p2, p3, p4, p5, p6, p7, p8, p9 });
            blockingList = new List<Process>();

            Process runningP = null;

            counter = 0;

            Console.WriteLine("SJF Algorithm Running....\n");

            // SJF Algorithm
            RunSJF(runningP);

            Console.WriteLine("Total CPU Time Units: {0}", counter);
            Console.WriteLine();

            PrintCalculations(p1, p2, p3, p4, p5, p6, p7, p8, p9);

            //-------------- MLFQ -----------------

            ResetAllProcesses(p1, p2, p3, p4, p5, p6, p7, p8, p9);
            counter = 0;

            SetUpProcesses(p1, p2, p3, p4, p5, p6, p7, p8, p9);

            readyQ1 = new Queue<Process>(new Process[] { p1, p2, p3, p4, p5, p6, p7, p8, p9 });
            readyQ2 = new Queue<Process>();
            readyQ3 = new Queue<Process>();
            
            blockingList.Clear();

            Console.WriteLine();
            Console.WriteLine("\nMLFQ Algorithm Running....");

            // MLFQ Algorithm
            RunMLFQ(runningP);

            Console.WriteLine();
            Console.WriteLine("Total CPU Time Units: {0}", counter);
            Console.WriteLine();

            PrintCalculations(p1, p2, p3, p4, p5, p6, p7, p8, p9);

            Console.Read();   

        }

        
    }
}
