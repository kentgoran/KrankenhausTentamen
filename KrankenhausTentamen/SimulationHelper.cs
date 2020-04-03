using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KrankenhausTentamen.EventArguments;
using KrankenhausTentamen.Enums;

namespace KrankenhausTentamen
{
    public class SimulationHelper
    {
        public TimeSpan TotalTimeSpent = new TimeSpan();
        public void RunSimulation()
        {
            Simulator simulator = new Simulator();
            simulator.SimulationFinished += OnSimulationFinished;
            simulator.PatientsSorted += OnPatientsSorted;
            simulator.PatientsRecoveredOrDied += OnPatientsDyingOrRecovering;
            int counter = 0;
            while(counter < 10)
            {
                simulator.Start();
                counter++;
            }
            Console.WriteLine("All 10 simulations done. This took {0:0} seconds.", TotalTimeSpent.TotalSeconds);
            Console.ReadLine();
        }

        /// <summary>
        /// Takes a list of TimeSpan? and calculates the average timespan. If a value is null, subtract that number from the division
        /// </summary>
        /// <param name="timeSpentInQueue"></param>
        /// <returns></returns>
        private TimeSpan CalculateAverageTimeSpan(List<TimeSpan?> timeSpentInQueue)
        {
            long totalTime = 0;
            int patients = timeSpentInQueue.Count;
            foreach (var timeSpent in timeSpentInQueue)
            {
                if (timeSpent.HasValue)
                {
                    totalTime += timeSpent.Value.Ticks;
                }
                else
                {
                    patients -= 1;
                }
            }
            TimeSpan averageTime = new TimeSpan(0);
            //Remove chance of divideByZero-exception
            if(patients != 0)
            {
                averageTime = new TimeSpan(totalTime / patients);
            }
            return averageTime;
        }
        /// <summary>
        /// To be tied to PatientsSorted 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnPatientsSorted(object sender, PatientsSortedEventArgs args)
        {
            Console.WriteLine($"{args.PatientsMovedToICU} patients was moved to ICU, which now has {args.PatientsInICU} patients.");
            Console.WriteLine($"{args.PatientsMovedToSanatorium} patients was moved to the Sanatorium, which now has {args.PatientsInSanatorium} patients.");
            if (args.PatientsInSanatorium == 0 && args.PatientsInICU == 0)
            {
                args.CancellationRequested = true;
            }
            //write to file
        }
        private void OnPatientsDyingOrRecovering(object sender, PatientsRecoveredOrDiedEventArgs args)
        {
            Console.WriteLine($"{args.PatientsRecovered} just recovered and {args.PatientsDied} died.");
            if (args.PatientsLeft == 0)
            {
                args.CancellationRequested = true;
            }
        }

        /// <summary>
        /// To be connected to SimulationFinished-Event. Extracts data and sends it to the database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnSimulationFinished(object sender, SimulationFinishedEventArgs args)
        {
            SimulationData simulationData = new SimulationData();
            simulationData.ExecutionTime = args.ExecutionTime;
            TotalTimeSpent += args.ExecutionTime;
            using (var hospitalContext = new HospitalContext())
            {
                var deadPatients = (from patient in hospitalContext.Patients
                                    where patient.Status == PatientStatus.Afterlife
                                    select patient).Count();
                var recoveredPatients = (from patient in hospitalContext.Patients
                                         where patient.Status == PatientStatus.Recovered
                                         select patient).Count();
                var timeSpentInQueue = (from patient in hospitalContext.Patients
                                        select patient.TimeSpentInQueue).ToList();

                simulationData.AmountOfDeadPatients = deadPatients;
                simulationData.AmountOfRecoveredPatients = recoveredPatients;
                simulationData.AverageTimeSpentInQueue = CalculateAverageTimeSpan(timeSpentInQueue);

                hospitalContext.SimulationData.Add(simulationData);
                hospitalContext.SaveChanges();

                //Empties out the patients and doctors-tables after finished simulation
                hospitalContext.Database.ExecuteSqlCommand("TRUNCATE TABLE Patients");
                hospitalContext.Database.ExecuteSqlCommand("TRUNCATE TABLE Doctors");
                hospitalContext.SaveChanges();
            }
            Console.WriteLine($"All patients has left the hospital. This simulation took {args.ExecutionTime.TotalSeconds:0} seconds.");
            Console.WriteLine("---------------------------------------------");

        }
    }
}
