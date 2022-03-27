using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Ross.Decals
{
    public class Projector
    {
        public Material material;

        public Vector3 position;
        public Quaternion rotation;
        public Vector2 size;
        public float depth;

        Matrix4x4 view;
        Matrix4x4 projection;
        Matrix4x4 localToWorld;

        public Projector(Vector3 position, Quaternion rotation, Vector2 size, float depth, Material material)
        {
            this.position = position;
            this.rotation = rotation;
            this.size = size;
            this.depth = depth;
            this.material = material;
        }

        /// <summary>
        /// Get view matrix for current position and rotation. Sets the matrix in material.
        /// </summary>
        public Matrix4x4 View
        {
            get
            {
                //view matrix is inverse of camera matrix. view matrix transforms point from world-space to view-space
                view = Matrix4x4.Inverse(Matrix4x4.TRS(position, rotation, new Vector3(1, 1, -1)));
                material.SetMatrix("_view", view);
                return view;
            }
        }

        /// <summary>
        /// Get projection matrix for current size and depth. Sets the matrix in material.
        /// </summary>
        public Matrix4x4 Projection
        {
            get
            {
                projection = Matrix4x4.Ortho(-size.x, size.x, -size.y, size.y, 0f, depth);
                material.SetMatrix("_projection", projection);
                return projection;
            }
        }

        public Matrix4x4 LocalToWorld
        {
            get
            {
                return Matrix4x4.TRS(position, rotation, Vector3.one);
            }
        }

        /// <summary>
        /// Draw projector at its current position and rotation. TODO: non-MonoBehaviour implementation.
        /// </summary>
        /// <param name="selected"></param>
        public void DrawProjector(bool selected)
        {
            var col = new Color(0.7f, 0.1f, 1f, 1.0f);
            col.a = selected ? 0.3f : 0.1f;
            Gizmos.color = col;
            Gizmos.matrix = LocalToWorld;
            Gizmos.DrawCube(Vector3.zero, new Vector3(size.x, size.y, depth));
            col.a = selected ? 0.5f : 0.2f;
            Gizmos.color = col;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(size.x, size.y, depth));
        }

    }
}
