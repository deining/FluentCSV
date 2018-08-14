using FluentCsv.CsvParser;
using FluentCsv.CsvParser.Splitters;

namespace FluentCsv.FluentReader
{
    public class ResultSetBuilder : CsvParametersContainer
    {        
        internal ResultSetBuilder(CsvParameters csvParameters) : base(csvParameters){}                

        public ColumnsBuilder<TLine> ArrayOf<TLine>() where TLine : new()
        {
            var parser = new CsvFileParser<TLine>(CsvParameters.Source, CsvParameters.DataSplitter, CsvParameters.CultureInfo)
            {
                ColumnDelimiter = CsvParameters.ColumnDelimiter,
                LineDelimiter = CsvParameters.EndLineDelimiter,
                HeadersAsCaseInsensitive = CsvParameters.HeaderCaseInsensitive,
            };

            if(CsvParameters.FirstLineHasHeader)
                parser.DeclareFirstLineHasHeader();

            return new ColumnsBuilder<TLine>(parser);
        }
    }
}