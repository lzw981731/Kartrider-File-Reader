using System.Diagnostics;
using System.Numerics;
using System.Text;
using eP.Text;

namespace KartCity.Common.Engine
{
    public struct BoundingBox
    {
        public Vector3 MinPosition { get; set; }

        public Vector3 MaxPosition { get; set; }

        public BoundingBox(Vector3 minPos, Vector3 maxPos)
        {
            MinPosition = minPos;
            MaxPosition = maxPos;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<BoundingBox>");
            stringBuilder.ConstructPropertyString(1, "MinPosition", MinPosition);
            stringBuilder.ConstructPropertyString(1, "MaxPosition", MaxPosition);
            stringBuilder.AppendLine("</BoundingBox>");
            return stringBuilder.ToString();
        }

        public float GetMinDistance(Vector3 cameraPosition, ref Matrix4x4 transMatrix)
        {
            Vector3 transMinPos = Vector3.Transform(MinPosition, transMatrix);
            Vector3 transMaxPos = Vector3.Transform(MaxPosition, transMatrix);
            Vector3 vecA = transMinPos - cameraPosition;
            Vector3 vecB = transMaxPos - transMinPos;
            Vector3 vecC = MathF.Max(MathF.Min(1, Vector3.Dot(vecA, vecB) / vecB.LengthSquared()), 0) * vecB;
            Vector3 minDisPos = transMinPos + vecC;
            return Vector3.Distance(minDisPos, cameraPosition);
        }

        public void GetMinMaxZ(ref Matrix4x4 projViewMat, ref Matrix4x4 modelMat, out double outMinZ,
            out double outMaxZ)
        {
            Matrix4x4 transMat = modelMat * projViewMat;
            Vector3 zTransform = new Vector3(transMat.M13, transMat.M23, transMat.M33);
            // Vector3 minZVec = MinPosition * zTransform;
            // Vector3 maxZVec = MaxPosition * zTransform;
            
            // Vector3 wTransform = new Vector3(transMat.M14, transMat.M24, transMat.M34);
            // float minZ = float.MaxValue;
            // float maxZ = float.MinValue;
            // float[] xAxis = [MinPosition.X, MaxPosition.X];
            // float[] yAxis = [MinPosition.Y, MaxPosition.Y];
            // float[] zAxis = [MinPosition.Z, MaxPosition.Z];
            // foreach (var x in xAxis)
            // foreach (var y in yAxis)
            // foreach (var z in zAxis)
            // {
            //     Vector3 pos = new Vector3(x, y, z);
            //     pos = Vector3.Transform(pos, transMat);
            //     minZ = Math.Min(pos.Z, minZ);
            //     maxZ = Math.Max(pos.Z, maxZ);
            // }
            
            // Notice that for vector dot operation, 
            // vecA dot vecB =  vecA.x * vecB.x
            //                + vecA.y * vecB.y 
            //                + vecA.z * vecB.z
            
            double minX = MinPosition.X * zTransform.X;
            double minY = MinPosition.Y * zTransform.Y;
            double minZ = MinPosition.Z * zTransform.Z;
            
            double maxX = MaxPosition.X * zTransform.X;
            double maxY = MaxPosition.Y * zTransform.Y;
            double maxZ = MaxPosition.Z * zTransform.Z;
            
            if (minX > maxX)
                (minX, maxX) = (maxX, minX);
            if (minY > maxY)
                (minY, maxY) = (maxY, minY);
            if (minZ > maxZ)
                (minZ, maxZ) = (maxZ, minZ);
            
            // float minW = Vector3.Dot(new Vector3(minX, minY, minZ), wTransform);
            // float maxW = Vector3.Dot(new Vector3(maxX, maxY, maxZ), wTransform);

            double minRes = (minX + minY + minZ) + transMat.M43;
            double maxRes = (maxX + maxY + maxZ) + transMat.M43;

            outMinZ = minRes;
            outMaxZ = maxRes;
        }
        
        public void GetMinMaxZSlow(ref Matrix4x4 projViewMat, ref Matrix4x4 modelMat, out float outMinZ, out float outMaxZ)
        {
            Matrix4x4 transMat = modelMat * projViewMat;
            Vector3 zTransform = new Vector3(transMat.M13, transMat.M23, transMat.M33);
            Vector3 minZVec = MinPosition * zTransform;
            Vector3 maxZVec = MaxPosition * zTransform;
            Vector3 wTransform = new Vector3(transMat.M14, transMat.M24, transMat.M34);
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;
            float[] xAxis = [MinPosition.X, MaxPosition.X];
            float[] yAxis = [MinPosition.Y, MaxPosition.Y];
            float[] zAxis = [MinPosition.Z, MaxPosition.Z];
            foreach (var x in xAxis)
            foreach (var y in yAxis)
            foreach (var z in zAxis)
            {
                Vector3 pos = new Vector3(x, y, z);
                pos = Vector3.Transform(pos, transMat);
                minZ = Math.Min(pos.Z, minZ);
                maxZ = Math.Max(pos.Z, maxZ);
            }
            
            outMinZ = minZ;
            outMaxZ = maxZ;
        }
    }
}
