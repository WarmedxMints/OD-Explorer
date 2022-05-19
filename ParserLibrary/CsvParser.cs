using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace ParserLibrary
{
    public enum CsvType
    {
        [Description("Road To Riches")]
        RoadToRiches = 0,
        [Description("Fleet Carrier Route")]
        FleetCarrier,
        [Description("Neutron Plotter")]
        NeutronRoute,
        [Description("Galaxy Plotter")]
        GalaxyPlotter,
        [Description("World Type Route")]
        WorldTypeRoute,
        [Description("Tourist Route")]
        TouristRoute,
        [Description("Exobiology Route")]
        Exobiology
    }

    public class CsvParser
    {
        private static readonly List<string[]> csvHeaders = new()
        {
            new string[] { "System Name", "Body Name", "Body Subtype", "Is Terraformable", "Distance To Arrival", "Estimated Scan Value", "Estimated Mapping Value", "Jumps" }, //roadToRichesRoute
            new string[] { "System Name", "Distance", "Distance Remaining", "Tritium in tank", "Tritium in market", "Fuel Used", "Icy Ring", "Pristine", "Restock Tritium" }, //fleetCarrierRoute
            new string[] { "System Name", "Distance To Arrival", "Distance Remaining", "Neutron Star", "Jumps" }, //neutronRoute
            new string[] { "System Name", "Distance", "Distance Remaining", "Fuel Left", "Fuel Used", "Refuel", "Neutron Star" }, //galaxyPlotterRoute
            new string[] { "System Name", "Body Name", "Distance To Arrival", "Jumps" }, //worldTypeRoute
            new string[] { "System Name", "Jumps" }, //touristRoute
            new string[] { "System Name", "Body Name", "Body Subtype", "Distance To Arrival", "Landmark Type", "Value", "Jumps" } //ExoBiology
        };

        public static CsvParserReturn ParseCsv(string filename)
        {
            try
            {
                using TextFieldParser parser = new(filename);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                string[] firstline = parser.ReadFields();

                Tuple<CsvType, bool> csvType = CheckCsvType(firstline);

                if (csvType.Item2 == false)
                {
                    return null;
                }

                return csvType.Item1 switch
                {
                    CsvType.RoadToRiches => ProcessRoadToRichesRoute(parser),
                    CsvType.FleetCarrier => ProcessFleetCarrierRoute(parser),
                    CsvType.NeutronRoute => ProcessNeutronRoute(parser),
                    CsvType.GalaxyPlotter => ProcessGalaxyPlotterRoute(parser),
                    CsvType.WorldTypeRoute => ProcessWorldTypeRoute(parser),
                    CsvType.TouristRoute => ProcessTouristRoute(parser),
                    CsvType.Exobiology => ProcessExoRoute(parser),
                    _ => null,
                };
            }
            catch
            {
                return null;
            }
        }

        private static CsvParserReturn ProcessExoRoute(TextFieldParser parser)
        {
            CsvParserReturn ret = new()
            {
                CsvType = CsvType.Exobiology,
                Targets = new()
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[] fields = parser.ReadFields();

                string sysname = fields[0];

                ExplorationTarget target = ret.Targets.Find(x => x.SystemName.Contains(sysname, StringComparison.OrdinalIgnoreCase));

                if (target == null)
                {
                    target = new ExplorationTarget
                    {
                        SystemName = sysname.ToUpperInvariant(),
                    };

                    ret.Targets.Add(target);
                }

                BodiesInfo bodyinfo = new();

                bodyinfo.Body = GetBodyName(fields[1], target.SystemName);
                bodyinfo.Distance = fields[4].ToUpperInvariant();
                bodyinfo.Property1 = $"{double.Parse(fields[5], new CultureInfo("en-GB")):N0}";


                if (target.BodiesInfo == null)
                {
                    target.BodiesInfo = new List<BodiesInfo>();
                }

                target.BodiesInfo.Add(bodyinfo);
            }

            return ret;
        }

        private static CsvParserReturn ProcessTouristRoute(TextFieldParser parser)
        {
            CsvParserReturn ret = new()
            {
                CsvType = CsvType.TouristRoute,
                Targets = new()
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[] fields = parser.ReadFields();

                ExplorationTarget target = new()
                {
                    SystemName = fields[0].ToUpperInvariant(),
                    Property1 = $"{double.Parse(fields[1], new CultureInfo("en-GB")):N0}", //jumps
                };

                ret.Targets.Add(target);
            }

            return ret;
        }

        private static CsvParserReturn ProcessWorldTypeRoute(TextFieldParser parser)
        {
            CsvParserReturn ret = new()
            {
                CsvType = CsvType.WorldTypeRoute,
                Targets = new()
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[] fields = parser.ReadFields();

                string sysname = fields[0];

                ExplorationTarget target = ret.Targets.Find(x => x.SystemName.Contains(sysname, StringComparison.OrdinalIgnoreCase));

                if (target == null)
                {
                    target = new ExplorationTarget
                    {
                        SystemName = sysname.ToUpperInvariant(),
                    };

                    ret.Targets.Add(target);
                }

                BodiesInfo bodyinfo = new();

                bodyinfo.Body = GetBodyName(fields[1], target.SystemName);
                bodyinfo.Distance = $"{double.Parse(fields[2], new CultureInfo("en-GB")):N0} ls";
                bodyinfo.Property1 = $"{double.Parse(fields[3], new CultureInfo("en-GB")):N0}";
                

                if (target.BodiesInfo == null)
                {
                    target.BodiesInfo = new List<BodiesInfo>();
                }

                target.BodiesInfo.Add(bodyinfo);
            }

            return ret;
        }

        private static CsvParserReturn ProcessGalaxyPlotterRoute(TextFieldParser parser)
        {
            //"System Name", "Distance", "Distance Remaining", "Fuel Left", "Fuel Used", "Refuel", "Neutron Star"

            CsvParserReturn ret = new()
            {
                CsvType = CsvType.GalaxyPlotter,
                Targets = new()
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[] fields = parser.ReadFields();

                ExplorationTarget target = new()
                {
                    SystemName = fields[0].ToUpperInvariant(),
                    Property1 = $"{double.Parse(fields[1], new CultureInfo("en-GB")):N0}", //distance
                    Property2 = $"{double.Parse(fields[2], new CultureInfo("en-GB")):N0}", //distance remaining
                    Property3 = fields[5], //refuel 
                    Property4 = fields[6] //neutron star
                };

                ret.Targets.Add(target);
            }

            return ret;
        }

        private static CsvParserReturn ProcessNeutronRoute(TextFieldParser parser)
        {
            //"System Name","Distance To Arrival","Distance Remaining","Neutron Star","Jumps"

            CsvParserReturn ret = new()
            {
                CsvType = CsvType.NeutronRoute,
                Targets = new()
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[] fields = parser.ReadFields();

                ExplorationTarget target = new()
                {
                    SystemName = fields[0].ToUpperInvariant(),
                    Property1 = $"{double.Parse(fields[1], new CultureInfo("en-GB")):N0}", //distance
                    Property2 = $"{double.Parse(fields[2], new CultureInfo("en-GB")):N0}", //distance remaining
                    Property3 = fields[4], //jumps 
                    Property4 = fields[3]
                };

                ret.Targets.Add(target);
            }

            return ret;
        }

        private static CsvParserReturn ProcessFleetCarrierRoute(TextFieldParser parser)
        {
            //    0            1             2                  3                    4                 5         6         7             8
            //"System Name","Distance","Distance Remaining","Tritium in tank","Tritium in market","Fuel Used","Icy Ring","Pristine","Restock Tritium"

            CsvParserReturn ret = new()
            {
                CsvType = CsvType.FleetCarrier,
                Targets = new()
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[] fields = parser.ReadFields();

                ExplorationTarget target = new()
                {
                    SystemName = fields[0].ToUpperInvariant(),
                    Property1 = $"{double.Parse(fields[1], new CultureInfo("en-GB")):N0}", //distance
                    Property2 = $"{double.Parse(fields[2], new CultureInfo("en-GB")):N0}", //distance remaining
                    Property3 = GetRingInfo(fields[6], fields[7]),
                };

                ret.Targets.Add(target);
            }

            return ret;
        }

        private static string GetRingInfo(string v1, string v2)
        {
            bool bool1 = v1.Contains("Yes", StringComparison.OrdinalIgnoreCase);

            bool bool2 = v2.Contains("Yes", StringComparison.OrdinalIgnoreCase);

            return bool1 ? bool2 ? "Pristine" : "Yes" : "No";
        }

        private static CsvParserReturn ProcessRoadToRichesRoute(TextFieldParser parser)
        {
            CsvParserReturn ret = new()
            {
                CsvType = CsvType.RoadToRiches,
                Targets = new()
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[] fields = parser.ReadFields();

                string sysname = fields[0];

                ExplorationTarget target = ret.Targets.Find(x => x.SystemName.Contains(sysname, StringComparison.OrdinalIgnoreCase));

                if (target == null)
                {
                    target = new ExplorationTarget
                    {
                        SystemName = sysname.ToUpperInvariant(),
                        Property1 = fields[7] //Jumps to System
                    };

                    ret.Targets.Add(target);
                }

                BodiesInfo bodyinfo = new();

                bodyinfo.Body = GetBodyName(fields[1], target.SystemName);
                bodyinfo.Distance = $"{double.Parse(fields[4], new CultureInfo("en-GB")):N0} ls";
                bodyinfo.Property1 = $"{double.Parse(fields[6], new CultureInfo("en-GB")):N0}";
                

                if (target.BodiesInfo == null)
                {
                    target.BodiesInfo = new List<BodiesInfo>();
                }

                target.BodiesInfo.Add(bodyinfo);
            }

            return ret;
        }

        private static Tuple<CsvType, bool> CheckCsvType(string[] firstLine)
        {
            for (int i = 0; i < csvHeaders.Count; i++)
            {
                if (firstLine.SequenceEqual(csvHeaders[i]))
                {
                    return Tuple.Create((CsvType)i, true);
                }
            }

            return Tuple.Create(CsvType.RoadToRiches, false);
        }

        private static string GetBodyName(string bodyName, string systemName)
        {            
            if(bodyName.Length > systemName.Length)
            {
                bodyName = bodyName.Remove(0, systemName.Length);

                return bodyName;
            }

            return bodyName.ToUpperInvariant();

        }
    }
}