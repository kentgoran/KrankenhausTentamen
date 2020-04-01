namespace KrankenhausTentamen.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FirstMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Doctors",
                c => new
                    {
                        DoctorId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Energy = c.Int(nullable: false),
                        Skill = c.Int(nullable: false),
                        Assignment = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.DoctorId);
            
            CreateTable(
                "dbo.Patients",
                c => new
                    {
                        PatientId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Ssn = c.String(),
                        BirthDate = c.DateTime(nullable: false),
                        SymptomLevel = c.Int(nullable: false),
                        ArrivalTime = c.DateTime(nullable: false),
                        TimeSpentInQueue = c.Time(precision: 7),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PatientId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Patients");
            DropTable("dbo.Doctors");
        }
    }
}
