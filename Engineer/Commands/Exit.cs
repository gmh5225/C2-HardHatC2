﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Engineer.Models;


namespace Engineer.Commands
{
    internal class Exit : EngineerCommand
    {
       public static Stopwatch stopwatch = new Stopwatch();
        public override string Name => "exit";

        public override string Execute(EngineerTask task)
        {
            task.Arguments.TryGetValue("/time",out string time);
            int sleepTime;
            if (!int.TryParse(time, out sleepTime))
            {
                sleepTime = 5;
            }


            //set a timer to exit the program after 5 seconds
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < sleepTime * 1000)
            {
                Thread.Sleep(100);
            }
            Environment.Exit(0);
            return $"Exiting Engineer in {sleepTime} seconds";
        }
    }
}
