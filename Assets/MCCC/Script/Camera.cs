using UnityEngine;
using UnityEngine.UI;

namespace MCCC.Script
{
    public class Camera : MonoBehaviour
    {
        [SerializeField] private RawImage rawImage;

        private WebCamTexture webCamTexture;
        private const int INPUT_WIDTH = 640;
        private const int INPUT_HEIGHT = 480;
        private const int FPS = 30;

        void Start()
        {
            // Webカメラの開始
            webCamTexture = new WebCamTexture(INPUT_WIDTH, INPUT_HEIGHT, FPS);
            rawImage.texture = webCamTexture;
            webCamTexture.Play();
        }
    }
}