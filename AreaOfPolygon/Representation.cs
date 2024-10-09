using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xbim.Common.Geometry;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.TopologyResource;
using Xbim.Ifc4x3.GeometryResource;

namespace AreaOfPolygon
{
    public class Representation
    {
        public static void GetRepresentaionIndentifierAndTypesNames(IIfcBuildingElement element)
        {
            var represents = element.Representation.Representations;
            if (represents != null)
            {
                foreach(var represent in represents)
                {
                    Console.WriteLine("\nRepresentation identifier: "+represent.RepresentationIdentifier);
                    Console.WriteLine("Representation type: "+represent.RepresentationType);
                    foreach(var item in represent.Items) 
                        Console.WriteLine("Representation item: "+item.GetType().Name);
                }
            }
        }
        public static void GetRepresentationsDetails(IIfcBuildingElement element)
        {
            var represent = element.Representation.Representations;
            if (represent != null)
            {
                foreach (var representation in represent)
                {
                    if (representation.RepresentationIdentifier == "Box")
                    {

                        Console.WriteLine($"\nrepresentation identifier : {representation.RepresentationIdentifier}");
                        Console.WriteLine($"representation type : {representation.RepresentationType}");
                        foreach (var item in representation.Items)
                        {
                            if (item is IIfcBoundingBox box)
                            {
                                Console.WriteLine($"Bounding box width: {box.XDim}");
                                Console.WriteLine($"Bounding box length: {box.YDim}");
                                Console.WriteLine($"Bounding box height: {box.ZDim}");

                            }
                        }
                    }
                    else if (representation.RepresentationIdentifier == "Body")
                    {

                        Console.WriteLine($"\nrepresentation identifier : {representation.RepresentationIdentifier}");
                        Console.WriteLine($"representation type : {representation.RepresentationType}");
                        foreach (var item in representation.Items)
                        {
                            if (item is IIfcExtrudedAreaSolid extrudedSolid)
                            {
                                Console.WriteLine($"Extrusion depth: {extrudedSolid.Depth}");
                                Console.WriteLine($"Extrusion swept area : {extrudedSolid.SweptArea}");
                                Console.WriteLine("Extrusion direction: "+extrudedSolid.ExtrudedDirection.ToString());

                                var profile = extrudedSolid.SweptArea as IIfcArbitraryClosedProfileDef;
                                var extrusionDepth = extrudedSolid.Depth;

                                if (profile is IIfcArbitraryClosedProfileDef closedProfile)
                                {
                                    var outerCurve = closedProfile.OuterCurve;

                                    if (outerCurve is IIfcPolyline polyline)
                                    {
                                        foreach (var point in polyline.Points)
                                        {
                                            Console.WriteLine($"Profile Point: X={point.X}, Y={point.Y}, Z={point.Z}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Unsupported curve type for profile outer curve.");
                                    }
                                }
                            }
                            if(item is IIfcMappedItem mappedItem)
                            {
                                Console.WriteLine($"Mapped item: {item.StyledByItem}");
                            }
                           if(item is IIfcSweptAreaSolid sweptAreaSolid)
                            {
                                var profile = sweptAreaSolid as IIfcArbitraryClosedProfileDef;

                                if (profile is IIfcArbitraryClosedProfileDef closedProfile)
                                {
                                    var outerCurve = closedProfile.OuterCurve;

                                    if (outerCurve is IIfcPolyline polyline)
                                    {
                                        foreach (var point in polyline.Points)
                                        {
                                            Console.WriteLine($"Profile Point: X={point.X}, Y={point.Y}, Z={point.Z}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Unsupported curve type for profile outer curve.");
                                    }
                                }
                            }
                        }
                    }
                    else if (representation.RepresentationIdentifier == "Axis")
                    {
                        Console.WriteLine($"\nrepresentation identifier : {representation.RepresentationIdentifier}");
                        Console.WriteLine($"representation type : {representation.RepresentationType}");
                        foreach (var item in representation.Items)
                        {
                            if (item is IIfcCurve curve)
                            {
                                Console.WriteLine($"curve dim: {curve.Dim}");
                            }
                        }
                    }
                }
            }
        }

        public static void ShapeRepresentation(IIfcBuildingElement element)
        {var placement = element.ObjectPlacement as IIfcLocalPlacement;
            var represents = element.Representation.Representations;
            if (represents != null)
            {
                foreach (var represent in represents)
                {
                    if (represent is IIfcShapeRepresentation shapes)
                    {
                        foreach (var items in shapes.Items)
                        {
                            ExtractVertexFromRepresentationItem(items, placement);
                        }
                    }
                    else
                    { Console.WriteLine("No shape representation found for this element."); }
                }
            }
        }

        public static void ExtractVertexFromRepresentationItem(IIfcRepresentationItem items, IIfcLocalPlacement placement)
        {
            if (items is IIfcFacetedBrep facetedBrep) 
                RepresentationItems.FacetedBrepItemHandler(facetedBrep, placement);
            else if (items is IIfcExtrudedAreaSolid extrudedSolid)
               RepresentationItems.ExtrudedAreaSolidItemHandler(extrudedSolid, placement);
            else if (items is IIfcPolyline polyline)
                RepresentationItems.PolyLineItemHandler(polyline, placement);
            else if (items is IIfcBoundingBox boundingBox)
                RepresentationItems.BoundingBoxItemHandler(boundingBox, placement);
            else if (items is IIfcMappedItem mappedItem)
                RepresentationItems.MappedItemHandler(mappedItem, placement);
            else if (items is IIfcAdvancedBrep advancedBrep)
                RepresentationItems.AdvancedBrepItemHandler(advancedBrep, placement);
            else if (items is IIfcBooleanClippingResult clippingResult)
                RepresentationItems.BooleanClippingResultItemHandler(clippingResult, placement);
        }
        public static void TopologyRepresentation(IIfcBuildingElement element)
        {
            var represents = element.Representation.Representations;
            if(represents != null)
            {
                foreach (var represent in represents)
                {
                    if (represent is IIfcTopologyRepresentation topologyRep)
                    {
                        foreach (var representationItem in topologyRep.Items)
                        {
                            // Check if the representation is a face-based surface model
                            if (representationItem is IIfcFaceBasedSurfaceModel faceModel)
                            {
                                foreach (var faceSet in faceModel.FbsmFaces)
                                {
                                    foreach (var face in faceSet.CfsFaces)
                                    {
                                        foreach (var bound in face.Bounds)
                                        {
                                            if (bound.Bound is IIfcPolyLoop polyLoop)
                                            {
                                                foreach (var point in polyLoop.Polygon)
                                                {
                                                    Console.WriteLine($"Vertex: X={point.X}, Y={point.Y}, Z={point.Z}");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (representationItem is IIfcVertexPoint vertexItem)
                            {
                                var vertexGeometry = vertexItem.VertexGeometry;
                                if (vertexGeometry is IIfcPointOnSurface pointOnSurface)
                                {
                                    var basicSurface = pointOnSurface.BasisSurface;
                                    Console.WriteLine(basicSurface.ToString());
                                }
                            }
                            else
                                Console.WriteLine("Unsupported topology item.");
                        }
                    }
                    else
                        Console.WriteLine("No Topology representation found for this element."); 
                }
            }
        }

    }
}
