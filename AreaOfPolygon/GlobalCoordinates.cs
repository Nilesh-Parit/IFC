using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common.Geometry;
using Xbim.Ifc4.Interfaces;

namespace AreaOfPolygon
{
    public class GlobalCoordinates
    {
        #region Method 01- Get global coordinate using Point Class
        public static Points GetGlobalCords(IIfcObjectPlacement placement)
        {
            if (placement is IIfcLocalPlacement localPlacement)
            {
                var relativePlacement = localPlacement.RelativePlacement as IIfcAxis2Placement3D;
                if (relativePlacement != null)
                {
                    var localCords = new Points(
                        relativePlacement.Location.X,
                        relativePlacement.Location.Y,
                        relativePlacement.Location.Z
                    );

                    // openings on Y-axis walls => swap X and Y cord
                    if (IsWallAlongYAxis(localPlacement.PlacementRelTo))
                    {
                        var parentRelativePlacement = localPlacement.PlacementRelTo as IIfcLocalPlacement;
                        var parentAxis = (parentRelativePlacement?.RelativePlacement as IIfcAxis2Placement3D)?.RefDirection?.DirectionRatios;

                        // if it's along the negative Y-axis (RefDirection = (0, -1, 0))
                        if (parentAxis != null && Math.Abs(parentAxis[1] + 1) < 0.001)
                        {
                            // swap X and Y with sign correction
                            var temp = localCords.X;
                            localCords.X = Math.Abs(localCords.Y); // note- x take only value of y  
                            localCords.Y = -Math.Abs(temp);       //y is negative
                        }
                        else
                        {
                            var temp = localCords.X;
                            localCords.X = localCords.Y;
                            localCords.Y = temp;
                        }
                    }

                    if (localPlacement.PlacementRelTo != null)
                    {
                        var parentPlacementCord = GetGlobalCords(localPlacement.PlacementRelTo);

                        var globalCord = new Points(parentPlacementCord.X + localCords.X,
                                                     parentPlacementCord.Y + localCords.Y,
                                                      parentPlacementCord.Z + localCords.Z
                        );
                        return globalCord;
                    }
                    return localCords;
                }
            }
            return new Points(0, 0, 0);
        }
        #endregion

        // Method to check if the wall is along the Y-axis
        private static bool IsWallAlongYAxis(IIfcObjectPlacement placement)
        {
            if (placement is IIfcLocalPlacement localPlacement)
            {
                var relativePlacement = localPlacement.RelativePlacement as IIfcAxis2Placement3D;
                if (relativePlacement != null && relativePlacement.RefDirection != null)
                {
                    // Check if the wall's RefDirection indicates Y-axis alignment
                    var refDir = relativePlacement.RefDirection.DirectionRatios;
                     return refDir[0] == 0 && Math.Abs(refDir[1]) == 1;  // Y-axis alignment
//                    const double tolerance = 0.0001;
//                    return Math.Abs(refDir[0]) < tolerance && Math.Abs(Math.Abs(refDir[1]) - 1) < tolerance && Math.Abs(refDir[2]) < tolerance;
//
                }
            }
            return false;
        }

        private static bool IsElementAlongNegativeXAxis(IIfcLocalPlacement placement)
        {
            if(placement is IIfcLocalPlacement localPlacement)
            {
                var relativePlacement = localPlacement.RelativePlacement as IIfcAxis2Placement3D;
                if(relativePlacement!=null && relativePlacement.RefDirection != null)
                {
                    var refDir = relativePlacement.RefDirection.DirectionRatios;
                    return refDir[0] == -1 && refDir[1] == 0;
                }
            }
            return false;
        }

        private static bool IsElementAlongNegativeYAxis(IIfcLocalPlacement placement)
        {
            if (placement is IIfcLocalPlacement localPlacement)
            {
                var relativePlacement = localPlacement.RelativePlacement as IIfcAxis2Placement3D;
                if (relativePlacement != null && relativePlacement.RefDirection != null)
                {
                    var refDir = relativePlacement.RefDirection.DirectionRatios;
                    return refDir[0] == 0 && refDir[1] == -1;
                }
            }
            return false;
        }

        public static XbimPoint3D Set3DPointsRelToRefAxis(XbimPoint3D localPoint , IIfcLocalPlacement placement)
        {
            var point = localPoint;
            if (placement != null)
            {
                var relativePlacement = placement.RelativePlacement as IIfcAxis2Placement3D;
                if (relativePlacement != null && relativePlacement.RefDirection != null)
                {
                    var refDir = relativePlacement.RefDirection.DirectionRatios;

                    // Define a tolerance for floating-point comparison
                    const double tolerance = 0.0001;
                    // Negative X-axis (RefDirection: (-1, 0, 0))
                    if (refDir[0] == -1 && Math.Abs(refDir[1]) < tolerance && Math.Abs(refDir[2]) < tolerance)
                        return new XbimPoint3D(-Math.Abs(localPoint.X), localPoint.Y, localPoint.Z);
                    // Y-axis (RefDirection: (0, 1, 0))
                    else if (Math.Abs(refDir[0]) < tolerance && Math.Abs(refDir[1] - 1) < tolerance && Math.Abs(refDir[2]) < tolerance)
                        return new XbimPoint3D(localPoint.Y, localPoint.X, localPoint.Z);
                    //negative Y-axis (RefDirection: (0, -1, 0))
                    else if (Math.Abs(refDir[0]) < tolerance && refDir[1] == -1 && Math.Abs(refDir[2]) < tolerance)
                       return new XbimPoint3D(localPoint.Y, -Math.Abs(localPoint.X), localPoint.Z);
                }
            }
            return point;
        }
        #region Method 02- Get Global Cord using System.Numerics.Matrix
        public static Matrix4x4 GetGlobalMatrix(IIfcObjectPlacement placement)
        {
            if (placement is IIfcLocalPlacement localPlacement)
            {
                var relativePlacement = localPlacement.RelativePlacement as IIfcAxis2Placement3D;
                if (relativePlacement != null)
                {
                    // Create local transformation matrix
                    Matrix4x4 localMatrix = CreateTransformationMatrix(relativePlacement);

                    if (IsWallAlongYAxis(localPlacement.PlacementRelTo))
                    {
                        var parentRelativePlacement = localPlacement.PlacementRelTo as IIfcLocalPlacement;
                        var parentAxis = (parentRelativePlacement?.RelativePlacement as IIfcAxis2Placement3D)?.RefDirection?.DirectionRatios;

                        // if it's along the negative Y-axis (RefDirection = (0, -1, 0))
                        if (parentAxis != null && Math.Abs(parentAxis[1] + 1) < 0.001)
                        {
                            // swap X and Y with sign correction
                            var temp = localMatrix.M41;
                            localMatrix.M41 = localMatrix.M42; // note- x take only value of y  
                            localMatrix.M42 = -Math.Abs(temp);       //y is negative
                        }
                        else
                        {
                            var temp = localMatrix.M41;
                            localMatrix.M41 = localMatrix.M42;
                            localMatrix.M42 = temp;
                        }
                    }

                    // If there is a parent placement, multiply the local matrix with the parent's global matrix
                    if (localPlacement.PlacementRelTo != null)
                    {
                        var parentMatrix = GetGlobalMatrix(localPlacement.PlacementRelTo);
                        return parentMatrix * localMatrix; // Combine with parent's matrix
                    }

                    // If no parent, return the local matrix
                    return localMatrix;
                }
            }

            // If no placement or no relativePlacement, return identity matrix
            return Matrix4x4.Identity;
        }

        public static Matrix4x4 CreateTransformationMatrix(IIfcAxis2Placement3D placement)
        {
            // Get location (translation)
            var translation = new Vector3(
                (float)placement.Location.X,
                (float)placement.Location.Y,
                (float)placement.Location.Z
            );

            // Get rotation from Axis and RefDirection
            // Assuming Axis is Z-axis, RefDirection is X-axis
            var rotationMatrix = Matrix4x4.Identity;

            //rotation logic
            /* if (placement.RefDirection != null)            
             {
                 // Assuming RefDirection is the X-axis direction
                 var xDir = placement.RefDirection.DirectionRatios.Select(r=>(double)r.Value).ToArray();

                 // Assuming Axis is Z-axis (common for walls), or use placement.Axis if provided
                 var zDir = placement.Axis?.DirectionRatios.Select(r => (double)r.Value).ToArray() ?? new double[] { 0.0, 0.0, 1.0 };

                 // Swap X and Y for walls along Y - axis
             if (Math.Abs(xDir[0]) < 0.001 && Math.Abs(xDir[1]) > 0.001)
                {
                         // Wall is aligned along the Y-axis, handle swap and sign change
                         var isNegativeYAxis = xDir[1] < 0;

                     // Swap translation X and Y values
                     var temp = translation.X;
                     translation.X = translation.Y;
                     translation.Y = temp;

                     // Adjust sign if the wall is along the negative Y-axis
                     if (isNegativeYAxis)
                     {
                         translation.Y = -translation.Y;
                     }
                     // Adjust X-axis direction (RefDirection)
                     xDir = new double[] { 0, isNegativeYAxis ? -1.0 : 1.0, 0 };
                }
                 // Compute Y-axis direction by taking cross product of Z and X
                 var yDir = CrossProduct(xDir, zDir);

                 // Set rotation matrix (3x3 part of 4x4 matrix)
                 rotationMatrix = new Matrix4x4(
                     (float)xDir[0], (float)yDir[0], (float)zDir[0], 0,
                     (float)xDir[1], (float)yDir[1], (float)zDir[1], 0,
                     (float)xDir[2], (float)yDir[2], (float)zDir[2], 0,
                     0, 0, 0, 1
                 );
             }*/

            // Combine rotation and translation into one matrix
            var transformMatrix = Matrix4x4.CreateTranslation(translation) * rotationMatrix;
            return transformMatrix;
        }

        // Helper function for cross product
        public static double[] CrossProduct(double[] a, double[] b)
        {
            return new double[]
            {
        a[1] * b[2] - a[2] * b[1],
        a[2] * b[0] - a[0] * b[2],
        a[0] * b[1] - a[1] * b[0]
            };
        }
        #endregion

        #region Method 03-Get Global Cord using  XbimMatrix3D
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
                localPlacement = nextPlacement!;
            }

            return transformationMatrix;
        }

        // Helper function to extract matrix transformation from IfcAxis2Placement3D
        static XbimMatrix3D GetMatrixFromPlacement(IIfcAxis2Placement3D placement)
        {
            var location = placement.Location?.Coordinates;

            double x = location![0];
            double y = location[1];
            double z = location[2];

            // Create a translation matrix using the XbimMatrix3D constructor with translation values
            XbimMatrix3D matrix = XbimMatrix3D.Identity;
            matrix = XbimMatrix3D.CreateTranslation(x, y, z);

            return matrix;
        }
        #endregion

        #region Get Global Coordinate Using XbimPoint3D for vertices
      /*  public static XbimPoint3D GetGlobalCoordinates(XbimPoint3D localPoint, IIfcLocalPlacement placement)
        {
            var globalMatrix = XbimMatrix3D.Identity;
            var point = localPoint;
            if (placement != null)
            {
                if (IsWallAlongYAxis(placement!.PlacementRelTo))
                {
                    var parentRelativePlacement = placement.PlacementRelTo as IIfcLocalPlacement;
                    var parentAxis = (parentRelativePlacement?.RelativePlacement as IIfcAxis2Placement3D)?.RefDirection?.DirectionRatios;

                    // if it's along the negative Y-axis (RefDirection = (0, -1, 0))
                    if (parentAxis != null && Math.Abs(parentAxis[1] + 1) < 0.001)
                        point = new XbimPoint3D(localPoint.Y, -Math.Abs(localPoint.X), localPoint.Z);
                    else
                        point = new XbimPoint3D(localPoint.Y, localPoint.X, localPoint.Z);
                }
            }
            while (placement != null)
            {
                // Extract the transformation matrix for this placement
                var localMatrix = GetTransformationMatrix(placement.RelativePlacement as IIfcAxis2Placement3D);

                // Multiply the global matrix by the local transformation
                globalMatrix = localMatrix * globalMatrix;

                // Move up the hierarchy if there's a parent placement
                placement = placement.PlacementRelTo as IIfcLocalPlacement;
            }

            if (double.IsNaN(point.Z))
                return globalMatrix.Transform(new XbimPoint3D(-point.X, -point.Y, 0));
            return globalMatrix.Transform(localPoint);
        }*/

        public static XbimPoint3D GetGlobalCoordinates(XbimPoint3D localPoint, IIfcLocalPlacement placement)
        {
            var globalMatrix = XbimMatrix3D.Identity;
            var point = localPoint;

            while (placement != null)
            {
                var localMatrix = GetTransformationMatrix(placement.RelativePlacement as IIfcAxis2Placement3D);
                point = Set3DPointsRelToRefAxis(point, placement);
                globalMatrix = localMatrix * globalMatrix;
                placement = placement.PlacementRelTo as IIfcLocalPlacement;
            }
            if (double.IsNaN(point.Z))
                return globalMatrix.Transform(new XbimPoint3D(point.X, point.Y, 0));

            return globalMatrix.Transform(point);
        }

        // Helper function to extract transformation matrix from the placement
        public static XbimMatrix3D GetTransformationMatrix(IIfcAxis2Placement3D placement)
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

            return  translationMatrix;
        }

        /*// Helper function to create rotation matrix from axis and refDirection
        private static XbimMatrix3D GetRotationMatrix(IIfcDirection axis, IIfcDirection refDirection)
        {
            // Assuming axis and refDirection define a local coordinate system
            // Axis defines the Z axis, and RefDirection defines the X axis.

            // Extracting direction vectors
            var zAxis = new XbimVector3D(axis.X, axis.Y, axis.Z);
            var xAxis = new XbimVector3D(refDirection.X, refDirection.Y, refDirection.Z);

            // Calculate the Y axis as a cross product of Z and X
            var yAxis = zAxis.CrossProduct(xAxis);

            // Construct the rotation matrix
            return new XbimMatrix3D(xAxis, yAxis, zAxis);
        }*/ 
        #endregion
    }
}
