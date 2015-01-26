/*
 * Copyright (c) 2013 LowLevelTech
 * Alex Dunn
 * llt@dunnalex.com
*/

using UnityEngine;

namespace Fluidity.Auxilliary
{
    /// <summary>
    /// Storage container that holds and calculates all the CPU side dimension data used by the shaders.
    /// Storing these values CPU side saves GPU cost of calculating them on the fly.
    /// </summary>
    internal class DimensionData
    {
        public int3 Dimensions                { get; private set; }
        public int3 DimensionsMinusOne        { get; private set; }
        public Vector4 RecipDimensions          { get; private set; }
        public Vector4 DimensionsAspectRatio    { get; private set; }
        public Vector4 HalfVoxel                { get; private set; }
        public float MaxDimension               { get; private set; }
        public float RecipMaxDimension          { get; private set; }

        /// <summary>
        /// Builds set of dimension data from initial dimensions.
        /// </summary>
        /// <param name="maxDimensions">Initial dimension data.</param>
        public void RecalculateData(int3 maxDimensions)
        {
            maxDimensions = int3.FloorTo4(maxDimensions);
            Dimensions = maxDimensions;
            DimensionsMinusOne = new int3(maxDimensions.x-1, maxDimensions.y-1, maxDimensions.z-1);
            RecipDimensions = new Vector4(1.0f/maxDimensions.x, 1.0f/maxDimensions.y, 1.0f/maxDimensions.z, 0.0f);
            MaxDimension = Mathf.Max(maxDimensions.x, Mathf.Max(maxDimensions.z, maxDimensions.y));
            RecipMaxDimension = 1.0f/MaxDimension;
            DimensionsAspectRatio = (Vector4)Dimensions*RecipMaxDimension;
            HalfVoxel = RecipDimensions*0.5f;
        }
    }
}