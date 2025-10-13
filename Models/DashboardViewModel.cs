namespace NoSQL_Project.Models
{
  
    public class DashboardViewModel
    {
        // counts
        public int Total { get; set; }
        public int OpenCount { get; set; }                 // Open + InProgress + OnHold
        public int ResolvedCount { get; set; }             // Status == Resolved
        public int ClosedWithoutResolveCount { get; set; } // Status == Closed

        // percentages
        public double OpenPct => Total == 0 ? 0 : (OpenCount * 100.0 / Total);
        public double ResolvedPct => Total == 0 ? 0 : (ResolvedCount * 100.0 / Total);
        public double ClosedWithoutResolvePct => Total == 0 ? 0 : (ClosedWithoutResolveCount * 100.0 / Total);
    }


}
