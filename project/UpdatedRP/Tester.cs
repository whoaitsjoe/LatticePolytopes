﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;

namespace UpdatedRP
{
    public class Tester
    {
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
