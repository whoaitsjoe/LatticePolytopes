using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace UpdatedRP
{
	public class Shell
	{
		public static int totalShelling = 0;
		public static List<Graph> localSymmetryGraphs;
        
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

            int[] initial_gi = new int[Globals.d * 2];

            for (int i = 0; i < Globals.d * 2; i++)
                initial_gi[i] = Globals.gap * 2;

            return shellHelper(subSkeleton, vertexSet, nonvertexSet, coreSet, facetsUsed, u, v, facetDiameter, initial_gi);
		}

        private static List<Graph> shellHelper(Graph currFacets, List<Point> vertexSet, List<Point> nonVertexSet, List<Point> coreSet, bool[] facetsUsed, Point u, Point v, int[] facetDiameter, int[] _gi)
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
			int[] gi = updateGi(currFacets, u, v, _gi);

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
						result.AddRange(shellHelper(temp, _vertexSet, _nonVertexSet, _corePointSet, _facetsUsed, u, v, facetDiameter, gi));
					}
					else
					{
                        if (!CDD.compareAlToPoints(temp.Points)) //convex hull call -- old cdd call, some shelling produced have invalid edges?
                        //if(!CDD.convexHullAdjList())
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
        public static int[] updateGi(Graph currFacets, Point u, Point v, int[] old_gi)
		{
			int[] result = new int[Globals.d * 2];

			for (int i = 0; i < Globals.d * 2; i++)
			{
				result[i] = (i % 2 == 0) ? (Globals.gap - Globals.k + calculateDistanceToFacet(u, currFacets, i) + calculateDistanceToFacet(v, currFacets, i)) : (Globals.gap * 2 - result[i - 1]);

				if (result[i] < 0)
					result[i] = 0;

                result[i] = Math.Min(result[i], old_gi[i]);
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

        //Sort points lexicographically, then compare points list
		public static List<Graph> removeRedundantShelling(List<Graph> graphs)
        {
            List<Graph> uniqueGraphs = new List<Graph>();
			List<List<string>> points = new List<List<string>>();
			bool[] duplicate = new bool[graphs.Count];

			for (int i = 0; i < duplicate.Length; i++)
				duplicate[i] = false;

            //sort points lexicographically
			foreach (Graph g in graphs)
			{
                List<string> thisString = g.pointInStringArray();
				thisString.Sort();
				points.Add(thisString);
			}

            //find duplicates
            for (int i = 0; i < points.Count - 1; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    if (i == j || duplicate[j])
                        continue;

                    if (points[i].SequenceEqual(points[j]))
                        duplicate[j] = true;
                }
            }

            //add non-duplicates to result
            for (int i = 0; i < graphs.Count; i++)
            {
                if (!duplicate[i])
                    uniqueGraphs.Add(graphs[i]);
            }

            return uniqueGraphs;
        }

        public static List<Graph> symmetryGroup(List<Graph> graphs, bool checkFacetIncidence = false)
        {
            List<Graph> uniqueGraphs = new List<Graph>();
			Dictionary<int, List<Graph>> separateByVertexCount = new Dictionary<int, List<Graph>>();
            Dictionary<string, List<Graph>> separateByVertexDegree;
            Dictionary<string, List<Graph>> separateByFacetDegree;

            foreach (Graph g in graphs)
			{
				int vertexCount = g.Points.Count;

				if (separateByVertexCount.ContainsKey(vertexCount))
					separateByVertexCount[vertexCount].Add(g);
				else
					separateByVertexCount.Add(vertexCount, new List<Graph>() { g });
			}

			foreach (KeyValuePair<int, List<Graph>> entry in separateByVertexCount)
			{
				separateByVertexDegree = new Dictionary<string, List<Graph>>();

				foreach (Graph g in entry.Value)
				{
					string degreeSeq = getDegreeSequence(g);
					if (separateByVertexDegree.ContainsKey(degreeSeq))
						separateByVertexDegree[degreeSeq].Add(g);
					else
						separateByVertexDegree.Add(degreeSeq, new List<Graph>() { g });
				}

				foreach (KeyValuePair<string, List<Graph>> entry2 in separateByVertexDegree)
				{
                    separateByFacetDegree = new Dictionary<string, List<Graph>>();

                    if (checkFacetIncidence)
                    {
						foreach (Graph g in entry2.Value)
						{
							if (g.Points.Count > 0)
							{
								string degreeSeq2 = getFacetSequence(g);
								if (degreeSeq2.Length > 0)
								{
									if (separateByFacetDegree.ContainsKey(degreeSeq2))
										separateByFacetDegree[degreeSeq2].Add(g);
									else
										separateByFacetDegree.Add(degreeSeq2, new List<Graph>() { g });
								}
							}
						}

						foreach (KeyValuePair<string, List<Graph>> entry3 in separateByFacetDegree)
						{
							localSymmetryGraphs = new List<Graph>();
							foreach (Graph g in entry3.Value)
							{
								if (uniqueGraphs.Count == 0)
									uniqueGraphs.Add(g.clone());
								else if (!symmetryHelper(g, uniqueGraphs, 1))   //TODO -- update symmetryHelper so it checks all combinations of checks.
									uniqueGraphs.Add(g.clone());
							}
						}
                    }
                    else
                    {
                        localSymmetryGraphs = new List<Graph>();
                        foreach (Graph g in entry2.Value)
                        {
                            if (uniqueGraphs.Count == 0)
                                uniqueGraphs.Add(g.clone());
                            else if (!symmetryHelper(g, uniqueGraphs, 1))
                                uniqueGraphs.Add(g.clone());
                        }
                    }
				}
			}

			return uniqueGraphs;
        }

        //use adjlist to help determine symmetry
		//returns true if a symmetric shape is found in given list of shapes.
		public static bool symmetryHelper(Graph g, List<Graph> currentGraphs, int oper)
		{
			if (oper > 6)
				return false;

			Graph temp = g.clone();

			switch (oper)
			{
				//k - x1
				case 1:
					foreach (Point p in temp.Points)
					{
						//p.Coordinates[0] = Globals.k - p.Coordinates[0];
                        int[] tempArr = p.getIntArray();
                        tempArr[0] = Globals.k - tempArr[0];
                        p.Coordinates = Point.convertIntArrayToString(tempArr);
					}
					break;
				//k - x2
				case 2:
					foreach (Point p in temp.Points)
					{
						//p.Coordinates[1] = Globals.k - p.Coordinates[1];
						int[] tempArr = p.getIntArray();
						tempArr[1] = Globals.k - tempArr[1];
						p.Coordinates = Point.convertIntArrayToString(tempArr);
					}
					break;
				//k - x3
				case 3:
					foreach (Point p in temp.Points)
					{
						//p.Coordinates[2] = Globals.k - p.Coordinates[2];
						int[] tempArr = p.getIntArray();
						tempArr[2] = Globals.k - tempArr[2];
						p.Coordinates = Point.convertIntArrayToString(tempArr);
					}
					break;
				//x1 swap x2
				case 4:
					foreach (Point p in temp.Points)
					{
						//int tempVal = p.Coordinates[0];
						//p.Coordinates[0] = p.Coordinates[1];
						//p.Coordinates[1] = tempVal;

                        int[] tempArr = p.getIntArray();
                        int tempVal = tempArr[0];
                        tempArr[0] = tempArr[1];
                        tempArr[1] = tempVal;
                        p.Coordinates = Point.convertIntArrayToString(tempArr);
					}
					break;
				//x1 swap x3
				case 5:
					foreach (Point p in temp.Points)
					{
						//int tempVal = p.Coordinates[0];
						//p.Coordinates[0] = p.Coordinates[2];
						//p.Coordinates[2] = tempVal;

						int[] tempArr = p.getIntArray();
						int tempVal = tempArr[0];
						tempArr[0] = tempArr[2];
						tempArr[2] = tempVal;
						p.Coordinates = Point.convertIntArrayToString(tempArr);
					}
					break;
				//x2 swap x3
				case 6:
					foreach (Point p in temp.Points)
					{
						//int tempVal = p.Coordinates[1];
						//p.Coordinates[1] = p.Coordinates[2];
						//p.Coordinates[2] = tempVal;

						int[] tempArr = p.getIntArray();
						int tempVal = tempArr[1];
						tempArr[1] = tempArr[2];
						tempArr[2] = tempVal;
						p.Coordinates = Point.convertIntArrayToString(tempArr);
					}
					break;
				default:
					Console.WriteLine("ERROR: Unknown operation parameter for SymmetryHelper.");
					break;
			}

			foreach (Graph h in localSymmetryGraphs)
			{
				if (compareGraphs(temp, h))
					return false;
				localSymmetryGraphs.Add(temp);
			}

			foreach (Graph h in currentGraphs)
			{
				if (compareGraphs(temp, h))
					return true;
			}

			return symmetryHelper(g, currentGraphs, oper + 1) || symmetryHelper(temp, currentGraphs, oper + 1);
		}

		//determine sequence of operations.
        //recursively iterate over every operation at every level. Could this be too slow?

		public static bool symmetryHelperNew(Graph g, List<Graph> currentGraphs, int oper)
		{
            Graph temp = g.clone();

            for (int i = oper; i <= 6; i++)
			{
                switch(i)
                {
					//k - x1
					case 1:
						foreach (Point p in temp.Points)
						{
							int[] tempArr = p.getIntArray();
							tempArr[0] = Globals.k - tempArr[0];
							p.Coordinates = Point.convertIntArrayToString(tempArr);
						}
						break;
					//k - x2
					case 2:
						foreach (Point p in temp.Points)
						{
							int[] tempArr = p.getIntArray();
							tempArr[1] = Globals.k - tempArr[1];
							p.Coordinates = Point.convertIntArrayToString(tempArr);
						}
						break;
					//k - x3
					case 3:
						foreach (Point p in temp.Points)
						{
							int[] tempArr = p.getIntArray();
							tempArr[2] = Globals.k - tempArr[2];
							p.Coordinates = Point.convertIntArrayToString(tempArr);
						}
						break;
					//x1 swap x2
					case 4:
						foreach (Point p in temp.Points)
						{
							int[] tempArr = p.getIntArray();
							int tempVal = tempArr[0];
							tempArr[0] = tempArr[1];
							tempArr[1] = tempVal;
							p.Coordinates = Point.convertIntArrayToString(tempArr);
						}
						break;
					//x1 swap x3
					case 5:
						foreach (Point p in temp.Points)
						{
							int[] tempArr = p.getIntArray();
							int tempVal = tempArr[0];
							tempArr[0] = tempArr[2];
							tempArr[2] = tempVal;
							p.Coordinates = Point.convertIntArrayToString(tempArr);
						}
						break;
					//x2 swap x3
					case 6:
						foreach (Point p in temp.Points)
						{
							int[] tempArr = p.getIntArray();
							int tempVal = tempArr[1];
							tempArr[1] = tempArr[2];
							tempArr[2] = tempVal;
							p.Coordinates = Point.convertIntArrayToString(tempArr);
						}
						break;
					default:
						Console.WriteLine("ERROR: Unknown operation parameter for SymmetryHelper.");
						break;
                }

				foreach (Graph h in currentGraphs)
				{
					if (compareGraphs(temp, h))
						return true;
				}

                //recursively call remaining operations if current operation does not detect symmetry.
                if (symmetryHelperNew(g, currentGraphs, oper + 1))
                    return true;
            }

            return false;
		}

		//returns true if both graphs have the same vertices.
		public static bool compareGraphs(Graph g, Graph h)
		{
			if (g.Points.Count != h.Points.Count)
				return false;

			bool found;

			foreach (Point p in g.Points)
			{
				found = false;
				foreach (Point q in h.Points)
				{
					if (p.Equals(q))
					{
						found = true;
						break;
					}
				}

				if (!found)
					return false;
			}

			return true;
		}

		public static string getDegreeSequence(Graph g)
		{
			List<int> degrees = new List<int>();
			StringBuilder builder = new StringBuilder();

			foreach (KeyValuePair<string, List<string>> entry in g.AdjList)
			{
				degrees.Add(entry.Value.Count);
			}

			degrees.Sort();

			foreach (int i in degrees)
				builder.Append(i);

			return builder.ToString();
		}

        public static string getFacetSequence(Graph g)
        {
            List<int> degrees = new List<int>();
            StringBuilder builder = new StringBuilder();

            string fileName = "/Users/Joe/Code/research/Files/temp/out.ext";
            Parse.writeToFile(g.Points, fileName);

			var proc = new Process
			{
				StartInfo = new ProcessStartInfo
                {
                    FileName = "/Users/Joe/Code/research/cdd/src/scdd",
                    Arguments = fileName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = false,
					CreateNoWindow = true
				}
			};

            proc.Start();
            proc.WaitForExit();
                   
            string incidenceFile = "/Users/Joe/Code/research/Files/temp/out.icd";
            degrees = FileIO.readFacetIncidence(incidenceFile);

			foreach (int i in degrees)
				builder.Append(i);
            
            return builder.ToString();
        }

        public static List<Graph> checkInterior(List<Graph> shellings)
        {
			List<Point> points;
			List<Point> possibleInterior;
			List<Graph> result = new List<Graph>();

			foreach (Graph g in shellings)
			{
				points = new List<Point>();
				possibleInterior = new List<Point>();

				foreach (Point h in g.Points)
					points.Add(h.clone());

				//todo -- generalize, currently hardcoded for d=3
				for (int i = 1; i < Globals.k; i++)
				{
					for (int j = 1; j < Globals.k; j++)
					{
						for (int k = 1; k < Globals.k; k++)
						{
                            Point temp = new Point(i.ToString() + j.ToString() + k.ToString());
							points.Add(temp);

							if (CDD.convexHullVertexList(points))
								possibleInterior.Add(temp.clone());

							points.Remove(temp);
						}
					}
				}

				//Console.WriteLine("Total Number of interior points: {0}", possibleInterior.Count);
				//Console.WriteLine("u: {0}, {1}, {2}. v: {3}, {4}, {5}", Globals.u.Coordinates[0],
				//Globals.u.Coordinates[1], Globals.u.Coordinates[2], Globals.v.Coordinates[0],
				//Globals.v.Coordinates[1], Globals.v.Coordinates[2]);
				//Console.WriteLine("Number of potential interior points: {0}", possibleInterior.Count);
                List<Graph> tempVar = checkInteriorHelper(g, possibleInterior);

                if (tempVar.Count == 0)
                    Console.WriteLine("aa");

                result.AddRange(tempVar);
			}

			return result;
        }

		//takes in graph g (full shelling), and a list of possible interior points and tries to find all feasible shelling
		public static List<Graph> checkInteriorHelper(Graph g, List<Point> interiorPoints)
		{
			List<Point> points = new List<Point>();
			List<Graph> result = new List<Graph>();
			List<Point> graphPoints;
			Graph h = g.clone();

			if (interiorPoints.Count == 0)
			{
				if (CDD.convexHullAdjList(g.Points, new List<Point>(), out h))
				{
                    //todo -- see if this is relevant
					//Globals.internalPointShelling++;
					//Globals.interiorPolytopes.Add(h.clone());

					if (h.diameter() >= Globals.maxDiameter[Globals.k] + Globals.k - Globals.gap)
					{
						result.Add(h);
						return result;
					}

					return new List<Graph>();
				}
				else
					return new List<Graph>();
			}

			foreach (Point p in interiorPoints)
				points.Add(p.clone());

			graphPoints = h.getAllContainedPoints();
			graphPoints.Add(interiorPoints[0]);
			points.Remove(interiorPoints[0]);

			if (CDD.convexHullAdjList(graphPoints, new List<Point>(), out h))
			{
				result.AddRange(checkInteriorHelper(h, points));
			}

			if (interiorPoints.Count > 0)
				result.AddRange(checkInteriorHelper(g, points));

			return result;
		}

		//retract, check diameter (no other checks required)
		public static Graph retractable(Graph g, int direction, List<Point> unchangedVertexSet)
		{
			Graph newGraph = new Graph();
			Graph output = new Graph();
			bool found;

			List<Point> newPoints = new List<Point>();

			foreach (Point p in unchangedVertexSet)
				newPoints.Add(p.clone());

			foreach (Point p in g.Points)
			{
				found = false;
				foreach (Point q in unchangedVertexSet)
				{
					if (p.Equals(q))
					{
						found = true;
						break;
					}
				}

				if (!found)
				{
                    int[] tempArr = p.getIntArray();

					if (direction < Globals.d)
						tempArr[direction]--;
					else if (direction == Globals.d)
					{
						tempArr[0]--;
						tempArr[1]--;
					}
					else if (direction == Globals.d + 1)
					{
						tempArr[0]--;
						tempArr[2]--;
					}
					else if (direction == Globals.d + 2)
					{
						tempArr[1]--;
						tempArr[2]--;
					}
                    newPoints.Add(new Point(Point.convertIntArrayToString(tempArr)));
				}
			}

			CDD.convexHullAdjList(new List<Point>(), newPoints, out output);

            return output;
		}

        /*
        public static void retractable(Graph g, int direction, List<Point> unchangedVertexSet, out Graph output)
        {
            output = new Graph();
        }*/
		public static List<Graph> retractable(List<Graph> inputGraph)
		{
			List<Graph> result = new List<Graph>();

			return result;
		}


		//direction is from 0 up to d - 1, where 0 = x1, 1 = x2, ..., x1x2, x1x3, x2x3
		public static bool expandable(Graph g, out List<int> direction, out List<List<Point>> unchangedVertexSet)
		{
			bool result = false;

			unchangedVertexSet = new List<List<Point>>();
			direction = new List<int>();
			List<Point> sameVertexSet;

			//check 2*d directions, unit vectors + combinations of 2 vectors (e.g. 110,101,011).
			for (int i = 0; i < Globals.d * 2; i++)
			{
				if (expandableHelper(g, i, out sameVertexSet))
				{
                    unchangedVertexSet.Add(sameVertexSet);
					direction.Add(i);
					result = true;
				}
			}

			return result;
		}

		public static bool expandableHelper(Graph g, int direction, out List<Point> unchangedVertexSet)
		{
			List<Point> newPointList = new List<Point>();
			unchangedVertexSet = new List<Point>();
			Graph newGraph;

			foreach (Point p in g.Points)
			{
				int[] coords = new int[g.Points[0].getDimension()];
				for (int i = 0; i < g.Points[0].getDimension(); i++)
				{
					//todo -- generalize for all directions and all d's
					if (direction < Globals.d)
						coords[i] = (i == direction) ? p.Coordinates[i] + 1 : p.Coordinates[i];
					else if (direction == Globals.d)
						coords[i] = (i == 0 || i == 1) ? p.Coordinates[i] + 1 : p.Coordinates[i];
					else if (direction == Globals.d + 1)
						coords[i] = (i == 0 || i == 2) ? p.Coordinates[i] + 1 : p.Coordinates[i];
					else if (direction == Globals.d + 2)
						coords[i] = (i == 1 || i == 2) ? p.Coordinates[i] + 1 : p.Coordinates[i];
				}

				Point q = new Point(coords);

				newPointList.Add(q);
				newPointList.Add(p.clone());
			}

			if (!CDD.convexHullAdjList(new List<Point>(), newPointList, out newGraph))
				return false;

			if (newGraph.Points.Count == g.Points.Count)
            {
				foreach (Point p in g.Points)
				{
					foreach (Point q in newGraph.Points)
					{
						if (p.Equals(q))
						{
							unchangedVertexSet.Add(p.clone());
							break;
						}
					}
				}
                return true;
            }
			else
				return false;
		}

        //Takes in a graph, checks to see if it's expandable in every direction, then
        //retract in expandable directions and checks diameter of retracted to see
        //if result is a valid polytope.
        public static bool retractable(Graph g, out List<Graph> outGraphs)
        {
            outGraphs = new List<Graph>();
            List<int> directions;
            List<List<Point>> unchangedPoints;

            if(!expandable(g, out directions, out unchangedPoints))
                return false;
            else
            {
                foreach(int dir in directions)
                {
                    
                }
            }

            return true;
        }

		//retract, check diameter (no other checks required)
		public static void retractable(Graph g, int direction, List<Point> unchangedVertexSet, out Graph output)
		{
			Graph newGraph = new Graph();
			output = new Graph();
			bool found;

			List<Point> newPoints = new List<Point>();

			foreach (Point p in unchangedVertexSet)
				newPoints.Add(p.clone());

			foreach (Point p in g.Points)
			{
				found = false;
				foreach (Point q in unchangedVertexSet)
				{
					if (p.Equals(q))
					{
						found = true;
						break;
					}
				}

				if (!found)
				{
					Point temp = p.clone();

					if (direction < Globals.d)
						temp.Coordinates[direction]--;
					else if (direction == Globals.d)
					{
						temp.Coordinates[0]--;
						temp.Coordinates[1]--;
					}
					else if (direction == Globals.d + 1)
					{
						temp.Coordinates[0]--;
						temp.Coordinates[2]--;
					}
					else if (direction == Globals.d + 2)
					{
						temp.Coordinates[1]--;
						temp.Coordinates[2]--;
					}
					newPoints.Add(temp);
				}
			}

			CDD.convexHullAdjList(new List<Point>(), newPoints, out output);
		}
	}
}
