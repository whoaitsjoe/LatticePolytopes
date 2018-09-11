﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Timers;

namespace UpdatedRP
{
    public class Tester
    {

        public static void testNewSymmetry()
        {
            Graph passGraph = new Graph(), failGraph = new Graph();
            passGraph.Points = new List<Point>() { new Point("000"), new Point("001"), new Point("013"), new Point("023"),
			new Point("031"), new Point("030"), new Point("331"), new Point("330"), new Point("301"), new Point("300") };
            failGraph.Points = new List<Point>() { new Point("000"), new Point("001"), new Point("013"), new Point("023"),
			new Point("031"), new Point("030"), new Point("331"), new Point("330"), new Point("301"), new Point("302") };

            List<Graph> compGraphs = new List<Graph>();

			compGraphs.Add(new Graph(new List<Point>() { new Point("333"), new Point("330"), new Point("303"), new Point("300"),
                new Point("233"), new Point("230"), new Point("203"), new Point("200"), new Point("010"), new Point("020") },
				new Dictionary<int, List<int>>()));
			compGraphs.Add(new Graph(new List<Point>() { new Point("333"), new Point("330"), new Point("303"), new Point("300"),
				new Point("233"), new Point("230"), new Point("203"), new Point("200"), new Point("010"), new Point("030") },
				new Dictionary<int, List<int>>()));
			compGraphs.Add(new Graph(new List<Point>() { new Point("333"), new Point("330"), new Point("303"), new Point("300"),
				new Point("233"), new Point("230"), new Point("203"), new Point("200"), new Point("010"), new Point("021") },
				new Dictionary<int, List<int>>()));

			var watch = System.Diagnostics.Stopwatch.StartNew();
			Console.Write("\nThis should be TRUE: " + Shell.symmetryHelperNew(passGraph, compGraphs, 1));
			Console.Write("\nThis should be FALSE: " + Shell.symmetryHelperNew(failGraph, compGraphs, 1));
			watch.Stop();

			var elapsedMs = watch.ElapsedMilliseconds;
			Console.WriteLine("\nTotal elapsed time: " + elapsedMs);
        }


        public static void testPostShelling()
        {
			string[] fileEntries = Directory.GetFiles(Globals.directory + Globals.d.ToString() + Globals.k.ToString()
								  + Globals.gap.ToString() + "/");
            List<Graph> shellings = new List<Graph>();

            foreach (string file in fileEntries)
                shellings.AddRange(FileIO.readGraphFromFileOldFormat(file));
            Console.WriteLine("Total shellings: {0}", shellings.Count);
			PostShelling.postShellingProcess(shellings);
        }

        public static void testGenUV()
        {
            List<Point> u;
            u = GenerateUV.generateU();
            int vCount = 0;

            Console.WriteLine("Generating UV pairs for d=" + Globals.d + ", k=" + Globals.k + ", g=" + Globals.gap);
            foreach(Point ui in u)
            {
                List<Point> v = GenerateUV.generateV(ui);

                vCount += v.Count;
                Console.WriteLine("u: " + ui);
                MainClass.display(v);
            }

            Console.WriteLine("Total number of uv pairs: " + vCount);
        }

        public static void testInverse()
        {
            Point u = new Point("011");
            Point v = new Point("100");

            Console.WriteLine("u: " + u.ToString());
            Console.WriteLine("v: " + v.ToString());
            //Console.WriteLine(GenerateUV.checkInverse(u, v));
        }

        public static void testGeneratePolytopes()
        {
            Point u = new Point("00");

            List<Graph> dMinus1Polytopes = Generate.dMinus1Polytopes(new List<Point>(){u}, 1);

			Console.WriteLine("u: " + u + ", # of d - 1 polytopes: " + dMinus1Polytopes.Count);

            int vert4 = 0;
            int vert5 = 0;
            int vert6 = 0;
            int vert7 = 0;
			int vert8 = 0;
			int vert9 = 0;
            int vertOther = 0;

            for (int i = 0; i < dMinus1Polytopes.Count; i++)
            {
                switch(dMinus1Polytopes[i].getPointsCount())
                {
                    case 4:
                        vert4++;
                        break;
                    case 5:
                        vert5++;
                        break;
					case 6:
						vert6++;
						break;
					case 7:
						vert7++;
						break;
					case 8:
						vert8++;
						break;
					case 9:
						vert9++;
						break;
                    default:
                        vertOther++;
						break;
                }
            }

            Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}, {6}", vert4, vert5, vert6, vert7, vert8, vert9, vertOther);

			u = new Point("01");

			dMinus1Polytopes = Generate.dMinus1Polytopes(new List<Point>() { u }, 1);

			Console.WriteLine("u: " + u + ", # of d - 1 polytopes: " + dMinus1Polytopes.Count);

			vert4 = 0;
			vert5 = 0;
			vert6 = 0;
			vert7 = 0;
			vert8 = 0;
			vert9 = 0;
			vertOther = 0;

			for (int i = 0; i < dMinus1Polytopes.Count; i++)
			{
				switch (dMinus1Polytopes[i].getPointsCount())
				{
					case 4:
						vert4++;
						break;
					case 5:
						vert5++;
						break;
					case 6:
						vert6++;
						break;
					case 7:
						vert7++;
						break;
					case 8:
						vert8++;
						break;
					case 9:
						vert9++;
						break;
					default:
						vertOther++;
						break;
				}
			}

            Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}, {6}", vert4, vert5, vert6, vert7, vert8, vert9, vertOther);
        }
    }
}
