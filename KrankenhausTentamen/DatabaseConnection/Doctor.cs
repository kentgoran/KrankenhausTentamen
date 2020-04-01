using KrankenhausTentamen.Enums;
using System.ComponentModel.DataAnnotations;

namespace KrankenhausTentamen
{
    public class Doctor
    {
        public int DoctorId { get; set; }
        //Required added as it otherwise gets nullable
        [Required]
        public string Name { get; set; }
        public int Energy { get; set; }
        public int Skill { get; set; }
        public DoctorAssignment Assignment { get; set; }

        public Doctor()
        {

        }
        public Doctor(string name, int skill, int energy = 3)
        {
            this.Name = name;
            this.Skill = skill;
            this.Energy = energy;

        }
    }
}
