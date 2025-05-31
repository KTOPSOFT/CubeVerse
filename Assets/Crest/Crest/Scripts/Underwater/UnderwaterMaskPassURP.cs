// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

#if CREST_URP

namespace Crest
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    partial class UnderwaterMaskPassURP : ScriptableRenderPass
    {
        const string PassName = "Ocean Mask";
        const string k_ShaderPathOceanMask = "Hidden/Crest/Underwater/Ocean Mask URP";

        readonly PropertyWrapperMaterial _oceanMaskMaterial;

        static int s_InstanceCount;
        UnderwaterRenderer _underwaterRenderer;

#if UNITY_6000_0_OR_NEWER
        RTHandle _maskRT;
        RTHandle _depthRT;
        RTHandle _volumeFrontFaceRT;
        RTHandle _volumeBackFaceRT;

        // Fixes a bug with Unity, as we should not have to do this ourselves.
        // Only required under the following conditions:
        // Camera > URP Dynamic Resolution = checked
        // URP Asset > Anti Aliasing (MSAA) = unchecked
        // URP Asset > Upscale Filter = Spatial-Temporal Post-Processing
        static void ScaleViewport(Camera camera, CommandBuffer buffer, RTHandle handle)
        {
            // Causes problems if we continue when this is checked.
            if (camera.allowDynamicResolution) return;

            var size = handle.GetScaledSize(handle.rtHandleProperties.currentViewportSize);
            if (size == Vector2Int.zero) return;
            buffer.SetViewport(new(0f, 0f, size.x, size.y));
        }
#endif

        public UnderwaterMaskPassURP()
        {
            // Will always execute and matrices will be ready.
#if UNITY_2021_3_OR_NEWER
            renderPassEvent = RenderPassEvent.BeforeRenderingPrePasses;
#else
            renderPassEvent = RenderPassEvent.BeforeRenderingPrepasses;
#endif
            _oceanMaskMaterial = new PropertyWrapperMaterial(k_ShaderPathOceanMask);
            _oceanMaskMaterial.material.hideFlags = HideFlags.HideAndDontSave;
        }

        internal void CleanUp()
        {
            CoreUtils.Destroy(_oceanMaskMaterial.material);
        }

        public void Enable(UnderwaterRenderer underwaterRenderer)
        {
            s_InstanceCount++;
            _underwaterRenderer = underwaterRenderer;

#if UNITY_6000_0_OR_NEWER
            _underwaterRenderer.SetUpMaterials();
            _underwaterRenderer.SetUpFixMaskArtefactsShader();
#else
            _underwaterRenderer.OnEnableMask();
#endif

            RenderPipelineManager.beginCameraRendering -= EnqueuePass;
            RenderPipelineManager.beginCameraRendering += EnqueuePass;
        }

        public void Disable()
        {
#if UNITY_6000_0_OR_NEWER
            _volumeFrontFaceRT?.Release();
            _volumeBackFaceRT?.Release();
            _maskRT?.Release();
            _depthRT?.Release();
#else
            _underwaterRenderer.OnDisableMask();
#endif

            if (--s_InstanceCount <= 0)
            {
                RenderPipelineManager.beginCameraRendering -= EnqueuePass;
            }
        }

        static void EnqueuePass(ScriptableRenderContext context, Camera camera)
        {
            var ur = UnderwaterRenderer.Get(camera);

            if (!ur || !ur.IsActive)
            {
                return;
            }

            if (!Helpers.MaskIncludesLayer(camera.cullingMask, OceanRenderer.Instance.Layer))
            {
                return;
            }

            // Enqueue the pass. This happens every frame.
            camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(ur._urpMaskPass);
        }

#if UNITY_2023_3_OR_NEWER
        void OnSetup(CommandBuffer buffer, PassData renderingData)
#else
        public override void OnCameraSetup(CommandBuffer buffer, ref RenderingData renderingData)
#endif
        {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            // Keywords and other things.
            _underwaterRenderer.SetUpVolume(_oceanMaskMaterial.material);

#if UNITY_6000_0_OR_NEWER
            {
                descriptor = _underwaterRenderer.GetMaskColorRTD(descriptor);
                _maskRT ??= RTHandles.Alloc(descriptor);
                RenderingUtils.ReAllocateHandleIfNeeded(ref _maskRT, descriptor);
                _underwaterRenderer._maskTarget = new(_maskRT, mipLevel: 0, CubemapFace.Unknown, depthSlice: -1);

                descriptor = _underwaterRenderer.GetMaskDepthRTD(descriptor);
                _depthRT ??= RTHandles.Alloc(descriptor);
                RenderingUtils.ReAllocateHandleIfNeeded(ref _depthRT, descriptor);
                _underwaterRenderer._depthTarget = new(_depthRT, mipLevel: 0, CubemapFace.Unknown, depthSlice: -1);

                if (_underwaterRenderer._mode != UnderwaterRenderer.Mode.FullScreen && _underwaterRenderer._volumeGeometry != null)
                {
                    _volumeFrontFaceRT ??= RTHandles.Alloc(descriptor);
                    RenderingUtils.ReAllocateHandleIfNeeded(ref _volumeFrontFaceRT, descriptor);
                    _underwaterRenderer._volumeFrontFaceTarget = new(_volumeFrontFaceRT, mipLevel: 0, CubemapFace.Unknown, depthSlice: -1);

                    if (_underwaterRenderer._mode == UnderwaterRenderer.Mode.Volume || _underwaterRenderer._mode == UnderwaterRenderer.Mode.VolumeFlyThrough)
                    {
                        _volumeBackFaceRT ??= RTHandles.Alloc(descriptor);
                        RenderingUtils.ReAllocateHandleIfNeeded(ref _volumeBackFaceRT, descriptor);
                        _underwaterRenderer._volumeBackFaceTarget = new(_volumeBackFaceRT, mipLevel: 0, CubemapFace.Unknown, depthSlice: -1);
                    }
                }
            }
#else
            _underwaterRenderer.SetUpMaskTextures(descriptor);

            if (_underwaterRenderer._mode != UnderwaterRenderer.Mode.FullScreen && _underwaterRenderer._volumeGeometry != null)
            {
                _underwaterRenderer.SetUpVolumeTextures(descriptor);
            }
#endif
        }

#if UNITY_2023_3_OR_NEWER
        void ExecutePass(ScriptableRenderContext context, CommandBuffer commandBuffer, PassData renderingData)
#else
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
#endif
        {
            var camera = renderingData.cameraData.camera;

            XRHelpers.Update(camera);
            XRHelpers.UpdatePassIndex(ref UnderwaterRenderer.s_xrPassIndex);

#if !UNITY_2023_3_OR_NEWER
            CommandBuffer commandBuffer = CommandBufferPool.Get(PassName);
#endif

            // Populate water volume before mask so we can use the stencil.
            if (_underwaterRenderer._mode != UnderwaterRenderer.Mode.FullScreen && _underwaterRenderer._volumeGeometry != null)
            {
#if UNITY_6000_0_OR_NEWER
                CoreUtils.SetRenderTarget(commandBuffer, _volumeFrontFaceRT);
                ScaleViewport(camera, commandBuffer, _volumeFrontFaceRT);
#endif
                _underwaterRenderer.PopulateVolumeFront(commandBuffer, _underwaterRenderer._volumeFrontFaceTarget, _underwaterRenderer._volumeBackFaceTarget);

#if UNITY_6000_0_OR_NEWER
                CoreUtils.SetRenderTarget(commandBuffer, _volumeBackFaceRT);
                ScaleViewport(camera, commandBuffer, _volumeBackFaceRT);
#endif
                _underwaterRenderer.PopulateVolumeBack(commandBuffer, _underwaterRenderer._volumeFrontFaceTarget, _underwaterRenderer._volumeBackFaceTarget);

                // Copy only the stencil by copying everything and clearing depth.
                commandBuffer.CopyTexture(_underwaterRenderer._mode == UnderwaterRenderer.Mode.Portal ? _underwaterRenderer._volumeFrontFaceTarget : _underwaterRenderer._volumeBackFaceTarget, _underwaterRenderer._depthTarget);

#if UNITY_6000_0_OR_NEWER
                CoreUtils.SetRenderTarget(commandBuffer, _underwaterRenderer._depthTarget);
                CoreUtils.ClearRenderTarget(commandBuffer, ClearFlag.Depth, Color.clear);
#else
                Helpers.Blit(commandBuffer, _underwaterRenderer._depthTarget, Helpers.UtilityMaterial, (int)Helpers.UtilityPass.ClearDepth);
#endif
            }

#if UNITY_6000_0_OR_NEWER
            CoreUtils.SetRenderTarget(commandBuffer, _maskRT, _depthRT);
            ScaleViewport(camera, commandBuffer, _maskRT);
#endif

            _underwaterRenderer.SetUpMask(commandBuffer, _underwaterRenderer._maskTarget, _underwaterRenderer._depthTarget);
            UnderwaterRenderer.PopulateOceanMask(
                commandBuffer,
                camera,
                OceanRenderer.Instance.Tiles,
                _underwaterRenderer._cameraFrustumPlanes,
                _oceanMaskMaterial.material,
                _underwaterRenderer._farPlaneMultiplier,
                _underwaterRenderer.EnableShaderAPI,
                _underwaterRenderer._debug._disableOceanMask
            );

            _underwaterRenderer.FixMaskArtefacts
            (
                commandBuffer,
                renderingData.cameraData.cameraTargetDescriptor,
                _underwaterRenderer._maskTarget
            );

#if !UNITY_2023_3_OR_NEWER
            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
#endif
        }
    }
}

#endif // CREST_URP
