namespace LichDeBan.Helpers
{
    public static class CanChiHelper
    {
        private static readonly string[] Can = { "Canh", "Tân", "Nhâm", "Quý", "Giáp", "Ất", "Bính", "Đinh", "Mậu", "Kỷ" };
        private static readonly string[] Chi = { "Thân", "Dậu", "Tuất", "Hợi", "Tý", "Sửu", "Dần", "Mão", "Thìn", "Tỵ", "Ngọ", "Mùi" };

        public static string GetCanChiYear(int lunarYear)
        {
            string can = Can[lunarYear % 10];
            string chi = Chi[lunarYear % 12];
            return $"{can} {chi}";
        }
    }
}
