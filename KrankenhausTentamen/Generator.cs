using KrankenhausTentamen.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KrankenhausTentamen
{
    public class Generator
    {
        private Random rnd = new Random();
        private DateTime earliestBirth = new DateTime(1920, 01, 01);
        private List<string> lastNames;
        private List<string> firstNames;

        public Generator()
        {
            InitiateNames();
        }

        /// <summary>
        /// Generates a number of patients, with name, symptom-levels and date of birth randomized
        /// </summary>
        /// <param name="numberOfPatients">the amount of patients to generate</param>
        /// <returns>a list of patients</returns>
        public List<Patient> GeneratePatients(int numberOfPatients = 30)
        {
            List<Patient> patients = new List<Patient>(); 
            for(int i=0; i< numberOfPatients; i++)
            {
                Patient patient = new Patient()
                {
                    ArrivalTime = DateTime.Now,
                    Name = NameGenerator(),
                    BirthDate = DateOfBirthGenerator(),
                    Status = PatientStatus.Queue,
                    SymptomLevel = rnd.Next(1,10),
                };
                patient.Ssn = SsnGenerator(patient.BirthDate);
                patients.Add(patient);
            }
            return patients;
        }

        /// <summary>
        /// Generates a number of doctors, with name and skill-level
        /// </summary>
        /// <param name="numberOfDoctors">the number of doctors to return, default is 10</param>
        /// <returns>a List of doctors</returns>
        public List<Doctor> GenerateDoctors(int numberOfDoctors = 10)
        {
            List<Doctor> doctors = new List<Doctor>();
            for(int i=0; i<numberOfDoctors; i++)
            {
                Doctor doctor = new Doctor()
                {
                    Name = NameGenerator(),
                    Skill = rnd.Next(1, 100),
                    Energy = 3,
                    Assignment = DoctorAssignment.Waiting

                };
                doctors.Add(doctor);
            }


            return doctors;
        }

        /// <summary>
        /// Generates an SSN from a given DateTime
        /// </summary>
        /// <param name="birthDate">the day of birth to convert to SSN</param>
        /// <returns>a string representation of an SSN (YYYYMMDDxxxx)</returns>
        private string SsnGenerator(DateTime birthDate)
        {
            StringBuilder stringToReturn = new StringBuilder();
            stringToReturn.Append(birthDate.Year.ToString());
            
            if(birthDate.Month < 10)
            {
                stringToReturn.Append("0");
            }
            stringToReturn.Append(birthDate.Month.ToString());

            if(birthDate.Day < 10)
            {
                stringToReturn.Append("0");
            }
            stringToReturn.Append(birthDate.Day.ToString());

            //Four last digits entered here
            //This does however NOT make this a legit ssn, since one of the numbers are a control number
            stringToReturn.Append(rnd.Next(1000, 9999).ToString());
            return stringToReturn.ToString();
        }

        /// <summary>
        /// Generates a datetime between "earliestBirth" and today
        /// </summary>
        /// <returns>a DateTime with a made-up date of birth</returns>
        private DateTime DateOfBirthGenerator()
        {
            //amount of days until today, from the earliest determined datetime.
            //Adds between 0 and days until today to the earliest datetime
            int daysSpan = (DateTime.Now - earliestBirth).Days;
            return earliestBirth.AddDays(rnd.Next(0, daysSpan));
        }

        /// <summary>
        /// Generates a full name, first name + " " + last name
        /// </summary>
        /// <returns>a Name</returns>
        private string NameGenerator()
        {
            string fullName = firstNames[rnd.Next(0, firstNames.Count - 1)] 
                + " " + lastNames[rnd.Next(0, lastNames.Count - 1)];
            return fullName;
        }
        /// <summary>
        /// Fills the lists of names with actual names. Only put in a method to make it look cleaner
        /// </summary>
        private void InitiateNames()
        {
            lastNames = new List<string> { "Westman", "Wahlgren", "Johansson", 
                "Larsson", "Krcic", "Appelgren", "Namitabar", "Doe", "Smith", 
                "Pettersson", "Mohammad", "Lycke", "Svensson", 
                "Svendsen", "Tannenberg", "Croon", "Pool", "Martinique", "Nilsson", "Jutemark"};
            firstNames = new List<string> { "Simon", "Victoria", "Vincent", "Luna", "Jesper", 
                "Linn", "Johanna", "Peter", "Mahmoud", "Svend", "Thorgrim", "Ryan", "Johan", 
                "John", "Saga", "Emelie", "Denisa", "Patrik", "Paul", "Claes", "Jimmy", 
                "Dennis", "Stella", "Josefine", "Emma", "Martina", "Mabel", "Constantin", "Vladimir" };
        }
    }
}
