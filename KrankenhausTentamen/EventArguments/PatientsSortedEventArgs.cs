using System;

namespace KrankenhausTentamen.EventArguments
{
    public class PatientsSortedEventArgs : EventArgs
    {
        public bool CancellationRequested { get; set; }
        public int PatientsInICU { get; set; }
        public int PatientsInSanatorium { get; set; }
        public int PatientsMovedToSanatorium { get; set; }
        public int PatientsMovedToICU { get; set; }

    }
}
