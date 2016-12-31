using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YelpAcademicSet
{
    class WaybackAPIController
    {
        public static void GenerateList()
        {
                        var timestamps = new List<string>();
            timestamps.Add("20120101");
            timestamps.Add("20130101");
            timestamps.Add("20140101");
            timestamps.Add("20150101");
            var writer = new StreamWriter(".\\Master\\WaybackList.txt");
            var csvwriter = new StreamWriter(".\\Master\\WaybackList.csv");

            System.IO.StreamReader file = new System.IO.StreamReader(".\\Master\\CityList.txt");
            var line = "";
            while ((line = file.ReadLine()) != null)
            {
                foreach (var timestamp in timestamps)
                {
                    try
                    {
                        string sURL;
                        sURL = "http://archive.org/wayback/available?url=+" + line + "&timestamp="+timestamp;

                        WebRequest wrGETURL;
                        wrGETURL = WebRequest.Create(sURL);

                        var response = wrGETURL.GetResponse();
                        var result = "";
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            result = reader.ReadToEnd();
                        }
                        dynamic stuff = JsonConvert.DeserializeObject(result);
                        writer.WriteLine(stuff.archived_snapshots.closest.url);
                        csvwriter.WriteLine(line + ";" + stuff.archived_snapshots.closest.url);
                        csvwriter.Flush();
                        writer.Flush();

                    }
                    catch (Exception) { }
                }

            }
            file.Close();
            writer.Close();
        
        }
    }
}
