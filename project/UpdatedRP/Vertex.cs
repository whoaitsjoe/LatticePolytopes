using System;
using MIConvexHull;

namespace UpdatedRP.Properties
{
    /// <summary>
    /// A vertex is a simple class that stores the postion of a point, node or vertex.
    /// </summary>
    public class Vertex : IVertex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Vertex"/> class.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        public Vertex(double x, double y)
        {
            Position = new double[2] { x, y };
        }

        public double[] Position { get; set; }
    }
}
