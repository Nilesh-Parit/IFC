using System;
using System.Linq;
using Xbim.Common.Geometry;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;

namespace IfcPropExtract
{
    public class NoShapeInstance2
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
                else
                {
                    string? elementType = element.GetType().Name;
                    string? elementName = element.Name != null ? element.Name.ToString() : "***Unnamed***";

                    Console.WriteLine($"Element: {elementName}, Type: {elementType}");
                    Console.WriteLine();
                    Console.WriteLine($"Representation Count: {element.Representation.Representations.Count}");
                    foreach (var item in element.Representation.Representations)
                    {
                        Console.WriteLine($"Representation : {item.RepresentationType}");
                    }
                    Console.WriteLine();
                }

                //Get the local placement of the element
                var localPlacement = element.ObjectPlacement as IIfcLocalPlacement;

                // Create the context for geometry extraction
                var context = new Xbim3DModelContext(model);
                context.CreateContext();

                // Get the geometry representations directly from the element
                if (element.Representation != null && localPlacement != null)
                {
                    foreach (var representation in element.Representation.Representations)
                    {
                        // Process each representation item
                        foreach (var item in representation.Items)
                        {
                            HandleRepresentationItem(item, localPlacement);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No geometric representation found for the element.");
                }
            }
        }

        public static void HandleRepresentationItem(IIfcRepresentationItem item, IIfcLocalPlacement localPlacement)
        {
            switch (item)
            {
                case IIfcMappedItem mappedItem:
                    HandleMappedItem(mappedItem, localPlacement);
                    break;
                case IIfcBoundingBox boundingBox:
                    HandleBoundingBox(boundingBox, localPlacement);
                    break;
                case IIfcExtrudedAreaSolid extrudedAreaSolid:
                    HandleExtrudedAreaSolid(extrudedAreaSolid, localPlacement);
                    break;
                case IIfcAdvancedBrep advancedBrep:
                    HandleAdvancedBrep(advancedBrep, localPlacement);
                    break;
                case IIfcPolyline polyline:
                    HandlePolyline(polyline, localPlacement);
                    break;
                case IIfcBooleanClippingResult booleanClippingResult:
                    HandleBooleanClippingResult(booleanClippingResult, localPlacement);
                    break;
                case IIfcGeometricCurveSet geometricCurveSet:
                    HandleGeometricCurveSet(geometricCurveSet, localPlacement);
                    break;
                case IIfcTextLiteralWithExtent textLiteralWithExtent:
                    HandleTextLiteralWithExtent(textLiteralWithExtent, localPlacement);
                    break;
                default:
                    Console.WriteLine($"Unsupported representation item type: {item.GetType().Name}");
                    break;
            }
        }

        // Handler for IfcMappedItem
        private static void HandleMappedItem(IIfcMappedItem mappedItem, IIfcLocalPlacement localPlacement)
        {
            var source = mappedItem.MappingSource;
            var target = mappedItem.MappingTarget;
            Console.WriteLine("Handling IfcMappedItem - extracting source geometry and applying transformation.");

            // Check if the target is valid
            if (target == null)
            {
                Console.WriteLine("Invalid target transformation operator.");
                return;
            }

            // Recursively extract the source geometry
            var representationMap = source.MappedRepresentation;
            foreach (var item in representationMap.Items)
            {
                HandleRepresentationItem(item, localPlacement);
            }
        }

        // Handler for IfcBoundingBox
        private static void HandleBoundingBox(IIfcBoundingBox boundingBox, IIfcLocalPlacement localPlacement)
        {
            var min = boundingBox.Corner;
            var sizeX = boundingBox.XDim;
            var sizeY = boundingBox.YDim;
            var sizeZ = boundingBox.ZDim;

            // Calculate 8 corner vertices of the bounding box
            var vertices = new List<XbimPoint3D>
            {
                new XbimPoint3D(min.X, min.Y, min.Z),
                new XbimPoint3D(min.X + sizeX, min.Y, min.Z),
                new XbimPoint3D(min.X, min.Y + sizeY, min.Z),
                new XbimPoint3D(min.X, min.Y, min.Z + sizeZ),
                new XbimPoint3D(min.X + sizeX, min.Y + sizeY, min.Z),
                new XbimPoint3D(min.X + sizeX, min.Y, min.Z + sizeZ),
                new XbimPoint3D(min.X, min.Y + sizeY, min.Z + sizeZ),
                new XbimPoint3D(min.X + sizeX, min.Y + sizeY, min.Z + sizeZ)
            };

            foreach (var vertex in vertices)
            {
                var globalPoint = LocalGlobalCo.GetGlobalCoordinates(localPlacement,vertex);
                Console.WriteLine($"BoundingBox Vertex: X={globalPoint.X:F5}, Y={globalPoint.Y:F5}, Z={globalPoint.Z:F5}");
            }

            Console.WriteLine($"Total 8 vertices extracted from IfcBoundingBox.");
        }

        // Handler for IfcExtrudedAreaSolid
        private static void HandleExtrudedAreaSolid(IIfcExtrudedAreaSolid extrudedAreaSolid, IIfcLocalPlacement localPlacement)
        {
            var profile = extrudedAreaSolid.SweptArea;
            var direction = extrudedAreaSolid.ExtrudedDirection;
            var depth = extrudedAreaSolid.Depth;

            Console.WriteLine($"Handling IfcExtrudedAreaSolid - Profile and Extrusion Depth: {depth}");

            // Case 1:  Extract vertices from the profile and extrude them in the direction
            if (profile is IIfcArbitraryClosedProfileDef closedProfile)
            {
                if (closedProfile.OuterCurve is IIfcPolyline polyline)
                {
                    Console.WriteLine("Extruding profile along the specified direction.");
                    foreach (var point in polyline.Points)
                    {
                        var x = point.X;
                        var y = point.Y;
                        var z = 0.0; // Base profile is usually 2D

                        XbimPoint3D localPoint = new XbimPoint3D(x, y, z);

                        var globalPoint = LocalGlobalCo.GetGlobalCoordinates(localPlacement, localPoint);
                        Console.WriteLine($"Profile Vertex: X={globalPoint.X:F5}, Y={globalPoint.Y:F5}, Z={globalPoint.Z:F5}");
                    }
                }
            }
            // Case 2: Start from one corner and calculate all local coordinates, then apply extrusion
            // TODO - [bugs] 
            else if (profile is IIfcRectangleProfileDef rectangleProfile)
            {
                Console.WriteLine($"Rectangle Profile - Length: {rectangleProfile.XDim:F5}, Width: {rectangleProfile.YDim:F5}");

                // Step 1: Define the base corner (min corner) of the rectangle in local coordinates
                double xMin = -rectangleProfile.XDim / 2.0;
                double yMin = -rectangleProfile.YDim / 2.0;
                double z = 0; // Base profile is in 2D, so Z = 0 initially

                // Step 2: Define the local coordinates of the four rectangle corners
                List<XbimPoint3D> localVertices = new List<XbimPoint3D>
                {
                    new XbimPoint3D(xMin, yMin, z),                                                 // Bottom-left (min corner)
                    new XbimPoint3D(xMin + rectangleProfile.XDim, yMin, z),                         // Bottom-right
                    new XbimPoint3D(xMin + rectangleProfile.XDim, yMin + rectangleProfile.YDim, z), // Top-right
                    new XbimPoint3D(xMin, yMin + rectangleProfile.YDim, z)                          // Top-left
                };

                // Step 3: Apply extrusion to each vertex
                List<XbimPoint3D> extrudedVertices = new List<XbimPoint3D>();
                foreach (var localVertex in localVertices)
                {
                    // Extrude the vertex in the specified direction by the given depth
                    var extrudedVertex = new XbimPoint3D(
                        localVertex.X + direction.X * depth,
                        localVertex.Y + direction.Y * depth,
                        localVertex.Z + direction.Z * depth
                    );
                    extrudedVertices.Add(extrudedVertex);
                }

                // Step 4: Convert the extruded vertices to global coordinates
                foreach (var extrudedVertex in extrudedVertices)
                {
                    var globalVertex = LocalGlobalCo.GetGlobalCoordinates(localPlacement, extrudedVertex);
                    Console.WriteLine($"Extruded Vertex (Global): X={globalVertex.X:F5}, Y={globalVertex.Y:F5}, Z={globalVertex.Z:F5}");
                }
            }
        }

        // Handler for IfcAdvancedBrep
        private static void HandleAdvancedBrep(IIfcAdvancedBrep advancedBrep, IIfcLocalPlacement localPlacement)
        {
            Console.WriteLine("Handling IfcAdvancedBrep - extracting vertices from the boundary representation.");
            HashSet<XbimPoint3D> uniqueVertices = new HashSet<XbimPoint3D>();

            var shell = advancedBrep.Outer;
            foreach (var face in shell.CfsFaces)
            {
                if (face is IIfcAdvancedFace advancedFace)
                {
                    foreach (var bound in advancedFace.Bounds)
                    {
                        if (bound is IIfcFaceOuterBound outerBound)
                        {
                            var loop = outerBound.Bound;

                            if (loop is IIfcEdgeLoop edgeLoop)
                            {
                                foreach (var edge in edgeLoop.EdgeList)
                                {
                                    if (edge is IIfcOrientedEdge orientedEdge)
                                    {
                                        var edgeCurve = orientedEdge.EdgeElement;

                                        // Get the start and end vertices of the edge
                                        if (edgeCurve is IIfcEdgeCurve edgeCurveDetail)
                                        {
                                            var startVertex = edgeCurveDetail.EdgeStart as IIfcVertexPoint;
                                            var endVertex = edgeCurveDetail.EdgeEnd as IIfcVertexPoint;

                                            if (startVertex?.VertexGeometry is IIfcCartesianPoint startPoint)
                                            {
                                                // Convert local coordinates to global coordinates
                                                var localStart = new XbimPoint3D(startPoint.Coordinates[0], startPoint.Coordinates[1], startPoint.Coordinates[2]);
                                                var globalStart = LocalGlobalCo.GetGlobalCoordinates(localPlacement, localStart);
                                                uniqueVertices.Add(globalStart);
                                            }

                                            if (endVertex?.VertexGeometry is IIfcCartesianPoint endPoint)
                                            {
                                                // Convert local coordinates to global coordinates
                                                var localEnd = new XbimPoint3D(endPoint.Coordinates[0], endPoint.Coordinates[1], endPoint.Coordinates[2]);
                                                var globalEnd = LocalGlobalCo.GetGlobalCoordinates(localPlacement, localEnd);
                                                uniqueVertices.Add(globalEnd);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (var vertex in uniqueVertices)
            {
                Console.WriteLine($"AdvBrep Vertex: X={vertex.X:F5}, Y={vertex.Y:F5}, Z={vertex.Z:F5}");
            }
            Console.WriteLine("Vertex count= " + uniqueVertices.Count);
        }

        // Handler for IfcPolyline
        private static void HandlePolyline(IIfcPolyline polyline, IIfcLocalPlacement localPlacement)
        {
            Console.WriteLine("Handling IfcPolyline - extracting vertices.");
            foreach (var ifcPoint in polyline.Points)
            {
                // Extract the X, Y, and Z coordinates from IfcPoint
                double x = ifcPoint.Coordinates[0]; // X coordinate
                double y = ifcPoint.Coordinates[1]; // Y coordinate
                double z = 0;

                if (polyline.Points.Count == 3)
                {
                    z = double.IsNaN(ifcPoint.Coordinates[2]) ? 0 : ifcPoint.Coordinates[2];// Z coordinate
                }
                
                // Create and a new XbimPoint3D instance
                XbimPoint3D localPoint = new XbimPoint3D(x, y, z);

                var globalPoint = LocalGlobalCo.GetGlobalCoordinates(localPlacement, localPoint);
                Console.WriteLine($"Polyline Vertex: X={globalPoint.X:F5} , Y= {globalPoint.Y:F5} , Z= {globalPoint.Z:F5}");
            }
        }

        // Handler for IfcBooleanClippingResult
        private static void HandleBooleanClippingResult(IIfcBooleanClippingResult booleanClippingResult, IIfcLocalPlacement localPlacement)
        {
            Console.WriteLine("Handling IfcBooleanClippingResult - extracting the solid after boolean operations.");
            
            var firstOperand = booleanClippingResult.FirstOperand;
            var secondOperand = booleanClippingResult.SecondOperand;
            var operation = booleanClippingResult.Operator;

            // TODO - apply the boolean operations and extract the final solid geometry
            Console.WriteLine($"Boolean Operation: {operation}");
        }

        // Handler for IfcGeometricCurveSet
        private static void HandleGeometricCurveSet(IIfcGeometricCurveSet geometricCurveSet, IIfcLocalPlacement localPlacement)
        {
            Console.WriteLine("Handling IfcGeometricCurveSet - processing curves.");
            foreach (var geom in geometricCurveSet.Elements)
            {
                if (geom is IIfcPolyline polyline)
                {
                    HandlePolyline(polyline, localPlacement);
                }
            }
        }

        // Handler for IfcTextLiteralWithExtent
        private static void HandleTextLiteralWithExtent(IIfcTextLiteralWithExtent textLiteralWithExtent, IIfcLocalPlacement localPlacement)
        {
            Console.WriteLine($"Handling IfcTextLiteralWithExtent - Text: {textLiteralWithExtent.Literal}");
            // TODO - Extract additional information like extent and placement
        }
    }
}
