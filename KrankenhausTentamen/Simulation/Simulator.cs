using KrankenhausTentamen.EventArguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KrankenhausTentamen.Enums;
using System.Threading;

namespace KrankenhausTentamen
{
    public class Simulator
    {
        public EventHandler<PatientsSortedEventArgs> PatientsSorted;
        public EventHandler<PatientsRecoveredOrDiedEventArgs> PatientsRecoveredOrDied;
        public EventHandler<SimulationFinishedEventArgs> SimulationFinished;
        Random rnd = new Random();
        private bool runDoctorMovement = true;
        private int maxICU = 5;
        private int maxSanatorium = 10;

        /// <summary>
        /// Starts one run of Simulation. Throws SimulationFinished-event when finished
        /// </summary>
        public void Start()
        {
            SimulationFinishedEventArgs args = new SimulationFinishedEventArgs();
            Generator generator = new Generator();
            Thread thread1 = new Thread(generator.AddNewPatientsToDatabase);
            Thread thread2 = new Thread(SimulatePatientSorting);
            Thread thread3 = new Thread(SimulateSymptomsChanging);
            Thread thread4 = new Thread(SimulatePatientRecoveringOrDying);
            Thread thread5 = new Thread(generator.AddNewDoctorsToDatabase);
            Thread thread6 = new Thread(SimulateDoctorMovement);
            //Thread-prio for the threads creating patient and doctor-data
            thread1.Priority = ThreadPriority.Highest;
            thread5.Priority = ThreadPriority.Highest;
            thread1.Start();
            thread5.Start();

            thread2.Start();
            thread3.Start();
            thread4.Start();
            thread6.Start();

            //First, wait for thread4 to run to end, which will indicate the end
            thread4.Join();
            //Then thread 2 and 3, so that they can print a last time before ending the simulation
            thread2.Join();
            thread3.Join();
            //thread 6 needs to end before completed, if there are doctors left without patients
            if (thread6.IsAlive)
            {
                runDoctorMovement = false;
                thread6.Join();
            }
            
            args.SetExecutionTime();
            //In order for all threads to finish before invoking SimulationFinished
            Thread.Sleep(3000);
            SimulationFinished?.Invoke(this, args);
        }

        /// <summary>
        /// Simulates filling up the ICU and Sanatorium with patients according to symptom-level and patients age. Throws event PatientsSorted when done and
        /// if there are no more patients in neither the sanatorium or the queue, sets the cancellationToken to true
        /// </summary>
        public void SimulatePatientSorting()
        {
            PatientsSortedEventArgs args = new PatientsSortedEventArgs();
            while (!args.CancellationRequested)
            {
                //Reset the args values
                args.PatientsMovedToSanatorium = 0;
                args.PatientsMovedToICU = 0;
                
                Thread.Sleep(5000);
                using (var hospitalContext = new HospitalContext())
                {
                    var patientsInICU = (from patient in hospitalContext.Patients
                                         where patient.Status == PatientStatus.ICU
                                         select patient).ToList();
                    args.PatientsInICU = patientsInICU.Count;

                    //Check if ICU is full, otherwise, fill it
                    if (patientsInICU.Count < maxICU)
                    {
                        var patientsToMove = (from patient in hospitalContext.Patients
                                              where patient.Status == PatientStatus.Queue || patient.Status == PatientStatus.Sanatorium
                                              orderby patient.SymptomLevel descending, patient.BirthDate ascending
                                              select patient).Take(maxICU - patientsInICU.Count).ToList();
                        //Change status of the patients provided, to ICU
                        for (int i = 0; i < patientsToMove.Count; i++)
                        {
                            patientsToMove[i].Status = PatientStatus.ICU;
                        }
                        args.PatientsMovedToICU = patientsToMove.Count;
                        args.PatientsInICU += patientsToMove.Count;
                    }

                    hospitalContext.SaveChanges();

                    var patientsInSanatorium = (from patient in hospitalContext.Patients
                                                where patient.Status == PatientStatus.Sanatorium
                                                select patient).ToList();
                    args.PatientsInSanatorium = patientsInSanatorium.Count;

                    //Check if Sanatorium is full, otherwise, fill it
                    if (patientsInSanatorium.Count < maxSanatorium)
                    {
                        var patientsToMove = (from patient in hospitalContext.Patients
                                              where patient.Status == PatientStatus.Queue
                                              orderby patient.SymptomLevel descending, patient.BirthDate ascending
                                              select patient).Take(maxSanatorium - patientsInSanatorium.Count).ToList();
                        //Change status of the patients provided, to Sanatorium
                        for (int i = 0; i < patientsToMove.Count; i++)
                        {
                            patientsToMove[i].Status = PatientStatus.Sanatorium;
                        }
                        args.PatientsMovedToSanatorium = patientsToMove.Count;
                        args.PatientsInSanatorium += patientsToMove.Count;
                    }
                    hospitalContext.SaveChanges();
                }
                PatientsSorted?.Invoke(this, args);
                //If there are no patients left, stop the thread
                if (args.PatientsInICU == 0 && args.PatientsInSanatorium == 0)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Simulates time passing at the hospital, symptoms of the different patients alter, and doctors energy drops
        /// </summary>
        public void SimulateSymptomsChanging()
        {
            bool patientsPresent = true;
            while (patientsPresent)
            {
                Thread.Sleep(3000);
                using (var hospitalContext = new HospitalContext())
                {
                    var currentPatients = (from patient in hospitalContext.Patients
                                           where patient.Status != PatientStatus.Afterlife && patient.Status != PatientStatus.Recovered
                                           select patient).ToList();
                    var doctorICU = (from doctor in hospitalContext.Doctors
                                     where doctor.Assignment == DoctorAssignment.ICU
                                     select doctor).FirstOrDefault();
                    var doctorSanatorium = (from doctor in hospitalContext.Doctors
                                            where doctor.Assignment == DoctorAssignment.Sanatorium
                                            select doctor).FirstOrDefault();
                    for(int i=0; i<currentPatients.Count; i++)
                    {
                        if(currentPatients[i].Status == PatientStatus.ICU)
                        {
                            currentPatients[i].SymptomLevel += SymptomChangeInICU();
                            if(doctorICU != null && doctorICU.Energy > 0)
                            {
                                currentPatients[i].SymptomLevel += DoctorLowerSymptoms(doctorICU.Skill);
                            }
                        }
                        else if(currentPatients[i].Status == PatientStatus.Sanatorium)
                        {
                            currentPatients[i].SymptomLevel += SymptomChangeInSanatorium();
                            if (doctorSanatorium != null && doctorSanatorium.Energy > 0)
                            {
                                currentPatients[i].SymptomLevel += DoctorLowerSymptoms(doctorSanatorium.Skill);
                            }
                        }
                        else if(currentPatients[i].Status == PatientStatus.Queue)
                        {
                            currentPatients[i].SymptomLevel += SymptomChangeInQueue();
                        }
                    }
                    //Lower the energy of the doctors
                    if(doctorICU != null && doctorICU.Energy != 0)
                    {
                        doctorICU.Energy -= 1;
                    }
                    if(doctorSanatorium != null && doctorSanatorium.Energy != 0)
                    {
                        doctorSanatorium.Energy -= 1;
                    }
                    //if there are no patients, end the simulation
                    if(currentPatients.Count < 1)
                    {
                        patientsPresent = false;
                    }

                    hospitalContext.SaveChanges();
                }
            }

        }

        /// <summary>
        /// Checks all the patients in the hospital, and if they have recovered or died, switches their statuses
        /// </summary>
        public void SimulatePatientRecoveringOrDying()
        {
            PatientsRecoveredOrDiedEventArgs args = new PatientsRecoveredOrDiedEventArgs();
            while (!args.CancellationRequested)
            {
                Thread.Sleep(5000);
                //Reset args-values
                args.PatientsRecovered = 0;
                args.PatientsDied = 0;
                args.PatientsLeft = 0;
                using (var hospitalContext = new HospitalContext())
                {
                    var patients = (from patient in hospitalContext.Patients
                                    where patient.Status != PatientStatus.Afterlife && patient.Status != PatientStatus.Recovered
                                    select patient).ToList();
                    for(int i=0; i<patients.Count; i++)
                    {
                        if(patients[i].SymptomLevel < 1)
                        {
                            patients[i].Status = PatientStatus.Recovered;
                            args.PatientsRecovered += 1;
                        }
                        else if(patients[i].SymptomLevel > 9)
                        {
                            patients[i].Status = PatientStatus.Afterlife;
                            args.PatientsDied += 1;
                        }
                        else
                        {
                            args.PatientsLeft += 1;
                        }
                    }

                    hospitalContext.SaveChanges();
                }
                PatientsRecoveredOrDied?.Invoke(this, args);
            }
        }

        /// <summary>
        /// Checks the doctors every 5 seconds. If they have 0 energy, move them to Assignment Done. If the wards doesn't have a doctor, add the best one (for ICU) or the worst one (for sanatorium)
        /// </summary>
        public void SimulateDoctorMovement()
        {
            while (runDoctorMovement)
            {
                Thread.Sleep(5000);
                using (var hospitalContext = new HospitalContext())
                {
                    var doctors = (from doctor in hospitalContext.Doctors
                                   where doctor.Assignment != DoctorAssignment.Done
                                   select doctor).ToList();
                    //First, set aside the doctors without any energy left
                    //and check if there is any doctors assigned to ICU or Sanatorium after
                    bool doctorICUPresent = false;
                    bool doctorSanatoriumPresent = false;
                    for(int i=0; i<doctors.Count; i++)
                    {
                        if(doctors[i].Energy < 1)
                        {
                            doctors[i].Assignment = DoctorAssignment.Done;
                        }
                        if(doctors[i].Assignment == DoctorAssignment.ICU)
                        {
                            doctorICUPresent = true;
                        }
                        if(doctors[i].Assignment == DoctorAssignment.Sanatorium)
                        {
                            doctorSanatoriumPresent = true;
                        }
                    }
                    //If there is no ICU-doctor, assign the best one with energy left
                    if (!doctorICUPresent)
                    {
                        var newICUDoctor = (from doctor in hospitalContext.Doctors
                                            where doctor.Energy > 0
                                            orderby doctor.Skill descending
                                            select doctor).FirstOrDefault();
                        //Check so that there was a doctor to add
                        if(newICUDoctor != null)
                        {
                            //If the doctor to put in ICU is the sanatorium-doctor, set boolean for sanatoriumDoc to false
                            if(newICUDoctor.Assignment == DoctorAssignment.Sanatorium)
                            {
                                doctorSanatoriumPresent = false;
                            }
                            newICUDoctor.Assignment = DoctorAssignment.ICU;
                            doctorICUPresent = true;
                        }
                    }
                    //if there is no Sanatorium-doctor, assign the worst one with energy left
                    if (!doctorSanatoriumPresent)
                    {
                        var newSanatoriumDoctor = (from doctor in hospitalContext.Doctors
                                            where doctor.Energy > 0
                                            orderby doctor.Skill ascending
                                            select doctor).FirstOrDefault();
                        //Check so that there was a doctor to add, and that the doctor isn't assigned to ICU
                        //If assigned to ICU, that is the last doctor and there is no other to add, so do nothing
                        if (newSanatoriumDoctor != null && newSanatoriumDoctor.Assignment != DoctorAssignment.ICU)
                        {
                            newSanatoriumDoctor.Assignment = DoctorAssignment.Sanatorium;
                            doctorICUPresent = true;
                        }  
                    }
                    hospitalContext.SaveChanges();

                    //If no doctors are present in the wards after running, no doctors are left, and the simulation can end
                    if (!doctorICUPresent && !doctorSanatoriumPresent)
                    {
                        runDoctorMovement = false;
                    }
                }

            }
        }

        /// <summary>
        /// Randomly chooses to either return -1, 1, 3 or 0
        /// </summary>
        /// <returns>an integer representation of SymptomLevel changes in Queue</returns>
        private int SymptomChangeInQueue()
        {
            //10% to lower with one
            //30% to raise with one
            //10% to raise with three
            //50% to not change
            int randomNumber = rnd.Next(0, 100);
            if (randomNumber <= 10)
            {
                return -1;
            }
            else if (randomNumber <= 40)
            {
                return 1;
            }
            else if (randomNumber <= 50)
            {
                return 3;
            }
            return 0;
        }

        /// <summary>
        /// Randomly chooses to either return 1, 2, -3 or 0
        /// </summary>
        /// <returns>an integer representation of SymptomLevel changes in ICU</returns>
        private int SymptomChangeInICU()
        {
            //10% to raise with one
            //10% to raise with two
            //60% to lower with three
            //20% to not change
            int randomNumber = rnd.Next(0, 100);
            if (randomNumber <= 10)
            {
                return 1;
            }
            else if (randomNumber <= 20)
            {
                return 2;
            }
            else if (randomNumber <= 80)
            {
                return -3;
            }
            return 0;
        }

        /// <summary>
        /// Randomly chooses to either return -1, 1, 3 or 0
        /// </summary>
        /// <returns>an integer representation of SymptomLevel changes in Sanatorium</returns>
        private int SymptomChangeInSanatorium()
        {
            //20% to lower with one
            //10% to raise with one
            //5% to raise with three
            //65% to not change
            int randomNumber = rnd.Next(0, 100);
            if (randomNumber <= 20)
            {
                return -1;
            }
            else if (randomNumber <= 30)
            {
                return 1;
            }
            else if (randomNumber <= 35)
            {
                return 3;
            }
            return 0;
        }

        /// <summary>
        /// Takes input skill and attempts to lower symptoms by 1
        /// </summary>
        /// <param name="doctorSkill"></param>
        /// <returns>either 0 or -1</returns>
        private int DoctorLowerSymptoms(int doctorSkill)
        {
            if(doctorSkill >= rnd.Next(0, 100))
            {
                return -1;
            }

            return 0;
        }
    }
}
