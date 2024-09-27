using System;
using System.Configuration;
using System.Linq;
using Xbim.Ifc;
using Xbim.Ifc2x3.MeasureResource;
using Xbim.Ifc4.Interfaces;

/*
 * Entry Point Function / Main() Function
 */

namespace IfcPropExtract
{
    public class Program
    {
        static void Main(string[] args)
        {
            string? filePath = ConfigurationManager.AppSettings["IfcFilePath"];
            string? guid = ConfigurationManager.AppSettings["Guid"];

            //getWallFromGuid(wallGuid, filePath);

            //PropertySet.getPropertySet(wallGuid, filePath);

            //QuantitySet.getQuantitySet(wallGuid, filePath);

            //ColumnDetails.getProfileDetails("2XQ_HUVa946va8T6I45gbg");
            //ColumnDetails.getDimensions("1Z4ebjXojEHwWyXyiXHM$G");

            //AllProperties.getAllEntityProp(wallGuid, filePath);
            //AllProperties.getAllGeoProp(columnName, filePath);
            //AllProperties.getAllGeoProp("1Z4ebjXojEHwWyXyiXHM_M", filePath);

            //getIfcPropFromGuid(guid, filePath);

            //LocalCo.getCoordinates();

            Console.WriteLine("\n-------------------- With Shape Instance ------------------------\n");
            ShapeProp1.getShapeProp();
            Console.WriteLine();
            Console.WriteLine("\n------------------ Without Shape Instance -----------------------\n");
            //ShapeProp2.getShapeProp();

            NoShapeInstance2.getShapeProp();

            //RepresentationItems.getAllRepresentations();
        }

        public static void getWallFromGuid (string wallGuid, string? filePath)
        {
            if (filePath == null)
                return;
            
            // Open the IFC file
            using (var model = IfcStore.Open(filePath))
            {
                // Iterate through all IfcWall entities in the model
                var walls = model.Instances.OfType<IIfcWall>(); //.Where(x => x.GlobalId == wallGuid);

                foreach (var wall in walls)
                {
                    if (wall.GlobalId == wallGuid)
                    {
                        Console.WriteLine($"Wall Details: {wall.Name}, GUID: {wall.GlobalId}");
                        //Console.WriteLine(wall.HasOpenings.FirstOrDefault().GlobalId);
                    }
                }
            }
        }

        //public static void getIfcPropFromGuid(string guid, string? filePath)
        //{
        //    if (filePath == null)
        //        return;

        //    // Open the IFC file
        //    using (var model = IfcStore.Open(filePath))
        //    {
        //        var entity = model.Instances.FirstOrDefault<IIfcProduct>(x => x.GlobalId == guid);

        //        if (entity != null)
        //        {
        //            Console.WriteLine("GuId = " + guid);
        //            Console.WriteLine("Entity Name : " + entity.Name);
        //            Console.WriteLine("Entity Type : " + entity.GetType().Name);

        //            double? length = getPropertyValue(entity, "Length");
        //            double? area = getPropertyValue(entity, "Area");
        //            double? volume = getPropertyValue(entity, "Volume");

        //            Console.WriteLine("Length = " + length);
        //            Console.WriteLine("Area = " + area);
        //            Console.WriteLine("volume = " + volume);
        //        }
        //    }
        //}

        //public static double getPropertyValue(IIfcProduct entity, string propName)
        //{
        //    var propertySets = entity.IsDefinedBy
        //        .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
        //        .Select(r => r.RelatingPropertyDefinition as IIfcPropertySet);

        //    foreach (var pset in propertySets)
        //    {
        //        foreach (var property in pset.HasProperties.OfType<IIfcPropertySingleValue>())
        //        {
        //            if(property.Name.ToString().Equals(propName, StringComparison.OrdinalIgnoreCase))
        //            {
        //                var value = property.NominalValue;

        //                if (value is IfcReal realValue)
        //                {
        //                    return (double)realValue.Value; // Assuming 'Value' is of type double
        //                }
        //                else if (value is IfcInteger integerValue)
        //                {
        //                    return (double)integerValue.Value; // Assuming 'Value' is of type int
        //                }
        //                else if (value is IIfcMeasureValue measureValue && measureValue.Value is double measureDouble)
        //                {
        //                    return measureDouble; // Directly return if it's a double
        //                }
        //            }
        //        }
        //    }
        //    return 0.00;
        //}
    }
}
