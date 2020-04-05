using KrankenhausTentamen.Enums;
using KrankenhausTentamen.EventArguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


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
