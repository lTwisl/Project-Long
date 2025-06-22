using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System.Collections.Generic;

public class CustomRenderTarget : MonoBehaviour
{
    [SerializeField] private List<Camera> Cameras;

    [SerializeField] private List<RenderTexture> RenderTextures = new List<RenderTexture>();
    [SerializeField] private List<RenderTexture> TempRenderTextures = new List<RenderTexture>();
    [SerializeField] private List<Texture2D> Textures2D = new List<Texture2D>();

    private readonly Vector2 SCALE = new(1, -1);
    private readonly Vector2 OFFSET = new(0, 1);

    private void Awake()
    {
        for (int i = 0; i < Cameras.Count; i++)
        {
            // Создаем RenderTexture
            RenderTexture rt = null;
            InitializeRenderTexture(ref rt, Screen.width, Screen.height, $"RenderTexture_{Cameras[i].name}");
            RenderTextures.Add(rt);

            RenderTexture trt = null;
            InitializeRenderTexture(ref trt, Screen.width, Screen.height, $"RenderTexture_{Cameras[i].name}");
            TempRenderTextures.Add(trt);

            // Создаем Texture2D
            Texture2D tex2D = null;
            InitializeTexture2D(ref tex2D, rt, TextureFormat.RGBA32, $"Texture2D_{Cameras[i].name}");
            Textures2D.Add(tex2D);
        }
    }

    void OnEnable()
    {
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera targetCamera)
    {
        Debug.Log($"Камера - {targetCamera.name}. Завершила отрисовку");
        int index = Cameras.IndexOf(targetCamera);
        if (index == -1) return; // Камера не из нашего списка

        // Захват изображения в RenderTexture
        Graphics.Blit(targetCamera.activeTexture, RenderTextures[index]);

        // Переворот изображения
        Graphics.Blit(RenderTextures[index], TempRenderTextures[index], SCALE, OFFSET);

        // Получение Texture2D
        Graphics.CopyTexture(TempRenderTextures[index], Textures2D[index]);
    }

    public void InitializeRenderTexture(ref RenderTexture renderTex, int width, int height, string name)
    {
        if (renderTex != null)
        {
            renderTex.Release();
        }

        renderTex = new RenderTexture(width, height, 0, GraphicsFormat.R8G8B8A8_SRGB)
        {
            name = name,
            depthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt,
            antiAliasing = 4,
            useMipMap = false,
            autoGenerateMips = false
        };
        renderTex.Create();
    }

    public void InitializeTexture2D(ref Texture2D texture2D, RenderTexture renderTexture, TextureFormat format, string name)
    {
        if (texture2D == null || texture2D.width != renderTexture.width || texture2D.height != renderTexture.height)
        {
            texture2D = new Texture2D(renderTexture.width, renderTexture.height, format, false);
            texture2D.name = name;
        }
        else
        {
            texture2D.Reinitialize(renderTexture.width, renderTexture.height);
        }
        texture2D.Apply();
    }
}