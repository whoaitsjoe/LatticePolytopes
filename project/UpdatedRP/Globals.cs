using System;
using System.Collections.Generic;

//Holds all global variables/enums
namespace UpdatedRP
{
    public class Globals
    {
        public static int d;
		public static int k;
		public static int gap;
        public static int[] maxDiameter;
        public static int maxLength;
        public static bool messageOn;
        public static bool checkVStar = true;
        public static int convexHullCount;
        public static int diameter;
        public static long chTime;  //keeps track of time spent on convex hulls
        public static List<Point> nonVertices, corePoints;
        public static List<Point> fixedVertices; //allows user to customize certain points to be vertices.
        public static Point u, v;

        //vertex, nonvertex, core set definitions
		public static Dictionary<string, List<Point>> vertexSet;
		public static Dictionary<string, List<Point>> nonvertexSet;
		public static Dictionary<string, List<Point>> coreSet;

        //Directory for pre-computed data
        public static string directory = "/Users/Joe/Code/research/latticePolytopes/Files/";

        //bool to save results
        public static bool writeToFile = false;

        public static void initialize()
        {
            vertexSet = new Dictionary<string, List<Point>>();
            nonvertexSet = new Dictionary<string, List<Point>>();
            coreSet = new Dictionary<string, List<Point>>();
            fixedVertices = new List<Point>();
        }
    }
}
