using System.Collections.Generic;

namespace ParserLibrary
{
    public class CsvParserReturn
    {
        public CsvType CsvType { get; set; }

        public List<ExplorationTarget> Targets;
    }
}