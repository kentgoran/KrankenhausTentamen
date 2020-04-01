using KrankenhausTentamen.Enums;
using System;

namespace KrankenhausTentamen
{
    public class Patient
    {
        private PatientStatus status;
        public int PatientId { get; set; }
        public string Name { get; set; }
        public string Ssn { get; set; }
        public DateTime BirthDate { get; set; }
        public int SymptomLevel { get; set; }
        public DateTime ArrivalTime { get; set; }
        public TimeSpan? TimeSpentInQueue { get; set; }
        public PatientStatus Status
        {
            get
            {
                return status;
            }
            set
            {
                if(status == PatientStatus.Queue && value != PatientStatus.Queue)
                {
                    this.PatientLeftQueue();
                }
                status = value;
            }
        }

        public Patient()
        {
               
        }

        private void PatientLeftQueue()
        {
            TimeSpentInQueue = DateTime.Now - ArrivalTime;
        }
    }
}
