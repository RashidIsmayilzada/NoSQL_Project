namespace NoSQL_Project.ViewModels
{


    public record DonutDataset(
      IList<string> Labels,
      IList<int> Data,
      int Total
  );
    public class DashboardVM
    {
        // چارت ۱: درصد وضعیت تیکت‌ها 
        public DonutDataset StatusBreakdown { get; set; } =
            new(new List<string>(), new List<int>(), 0);
        public bool IsServiceDesk { get; set; }
        // چارت ۲: open vs overdue
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int OverdueOpen { get; set; }
    }
}

