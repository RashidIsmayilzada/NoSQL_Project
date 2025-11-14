namespace NoSQL_Project.ViewModels.Dashboard
{


    public record DonutDataset(
      IList<string> Labels,
      IList<int> Data,
      int Total
  );
    public class DashboardVM
    {
       
        //Chart 1: Ticket Status Percentage
        public DonutDataset StatusBreakdown { get; set; } =
            new(new List<string>(), new List<int>(), 0);
        public bool IsServiceDesk { get; set; }
        // Chart2: open vs overdue
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int OverdueOpen { get; set; }
    }
}

