﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentCsv.CsvParser.Splitters;
using FluentCsv.Exceptions;

namespace FluentCsv.CsvParser
{
    public class CsvFileParser<TResult> where TResult : new()
    {
        public string LineDelimiter { get; set; } = "\r\n";
        public string ColumnDelimiter { get; set; } = ";";

        private HeaderIndex _headerIndex;

        private readonly string _source;
        private readonly IColumnSplitter _columnSplitter;

        public CsvFileParser(string source, IColumnSplitter columnSplitter)
        {
            _source = source;
            _columnSplitter = columnSplitter ?? throw new ArgumentNullException(nameof(columnSplitter));
        }

        public void DeclareFirstLineHasHeader()
            => _headerIndex = _headerIndex ?? new HeaderIndex(SplitColumns(SplitLines(_source).First()));

        private readonly ColumnsResolver<TResult> _columns = new ColumnsResolver<TResult>();

        public void AddColumn<TMember>(int index, Expression<Func<TResult, TMember>> into, Func<string, TMember> setInThisWay = null) 
            => _columns.AddColumn(index, into, setInThisWay, _headerIndex?.GetColumnName(index));

        public void AddColumn<TMember>(string columnName, Expression<Func<TResult, TMember>> into, Func<string, TMember> setInThisWay = null)
        {
            DeclareFirstLineHasHeader();
            _columns.AddColumn(_headerIndex.GetColumnIndex(columnName), into, setInThisWay, columnName);
        }

        public ParseCsvResult<TResult> Parse()
        {
            var currentLineNumber = 1;
            var resultSet = SplitLines(_source)
                .Skip(HeaderIfExists())
                .Select(line => _columns.GetResult(SplitColumns(line), currentLineNumber++))
                .ToList();

            return new ParseCsvResult<TResult>(
                resultSet.OfType<TResult>().ToArray(), 
                resultSet.OfType<CsvParseError>().ToArray());

            int HeaderIfExists()
            {
                if (_headerIndex == null)
                    return 0;
                currentLineNumber++;
                return 1;
            }
        }

        public string[] SplitColumns(string line)
            => _columnSplitter.Split(line, ColumnDelimiter);

        private string[] SplitLines(string source) 
            => source.Split(new[] {LineDelimiter}, StringSplitOptions.None);

        private class HeaderIndex
        {
            private readonly Dictionary<string, int> _headerToIndex = new Dictionary<string, int>();
            private readonly Dictionary<int, string> _indexToHeader = new Dictionary<int, string>();
            private readonly HashSet<string> _duplicateColumnName = new HashSet<string>();

            public HeaderIndex(string[] headers)
            {
                var columnIndex = 0;

                headers.ForEach(MapHeaderToIndex);

                void MapHeaderToIndex(string header)
                {
                    var headerName = header.Trim();
                    if (_headerToIndex.ContainsKey(headerName))
                        _duplicateColumnName.Add(headerName);
                    else
                    {
                        _headerToIndex.Add(headerName, columnIndex);
                        _indexToHeader.Add(columnIndex, headerName);
                    }
                    columnIndex++;
                }
            }

            public int GetColumnIndex(string columnName)
            {
                var headerName = columnName.Trim();

                if (_duplicateColumnName.Contains(headerName))
                    throw new DuplicateColumnNameException(headerName);
                if(!_headerToIndex.ContainsKey(headerName))
                    throw new ColumnNameNotFoundException(headerName);
                return _headerToIndex[headerName];
            }

            public string GetColumnName(int index)
            {
                return _indexToHeader.ContainsKey(index) ? _indexToHeader[index] : null;
            }
        }
    }
}