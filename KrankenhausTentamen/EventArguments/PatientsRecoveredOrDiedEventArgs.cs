using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrankenhausTentamen.EventArguments
{
    public class PatientsRecoveredOrDiedEventArgs : EventArgs
    {
        public bool CancellationRequested { get; set; }
        public int PatientsRecovered { get; set; }
        public int PatientsDied { get; set; }
        public int PatientsLeft { get; set; }
    }
}
