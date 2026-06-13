namespace CMGWpf.Utilities
{
    /// <summary>
    /// A utility class for working with XML elements, providing methods to retrieve attribute values as different data types (bool, string, int, double) with default values if the attribute is not found or cannot be parsed. This class simplifies the process of extracting and converting attribute values from XML elements in a consistent manner.
    /// </summary>
    public static class XMLFunctions
    {
        public static bool GetAttributeBool(System.Xml.XmlElement elem, string name, bool def)
        {
            string? val = elem.GetAttribute(name);
            bool success = bool.TryParse(val, out bool result);
            if (success) return result;
            return def;
        }
        public static string GetAttributeString(System.Xml.XmlElement elem, string name, string def)
        {
            string? val = elem.GetAttribute(name);
            if (string.IsNullOrEmpty(val)) return def;
            return val;
        }
        public static int GetAttributeInt(System.Xml.XmlElement elem, string name, int def)
        {
            string? val = elem.GetAttribute(name);
            if (val == null) return def;
            if (!int.TryParse(val, out int result)) return def;
            return result;
        }
        public static double GetAttributeDouble(System.Xml.XmlElement elem, string name, double def)
        {
            string? val = elem.GetAttribute(name);
            if (val == null) return def;
            if (!double.TryParse(val, out double result)) return def;
            return result;
        }
    }
}
