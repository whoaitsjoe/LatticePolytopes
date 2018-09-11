using System;
using System.Collections.Generic;
using System.IO;

namespace UpdatedRP
{
    public class PostShelling
    {
        public static void postShellingProcess(List<Graph> shellings)
        {
            List<Graph> validShellings = new List<Graph>();
            foreach(Graph g in shellings)
            {
                if (g.diameter() >= Globals.maxLength)
                    validShellings.Add(g);
            }

            Console.WriteLine("Shellings with sufficient diameter: {0}", validShellings.Count);

            List<Graph> uniqueShellings = symmetryGroup(validShellings);
            List<Graph> uniquePolytopes = checkInterior(uniqueShellings);
            List<Graph> finalPolytopes = retractable(uniquePolytopes);
        }

        public static List<Graph> symmetryGroup(List<Graph> graphs)
        {
            List<Graph> uniqueGraphs;

            uniqueGraphs = Shell.symmetryGroup(graphs, false);

            Console.WriteLine("Unique Shellings: " + uniqueGraphs.Count);

			if (Globals.writeToFile)
                Parse.writeToFile(uniqueGraphs, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
							  + Globals.gap.ToString() + "/_uniqueShellings");

            return uniqueGraphs;
        }

        public static List<Graph> checkInterior(List<Graph> uniqueShellings)
        {
            List<Graph> uniquePolytopes;
            List<Graph> polytopes = Shell.checkInterior(uniqueShellings);
			uniquePolytopes = Shell.symmetryGroup(polytopes, true);

            Console.WriteLine("Unique Shellings (in checkInterior): " + uniqueShellings.Count);
			Console.WriteLine("All valid polytopes (after interior point check): " + polytopes.Count); 

            if (Globals.writeToFile)
				Parse.writeToFile(polytopes, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
							  + Globals.gap.ToString() + "/_allPolytopes");

			Console.WriteLine("Unique polytopes: " + uniquePolytopes.Count);

			if (Globals.writeToFile)
				Parse.writeToFile(polytopes, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
							  + Globals.gap.ToString() + "/_uniquePolytopes");

            return uniquePolytopes;
        }

        public static List<Graph> retractable(List<Graph> uniquePolytopes)
        {
            List<Graph> finalPolytopes;
			List<Graph> retractablePolytopes = Shell.retractable(uniquePolytopes);
            finalPolytopes = Shell.symmetryGroup(retractablePolytopes, false);

			Console.WriteLine("Retractable polytopes: " + uniquePolytopes.Count);

			if (Globals.writeToFile)
				Parse.writeToFile(retractablePolytopes, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
							  + Globals.gap.ToString() + "/_retractablePolytopes");

			Console.WriteLine("Final polytopes: " + uniquePolytopes.Count);

			if (Globals.writeToFile)
				Parse.writeToFile(finalPolytopes, Globals.directory + Globals.d.ToString() + Globals.k.ToString()
							  + Globals.gap.ToString() + "/_finalPolytopes");

            return finalPolytopes;
        }
    }
}
