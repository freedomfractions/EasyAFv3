namespace EasyAF.Engine
{
    /// <summary>
    /// Default constants for diff operations.
    /// </summary>
    public static class DiffDefaults
    {
        /// <summary>
        /// Marker used to separate old and new values in diff mode.
        /// Example: "10.5 ? 12.3" shows old value (10.5) changed to new value (12.3)
        /// </summary>
        public const string DiffMarker = " ? ";
    }
}
