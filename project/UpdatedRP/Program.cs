﻿using System;
using System.Collections.Generic;
using System.IO;

namespace UpdatedRP
{
    class MainClass
    {
        public static void Main()
        {
            //setting execution parameters
            Globals.d = 3;
			Globals.k = 2;
			Globals.gap = 1;
            //this should be read from file.
			Globals.maxDiameter = new int[] { 0, 2, 3, 4, 4, 5, 6, 6, 7, 8, 8 };
            Globals.diameter = Globals.maxDiameter[Globals.k];
            Globals.maxLength = Globals.diameter + Globals.k - Globals.gap; //delta(d-1,k)+k-gap, the number to be eliminated
			Globals.chTime = 0;
            Globals.messageOn = false;

            initialize();

            /*var watch = System.Diagnostics.Stopwatch.StartNew();
            shelling();
			watch.Stop();

			var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Total elapsed time: " + elapsedMs);*/

            Tester.testGenUV();
            //Tester.testInverse();
			//shelling();
        }

		public static void shelling()
		{
			List<Graph> tempResult, shellings, polytopes, uniquePolytopes, retractablePolytopes, finalPolytopes;
			List<Point> vertices = new List<Point>();
            List<Point> startPoints, endPoints;
			List<Tuple<Point, Point>> allPoints = new List<Tuple<Point, Point>>();

			startPoints = GenerateUV.generateU();

			foreach (Point p in startPoints)
			{
				endPoints = GenerateUV.generateV(p);
				foreach (Point q in endPoints)
					allPoints.Add(new Tuple<Point, Point>(p, q));
			}
			Console.WriteLine("Total u,v pairs: " + allPoints.Count);

            List<Graph> allShelling = new List<Graph>();
			int totalShellings = 0;

			foreach (Tuple<Point, Point> item in allPoints)
			{
				Globals.convexHullCount = 0;
				Console.WriteLine("Start Point: " + item.Item1 + ". End Point: " + item.Item2);
				Globals.u = item.Item1;
				Globals.v = item.Item2;

				tempResult = Shell.shell(item.Item1, item.Item2);
				allShelling.AddRange(tempResult);

				Parse.writeToFile(tempResult, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
								  + Globals.gap.ToString() + "/_" + item.Item1.Coordinates[0] + item.Item1.Coordinates[1]
								  + item.Item1.Coordinates[2] + item.Item2.Coordinates[0] + item.Item2.Coordinates[1]
								  + item.Item2.Coordinates[2] + "allShellings");

				Console.WriteLine("Number of Convex Hulls checked: " + Globals.convexHullCount);

				totalShellings += allShelling.Count;

				Console.WriteLine(allShelling.Count);
			}
			Console.WriteLine("Total Shellings: " + totalShellings);

			if (Globals.writeToFile)
				Parse.writeToFile(allShelling, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
							  + Globals.gap.ToString() + "/_allShellings");

			Shell.symmetryGroup(allShelling, out shellings);

			Console.WriteLine("Unique Shellings: " + shellings.Count);

			if (Globals.writeToFile)
				Parse.writeToFile(shellings, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
							  + Globals.gap.ToString() + "/_uniqueShellings");

			polytopes = Shell.checkInterior(shellings);
			Shell.symmetryGroup(polytopes, out uniquePolytopes);

			Console.WriteLine("All valid polytopes: " + polytopes.Count);

			if (Globals.writeToFile)
				Parse.writeToFile(polytopes, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
							  + Globals.gap.ToString() + "/_allPolytopes");

			Console.WriteLine("Unique polytopes: " + uniquePolytopes.Count);

			if (Globals.writeToFile)
				Parse.writeToFile(polytopes, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
							  + Globals.gap.ToString() + "/_uniquePolytopes");


			retractablePolytopes = Shell.retractable(uniquePolytopes);
			Shell.symmetryGroup(retractablePolytopes, out finalPolytopes);

			Console.WriteLine("Retractable polytopes: " + uniquePolytopes.Count);

			if (Globals.writeToFile)
				Parse.writeToFile(polytopes, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
							  + Globals.gap.ToString() + "/_retractablePolytopes");

			Console.WriteLine("Final polytopes: " + uniquePolytopes.Count);

			if (Globals.writeToFile)
				Parse.writeToFile(polytopes, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
							  + Globals.gap.ToString() + "/_finalPolytopes");
		}

        public static void shelling(Point u, Point v)
		{
			List<Graph> tempResult, shellings, polytopes, uniquePolytopes, retractablePolytopes, finalPolytopes;

			Globals.convexHullCount = 0;
			Console.WriteLine("Start Point: " + u + ". End Point: " + v);

			tempResult = Shell.shell(u, v);
			Console.WriteLine(tempResult.Count);
			Console.WriteLine("Number of Convex Hulls checked: " + Globals.convexHullCount);

			if (Globals.writeToFile)
				Parse.writeToFile(tempResult, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
								  + Globals.gap.ToString() + "/_" + u.Coordinates[0] + u.Coordinates[1]
								  + u.Coordinates[2] + v.Coordinates[0] + v.Coordinates[1]
								  + v.Coordinates[2] + "allShellings");

			Shell.symmetryGroup(tempResult, out shellings);

			Console.WriteLine("Unique Shellings: " + shellings.Count);

			if (Globals.writeToFile)
				Parse.writeToFile(shellings, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
								  + Globals.gap.ToString() + "/_" + u.Coordinates[0] + u.Coordinates[1]
								  + u.Coordinates[2] + v.Coordinates[0] + v.Coordinates[1]
								  + v.Coordinates[2] + "_uniqueShellings");

			polytopes = Shell.checkInterior(shellings);
			Shell.symmetryGroup(polytopes, out uniquePolytopes);

			Console.WriteLine("All valid polytopes: " + polytopes.Count);

			if (Globals.writeToFile)
				Parse.writeToFile(polytopes, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
								  + Globals.gap.ToString() + "/_" + u.Coordinates[0] + u.Coordinates[1]
								  + u.Coordinates[2] + v.Coordinates[0] + v.Coordinates[1]
								  + v.Coordinates[2] + "_allPolytopes");

			Console.WriteLine("Unique polytopes: " + uniquePolytopes.Count);

			if (Globals.writeToFile)
				Parse.writeToFile(polytopes, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
								  + Globals.gap.ToString() + "/_" + u.Coordinates[0] + u.Coordinates[1]
								  + u.Coordinates[2] + v.Coordinates[0] + v.Coordinates[1]
								  + v.Coordinates[2] + "_uniquePolytopes");


			retractablePolytopes = Shell.retractable(uniquePolytopes);
			Shell.symmetryGroup(retractablePolytopes, out finalPolytopes);
			Console.WriteLine("Retractable polytopes: " + uniquePolytopes.Count);

			if (Globals.writeToFile)
				Parse.writeToFile(polytopes, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
								  + Globals.gap.ToString() + "/_" + u.Coordinates[0] + u.Coordinates[1]
								  + u.Coordinates[2] + v.Coordinates[0] + v.Coordinates[1]
								  + v.Coordinates[2] + "_retractablePolytopes");

			Console.WriteLine("Final polytopes: " + uniquePolytopes.Count);

			if (Globals.writeToFile)
				Parse.writeToFile(polytopes, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
								  + Globals.gap.ToString() + "/_" + u.Coordinates[0] + u.Coordinates[1]
								  + u.Coordinates[2] + v.Coordinates[0] + v.Coordinates[1]
								  + v.Coordinates[2] + "_finalPolytopes");
        }

		public static void display(List<Graph> graphs)
		{
			foreach (Graph g in graphs)
				Console.WriteLine(g);
		}
		public static void display(List<Point> points)
		{
			foreach (Point p in points)
				Console.WriteLine(p);
		}

        //initialize functions
        public static void initialize()
        {
            Globals.initialize();

			//read from file
			Globals.vertexSet = initializeVertexSet();
            Globals.nonvertexSet = initializeNonVertexSet();
			Globals.coreSet = initializeCoreSet();
        }

        public static Dictionary<string, List<Point>> initializeVertexSet()
        {
            Dictionary<string, List<Point>> result = new Dictionary<string, List<Point>>();

            string[] folders = Directory.GetDirectories(Globals.directory);

            foreach (string path in folders)
            {
                string[] fileName = Directory.GetFiles(path, "vertexSet");
                List<Point> vertexSet = new List<Point>();
                foreach (string file in fileName)
                {
                    vertexSet = FileIO.readVertexList(path: file);
                }

                //get d,k,g from path.
                string key = path.Substring(path.Length - 3, 3);

                result.Add(key, vertexSet);
            }

            return result;
        }

		public static Dictionary<string, List<Point>> initializeNonVertexSet()
		{
			Dictionary<string, List<Point>> result = new Dictionary<string, List<Point>>();

			string[] folders = Directory.GetDirectories(Globals.directory);

			foreach (string path in folders)
			{
				string[] fileName = Directory.GetFiles(path, "nonvertexSet");
				List<Point> nonvertexSet = new List<Point>();
				foreach (string file in fileName)
				{
					nonvertexSet = FileIO.readVertexList(path: file);
				}

				//get d,k,g from path.
				string key = path.Substring(path.Length - 3, 3);

				result.Add(key, nonvertexSet);
			}

			return result;
		}

		public static Dictionary<string, List<Point>> initializeCoreSet()
		{
			Dictionary<string, List<Point>> result = new Dictionary<string, List<Point>>();

			string[] folders = Directory.GetDirectories(Globals.directory);

			foreach (string path in folders)
			{
				string[] fileName = Directory.GetFiles(path, "coreSet");
				List<Point> coreSet = new List<Point>();
				foreach (string file in fileName)
				{
					coreSet = FileIO.readVertexList(path: file);
				}

				//get d,k,g from path.
				string key = path.Substring(path.Length - 3, 3);

				result.Add(key, coreSet);
			}

			return result;
		}
	}
}
