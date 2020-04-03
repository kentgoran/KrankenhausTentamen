namespace KrankenhausTentamen.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSimulationData : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SimulationDatas",
                c => new
                    {
                        SimulationDataId = c.Int(nullable: false, identity: true),
                        AverageTimeSpentInQueue = c.Time(nullable: false, precision: 7),
                        AmountOfRecoveredPatients = c.Int(nullable: false),
                        AmountOfDeadPatients = c.Int(nullable: false),
                        ExecutionTime = c.Time(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.SimulationDataId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SimulationDatas");
        }
    }
}
