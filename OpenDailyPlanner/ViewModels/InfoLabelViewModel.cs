namespace OpenDailyPlanner.ViewModels
{
    public class InfoLabelViewModel
    {
        public InfoLabelType Type { get; set; }
        public string Message { get; set; }
    }

    public enum InfoLabelType { Info, Warning, Error, Succces }
}
