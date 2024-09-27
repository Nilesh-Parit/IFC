using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IfcPropExtract
{
    public class Polygon
    {
        int NoOfSides { get; set; }
        List<Point>? ordinates { get; set; }
        double Area { get; set; }
        double Perimeter { get; set; }

        public Polygon()
        {
            this.Input();
        }

        public void Input()
        {
            // Take input for no. of sides of polygon
            Console.WriteLine("Enter the no. of sides of polygon : ");
            this.NoOfSides = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine();

            // Take input of vertices of polygon as List<Point>
            Console.WriteLine("Now Enter the " + (NoOfSides + 1) + " no. of Vertices");
            this.ordinates = new List<Point>();

            for (int i = 1; i <= NoOfSides + 1; i++)
            {
                Point p = new Point();

                Console.WriteLine("Enter the x and y cordinate of vertex " + i);
                p.x = Convert.ToInt32(Console.ReadLine());
                p.y = Convert.ToInt32(Console.ReadLine());

                this.ordinates.Add(p);

                Console.WriteLine("(" + p.x + "," + p.y + ")");
            }
        }

        public bool checkforClosed(Point[] v)
        {
            if (v[0].x == v[v.Length - 1].x && v[0].y == v[v.Length - 1].y)
                return true;
            else
                return false;
        }

        public double CalculateArea()
        {
            Point[] vertices = this.ordinates.ToArray();
            double sum = 0;

            if (checkforClosed(vertices))
            {
                Console.WriteLine("Polygon is closed");
                for (int i = 0; i < vertices.Length - 1; i++)
                {
                    sum += ((vertices[i].x * vertices[i + 1].y) - (vertices[i].y * vertices[i + 1].x));
                }
                this.Area = 0.5 * sum;
                return this.Area;
            }
            else
            {
                Console.WriteLine("Polygon is not closed");
                return 0.00;
            }
        }

        public double CalculatePerimeter()
        {
            Point[] ver = this.ordinates.ToArray();
            double sum = 0;

            if (checkforClosed(ver))
            {
                Console.WriteLine("Polygon is closed");
                for (int i = 0; i < ver.Length - 1; i++)
                {
                    sum += Math.Sqrt(
                                     (Math.Pow((ver[i + 1].x - ver[i].x), 2))
                                   + (Math.Pow((ver[i + 1].y - ver[i].y), 2))
                                    );
                }

                this.Perimeter = sum;
                return this.Perimeter;
            }
            else
            {
                Console.WriteLine("Polygon is not closed");
                return 0.00;
            }
        }
    }
}
