using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Common.Geometry.Shapes;
using Xbim.Common.Geometry;
using Xbim.Ifc4.Interfaces;

namespace AreaOfPolygon
{
    public class RepresentationItems
    {
        public static void FacetedBrepItemHandler(IIfcFacetedBrep facetedBrep, IIfcLocalPlacement placement)
        {
            Console.WriteLine("FacetedBrep ");
            foreach (var face in facetedBrep.Outer.CfsFaces)
            {
                foreach (var loop in face.Bounds)
                {
                    if (loop is IIfcPolyLoop polyLoop)
                    {
                        foreach (var coord in polyLoop.Polygon)
                        {
                            var local = new XbimPoint3D(coord.X, coord.Y, coord.Z);
                            var global = GlobalCoordinates.GetGlobalCoordinates(local, placement);
                            Console.WriteLine($"Vertex: X={global.X}, Y={global.Y}, Z={global.Z}");
                        }
                    }
                }
            }
        }

        public static void ExtrudedAreaSolidItemHandler(IIfcExtrudedAreaSolid extrudedSolid, IIfcLocalPlacement placement)
        {
            var profile = extrudedSolid.SweptArea;
            var extrusionDepth = extrudedSolid.Depth;

            if (profile is IIfcArbitraryClosedProfileDef closedProfile)
            {
                Console.WriteLine("\nExtrudedSolid - SweptArea - ArbitaryClosedProfile ");
                var outerCurve = closedProfile.OuterCurve;

                if (outerCurve is IIfcPolyline polylines)
                {
                    PolyLineItemHandler(polylines,placement);
                }
                else
                {
                    Console.WriteLine("Unsupported curve type for profile outer curve.");
                }
                Console.WriteLine($"Extrusion Depth: {extrusionDepth}");
            }
            if (profile is IIfcRectangleProfileDef rectangleProfile)
            {//todo- prob
                Console.WriteLine("\nExtrudedSolid - SweptArea - RectangleProfile ");
                var point = new XbimPoint3D(rectangleProfile.XDim, rectangleProfile.YDim, double.NaN);
                var global = GlobalCoordinates.GetGlobalCoordinates(point, placement);
              /*  //to get other corner
                var refDir = rectangleProfile.Position.RefDirection.DirectionRatios;
                if (refDir[0] == -1 && refDir[1] == 0)
                    point = new XbimPoint3D(-rectangleProfile.XDim, rectangleProfile.YDim, double.NaN);
                else if (refDir[0] == 0 && refDir[1] == -1)
                    point = new XbimPoint3D(rectangleProfile.YDim, -rectangleProfile.XDim, double.NaN);
                else if (refDir[0]==0 && refDir[1]==1)
                    point = new XbimPoint3D(rectangleProfile.YDim, rectangleProfile.XDim,double.NaN);
*/

                Console.WriteLine($"x= {rectangleProfile.XDim.Value:F5},y= {rectangleProfile.YDim.Value:F5}, Depth= {extrusionDepth}");

                Console.WriteLine($"Bottom Vertex: X={global.X}, Y={global.Y}, Z={global.Z}");
                Console.WriteLine($"Bottom Vertex: X={global.X - point.X}, Y={global.Y}, Z={global.Z}");
                Console.WriteLine($"Bottom Vertex: X={global.X - point.X}, Y={global.Y - point.Y}, Z={global.Z}");
                Console.WriteLine($"Bottom Vertex: X={global.X}, Y={global.Y - point.Y}, Z={global.Z}");

                Console.WriteLine($"Top Vertex: X={global.X}, Y={global.Y}, Z={global.Z+extrusionDepth}");
                Console.WriteLine($"Top Vertex: X={global.X - point.X}, Y={global.Y}, Z={global.Z +extrusionDepth}");
                Console.WriteLine($"Top Vertex: X={global.X - point.X}, Y={global.Y - point.Y}, Z={global.Z+extrusionDepth}");
                Console.WriteLine($"Top Vertex: X={global.X}, Y={global.Y - point.Y}, Z={global.Z+extrusionDepth}");
            }
        }

        public static void PolyLineItemHandler(IIfcPolyline polyline, IIfcLocalPlacement placement)
        {
            Console.WriteLine("Polyline -");
            foreach (var point in polyline.Points)
            {
                var localPoint = new XbimPoint3D(point.X, point.Y, point.Z);
               // Console.WriteLine($"Polyline Global vertex: x={localPoint.X}, y={localPoint.Y}, z={localPoint.Z}");
                var globalPoint = GlobalCoordinates.GetGlobalCoordinates(localPoint, placement);
                Console.WriteLine($"Polyline Global vertex: x={globalPoint.X}, y={globalPoint.Y}, z={globalPoint.Z}");
            }
        }

        public static void BoundingBoxItemHandler(IIfcBoundingBox boundingBox, IIfcLocalPlacement placement)
        {
           // var corner = boundingBox.Corner;  //give location of bottom left corner
            Console.WriteLine("\nBounding box- corner= " + boundingBox.XDim + ", " + boundingBox.YDim + ", " + boundingBox.ZDim);
            var local = new XbimPoint3D(boundingBox.XDim,boundingBox.YDim,boundingBox.ZDim);
            var global = GlobalCoordinates.GetGlobalCoordinates(local, placement);
            Console.WriteLine($"Bounding box- Global top Vertex: X={global.X}, Y={global.Y}, Z={global.Z}");
        }

        public static void AdvancedBrepItemHandler(IIfcAdvancedBrep advancedBrep, IIfcLocalPlacement placement)
        {
            HashSet<XbimPoint3D> uniqueVertices = new HashSet<XbimPoint3D>();
            Console.WriteLine("Advanced Brep - ");

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
                                                var globalStart = GlobalCoordinates.GetGlobalCoordinates(localStart, placement);
                                                uniqueVertices.Add(globalStart);
                                            }

                                            if (endVertex?.VertexGeometry is IIfcCartesianPoint endPoint)
                                            {// Convert local coordinates to global coordinates
                                                var localEnd = new XbimPoint3D(endPoint.Coordinates[0], endPoint.Coordinates[1], endPoint.Coordinates[2]);
                                                var globalEnd = GlobalCoordinates.GetGlobalCoordinates(localEnd, placement);
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
                Console.WriteLine($"Global Vertex: X={vertex.X}, Y={vertex.Y}, Z={vertex.Z}");
            }
            Console.WriteLine("Vertex count= " + uniqueVertices.Count);
        }
        
        #region AdvancedBrep using surface
        /*else if(representationItem is IIfcAdvancedBrep advancedBrep)
                {
                     HashSet<XbimPoint3D> uniqueVertex = new HashSet<XbimPoint3D>(new PointComparer());
        XbimPoint3D local;
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
                                         local = new XbimPoint3D(cartesianPoint.X, cartesianPoint.Y, cartesianPoint.Z);
        var global = GlobalCoordinates.GetGlobalCoordinates(local, placement);
        Console.WriteLine($"Global Vertex: X={global.X}, Y={global.Y}, Z={global.Z}");
                                        uniqueVertex.Add(global);
                                    }
}
                            }
                            else if (surface is IIfcPlane plane)
{
    // Handle simple plane surfaces
    var origin = plane.Position.Location.Coordinates;
    local = new XbimPoint3D(origin[0], origin[1], origin[2]);
    var global = GlobalCoordinates.GetGlobalCoordinates(local, placement);
    uniqueVertex.Add(global);
    Console.WriteLine($"Plane surface origin: X={global.X}, Y={global.Y}, Z={global.Z} ");//{origin[2].Value:F5} use for precision in points

}
else if (surface is IIfcCylindricalSurface cylindricalSurface)
{
    // Handle cylindrical surfaces
    var location = cylindricalSurface.Position.Location.Coordinates;
    local = new XbimPoint3D(location[0], location[1], location[2]);
    var global = GlobalCoordinates.GetGlobalCoordinates(local, placement);
    uniqueVertex.Add(global);
    Console.WriteLine($"Radius: {cylindricalSurface.Radius}");
    Console.WriteLine($"Cylindrical surface location: X={global.X}, Y={global.Y}, Z={global.Z}");
}
else
{
    Console.WriteLine($"Unsupported surface type: {surface.GetType().Name}");
}
                        }
                    }
                    Console.WriteLine("count: " + uniqueVertex.Count);
                }
*/
        #endregion

        public static void MappedItemHandler(IIfcMappedItem mappedItem,IIfcLocalPlacement placement)
        {
            var mappedRepresentation = mappedItem.MappingSource.MappedRepresentation;
            
            foreach (var representationItem in mappedRepresentation.Items)
            {
                Console.WriteLine("\nVertex through Item: "+representationItem.GetType().Name);
                if (representationItem is IIfcPolyline polyline)
                    PolyLineItemHandler(polyline, placement);
                else if (representationItem is IIfcExtrudedAreaSolid extrudedAreaSolid)
                {
                    var extrusionDirection = extrudedAreaSolid.ExtrudedDirection as IIfcLocalPlacement;
                    if (extrusionDirection != null) { ExtrudedAreaSolidItemHandler(extrudedAreaSolid,extrusionDirection); } 
                    ExtrudedAreaSolidItemHandler(extrudedAreaSolid, placement); 
                }
                else if (representationItem is IIfcAdvancedBrep advancedBrep)
                    AdvancedBrepItemHandler(advancedBrep, placement);
                else if(representationItem is IIfcGeometricSet geometricSet)
                    GeometricSetItemHandler(geometricSet, placement);
                else
                {
                    Console.WriteLine("Unsupported representation item type for mapped geometry.");
                }
            }
        }

        private static void GeometricSetItemHandler(IIfcGeometricSet geometricSet, IIfcLocalPlacement placement)
        {
            foreach(var geometry in geometricSet.Elements)
            {
                if(geometry is IIfcTrimmedCurve trimmedCurve)
                { 
                    foreach(var trim in trimmedCurve.Trim1)
                    {
                        Console.WriteLine("Trim angle from "+trim.ToString());
                    }
                    foreach(var trim in trimmedCurve.Trim2)
                        Console.WriteLine("Trim angle to "+trim.ToString());
                }
                if(geometry is IIfcPolyline polyline)
                    PolyLineItemHandler(polyline, placement);
            }
        }

        public static void BooleanClippingResultItemHandler(IIfcBooleanClippingResult clippingResult, IIfcLocalPlacement placement)
        {
            Console.WriteLine("CLIPPING RESULT");
            // The base solid to be clipped (Operand 1)
            var baseSolid = clippingResult.FirstOperand as IIfcSolidModel;
            if (baseSolid != null)
            {
                  ExtractGeometryofClippingSolid.ExtractGeometryFromSolid(baseSolid, placement);
            }

            // The clipping element (Operand 2)
            var clippingElement = clippingResult.SecondOperand as IIfcHalfSpaceSolid;
            if (clippingElement != null)
            {
                 ExtractGeometryofClippingSolid.ExtractGeometryFromHalfSpaceSolid(clippingElement,placement);
            }
        } 

        public static void PolygonalBoundedHalfSpaceItemHandler(IIfcPolygonalBoundedHalfSpace boundedHalfSpace, IIfcLocalPlacement placement)
        {
            var boundary = boundedHalfSpace.PolygonalBoundary as IIfcPolyline;
            
            if (boundary != null)
            {
                Console.WriteLine("\nExtracting polyline points from the polygonal bounded half-space:");
                foreach (var point in boundary.Points)
                {
                    var local = new XbimPoint3D(point.X,- point.Y/2, point.Z);
                   // Console.WriteLine($"Point: X={local.X}, Y={local.Y}, Z={local.Z}");
                    var global = GlobalCoordinates.GetGlobalCoordinates(local,placement);
                    Console.WriteLine($"Point: X={global.X}, Y={global.Y}, Z={global.Z}");
                }
            }
            else
            {
                Console.WriteLine("No polyline boundary found.");
            }
        }
    }
}
