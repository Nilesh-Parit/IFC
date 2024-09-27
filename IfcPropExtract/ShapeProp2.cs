using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;

/*
 * This code gives the vertices of all the building elements
 * without using shape instance or shape geometry
 * There are some bugs 
 */

namespace IfcPropExtract
{
    public class ShapeProp2
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

                // Create a basic context for geometric information
                var context = new Xbim3DModelContext(model);
                context.CreateContext();  // This is needed to load the geometric data

                // Output some basic information about the product
                Console.WriteLine($"\nProduct: {element.Name}");
                Console.WriteLine($"Product Type: {element.GetType().Name}");
                Console.WriteLine($"Product Global ID: {element.GlobalId}\n");

                // Extract spatial information (Location)
                var placement = element.ObjectPlacement as IIfcLocalPlacement;
                if (placement != null)
                {
                    var relativePlacement = placement.RelativePlacement as IIfcAxis2Placement3D;
                    if (relativePlacement != null)
                    {
                        var location = relativePlacement.Location as IIfcCartesianPoint;
                        if (location != null)
                        {
                            Console.WriteLine($"Location (X, Y, Z): {string.Join(", ", location.Coordinates)}");
                        }
                    }
                }

                // If there are associated geometric representations, access them
                var rep = element.Representation;
                if (rep != null)
                {
                    Console.WriteLine("\nGeometry representations found:");
                    foreach (var item in rep.Representations)
                    {
                        Console.WriteLine($"- Representation type: {item.GetType().Name}, Representation identifier: {item.RepresentationIdentifier}");
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("No geometry representation found.");
                }

                ExtractVerticesFromElement(element);
            }
        }

        static void ExtractVerticesFromElement(IIfcProduct element)
        {
            if (element.Representation == null)
            {
                Console.WriteLine("No geometric representation found for the element.");
                return;
            }

            foreach (var representation in element.Representation.Representations)
            {
                foreach (var item in representation.Items)
                {
                    ExtractVerticesFromRepresentationItem(item);
                }
            }
        }

        public static void ExtractVerticesFromRepresentationItem(IIfcRepresentationItem geomItem)
        {
            if (geomItem is IIfcPolyline polyline)
            {
                Console.WriteLine("Found a Polyline. Extracting vertices...");
                foreach (var point in polyline.Points)
                {
                    if (point is IIfcCartesianPoint cartesianPoint)
                    {
                        var coords = cartesianPoint.Coordinates;
                        Console.WriteLine($"Vertex: X = {coords[0].Value:F5}, Y = {coords[1].Value:F5}");
                    }
                }
            }
            if (geomItem is IIfcFacetedBrep brep)
            {
                Console.WriteLine("Found a Faceted Brep representation. Extracting vertices...");

                // Loop through all faces and extract vertices
                foreach (var face in brep.Outer.CfsFaces)
                {
                    foreach (var bound in face.Bounds)
                    {
                        if (bound is IIfcPolyLoop polyLoop)
                        {
                            foreach (var point in polyLoop.Polygon)
                            {
                                var coords = point.Coordinates;
                                Console.WriteLine($"Vertex: X = {coords[0]:F5},\tY = {coords[1]:F5},\tZ = {coords[2]:F5}");
                            }
                        }
                    }
                }
            }
            if (geomItem is IIfcAdvancedBrep advancedBrep)
            {
                Console.WriteLine("Found an Advanced Brep representation. Extracting advanced geometry...");

                foreach (var face in advancedBrep.Outer.CfsFaces)
                {
                    if (face is IIfcAdvancedFace advancedFace)
                    {
                        var surface = advancedFace.FaceSurface;
                        if (surface is IIfcBSplineSurface bsplineSurface)
                        {
                            Console.WriteLine("Found a B-Spline surface. Extracting control points...");

                            foreach (var controlPoint in bsplineSurface.ControlPointsList)
                            {
                                foreach (var cartesianPoint in controlPoint)
                                {
                                    var coords = cartesianPoint.Coordinates;
                                    Console.WriteLine($"Control Point: X = {coords[0]}, Y = {coords[1]}, Z = {coords[2]}");
                                }
                            }
                        }
                        else if (surface is IIfcPlane plane)
                        {
                            // Handle simple plane surfaces
                            var origin = plane.Position.Location.Coordinates;
                            Console.WriteLine($"Plane surface origin: X = {origin[0].Value:F5}, Y = {origin[1].Value:F5}, Z = {origin[2].Value:F5}");
                        }
                        else if (surface is IIfcCylindricalSurface cylindricalSurface)
                        {
                            // Handle cylindrical surfaces
                            var location = cylindricalSurface.Position.Location.Coordinates;
                            Console.WriteLine($"Cylindrical surface location: X = {location[0]}, Y = {location[1]}, Z = {location[2]}");
                            Console.WriteLine($"Radius: {cylindricalSurface.Radius}");
                        }
                        else
                        {
                            Console.WriteLine($"Unsupported surface type: {surface.GetType().Name}");
                        }
                    }
                }
            }
            if (geomItem is IIfcExtrudedAreaSolid extrudedSolid)
            {
                Console.WriteLine("Found an Extruded Area Solid. Extracting profile and extrusion...");

                var profile = extrudedSolid.SweptArea;
                if (profile is IIfcArbitraryClosedProfileDef arbitraryProfile)
                {
                    var outerCurve = arbitraryProfile.OuterCurve as IIfcPolyline;
                    if (outerCurve != null)
                    {
                        foreach (var point in outerCurve.Points)
                        {
                            if (point is IIfcCartesianPoint cartesianPoint)
                            {
                                var coords = cartesianPoint.Coordinates;
                                Console.WriteLine($"Profile Vertex: X = {coords[0].Value:F5}, Y = {coords[1].Value:F5}");
                            }
                        }
                    }
                }
                else if (profile is IIfcRectangleProfileDef rectangleProfile)
                {
                    Console.WriteLine($"Rectangle Profile - Length: {rectangleProfile.XDim:F5}, Width: {rectangleProfile.YDim:F5}");
                }
            }
            if (geomItem is IIfcMappedItem mappedItem)
            {
                Console.WriteLine("Found a Mapped Item (reference to another shape).");

                // Retrieve the transformation applied to the mapped item
                var transformation = mappedItem.MappingTarget as IIfcCartesianTransformationOperator;
                if (transformation != null)
                {
                    var origin = transformation.LocalOrigin;
                    if (origin != null)
                    {
                        Console.WriteLine($"Mapped item origin: ({origin.X}, {origin.Y}, {origin.Z})");
                    }
                }
                // Retrieve the source shape
                var shape = mappedItem.MappingSource.MappedRepresentation;
                foreach (var repItem in shape.Items)
                {
                    ExtractVerticesFromRepresentationItem(repItem);  // Recurse into the original geometry
                }
            }
            if (geomItem is IIfcTessellatedFaceSet tessellatedFaceSet)
            {
                Console.WriteLine("Found a Tessellated Face Set. Extracting vertices...");

                foreach (var coordIndex in tessellatedFaceSet.Coordinates.CoordList)
                {
                    Console.WriteLine($"Vertex: X = {coordIndex[0]}, Y = {coordIndex[1]}, Z = {coordIndex[2]}");
                }
            }
        }
    }
}
