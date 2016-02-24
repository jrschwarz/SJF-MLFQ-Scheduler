﻿using System;
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
        public static Process GetNextProcess(List<Process> processes)
        {
            var p = processes.Min();
            processes.Remove(p);
            p.State = Process.Status.RUNNING;
            return p;
        }
        public static void UpdateQueues(ref Process p, List<Process> readyQ, List<Process> blockingList, int counter)
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
        public static bool AllFinished(Process runningP, List<Process> readyQ, List<Process> blockingList)
        {
            return readyQ.Count == 0 && blockingList.Count == 0 && runningP == null;
        }
        private static void CalcTime(Process p, int time)
        {
            p.TurnTime = time;
            p.WaitTime = p.TurnTime - p.GetTotalCPUTime();
        }
        public static void Main(string[] args)
        {
            Process p1 = new Process(),
                    p2 = new Process(),
                    p3 = new Process(),
                    p4 = new Process(),
                    p5 = new Process(),
                    p6 = new Process(),
                    p7 = new Process(),
                    p8 = new Process(),
                    p9 = new Process();

            SetUpProcesses(p1, p2, p3, p4, p5, p6, p7, p8, p9);

            List<Process> readyQ = new List<Process>( new Process[] { p1, p2, p3, p4, p5, p6, p7, p8, p9 });
            List<Process> blockingList = new List<Process>();

            Process runningP = null;

            int counter = 0;

            // SJF Algorithm
            while (true)
            {
                Console.WriteLine("Current Process: " + runningP);
                Console.WriteLine("ReadyQ: " + String.Join(",", readyQ));
                Console.WriteLine("BlockingList: " + String.Join(",", blockingList));
                Console.WriteLine("Counter: {0}", counter);
                Console.WriteLine();

                if (readyQ.Count > 0 || runningP != null)
                {
                    if (runningP == null ) runningP = GetNextProcess(readyQ);
                    if (runningP.ResponseTime == null) runningP.ResponseTime = counter;

                    runningP.CurCPUTime--;
                }

                foreach (Process p in blockingList)
                {
                    p.CurCPUTime--;
                    p.Update();
                }

                runningP?.Update();
                
                // Checks to see if all processes have finished
                if (AllFinished(runningP, readyQ, blockingList)) break;

                UpdateQueues(ref runningP, readyQ, blockingList, counter);
                counter++;
            }

            Console.WriteLine("Counter: {0}", counter);
            Console.Read();




            // MLFQ Algorithm     

        }
    }
}
