using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrankenhausTentamen
{
    public class SimulationData
    {
        public int SimulationDataId { get; set; }
        public TimeSpan AverageTimeSpentInQueue { get; set; }
        public int AmountOfRecoveredPatients { get; set; }
        public int AmountOfDeadPatients { get; set; }
        public TimeSpan ExecutionTime { get; set; }

        public SimulationData()
        {

        }
    }
}
