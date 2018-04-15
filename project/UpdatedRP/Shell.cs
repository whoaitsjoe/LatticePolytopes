﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace UpdatedRP
{
	public class Shell
	{
		public static int totalShelling = 0;
        
        //***********************
		//Shelling Main functions
		//***********************
		public static List<Graph> shell(Point u, Point v, List<Point> vertices = null)
		{
            //todo -- remove, test
            Globals.recursionDepth = 0;

			bool[] facetsUsed = new bool[Globals.d * 2];    //bool array to track which facets have been included. Order is x0 = 0, x0 = k, x1 = 0, etc.
			int[] facetDiameter = new int[Globals.d * 2];   //int array tracking upper bound of diameter of each facet
			Graph subSkeleton = new Graph();
            List<Point> vertexSet = new List<Point>();
            List<Point> nonvertexSet = new List<Point>();
            List<Point> coreSet = new List<Point>();

			for (int i = 0; i < Globals.d * 2; i++)
			{
				facetsUsed[i] = false;
				facetDiameter[i] = Globals.maxDiameter[Globals.k];
			}

			subSkeleton.Points.Add(u.clone());
            subSkeleton.AdjList.Add(u.ToString(), new List<string>());
			subSkeleton.Points.Add(v.clone());
			subSkeleton.AdjList.Add(v.ToString(), new List<string>());

            if (vertices != null)
            {
                foreach (Point p in vertices)
                {
                    subSkeleton.Points.Add(p.clone());
                    subSkeleton.AdjList.Add(p.ToString(), new List<string>());
                    vertexSet.Add(p);
                }
            }

            foreach (Point p in Globals.fixedVertices)
                vertexSet.Add(p);

            return shellHelper(subSkeleton, vertexSet, nonvertexSet, coreSet, facetsUsed, u, v, facetDiameter);
		}

        private static List<Graph> shellHelper(Graph currFacets, List<Point> vertexSet, List<Point> nonVertexSet, List<Point> coreSet, bool[] facetsUsed, Point u, Point v, int[] facetDiameter)
		{
            if (Globals.messageOn)
                Console.WriteLine("Number of added facets in shelling: " + facetsUsed.Where(c => c).Count());
            
			List<Graph> result = new List<Graph>();
			int sp;
			bool uRedundant, vRedundant;    //track if either u or v is redundant. For message output.
			bool[] _facetsUsed = new bool[facetsUsed.Length];
			List<Point> _nonVertexSet = new List<Point>(), _corePointSet = new List<Point>(), _vertexSet = new List<Point>();

			//creating copies of reference data
			for (int i = 0; i < facetsUsed.Length; i++)
				_facetsUsed[i] = facetsUsed[i];
            
			foreach (Point p in nonVertexSet)
				_nonVertexSet.Add(p.clone());

			foreach (Point p in coreSet)
				_corePointSet.Add(p.clone());

			foreach (Point p in vertexSet)
				_vertexSet.Add(p.clone());
            
			//calculates gi values
			int[] gi = updateGi(currFacets, u, v);

			//calculates next facet to add
			int nextFacet = calculateNextFacet(u, v, currFacets, facetsUsed, gi);

			List<Point> facetPoints = currFacets.getAllContainedPoints(nextFacet);
			List<Point> dMinus1FacetPoints = new List<Point>();

			if (Globals.messageOn)
                Console.WriteLine("Now adding facet: " + nextFacet);

			foreach (Point p in facetPoints)
			{
                if (Convert.ToInt32(p.Coordinates[nextFacet / 2].ToString()) == ((nextFacet % 2 == 0) ? 0 : Globals.k))
					dMinus1FacetPoints.Add(p.decreaseDimensionality(nextFacet / 2));
			}

			//generate all valid d-1 polytopes that can be considered as a facet.
			List<Graph> possibleFacets = Generate._dMinus1Polytopes(dMinus1FacetPoints, gi[nextFacet]);

			_facetsUsed[nextFacet] = true;

			if (possibleFacets != null && possibleFacets.Count > 0)
			{
				if (Globals.messageOn)
					Console.WriteLine("Number of possible facets: " + possibleFacets.Count);

				//try every possible facet as a shell
				foreach (Graph f in possibleFacets)
				{
					//todo -- test if .clone() is actually required
 					Graph temp = currFacets.clone();
					Graph h = f.clone();

					//set values for current selected facet to shell
					facetDiameter[nextFacet] = f.Points.Count / 2;

					//increase dimensionality of facet
					h.addDimensionality(nextFacet / 2, (nextFacet % 2 == 0) ? true : false);

					//add facet to gamma
					temp.AddFacet(h);

					//check 1 - looks for sp from u to v
					sp = ShortestPath.BFS(temp, u, v);
					if (sp >= 0 && sp < Globals.maxDiameter[Globals.k] + Globals.k - Globals.gap)
					{
						if (Globals.messageOn)
							Console.WriteLine("SP < " + Globals.maxLength);

						continue;
					}

					//check 2 - gamma check
					int gamma = calculateGamma(u, v, temp, gi, facetDiameter);
					if (gamma > 0)
					{
						if (Globals.messageOn)
							Console.WriteLine("Gamma > 0.");

						continue;
					}

					//check 3 - convex core check -- u and v must be vertices of the convex hull generated by the current subgraph.
					if (!checkCore(u, v, gi, temp.getAllContainedPoints(), out uRedundant, out vRedundant))
					{
						if (Globals.messageOn)
							Console.WriteLine("Convex Core Check. {0}", ((uRedundant && vRedundant) ? "u and v are not vertices." : (uRedundant ? "u is not a vertex." : "v is not a vertex.")));

						continue;
					}

					//if shelling is incomplete
					if (checkUnshelledFacet(_facetsUsed))
					{
						result.AddRange(shellHelper(temp, _vertexSet, _nonVertexSet, _corePointSet, _facetsUsed, u, v, facetDiameter));
					}
					else
					{
						if (!CDD.compareAlToPoints(temp.Points)) //convex hull call
							continue;

						totalShelling++;

						if (Globals.messageOn)
						{
							Console.WriteLine("***** FULL SHELLING FOUND *****");
							Console.WriteLine(temp);
						}

						result.Add(temp);
					}
				}

				facetDiameter[nextFacet] = Globals.maxDiameter[Globals.k];

				if (Globals.messageOn)
					Console.WriteLine("All facets checked.");

                //todo --  remove, test
                Globals.recursionDepth--;

				return result;
			}
			else
			{
				if (Globals.messageOn)
					Console.WriteLine("No possible facets.");

				//todo --  remove, test
				Globals.recursionDepth--;

				return new List<Graph>();
			}
        }

		//*************************
		//Shelling Helper Functions
		//*************************
		public static int[] updateGi(Graph currFacets, Point u, Point v)
		{
			int[] result = new int[Globals.d * 2];

			for (int i = 0; i < Globals.d * 2; i++)
			{
				result[i] = (i % 2 == 0) ? (Globals.gap - Globals.k + calculateDistanceToFacet(u, currFacets, i) + calculateDistanceToFacet(v, currFacets, i)) : (Globals.gap * 2 - result[i - 1]);

				if (result[i] < 0)
					result[i] = 0;
			}

			return result;
		}

		public static int calculateDistanceToFacet(Point u, Graph currFacets, int facet)
		{
			return Math.Min(ShortestPath.BFStoFacet(currFacets, u, facet), ShortestPath.BFStoFacetEstimator(currFacets, u, facet));
		}

		public static int calculateNextFacet(Point u, Point v, Graph currFacets, bool[] facetsUsed, int[] gi)
		{
			int min = Globals.k;
			int result = -1;

			for (int i = 0; i < gi.Length; i++)
			{
				if (!facetsUsed[i])
				{
                    if (result < 0)
                        result = i;
                    
					//first check for gi: score
					if (gi[i] < min)
					{
						min = gi[i];
						result = i;
					}
					else if (gi[i] == min)
					{
						List<Point> iPoints = currFacets.getAllContainedPoints(i / 2, (i % 2 == 0 ? 0 : 1));
						List<Point> mPoints = currFacets.getAllContainedPoints(result / 2, (result % 2 == 0 ? 0 : 1));

						//second check for gi: # of points
						if (iPoints.Count > mPoints.Count)
						{
							min = gi[i];
							result = i;
						}
						else if (iPoints.Count == mPoints.Count)
						{
							bool iContainsUV = false;
							bool mContainsUV = false;

							//third check for gi: facet contains u or v takes priority over facet containing neither
							foreach (Point p in iPoints)
							{
								if (p.Equals(u) || p.Equals(v))
								{
									iContainsUV = true;
									break;
								}
							}

							if (iContainsUV)
							{
								foreach (Point p in mPoints)
								{
									if (p.Equals(u) || p.Equals(v))
									{
										mContainsUV = true;
										break;
									}
								}

                                //todo -- shouldn't this check be mContainsUV and else assign i as result?
								if (!mContainsUV)
								{
									//fourth check for gi: ordering of Fi0, F20, ..., F1k, F2k, ..., Fdk
									if (i % 2 == 0)
									{
										if (result % 2 == 1)
										{
											min = gi[i];
											result = i;
										}
									}
								}
							}
						}
					}
				}
			}

			return result;
		}

		public static int calculateGamma(Point u, Point v, Graph currFacets, int[] gi, int[] facetDiameter)
		{
			int shortestPath = ShortestPath.BFS(currFacets, u, v);

			if (shortestPath < 0)
				shortestPath = Globals.maxDiameter[Globals.k] + Globals.k - Globals.gap;

			return Globals.maxDiameter[Globals.k] + Globals.k - Globals.gap - Math.Min(shortestPath, calculateDDot(u, v, currFacets, facetDiameter));
		}

		public static int calculateDDot(Point u, Point v, Graph currFacets, int[] facetDiameter)
		{
			int result = Globals.maxDiameter[Globals.k] + Globals.k - Globals.gap;
			int[] dTilda_uFi = new int[Globals.d * 2]; //order is F10, F1k, F20, F2k, ...
			int[] dTilda_vFi = new int[Globals.d * 2];
            int val;

			//calculate d tildas
			for (int i = 0; i < Globals.d * 2; i++)
			{
                dTilda_vFi[i] = Math.Min(calculateDistanceToFacet(v, currFacets, i), (i % 2 == 0) ? Convert.ToInt32(v.Coordinates[i / 2].ToString()) : Globals.k - Convert.ToInt32(v.Coordinates[i / 2].ToString()));
                dTilda_uFi[i] = Math.Min(calculateDistanceToFacet(u, currFacets, i), (i % 2 == 0) ? Convert.ToInt32(u.Coordinates[i / 2].ToString()) : Globals.k - Convert.ToInt32(u.Coordinates[i / 2].ToString()));
			}

			for (int i = 0; i < Globals.d; i++)
			{
                val = Math.Min(dTilda_uFi[i * 2] + dTilda_vFi[i * 2] + (facetDiameter[i * 2]), dTilda_uFi[i * 2 + 1] + dTilda_vFi[i * 2 + 1] + (facetDiameter[i * 2]));
                if (val < result)
                    result = val;
			}

            if(Globals.messageOn)
			    Console.WriteLine("dTilda_u: {0}, {1}, {2}, {3}, {4}, {5}. dTilda_v: {6}, {7}, {8}, {9}, {10}, {11}. facet diameter: {12}, {13}, {14}, {15}, {16}, {17}",
							 dTilda_uFi[0], dTilda_uFi[1], dTilda_uFi[2], dTilda_uFi[3], dTilda_uFi[4], dTilda_uFi[5], dTilda_vFi[0], dTilda_vFi[1],
							 dTilda_vFi[2], dTilda_vFi[3], dTilda_vFi[4], dTilda_vFi[5], facetDiameter[0], facetDiameter[1], facetDiameter[2],
							 facetDiameter[3], facetDiameter[4], facetDiameter[5]);

			return result;
		}

		public static bool checkCore(Point u, Point v, int[] gi, List<Point> vertices, out bool uRedundant, out bool vRedundant)
		{
			List<Point> corePoints = new List<Point>();
            string index;

			corePoints.AddRange(vertices);

            for (int i = 0; i < Globals.d; i++)
            {
				index = (Globals.d - 1).ToString() + Globals.k.ToString() + gi[i].ToString();
               
                if(Globals.coreSet.ContainsKey(index))
				{
                    foreach(Point p in Globals.coreSet[index])
                    {
                        Point q = p.clone();
                        q.increaseDimensionality(i / 2, (i % 2 == 0));
                        corePoints.Add(q);
                    }
                }
            }

            //todo -- this should check that the vertices remain vertices after chull.
			return CDD.convexHullVertex(corePoints, u, v, out uRedundant, out vRedundant);
		}

		public static bool checkUnshelledFacet(bool[] usedFacets)
		{
			foreach (bool x in usedFacets)
			{
				if (!x)
					return true;
			}
			return false;
		}

        public static void updateCorePoints(List<Point> nonVertexPoints, Graph g, int nextFacet)
        {
			bool found;

			//hard coded for d = 3
			for (int i = 0; i <= Globals.k; i++)
			{
				for (int j = 0; j <= Globals.k; j++)
				{
                    Point temp = new Point(i.ToString("0") + j.ToString("0"));
					found = false;
					foreach (Point p in g.Points)
					{
						if (p.Equals(temp))
						{
							found = true;
							break;
						}
					}

					if (!found)
					{
						temp.increaseDimensionality(nextFacet);
						nonVertexPoints.Add(temp);
					}
				}
			}
        }

        public static List<Point> updateNonVertexSet(List<Point> nonVertexPoints, Graph g, int nextFacet)
        {
            List<Point> result = new List<Point>();
            bool found;

            foreach (Point p in nonVertexPoints)
                result.Add(p);

            //hard coded for d = 3
            for (int i = 0; i <= Globals.k; i++)
            {
                for (int j = 0; j <= Globals.k; j++)
				{
					Point temp = new Point(i.ToString("0") + j.ToString("0"));
                    found = false;
                    foreach(Point p in g.Points)
                    {
                        if(p.Equals(temp))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        temp.increaseDimensionality(nextFacet);
                        result.Add(temp);
                    }
                }
            }

            return result;
        }

		//************************
		//After shelling functions
		//************************
		//todo -- any reason why one function has out parameter and another returns something?
		public static void symmetryGroup(List<Graph> inputGraphs, out List<Graph> graphTypes)
        {
            graphTypes = new List<Graph>();
        }
        public static List<Graph> checkInterior(List<Graph> shellings)
        {
            return new List<Graph>();
        }
        public static void retractable(Graph g, int direction, List<Point> unchangedVertexSet, out Graph output)
        {
            output = new Graph();
        }
		public static List<Graph> retractable(List<Graph> inputGraph)
		{
			List<Graph> result = new List<Graph>();

			return result;
		}
	}
}
