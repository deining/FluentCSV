namespace FluentCsv.FluentReader
{
    public sealed class CsvParameters
    {
        public string Source { get; set; }
        public string ColumnDelimiter { get; set; } = ";";
        public string EndLineDelimiter { get; set; } = "\r\n";
    }
}