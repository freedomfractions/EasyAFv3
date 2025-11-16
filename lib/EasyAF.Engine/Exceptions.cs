using System;

namespace EasyAF.Engine
{
    public class SpecLoadException : Exception
    {
        public SpecLoadException(string message) : base(message) { }
        public SpecLoadException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class MappingException : Exception
    {
        public MappingException(string message) : base(message) { }
        public MappingException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class TemplatePopulationException : Exception
    {
        public string? TableId { get; }
        public int? ColumnIndex { get; }
        public TemplatePopulationException(string message, string? tableId = null, int? columnIndex = null) : base(message)
        { TableId = tableId; ColumnIndex = columnIndex; }
        public TemplatePopulationException(string message, Exception innerException, string? tableId = null, int? columnIndex = null) : base(message, innerException)
        { TableId = tableId; ColumnIndex = columnIndex; }
    }
}
