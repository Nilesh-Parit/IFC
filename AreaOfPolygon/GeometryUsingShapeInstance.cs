using Xbim.Common.Geometry;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;

namespace AreaOfPolygon
{
    public class GeometryUsingShapeInstance
    {
        public static void GetGlobalCoordinates(Xbim3DModelContext context, IIfcBuildingElement element)
        {
            context.CreateContext();
            var shapeInstances = context.ShapeInstancesOf(element);  //to get wall typecasted ifcwallstandardcase is added in wall
            Console.WriteLine("Using Shape Instance :");

            foreach (var shape in shapeInstances)
            {
                var label = shape.ShapeGeometryLabel;  //create label
                if (label != 0)
                {
                    var geometry = context.ShapeGeometry(label) as XbimShapeGeometry;     //create geometry
                    if (geometry != null)
                    {
                        var vertices = geometry.Vertices;           //get vertices of geometry
                        if (vertices != null)
                        {
                            List<XbimPoint3D> points = new List<XbimPoint3D>();

                            // Get the transformation matrix for this element's placement
                            var transform = shape.Transformation; // This gives you the global transformation matrix
                           
                            foreach (var vertex in vertices)
                            {
                                // Convert local vertex to global vertex using the transformation matrix
                                var gvertex = transform.Transform(vertex);
                                Console.WriteLine($"Vertex: X={gvertex.X}, Y={gvertex.Y}, Z={gvertex.Z}");
                                points.Add(new XbimPoint3D(gvertex.X,gvertex.Y,gvertex.Z));
                            }
                            Console.WriteLine("Vertex count= " + points.Count);
                        }
                    }
                    else
                        Console.WriteLine("Could not found vertices for this geometry ");
                }
            }
        }
    }
}


   
