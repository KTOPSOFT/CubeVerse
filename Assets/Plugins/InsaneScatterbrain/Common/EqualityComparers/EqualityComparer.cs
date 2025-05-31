namespace InsaneScatterbrain
{
    public static class EqualityComparer
    {
        private static Color32EqualityComparer color32;
        public static Color32EqualityComparer Color32 => color32 ?? (color32 = new Color32EqualityComparer());
    }
}