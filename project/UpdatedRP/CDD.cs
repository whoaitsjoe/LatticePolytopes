using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UpdatedRP
{
	public class CDD
	{
		//cdd using that compares adjacency list count vs # of points count
		public static bool compareAlToPoints(List<Point> points)
		{
			Globals.convexHullCount++;

			bool flag = false;
			int count = 0;
			List<string> adjList = new List<string>();

			string outFile = "/Users/Joe/Code/research/Files/out.ext";

			Parse.writeToFile(points, outFile);

			var proc = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "/Users/Joe/Code/research/cdd/src/_cdd_exe",
					Arguments = outFile,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				}
			};

			proc.Start();
			while (!proc.StandardOutput.EndOfStream)
			{
				string line = proc.StandardOutput.ReadLine();
				if (line == "begin")
				{
					flag = true;
					continue;
				}

				if (flag)
				{
					adjList.Add(line);
					count++;
				}
			}

			return (count == points.Count + 2) ? true : false;
		}


		//returns true if both u and v are vertices of convex core.
		public static bool convexHullVertex(List<Point> points, Point u, Point v, out bool uRedundant, out bool vRedundant)
		{
			Globals.convexHullCount++;

			bool flag = false;
			bool uFound = false, vFound = false;
			List<string> adjList = new List<string>();
			Dictionary<int, List<int>> adjList2 = new Dictionary<int, List<int>>();

			string outFile = "/Users/Joe/Code/research/Files/out.ext";

			Parse.writeToFile(points, outFile);

			var proc = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "/Users/Joe/Code/research/cdd/src/_minimalRepresentation",
					Arguments = outFile,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				}
			};

			proc.Start();
			while (!proc.StandardOutput.EndOfStream)
			{
				string line = proc.StandardOutput.ReadLine();

				if (line == "begin")
				{
					flag = true;
					continue;
				}

				//checks to see if point is u or v and sets corresponding flag to true
				if (flag)
				{
					char[] delimiter = new char[] { ' ' };
					string[] tokens = line.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

					for (int i = 1; i < tokens.Length; i++)
					{
						if (!uFound)
						{
							try
							{
                                if (tokens[i] != u.Coordinates[i - 1].ToString())
									break;
							}
							catch (FormatException)
							{

							}
						}
						else
							break;

						if (i == tokens.Length - 1)
							uFound = true;
					}

					for (int i = 1; i < tokens.Length; i++)
					{
						if (!vFound)
						{
							try
							{
								if (tokens[i] != v.Coordinates[i - 1].ToString())
									break;
							}
							catch (FormatException)
							{

							}
						}
						else
							break;

						if (i == tokens.Length - 1)
							vFound = true;
					}
				}
			}

			uRedundant = !uFound;
			vRedundant = !vFound;

			proc.WaitForExit();
			return (uFound && vFound);
		}


		//takes a list of points and checks convex hull to see if every point is a vertex
		public static bool convexHullVertexList(List<Point> points)
		{
			Globals.convexHullCount++;

			var watch = System.Diagnostics.Stopwatch.StartNew();

			bool flag = false;
			int count = points.Count;
			int num = 0;
			bool[] found = new bool[count];
			char[] delimiter = new char[] { ' ' };
			string[] tokens;
			List<string> lines1 = new List<string>();

			for (int i = 0; i < count; i++)
				found[i] = false;

			string outFile = "/Users/Joe/Code/research/Files/out.ext";

			Parse.writeToFile(points, outFile);

			var proc = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "/Users/Joe/Code/research/cdd/src/_minimalRepresentation",
					Arguments = outFile,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				}
			};

			proc.Start();
			while (!proc.StandardOutput.EndOfStream)
			{
				string line = proc.StandardOutput.ReadLine();

				if (line == "begin")
				{
					flag = true;
					continue;
				}

				if (flag)
				{
					lines1.Add(line);
					tokens = line.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
					num++;

					if (tokens.Length < 3)
						continue;

					for (int j = 0; j < count; j++)
					{
						if (!found[j])
						{
							try
							{
								found[j] = true;
								for (int i = 1; i < tokens.Length; i++)
								{
									if (Convert.ToInt16(tokens[i]) != points[j].Coordinates[i - 1])
									{
										found[j] = false;
									}
								}
								if (found[j])
									break;
							}
							catch (FormatException)
							{
								found[j] = false;
								break;
							}
						}
					}
				}
			}

			watch.Stop();
			Globals.chTime += watch.ElapsedMilliseconds;

			foreach (bool x in found)
				if (!x)
					return false;

			return true;
		}

		//takes a list of non-vertices and a list of vertices. Checks to see if every vertex is a vertex in CH of points
		public static bool convexHullVertexList(List<Point> points, List<Point> corePoints)
		{
			Globals.convexHullCount++;

			bool flag = false;
			int count = points.Count;
			int num = 0;
			bool[] found = new bool[count];
			char[] delimiter = new char[] { ' ' };
			string[] tokens;
			List<Point> temp = new List<Point>();

			for (int i = 0; i < count; i++)
				found[i] = false;

			foreach (Point p in points)
				temp.Add(p.clone());
			foreach (Point p in corePoints)
				temp.Add(p.clone());

			string outFile = "/Users/Joe/Code/research/Files/out.ext";

			Parse.writeToFile(temp, outFile);

			var proc = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "/Users/Joe/Code/research/cdd/src/_minimalRepresentation",
					Arguments = outFile,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				}
			};

			var watch = System.Diagnostics.Stopwatch.StartNew();

			proc.Start();
			while (!proc.StandardOutput.EndOfStream)
			{
				string line = proc.StandardOutput.ReadLine();

				if (line == "begin")
				{
					flag = true;
					continue;
				}

				if (flag)
				{
					tokens = line.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
					num++;

                    if (tokens.Length < 3)
						continue;

					for (int j = 0; j < count; j++)
					{
						if (!found[j])
						{
							try
							{
								found[j] = true;
								for (int i = 1; i < tokens.Length; i++)
								{
									if (Convert.ToInt16(tokens[i]) != points[j].Coordinates[i - 1])
									{
										found[j] = false;
									}
								}
								if (found[j])
									break;
							}
							catch (FormatException)
							{
								found[j] = false;
								break;
							}
						}
					}
				}
			}

			watch.Stop();
			Globals.chTime += watch.ElapsedMilliseconds;

			foreach (bool x in found)
				if (!x)
					return false;

			return true;
		}
        //checks min representation of points + core points. If all points are vertices, then return a graph of the points and adj list.
		public static bool convexHullAdjList(List<Point> points, List<Point> corePoints, out Graph result)
		{
			Globals.convexHullCount++;

			bool minRepFlag = false, adjFlag = false, active = false;
			int count = points.Count;
			int num = 0;
			bool[] found = new bool[count];
			char[] delimiter = new char[] { ' ' };
			string[] tokens;
			List<Point> temp = new List<Point>();
			result = new Graph();
			List<Point> _pointList = new List<Point>();
			Dictionary<string, List<string>> adjList = new Dictionary<string, List<string>>();
			List<string> neighbours;

			for (int i = 0; i < count; i++)
				found[i] = false;

			foreach (Point p in points)
				temp.Add(p.clone());
			foreach (Point p in corePoints)
				temp.Add(p.clone());

			string outFile = "/Users/Joe/Code/research/Files/out.ext";

			Parse.writeToFile(temp, outFile);

			var proc = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "/Users/Joe/Code/research/cdd/src/_minRepAdjList",
					Arguments = outFile,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				}
			};

			var watch = System.Diagnostics.Stopwatch.StartNew();

			proc.Start();
			while (!proc.StandardOutput.EndOfStream)
			{
				string line = proc.StandardOutput.ReadLine();

				if (line == "begin")
				{
					active = true;
					if (!minRepFlag)
						minRepFlag = true;
					else
					{
						minRepFlag = false;
						adjFlag = true;
					}

					continue;
				}
				else if (line == "end")
					active = false;

				if (active && minRepFlag)
				{
					tokens = line.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
					num++;

                    //checks to see if last token is parseable
                    if (!int.TryParse(tokens[tokens.Length - 1], out int tempVar))
						continue;

					for (int j = 0; j < count; j++)
					{
						if (!found[j])
						{
							try
							{
								found[j] = true;
								for (int i = 1; i < tokens.Length; i++)
								{
                                    if (Convert.ToInt16(tokens[i]) != (int)char.GetNumericValue(points[j].Coordinates[i - 1]))
									{
										found[j] = false;
                                        break;
									}
								}
								if (found[j])
								{
									_pointList.Add(points[j].clone());
									break;
								}
							}
							catch (FormatException)
							{
								found[j] = false;
								break;
							}
						}
					}
				}
				else if (!active && minRepFlag)
				{
					foreach (bool x in found)
						if (!x)
							return false;
				}
				else if (active && adjFlag)
				{
					tokens = line.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
					neighbours = new List<string>();

					if (tokens.Length < 4)
						continue;
					try
					{
                        //i = 3 hard code is correct. First 3 tokens are as follows: point referenced, # of neighbours, ":"
						for (int i = 3; i < Convert.ToInt16(tokens[1]) + 3; i++)
						{
                            neighbours.Add(_pointList[Convert.ToInt16(tokens[i]) - 1].ToString());
						}
						adjList.Add(_pointList[Convert.ToInt16(tokens[0]) - 1].ToString(), neighbours);

					}
					catch (Exception) { }
				}
			}

			watch.Stop();
			Globals.chTime += watch.ElapsedMilliseconds;

			result.Points = _pointList;
			result.AdjList = adjList;
			return true;
		}
	}
}
