namespace LichDeBan.Helpers
{
    public class LunarInfo
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public bool IsLeapMonth { get; set; }
        public string CanChi { get; set; } = string.Empty;

        public override string ToString()
        {
            if (Day == 1)
                return $"{Day}/{Month}{(IsLeapMonth ? " (Nhuận)" : "")}";
            return Day.ToString();
        }
    }
}
