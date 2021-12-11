using ParserLibrary;
using System;
using System.Collections.Generic;

namespace ODExplorer.CsvControl
{
    [Serializable]
    public class CsvSave
    {
        public CsvSave() { }
        public CsvSave(CsvController controller)
        {
            Containers = new List<CsvContainer>(controller.CsvContainers);
            CurrentCsvType = controller.CurrentCsvType;
        }
        public CsvType CurrentCsvType { get; set; }
        public List<CsvContainer> Containers { get; set; }
    }
}
