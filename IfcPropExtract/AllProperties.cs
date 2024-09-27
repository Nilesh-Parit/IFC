using System;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.Common.Step21;
using Xbim.Common.Metadata;
using Xbim.Common.Geometry;
using Xbim.Ifc2x3.ProductExtension;
using Xbim.Ifc2x3.MeasureResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.PropertyResource;

namespace IfcPropExtract
{
    public class AllProperties
    {
        public static void getAllEntityProp(string? guid, string? filepath)
        {
            // Open the IFC file
            using (var model = IfcStore.Open(filepath))
            {
                // Iterate through all IfcWall entities in the model
                var wall = model.Instances.FirstOrDefault<IIfcWall>(x => x.GlobalId == guid);

                    Console.WriteLine($"Wall: {wall.Name}, GUID: {wall.GlobalId}");

                    // Get the property sets associated with the wall
                    var propertySets = wall.IsDefinedBy
                        .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                        .Select(r => r.RelatingPropertyDefinition as IIfcPropertySet);

                    foreach (var pSet in propertySets)
                    {
                        if (pSet == null) continue;

                        Console.WriteLine($"  Property Set: {pSet.Name}");

                        foreach (var property in pSet.HasProperties.OfType<IIfcPropertySingleValue>())
                        {
                            Console.WriteLine($"{property.Name}: {property.NominalValue}");
                        }
                        Console.WriteLine();
                    }

                    // Optionally, get geometric representation if needed
                    var representation = wall.Representation;
                    if (representation != null)
                    {
                        foreach (var rep in representation.Representations)
                        {
                            Console.WriteLine($"  Representation Type: {rep.RepresentationType}");

                            foreach (var item in rep.Items)
                            {
                                Console.WriteLine($"    Item: {item.GetType().Name}");
                            }
                            Console.WriteLine();
                        }
                    }

                //Quantityset
                //wall = model.Instances.FirstOrDefault<IIfcWall>(x => x.GlobalId == guid);
                var quantitySets = wall.IsDefinedBy.Where(rel => rel.RelatingPropertyDefinition is IfcElementQuantity)
                                        .Select(rel => rel.RelatingPropertyDefinition as IfcElementQuantity);
              

                foreach (var quantityset in quantitySets)
                {
                    Console.WriteLine($"Quantityset name: {quantityset!.Name}");
                    foreach (var quantity in quantityset.Quantities)
                    {
                        if (quantity is IIfcQuantityLength quantityLength)
                            Console.WriteLine($"{quantityLength.Name}: {quantityLength.LengthValue}, unit: {quantityLength.Unit?.ToString() ?? "no unit"} ");
                        if (quantity is IIfcQuantityArea quantityArea)
                            Console.WriteLine($"{quantityArea.Name}: {quantityArea.AreaValue} ,unit: {quantityArea.Unit?.ToString() ?? "no unit"}");
                        if (quantity is IIfcQuantityVolume quantityVolume)
                            Console.WriteLine($"{quantityVolume.Name}: {quantityVolume.VolumeValue}");
                    }
                }
            }
        }

        public static void getAllGeoProp(string? guid, string? filepath)
        {
            using (var model = IfcStore.Open(filepath))
            {
                // Find the entity with the given GUID
                var entity = model.Instances.FirstOrDefault<IIfcProduct>(e => e.GlobalId == guid);

                if (entity != null)
                {
                    Console.WriteLine($"Entity Name: {entity.Name}");
                    Console.WriteLine($"Entity Type: {entity.GetType().Name}");

                    // Extract location details
                    if (entity.ObjectPlacement is IIfcLocalPlacement localPlacement)
                    {
                        var relativePlacement = localPlacement.RelativePlacement as IIfcAxis2Placement3D;
                        if (relativePlacement != null)
                        {
                            Console.WriteLine($"Location - Global X: {relativePlacement.Location.X}");
                            Console.WriteLine($"Location - Global Y: {relativePlacement.Location.Y}");
                            Console.WriteLine($"Location - Global Z: {relativePlacement.Location.Z}");
                        }
                    }

                    // Extract geometry details like bounding box dimensions
                    if (entity.Representation != null)
                    {
                        var bbox = entity.Representation.Representations.OfType<IIfcShapeRepresentation>()
                                        .SelectMany(r => r.Items)
                                        .OfType<IIfcBoundingBox>()
                                        .FirstOrDefault();
                        if (bbox != null)
                        {
                            Console.WriteLine($"Bounding Box Length: {bbox.XDim}");
                            Console.WriteLine($"Bounding Box Width: {bbox.YDim}");
                            Console.WriteLine($"Bounding Box Height: {bbox.ZDim}");
                        }
                    }

                    // Extract additional properties such as Area, Length, Volume
                    double? area = GetPropertyAsDouble(entity, "Area");
                    double? length = GetPropertyAsDouble(entity, "Length");
                    double? volume = GetPropertyAsDouble(entity, "Volume");

                    Console.WriteLine($"Area: {(area.HasValue ? area.Value.ToString() : "Not Available")}");
                    Console.WriteLine($"Length: {(length.HasValue ? length.Value.ToString() : "Not Available")}");
                    Console.WriteLine($"Volume: {(volume.HasValue ? volume.Value.ToString() : "Not Available")}");
                }
                else
                {
                    Console.WriteLine("Entity with the specified GUID was not found in the model.");
                }
            }
        }
        static double? GetPropertyAsDouble(IIfcProduct entity, string propertyName)
        {
            var propertySets = entity.IsDefinedBy
                .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                .Select(r => r.RelatingPropertyDefinition as IIfcPropertySet);

            foreach (var pSet in propertySets)
            {
                if (pSet == null) continue;
                foreach (var property in pSet.HasProperties.OfType<IIfcPropertySingleValue>())
                {
                    if (property.Name.ToString().Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                    {
                        var value = property.NominalValue as IfcValue;
                        if (value is IfcAreaMeasure areaMeasure)
                            return (double)areaMeasure.Value;
                        else if (value is IfcLengthMeasure lengthMeasure)
                            return (double)lengthMeasure.Value;
                        else if (value is IfcVolumeMeasure volumeMeasure)
                            return (double)volumeMeasure.Value;
                    }
                }
            }
            return null; // Return null if the property is not found
        }
    }
}
