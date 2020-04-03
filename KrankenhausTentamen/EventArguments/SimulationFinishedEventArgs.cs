using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
