using KrankenhausTentamen.Enums;
using KrankenhausTentamen.EventArguments;
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
            var patients = generator.GeneratePatients();
            using (var context = new HospitalContext())
            {
                context.Database.Log = Console.WriteLine;
                context.Patients.AddRange(patients);
                context.SaveChanges();
            }
            Console.Write("Finished adding patients... Press any key to fill ICU and Sanatorium once.");
            Console.ReadLine();
            Simulator sim = new Simulator();
            var time = DateTime.Now;
            sim.SimulatePatientSorting();
            Console.Write("Done simulating sorting. This took {0:0} ms", (DateTime.Now - time).TotalMilliseconds);
            Console.ReadLine();
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
