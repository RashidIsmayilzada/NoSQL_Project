namespace NoSQL_Project.ViewModels.Tickets
{
    public class TicketListVM
    {
        public IEnumerable<TicketListItemVM> Items { get; set; } = Enumerable.Empty<TicketListItemVM>();
        public bool IsServiceDesk { get; set; } // برای نمایش دکمه‌های اضافی

    }
}