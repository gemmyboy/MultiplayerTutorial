/*
 * Copyright (c) 2013 LowLevelTech
 * Alex Dunn
 * llt@dunnalex.com
*/

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fluidity.Auxilliary
{
    /// <summary>
    /// Simple helper object that is used to wrap up profile marker code using the 'using' keyword.
    /// 
    /// It's just to create neater, easier to read, code.
    /// </summary>
    public class ProfilerMarker : IDisposable
    {
        public ProfilerMarker(string name)
        {
            Profiler.BeginSample(name);
        }

        public void Dispose()
        {
            Profiler.EndSample();
        }
    }

    /// <summary>
    /// Mixed static helper class, includes all the custom helper functions used in Fluidity.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Generic wrapper for Unitys find object of type function (<see cref="Object.FindObjectsOfType(Type)"/>).
        /// </summary>
        /// <typeparam name="T">Type of object to find, must inherit from UnityEngine.Object (<see cref="UnityEngine.Object"/>).</typeparam>
        /// <returns>An array containing all the objects found matching the type specified.</returns>
        public static T[] FindObjectsOfType<T>()  where T : Object
        {
            T[] objects = Object.FindObjectsOfType(typeof (T)) as T[];
            return objects;
        }

        /// <summary>
        /// Extension: Sets the alpha of a colour and return a copy it.
        /// 
        /// Using this we can tidy up some inline code.  The original colour does not change, it is a struct.
        /// </summary>
        /// <param name="c">Input colour.</param>
        /// <param name="a">Desired alpha.</param>
        /// <returns>Copy of input colour with modified alpha value.</returns>
        public static Color SetAlpha(this Color c, float a)
        {
            return new Color(c.r, c.g, c.b, a);
        }

        /// <summary>
        /// Gizmo helper.
        /// 
        /// Draws a 3D arrow at a given position in a given direction.
        /// </summary>
        /// <param name="pos">Position to draw arrow in world space.</param>
        /// <param name="direction">Direction to orient arrow in world space.</param>
        public static void DrawArrow(Vector3 pos, Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.001f) return;

            const float headLength = 0.5f;
            const float tipLength = headLength * 0.2f;
            const float bodyLength = 2;
            const int step = 30;

            Vector3 directionNormalized = direction.normalized;
            Quaternion directionQuaternion = Quaternion.LookRotation(directionNormalized);
            Vector3 coneScale = new Vector3(0.2f, 0.2F, 1);

            Gizmos.DrawRay(pos, directionNormalized * bodyLength);

            for (int i = 0; i < 360 / step; i++)
            {
                Vector3 cone = directionQuaternion * Quaternion.Euler(0, 180, i * step) * coneScale;
                Gizmos.color = Color.white;
                Gizmos.DrawRay(pos + directionNormalized * bodyLength, cone * headLength);
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(pos + directionNormalized * bodyLength, cone * tipLength);
            }

            Gizmos.color = Color.white;
        }

        /// <summary>
        /// Creates a mesh object containing the data to render a cube.
        /// 
        /// The cube created is absolutly optimal and does not include normals.  
        /// This is used to render the stencil mesh for the fluid render mask.
        /// </summary>
        /// <returns>An optimal cube mesh.</returns>
        public static Mesh CreateCube()
        {
            Vector3[] v = new []
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3( 0.5f, -0.5f, -0.5f),
                new Vector3( 0.5f,  0.5f, -0.5f),
                new Vector3(-0.5f,  0.5f, -0.5f),

                new Vector3(-0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f,  0.5f,  0.5f),
                new Vector3(-0.5f,  0.5f,  0.5f)
            };

            Color[] c = new[]
            {
                new Color( 0, 0, 0, 1 ),
                new Color( 1, 0, 0, 1 ),
                new Color( 1, 1, 0, 1 ),
                new Color( 0, 1, 0, 1 ),

                new Color( 0, 0, 1, 1 ),
                new Color( 1, 0, 1, 0 ),
                new Color( 1, 1, 1, 1 ),
                new Color( 0, 1, 1, 1 )
            };

            int[] i = new []
            {
                3, 2, 1, 0,
                4, 5, 6, 7,
                0, 1, 5, 4,
                2, 3, 7, 6,
                4, 7, 3, 0,
                1, 2, 6, 5
            };

            Mesh mesh = new Mesh();
            mesh.Clear();

            mesh.name = "Fluidity Cube";
            mesh.vertices = v;
            mesh.colors = c;
            mesh.SetIndices(i, MeshTopology.Quads, 0);

            return mesh;
        }

        /// <summary>
        /// Helper function to pack a Vector4 into a single 32bit integer.
        /// </summary>
        /// <param name="vValue">Value to pack.</param>
        /// <returns>Vector4 squeezed into a 32bit integer.</returns>
        public static int PackFloat4IntoInt(Vector4 vValue)
        {
            return (((int)(vValue.x * 255)) << 24) | (((int)(vValue.y * 255)) << 16) | (((int)(vValue.z * 255)) << 8) | (int)(vValue.w * 255);
        }

    }
}