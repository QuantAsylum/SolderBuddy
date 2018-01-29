using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolderBuddy
{
    class OneLineBOM
    {
        public class Part
        {
            public string RefDes;
            public PointF Location;
            public string Value;

        }

        /// <summary>
        /// The dictionary holds lists of PartType, one for each value. So, there's a separate list for 1K, 10K, etc
        /// </summary>
        Dictionary<string, List<Part>> PartDictionary = new Dictionary<string,List<Part>>();

        List<Part> FindPartList(string value)
        {
           
            if (PartDictionary.ContainsKey(value))
                return PartDictionary[value];
            else
                return new List<Part>();
        }

        void AddPart(Part p)
        {
            if (PartDictionary.ContainsKey(p.Value))
                PartDictionary[p.Value].Add(p);
            else
            {
                List<Part> newPartList = new List<Part>();
                newPartList.Add(p);
                PartDictionary.Add(p.Value, newPartList);
            }
        }

        /// <summary>
        /// Loads a BOM file where the fields are as follows:
        /// REFDES, NAME, X (mm), Y (mm), Side, Rotate, Value
        /// Value is 0.1uF/50V or similar
        /// </summary>
        /// <param name="file"></param>
        //public Dictionary<string, List<Part>> LoadFile(string file)
        public List<List<Part>> LoadFile(string file, string side)
        {
            PartDictionary = new Dictionary<string,List<Part>>();

            string[] lines = File.ReadAllLines(file);

            // Skip first line
            for (int i=1; i<lines.Length; i++)
            {
                string[] toks = lines[i].Split(',');

                float x = Convert.ToSingle(toks[2]);
                float y = Convert.ToSingle(toks[3]);

                PointF loc = new PointF(x, y);

                if (toks[4].ToUpper() == side.ToUpper())
                {
                    Part p = new Part { RefDes = toks[0], Location = loc, Value = toks[6] };
                    AddPart(p);
                }
            }

            // Now sort the dictionary based on length of each list
            // http://stackoverflow.com/questions/289/how-do-you-sort-a-dictionary-by-value
            PartDictionary = PartDictionary.OrderByDescending(o => o.Value.Count).ToDictionary(x => x.Key, x => x.Value);

            return PartDictionary.Values.ToList();
        }



       
    }
}
