using System;

namespace EasyAF.Data.Models
{
    public enum VersionSeverity { Info, Warn, Error }

    public static class VersionUtil
    {
        public static (VersionSeverity severity, string message) Compare(string actual, string expected, string label)
        {
            if (!TryParse(actual, out var a) || !TryParse(expected, out var e))
            {
                return (VersionSeverity.Warn, $"{label} version unparsable (actual='{actual}', expected='{expected}')");
            }
            if (a.Major != e.Major)
            {
                return (VersionSeverity.Error, $"{label} major version mismatch (actual={a.Major}, expected={e.Major})");
            }
            if (a.Minor < e.Minor)
            {
                return (VersionSeverity.Warn, $"{label} version older than expected (actual={a}, expected>={e})");
            }
            if (a.Minor > e.Minor)
            {
                return (VersionSeverity.Warn, $"{label} version newer than expected (actual={a}, expected={e})");
            }
            if (a.Build >= 0 && e.Build >= 0 && a.Build != e.Build)
            {
                return (VersionSeverity.Info, $"{label} patch/build differs (actual={a}, expected={e})");
            }
            return (VersionSeverity.Info, $"{label} version compatible (actual={a}, expected={e})");
        }

        public static bool TryParse(string v, out Version version)
        {
            version = new Version(0, 0, 0);
            if (string.IsNullOrWhiteSpace(v))
            {
                return false;
            }
            v = Normalize(v);
            if (Version.TryParse(v, out var parsed) && parsed != null)
            {
                version = parsed;
                return true;
            }
            return false;
        }

        public static string Normalize(string v)
        {
            var dotCount = 0;
            foreach (var c in v)
            {
                if (c == '.')
                {
                    dotCount++;
                }
            }
            return dotCount switch
            {
                0 => v + ".0.0",
                1 => v + ".0",
                _ => v
            };
        }
    }
}
