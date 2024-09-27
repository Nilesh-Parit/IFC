using System;
using System.Linq;
using Xbim.Common.Geometry;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;
using System.Collections.Generic;
using System.Configuration;

/*
 * This code gives the vertices of all the building elements
 * without using shape instance or shape geometry
 */

namespace IfcPropExtract
{
    public class NoShapeInstance1
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

                // Get the geometry representations directly from the element
                if (element.Representation != null)
                {
                    foreach (var representation in element.Representation.Representations)
                    {
                        // Process each representation item
                        foreach (var item in representation.Items)
                        {
                            // Only process IfcFacetedBrep or IfcPolygonalFaceSet geometries
                            if (item is IIfcFacetedBrep brep)
                            {
                                ExtractVerticesFromBrep(brep);
                            }
                            else if (item is IIfcPolygonalFaceSet faceSet)
                            {
                                ExtractVerticesFromFaceSet(faceSet);
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No geometric representation found for the element.");
                }
            }
        }

        private static void ExtractVerticesFromBrep(IIfcFacetedBrep brep)
        {
            // Extract vertices from IfcFacetedBrep
            var vertices = new List<XbimPoint3D>();

            foreach (var face in brep.Outer.CfsFaces)
            {
                foreach (var bound in face.Bounds)
                {
                    if (bound is IIfcPolyLoop loop)
                    {
                        foreach (var coord in loop.Polygon)
                        {
                            var point = new XbimPoint3D(coord.X, coord.Y, coord.Z);
                            vertices.Add(point);
                            Console.WriteLine($"Vertex: X={point.X:F5}, Y={point.Y:F5}, Z={point.Z:F5}");
                        }
                    }
                }
            }

            Console.WriteLine($"Total {vertices.Count} vertices found in IfcFacetedBrep.");
        }

        private static void ExtractVerticesFromFaceSet(IIfcPolygonalFaceSet faceSet)
        {
            // Extract vertices from IfcPolygonalFaceSet
            var vertices = new List<XbimPoint3D>();

            foreach (var coord in faceSet.Coordinates.CoordList)
            {
                var point = new XbimPoint3D(coord[0], coord[1], coord[2]);
                vertices.Add(point);
                Console.WriteLine($"Vertex: X={point.X:F5}, Y={point.Y:F5}, Z={point.Z:F5}");
            }

            Console.WriteLine($"Total {vertices.Count} vertices found in IfcPolygonalFaceSet.");
        }
    }
}
