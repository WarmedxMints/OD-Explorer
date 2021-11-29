using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace LoadSaveSystem
{
    public class LoadSave
    {
        public static T LoadJson<T>(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    string saveString = File.ReadAllText(path);

                    T rslt = JsonConvert.DeserializeObject<T>(saveString);

                    return rslt;
                }
                else
                {
                    return default;
                }
            }
            catch
            {
                return default;
            }
        }

        public static bool SaveJson<T>(T objectToSave, string path)
        {
            try
            {
                string directory = Path.GetDirectoryName(path);

                if (!Directory.Exists(directory))
                {
                    _ = Directory.CreateDirectory(directory);
                }

                string json = JsonConvert.SerializeObject(objectToSave, Formatting.Indented);

                using (FileStream fs = new(path, FileMode.Create))
                {
                    using StreamWriter writer = new(fs);
                    writer.Write(json);
                }
                return true;
            }
            catch (ArgumentException e)
            {
#if DEBUG
                Trace.WriteLine(e.Message);
#endif
                return false;
            }
        }
    }
}
