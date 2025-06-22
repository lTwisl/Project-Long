using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class BlitSource : MonoBehaviour
{
    [SerializeField] private bool IsRecord;
    [SerializeField] private List<RenderTexture> _renderTexture = new();
    [SerializeField] private List<Texture2D> _outputTexture = new();

    [SerializeField] private List<Camera> _cameras;

    private void Start()
    {
        foreach (Camera cam in _cameras)
        {
            RenderTexture RT = new RenderTexture(Screen.width, Screen.height, 0, GraphicsFormat.R8G8B8A8_SRGB);
            InitializeSceneRenderTexture(RT, Screen.width, Screen.height);
            _renderTexture.Add(RT);

            Texture2D texture2D = new Texture2D(Screen.width, Screen.height, GraphicsFormatUtility.GetTextureFormat(RT.graphicsFormat), false);
            texture2D = InitializeTexture(texture2D, RT, GraphicsFormatUtility.GetTextureFormat(RT.graphicsFormat), "Tex Scene");
            _outputTexture.Add(texture2D);
        }
    }

    void Update()
    {
        if (!IsRecord) return;

        for (int i = 0; i < _renderTexture.Count; i++)
        {
            _outputTexture[i] = ScreenCapture.CaptureScreenshotAsTexture();
            //// Создаем CommandBuffer
            //CommandBuffer commandBuffer = new CommandBuffer();
            //commandBuffer.name = "Blit Capture";

            //// Выполняем Blit из основного цветового буфера в нашу RenderTexture
            ////commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, _renderTexture[i]);
            //Graphics.Blit(_renderTexture[i], _renderTexture[i]);

            //// Добавляем команды в камеру
            //_camerasScene.СameraInfo[i].mainCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, commandBuffer);

            //// Запускаем выполнение команд
            //Graphics.ExecuteCommandBuffer(commandBuffer);

            //// Копируем данные из RenderTexture в Texture2D
            //Graphics.CopyTexture(_renderTexture[i], _outputTexture[i]);

            //// Освобождаем ресурсы
            //commandBuffer.Release();
            //_camerasScene.СameraInfo[i].mainCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, commandBuffer);
        }
    }

    public void InitializeSceneRenderTexture(RenderTexture renderTex, int pixelWidth, int pixelHeight)
    {
        if (renderTex) renderTex.Release();

        renderTex = new RenderTexture(pixelWidth, pixelHeight, 0, GraphicsFormat.R8G8B8A8_SRGB)
        {
            name = "Scene Render Texture",
            depthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt,
            antiAliasing = 4
        };
        renderTex.Create();
    }

    private Texture2D InitializeTexture(Texture2D texture2D, RenderTexture renderTexture, TextureFormat textureFormat, string name)
    {
        if (!texture2D)
        {
            texture2D = new Texture2D(renderTexture.width, renderTexture.height, textureFormat, false);
            texture2D.name = name;
        }
        else if (texture2D.width != renderTexture.width || texture2D.height != renderTexture.height)
        {
            texture2D.Reinitialize(renderTexture.width, renderTexture.height);
        }
        texture2D.Apply();

        return texture2D;
    }
}