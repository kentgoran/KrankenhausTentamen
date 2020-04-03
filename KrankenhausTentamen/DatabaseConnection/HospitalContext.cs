using System.Data.Entity;

namespace KrankenhausTentamen
{
    public class HospitalContext : DbContext
    {
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<SimulationData> SimulationData { get; set; }
    }
}
