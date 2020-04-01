using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrankenhausTentamen.EventArguments
{
    public class PatientsSortedEventArgs : EventArgs
    {
        public bool CancellationRequested { get; set; }
        public int PatientsInICU { get; set; }
        public int PatientsInSanatorium { get; set; }

        public PatientsSortedEventArgs(bool cancellationRequested = false)
        {
            this.CancellationRequested = cancellationRequested;
        }
    }
}
