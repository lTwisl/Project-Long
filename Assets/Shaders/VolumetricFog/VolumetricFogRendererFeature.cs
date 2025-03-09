using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VolumetricFog
{
    public interface IVolumetricFog
    {
        int GetDownsampleLevel();
        void SetDownsampleLevel(int downsampleLevel);
    }

    public class VolumetricFogRendererFeature : ScriptableRendererFeature, IVolumetricFog
    {
        [Serializable]
        public enum RenderTextureQuality
        {
            Low,
            Medium,
            High
        }

        [Serializable]
        public class Settings
        {
            [Tooltip("Даунсеплинг качества тумана"), Range(1, 8)]
            public int fogDownsampleLevel = 4;

            [Space]
            [Tooltip("Материал рассчета тумана")]
            public Material fogMaterial;

            [Tooltip("Материал рассчета глубины сцены")]
            public Material depthMaterial;

            [Space]
            [Tooltip("Материал рассчета композиции сцены")]
            public Material compositeMaterial;

            [Space]
            public string compositeMaterialColourTextureName = "_ColourTexture";
            public string compositeMaterialFogTextureName = "_FogTexture";
            public string compositeMaterialDepthTextureName = "_DepthTexture";

            [Space]
            [Tooltip("Качество(формат) рендер текстуры")]
            public RenderTextureQuality renderTextureQuality = RenderTextureQuality.High;
        }

        [Tooltip("Отображать туман в сцене?")]
        public bool renderInSceneView = true;

        [Tooltip("Очередность прохода отрисовки")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;

        [Space] public Settings settings;
        private CustomRenderPass customRenderPass;

        public int GetDownsampleLevel() => settings?.fogDownsampleLevel ?? 4;

        public void SetDownsampleLevel(int downsampleLevel)
        {
            if (settings != null) settings.fogDownsampleLevel = downsampleLevel;
        }

        public override void Create()
        {
            if (settings == null ||
                settings.fogMaterial == null ||
                settings.depthMaterial == null ||
                settings.compositeMaterial == null)
            {
                Debug.LogError("VolumetricFog: Settings or materials not assigned!");
                return;
            }
            customRenderPass = new CustomRenderPass(settings) { renderPassEvent = renderPassEvent };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (customRenderPass == null || settings == null) return;

            var cameraType = renderingData.cameraData.cameraType;
            bool shouldRender = cameraType == CameraType.Game
                             || cameraType == CameraType.Reflection
                             || (renderInSceneView && cameraType == CameraType.SceneView);

            if (shouldRender) renderer.EnqueuePass(customRenderPass);
        }

        protected override void Dispose(bool disposing) => customRenderPass?.Dispose();

        private class CustomRenderPass : ScriptableRenderPass
        {
            private readonly Settings settings;
            private RTHandle colourHandle;
            private RTHandle fogHandle;
            private RTHandle depthHandle;

            public CustomRenderPass(Settings settings)
            {
                if (settings == null) throw new ArgumentNullException(nameof(settings));
                this.settings = settings;
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraDesc)
            {
                if (settings == null) return;

                // Оптимизация: кэширование вычислений для уменьшения повторных операций.
                int width = cameraDesc.width;
                int height = cameraDesc.height;
                int downsample = Mathf.Clamp(settings.fogDownsampleLevel, 1, 8); // Ограничение диапазона.

                // Использование масштабирования через RTHandle для эффективного даунсэмплинга.
                Vector2Int fogResolution = new Vector2Int(
                    Mathf.Max(1, width / downsample),
                    Mathf.Max(1, height / downsample)
                );

                // Упрощенное создание дескрипторов с использованием метода Copy.
                RenderTextureDescriptor colorDesc = cameraDesc;
                colorDesc.colorFormat = GetRenderTextureFormat(settings.renderTextureQuality);
                colorDesc.depthBufferBits = 0;
                colorDesc.msaaSamples = 1;

                RenderTextureDescriptor fogDesc = colorDesc;
                fogDesc.width = fogResolution.x;
                fogDesc.height = fogResolution.y;

                // Глубина использует RFloat и то же разрешение, что и туман.
                RenderTextureDescriptor depthDesc = fogDesc;
                depthDesc.colorFormat = RenderTextureFormat.RFloat;

                // Пересоздание RTHandle только при изменении параметров.
                RecreateRTHandle(ref colourHandle, colorDesc, "_VolumetricColor");
                RecreateRTHandle(ref fogHandle, fogDesc, "_VolumetricFog");
                RecreateRTHandle(ref depthHandle, depthDesc, "_VolumetricDepth");
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData data)
            {
                if (!ValidateResources()) return;

                var cmd = CommandBufferPool.Get("VolumetricFog");
                try
                {
                    var cameraTarget = data.cameraData.renderer.cameraColorTargetHandle;

                    // Основные пассы рендера
                    Blit(cmd, cameraTarget, colourHandle);
                    Blit(cmd, cameraTarget, depthHandle, settings.depthMaterial);
                    Blit(cmd, cameraTarget, fogHandle, settings.fogMaterial);

                    // Привязка текстур к материалу
                    settings.compositeMaterial.SetTexture(settings.compositeMaterialColourTextureName, colourHandle);
                    settings.compositeMaterial.SetTexture(settings.compositeMaterialDepthTextureName, depthHandle);
                    settings.compositeMaterial.SetTexture(settings.compositeMaterialFogTextureName, fogHandle);

                    // Финальная композиция
                    Blit(cmd, fogHandle, cameraTarget, settings.compositeMaterial);

                    context.ExecuteCommandBuffer(cmd);
                }
                finally
                {
                    CommandBufferPool.Release(cmd);
                }
            }

            public void Dispose()
            {
                SafeRelease(ref colourHandle);
                SafeRelease(ref fogHandle);
                SafeRelease(ref depthHandle);
            }

            private bool ValidateResources()
            {
                return settings != null
                    && settings.compositeMaterial != null
                    && settings.depthMaterial != null
                    && settings.fogMaterial != null
                    && IsHandleValid(colourHandle)
                    && IsHandleValid(fogHandle)
                    && IsHandleValid(depthHandle);
            }

            private void RecreateRTHandle(ref RTHandle handle, RenderTextureDescriptor desc, string name)
            {
                if (handle != null &&
                    handle.rt != null &&
                    handle.rt.descriptor.width == desc.width &&
                    handle.rt.descriptor.height == desc.height &&
                    handle.rt.descriptor.colorFormat == desc.colorFormat)
                {
                    return;
                }

                handle?.Release();
                RenderingUtils.ReAllocateIfNeeded(
                    ref handle,
                    desc,
                    FilterMode.Bilinear,
                    TextureWrapMode.Clamp,
                    isShadowMap: false,
                    1,
                    name: name
                );
            }

            private void SafeRelease(ref RTHandle handle)
            {
                handle?.Release();
                handle = null;
            }

            private static bool IsHandleValid(RTHandle handle) =>
                handle != null && handle.rt != null && handle.rt.IsCreated();

            private static RenderTextureFormat GetRenderTextureFormat(RenderTextureQuality quality)
            {
                var format = quality switch
                {
                    RenderTextureQuality.Low => RenderTextureFormat.Default,
                    RenderTextureQuality.Medium => RenderTextureFormat.ARGB64,
                    RenderTextureQuality.High => RenderTextureFormat.ARGBFloat,
                    _ => RenderTextureFormat.Default
                };

                if (!SystemInfo.SupportsRenderTextureFormat(format))
                {
                    Debug.LogWarning($"RenderTextureFormat {format} not supported. Using default.");
                    return RenderTextureFormat.Default;
                }
                return format;
            }
        }
    }
}