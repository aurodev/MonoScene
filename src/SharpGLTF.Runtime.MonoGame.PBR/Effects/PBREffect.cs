﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SharpGLTF.Runtime;

namespace SharpGLTF.Runtime.Effects
{
    public abstract class PBREffect : Effect, IEffectMatrices, IEffectBones, PBRLight.IEffect
    {
        #region lifecycle

        /// <summary>
        /// Creates a new AlphaTestEffect with default parameter settings.
        /// </summary>
        public PBREffect(GraphicsDevice device, byte[] effectCode) : base(device, effectCode)
        {

        }        

        #endregion        

        #region data

        private Matrix _World;
        private Matrix _View;
        private Matrix _Proj;

        private int _BoneCount;
        private readonly Matrix[] _Bones = new Matrix[128];

        private readonly PBRLight[] _Lights = new PBRLight[3];
        private readonly Vector4[] _LightParams0 = new Vector4[3];
        private readonly Vector4[] _LightParams1 = new Vector4[3];
        private readonly Vector4[] _LightParams2 = new Vector4[3];
        private readonly Vector4[] _LightParams3 = new Vector4[3];        

        private Vector4 _NormalScale = Vector4.One;
        private Texture2D _NormalMap;

        private Vector4 _EmissiveScale = Vector4.One;
        private Texture2D _EmissiveMap;

        private Vector4 _OcclusionScale = Vector4.One;
        private Texture2D _OcclusionMap; // this is the AO map

        #endregion

        #region properties - transform

        public int MaxBones => _Bones.Length;

        public int BoneCount => _BoneCount;

        public Matrix World
        {
            get => _World;
            set { _World = value; }
        }

        public Matrix View
        {
            get => _View;
            set { _View = value; }
        }

        public Matrix Projection
        {
            get => _Proj;
            set { _Proj = value; }
        }

        #endregion

        #region properties - lights

        public float Exposure { get; set; } = 1;

        public bool LightingEnabled { get; set; }

        public Vector3 AmbientLightColor { get; set; }

        public PBRLight GetLight(int index) => _Lights[index];

        public void SetLight(int index, PBRLight light) => _Lights[index] = light;

        #endregion

        #region properties - material        

        public Vector4 NormalScale { get => _NormalScale; set => _NormalScale = value; }

        public Texture2D NormalMap { get => _NormalMap; set => _NormalMap = value; }

        public Texture2D EmissiveMap { get => _EmissiveMap; set => _EmissiveMap = value; }

        public Vector4 OcclusionScale { get => _OcclusionScale; set => _OcclusionScale = value; }

        public Texture2D OcclusionMap { get => _OcclusionMap; set => _OcclusionMap = value; }

        #endregion

        #region API

        public void EnableDefaultLighting() { }

        // Note: setting boneCount to 0 disables Skinning
        public void SetBoneTransforms(Matrix[] boneTransforms, int boneStart, int boneCount)
        {
            _BoneCount = boneCount; if (_BoneCount == 0) return;
            Array.Copy(boneTransforms, boneStart, _Bones, 0, boneCount);
        }

        protected void ApplyPBR()
        {
            Parameters["World"].SetValue(World);
            Parameters["View"].SetValue(View);
            Parameters["Projection"].SetValue(Projection);
            if (_BoneCount > 0) Parameters["Bones"].SetValue(_Bones);

            Parameters["CameraPosition"].SetValue(-View.Translation);

            Resources.GenerateDotTextures(this.GraphicsDevice);

            Parameters["Exposure"].SetValue(this.Exposure);

            PBRLight.Encode(_Lights, _LightParams0, _LightParams1, _LightParams2, _LightParams3);
            Parameters["LightParam0"].SetValue(_LightParams0);
            Parameters["LightParam1"].SetValue(_LightParams1);
            Parameters["LightParam2"].SetValue(_LightParams2);
            Parameters["LightParam3"].SetValue(_LightParams3);

            if (_NormalMap != null)
            {
                Parameters["NormalScale"].SetValue(_NormalScale);
                Parameters["NormalTextureSampler+NormalTexture"].SetValue(_NormalMap ?? Resources.WhiteDotTexture);
            }

            if (_OcclusionMap != null)
            {
                Parameters["OcclusionScale"].SetValue(_OcclusionScale);
                Parameters["OcclusionTextureSampler+OcclusionTexture"].SetValue(_OcclusionMap ?? Resources.WhiteDotTexture);
            }
        }        

        #endregion
    }

    
}