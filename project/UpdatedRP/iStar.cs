using System;
using System.Collections.Generic;
using System.Linq;

namespace UpdatedRP
{
    public class iStar
    {
        List<Point> points;

        public iStar()
        {
        }
        public iStar(List<Graph> shapes)
        {
            List<Point> candidates = findIStarCandidates(shapes);
        }

		public List<Point> Points
		{
			get
			{
				return points;
			}
		} 

        //shapes argument is the set fStar (e.g. all facets achieving ∂(d,k).
        public static List<Point> findIStarCandidates(List<Graph> shapes)
        {
            Dictionary<Point, int> iStar = new Dictionary<Point, int>();

            foreach(Point p in shapes[0].getAllContainedPoints())
            {
                iStar.Add(p, 1);
            }

            foreach(Graph shape in shapes)
            {
                foreach(Point p in iStar.Keys.ToArray())
                {
                    if (!shape.getAllContainedPoints().Contains(p))
                        iStar.Remove(p);
                }

                if (iStar.Count() < 1)
                    return new List<Point>();
            }

            return iStar.Keys.ToList();
        }
    }
}
