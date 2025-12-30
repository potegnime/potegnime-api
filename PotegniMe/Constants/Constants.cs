namespace PotegniMe.Constants
{
    public static class Constants
    {
        public static readonly string TmdbLanguageEnUs = "en-US";
        public static readonly string TmdbLanguageSlSi = "sl-SI";
        public static readonly  string DefaultTimeWindow = "day";

        public static readonly  IDictionary<int, string> TmdbGenresEng = new Dictionary<int, string>
        {
            { 28, "Action" },
            { 12, "Adventure" },
            { 16, "Animation" },
            { 35, "Comedy" },
            { 80, "Crime" },
            { 99, "Documentary" },
            { 18, "Drama" },
            { 10751, "Family" },
            { 14, "Fantasy" },
            { 36, "History" },
            { 27, "Horror" },
            { 10402, "Music" },
            { 9648, "Mystery" },
            { 10749, "Romance" },
            { 878, "Science Fiction" },
            { 10770, "TV Movie" },
            { 53, "Thriller" },
            { 10752, "War" },
            { 37, "Western" },
            { 10759, "Action & Adventure" },
            { 10762, "Kids" },
            { 10763, "News" },
            { 10764, "Reality" },
            { 10765, "Sci-Fi & Fantasy" },
            { 10766, "Soap" },
            { 10767, "Talk" },
            { 10768, "War & Politics" }
        };

        public static readonly  IDictionary<int, string> TmdbGenresSl = new Dictionary<int, string>
        {
            { 28, "Akcija" },
            { 12, "Pustolovščina" },
            { 16, "Animacija" },
            { 35, "Komedija" },
            { 80, "Kriminalka" },
            { 99, "Dokumentarni" },
            { 18, "Drama" },
            { 10751, "Družinski" },
            { 14, "Fantazija" },
            { 36, "Zgodovinski" },
            { 27, "Grozljivka" },
            { 10402, "Glasbeni" },
            { 9648, "Misterij" },
            { 10749, "Romantični" },
            { 878, "Znanstvena fantastika" },
            { 10770, "TV film" },
            { 53, "Triler" },
            { 10752, "Vojni" },
            { 37, "Vestern" },
            { 10759, "Akcija & Pustolovščina" },
            { 10762, "Otroški" },
            { 10763, "Novice" },
            { 10764, "Resničnostni" },
            { 10765, "Znanstvena fantastika & Fantazija" },
            { 10766, "Telenovela" },
            { 10767, "Pogovorna oddaja" },
            { 10768, "Vojna & Politika" }
        };

        public static readonly string DotEnvErrorCode = "POTEGNIME_DOTENV_ERROR";
        public static readonly string AppSettingsErrorCode = "POTEGNIME_APPSETTINGS_ERROR";
        public static readonly string InternalErrorCode = "POTEGNIME_INTERNAL_ERROR";
    }
}