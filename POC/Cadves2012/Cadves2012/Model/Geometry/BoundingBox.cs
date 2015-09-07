using System;
using System.Text;

namespace Cadves2012.Model.Geometry
{
    public class BoundingBox
    {
        #region Private Members

        private const int DIMENSIONS = 3;

        // Array containing the minimum value for each dimension; ie { min(x), min(y) }
        private double[] max;

        // Array containing the maximum value for each dimension; ie { max(x), max(y) }
        private double[] min;
        
        #endregion

        public double Xmin { get { return min[0]; } }
        public double Ymin { get { return min[1]; } }
        public double Zmin { get { return min[2]; } }

        public double Xmax { get { return max[0]; } }
        public double Ymax { get { return max[1]; } }
        public double Zmax { get { return max[2]; } }

        public BoundingBox(double x1, double y1, double x2, double y2, double z1, double z2)
        {
            this.min = new double[DIMENSIONS];
            this.max = new double[DIMENSIONS];
            SetBoundingBox(x1, y1, x2, y2, z1, z2);
        }

        public BoundingBox(double[] min, double[] max)
        {
            if (min.Length != DIMENSIONS || max.Length != DIMENSIONS)
            {
                throw new Exception("Error in Rectangle constructor: " +
                          "min and max arrays must be of length " + DIMENSIONS);
            }

            this.min = new double[DIMENSIONS];
            this.max = new double[DIMENSIONS];

            SetBoundingBox(min, max);
        }

        private void SetBoundingBox(double x1, double y1, double x2, double y2, double z1, double z2)
        {
            min[0] = Math.Min(x1, x2);
            min[1] = Math.Min(y1, y2);
            min[2] = Math.Min(z1, z2);
            max[0] = Math.Max(x1, x2);
            max[1] = Math.Max(y1, y2);
            max[2] = Math.Max(z1, z2);
        }

        private void SetBoundingBox(double[] min, double[] max)
        {
            Array.Copy(min, 0, this.min, 0, DIMENSIONS);
            Array.Copy(max, 0, this.max, 0, DIMENSIONS);
        }

        public BoundingBox Copy()
        {
            return new BoundingBox(min, max);
        }
    
        /// <summary>
        /// Determine whether an edge of this rectangle overlies the equivalent edge of the passed rectangle
        /// </summary>
        /// <param name="r">Rectangle</param>
        /// <returns></returns>
        public bool EdgeOverlaps(BoundingBox r)
        {
            for (int i = 0; i < DIMENSIONS; i++)
            {
                if (min[i] == r.min[i] || max[i] == r.max[i])
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determine whether this rectangle intersects the passed rectangle
        /// </summary>
        /// <param name="r">The rectangle that might intersect this rectangle</param>
        /// <returns>True if the rectangles intersect, false if they do not intersect</returns>
        public bool Intersects(BoundingBox r)
        {
            // Every dimension must intersect. If any dimension does not intersect, return false immediately.
            for (int i = 0; i < DIMENSIONS; i++)
            {
                if (max[i] < r.min[i] || min[i] > r.max[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Determine whether this rectangle contains the passed rectangle
        /// </summary>
        /// <param name="r">The rectangle that might be contained by this rectangle</param>
        /// <returns>If this rectangle contains the passed rectangle, false if it does not</returns>
        public bool Contains(BoundingBox r)
        {
            for (int i = 0; i < DIMENSIONS; i++)
            {
                if (max[i] < r.max[i] || min[i] > r.min[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Determine whether this rectangle is contained by the passed rectangle
        /// </summary>
        /// <param name="r">The rectangle that might contain this rectangle</param>
        /// <returns>If the passed rectangle contains this rectangle, false if it does not</returns>
        public bool ContainedBy(BoundingBox r)
        {
            for (int i = 0; i < DIMENSIONS; i++)
            {
                if (max[i] > r.max[i] || min[i] < r.min[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Return the distance between this rectangle and the passed point. If the rectangle contains the point, the distance is zero.
        /// </summary>
        /// <param name="x">The X-coord of point to find the distance to</param>
        /// <param name="y">The Y-coord of point to find the distance to</param>
        /// <param name="z">The Z-coord of point to find the distance to</param>
        /// <returns>Distance beween this rectangle and the passed point</returns>
        public double Distance(double x, double y, double z)
        {
            double distanceSquared = 0;

            for (int i = 0; i < DIMENSIONS; i++)
            {
                double coord = 0.0d;

                if (i == 0) coord = x;
                if (i == 1) coord = y;
                if (i == 2) coord = z;

                double greatestMin = Math.Max(min[i], coord);
                double leastMax = Math.Min(max[i], coord);

                if (greatestMin > leastMax)
                {
                    distanceSquared += ((greatestMin - leastMax) * (greatestMin - leastMax));
                }
            }
            return Math.Sqrt(distanceSquared);
        }

        /// <summary>
        /// Return the distance between this rectangle and the passed rectangle.  If the rectangles overlap, the distance is zero.
        /// </summary>
        /// <param name="r">Rectangle to find the distance to</param>
        /// <returns>Distance between this rectangle and the passed rectangle</returns>
        public double Distance(BoundingBox r)
        {
            double distanceSquared = 0;

            for (int i = 0; i < DIMENSIONS; i++)
            {
                double greatestMin = Math.Max(min[i], r.min[i]);
                double leastMax = Math.Min(max[i], r.max[i]);

                if (greatestMin > leastMax)
                {
                    distanceSquared += ((greatestMin - leastMax) * (greatestMin - leastMax));
                }
            }

            return Math.Sqrt(distanceSquared);
        }

        /// <summary>
        /// Return the squared distance from this rectangle to the passed point
        /// </summary>
        /// <param name="dimension"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public double DistanceSquared(int dimension, double point)
        {
            double distanceSquared = 0;
            double tempDistance = point - max[dimension];

            for (int i = 0; i < 2; i++)
            {
                if (tempDistance > 0)
                {
                    distanceSquared = (tempDistance * tempDistance);
                    break;
                }
                tempDistance = min[dimension] - point;
            }
            return distanceSquared;
        }

        /// <summary>
        /// Find the distance between this rectangle and each corner of the passed rectangle, and use the maximum.
        /// </summary>
        /// <param name="r">Rectangle to find the distance to</param>
        /// <returns>Return the furthst possible distance between this rectangle and the passed rectangle.</returns>
        public double FurthestDistance(BoundingBox r)
        {
            double distanceSquared = 0;

            for (int i = 0; i < DIMENSIONS; i++)
            {
                distanceSquared += Math.Max(r.min[i], r.max[i]);

                //distanceSquared += Math.Max(distanceSquared(i, r.min[i]), distanceSquared(i, r.max[i]));
            }

            return Math.Sqrt(distanceSquared);
        }

        /// <summary>
        /// Calculate the area by which this rectangle would be enlarged if added to the passed rectangle. Neither rectangle is altered.
        /// </summary>
        /// <param name="r">Rectangle to union with this rectangle, in order to compute the difference in area of the union and the original rectangle.</param>
        /// <returns>Enlargement</returns>
        public double Enlargement(BoundingBox r)
        {
            double enlargedArea = (Math.Max(max[0], r.max[0]) - Math.Min(min[0], r.min[0])) *
                                  (Math.Max(max[1], r.max[1]) - Math.Min(min[1], r.min[1]));

            return enlargedArea - Area();
        }

        /// <summary>
        /// Compute the area of this rectangle.
        /// </summary>
        /// <returns>The area of this rectangle</returns>
        public double Area()
        {
            return (max[0] - min[0]) * (max[1] - min[1]);
        }

        /// <summary>
        /// Computes the union of this rectangle and the passed rectangle, storing the result in this rectangle.
        /// </summary>
        /// <param name="r">Rectangle to add to this rectangle</param>
        public void Add(BoundingBox r)
        {
            for (int i = 0; i < DIMENSIONS; i++)
            {
                if (r.min[i] < min[i])
                {
                    min[i] = r.min[i];
                }
                if (r.max[i] > max[i])
                {
                    max[i] = r.max[i];
                }
            }
        }

        /// <summary>
        /// Find the the union of this rectangle and the passed rectangle. Neither rectangle is altered.
        /// </summary>
        /// <param name="r">The rectangle to union with this rectangle</param>
        /// <returns>Union rectangle</returns>
        public BoundingBox Union(BoundingBox r)
        {
            BoundingBox union = this.Copy();
            union.Add(r);
            return union;
        }

        private bool CompareArrays(double[] a1, double[] a2)
        {
            if ((a1 == null) || (a2 == null)) return false;

            if (a1.Length != a2.Length) return false;

            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i])
                    return false;
            
            return true;
        }

        /// <summary>
        /// Determine whether this rectangle is the same as another object. Note that two rectangles can be equal but not the same object, if they both have the same bounds.
        /// </summary>
        /// <param name="obj">The object to compare with this rectangle.</param>
        /// <returns></returns>
        public bool SameObject(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Determine whether this rectangle is equal to a given object. Equality is determined by the bounds of the rectangle.
        /// </summary>
        /// <param name="obj">The object to compare with this rectangle</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            bool equals = false;

            if (obj.GetType() == typeof(BoundingBox))
            {
                BoundingBox r = (BoundingBox)obj;

                if (CompareArrays(r.min, min) && CompareArrays(r.max, max))
                {
                    equals = true;
                }
            }
            return equals;
        }

        public override int GetHashCode()
        {
            int hashcode = 0;

            for (int i = 0; i < DIMENSIONS; i++)
            {
                hashcode ^= max[i].GetHashCode() ^ min[i].GetHashCode();
            }

            return hashcode ^ DIMENSIONS.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            // min coordinates
            sb.Append('(');
            for (int i = 0; i < DIMENSIONS; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(min[i]);
            }
            sb.Append("), (");

            // max coordinates
            for (int i = 0; i < DIMENSIONS; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(max[i]);
            }
            sb.Append(')');
            return sb.ToString();
        }
    }
}
