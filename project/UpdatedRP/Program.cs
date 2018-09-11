﻿﻿using System;
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
			Globals.k = 3;
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

            //Tester.testGenUV();
            //Tester.testInverse();
            //Tester.testGeneratePolytopes();
            //shelling();
            //shelling(new Point("011"), new Point("211"));

            Globals.writeToFile = false;
            //Tester.testPostShelling();
            Tester.testNewSymmetry();
        }

		public static void shelling()
		{
			List<Graph> tempResult;
       		List<Graph> allShelling = new List<Graph>();
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

			foreach (Tuple<Point, Point> item in allPoints)
			{
				Globals.convexHullCount = 0;

                //test - debug
                Globals.CDD_convexHullAdjList_counter = 0;
                if (sumUpToK(item.Item1, item.Item2))
                    continue;

				Console.WriteLine("Start Point: " + item.Item1 + ". End Point: " + item.Item2);
				Globals.u = item.Item1;
				Globals.v = item.Item2;

				tempResult = Shell.shell(item.Item1, item.Item2);
				allShelling.AddRange(tempResult);

				//test - debug
				Console.WriteLine("convexHullAdjList counter: " + Globals.CDD_convexHullAdjList_counter);

				Parse.writeToFile(tempResult, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
								  + Globals.gap.ToString() + "/_" + item.Item1.Coordinates[0] + item.Item1.Coordinates[1]
								  + item.Item1.Coordinates[2] + item.Item2.Coordinates[0] + item.Item2.Coordinates[1]
								  + item.Item2.Coordinates[2] + "allShellings");

				Console.WriteLine("Number of Convex Hulls checked: " + Globals.convexHullCount);
			}
			Console.WriteLine("Total Shellings: " + allShelling.Count);

			if (Globals.writeToFile)
				Parse.writeToFile(allShelling, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
							  + Globals.gap.ToString() + "/_allShellings");

            PostShelling.postShellingProcess(allShelling);
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

			shellings = Shell.symmetryGroup(tempResult);

			Console.WriteLine("Unique Shellings: " + shellings.Count);

			if (Globals.writeToFile)
				Parse.writeToFile(shellings, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
								  + Globals.gap.ToString() + "/_" + u.Coordinates[0] + u.Coordinates[1]
								  + u.Coordinates[2] + v.Coordinates[0] + v.Coordinates[1]
								  + v.Coordinates[2] + "_uniqueShellings");

			polytopes = Shell.checkInterior(shellings);
			uniquePolytopes = Shell.symmetryGroup(polytopes);

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
            finalPolytopes = Shell.symmetryGroup(retractablePolytopes);
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
            Globals._222polytopes = new Dictionary<string, List<Graph>>();

            //todo -- temp method of generating all 222 polytopes
            for (int i = 0; i < 9; i++)
            {
                Point p = new Point(new int[2]{i%3, i/3});
                List<Graph> polytopes = Generate.dMinus1Polytopes(new List<Point>() { p }, 2);
                Globals._222polytopes.Add(p.ToString(), polytopes);
            }

            Console.WriteLine();
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

        public static bool sumUpToK(Point u, Point v)
        {
            for (int i = 0; i < Globals.d; i++)
            {
                if (u.Coordinates[i] + v.Coordinates[i] != Globals.k)
                    return false;
            }
            return true;
        }
	}
}
