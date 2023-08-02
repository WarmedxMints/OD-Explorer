using EliteJournalReader;
using ODExplorer.NavData;
using System;

namespace ODExplorer.Utils
{
    public class MathFunctions
    {
        // see https://forums.frontier.co.uk/showthread.php/232000-Exploration-value-formulae/ 
        // and https://github.com/EDSM-NET/Component/blob/master/Body/Value.php for details
        public static int GetBodyValue(SystemBody bodyToAdd, bool odyssey, bool mapped, bool withEfficeinctBonus)
        {
            if (bodyToAdd.IsStar)
            {
                return GetStarValue(bodyToAdd.StarType, bodyToAdd.StellarMass);
            }
            if (bodyToAdd.IsPlanet)
            {
                return GetPlanetValue(bodyToAdd.PlanetClass, bodyToAdd.MassEM, !bodyToAdd.WasDiscovered, !bodyToAdd.Wasmapped, bodyToAdd.Terraformable, odyssey, mapped, withEfficeinctBonus);
            }

            return 0;
        }

        public static int GetStarValue(StarType starType, double stellarMass)
        {
            var k = starType switch
            {
                StarType.D or StarType.DA or StarType.DAB or StarType.DAO or StarType.DAZ or StarType.DAV or StarType.DB or StarType.DBZ or StarType.DBV or StarType.DO or StarType.DOV or StarType.DQ or StarType.DC or StarType.DCV or StarType.DX => 14057,
                StarType.N or StarType.H => 22628,
                StarType.SupermassiveBlackHole => 33.5678,
                _ => 1200,
            };
            return (int)(k + (stellarMass * k / 66.25));
        }

        public static int GetPlanetValue(PlanetClass planetClass, double mass, bool isFirstDiscoverer, bool isFirstMapped, bool terraformable, bool isOdyssey, bool isMapped, bool withEfficientBonus)
        {
            //If a body has been mapped before but is reported to have not been discovered,
            //assume we are mapping a bubble planet and don't apply the ody bonus.
            if (!isFirstMapped && isFirstDiscoverer)
            {
                isOdyssey = false;
            }

            double k;

            switch (planetClass)
            {
                case PlanetClass.MetalRichBody:
                    k = 21790;
                    if (terraformable)
                    {
                        k += 65631;
                    }
                    break;
                case PlanetClass.AmmoniaWorld:
                    k = 96932;
                    break;
                case PlanetClass.SudarskyClassIGasGiant:
                    k = 1656;
                    break;
                case PlanetClass.SudarskyClassIIGasGiant:
                    k = 9654;
                    break;
                case PlanetClass.HighMetalContentBody:
                case PlanetClass.HighMetalContentWorld:
                    k = 9654;
                    if (terraformable)
                    {
                        k += 100677;
                    }
                    break;
                case PlanetClass.WaterWorld:
                    k = 64831;
                    if (terraformable)
                    {
                        k += 116295;
                    }
                    break;
                case PlanetClass.EarthlikeBody:
                case PlanetClass.EarthLikeWorld:
                    k = 64831 + 116295;
                    break;
                default:
                    k = 300;
                    if (terraformable)
                    {
                        k += 93328;
                    }
                    break;
            }

            const double q = 0.56591828;

            double mappingMultiplier = 1;

            if (isMapped)
            {
                if (isFirstDiscoverer && isFirstMapped)
                {
                    mappingMultiplier = 3.699622554;
                }
                else
                {
                    mappingMultiplier = isFirstMapped ? 8.0956 : 3.3333333333;
                }
                //MattG's forum post suggests multipling the value with the bonus
                //However, in my personal testing I have found that applying it to the mapping multipler
                //has been correct
                if (withEfficientBonus)
                {
                    mappingMultiplier *= 1.25;
                }
            }

            double value = Math.Max((k + k * q * Math.Pow(mass, 0.2)) * mappingMultiplier, 500);

            if (isMapped)
            {
                if (isOdyssey)
                {
                    value += Math.Max(value * 0.3, 555);
                }

                //if (withEfficientBonus)
                //{
                //    value *= 1.25;
                //}
            }

            value *= isFirstDiscoverer ? 2.6 : 1;

            return (int)Math.Round(value);
        }
    }
}
