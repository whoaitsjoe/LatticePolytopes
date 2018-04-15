using System;
using System.Collections.Generic;

namespace UpdatedRP
{
    public class GenerateUV
    {
        public static List<Point> generateU()
        {
            List<Point> result = new List<Point>();
            List<Point> vertexSet = FileIO.readVertexList();

            string terminator = "", curr = "";

            for (int i = 0; i < Globals.d; i++)
            {
                curr += "0";
                terminator += (Globals.k / 2).ToString();
            }

            while (true)
            {
                if (vertexSet.Count == 0 || containsPoint(vertexSet, curr))
                {
                    if(!symmetryCheck(curr))
                        result.Add(new Point(curr));
                }

                if (curr.Equals(terminator, StringComparison.Ordinal))
                    break;
                
                curr = Point.incrementPoint(curr, true);
            }

            return result;
        }

        //returns true if point u is symmetric (i.e. an element at [i] is larger than [i+1]. e.g. 100 returns true since 001 should have been checked already)
        private static bool symmetryCheck(string u)
        {
            char[] temp = u.ToCharArray();

            for (int i = 0; i < temp.Length - 1; i++)
            {
                if (temp[i].CompareTo(temp[i+1]) > 0)
                    return true;
            }

            return false;
        }
		private static bool symmetryCheck(string u, string v)
		{
			char[] utemp = u.ToCharArray();
            char[] vtemp = v.ToCharArray();

            for (int i = 0; i < utemp.Length - 1; i++)
            {
                if(utemp[i].CompareTo(utemp[i+1]) == 0)
                {
                    if (vtemp[i].CompareTo(vtemp[i + 1]) > 0)
                        return true;
                }
            }

			return false;
		}

        public static bool containsPoint(List<Point> validPoints, Point p)
        {
            foreach(Point q in validPoints)
            {
                if (q.Equals(p))
                    return true;
            }
            return false;
        }

		public static bool containsPoint(List<Point> validPoints, string s)
		{
			foreach (Point p in validPoints)
			{
				if (p.Equals(s))
					return true;
			}
			return false;
		}

        public static List<Point> generateV(Point u)
        {
            List<Point> result = new List<Point>();
            Point new_v;
            int[] g0 = new int[Globals.d];
            bool uRedundant, vRedundant, terminateFlag;

            int[] uPoints = u.getIntArray();
            int[] vPoints = new int[uPoints.Length];

            //set first point v to check
            for (int i = 0; i < uPoints.Length; i++)
            {
                vPoints[i] = Math.Max(Globals.k - uPoints[i] - Globals.gap, 0);
            }

            while (true)
            {
                new_v = new Point(vPoints);

                //check same point
                if (u.Equals(new_v))
                {
                    if (Globals.messageOn)
                        Console.WriteLine(new_v.ToString() + ": Eliminated. Same point as u.");
                }
                //inverse check for (k - u)
                else if (checkInverse(u, new_v))
                {
                    if(Globals.messageOn)
                        Console.WriteLine(new_v.ToString() + ": Eliminated. Inverse already checked (k - u).");
                }
				//inverse check for v in lexicographical order
				else if (checkLexicographical(u, new_v))
				{
					if (Globals.messageOn)
                        Console.WriteLine(new_v.ToString() + ": Eliminated. Inverse already checked (lexico v).");
				}
				else
				{
                    //generate gi 
                    for (int i = 0; i < Globals.d; i++)
                    {
                        g0[i] = Globals.gap + u.getIntArray()[i] + vPoints[i] - Globals.k;
                    }

					//check vStar, given gap i, check that both u and v exist within the vertex set of the corresponding facets.
					if (!checkVertexSet(u, new_v, g0, Globals.d - 1, Globals.k))  
					{
						if (Globals.messageOn)
							Console.WriteLine(new_v.ToString() + ": Eliminated.  Point does not belong to vertex set.");
					}

                    //check convex core
                    else if (checkConvexCore(u, new_v, g0, out uRedundant, out vRedundant))
                    {
						result.Add(new_v);

						if (Globals.messageOn)
							Console.WriteLine("u: " + u + ". v: " + new_v);
                    }
                    else
					{
                        if (Globals.messageOn)
                        {
                            Console.Write("u: " + u + ". v: ");
                            foreach (int i in vPoints)
                                Console.Write(i + " ");

                            Console.WriteLine(" eliminated, " + (uRedundant && vRedundant ? "u and v are not vertices " :
                                               (uRedundant ? "u is not a vertex " : "v is not a vertex ")) +
                                              "of the convex core.");
                        }
                    }
				}

                do
                {
                    incrementPoint(u.getIntArray(), vPoints, out terminateFlag);
                }
                while (symmetryCheck(u.Coordinates, Point.convertIntArrayToString(vPoints)) && !terminateFlag);

                //termination condition check
                if (terminateFlag)
                    break;
            }

            return result;
        }

        //return true if current coord of v has not reached the max value it can be. v[i] is valid if v[i] < MIN(k, k - u[i] + g)
        private static bool checkValidCoord(int u, int v)
        {
            return v <= Math.Min(Globals.k, Globals.k - u + Globals.gap);
        }

        //returns true if inverse has been checked (i.e. u,v is not a valid pair).
        private static bool checkInverse(Point u, Point v)
        {
            int[] vCoords = v.getIntArray();

            //checks to see if any element of v is less than half of k/2
            for (int i = 0; i < Globals.d; i++)
            {
                if (vCoords[i] < Math.Ceiling((double)Globals.k / 2))
                    return false;
            }

            //checks to see if v inverse ordered lexicographically is smaller than u
            string vInverseString = "";
            int[] tempVal = new int[Globals.d];

            for (int i = 0; i < Globals.d; i++)
            {
                tempVal[i] = Globals.k - vCoords[i];
            }

            Array.Sort(tempVal);

			for (int i = 0; i < Globals.d; i++)
			{
                vInverseString += tempVal[i].ToString();
			}

            if (Convert.ToInt16(vInverseString) >= u.getIntRepresentation())
                return false;

            return true;
        }

		//returns true if inverse has been checked (i.e. u,v is not a valid pair).
		private static bool checkLexicographical(Point u, Point v)
        {
			int[] vCoords = v.getIntArray();

			//checks to see if any element of v is less than half of k/2
			for (int i = 0; i < Globals.d; i++)
			{
				if (vCoords[i] < Math.Ceiling((double)Globals.k / 2))
					return false;
			}

            string vString = v.getLexicographicalString();

            if (Convert.ToInt16(vString) >= u.getIntRepresentation())
                return false;
            
            return true;
        }

        //returns true if point is in vertex set
        private static bool checkVertexSet(Point u, Point v, int[] gap0, int d, int k)
        {
            if (!Globals.checkVStar)
                return true;

            string index;
            bool found;

            for (int i = 0; i < Globals.d; i++)
            {
                //check that point is in intersection with hypercube
                if (v.getIntArray()[i] != Globals.k && v.getIntArray()[i] != 0)
                    continue;
                
                index = d.ToString() + k.ToString() + (2*Globals.gap - gap0[i]).ToString();

                //check for existence of vertex set
                if (!Globals.vertexSet.ContainsKey(index) || Globals.vertexSet[index].Count == 0)
					continue;

				Point q = v.decreaseDimensionality(i);
				found = false;
                
				foreach(Point p in Globals.vertexSet[index])
                {
                    if(p.Equals(q))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    return false;
            }
			for (int i = 0; i < Globals.d; i++)
			{
                //check that point is in intersection with hypercube
				if (u.getIntArray()[i] != Globals.k && u.getIntArray()[i] != 0)
					continue;
                
				index = d.ToString() + k.ToString() + gap0[i].ToString();

				//check for existence of vertex set
				if (!Globals.vertexSet.ContainsKey(index) || Globals.vertexSet[index].Count == 0)
					continue;
                
				Point q = u.decreaseDimensionality(i);
				found = false;
                
				foreach (Point p in Globals.vertexSet[index])
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

        //returns true if points are vertices of the convex core
        private static bool checkConvexCore(Point u, Point v, int[] gap, out bool uRedundant, out bool vRedundant)
        {
            List<Point> corePoints = new List<Point>();
            string index0, indexk;
            Point q;

            corePoints.Add(u);
            corePoints.Add(v);

            for (int i = 0; i < Globals.d; i++)
            {
                index0 = (Globals.d-1).ToString() + Globals.k.ToString() + gap[i].ToString();
                indexk = (Globals.d-1).ToString() + Globals.k.ToString() + (((2*Globals.gap - gap[i]) > Globals.gap) ? (Globals.gap) : (2 * Globals.gap - gap[i])).ToString();

                //List<Point> corePoints0 = FileIO.readPointsFromFile(Globals.directory + "/" + index0 + "/coreSet");
                //List<Point> corePointsk = FileIO.readPointsFromFile(Globals.directory + "/" + indexk + "/coreSet");

                List<Point> corePoints0 = (Globals.coreSet.ContainsKey(index0)) ? Globals.coreSet[index0] : new List<Point>();
				List<Point> corePointsk = (Globals.coreSet.ContainsKey(indexk)) ? Globals.coreSet[indexk] : new List<Point>();

                foreach(Point p in corePoints0)
                {
                    q = p.clone();
                    q.increaseDimensionality(i, true);
                    corePoints.Add(q);
                }
				foreach (Point p in corePointsk)
				{
                    q = p.clone();
					q.increaseDimensionality(i, false);
					corePoints.Add(q);
				}
            }

            return CDD.convexHullVertex(corePoints, u, v, out uRedundant, out vRedundant);
        }

        //find next valid point. If no valid points, then sets terminateFlag to true;
        public static void incrementPoint(int[] u, int[] points, out bool terminateFlag)
		{
			terminateFlag = true;

            bool changeFlag = false;
            int i = points.Length - 1;

            while (!changeFlag)
            {
                if (i < 0)
                    break;
                
                points[i]++;

                //checks for validity of coords and symmetry
                if(checkValidCoord(u[i], points[i]))
				{
					changeFlag = true;
					terminateFlag = false;
                }
                else
                {
                    points[i] = Math.Max(0, Globals.k - u[i] - Globals.gap);

                    i--;
                }
            }
        }
	}
}
