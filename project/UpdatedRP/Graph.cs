using System;
using System.Collections.Generic;

//Used to store graphs and related functions.
namespace UpdatedRP
{
    public class Graph
    {
		//Variables
		//---------
		List<Point> points; //List of points in point format
        Dictionary<string, List<string>> adjList;

		//Constructors
		//------------
		public Graph()
        {
            points = new List<Point>();
            adjList = new Dictionary<string, List<string>>();
        }

        //todo -- test
        //constructor taking in adj list in integer format where key references point in list and value references neighbouring points. (1-based indexing assumed).
        public Graph(List<Point> pts, Dictionary<int, List<int>> aL)
        {
			points = new List<Point>();
			adjList = new Dictionary<string, List<string>>();
			
            foreach (Point p in pts)
				points.Add(p);
            
			foreach (KeyValuePair<int, List<int>> entry in aL)
            {
                List<string> temp = new List<string>();

                foreach(int i in entry.Value)
                {
                    temp.Add(points[i - 1].Coordinates);
                }

                adjList.Add(points[entry.Key - 1].ToString(), temp);
            }
        }

        public Graph(List<Point> pts, Dictionary<string, List<string>> aL)
        {
            points = pts;
            adjList = aL;
        }

		public List<Point> Points
		{
			get
			{
				return points;
			}
			set
			{
				points = value;
			}
		}
		public Dictionary<string, List<string>> AdjList
		{
			get
			{
				return adjList;
			}
			set
			{
				adjList = value;
			}
		}

		//Functions
		//---------
		public List<Point> getAllContainedPoints()
        {
            List<Point> result = new List<Point>();
            foreach (Point p in points)
                result.Add(p.clone());
            return result;
        }

        //todo -- check needs to be updated.
		public List<Point> getAllContainedPoints(int dim, int minMax)
		{
			List<Point> result = new List<Point>();
            foreach (Point p in points)
            {
                if(Convert.ToInt32(p.Coordinates[dim].ToString()) == ((minMax == 0) ? 0 : Globals.k))
                    result.Add(p.clone());
            }
			return result;
		}

        public List<Point> getAllContainedPoints(int nextFacet)
        {
			List<Point> result = new List<Point>();
			foreach (Point p in points)
			{
                if (Convert.ToInt32(p.Coordinates[nextFacet/2].ToString()) == ((nextFacet % 2 == 0) ? 0 : Globals.k))
					result.Add(p.clone());
			}
			return result;
        }

        public List<string> getAdjList(int index)
		{
            if (index < 1)
                return null;
            
            return adjList[points[index].Coordinates];
        }

        public int getPointsCount()
        {
            return points.Count;
        }

        //adds input parameter to current graph
        public void AddFacet(Graph g)
        {
            bool found;

            //copies points which are currently not in the points list
            foreach(Point p in g.Points)
            {
                found = false;

                foreach(Point q in points)
                {
                    if(p.Equals(q))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    points.Add(p.clone());
            }

            //copies over new entries in adjList
            foreach(KeyValuePair<string, List<string>> entry in g.AdjList)
            {
                //if entry exists for key (e.g. point was previously in graph and has edges)
                if(adjList.ContainsKey(entry.Key))
                {
                    foreach(string s in entry.Value)
                    {
                        found = false;

                        foreach(string t in adjList[entry.Key])
                        {
                            if(t.Equals(s, StringComparison.Ordinal))
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            adjList[entry.Key].Add(s);
                    }
                }
                else
                {
                    adjList.Add(entry.Key, entry.Value);
                }
            }
        }

        public override string ToString()
        {
            string result = "Points: \n";

            foreach (Point x in points)
                result += x + "\n";

            result += "Adjacency List: \n";

            foreach (KeyValuePair<string, List<string>> pair in adjList)
            {
                result += pair.Key + " : ";
                foreach (string i in pair.Value)
                    result += i + " ";
                result += "\n";
            }

            return result;
        }

		public Graph clone()
		{
			List<Point> pts = new List<Point>();
			Dictionary<string, List<string>> aL = new Dictionary<string, List<string>>();

			foreach (Point p in points)
				pts.Add(p.clone());

            foreach (KeyValuePair<string, List<string>> entry in adjList)
            {
                List<string> valueList = new List<string>();

                foreach (string s in entry.Value)
                    valueList.Add(s);
                
                aL.Add(entry.Key, valueList);
            }

			return new Graph(pts, aL);
		}

		public void addDimensionality(int dim, bool fZero)
		{
            //add dimension to all points
            foreach(Point p in points)
            {
                p.increaseDimensionality(dim, fZero);
            }

			//add dimension to all entries in adjList
            Dictionary<string, List<string>> newAL = new Dictionary<string, List<string>>();
			foreach (KeyValuePair<string, List<string>> entry in adjList)
			{
                string tempKey = Point.increaseDimensionality(entry.Key, dim, fZero);
                List<string> tempVal = new List<string>();

                foreach(string s in entry.Value)
                {
                    tempVal.Add(Point.increaseDimensionality(s, dim, fZero));
                }

                newAL.Add(tempKey, tempVal);
			}

            adjList = newAL;
		}
    }
}
