namespace DryLatexBackend
{
    public static class DailyPriceStore
    {
        public static bool IsPriceSet = false;

        public static Dictionary<string, Dictionary<string, decimal>> Prices
            = new Dictionary<string, Dictionary<string, decimal>>();
    }
}
