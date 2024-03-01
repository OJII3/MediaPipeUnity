using System.Threading;
using Cysharp.Threading.Tasks;
using Mediapipe;
using Mediapipe.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace MCCC.Script
{
    public class MediaPipeHolistic : MonoBehaviour
    {
        [SerializeField] private RawImage _rawImage;
        [SerializeField] private int width = 640;
        [SerializeField] private int height = 480;
        [SerializeField] private int fps = 30;
        [SerializeField] private int deviceIndex = 0;

        [SerializeField] private TextAsset _configAsset;

        private WebCamTexture _webCamTexture;
        private ResourceManager _resourceManager;

        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();
            StartTracking(token).Forget();
        }

        private async UniTask StartTracking(CancellationToken token)
        {
            if (WebCamTexture.devices.Length == 0)
            {
                Debug.LogError("No camera detected");
                return;
            }

            var webCamDevice = WebCamTexture.devices[deviceIndex];
            _webCamTexture = new WebCamTexture(webCamDevice.name, width, height, fps);
            _rawImage.texture = _webCamTexture;
            _webCamTexture.Play();

            await UniTask.WaitUntil(() => _webCamTexture.width > 16, cancellationToken: token);
            _rawImage.rectTransform.sizeDelta = new Vector2(width, height);
            _rawImage.texture = _webCamTexture;

            // start tracking ========================================

            _resourceManager = new LocalResourceManager();
            await _resourceManager.PrepareAssetAsync("face_detection_short_range.bytes");
            await _resourceManager.PrepareAssetAsync("face_landmark_with_attention.bytes");

            var graph = new CalculatorGraph(_configAsset.text);
            graph.StartRun();

            var inputTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixelData = new Color32[width * height];

            while (!token.IsCancellationRequested)
            {
                inputTexture.SetPixels32(_webCamTexture.GetPixels32(pixelData));
                // It's the byte offset between a pixel value and the same pixel and channel in the next row.
                // In most cases, this is equal to the product of the width and the number of channels.
                var imageFrame = new ImageFrame(ImageFormat.Types.Format.Srgba, width, height, width * 4,
                    inputTexture.GetRawTextureData<byte>());

                graph.AddPacketToInputStream("input_video", Packet.CreateImageFrame(imageFrame));

                await UniTask.Yield(PlayerLoopTiming.LastUpdate);
            }
        }
    }
}