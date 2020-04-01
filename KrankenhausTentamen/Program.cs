using KrankenhausTentamen.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace KrankenhausTentamen
{
    class Program
    {
        static void Main(string[] args)
        {
            Generator generator = new Generator();
            var doctors = generator.GenerateDoctors();
            using (var context = new HospitalContext())
            {
                context.Database.Log = Console.WriteLine;
                context.Doctors.AddRange(doctors);
                context.SaveChanges();
            }
            //Generator generator = new Generator();
            //var patients = generator.GeneratePatients(30);
            //foreach(var patient in patients)
            //{
            //    Console.WriteLine($"{patient.Name}\n{patient.BirthDate:yyyyMMdd}\nSSN: {patient.Ssn}\nsymptoms: {patient.SymptomLevel}");
            //    Console.WriteLine("-------------------");
            //}
            Console.ReadLine();
        }
    }
}
