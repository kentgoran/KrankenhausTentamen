using System;

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
