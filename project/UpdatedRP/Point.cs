using System;
using System.Collections.Generic;

//Used to store a point in d dimension and associated state
namespace UpdatedRP
{
    public class Point : IEquatable<Point>
    {
		//Variables
		//---------
		string coordinates;

		//Constructors
		//------------
		public Point()
		{
            coordinates = "";
		}
		public Point(string coord)
		{
            coordinates = coord;
		}
        public Point(int[] input)
        {
            coordinates = string.Join("",input);
        }

		public string Coordinates
		{
			get
			{
                return coordinates;
			}
			set
			{
                coordinates = value;
			}
		}

		public int getDimension()
		{
			return coordinates.Length;
		}


        //Functions
		//---------
        public override string ToString()
		{
            return coordinates;
		}

        //todo -- test
		public bool Equals(Point p)
		{
            return coordinates.Equals(p.Coordinates, StringComparison.Ordinal);
		}

        public bool Equals(string s)
        {
            return coordinates.Equals(s, StringComparison.Ordinal);
        }

        //todo -- test
		public Point clone()
		{			
			return new Point(coordinates);
		}

        //todo -- test
        public bool shareFacet(Point p)
        {
            if (p.Coordinates.Length != coordinates.Length)
                return false;
            
            for (int i = 0; i < coordinates.Length; i++)
            {
                if (p.Coordinates[i] == 0 && coordinates[i] == 0 || p.Coordinates[i] == Globals.k && coordinates[i] == Globals.k)
                    return true;
            }

            return false;
        }

        //todo -- test
        //increase current point by 1 dimension, where dim is the dimension, and fZero specifies whether a 0 or k value is added.
		public void increaseDimensionality(int dim, bool fZero)
		{
            string temp;

            temp = coordinates.Substring(0, dim);
            temp += (fZero) ? ("0") : Globals.k.ToString();
            temp += coordinates.Substring(dim);

            coordinates = temp;
		}
		public void increaseDimensionality(int nextFacet)
		{
			string temp;

			temp = coordinates.Substring(0, nextFacet/2);
			temp += (nextFacet % 2 == 0) ? ("0") : Globals.k.ToString();
			temp += coordinates.Substring(nextFacet/2);

			coordinates = temp;
		}
		public static string increaseDimensionality(string coords, int dim, bool fZero)
		{
			string temp;

			temp = coords.Substring(0, dim);
			temp += (fZero) ? ("0") : Globals.k.ToString();
			temp += coords.Substring(dim);

            return temp;
		}

        //todo -- test
        //returns projection of point onto plane in 1 lower dimension (defined by input parameter)
        public Point decreaseDimensionality(int dim)
        {
            string temp;
             
            temp = coordinates.Substring(0, dim);
            temp += coordinates.Substring(dim + 1);

            return new Point(temp);
        }

        public static string incrementPoint(string s, bool symmetryCheck)
        {
            char[] temp = s.ToCharArray();

            for (int i = temp.Length - 1; i >= 0; i--)
            {
                if (Convert.ToInt16(temp[i].ToString()) < ((symmetryCheck) ? (Globals.k / 2) : (Globals.k)))
                {
                    temp[i] = (char)(Convert.ToInt16(temp[i]) + 1);
                    break;
                }
                else
                    temp[i] = '0';
            }

            return new string(temp);
        }

        public int[] getIntArray()
        {
            return new List<char>(coordinates.ToCharArray()).ConvertAll(c => Convert.ToInt32(c.ToString())).ToArray();
        }

        public string getLexicographicalString()
        {
            char[] c = coordinates.ToCharArray();

            Array.Sort(c);

            return new string(c);
        }

        //todo -- change data type to long or double to accomodate larger d/k values.
        public int getIntRepresentation()
        {
            return Convert.ToInt32(coordinates);
        }

        public static string convertIntArrayToString(int[] arr)
        {
			return string.Join("", arr);
        }
    }
}
