/*
 * Copyright (c) 2013 LowLevelTech
 * Alex Dunn
 * llt@dunnalex.com
*/

using Fluidity.Renderer;
using UnityEngine;
using Fluidity.Auxilliary;

namespace Fluidity.Volume
{
    /// <summary>
    /// A basic volume, can be used to volume render a 3D texture of data.
    /// </summary>
    public class TextureVolume : BaseVolume
    {
        /// <summary>
        /// Volume texture to raymarch through.
        /// </summary>
        [SerializeField] private Texture3D volumeTexture;

        /// <summary>
        /// Volume texture to perform raymarching on.
        /// </summary>
        public override Texture VolumeTexture { get{ return volumeTexture; } }

        /// <summary>
        /// Dimensions of volume texture.
        /// </summary>
        protected override int3 VolumeDimensions { get { return new int3(volumeTexture.width, volumeTexture.height, volumeTexture.depth); } }
    }
}
