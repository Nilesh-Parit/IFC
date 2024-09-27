using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Configuration;
using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.Common.Geometry;

/*
 * This code finds the location of element in local coordinates 
 * and then convert it to global coordinate
 * using matrix translation
 */

namespace IfcPropExtract
{
    public class LocalGlobalCo
    {
        public static void getCoordinates()
        {
            string? guid = ConfigurationManager.AppSettings["Guid"];
            string? ifcFilePath = ConfigurationManager.AppSettings["IfcFilePath"];

            using (var model = IfcStore.Open(ifcFilePath))
            {
                var element = model.Instances.FirstOrDefault<IIfcBuildingElement>(x=>x.GlobalId==guid);

                string? elementType = element.GetType().Name;
                string? elementName = element.Name != null ? element.Name.ToString() : "***Unnamed***";

                Console.WriteLine($"Element: {elementName}, Type: {elementType}");

                var localPlacement = element.ObjectPlacement as IIfcLocalPlacement;

                if (localPlacement != null)
                {
                    var localCoordinates = GetLocalCoordinates(localPlacement);
                    var globalCoordinates = GetGlobalCoordinates(localPlacement);

                    Console.WriteLine("Local Coordinates: " + localCoordinates.Translation.ToString());
                    Console.WriteLine("Global Coordinates: " + globalCoordinates.Translation.ToString());
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("No data found");
                }
            }
        }

        static XbimMatrix3D GetLocalCoordinates(IIfcLocalPlacement localPlacement)
        {
            // Local coordinates from the relative placement
            var relativePlacement = localPlacement.RelativePlacement as IIfcAxis2Placement3D;
            if (relativePlacement != null)
            {
                return GetMatrixFromPlacement(relativePlacement);
            }
            return new XbimMatrix3D();
        }

        // Helper function to extract matrix transformation from IfcAxis2Placement3D
        static XbimMatrix3D GetMatrixFromPlacement(IIfcAxis2Placement3D placement)
        {
            var location = placement.Location?.Coordinates;

            double x = location[0];
            double y = location[1];
            double z = location[2];

            // Create a translation matrix using the XbimMatrix3D constructor with translation values
            XbimMatrix3D matrix = XbimMatrix3D.Identity;
            matrix = XbimMatrix3D.CreateTranslation(x, y, z);

            return matrix;
        }

        public static XbimMatrix3D GetGlobalCoordinates(IIfcLocalPlacement localPlacement)
        {
            XbimMatrix3D transformationMatrix = XbimMatrix3D.Identity;

            while (localPlacement != null)
            {
                var relativePlacement = localPlacement.RelativePlacement as IIfcAxis2Placement3D;
                if (relativePlacement != null)
                {
                    var matrix = GetMatrixFromPlacement(relativePlacement);
                    transformationMatrix = transformationMatrix * matrix;
                }

                var nextPlacement = localPlacement.PlacementRelTo as IIfcLocalPlacement;
                localPlacement = nextPlacement;
            }

            return transformationMatrix;
        }

        public static XbimPoint3D GetGlobalCoordinates(IIfcLocalPlacement localPlacement, XbimPoint3D localPoint)
        {
            var locPlace = localPlacement;

            XbimMatrix3D transformationMatrix = XbimMatrix3D.Identity;
            XbimMatrix3D localMatrix = XbimMatrix3D.CreateTranslation(localPoint.X,localPoint.Y,localPoint.Z);

            while (locPlace != null)
            {
                var relativePlacement = locPlace.RelativePlacement as IIfcAxis2Placement3D;
                if (relativePlacement != null)
                {
                    var matrix = GetMatrixFromPlacement(relativePlacement);
                    transformationMatrix = transformationMatrix * matrix;
                }

                var nextPlacement = locPlace.PlacementRelTo as IIfcLocalPlacement;
                locPlace = nextPlacement;
            }

            XbimMatrix3D globalMatrix = transformationMatrix * localMatrix;
            var translation = globalMatrix.Translation;

            return new XbimPoint3D(translation.X, translation.Y, translation.Z);
        }

        // Helper function to extract transformation matrix from the placement
        private static XbimMatrix3D UnnGetTransformationMatrix(IIfcAxis2Placement3D placement)
        {
            if (placement == null)
                return XbimMatrix3D.Identity;

            // Get the local origin (translation)
            var origin = new XbimPoint3D(placement.Location.X, placement.Location.Y, placement.Location.Z);

            // Create an identity matrix
            XbimMatrix3D translationMatrix = XbimMatrix3D.Identity;
            translationMatrix.Transform(origin);

            translationMatrix.OffsetX = placement.Location.X;
            translationMatrix.OffsetY = placement.Location.Y;
            translationMatrix.OffsetZ = placement.Location.Z;

            // Handle rotation using Axis and RefDirection if available
            var rotationMatrix = XbimMatrix3D.Identity;
            /*  if (placement.Axis != null && placement.RefDirection != null)
              {
                  rotationMatrix = GetRotationMatrix(placement.Axis, placement.RefDirection);
              }*/

            // Combine rotation and translation into one matrix
            return rotationMatrix * translationMatrix;
        }

        public static XbimPoint3D UnnGetGlobalCoordinates(XbimPoint3D localPoint, IIfcLocalPlacement placement)
        {
            var globalMatrix = XbimMatrix3D.Identity;

            while (placement != null)
            {
                // Extract the transformation matrix for this placement
                var localMatrix = UnnGetTransformationMatrix(placement.RelativePlacement as IIfcAxis2Placement3D);

                // Multiply the global matrix by the local transformation
                globalMatrix = localMatrix * globalMatrix;

                // Move up the hierarchy if there's a parent placement
                placement = placement.PlacementRelTo as IIfcLocalPlacement;
            }
            if (double.IsNaN(localPoint.Z))
                return globalMatrix.Transform(new XbimPoint3D(-localPoint.X, -localPoint.Y, 0));//todo points are adjusted
                                                                                                // Apply the global transformation matrix to the local point
            return globalMatrix.Transform(localPoint);
        }
    }
}
