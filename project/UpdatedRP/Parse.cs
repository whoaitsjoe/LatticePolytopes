using System;
using System.Collections.Generic;
namespace UpdatedRP
{
    public class Parse
    {
		//converts each line that's read-in into List of ints.
		//skips all elements before ':', then convert each token thereafter to an int.
		public static List<int> parseAdjList(string line)
		{
			List<int> result = new List<int>();
			string[] tokens = line.Split(' ');
			bool flag = false;
			int temp;
			foreach (string word in tokens)
			{
				if (word == ":")
				{
					flag = true;
					continue;
				}
				if (flag)
				{
					Int32.TryParse(word, out temp);
					if (temp > 0)
						result.Add(temp);
				}
			}

			return result;
		}
		//parse vertex from string, assuming no garbage tokens.
		public static Point parseVertexFromString(string line)
		{
			Point result = new Point();
			string[] tokens = line.Split(' ');
			int temp;
            string coord = "";

			for (int i = 0; i < tokens.Length; i++)
			{
				if (Int32.TryParse(tokens[i], out temp))
					coord += temp;
			}
            result.Coordinates = coord;

			return result;
		}

        //*******Write to File***********//
        //todo -- add check and create directory if directory does not exist.
		//Write list of points to file in V Representation
		public static void writeToFile(List<Point> points, string Filename)
		{
			System.IO.File.WriteAllLines(Filename, formatOutput(points));
		}
		//Write Facet to File.
		public static void writeToFile(List<Graph> facets, string Filename)
		{
			System.IO.File.WriteAllLines(Filename, formatOutput(facets));
		}
		//*******Write to File***********//

		//*******Formatting***********//
        //convert List of points to output format
		public static String[] formatOutput(List<Point> points)
		{
			String[] result = new string[points.Count + 4];
			result[0] = "V-representation";
			result[1] = "begin";
            result[2] = points.Count + " " + (points[0].getDimension() + 1) + " " + "integer";
			result[result.Length - 1] = "end";
			for (int i = 0; i < points.Count; i++)
			{
				string temp = " 1 ";
				foreach (char c in points[i].Coordinates)
				{
					temp += c + " ";
				}
				result[3 + i] = temp;
			}
			return result;
		}

		//convert facets to output format
		public static String[] formatOutput(List<Graph> facets)
		{
			List<string> result = new List<string>();
			result.Add("F-Representation");
			result.Add("begin");
			string temp = "";
			for (int i = 0; i < facets.Count; i++)
			{
				result.Add("Facet " + (i + 1).ToString() + ": ");
				foreach (Point p in facets[i].Points)
				{
					result.Add(p.ToString());
				}
				result.Add("--");
				foreach (KeyValuePair<string, List<string>> entry in facets[i].AdjList)
				{
                    for (int j = 0; j < facets[i].getPointsCount(); j++)
                    {
                        if (facets[i].Points[j].Equals(entry.Key))
                        {
                            temp = (j+1) + " : ";
                            break;
                        }                    
                    }

                    foreach (string s in entry.Value)
                    {
                        for (int j = 0; j < facets[i].getPointsCount(); j++)
                        {
                            if (facets[i].Points[j].Equals(s))
							{
                                temp += (j+1) + " ";
                                break;
                            }
                        }
                    }
					result.Add(temp);
				}
			}

			result.Add("end");

			return result.ToArray();
		}
		//*******Formatting***********//
   	}
}
