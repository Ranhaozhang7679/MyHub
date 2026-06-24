using MathNetExtend.Model.Position;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;

namespace MathNetExtend.Utils
{
    public class Coordinate3dHelper
    {
        public static PositionXYZ ToPosition(Vector<double> vec)
        {
            return new PositionXYZ(vec.At(0), vec.At(1), vec.At(2));
        }
        public static Vector<double> ToVector(PositionXYZ posi, bool qici = false)
        {
            var lis = new PositionXYZ(posi).ToPosiLis();
            if (qici) lis.Add(1);
            return DenseVector.OfArray(lis.ToArray());
        }
        const double TWOPI = 2 * Math.PI;
        const double PIOVERTWO = Math.PI / 2;

        public static Vector<double> xyz2rap(Vector<double> xyz)
        {
            double x = xyz.At(0);
            double y = xyz.At(1);
            double z = xyz.At(2);
            double r = Math.Sqrt(x * x + y * y + z * z);
            double h = 0;
            double p = 0;
            if (r > 0)
            {
                p = Math.Asin(x / r);
                if (Math.Abs(p) >= PIOVERTWO * 0.9999) h = 0;
                else h = Math.Atan2(-y, z);
            }
            else
                h = p = 0;
            return DenseVector.OfArray(new double[] { r, h, p });
        }
        public static Vector<double> rap2xyz(Vector<double> rap)
        {
            double r = rap.At(0);
            double h = rap.At(1);
            double p = rap.At(2);
            double x = 0;
            double y = 0;
            double z = 0;
            if (r > 0)
            {
                x = r * Math.Sin(p);
                double _yz = r * Math.Cos(p);
                if (_yz > 0)
                {
                    z = _yz * Math.Cos(h);
                    y = -_yz * Math.Sin(h);
                }
                else
                    x = y = 0;

            }
            else
                x = y = z = 0;
            return DenseVector.OfArray(new double[] { x, y, z });
        }
        public static Vector<double> xyz2rcp(Vector<double> xyz)
        {
            double x = xyz.At(0);
            double y = xyz.At(1);
            double z = xyz.At(2);
            double r = Math.Sqrt(x * x + y * y + z * z);
            double h = 0;
            double p = 0;
            if (r > 0)
            {
                p = Math.Asin(z / r);
                if (Math.Abs(p) >= PIOVERTWO * 0.9999) h = 0;
                else h = Math.Atan2(y, x);
            }
            else
                h = p = 0;
            return DenseVector.OfArray(new double[] { r, h, p });
        }
        public static Vector<double> rcp2xyz(Vector<double> rcp)
        {
            double r = rcp.At(0);
            double h = rcp.At(1);
            double p = rcp.At(2);
            double x = 0;
            double y = 0;
            double z = 0;
            if (r > 0)
            {
                z = r * Math.Sin(p);
                double xy = r * Math.Cos(p);
                if (xy > 0)
                {
                    x = xy * Math.Cos(h);
                    y = xy * Math.Sin(h);
                }
                else
                    x = y = 0;

            }
            else
                x = y = z = 0;
            return DenseVector.OfArray(new double[] { x, y, z });
        }
        public static void gramSchmidt(Vector<double> v1, Vector<double> v2, Vector<double> v3, out Vector<double> ov1, out Vector<double> ov2, out Vector<double> ov3)
        {
            ov1 = v1.Normalize(2);
            ov2 = (v2 - ((v2 * ov1) / (ov1 * ov1)) * ov1).Normalize(2);
            ov3 = (v3 - ((v3 * ov1) / (ov1 * ov1)) * ov1 - ((v3 * ov2) / (ov2 * ov2)) * ov2).Normalize(2);
        }
        public static void gramSchmidt(double k, int count, Vector<double> v1, Vector<double> v2, Vector<double> v3, out Vector<double> ov1, out Vector<double> ov2, out Vector<double> ov3)
        {
            Vector<double> tiv1 = v1, tiv2 = v2, tiv3 = v3;
            Vector<double> tov1 = v1, tov2 = v2, tov3 = v3;
            for (int i = 0; i < count; i++)
            {
                gramSchmidt(k, tiv1, tiv2, tiv3, out tov1, out tov2, out tov3);
                tiv1 = tov1;
                tiv2 = tov2;
                tiv3 = tov3;
            }
            gramSchmidt(tov1, tov2, tov3, out ov1, out ov2, out ov3);
        }
        public static void gramSchmidt(double k, Vector<double> v1, Vector<double> v2, Vector<double> v3, out Vector<double> ov1, out Vector<double> ov2, out Vector<double> ov3)
        {
            ov1 = (v1 - k * ((v1 * v2) / (v2 * v2)) * v2 - k * ((v1 * v3) / (v3 * v3)) * v3).Normalize(2);
            ov2 = (v2 - k * ((v2 * v1) / (v1 * v1)) * v1 - k * ((v2 * v3) / (v3 * v3)) * v3).Normalize(2);
            ov3 = (v3 - k * ((v3 * v1) / (v1 * v1)) * v1 - k * ((v3 * v2) / (v2 * v2)) * v2).Normalize(2);
        }
        public static Matrix<double> getRotateByLine(Vector<double> v1, Vector<double> v2, double theta)
        {
            var n = (v2 - v1).Normalize(2);
            var T = DenseMatrix.OfArray(new double[,]
            {
                {1,0,0,-v1.At(0) },
                {0,1,0,-v1.At(1) },
                {0,0,1,-v1.At(2) },
                {0,0,0,1 }
            });
            var rotate = GetAnyVectorRotate(n, theta);
            return T.Inverse() * rotate * T;
        }
        public static Matrix getRotateBypoint(double x, double y, double z, double rx, double ry, double rz)
        {
            double[,] arrayRx = new double[,] {
                { 1,0,0,0    }
                , { 0,Math.Cos(rx),-Math.Sin(rx),y*(1-Math.Cos(rx))+z*Math.Sin(rx)  }
                , {0,Math.Sin(rx),Math.Cos(rx),   z*(1-Math.Cos(rx))-y*Math.Sin(rx)  }
                , { 0,0,0,1}};
            double[,] arrayRy = new double[,] {
                { Math.Cos(ry),0,Math.Sin(ry),x*(1-Math.Cos(ry))-z*Math.Sin(ry)  }
                , { 0,1,0,0}
                , { -Math.Sin(ry),0,Math.Cos(ry),z*(1-Math.Cos(ry))+x*Math.Sin(ry)     }
                , { 0,0,0,1}};
            double[,] arrayRz = new double[,] {
                { Math.Cos(rz),-Math.Sin(rz),0,x*(1-Math.Cos(rz))+y*Math.Sin(rz)}
                , { Math.Sin(rz),Math.Cos(rz),0,y*(1-Math.Cos(rz))-x*Math.Sin(rz)    }
                , { 0,0,1,0}
                , { 0,0,0,1}};
            var rotateRx = DenseMatrix.OfArray(arrayRx);
            var rotateRy = DenseMatrix.OfArray(arrayRy);
            var rotateRz = DenseMatrix.OfArray(arrayRz);
            return rotateRz * rotateRy * rotateRx;
        }
        public static Matrix getOffsetBypoint(double x, double y, double z, double sx, double sy, double sz)
        {
            double[,] scale = new double[,] {
                { sx,0,0,x*(1-sx)}
                , { 0,sy,0,y*(1-sy)}
                , {0,0,sz,z*(1-sz)}
                , { 0,0,0,1}};
            return DenseMatrix.OfArray(scale);
        }
        public static Matrix getWorld2WorkMatrix(double x, double y, double z, double rx, double ry, double rz)
        {
            x = -x;
            y = -y;
            z = -z;
            rx = -rx;
            ry = -ry;
            rz = -rz;
            double[,] arrayRx = new double[,] {
                { 1,0,0,0    }
                , { 0,Math.Cos(rx),-Math.Sin(rx),0   }
                , {0,Math.Sin(rx),Math.Cos(rx),0    }
                , { 0,0,0,1}};
            double[,] arrayRy = new double[,] {
                { Math.Cos(ry),0,Math.Sin(ry),0   }
                , { 0,1,0,0}
                , { -Math.Sin(ry),0,Math.Cos(ry),0    }
                , { 0,0,0,1}};
            double[,] arrayRz = new double[,] {
                { Math.Cos(rz),-Math.Sin(rz),0,0}
                , { Math.Sin(rz),Math.Cos(rz),0,0    }
                , { 0,0,1,0}
                , { 0,0,0,1}};
            double[,] arrayOffset = new double[,]
            {
                {1,0,0,x },
                {0,1,0,y },
                {0,0,1,z },
                {0,0,0,1 },
            };
            var rotateRx = DenseMatrix.OfArray(arrayRx);
            var rotateRy = DenseMatrix.OfArray(arrayRy);
            var rotateRz = DenseMatrix.OfArray(arrayRz);
            var offset = DenseMatrix.OfArray(arrayOffset);
            return rotateRx * rotateRy * rotateRz * offset;
        }
        public static Vector<double> getParallel(Vector<double> v, Vector<double> refer)
        {
            return refer.Normalize(2) * v * refer.Normalize(2);
        }
        public static Vector<double> getVertical(Vector<double> v, Vector<double> refer)
        {
            return v - getParallel(v, refer);
        }
        public static Vector<double> getNormal(Vector<double> v1, Vector<double> v2)
        {
            return cross(v1, v2).Normalize(2);
        }
        public static Matrix<double> GetAnyVectorRotate(Vector<double> refer, double rotateRad)
        {
            var n = refer.Normalize(2);
            double nx = n.At(0);
            double ny = n.At(1);
            double nz = n.At(2);
            return DenseMatrix.OfArray(new double[,]
             {
                {nx*nx*(1-Math.Cos(rotateRad))+ Math.Cos(rotateRad)
                , nx*ny*(1-Math.Cos(rotateRad))+nz*Math.Sin(rotateRad)
                ,nx*nz*(1-Math.Cos(rotateRad))-ny*Math.Sin(rotateRad)
                 ,0},
                 {nx*ny*(1-Math.Cos(rotateRad))-nz* Math.Sin(rotateRad)
                ,ny*ny*(1-Math.Cos(rotateRad))+Math.Cos(rotateRad)
                ,ny*nz*(1-Math.Cos(rotateRad))+nx*Math.Sin(rotateRad)
                 ,0},
                 {nx*nz*(1-Math.Cos(rotateRad))+ny* Math.Sin(rotateRad)
                ,ny*nz*(1-Math.Cos(rotateRad))-nx*Math.Sin(rotateRad)
                ,nz*nz*(1-Math.Cos(rotateRad))+Math.Cos(rotateRad)
                 ,0},
                 {0,0,0,1 }
             }).Transpose();
        }
        public static Matrix<double> GetAnyVectorScale(Vector<double> refer, double k)
        {
            var n = refer.Normalize(2);
            double nx = n.At(0);
            double ny = n.At(1);
            double nz = n.At(2);
            return DenseMatrix.OfArray(new double[,]
            {
                {1+(k-1)* nx*nx,(k-1)*nx*ny,(k-1)*nx*nz },
                {(k-1)*nx*ny,1+(k-1)*ny*ny,(k-1)*ny*nz },
                {(k-1)*nx*nz,(k-1)*ny*nz,1+(k-1)*nz*nz },
            }).Transpose();
        }
        public static Matrix<double> GetAnyVectorProjection(Vector<double> refer)
        {
            var n = refer.Normalize(2);
            double nx = n.At(0);
            double ny = n.At(1);
            double nz = n.At(2);
            return DenseMatrix.OfArray(new double[,]
            {
                {1-nx*nx,-nx*ny,-nx*nz },
                {-nx*ny,1-ny*ny,-ny*nz },
                {-nx*nz,-ny*nz,1-nz*nz }
            }).Transpose();
        }
        public static Matrix<double> GetAnyVectorMirror(Vector<double> refer)
        {
            var n = refer.Normalize(2);
            double nx = n.At(0);
            double ny = n.At(1);
            double nz = n.At(2);
            return DenseMatrix.OfArray(new double[,]
            {
                {1-2*nx*nx,-2*nx*ny,-2*nx*nz },
                {-2*nx*ny,1-2*ny*ny,-2*ny*nz },
                {-2*nx*nz,-2*ny*nz,1-2*nz*nz }
            }).Transpose();
        }
        public static Vector<double> getVectorDivide(Vector<double> v, Vector<double> p, Vector<double> q, Vector<double> r)
        {
            return DenseVector.OfArray(new double[]
            {
                v* p,v*q,v*r
            });
        }
        public static double getVectorAngle(Vector<double> v, Vector<double> refer)
        {
            double vn = v.Norm(2);
            double rn = refer.Norm(2);
            // 零向量或范数接近零时无法计算夹角，直接返回 0
            if (vn < PosiHelper.MAX_DOUBLE_ERROR || rn < PosiHelper.MAX_DOUBLE_ERROR)
                return 0;
            double acos = (v * refer) / (vn * rn);
            if (acos < -1) acos += PosiHelper.MAX_DOUBLE_ERROR;
            else if (acos > 1) acos -= PosiHelper.MAX_DOUBLE_ERROR;
            // 钳位到 [-1, 1] 防止浮点误差导致 Math.Acos 返回 NaN
            acos = Math.Max(-1, Math.Min(1, acos));
            double rad = Math.Acos(acos);
            return AngleHelper.RadToAngle(rad, AngleHelper.AngleRange.Angle_N180_P180);
        }
        public static Vector<double> cross(Vector<double> left, Vector<double> right)
        {
            if ((left.Count != 3 || right.Count != 3))
            {
                string message = "Vectors must have a length of 3.";
                throw new Exception(message);
            }
            Vector<double> result = new DenseVector(3);
            result[0] = left[1] * right[2] - left[2] * right[1];
            result[1] = -left[0] * right[2] + left[2] * right[0];
            result[2] = left[0] * right[1] - left[1] * right[0];
            return result;
        }
    }
}
