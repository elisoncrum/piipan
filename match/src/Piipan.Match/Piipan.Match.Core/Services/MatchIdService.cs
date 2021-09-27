namespace Piipan.Match.Core.Services
{
    public class MatchIdService : IMatchIdService
    {
        private const string Chars = "23456789BCDFGHJKLMNPQRSTVWXYZ";
        private const int Length = 7;

        public string GenerateId()
        {
            return Nanoid.Nanoid.Generate(Chars, Length);
        }
    }
}
