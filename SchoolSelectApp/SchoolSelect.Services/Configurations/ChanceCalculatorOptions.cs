namespace SchoolSelect.Services.Configurations
{
    public class ChanceCalculatorOptions
    {
        public double HighThreshold { get; set; } = 20;
        public double HighMinChance { get; set; } = 75;
        public double HighMaxChance { get; set; } = 95;
        public double LowThreshold { get; set; } = 20;
        public double LowMinChance { get; set; } = 15;
        public double LowMaxChance { get; set; } = 75;
        public double VeryHighChance { get; set; } = 99;
        public double VeryLowChance { get; set; } = 10;
        public double DefaultChance { get; set; } = 50;
    }
}
