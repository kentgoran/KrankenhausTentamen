using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KrankenhausTentamen.EventArguments;
using KrankenhausTentamen.Enums;
using System.IO;

namespace KrankenhausTentamen
{
    public class SimulationHelper
    {
        private TimeSpan TotalTimeSpent = new TimeSpan();
        private DateTime startTime;
        private string fileName = "EventPrint.txt";
        private int timesToRun;
        public SimulationHelper(int timesToRun = 1)
        {
            this.timesToRun = timesToRun;
        }

        /// <summary>
        /// Starts the simulations, and runs them "timesToRun" amount of times
        /// </summary>
        public void RunSimulation()
        {
            Simulator simulator = new Simulator();
            simulator.SimulationFinished += OnSimulationFinished;
            simulator.PatientsSorted += OnPatientsSorted;
            simulator.PatientsRecoveredOrDied += OnPatientsDyingOrRecovering;
            int counter = 1;
            startTime = DateTime.Now;
            while(counter <= timesToRun)
            {
                Console.WriteLine($"Running simulation number {counter}.");
                simulator.Start();
                counter++;
            }
            TotalTimeSpent = DateTime.Now - startTime;
            LogStatistics();
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

            //Removes chance of divideByZero-exception, and sends back 0 as avg time spent
            if(patients == 0)
            {
                return new TimeSpan(0);
            }
            TimeSpan averageTime = new TimeSpan(totalTime / patients);
            return averageTime;
        }

        /// <summary>
        /// Takes a list of TimeSpan and calculates the average timeSpan
        /// </summary>
        /// <param name="timeSpentInQueue"></param>
        /// <returns>A TimeSpan containing average time from the list</returns>
        private TimeSpan CalculateAverageTimeSpan(List<TimeSpan> timeSpentInQueue)
        {
            long totalTime = 0;
            foreach (var timeSpent in timeSpentInQueue)
            {
                totalTime += timeSpent.Ticks;
            }
            TimeSpan averageTime = new TimeSpan(totalTime / timeSpentInQueue.Count);
            return averageTime;
        }

        /// <summary>
        /// To be tied to PatientsSorted. Prints changes to console and to file "fileName". Also 'ticks' the cancellation-token if there are no patients present
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnPatientsSorted(object sender, PatientsSortedEventArgs args)
        {
            string toWrite = $"{args.PatientsMovedToICU} patients was moved to ICU, which now has {args.PatientsInICU} patients.\n" +
                $"{args.PatientsMovedToSanatorium} patients was moved to the Sanatorium, which now has {args.PatientsInSanatorium} patients.\n";
            lock (this)
            {
                File.AppendAllText(fileName, toWrite);
            }
            Console.Write(toWrite);
            if (args.PatientsInSanatorium == 0 && args.PatientsInICU == 0)
            {
                args.CancellationRequested = true;
            }
        }

        /// <summary>
        /// To be tied to PatientsRecoveringOrDying. Logs event-changes. also checks if there are any patients left. if not, 'ticks' the cancellationToken
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnPatientsDyingOrRecovering(object sender, PatientsRecoveredOrDiedEventArgs args)
        {
            string toWrite = $"{args.PatientsRecovered} just recovered and {args.PatientsDied} died.\n";
            lock (this)
            {
                File.AppendAllText(fileName, toWrite);
            }
            Console.Write(toWrite);
            if (args.PatientsLeft == 0)
            {
                args.CancellationRequested = true;
            }
        }

        /// <summary>
        /// To be connected to SimulationFinished-Event. Extracts data and logs it in the database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnSimulationFinished(object sender, SimulationFinishedEventArgs args)
        {
            SimulationData simulationData = new SimulationData();
            simulationData.ExecutionTime = args.ExecutionTime;
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
            string toWrite = $"All patients has left the hospital. This simulation took {args.ExecutionTime.TotalSeconds:0} seconds.\n" +
                "---------------------------------------------\n\n";
            File.AppendAllText(fileName, toWrite);
            Console.Write(toWrite);
            
        }

        /// <summary>
        /// Logs statistics, averages etc to console and file
        /// </summary>
        private void LogStatistics()
        {

            int totalRecovered = 0;
            int totalDead = 0;
            List<TimeSpan> timePerSimulation = new List<TimeSpan>();
            List<TimeSpan> averageTimesInQueue = new List<TimeSpan>();

            using (var hospitalContext = new HospitalContext())
            {
                var simulationData = (from simData in hospitalContext.SimulationData
                                      orderby simData.SimulationDataId descending
                                      select simData).Take(timesToRun).ToList();
                foreach(var simData in simulationData)
                {
                    totalRecovered += simData.AmountOfRecoveredPatients;
                    totalDead += simData.AmountOfDeadPatients;
                    timePerSimulation.Add(simData.ExecutionTime);
                    averageTimesInQueue.Add(simData.AverageTimeSpentInQueue);
                }
            }

            double averageRecovered = (double)totalRecovered / (double)timesToRun;
            double averageDead = (double)totalDead / (double)timesToRun;
            TimeSpan averageSimulationTime = CalculateAverageTimeSpan(timePerSimulation);
            TimeSpan averageTimeInQueue = CalculateAverageTimeSpan(averageTimesInQueue);
            StringBuilder toWrite = new StringBuilder();
            toWrite.AppendLine($"Simulation - {DateTime.Now}");
            toWrite.AppendLine($"Average amount of recovered patients was {averageRecovered:0.0}.");
            toWrite.AppendLine($"Average amount of patients that died was {averageDead:0.0}.");
            toWrite.AppendLine($"Average time spent in queue was {averageTimeInQueue.TotalSeconds:0.00} seconds.");
            toWrite.AppendLine($"Average time per simulation was {averageSimulationTime.TotalSeconds:0.00} seconds.");
            toWrite.AppendLine($"Thanks for this run. Simulation finished after {TotalTimeSpent.TotalMinutes:0} minutes and {TotalTimeSpent.Seconds} seconds, totalling in {timesToRun} simulations.\n");
            Console.Write(toWrite.ToString());
            lock (this)
            {
                File.AppendAllText(fileName, toWrite.ToString());
            }
        }
    }
}
