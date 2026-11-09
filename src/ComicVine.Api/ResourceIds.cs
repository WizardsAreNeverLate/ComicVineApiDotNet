using System.Globalization;

namespace ComicVine.Api
{
    /// <summary>Builds the type-prefixed ids (such as <c>4005-1699</c>) that Comic Vine detail URLs require.</summary>
    public static class ResourceIds
    {
        public static string Issue(int id) => Format(4000, id);
        public static string Character(int id) => Format(4005, id);
        public static string Publisher(int id) => Format(4010, id);
        public static string Concept(int id) => Format(4015, id);
        public static string Location(int id) => Format(4020, id);
        public static string Movie(int id) => Format(4025, id);
        public static string Origin(int id) => Format(4030, id);
        public static string Power(int id) => Format(4035, id);
        public static string Person(int id) => Format(4040, id);
        public static string StoryArc(int id) => Format(4045, id);
        public static string Volume(int id) => Format(4050, id);
        public static string Object(int id) => Format(4055, id);
        public static string Team(int id) => Format(4060, id);
        public static string Episode(int id) => Format(4070, id);
        public static string Series(int id) => Format(4075, id);
        public static string Video(int id) => Format(2300, id);

        private static string Format(int prefix, int id) =>
            prefix.ToString(CultureInfo.InvariantCulture) + "-" + id.ToString(CultureInfo.InvariantCulture);
    }
}
