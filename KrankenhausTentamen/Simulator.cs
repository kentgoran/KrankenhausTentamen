using KrankenhausTentamen.EventArguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KrankenhausTentamen.Enums;

namespace KrankenhausTentamen
{
    public class Simulator
    {
        public EventHandler<PatientsSortedEventArgs> PatientsSorted;
        public EventHandler<PatientsSymptomsChangedEventArgs> PatientsSymptomsChanged;
        public EventHandler<PatientsRecoveredOrDiedEventArgs> PatientsRecoveredOrDied;

        /// <summary>
        /// Simulates filling up the ICU and Sanatorium with patients according to symptom-level and patients age. Throws event PatientsSorted when done and
        /// if there are no more patients in neither the sanatorium or the queue, sets the cancellationToken to true
        /// </summary>
        public void SimulatePatientSorting()
        {
            PatientsSortedEventArgs args = new PatientsSortedEventArgs();
            using (var hospitalContext = new HospitalContext())
            {
                hospitalContext.Database.Log = Console.WriteLine;
                var patientsInICU = (from patient in hospitalContext.Patients
                                     where patient.Status == PatientStatus.ICU
                                     select patient).ToList();
                //Check if ICU is full, otherwise, fill it
                if (patientsInICU.Count < 5)
                {
                    //LINQ statement getting all patients to put in the ICU
                    var sortedPatients = (from patient in hospitalContext.Patients
                                         where patient.Status == PatientStatus.Queue || patient.Status == PatientStatus.Sanatorium
                                         orderby patient.SymptomLevel descending, patient.BirthDate ascending
                                         select patient).Take(5-patientsInICU.Count).ToList();
                    //Change status of the patients provided, to ICU
                    for(int i=0; i<sortedPatients.Count; i++)
                    {
                        sortedPatients[i].Status = PatientStatus.ICU;
                    }
                    args.PatientsInICU = patientsInICU.Count + sortedPatients.Count;
                }
                hospitalContext.SaveChanges();

                var patientsInSanatorium = (from patient in hospitalContext.Patients
                                            where patient.Status == PatientStatus.Sanatorium
                                            select patient).ToList();

                //Check if Sanatorium is full, otherwise, fill it
                if (patientsInSanatorium.Count < 10)
                {
                    //LINQ statement getting all patients to put in the Sanatorium
                    var sortedPatients = (from patient in hospitalContext.Patients
                                          where patient.Status == PatientStatus.Queue
                                          orderby patient.SymptomLevel descending, patient.BirthDate ascending
                                          select patient).Take(10 - patientsInSanatorium.Count).ToList();
                    //Change status of the patients provided, to Sanatorium
                    for (int i = 0; i < sortedPatients.Count; i++)
                    {
                        sortedPatients[i].Status = PatientStatus.Sanatorium;
                    }
                    args.PatientsInSanatorium = patientsInSanatorium.Count + sortedPatients.Count;
                }

                //No more moving patients in this simulation needed, if there's no patients in sanatorium
                if(args.PatientsInSanatorium < 1)
                {
                    args.CancellationRequested = true;
                }
                hospitalContext.SaveChanges();
            }
            PatientsSorted?.Invoke(this, args);
        }
        public void SimulateSymptomsChanging()
        {
            throw new NotImplementedException();
        }
        public void SimulatePatientRecoveringOrDying()
        {
            throw new NotImplementedException();
        }
        public void SimulateDoctorMovement()
        {
            throw new NotImplementedException();
        }
    }
}
