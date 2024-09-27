using System;
using System.Linq;
using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;
using Xbim.Geometry.Engine.Interop;
using System.Collections.Generic;
using System.Configuration;
using Xbim.Ifc2x3;
using Xbim.Ifc2x3.Interfaces;
using System.Reflection.Emit;

/*
 * This code gives the vertices of all the building elements
 * using shape instance and shape geometry
 */

namespace IfcPropExtract
{
    public class ShapeProp1
    {
        public static void getShapeProp()
        {
            // Provide the path to the IFC file
            string? ifcFilePath = ConfigurationManager.AppSettings["IfcFilePath"];

            // Provide the GUID of the IFC element to extract (e.g., wall, column, or beam)
            string? elementGuid = ConfigurationManager.AppSettings["Guid"];

            // Open the IFC file
            using (var model = IfcStore.Open(ifcFilePath))
            {
                // Retrieve the element by its GUID
                var element = model.Instances.FirstOrDefault<IIfcProduct>(e => e.GlobalId == elementGuid);

                if (element == null)
                {
                    Console.WriteLine("Element with specified GUID not found.");
                    return;
                }

                // Create the context for geometry extraction
                var context = new Xbim3DModelContext(model);
                context.CreateContext();

                // Get the shape instances associated with the element
                var shapeInstances = context.ShapeInstancesOf(element);

                // Store vertices

                foreach (var shapeInstance in shapeInstances)
                {
                    var label = shapeInstance.ShapeGeometryLabel;  //create label

                    // Get the geometry of the shape instance
                    var shapeGeometry = context.ShapeGeometry(label) as XbimShapeGeometry;

                    if (shapeGeometry == null)
                        continue;

                    //get vertices of geometry
                    var vertices = shapeGeometry.Vertices;
                    if (vertices != null)
                    {
                        List<XbimPoint3D> points = new List<XbimPoint3D>();
                        foreach (var vertex in vertices)
                        {
                            points.Add(new XbimPoint3D(vertex.X, vertex.Y, vertex.Z ));
                            Console.WriteLine($"Vertex: X={vertex.X:F5},\tY={vertex.Y:F5},\tZ={vertex.Z:F5}");

                        }
                        int n = points.Count;
                        Console.WriteLine("Total "+n+" no. of coordinate found");

                    }
                }
            }
        }
    }
}
