using System;
using System.Collections.Generic;
using System.Linq;

namespace UpdatedRP
{
	public class ShortestPath
	{
		//BFS where u and v must be in g, return -1 if no path exists.
		public static int BFS(Graph g, Point u, Point v)
		{
			if (!g.Points.Contains(u) || !g.Points.Contains(v))
				return -1;

			Queue<string> queue = new Queue<string>();
			Dictionary<string, int> visited = new Dictionary<string, int>();
            string curr;

            queue.Enqueue(u.Coordinates);
            visited.Add(u.Coordinates, 0);

            while(queue.Count > 0)
            {
                curr = queue.Dequeue();
                foreach (string x in g.AdjList[curr])
                {
                    if (x.Equals(v.Coordinates))
                        return visited[curr] + 1;
                    else if(!visited.ContainsKey(x))
                    {
                        visited.Add(x, visited[curr] + 1);
                        queue.Enqueue(x);
                    }
                }
            }

			return -1;
		}

		//BFS to find largest distance to any point from point u
		public static int BFS(Graph g, Point u)
		{
			if (!g.Points.Contains(u))
				return -1;

			Queue<string> queue = new Queue<string>();
			Dictionary<string, int> visited = new Dictionary<string, int>();
			string curr;

			queue.Enqueue(u.Coordinates);
			visited.Add(u.Coordinates, 0);

			while (queue.Count > 0)
			{
				curr = queue.Dequeue();
				foreach (string x in g.AdjList[curr])
				{
                    if (!visited.ContainsKey(x))
					{
						visited.Add(x, visited[curr] + 1);
						queue.Enqueue(x);
					}
				}
			}

            return visited.Values.Max();
		}

        //BFS from point u to intersection with hypercube, denoted by int, where 0 = x1=0, 1 = x1=k, 2 = x2=0, etc.
		public static int BFStoFacet(Graph g, Point u, int facet)
		{
			if (!g.Points.Contains(u))
				return -1;

			Queue<string> queue = new Queue<string>();
			Dictionary<string, int> visited = new Dictionary<string, int>();
			string curr;

			queue.Enqueue(u.Coordinates);
			visited.Add(u.Coordinates, 0);

			while (queue.Count > 0)
			{
				curr = queue.Dequeue();

                if (containsFacet(new Point(curr), facet))
                    return visited[curr];
                
				foreach (string x in g.AdjList[curr])
				{
					if (!visited.ContainsKey(x))
					{
						visited.Add(x, visited[curr] + 1);
						queue.Enqueue(x);
					}
				}
			}

			return Globals.k + 1;
		}

		public static int BFStoFacetEstimator(Graph g, Point u, int facet)
		{
            if (!g.Points.Contains(u))
                //return (facet % 2 == 0) ? Convert.ToInt32(u.Coordinates[facet / 2].ToString()) : Globals.k - Convert.ToInt32(u.Coordinates[facet / 2].ToString());
                return Globals.k;

			Queue<string> queue = new Queue<string>();
			Dictionary<string, int> visited = new Dictionary<string, int>();
			string curr;
            int min = Globals.k;

			queue.Enqueue(u.Coordinates);
			visited.Add(u.Coordinates, 0);

			while (queue.Count > 0)
			{
				curr = queue.Dequeue();

				//distance from u to point + coordinate of interested facet of point (if going towards 0) or k - coordinate of interested facet if going towards k
				int temp = visited[curr] +
                    ((facet % 2 == 0) ? Convert.ToInt32(curr[facet / 2].ToString()) : (Globals.k - Convert.ToInt32(curr[facet / 2].ToString())));
				
                if (temp < min)
					min = temp;

				foreach (string x in g.AdjList[curr])
				{
					if (!visited.ContainsKey(x))
					{
						visited.Add(x, visited[curr] + 1);
						queue.Enqueue(x);
					}
				}
			}

			return min;
		}

		public static bool containsFacet(Point p, int facet)
		{
			if (facet % 2 == 0)
			{
                if (Convert.ToInt32(p.Coordinates[facet / 2].ToString()) == 0)
					return true;
			}
			else
			{
				if (Convert.ToInt32(p.Coordinates[facet / 2].ToString()) == Globals.k)
					return true;
			}
			return false;
		}
	}
}
