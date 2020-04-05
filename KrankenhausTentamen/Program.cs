namespace KrankenhausTentamen
{
    class Program
    {
        static void Main(string[] args)
        {
            SimulationHelper simulationHelper = new SimulationHelper(10);
            simulationHelper.RunSimulation();
        }
    }
}
