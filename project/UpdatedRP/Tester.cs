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
    }
}
