using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.ProductExtension;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;
using Xbim.Common.Step21;
using Xbim.Common.Metadata;
using Xbim.Common.Geometry;
using Xbim.Ifc2x3.MeasureResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.Ifc4.PropertyResource;
using Xbim.IO.Xml.BsConf;
using System.Xml.Linq;
using static Microsoft.Isam.Esent.Interop.EnumeratedColumn;

namespace IfcPropExtract
{
    public class ColumnDetails
    {
        public static void getProfileDetails(string guid)
        {
            string? filepath = ConfigurationManager.AppSettings["IfcFilePath"];

            using(var model = IfcStore.Open(filepath))
            {
                var column = model.Instances.FirstOrDefault<IfcColumn>(x => x.GlobalId == guid);

                if (column == null) return;
                Console.WriteLine("Column Name : "+column.Name);
                Console.WriteLine("Column Guid : "+column.GlobalId);

                Console.WriteLine();

                /*
                    Checking whether a column has a defined geometry by traversing through its representations
                    and extracting the first available extruded area solid's swept area. If it finds one,
                    it assigns it to profileDef; if not, profileDef will be null.
                */

                var profileDef = column.Representation?.Representations
                        .SelectMany(r => r.Items)
                        .OfType<IIfcShapeRepresentation>()
                        .SelectMany(sr => sr.Items)
                        .OfType<IIfcExtrudedAreaSolid>()
                        .Select(eas => eas.SweptArea)
                        .FirstOrDefault();

                //if (profileDef is IIfcRectangleProfileDef rectProfile)
                //{
                //    // Access the profile details: Name, XDim, YDim
                //    string? profileName = rectProfile.ProfileName;
                //    double xDim = rectProfile.XDim;
                //    double yDim = rectProfile.YDim;

                //    Console.WriteLine($"Profile Name: {profileName}");
                //    Console.WriteLine($"XDim: {xDim} meters");
                //    Console.WriteLine($"YDim: {yDim} meters");
                //}
                //else
                //{
                //    Console.WriteLine("No rectangle profile found for the specified column.");
                //}
            }

        }

        public static void getDimensions(string guid)
        {
            double? width, height, length;

            string? filepath = ConfigurationManager.AppSettings["IfcFilePath"];

            IIfcShapeRepresentation? shapeRepresentation = null;

            using (var model = IfcStore.Open(filepath))
            {
                var column = model.Instances.FirstOrDefault<IfcWall>(x => x.GlobalId == guid);

                if(column.Representation != null)
                {
                    shapeRepresentation = column.Representation.Representations
                    .OfType<IIfcShapeRepresentation>()
                    .FirstOrDefault();

                    foreach (var item in shapeRepresentation.Items)
                    {
                        if(item is IIfcExtrudedAreaSolid extrudedAreaSolid)
                        {
                            var profile = extrudedAreaSolid.SweptArea;

                            if(profile is IIfcRectangleProfileDef rectangleProfile)
                            {
                                width = rectangleProfile.XDim;
                                height = rectangleProfile.YDim;
                            }
                            else if(profile is IIfcCircleProfileDef circleProfile)
                            {
                                width = height = circleProfile.Radius * 2;
                            }

                            // Length is based on extrusion depth
                            length = extrudedAreaSolid.Depth;
                        }
                    }
                }
            }

            // Display the retrieved information
            //Console.WriteLine($"Element GUID: {elementGuid}");
            //if (width.HasValue && height.HasValue)
            //    Console.WriteLine($"Width (b): {width.Value} meters, Height (h): {height.Value} meters");

            //if (length.HasValue)
            //    Console.WriteLine($"Length: {length.Value} meters");

            //if (volume.HasValue)
            //    Console.WriteLine($"Volume: {volume.Value} cubic meters");
            //else
            //    Console.WriteLine("Volume: Not available");
        }
    }
}
