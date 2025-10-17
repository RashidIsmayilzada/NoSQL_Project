using Microsoft.AspNetCore.Mvc;

namespace NoSQL_Project.ViewComponents
{
    public class DonutChartViewComponent : ViewComponent
    {
        public class DonutChartModel
        {
            public string CanvasId { get; init; } = "chart-" + Guid.NewGuid().ToString("N");
            public string Title { get; init; } = "";
            public IList<string> Labels { get; init; } = new List<string>();
            public IList<int> Data { get; init; } = new List<int>();
            public int? Total { get; init; } // برای نمایش x/y اگر بخوای
        }

        public IViewComponentResult Invoke(DonutChartModel model) => View(model);
    }
}