using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreaOfPolygon
{
    public class Points
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Points()
        {
            /*  List<Points> points = new List<Points>();
        // points.Insert({ (1, 2, 2),(2, 4, 2),(3, 4, 2),(4, 4, 2)(1, 2, 2)});
        points.Add(new Points() { X = 1, Y = 2, Z = 2 });
        points.Add(new Points() { X = 2, Y = 2, Z = 2 });
        points.Add(new Points() { X = 3, Y = 4, Z = 2 });
        points.Add(new Points() { X = 2, Y = 1, Z = 2 });
        points.Add(new Points() { X = 4, Y = 5, Z = 2 });
        points.Add(new Points() { X = 2, Y = 3, Z = 2 });
        points.Add(new Points() { X = 1, Y = 2, Z = 2 });*/
        }
        public Points(double x, double y, double z)
        {
            this.X = x; this.Y = y; this.Z = z;
        }
        public override string ToString()
        {
            return X+", "+Y+", "+Z;
        }

        public double AreaCalculation(List<Points> points)
        {
            int n = points.Count;
            double addPart = 0;
            double subpart = 0;
            for (int i = 0; i < n - 2; i++)
            {
                addPart += points[i].X * points[i + 1].Y;
                subpart += points[i + 1].X * points[i].Y;
            }
            //area = 0.5 * |(x1y2 + x2y3 + x3y1) - (x2y3 + x3y2 + x1y3)|
            double area = 0.5 * Math.Abs(addPart - subpart);
            return area;
        }

        public double PerimeterCalulation(List<Points> points)
        {
            int n = points.Count;
            double perimeter = 0;
            for (int i = 0; i < n - 2; i++)
            {
                double sideLength = Math.Sqrt(Math.Pow((points[i + 1].X - points[i].X), 2) +
                   Math.Pow((points[i + 1].Y - points[i].Y), 2));
                perimeter += sideLength;
            }
            return perimeter;
        }
    }
}
