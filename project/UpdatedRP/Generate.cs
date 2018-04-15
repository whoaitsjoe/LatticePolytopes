using System;
using System.Collections.Generic;

namespace UpdatedRP
{
	public class Generate
	{
        public static int totalCount = 0;

		public static List<Graph> _dMinus1Polytopes(List<Point> vertices, int gap)
		{
			bool found;
			List<Graph> result = new List<Graph>();

            if (vertices.Count == 0)
                return dMinus1Polytopes(vertices, gap);
            
            List<Graph> validGraphs = Globals._222polytopes[vertices[0].ToString()];

			foreach (Graph g in validGraphs)
			{
				if ((gap == 0 && g.getPointsCount() < 6) || (gap == 1 && g.getPointsCount() < 4))
					continue;

				found = true;
				foreach (Point p in vertices)
				{
					if (!g.contains(p))
					{
						found = false;
						break;
					}
				}

				if (found)
					result.Add(g);
			}

			return result;
		}

		public static List<Graph> dMinus1Polytopes(List<Point> vertices, int gap)
		{
            string index = Convert.ToInt32(Globals.d - 1).ToString() + Convert.ToInt32(Globals.k).ToString() + gap.ToString();
            List<Point> nonVertices = (Globals.nonvertexSet.ContainsKey(index)) ? Globals.nonvertexSet[index] : new List<Point>();
            List<Point> corePoints = (Globals.coreSet.ContainsKey(index)) ? Globals.coreSet[index] : new List<Point>();

            //todo -- temp, remove
            List<Graph> temp = dMinus1PolytopesHelper(vertices, nonVertices, corePoints, 0, -1, gap);
            return temp;
		}

		public static void dMinus1PolytopesToFile(List<Point> vertices, int gap)
		{
			string index = Convert.ToInt32(Globals.d - 1).ToString() + Convert.ToInt32(Globals.k).ToString() + gap.ToString();
			List<Point> nonVertices = (Globals.nonvertexSet.ContainsKey(index)) ? Globals.nonvertexSet[index] : new List<Point>();
			List<Point> corePoints = (Globals.coreSet.ContainsKey(index)) ? Globals.coreSet[index] : new List<Point>();

            //dMinus1PolytopesHelper(vertices, nonVertices, corePoints, 0, -1, gap);
		}

        //todo -- generalize this function to any d, currently using iVal and jVal.
		//This function currently generates just delta(d-1,k) - gap polytopes, should also generate delta(d-1,k) polytopes
		public static List<Graph> dMinus1PolytopesHelper(List<Point> vertices, List<Point> nonVertices, List<Point> corePoints, int iVal, int jVal, int gap)
		{         
			List<Graph> result = new List<Graph>();
			List<Point> currVertices = new List<Point>();
			List<Point> currNonVertices = new List<Point>();
			Graph facet;

			if (vertices.Count > (Globals.maxDiameter[Globals.k] * 2 + 1))
				return result;

            //commented out to remove duplicates
			/*if (vertices.Count == (Globals.maxDiameter[Globals.k] * 2 + 1) || vertices.Count == (Globals.maxDiameter[Globals.k] * 2))
			{
				Graph tempGraph;
				if (CDD.convexHullAdjList(vertices, new List<Point>(), out tempGraph))
				{
					totalCount++;
					result.Add(tempGraph.clone());
				}
			}*/

			foreach (Point p in vertices)
				currVertices.Add(p.clone());
			foreach (Point p in nonVertices)
				currNonVertices.Add(p.clone());

			for (int i = iVal; i <= Globals.k; i++)
			{
				int vCount = vertexCount(vertices, i);
				bool _found;
				//find max number of potential vertices (e.g. terminate if not enough vertices can exist)
				if (vCount + (2 * (Globals.k - (iVal - 1))) < 2 * (Globals.maxDiameter[Globals.k] - gap))
					return result;

				for (int j = (i == iVal ? jVal + 1 : 0); j <= Globals.k; j++)
				{
					_found = false;

					foreach (Point p in nonVertices)
					{
                        if (i == (int)Char.GetNumericValue(p.Coordinates[0]) && j == (int)Char.GetNumericValue(p.Coordinates[1]))
						{
							_found = true;
							break;
						}
					}
					if (!_found)
					{
						foreach (Point p in vertices)
						{
                            if (i == (int)Char.GetNumericValue(p.Coordinates[0]) && j == (int)Char.GetNumericValue(p.Coordinates[1]))
							{
								_found = true;
								break;
							}
						}
					}

					if (_found)
						continue;

					Point tempVertex = new Point(new int[2] { i, j });

					currVertices.Add(tempVertex);

					if (!check2PointsInRow(currVertices, i))
					{
						currVertices.Remove(tempVertex);
						jVal = 0;
						break;
					}
					else if (!check2PointsInCol(currVertices, j))
					{
						currVertices.Remove(tempVertex);
						continue;
					}
					else if (!checkDiagonals(currVertices))
					{
						currVertices.Remove(tempVertex);
						continue;
					}
					else
					{
						if (CDD.convexHullAdjList(currVertices, corePoints, out facet))
						{
							if (currVertices.Count >= (Globals.maxDiameter[Globals.k] - gap) * 2)
							{
								totalCount++;
							    result.Add(facet.clone());
								result.AddRange(dMinus1PolytopesHelper(currVertices, currNonVertices, corePoints, i, j, gap));
							}
							else
								result.AddRange(dMinus1PolytopesHelper(currVertices, currNonVertices, corePoints, i, j, gap));
						}
					}

					currVertices.Remove(tempVertex);
					currNonVertices.Add(tempVertex.clone());
				}
			}

			return result;
		}

		public static bool check2PointsInRow(List<Point> points, int row)
		{
			int count = 0;
			foreach (Point p in points)
			{
				if (p.Coordinates[0] == row)
					count++;
			}
			if (count > 2)
				return false;

			return true;
		}
		public static bool check2PointsInCol(List<Point> points, int col)
		{
			int count = 0;
			foreach (Point p in points)
				if (p.Coordinates[1] == col)
					count++;

			if (count > 2)
				return false;

			return true;
		}
		public static bool checkDiagonals(List<Point> points)
		{
			int count1 = 0;
			int count2 = 0;
			foreach (Point p in points)
			{
				if (p.Coordinates[0] == p.Coordinates[1])
					count1++;
				if (p.Coordinates[0] + p.Coordinates[1] == Globals.k)
					count2++;
			}

			if (count1 > 2 || count2 > 2)
				return false;

			return true;
		}

		public static int vertexCount(List<Point> points, int iVal)
		{
			int result = 0;

			foreach (Point p in points)
			{
                if ((int)Char.GetNumericValue(p.Coordinates[0]) < iVal)
					result++;
			}
			return result;
		}

		/*
        public static List<Graph> generatePolytopesScore(List<Point> vertices, List<Point> nonVertices, List<Point> core, int[] rowScores, int[] colScores)
        {
            List<Point> tempList = new List<Point>();
            //List<Point> _vertices = new List<Point>();
            List<Point> _nonVertices = new List<Point>();
            List<Graph> result = new List<Graph>();

            foreach (Point q in nonVertices)
                _nonVertices.Add(q.clone());

            //first, iterate through all row scores of 1
            for (int i = 0; i < rowScores.Length; i++)
            {
                if(rowScores[i] == 1)
                {
                    //first, iterate through all col scores of 1
                    for (int j = 0; j < colScores.Length; j++)
                    {
                        if(colScores[j] == 1)
                        {
                            Point temp = new Point(2, new int[2] { i, j });

                            if (checkPointInList(vertices, _nonVertices, temp))
                                continue;
                            
                            vertices.Add(temp);
                            rowScores[i]++;
                            colScores[j]++;

                            if (rowScores[i] == 2)
                                updateNV(ref _nonVertices, true, i);
                            if (colScores[i] == 2)
                                updateNV(ref _nonVertices, false, i);

                            //move to next check, convex hull
                            if(CDD.convexHullVertexList(vertices))
                            {
                                if(vertices.Count == Globals.diameter * 2 + 1)
                                {
                                    result.Add(new Graph(vertices, new Dictionary<int, List<int>>()));
                                    vertices.Remove(temp);
                                    rowScores[i]--;
                                    colScores[j]--;
                                    return result;
                                }
                                else if (vertices.Count == Globals.diameter * 2)
                                    result.Add(new Graph(vertices, new Dictionary<int, List<int>>()));
                                
                                //current point can be considered as a vertex
                                result.AddRange(generatePolytopesScore(vertices, core, _nonVertices, rowScores, colScores));
                            }

                            vertices.Remove(temp);
                            rowScores[i]--;
                            colScores[j]--;
                            _nonVertices.Add(temp);
                        }
                    }
                    //then, iterate through all col scores of 0
                    for (int j = 0; j < colScores.Length; j++)
                    {
                        if (colScores[j] == 0)
                        {
                            Point temp = new Point(2, new int[2] { i, j });

                            if (checkPointInList(vertices, _nonVertices, temp))
                                continue;

                            vertices.Add(temp);
                            rowScores[i]++;
                            colScores[j]++;

                            if (rowScores[i] == 2)
                                updateNV(ref _nonVertices, true, i);
                            if (colScores[i] == 2)
                                updateNV(ref _nonVertices, false, i);
                            
                            //move to next check, convex hull
                            if (CDD.convexHullVertexList(vertices))
                            {
                                if (vertices.Count == Globals.diameter * 2 + 1)
                                {
                                    result.Add(new Graph(vertices, new Dictionary<int, List<int>>()));
                                    vertices.Remove(temp);
                                    rowScores[i]--;
                                    colScores[j]--;
                                    return result;
                                }
                                else if (vertices.Count == Globals.diameter * 2)
                                    result.Add(new Graph(vertices, new Dictionary<int, List<int>>()));

                                //current point can be considered as a vertex
                                result.AddRange(generatePolytopesScore(vertices, core, _nonVertices, rowScores, colScores));
                            }

                            vertices.Remove(temp);
                            rowScores[i]--;
                            colScores[j]--;
                            _nonVertices.Add(temp);
                        }
                    }
                }
            }
            //Then iterate through all row scores of 0
            for (int i = 0; i < rowScores.Length; i++)
            {
                if (rowScores[i] == 0)
                {
                    for (int j = 0; j < colScores.Length; j++)
                    {
                        if (colScores[j] == 1)
                        {
                            Point temp = new Point(2, new int[2] { i, j });

                            if (checkPointInList(vertices, _nonVertices, temp))
                                continue;

                            vertices.Add(temp);
                            rowScores[i]++;
                            colScores[j]++;

                            //checks and updates 
                            if (rowScores[i] == 2)
                                updateNV(ref _nonVertices, true, i);
                            if (colScores[i] == 2)
                                updateNV(ref _nonVertices, false, i);

                            //move to next check, convex hull
                            if (CDD.convexHullVertexList(vertices))
                            {
                                if (vertices.Count == Globals.diameter * 2 + 1)
                                {
                                    result.Add(new Graph(vertices, new Dictionary<int, List<int>>()));
                                    vertices.Remove(temp);
                                    rowScores[i]--;
                                    colScores[j]--;
                                    return result;
                                }
                                else if (vertices.Count == Globals.diameter * 2)
                                    result.Add(new Graph(vertices, new Dictionary<int, List<int>>()));

                                //current point can be considered as a vertex
                                result.AddRange(generatePolytopesScore(vertices, core, _nonVertices, rowScores, colScores));
                            }

                            vertices.Remove(temp);
                            rowScores[i]--;
                            colScores[j]--;
                            _nonVertices.Add(temp);
                        }
                    }
                    for (int j = 0; j < colScores.Length; j++)
                    {
                        if (colScores[j] == 0)
                        {
                            Point temp = new Point(2, new int[2] { i, j });

                            if (checkPointInList(vertices, _nonVertices, temp))
                                continue;

                            vertices.Add(temp);
                            rowScores[i]++;
                            colScores[j]++;

                            //move to next check, convex hull
                            if (CDD.convexHullVertexList(vertices))
                            {
                                if (vertices.Count == Globals.diameter * 2 + 1)
                                {
                                    result.Add(new Graph(vertices, new Dictionary<int, List<int>>()));
                                    vertices.Remove(temp);
                                    rowScores[i]--;
                                    colScores[j]--;
                                    return result;
                                }
                                else if (vertices.Count == Globals.diameter * 2)
                                    result.Add(new Graph(vertices, new Dictionary<int, List<int>>()));

                                //current point can be considered as a vertex
                                result.AddRange(generatePolytopesScore(vertices, core, _nonVertices, rowScores, colScores));
                            }

                            vertices.Remove(temp);
                            rowScores[i]--;
                            colScores[j]--;
                            _nonVertices.Add(temp);
                        }
                    }
                }
            }

            return result;
        }

        //checks to see if Point p is in either list1 or list2
        public static bool checkPointInList(List<Point> list1, List<Point> list2, Point p)
        {
            foreach (Point q in list1)
            {
                if (p.Equals(q))
                    return true;
            }
            foreach (Point q in list2)
            {
                if (p.Equals(q))
                    return true; 
            }

            return false;
        }

        public static void updateNV(ref List<Point> nonVertices, bool row, int index)
        {
            bool _found;
            for (int i = 0; i < Globals.k; i++)
            {
                if(row)
                {
                    Point p = new Point(2, new int[2] { index, i });
                    _found = false;

                    foreach(Point q in nonVertices)
                    {
                        if(q.Equals(p))
                        {
                            _found = true;
                            break;
                        }
                    }

                    if (_found)
                    {
                        continue;
                    }

                    nonVertices.Add(p.clone());
                }
            }
        }*/
	}
}
