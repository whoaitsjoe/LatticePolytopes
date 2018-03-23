using System;
using System.Collections.Generic;
using System.Linq;

namespace UpdatedRP
{
    public class LineSearch
    {
        public static List<int> search()
        {
            List<int> result = new List<int>();

            return result;
        }

        public static Graph search(Graph graph)
        {

            return graph;
        }

        public static int[] search(int[] input)
        {


            return input;
        }

        //How to determine which points are used as input??
        public static int[] findVector(Point a, Point b) //where a is starting point (i.e. ray shoots towards b)
        {
            int[] result = new int[a.Dimension];

            for (int i = 0; i < a.Dimension; i++)
                result[i] = b.Coordinates[i] - a.Coordinates[i];

            int gcd = GCD(result);

            for (int i = 0; i < a.Dimension; i++)
                result[i] = result[i] / gcd;

            return result;
        }

        public static int GCD(int[] input)
        {
            return input.Aggregate(GCD);
        }
        public static int GCD(int a, int b)
        {
            return b == 0 ? a : GCD(b, a % b);
        }
    }
}
