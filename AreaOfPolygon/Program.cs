using AreaOfPolygon;
using System.Configuration;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.Ifc4.ProductExtension;
using Xbim.Common.Geometry;
using Tekla.Common.Geometry.Shapes;
using Xbim.Ifc4.RepresentationResource;
public class Polygons
{
    public static void Main(string[] args)
    {
        string? filePath = ConfigurationManager.AppSettings["filePath"];
        //filePath = @"C:\Users\unnati.kurekar\Downloads\USA-SMITH RESIDENCE_10-05-2024.ifc";

        string guid = "1Z4ebjXojEHwWyXyiXHM_I";

        // Open the IFC file
        using (var model = IfcStore.Open(filePath))
        {
            var buildingStorey = model.Instances.OfType<IfcBuildingStorey>().ToList();

            //general formula to find any element in building in ifc
            var element = model.Instances.FirstOrDefault<IIfcBuildingElement>(x => x.GlobalId == guid);

            if (element == null)
            { Console.WriteLine($"Element for Guid = {guid} not found "); return; }
            Console.WriteLine($"Element name : {element.Name}, id: {element.GlobalId}");
                

            //create context for 3d model
            var context = new Xbim3DModelContext(model);
            GeometryUsingShapeInstance.GetGlobalCoordinates(context,element);

            // PropertySet.PropertySetValues(element);
            // QuantitySet.QuantitySetValues(element);
             // Location.LocationOfElement(element);

            //------------------------------------------------------------
            #region Walls
            //to typecast the shapeinstance into xbim common.geometry
            /* var wall = model.Instances.FirstOrDefault<IIfcWall>(x => x.GlobalId == wallGuid)
                 ?? model.Instances.FirstOrDefault<IfcWallStandardCase>(x => x.GlobalId == wallGuid);*/

            /* //opening elements
             var openings = wall.HasOpenings
                        .Where(x => x.RelatedOpeningElement is IfcOpeningElement)
                        .Select(x => x.RelatedOpeningElement as IfcOpeningElement);

             foreach (var opening in openings)
             {
                 Console.WriteLine($"\nOpening ID: {opening?.GlobalId}, Name: {opening?.Name}");

                 // Get the placement and dimensions of the opening (optional)
                 if (opening?.ObjectPlacement is IIfcLocalPlacement placement)
                 {
                     var relativeloc = placement.RelativePlacement as IIfcAxis2Placement3D;
                     if (relativeloc != null)
                     {
                         Console.WriteLine($"global X: {relativeloc.Location.X}");
                         Console.WriteLine($"global Y: {relativeloc.Location.Y}");
                         Console.WriteLine($"global Z: {relativeloc.Location.Z}");
                     }
                 }
                */

            //Association
            /*  var materialAssociations = wall.HasAssociations
                                            .Where(rel => rel is IIfcRelAssociatesMaterial)
                                            .Select(rel => ((IIfcRelAssociatesMaterial)rel).RelatingMaterial);

              foreach (var material in materialAssociations)
              {
                  if (material is IIfcMaterialProfileSet materialProfileSet)
                  {
                      Console.WriteLine($"\nMaterial Profile Set: {materialProfileSet.Name}");
                      foreach (var profile in materialProfileSet.MaterialProfiles)
                      {
                          Console.WriteLine($"Profile: {profile.Profile?.ProfileName}, Material: {profile.Material?.Name}");
                      }
                  }
              }*/

            //Representation
            var represent = element.Representation.Representations;

          /*  if (represent != null)
            {
                foreach(var representation in represent) 
                {
                    if (representation is IIfcShapeRepresentation shapes)
                    {
                        foreach (var items in shapes.Items)
                        {
                            if (items is IIfcExtrudedAreaSolid solid)
                            {
                                var point = GetPointsOfExtrudedSolidArea(solid);
                               
                            }
                        }
                    }
                    else if (representation is IIfcMappedItem mappedItem)
                    {
                        var source = mappedItem.MappingSource;
                        var target = mappedItem.MappingTarget;

                        // Process source and target, often part of a representation map
                        if (source is IIfcRepresentationMap representationMap)
                        {
                            foreach (var item in representationMap.MappedRepresentation.Items)
                            {
                                // Handle the mapped items, usually geometry like extrusions or breps
                                Console.WriteLine($"Mapped Item Type: {item.GetType()}");
                            }
                        }
                    } 
                }
            }*/

            //LOCATION - GLOBAL CO-ORDINATES
            //location - top and bottom elevation and global coordinates
            if (element.ObjectPlacement is IIfcLocalPlacement placement)
              {
                  var global = GlobalCoordinates.GetGlobalMatrix(placement);
                  Console.WriteLine("Global cord: " + global.Translation.ToString());

                  var globals = GlobalCoordinates.GetGlobalCoordinates(placement);
                  Console.WriteLine("Matrix Global cord: " + globals.Translation.ToString());

                  var globalCords = GlobalCoordinates.GetGlobalCords(placement);//bottom left vertex coord will extrated from this method for other we have to add length and height               
                  Console.WriteLine($"\nBottom left- Global co-ordinates: X = {globalCords.X}, Y = {globalCords.Y}, Z = {globalCords.Z}");

                  /* Console.WriteLine($"Bottom elevation: {bottomElevation}");
                   double height;
                  var bottomElevation = globalCords.Z;
                  if (represent != null)
                  {
                      foreach (var rep in represent)
                      {
                          if (rep.RepresentationIdentifier == "Box")
                          {
                              foreach (var item in rep.Items)
                              {
                                  if (item is IIfcBoundingBox box)
                                  {
                                      // width = box.XDim;length = box.YDim;heigth = box.ZDim; ===> for  wall length-xdim, width-ydim
                                      Console.WriteLine($"Bottom Right- Global cord: X = {globalCords.X}, Y = {globalCords.Y - box.XDim}, Z = {globalCords.Z}");
                                      Console.WriteLine($"Top Left- Global cord: X = {globalCords.X}, Y = {globalCords.Y}, Z = {globalCords.Z + box.ZDim}");
                                      Console.WriteLine($"Top Right- Global cord: X = {globalCords.X}, Y = {globalCords.Y - box.XDim}, Z = {globalCords.Z + box.ZDim}");

                                  }
                              }
                          }
                      }
                  }*/
              }

            //Location- Global Coordinates of Wall Openings
            /*  if (element.HasOpenings != null)
              {
                  foreach (var opening in element.HasOpenings)
                  {
                      // Console.WriteLine(opening.RelatingBuildingElement.GlobalId); //using this we get parent element in heirarchy
                      Console.WriteLine($"\nOpening : {opening.Name} - {opening.GlobalId}");

                      //if we  use relatingBuildingElement- gave same same cord for all openings
                      if (opening.RelatedOpeningElement.ObjectPlacement is IIfcLocalPlacement placement1)
                      {
                          var globalCordOfWindow = GlobalCoordinates.GetGlobalCords(placement1);
                          Console.WriteLine($"Global Coordinate: X = {globalCordOfWindow.X}, Y= {globalCordOfWindow.Y}, Z= {globalCordOfWindow.Z}");

                          var global = GlobalCoordinates.GetGlobalMatrix(placement1);
                          Console.WriteLine("Matrix Global cord: " + global.Translation.ToString());

                          var globals = GlobalCoordinates.GetGlobalCoordinates(placement1);
                          Console.WriteLine("Matrix Global cord: " + globals.Translation.ToString());
                      }
                  }
              }*/

            // to get all coordinates of element
            /*    if (represent != null)
                {
                    foreach (var rep in represent)
                    {
                        if (rep is IIfcShapeRepresentation shapeRepresentation)
                        {
                            foreach (var item in rep.Items)
                            {
                                if (item is IIfcExtrudedAreaSolid extrutedSolid)
                                {
                                    var localVertices = GetLocalVerticesFromProfileWithOffset(extrutedSolid);
                                    //   var point = GetPointsOfExtrudedSolidArea(extrutedSolid);
                                }
                            }
                        }
                    }
                }*/

            // Iterate through all IfcWall entities in the model
            /*  foreach (var wal in wall!)
              {

                  Console.WriteLine($"Wall Details: {wal.Name}");
                  Console.WriteLine($"Wall Object: {wal.ObjectType}");
                  Console.WriteLine($"opening :{wal.HasOpenings.First().GlobalId}");
              }*/

            #endregion

            //------------------------------------------------------------------------------
            //TODO representation
            Console.WriteLine("----------------");
            Representation.GetRepresentaionIndentifierAndTypesNames(element);
            Console.WriteLine("----------------");
         //   Representation.GetRepresentationsDetails(element);
            Console.WriteLine("----------------");
         //   Representation.TopologyRepresentation(element);
            Console.WriteLine("----------------");
            Representation.ShapeRepresentation(element);
            Console.WriteLine("----------------");
            //----------------------------------------------------------------------------------

            //Calculate open space area of floor
            var floor = buildingStorey.FirstOrDefault(x => x.LongName == "FIRST FLOOR");
                /*  var floorOpenArea = FloorRoomOpenspace(floor!);
                   if (floor != null)
                   {
                       var slabs = floor.ContainsElements.SelectMany(x =>x.RelatedElements).OfType<IIfcSlab>();
                       foreach (var slab in slabs)
                       {
                           Console.WriteLine("\n"+slab.Name);
                           if (slab.Name == "Floors : Floor : CIVIT-BIM - Floor Base Slab")
                           {
                               var netSlabArea = GetNetFloorAreaForSlab(slab);
                               var wallArea = netSlabArea - floorOpenArea;
                               Console.WriteLine("Wall area = " + wallArea);
                              // Console.WriteLine("Floor Base Slab excluding wall area = " + (netSlabArea - wallArea));
                           }
                            GetNetFloorAreaForSlab(slab);
                       }
                   }*/
            
        }
    }





    //to get points from extrudedAreaSolid
    public static List<Points> GetPointsOfExtrudedSolidArea(IIfcExtrudedAreaSolid solid)
    {
        List<Points> points = new List<Points>();

        if (solid.SweptArea is IIfcArbitraryClosedProfileDef profile)
        {
            if (profile.OuterCurve is IIfcPolyline polyline)
            {
                foreach (var point in polyline.Points)
                {
                    points.Add(new Points(point.X, point.Y, point.Z));
                    Console.WriteLine(points.ToString());
                }
            }
        }
        else if (solid.SweptArea is IIfcRectangleProfileDef rectProfile)
        {//for base
            points.Add(new Points(rectProfile.XDim, rectProfile.YDim, 0));
            points.Add(new Points(-rectProfile.XDim / 2, rectProfile.YDim / 2, 0));
            points.Add(new Points(-rectProfile.XDim / 2, -rectProfile.YDim / 2, 0));
            points.Add(new Points(rectProfile.XDim / 2, -rectProfile.YDim / 2, 0));

            //for top
            points.Add(new Points(rectProfile.XDim, rectProfile.YDim, solid.Depth));
            points.Add(new Points(-rectProfile.XDim / 2, rectProfile.YDim / 2, solid.Depth));
            points.Add(new Points(-rectProfile.XDim / 2, -rectProfile.YDim / 2, solid.Depth));
            points.Add(new Points(rectProfile.XDim / 2, -rectProfile.YDim / 2, solid.Depth));

            foreach (var item in points)
            {
                Console.WriteLine(item.ToString());
            }
        }
        return points;
    }

        //get NetFloorArea for space
        public static Xbim.Ifc4.MeasureResource.IfcAreaMeasure GetNetFloorAreaForSpace(IIfcSpace space)
        {
            double area = 0;
            var quantities = space.IsDefinedBy.Where(x => x.RelatingPropertyDefinition is IfcElementQuantity)
                           .Select(x => x.RelatingPropertyDefinition as IfcElementQuantity);
            foreach (var quantity in quantities)
            {
                foreach (var quant in quantity!.Quantities)
                {
                    if (quant is IIfcQuantityArea quantityArea)
                        if (quantityArea.Name == "NetFloorArea")
                        {
                            Console.WriteLine("NetFloorArea: " + quantityArea.AreaValue);
                            area = quantityArea.AreaValue;
                            return area;
                        }
                }
            } return area;
        }

        //get netfloorarea for slab
        public static double GetNetFloorAreaForSlab(IIfcSlab slab)
        {
            double area = 0;
            var quantities = slab.IsDefinedBy.Where(x => x.RelatingPropertyDefinition is IfcElementQuantity)
                           .Select(x => x.RelatingPropertyDefinition as IfcElementQuantity);
            foreach (var quantity in quantities)
            {
                foreach (var quant in quantity!.Quantities)
                {
                    if (quant is IIfcQuantityArea quantityArea)
                        if (quantityArea.Name == "GrossArea")
                        {
                            Console.WriteLine("NetFloorArea: " + quantityArea.AreaValue);
                            area = quantityArea.AreaValue;
                            return area;
                        }
                }
            }
            return area;
        }
        // get room available usage space
        public static double FloorRoomOpenspace(IfcBuildingStorey floor)
        {
            double totalAvailableSpace = 0;
            if (floor != null)
            {
                foreach (var space in floor.Spaces)
                {
                    if (space.LongName == "TOILET" || space.LongName == "STAIRCASE")
                    {
                        Console.WriteLine($" \nSpace name: {space.Name}, {space.LongName} {space.PredefinedType}, spaceGuid: {space.GlobalId}");
                        totalAvailableSpace -= GetNetFloorAreaForSpace(space);
                    }
                    else
                    {
                        Console.WriteLine($" \nSpace name: {space.Name}, {space.LongName} {space.PredefinedType}, spaceGuid: {space.GlobalId}");
                        totalAvailableSpace += GetNetFloorAreaForSpace(space);
                    }
                }
                Console.WriteLine($"\nAll Rooms area on first floor = {totalAvailableSpace}\n ");
            }
            return totalAvailableSpace;
        }
      

    } 
