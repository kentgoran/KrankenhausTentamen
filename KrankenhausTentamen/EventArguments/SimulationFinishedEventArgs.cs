using System;

namespace KrankenhausTentamen.EventArguments
{
    public class SimulationFinishedEventArgs : EventArgs
    {
        public TimeSpan ExecutionTime { get; set; }
        private DateTime StartTime;

        public SimulationFinishedEventArgs()
        {
            StartTime = DateTime.Now;
        }
        public void SetExecutionTime()
        {
            ExecutionTime = DateTime.Now - StartTime;
        }
    }
}
