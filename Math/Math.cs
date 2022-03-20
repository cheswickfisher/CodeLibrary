using System;
using UnityEngine;

namespace Ross.Math
{
        public class Matrix
        {
            public int Rows { get { return rows; } }
            public int Columns { get { return columns; } }
            public float[,] matrix { get { return _matrix; } }

            private int rows;
            private int columns;
            private float[,] _matrix;

            public Matrix(int rows, int columns)
            {
                this.rows = rows;
                this.columns = columns;
                _matrix = new float[rows, columns];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        _matrix[i, j] = 0;
                    }
                }
            }

            public void SetRow(float[] row, int rowIndex)
            {
                if (row.Length != columns) { Debug.LogError("row lengths don't match"); return; }
                for (int i = 0; i < columns; i++)
                {
                    _matrix[rowIndex, i] = row[i];
                }
            }

            public float[] GetRow(int rowIndex)
            {
                if(rowIndex > rows || rowIndex < 0) { throw new ArgumentException("invalid rowIndex."); }

                float[] row = new float[columns];
                for (int j = 0; j < columns; j++)
                {
                    row[j] = _matrix[rowIndex, j];
                }
                return row;
            }

            public void SetColumn(float[] column, int columnIndex)
            {
                if (column.Length != rows) { Debug.LogError("column lengths don't match"); return; }

                for (int i = 0; i < rows; i++)
                {
                    _matrix[i, columnIndex] = column[i];
                }
            }

            public float[] GetColumn(int columnIndex)
            {
            if (columnIndex > columns || columnIndex < 0) { throw new ArgumentException("invalid columnIndex."); }

            float[] column = new float[rows];
                for (int i = 0; i < rows; i++)
                {
                    column[i] = _matrix[i, columnIndex];
                }
                return column;
            }

            public static Matrix operator +(Matrix A, Matrix B)
            {
                if (A.rows != B.rows || A.columns != B.columns) { throw new ArgumentException("matrix dimensions don't match"); }

                Matrix C = new Matrix(A.rows, B.rows);

                for (int i = 0; i < A.rows; i++)
                {
                    for (int j = 0; j < A.columns; j++)
                    {
                        C._matrix[i, j] = A._matrix[i, j] + B._matrix[i, j];
                    }
                }

                return C;
            }


            public static Matrix operator -(Matrix A, Matrix B)
            {
                if (A.rows != B.rows || A.columns != B.columns) { throw new ArgumentException("matrix dimensions don't match"); }

                Matrix C = new Matrix(A.rows, B.rows);

                for (int i = 0; i < A.rows; i++)
                {
                    for (int j = 0; j < A.columns; j++)
                    {
                        C._matrix[i, j] = A._matrix[i, j] - B._matrix[i, j];
                    }
                }

                return C;
            }

            public static Matrix operator *(Matrix A, Matrix B)
            {
                if (A.columns != B.rows) { throw new ArgumentException("incompatible matrices");}

                Matrix C = new Matrix(A.rows, B.columns);

                for (int m = 0; m < A.rows; m++)
                {
                    for (int j = 0; j < B.columns; j++)
                    {
                        float Cmj = 0.0f;
                        for (int i = 0; i < B.rows; i++)
                        {
                            Cmj += A._matrix[m, i] * B._matrix[i, j];
                        }
                        C._matrix[m, j] = Cmj;
                    }
                }

                return C;
            }
        }

        public struct Plane
        {
            /// <summary>
            /// startpoint of the plane
            /// </summary>
            public Vector3 S { get; }

            public Vector3 normal { get; }

            public Plane(Vector3 normal, Vector3 S)
            {
                this.normal = normal;
                this.S = S;
            }
        }

        public struct Line
        {
            /// <summary>
            /// direction
            /// </summary>
            public Vector3 d { get; }
            /// <summary>
            /// starting point
            /// </summary>
            public Vector3 S { get; }
            /// <summary>
            /// scalar component
            /// </summary>
            /// <param name="d"></param>
            /// <param name="S"></param>
            public float t { get; set; }

            public Line(Vector3 d, Vector3 S, float t)
            {
                this.d = d;
                this.S = S;
                this.t = t;
            }
        }

        public static class MathFunctions
        {
        #region Basic Linear Algebra
            public static float Length(Vector3 v)
            {
                return (float)System.Math.Sqrt(Dot(v, v));
            }

            public static float SquaredLength(Vector3 v)
            {
                return Dot(v, v);
            }

            public static float Dot(Vector3 u, Vector3 v)
            {
                return (u.x * v.x + u.y * v.y + u.z * v.z);
            }

            public static Vector3 Cross(Vector3 u, Vector3 v)
            {
                return new Vector3(u.y * v.z - u.z * v.y, u.z * v.x - u.x * v.z, u.x * v.y - u.y * v.x);
            }

            /// <summary>
            /// returns smallest angle between u and v
            /// </summary>
            /// <param name="u"></param>
            /// <param name="v"></param>
            /// <returns></returns>
            public static float AngleBetween(Vector3 u, Vector3 v)
            {
                //derived from immersivemath.com, ch3, example 3.4

                float d = Dot(u, v);
                float lu = Length(u);
                float lv = Length(v);
                float cos = d / (lu * lv);
                return (float)System.Math.Acos(cos); //angle
            }

            public static Vector3 Normalize(Vector3 v)
            {
                return v / Length(v);
            }

            /// <summary>
            /// assumes line is not normalized
            /// </summary>
            /// <param name="u"></param>
            /// <param name="line"></param>
            /// <returns></returns>
            public static Vector3 ProjectOnLine(Vector3 u, Vector3 line)
            {
                float lenLine = Length(line);
                return Dot(u, line) / (lenLine * lenLine) * line;
            }

            /// <summary>
            /// projects along the plane normal
            /// </summary>
            /// <param name="v"></param>
            /// <param name="p"></param>
            /// <returns></returns>
            public static Vector3 ProjectOnPlane(Vector3 v, Plane p)
            {
                float eP = SignedDistanceFunction(p.normal, v, p.S);
                int sign = System.Math.Sign(eP);
                Vector3 d = p.normal * -sign;

                float ndotd = Dot(p.normal, v - p.S);
                if (ndotd <= 0.001) { return Vector3.positiveInfinity; }

                float t = Dot(p.normal, p.S - v) / ndotd;

                return v + t * d;
            }

            public static Vector3 ProjectOnPlane(Vector3 directionVector, Vector3 planeNormal)
            {
                Vector3 n = planeNormal.normalized;
                Vector3 b = directionVector.normalized;
                //use planeNormal in dot product or n?
                return b - (Dot(planeNormal, b) * n).normalized;
            }

            /// <summary>
            /// projects along direction provided
            /// </summary>
            /// <param name="v"></param>
            /// <param name="direction"></param>
            /// <param name="p"></param>
            /// <param name="t"> here if i need it. it's the scalar value that gets multiplied by the direction and then added to v in order to perform the projection</param>
            /// <returns></returns>
            public static Vector3 ProjectOnPlane(Vector3 v, ref Vector3 direction, Plane p, ref float t)
            {
                float eP = SignedDistanceFunction(p.normal, v, p.S);
                int sign = System.Math.Sign(eP);
                direction *= -sign;

                float ndotd = Dot(p.normal, direction);
                if (ndotd <= 0.001) { return v; }

                t = Dot(p.normal, p.S - v) / ndotd;
                Vector3 intersectionPt = v + t * direction;

                return intersectionPt;
            }

            public static Vector3 Reflect(Vector3 v, Vector3 direction, Plane p, float reflectionStrength)
            {
                float t = 1.0f;
                Vector3 intersectionPt = ProjectOnPlane(v, ref direction, p, ref t);

                if (intersectionPt == v) { return v; }

                Vector3 i = intersectionPt + t * direction;
                Vector3 z = i - intersectionPt;
                //calculate z projected onto n. z is i positioned at world origin so it can be accurately projected onto the normal.
                Vector3 Pni = Vector3.Dot(p.normal, z) * p.normal;
                Vector3 r = i - 2 * Pni;
                Vector3 rr = intersectionPt + Normalize(r - intersectionPt) * reflectionStrength;

                return rr;
            }
        

            public static float SignedDistanceFunction(Vector3 normal, Vector3 point, Vector3 startPoint)
            {
                return Dot(Normalize(normal), point - startPoint);
            }
        #endregion

        #region Quaternion Helper Methods
        public static Vector3 RotateVector(Quaternion rotation, Vector3 v)
        {
            return rotation * v;
        }

        ///<summary>Create an orthonormal basis from a forward vector and return the up vector of that basis.
        ///Mainly used with Quaternion.LookRotation to calculate the up vector3 parameter.
        ///See CreateRotationMatrix3x3 for use of this technique with matrices, as well as for additional info.</summary>
        public static Vector3 GetUpFromForward(Vector3 forward)
        {            
            Vector3 right = Vector3.Cross(forward, Vector3.up).normalized;
            Vector3 up = Vector3.Cross(right, forward).normalized;

            return up;
        }
        /// <summary>
        /// !!UVToTransform AND newBasisForwardUV are assumed to be normalized before inputting them into this function!!
        /// Creates an orthonormal basis using newBasisForwardUV as the z axis.
        /// transforms UVToTransform into that basis.
        /// returns the resulting unit vector.
        /// </summary>
        public static Vector3 TransformUVFrameOfReference(Vector3 UVToTransform, Vector3 newBasisForwardUV)
        {
            Quaternion rot = Quaternion.LookRotation(newBasisForwardUV, GetUpFromForward(newBasisForwardUV));
            return rot * UVToTransform;
        }

        #endregion

        #region Ellipse

        /// <summary>
        /// returns a Vector3 point along the curve of an ellipse.
        /// semiMajor is half the long axis. semiMinor is half the short axis. they define the shape of the ellipse.
        /// radians defines where on that ellipse the point is located.
        /// </summary>
        /// <param name="semiMajor"></param>
        /// <param name="semiMinor"></param>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static Vector3 ParametricEllipseFunction(float semiMajor, float semiMinor, float radians)
        {
            return new Vector3()
            {
                x = semiMajor * Mathf.Cos(radians),
                y = semiMinor * Mathf.Sin(radians),
                z = 0
            };
        }

        /// <summary>
        /// are the supplied x,y coords within the ellipse defined by semiMajor and semiMinor?
        /// if returned value is less than or equal to 1.0f, then yes. otherwise, no. result will never be negative.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="semiMajor"></param>
        /// <param name="semiMinor"></param>
        /// <returns></returns>
        public static float CheckPointAgainstEllipse(float x, float y, float semiMajor, float semiMinor)
        {
            float e = x * x / (semiMajor * semiMajor) + y * y / (semiMinor * semiMinor);
            return e;
        }

        /// <summary>
        /// used to visualize an ellipse in the editor window.
        /// wsPosition is where the center of the ellipse is located in worldspace.
        /// rotation is the worldspace rotation of the ellipse.
        /// semiMajor is half the long axis of the ellipse. semiMinor is half the short axis of the ellipse.
        /// </summary>
        /// <param name="semiMajor"></param>
        /// <param name="semiMinor"></param>
        /// <param name="wsPosition"></param>
        /// <param name="rotation"></param>
        public static void DrawDebugEllipse(float semiMajor, float semiMinor, Vector3 wsPosition, Quaternion rotation)
        {
            float step = 60;
            float ts = 2 * Mathf.PI;
            float r = ts / step;
            float s = 0;
            Vector3 ellipse = ParametricEllipseFunction(semiMajor, semiMinor, 0.0f);

            ellipse = wsPosition + rotation * ellipse;

            while (s < ts)
            {
                s += r;
                Vector3 prevPoint = ellipse;
                ellipse = ParametricEllipseFunction(semiMajor, semiMinor, s);
                ellipse = wsPosition + rotation * ellipse;

                Debug.DrawLine(prevPoint, ellipse, Color.red, 10.0f);
            }
        }


        /// <summary>
        /// returns the point on the ellipse(defined by semiMajor/semiMinor) that is the closest to p.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="semiMajor"></param>
        /// <param name="semiMinor"></param>
        /// <returns></returns>
        public static Vector2 NearestPointOnEllipse(Vector2 p, float semiMajor, float semiMinor)
        {
            ///p is the point you want placed on the ellipse
            float px = Mathf.Abs(p.x);
            float py = Mathf.Abs(p.y);

            ///this is an optimization. it's equivalent to sin(45) and cos(45). or in radians pi/4.
            float tx = 0.7071067f;
            float ty = 0.7071067f;

            float x, y, ex, ey, rx, ry, qx, qy, r, q, t = 0;

            for (int i = 0; i < 3; i++)
            {
                ///the parametric form of an ellipse is:
                ///x = semiMajor * sin(t)
                ///y = semiMinor * cos(t)
                ///
                ///semiMajor and semiMinor are the x & y axes.
                x = semiMajor * tx;
                y = semiMinor * ty;

                ///e is the evolute of the ellipse.
                ///the equation is the parametric form of the evolute.
                ///the evolute will give you the center point of the circle that you will use to approximate the curvature of a section of the ellipse.
                ex = (semiMajor * semiMajor - semiMinor * semiMinor) * Mathf.Pow(tx, 3) / semiMajor;
                ey = (semiMinor * semiMinor - semiMajor * semiMajor) * Mathf.Pow(ty, 3) / semiMinor;

                ///line from the ellipse point to the evolute.
                rx = x - ex;
                ry = y - ey;

                ///line from the query point to the evolute.
                qx = px - ex;
                qy = py - ey;

                ///magnitudes of r and q
                r = Mathf.Sqrt(rx * rx + ry * ry);
                q = Mathf.Sqrt(qx * qx + qy * qy);

                ///create a vector extending from the evolute in the direction of the query point. it's magnitude is equal to r.
                ///IE: (q * |r|/|q| + e) , where q/|q| is a unit a vector in the direction of q
                ///divide the vector's x & y components by semiMajor & semiMinor axes.
                tx = Mathf.Min(1, Mathf.Max(0, (qx * r / q + ex) / semiMajor));
                ty = Mathf.Min(1, Mathf.Max(0, (qy * r / q + ey) / semiMinor));

                ///magnitude
                t = Mathf.Sqrt(tx * tx + ty * ty);

                ///normalize the vector t.
                tx /= t;
                ty /= t;
            }
            ///return the new point on the ellipse
            return new Vector2((semiMajor * (p.x < 0 ? -tx : tx)), (semiMinor * (p.y < 0 ? -ty : ty)));
        }


        #endregion

        #region Matrices

        ///creates an orthonormal basis from a reference direction.
        ///multiply this matrix by a direction in world space to transform that direction into this basis.
        ///i.e. it will be the same direction, but now it will be oriented according to whatever the rotation of the new orthonormal basis created in this method is.
        ///see: https://github.com/FedUni/caliko/blob/9953ac3e6e95c271ad099d4d271568f391e7c2ad/caliko/src/main/java/au/edu/federation/utils/Mat3f.java#L151
        ///also see: https://graphics.pixar.com/library/OrthonormalB/paper.pdf
        public static Matrix CreateRotationMatrix3x3(Vector3 referenceDirection)
        {
            Matrix rotMat = new Matrix(3, 3);

            //deal with singularity issue
            if(Mathf.Abs(referenceDirection.y) > 0.9999f)
            {
                Vector3 z = Normalize(referenceDirection);
                float[] zBasis = new float[3] { z.x, z.y, z.z };
                Vector3 x = new Vector3(1.0f, 0.0f, 0.0f);
                float[] xBasis = new float[3] { x.x, x.y, x.z };
                Vector3 y = Normalize(Cross(x, z));
                float[] yBasis = new float[3] { y.x, y.y, y.z };

                rotMat.SetColumn(xBasis, 0);
                rotMat.SetColumn(yBasis, 1);
                rotMat.SetColumn(zBasis, 2);
            }

            else
            {
                Vector3 z = Normalize(referenceDirection);
                float[] zBasis = new float[3] { z.x, z.y, z.z };
                Vector3 x = Normalize(Cross(referenceDirection, new Vector3(0.0f, 1.0f, 0.0f)));
                float[] xBasis = new float[3] { x.x, x.y, x.z };
                Vector3 y = Normalize(Cross(x, z));
                float[] yBasis = new float[3] { y.x, y.y, y.z };

                rotMat.SetColumn(xBasis, 0);
                rotMat.SetColumn(yBasis, 1);
                rotMat.SetColumn(zBasis, 2);
            }

            return rotMat;
        }

        #endregion

        /// <summary>
        /// use this when you want to force one vector (vectorToConstrain) to be within certain degrees (constraintAngleDegs) of another vector (baseVector).
        /// </summary>
        /// <param name="baseVector"></param>
        /// <param name="vectorToConstrain"></param>
        /// <param name="constraintAngleDegs"></param>
        /// <returns></returns>
        /// EXPLANATION:
        /// normalizes baseVector and vectorToConstrain so that they are Unit Vectors. 
        /// gets the angle between them. 
        /// if the angle is greater than constraintAngleDegs, then baseVector is rotated by 
        /// constraintAngleDegs along the axis that is the normalized cross product of baseVector and vectorToConstrain.
        /// this axis ensures that the new vector will be in the same plane as the previous 2.
        /// vectorToConstrain is set to equal this vector. 
        /// returns the new vectorToConstrain.

        public static Vector3 GetUnitVectorConstrainedToAngleDegs(Vector3 baseVector, Vector3 vectorToConstrain, float constraintAngleDegs)
            {
                Vector3 baseVectorUV = Normalize(baseVector);
                Vector3 vectorToConstraintUV = Normalize(vectorToConstrain);

                float angleBetweenDegs = AngleBetween(baseVectorUV, vectorToConstraintUV);

                if(angleBetweenDegs > constraintAngleDegs)
                {
                    Vector3 correctionAxis = Normalize(Cross(baseVectorUV, vectorToConstraintUV));
                    Quaternion rot = Quaternion.AngleAxis(constraintAngleDegs, correctionAxis);
                    vectorToConstraintUV = RotateVector(rot, baseVectorUV);                    
                }

                return vectorToConstraintUV;
            }
        }
}