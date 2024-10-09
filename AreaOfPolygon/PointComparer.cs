using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common.Geometry;

namespace AreaOfPolygon
{
    public class PointComparer : IEqualityComparer<XbimPoint3D>
    {
        public bool Equals(XbimPoint3D p1, XbimPoint3D p2)
        {
            return Math.Abs(p1.X - p2.X) < 0.0001 &&
                   Math.Abs(p1.Y - p2.Y) < 0.0001 &&
                   Math.Abs(p1.Z - p2.Z) < 0.0001;
        }

        public int GetHashCode(XbimPoint3D p)
        {
            return p.X.GetHashCode() ^ p.Y.GetHashCode() ^ p.Z.GetHashCode();
        }
    }

}
