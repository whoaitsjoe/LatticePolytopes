using System;
using System.Collections.Generic;
using System.IO;

namespace UpdatedRP
{
    public class FileIO
    {
        public static List<Graph> readPolytopes()
        {
            List<Graph> result;

            var path = "/Users/Joe/Code/research/latticePolytopes/Files/" + Globals.d + Globals.k + Globals.gap + "/Polytopes";

            result = readGraphFromFile(path);

            return result;
        }

        public static List<Point> readVertexList(int gap = -1, string path = "")
        {
            List<Point> result = new List<Point>();

            if(path.Equals(""))
                path = "/Users/Joe/Code/research/latticePolytopes/Files/" + (Globals.d - 1) + Globals.k + ((gap == -1) ? Globals.gap : gap) + "/vertex";

			result = readPointsFromFile(path);

            return result;
        }
        public static List<Point> readNonvertexList(int gap = -1, string path = "")
        {
			List<Point> result = new List<Point>();

			if (path.Equals(""))
			    path = "/Users/Joe/Code/research/latticePolytopes/Files/" + (Globals.d - 1) + Globals.k + ((gap == -1) ? Globals.gap : gap) + "/nonvertex";

			result = readPointsFromFile(path);

			return result;
		}

        public static List<Point> readCoreList(int gap = -1, string path = "")
        {
			List<Point> result = new List<Point>();

			if (path.Equals(""))
			    path = "/Users/Joe/Code/research/latticePolytopes/Files/" + (Globals.d - 1) + Globals.k + ((gap == -1) ? Globals.gap : gap) + "/core";

			result = readPointsFromFile(path);

			return result;
		}

        //Reads list of graphs from file
        //File has format as follows:
        //Facet #
        //point 1 (e.g. 001)
        //--
        //edge 1 (e.g. 001 : 002 010)
		public static List<Graph> readGraphFromFile(string fName)
		{
			List<Graph> result = new List<Graph>();
			string[] lines = System.IO.File.ReadAllLines(fName);
			bool vertex = false;
			bool edge = false;
			List<Point> pts = new List<Point>();
			Dictionary<string, List<string>> a = new Dictionary<string, List<string>>();
            string id;

			for (int i = 0; i < lines.Length; i++)
			{
				if (lines[i].StartsWith("Facet", StringComparison.Ordinal))
				{
					vertex = true;
					edge = false;
					i++;
					if (pts.Count > 0)
					{
						result.Add(new Graph(pts, a));
					}
					pts = new List<Point>();
				}
				else if (lines[i] == "--")
				{
					vertex = false;
					edge = true;
					i++;
					a = new Dictionary<string, List<string>>();
				}
				else if (lines[i] == "end")
				{
					result.Add(new Graph(pts, a));
				}

				if (vertex)
				{
                    pts.Add(new Point(lines[i]));
				}
				if (edge)
				{
                    List<string> adjList = parseAdjList(lines[i], out id);
                    a.Add(id, adjList);
                    /*if(id != "end")
                        a.Add(pts[Convert.ToInt32(id) - 1].ToString(), adjList);*/
				}
			}
			return result;
		}

		//Reads list of graphs from file
		//File has format as follows:
		//Facet #
		//point 1 (e.g. 001)
		//--
		//edge 1 (e.g. 1 : 2 3)
		public static List<Graph> readGraphFromFileOldFormat(string fName)
		{
			List<Graph> result = new List<Graph>();
			string[] lines = System.IO.File.ReadAllLines(fName);
			bool vertex = false;
			bool edge = false;
			List<Point> pts = new List<Point>();
			Dictionary<string, List<string>> a = new Dictionary<string, List<string>>();
			string id;

			for (int i = 0; i < lines.Length; i++)
			{
				if (lines[i].StartsWith("Facet", StringComparison.Ordinal))
				{
					vertex = true;
					edge = false;
					i++;
					if (pts.Count > 0)
					{
						result.Add(new Graph(pts, a));
					}
					pts = new List<Point>();
				}
				else if (lines[i] == "--")
				{
					vertex = false;
					edge = true;
					i++;
					a = new Dictionary<string, List<string>>();
				}
				else if (lines[i] == "end")
				{
					result.Add(new Graph(pts, a));
				}

				if (vertex)
				{
					pts.Add(new Point(lines[i]));
				}
				if (edge)
				{
					List<string> tempList = parseAdjList(lines[i], out id);
                    List<string> adjList = new List<string>();

                    foreach(string s in tempList)
                    {
                        int tempVal;
                        if(Int32.TryParse(s, out tempVal))
                        {
                            adjList.Add(pts[tempVal - 1].ToString());
                        }
                    }
                        
					//a.Add(id, adjList);
					if(id != "end")
                        a.Add(pts[Convert.ToInt32(id) - 1].ToString(), adjList);
				}
			}
			return result;
		}

        //
		public static void writeGraphToFile(string fName, List<Graph> graphs)
		{
            List<String> lines = new List<string>();

            lines.Add("begin");

            for (int i = 0; i < graphs.Count; i++)
            {
                lines.Add("Facet #" + (i+1));

                foreach (Point p in graphs[i].Points)
                    lines.Add(p.ToString());
                
                lines.Add("--");
                //add adjList
            }

            lines.Add("end");
		}

        //reads points from file
        //File has one point per line (e.g. 001).
		public static List<Point> readPointsFromFile(string fName)
        {
			List<Point> result = new List<Point>();
            string[] lines;

            try
			{
				lines = System.IO.File.ReadAllLines(fName);
            }
            catch(Exception e)
            {
                return new List<Point>();
            }

            for (int i = 0; i < lines.Length; i++)
            {
                result.Add(new Point(lines[i]));
            }

            return result;
        }

		//parse vertex from string, assuming no garbage tokens. --unnecessary
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

		//converts each line that's read-in into List of ints.
		//skips all elements before ':', then convert each token thereafter to an int.
		public static List<string> parseAdjList(string line, out string id)
		{
			List<string> result = new List<string>();
			string[] tokens = line.Split(' ');
			bool flag = false;
            id = tokens[0];

			foreach (string word in tokens)
			{
				if (word == ":")
				{
					flag = true;
					continue;
				}
				if (flag)
				{
					result.Add(word);
				}
			}

			return result;
		}

		//reads facet incidence file and returns facet incidence as list of ints (sorted).
		//File has format as follows:
		//icd_file: ...
		//begin
		// int1 int2
		//1 incidence# : incidence1 incidence2 ...
		//2 incidence# : incidence1 incidence2 ...
		public static List<int> readFacetIncidence(string fName)
        {
            List<int> result = new List<int>();
			string[] lines = System.IO.File.ReadAllLines(fName);

			for (int i = 3; i < lines.Length - 1; i++)
			{
                string[] tokens = lines[i].Split(' ');

                if (tokens.Length < 3)
                    continue;

                int incidence = Int32.Parse(tokens[2]);
                result.Add(incidence);
			}

            result.Sort();

            return result;
        }
    }
}
