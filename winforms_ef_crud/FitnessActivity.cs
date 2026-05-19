using System;

namespace winforms_ef_crud
{
    // Simple POCO that represents a fitness activity record.
    public class FitnessActivity
    {
        public int FitnessActivityId { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public int CaloriesBurned { get; set; }
        public string Intensity { get; set; }
        public DateTime Date { get; set; }

            // Helpful ToString used by some controls when a DisplayMember is not set.
            public override string ToString() => $"{Title} ({Duration} min)";
    }
}
