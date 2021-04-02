namespace Piipan.Match.Orchestrator
{
    static class LookupId
    {
        private const string Chars = "23456789BCDFGHJKLMNPQRSTVWXYZ";
        private const int Length = 7;

        /// <summary>
        /// Generate a random alphanumeric ID that conforms to a limited alphabet
        /// limited alphabet and fixed length.
        /// </summary>
        /// <returns>A random 7-character alpha-numeric ID string</returns>
        public static string Generate()
        {
            return Nanoid.Nanoid.Generate(Chars, Length);
        }
    }
}
