using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ParserLibrary
{
    public class CsvParser
    {
        public static List<ExplorationTarget> ParseCsv(string filename)
        {
            List<ExplorationTarget> targets = new();

            try
            {
                using TextFieldParser parser = new(filename);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                while (!parser.EndOfData)
                {
                    //Process row
                    string[] fields = parser.ReadFields();

                    if (fields.Length < 7)
                    {
                        //Not a line from spansh
                        continue;
                    }

                    string sysname = fields[0].Trim('"');

                    //Skip the headers line.
                    if (sysname.Contains("System"))
                    {
                        continue;
                    }

                    ExplorationTarget target = targets.Find(x => x.SystemName.Contains(sysname, StringComparison.OrdinalIgnoreCase));

                    if (target == null)
                    {
                        target = new ExplorationTarget
                        {
                            SystemName = sysname.ToUpperInvariant()
                        };

                        targets.Add(target);
                    }

                    BodiesInfo bodyinfo = new()
                    {
                        Body = fields[1].Trim('"').Remove(0, target.SystemName.Length).ToUpperInvariant(),
                        Distance = $"{int.Parse(fields[4].Trim('"'), new CultureInfo("en-GB")):n0}",
                        EstimatedMappingValue = $"{int.Parse(fields[6].Trim('"'), new CultureInfo("en-GB")):n0}"
                    };

                    if (target.BodiesInfo == null)
                    {
                        target.BodiesInfo = new List<BodiesInfo>();
                    }
                    target.BodiesInfo.Add(bodyinfo);
                }
                return targets;
            }
            catch
            {
                return targets;
            }

        }
    }
}