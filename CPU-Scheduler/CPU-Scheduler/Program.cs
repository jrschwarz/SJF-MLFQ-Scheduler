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
        //Keeps track of cpu time. 
        //Every increment is on unit of cpu time
        private static int counter = 0;

        //Keeps track of the time a process started running
        private static int startOfRunningP;

        //Time quantums
        private const int tq1 = 7;
        private const int tq2 = 14;

        //SJF ready queue
        private static List<Process> readyQ;

        //MLFQ three priority queues (1 is highest priority)
        private static Queue<Process> readyQ1;
        private static Queue<Process> readyQ2;
        private static Queue<Process> readyQ3;

        //List that keeps track of all processes in I/O
        private static List<Process> blockingList;

        //List that keeps track of all finished processes
        private static List<Process> finished; 



        // ------------ SJF Auxillary Methods -------------------------------------------------------------------------------

        /// <summary>
        /// Retrieves next process from the passed in ready queue.
        /// Next process is determined by the process with the smallest cpu bursts.
        /// </summary>
        /// <param name="processes">Ready queue</param>
        /// <returns>A process</returns>
        public static Process SJFGetNextProcess(List<Process> processes)
        {
            var p = processes.Min();
            processes.Remove(p);
            p.State = Process.Status.RUNNING;
            return p;
        }

        /// <summary>
        /// Updates all the queues used, ensuring all processes that are blocked
        /// are in the blocking list and all that are ready are in the ready queue.
        /// If a process is finished it will also ensure it is in neither the ready queue
        /// nor the blocking list.
        /// </summary>
        /// <param name="p">The current process that is running</param>
        public static void SJFUpdateQueues(ref Process p)
        {
            if(p?.State == Process.Status.READY) readyQ.Add(p);
            if (p?.State == Process.Status.BLOCKED) blockingList.Add(p);

            List<Process> readyProcesses = blockingList.FindAll(x => x?.State == Process.Status.READY);

            readyQ.AddRange(readyProcesses);

            readyQ.RemoveAll(x => x?.State == Process.Status.BLOCKED || x?.State == Process.Status.FINISHED);
            blockingList.RemoveAll(x => x?.State == Process.Status.READY || x?.State == Process.Status.FINISHED);

            if (p?.State == Process.Status.BLOCKED) p = null;

            if (p?.State == Process.Status.FINISHED)
            {
                finished.Add(p);
                CalcTime(p, counter);
                p.FinishTime = counter;
                p = null;
            }
        }

        /// <summary>
        /// The algorithm that runs SJF on a list of processes. It finishes when
        /// all queues are empty and there aren't any processes currently running.
        /// </summary>
        /// <param name="runningP">The Current Process => should be null to start</param>
        private static void RunSJF(Process runningP)
        {

            while (true)
            {
                Process tempP = runningP;

                if (readyQ.Count > 0)
                {
                    if (runningP == null) runningP = SJFGetNextProcess(readyQ);
                    if (runningP.ResponseTime == null) runningP.ResponseTime = counter;
                }

                if (tempP != runningP)
                {
                    SJFPrint(runningP);
                    tempP = runningP;
                }

                if (runningP != null) runningP.CurCPUTime--;
                
                runningP?.Update();

                foreach (Process p in blockingList)
                {
                    p.CurCPUTime--;
                    p.Update();
                }

                // Checks to see if all processes have finished
                if (SJFAllFinished(runningP)) break;

                counter++;
                SJFUpdateQueues(ref runningP);

                if (tempP != runningP)
                {
                    SJFPrint(runningP);
                }

            }
        }

        private static void SJFPrint(Process runningP)
        {
            Console.WriteLine("Current Time: {0}", counter);
            Console.WriteLine("Now Running: " + runningP);
            Console.WriteLine("Ready Queue: " + String.Join(", ", readyQ));
            Console.WriteLine("Now in I/O: " + String.Join(", ", blockingList));
            if (finished.Any()) Console.WriteLine("Completed: " + String.Join(", ", finished));
            Console.WriteLine();
        }

        /// <summary>
        /// Returns true if ready queue and blocking list is empty and there is no 
        /// current process running. Otherwise, it returns false.
        /// </summary>
        /// <param name="runningP">The current process that is running</param>
        /// <returns>True or False</returns>
        public static bool SJFAllFinished(Process runningP)
        {
            return readyQ.Count == 0 && blockingList.Count == 0 && runningP == null;
        }




        // ------------ MLFQ Auxillary Methods ------------------------------------------------------------------------------

        /// <summary>
        /// Updates the passed in queue based on the passed in process. It ensures that the process, based on 
        /// its state, is in the proper queue.This is also used to manage the downgrading on of a process
        /// from one priority queue to another.
        /// </summary>
        /// <param name="p">The current process that is running</param>
        /// <param name="readyQ">Either readyQ1, readyQ2, readyQ3...whichever you pass in</param>
        public static void MLFQUpdateQueues(ref Process p, Queue<Process> readyQ)
        {
            if (p?.State == Process.Status.READY || p?.State == Process.Status.DOWNGRADED) readyQ.Enqueue(p);
            if (p?.State == Process.Status.BLOCKED) blockingList.Add(p);

            if (p?.State == Process.Status.BLOCKED) p = null;

            if (p?.State == Process.Status.DOWNGRADED)
            {
                p.State = Process.Status.READY;
                p = null;
            }

            if (p?.State == Process.Status.FINISHED)
            {
                finished.Add(p);
                CalcTime(p, counter);
                p.FinishTime = counter;
                p = null;
            }
        }

        /// <summary>
        /// Decrements all processes in the blocking list by one cpu unit of time and removes any
        /// process that are finished with I/O time.
        /// </summary>
        /// <param name="blockingList">The list of processes in I/O</param>
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

        /// <summary>
        /// This handles the preempting of a process by another process. It places a process
        /// back in it corresponding based on its prioity type and retrieves the process
        /// from the higher priority queue which is passed in.
        /// </summary>
        /// <param name="p">The current process that is running</param>
        /// <param name="readyQ">The queue of higher priority that causes the preemption</param>
        /// <param name="priorityType">The priority type of the origin running process (used to 
        /// determine which queue to place the original running process back in)</param>
        public static void Preempt(ref Process p, Queue<Process> readyQ, int priorityType)
        {
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

        /// <summary>
        /// The algorithm that runs the MLFQ scheduler.
        /// </summary>
        /// <param name="runningP">The current process => should be null to start</param>
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

                if (MLFQAllFinished(runningP)) break;

                counter++;

                Process tempP = runningP;

                if (runningP != null)
                {
                    if (runningP.PriorityType == 1) MLFQUpdateQueues(ref runningP, readyQ1);
                    else if (runningP.PriorityType == 2) MLFQUpdateQueues(ref runningP, readyQ2);
                    else if (runningP.PriorityType == 3) MLFQUpdateQueues(ref runningP, readyQ3);
                }

                if (tempP != runningP)
                {
                    MLFQPrint(runningP);
                }
            }
        }

        private static void MLFQPrint(Process runningP)
        {
            Console.WriteLine("Current Time: {0}", counter);
            Console.WriteLine("Now running: " + runningP);
            Console.WriteLine("Ready Queue 1: " + String.Join(", ", readyQ1));
            Console.WriteLine("Ready Queue 2: " + String.Join(", ", readyQ2));
            Console.WriteLine("Ready Queue 3: " + String.Join(", ", readyQ3));
            Console.WriteLine("Now in I/O: " + String.Join(", ", blockingList));
            if (finished.Any()) Console.WriteLine("Completed: " + String.Join(", ", finished));
            Console.WriteLine();
        }

        /// <summary>
        /// Gets the next process starting from highest priority queue first. Will preempt a proceess
        /// if a process of a lower priority is running and there are process in higher priority queues
        /// that are ready to run.
        /// </summary>
        /// <param name="runningP">The current process that is running</param>
        /// <param name="startOfRunningP">The time at which that process started running</param>
        /// <returns>The next process</returns>
        private static Process MLFQGetNext(Process runningP, ref int startOfRunningP)
        {
            Process p = runningP;

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

            if (p != runningP)
            {
                MLFQPrint(runningP);
            }

            return runningP;
        }

        /// <summary>
        /// Takes the current process and decrements its current cpu time based on the priority
        /// queue in which it is in as well as the time quantum if applicable. It returns true if a process's
        /// CPU time is able to be decremented. It returns false if a process is downgraded and therefore its CPU time isn't downgraded.
        /// A false return will skip the current iteration of the while loop from incrementing the counter, as there was no CPU decrement
        /// from the current process.
        /// </summary>
        /// <param name="startOfRunningP">The time at which the process started running</param>
        /// <param name="runningP">The current process that is running</param>
        /// <returns>True or False</returns>
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

                                //Downgrade process to priority 2 queue
                                MLFQUpdateQueues(ref runningP, readyQ2); 
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

                                //Downgrade process to priority 2 queue
                                MLFQUpdateQueues(ref runningP, readyQ3);
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

        /// <summary>
        /// Returns true if all ready queues and the blocking list are empty and there is no process
        /// currently running.
        /// </summary>
        /// <param name="runningP">The current process that is running</param>
        /// <returns>True or False</returns>
        private static bool MLFQAllFinished(Process runningP)
        {
            return (runningP == null &&
                    readyQ1.Count == 0 &&
                    readyQ2.Count == 0 &&
                    readyQ3.Count == 0 &&
                    blockingList.Count == 0);
        }





        // ------------ Generic Auxillary Methods ---------------------------------------------------------------------------

        /// <summary>
        /// Calculates the TT and WT based on the passed in time
        /// </summary>
        /// <param name="p">A process</param>
        /// <param name="time">The time at which it finished</param>
        private static void CalcTime(Process p, int time)
        {
            p.TurnTime = time;
            p.WaitTime = p.TurnTime - p.GetTotal();
        }

        /// <summary>
        /// Adds the array of CPU time and I/O time to each process.
        /// </summary>
        /// <param name="p1">P1</param>
        /// <param name="p2">P2</param>
        /// <param name="p3">P3</param>
        /// <param name="p4">P4</param>
        /// <param name="p5">P5</param>
        /// <param name="p6">P6</param>
        /// <param name="p7">P7</param>
        /// <param name="p8">P8</param>
        /// <param name="p9">P9</param>
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

        /// <summary>
        /// Displays the RT, WT, TT, and U for each process as well as the average
        /// </summary>
        /// <param name="processes">A list of processes</param>
        public static void PrintCalculations(params Process[] processes)
        {

            List<int?> responseTimes = new List<int?>();
            List<int?> waitTimes = new List<int?>();
            List<int?> turnTimes = new List<int?>();


            foreach (Process p in processes)
            {
                Console.WriteLine("{0}  ResponseTime: {1}", p.Name, p.ResponseTime);
                Console.WriteLine("    WaitTime: {0}", p.WaitTime);
                Console.WriteLine("    TurnTime: {0}", p.TurnTime);
                Console.WriteLine();

                responseTimes.Add(p.ResponseTime);
                waitTimes.Add(p.WaitTime);
                turnTimes.Add(p.TurnTime);
            }

            Console.WriteLine("----------------------------");
            Console.WriteLine("AVG  ResponseTime: {0:0.00}", Avg(responseTimes));
            Console.WriteLine("     WaitTime: {0:0.00}", Avg(waitTimes));
            Console.WriteLine("     TurnTime: {0:0.00}", Avg(turnTimes));
        }

        /// <summary>
        /// Takes the average of the passed in integers
        /// </summary>
        /// <param name="times">A list of integers</param>
        /// <returns>The average of the list of numbers</returns>
        public static double Avg(List<int?> times)
        {
            double result = times.Cast<int>().Aggregate<int, double>(0, (current, item) => current + item);

            result /= times.Count;

            return result;
        }

        /// <summary>
        /// Resets all processes back to the beginning. This is used to 
        /// transition from SJF to MLFQ
        /// </summary>
        /// <param name="processes">List of processes to be reset</param>
        public static void ResetAllProcesses(params Process[] processes)
        {
            foreach (Process p in processes)
            {
                p.Reset();
            }
        }

        /// <summary>
        /// Calculates the total CPU Utilization of the passed in
        /// processes.
        /// </summary>
        /// <param name="processes">A list of processes</param>
        /// <returns>Cpu Utilization</returns>
        public static double GetCpuUtilization(params Process[] processes)
        {
            List<int> totalCpuTime = new List<int>();

            foreach (Process p in processes)
            {
                totalCpuTime.Add(p.GetTotalCPUTime());
            }

            return ((double)totalCpuTime.Sum()/counter)*100;
        }    




        // ------------- Main Program Entry Point ---------------------------------------------------------------------------

        /// <summary>
        /// Main entry point of the program. Uses all auxillary funtions to calculate the results from a 
        /// SJF scheduler and a MLFQ sheduler.
        /// </summary>
        /// <param name="args">User input => none</param>
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
            finished = new List<Process>();

            Process runningP = null;

            counter = 0;

            Console.WriteLine("SJF Algorithm Running....\n");

            // SJF Algorithm
            RunSJF(runningP);

            Console.WriteLine("Finished\n");
            Console.WriteLine("Total CPU Time Units: {0}", counter);
            Console.WriteLine("CPU Utilization: {0:0.00}%", GetCpuUtilization(p1,p2,p3,p4,p5,p6,p7,p8,p9));
            Console.WriteLine();

            PrintCalculations(p1, p2, p3, p4, p5, p6, p7, p8, p9);

            //-------------- MLFQ -----------------

            Console.WriteLine("\nPress Enter to run the MLFQ Scheduler...");
            Console.ReadLine();
            ResetAllProcesses(p1, p2, p3, p4, p5, p6, p7, p8, p9);
            counter = 0;

            SetUpProcesses(p1, p2, p3, p4, p5, p6, p7, p8, p9);

            readyQ1 = new Queue<Process>(new Process[] { p1, p2, p3, p4, p5, p6, p7, p8, p9 });
            readyQ2 = new Queue<Process>();
            readyQ3 = new Queue<Process>();
            
            blockingList.Clear();
            finished.Clear();

            Console.WriteLine("\n\n\n\n\n");
            Console.WriteLine("\nMLFQ Algorithm Running....\n");

            // MLFQ Algorithm
            RunMLFQ(runningP);

            Console.WriteLine();
            Console.WriteLine("Finished\n");
            Console.WriteLine("Total CPU Time Units: {0}", counter);
            Console.WriteLine("CPU Utilization: {0:0.00}%", GetCpuUtilization(p1, p2, p3, p4, p5, p6, p7, p8, p9));
            Console.WriteLine();

            PrintCalculations(p1, p2, p3, p4, p5, p6, p7, p8, p9);

            Console.Read();   

        }

        
    }
}
