using UnityEngine;

namespace Fluidity.Auxilliary
{
    /// <summary>
    /// Container class for a trio of integers.  Similar to int3/uint3 in GPU land.
    /// </summary>
    [System.Serializable]
    public class int3
    {
        public int x, y, z;

        public int3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static int3 operator *(int3 i, int f)
        {
            return new int3(i.x*f, i.y*f, i.z*f);
        }

        public static int3 operator /(int3 i, int f)
        {
            return new int3(i.x/f, i.y/f, i.z/f);
        }

        public static implicit operator Vector3(int3 i)
        {
            return new Vector3(i.x, i.y, i.z);
        }

        public static implicit operator Vector4(int3 i)
        {
            return new Vector3(i.x, i.y, i.z);
        }

        public static int3 FloorTo4(int3 i)
        {
            return new int3(i.x - (i.x%4), i.y - (i.y%4), i.z - (i.z%4));
        }
    }
}