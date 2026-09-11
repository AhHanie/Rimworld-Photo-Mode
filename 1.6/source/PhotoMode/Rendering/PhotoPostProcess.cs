using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public class PhotoPostProcess : MonoBehaviour
    {
        private const string ShaderPath = "PhotoMode/OnePass";
        private const string FallbackShaderName = "Sprites/Default";
        private const string BloomShaderPath = "PhotoMode/Bloom";
        private const string SelectiveBlurShaderPath = "PhotoMode/SelectiveBlur";

        private const int BloomDownsampleFactor = 4;
        private const int BloomThresholdPass = 0;
        private const int BloomBlurPass = 1;
        private const int BloomCompositePass = 2;

        private const int SelectiveBlurDownsampleFactor = 4;
        private const int SelectiveBlurBlurPass = 0;
        private const int SelectiveBlurCompositePass = 1;

        private static readonly int ExposureId = Shader.PropertyToID("_Exposure");
        private static readonly int BrightnessId = Shader.PropertyToID("_Brightness");
        private static readonly int ContrastId = Shader.PropertyToID("_Contrast");
        private static readonly int GammaId = Shader.PropertyToID("_Gamma");
        private static readonly int SaturationId = Shader.PropertyToID("_Saturation");
        private static readonly int TemperatureId = Shader.PropertyToID("_Temperature");
        private static readonly int TintId = Shader.PropertyToID("_Tint");
        private static readonly int VignetteIntensityId = Shader.PropertyToID("_VignetteIntensity");
        private static readonly int VignetteRadiusId = Shader.PropertyToID("_VignetteRadius");
        private static readonly int VignetteSoftnessId = Shader.PropertyToID("_VignetteSoftness");
        private static readonly int SharpenStrengthId = Shader.PropertyToID("_SharpenStrength");
        private static readonly int LutTexId = Shader.PropertyToID("_LutTex");
        private static readonly int LutIntensityId = Shader.PropertyToID("_LutIntensity");
        private static readonly int GrainIntensityId = Shader.PropertyToID("_GrainIntensity");
        private static readonly int GrainSizeId = Shader.PropertyToID("_GrainSize");
        private static readonly int ChromaticAberrationIntensityId = Shader.PropertyToID("_ChromaticAberrationIntensity");

        private static readonly int BloomTexId = Shader.PropertyToID("_BloomTex");
        private static readonly int BloomThresholdId = Shader.PropertyToID("_BloomThreshold");
        private static readonly int BloomIntensityId = Shader.PropertyToID("_BloomIntensity");
        private static readonly int BlurDirectionId = Shader.PropertyToID("_BlurDirection");
        private static readonly int BlurOffsetId = Shader.PropertyToID("_BlurOffset");

        private static readonly int SelectiveBlurTexId = Shader.PropertyToID("_BlurTex");
        private static readonly int FocusPositionId = Shader.PropertyToID("_FocusPosition");
        private static readonly int FocusWidthId = Shader.PropertyToID("_FocusWidth");
        private static readonly int TiltShiftStrengthId = Shader.PropertyToID("_TiltShiftStrength");
        private static readonly int FalloffId = Shader.PropertyToID("_Falloff");
        private static readonly int EdgeBlurStrengthId = Shader.PropertyToID("_EdgeBlurStrength");

        private static Shader shader;
        private static bool shaderLoadAttempted;
        private static Shader bloomShader;
        private static bool bloomShaderLoadAttempted;
        private static Shader selectiveBlurShader;
        private static bool selectiveBlurShaderLoadAttempted;
        private static PhotoPostProcess active;

        private Material material;
        private Material bloomMaterial;
        private Material selectiveBlurMaterial;
        private Camera cachedCamera;

        private Texture2D lutTexture;
        private string loadedLutId;

        public static void EnsureAttached(Camera camera)
        {
            if (camera == null)
            {
                return;
            }

            if (active != null && active.gameObject == camera.gameObject)
            {
                return;
            }

            Detach();
            active = camera.gameObject.AddComponent<PhotoPostProcess>();
            Logger.Message("PhotoPostProcess attached to " + camera.gameObject.name + ".");
        }

        public static void Detach()
        {
            if (active == null)
            {
                return;
            }

            PhotoPostProcess component = active;
            active = null;
            Destroy(component);
            Logger.Message("PhotoPostProcess detached.");
        }

        private void Awake()
        {
            cachedCamera = GetComponent<Camera>();

            if (!shaderLoadAttempted)
            {
                shaderLoadAttempted = true;
                shader = ContentFinder<Shader>.Get(ShaderPath, reportFailure: false) ?? Shader.Find(FallbackShaderName);
            }

            if (shader != null && shader.isSupported)
            {
                material = new Material(shader) { color = Color.white };
            }

            Logger.Message("PhotoPostProcess shader: " + (shader != null ? shader.name : "none") + ".");

            if (!bloomShaderLoadAttempted)
            {
                bloomShaderLoadAttempted = true;
                bloomShader = ContentFinder<Shader>.Get(BloomShaderPath, reportFailure: false);
            }

            if (bloomShader != null && bloomShader.isSupported)
            {
                bloomMaterial = new Material(bloomShader);
            }

            Logger.Message("PhotoPostProcess bloom shader: " + (bloomShader != null ? bloomShader.name : "none") + ".");

            if (!selectiveBlurShaderLoadAttempted)
            {
                selectiveBlurShaderLoadAttempted = true;
                selectiveBlurShader = ContentFinder<Shader>.Get(SelectiveBlurShaderPath, reportFailure: false);
            }

            if (selectiveBlurShader != null && selectiveBlurShader.isSupported)
            {
                selectiveBlurMaterial = new Material(selectiveBlurShader);
            }

            Logger.Message("PhotoPostProcess selective blur shader: " + (selectiveBlurShader != null ? selectiveBlurShader.name : "none") + ".");
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!ShouldGrade())
            {
                Graphics.Blit(source, destination);
                NotifyCaptureComplete();
                return;
            }

            ApplyImageSettings();

            PhotoImageSettings image = PhotoModeManager.Current.State.Image;
            bool bloomActive = image.BloomEnabled && bloomMaterial != null;
            bool selectiveBlurActive = (image.TiltShiftEnabled || image.EdgeBlurEnabled) && selectiveBlurMaterial != null;

            if (!bloomActive && !selectiveBlurActive)
            {
                Graphics.Blit(source, destination, material);
                NotifyCaptureComplete();
                return;
            }

            RenderTextureFormat hdrFormat = RenderTextureFormat.DefaultHDR;
            RenderTexture graded = RenderTexture.GetTemporary(source.width, source.height, 0, hdrFormat);
            RenderTexture stage = null;
            try
            {
                Graphics.Blit(source, graded, material);
                RenderTexture current = graded;

                if (bloomActive)
                {
                    stage = RenderTexture.GetTemporary(source.width, source.height, 0, hdrFormat);
                    RenderBloom(current, stage, image);
                    current = stage;
                }

                if (selectiveBlurActive)
                {
                    RenderTexture blurredStage = RenderTexture.GetTemporary(source.width, source.height, 0, hdrFormat);
                    RenderSelectiveBlur(current, blurredStage, image);
                    if (stage != null)
                    {
                        RenderTexture.ReleaseTemporary(stage);
                    }

                    stage = blurredStage;
                    current = blurredStage;
                }

                Graphics.Blit(current, destination);
            }
            finally
            {
                RenderTexture.ReleaseTemporary(graded);
                if (stage != null)
                {
                    RenderTexture.ReleaseTemporary(stage);
                }
            }

            NotifyCaptureComplete();
        }

        private void RenderBloom(RenderTexture gradedSource, RenderTexture destination, PhotoImageSettings image)
        {
            RenderTextureFormat hdrFormat = RenderTextureFormat.DefaultHDR;
            RenderTexture bloomA = null;
            RenderTexture bloomB = null;
            try
            {
                int bloomWidth = Mathf.Max(1, gradedSource.width / BloomDownsampleFactor);
                int bloomHeight = Mathf.Max(1, gradedSource.height / BloomDownsampleFactor);

                bloomA = RenderTexture.GetTemporary(bloomWidth, bloomHeight, 0, hdrFormat);
                bloomB = RenderTexture.GetTemporary(bloomWidth, bloomHeight, 0, hdrFormat);

                bloomMaterial.SetFloat(BloomThresholdId, image.BloomThreshold);
                bloomMaterial.SetFloat(BloomIntensityId, image.BloomIntensity);
                Graphics.Blit(gradedSource, bloomA, bloomMaterial, BloomThresholdPass);

                float blurOffset = Mathf.Lerp(1f, 3f, image.BloomSpread);
                int iterations = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(1, 4, image.BloomSpread)), 1, 4);
                bloomMaterial.SetFloat(BlurOffsetId, blurOffset);

                for (int i = 0; i < iterations; i++)
                {
                    bloomMaterial.SetVector(BlurDirectionId, new Vector4(1f, 0f, 0f, 0f));
                    Graphics.Blit(bloomA, bloomB, bloomMaterial, BloomBlurPass);

                    bloomMaterial.SetVector(BlurDirectionId, new Vector4(0f, 1f, 0f, 0f));
                    Graphics.Blit(bloomB, bloomA, bloomMaterial, BloomBlurPass);
                }

                bloomMaterial.SetTexture(BloomTexId, bloomA);
                Graphics.Blit(gradedSource, destination, bloomMaterial, BloomCompositePass);
            }
            finally
            {
                if (bloomA != null)
                {
                    RenderTexture.ReleaseTemporary(bloomA);
                }

                if (bloomB != null)
                {
                    RenderTexture.ReleaseTemporary(bloomB);
                }
            }
        }

        private void RenderSelectiveBlur(RenderTexture source, RenderTexture destination, PhotoImageSettings image)
        {
            RenderTextureFormat hdrFormat = RenderTextureFormat.DefaultHDR;
            RenderTexture blurA = null;
            RenderTexture blurB = null;
            try
            {
                int blurWidth = Mathf.Max(1, source.width / SelectiveBlurDownsampleFactor);
                int blurHeight = Mathf.Max(1, source.height / SelectiveBlurDownsampleFactor);

                blurA = RenderTexture.GetTemporary(blurWidth, blurHeight, 0, hdrFormat);
                blurB = RenderTexture.GetTemporary(blurWidth, blurHeight, 0, hdrFormat);

                Graphics.Blit(source, blurA);

                float tiltShiftStrength = image.TiltShiftEnabled ? image.TiltShiftStrength : 0f;
                float edgeBlurStrength = image.EdgeBlurEnabled ? image.EdgeBlurStrength : 0f;
                float blurAmount = Mathf.Max(tiltShiftStrength, edgeBlurStrength);

                float blurOffset = Mathf.Lerp(1f, 4f, blurAmount);
                int iterations = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(1, 3, blurAmount)), 1, 3);
                selectiveBlurMaterial.SetFloat(BlurOffsetId, blurOffset);

                for (int i = 0; i < iterations; i++)
                {
                    selectiveBlurMaterial.SetVector(BlurDirectionId, new Vector4(1f, 0f, 0f, 0f));
                    Graphics.Blit(blurA, blurB, selectiveBlurMaterial, SelectiveBlurBlurPass);

                    selectiveBlurMaterial.SetVector(BlurDirectionId, new Vector4(0f, 1f, 0f, 0f));
                    Graphics.Blit(blurB, blurA, selectiveBlurMaterial, SelectiveBlurBlurPass);
                }

                selectiveBlurMaterial.SetTexture(SelectiveBlurTexId, blurA);
                selectiveBlurMaterial.SetFloat(FocusPositionId, image.TiltShiftFocusPosition);
                selectiveBlurMaterial.SetFloat(FocusWidthId, image.TiltShiftFocusWidth);
                selectiveBlurMaterial.SetFloat(TiltShiftStrengthId, tiltShiftStrength);
                selectiveBlurMaterial.SetFloat(FalloffId, image.TiltShiftFalloff);
                selectiveBlurMaterial.SetFloat(EdgeBlurStrengthId, edgeBlurStrength);
                Graphics.Blit(source, destination, selectiveBlurMaterial, SelectiveBlurCompositePass);
            }
            finally
            {
                if (blurA != null)
                {
                    RenderTexture.ReleaseTemporary(blurA);
                }

                if (blurB != null)
                {
                    RenderTexture.ReleaseTemporary(blurB);
                }
            }
        }

        private void NotifyCaptureComplete()
        {
            PhotoModeManager current = PhotoModeManager.Current;
            if (current != null)
            {
                current.CaptureService.HandleCameraRenderComplete(cachedCamera);
            }
        }

        private void ApplyImageSettings()
        {
            PhotoImageSettings image = PhotoModeManager.Current.State.Image;
            material.SetFloat(ExposureId, image.Exposure);
            material.SetFloat(BrightnessId, image.Brightness);
            material.SetFloat(ContrastId, image.Contrast);
            material.SetFloat(GammaId, image.Gamma);
            material.SetFloat(SaturationId, image.Saturation);
            material.SetFloat(TemperatureId, image.Temperature);
            material.SetFloat(TintId, image.Tint);
            material.SetFloat(VignetteIntensityId, image.VignetteEnabled ? image.VignetteIntensity : 0f);
            material.SetFloat(VignetteRadiusId, image.VignetteRadius);
            material.SetFloat(VignetteSoftnessId, image.VignetteSoftness);
            material.SetFloat(SharpenStrengthId, image.SharpenEnabled ? image.SharpenStrength : 0f);
            material.SetFloat(GrainIntensityId, image.GrainEnabled ? image.GrainIntensity : 0f);
            material.SetFloat(GrainSizeId, image.GrainSize);
            material.SetFloat(ChromaticAberrationIntensityId, image.ChromaticAberrationEnabled ? image.ChromaticAberrationIntensity : 0f);

            ApplyLutSettings(image);
        }

        private void ApplyLutSettings(PhotoImageSettings image)
        {
            if (image.LutId != loadedLutId)
            {
                if (lutTexture != null)
                {
                    Destroy(lutTexture);
                    lutTexture = null;
                }

                loadedLutId = image.LutId;

                if (string.IsNullOrEmpty(image.LutId))
                {
                    image.LutStatus = PhotoLutStatus.None;
                    image.LutStatusMessage = null;
                }
                else
                {
                    PhotoLutStatus status = PhotoLutLibrary.TryLoad(image.LutId, out Texture2D texture);
                    lutTexture = texture;
                    image.LutStatus = status;
                    image.LutStatusMessage = status == PhotoLutStatus.Loaded ? null : ("PhotoMode.Lut.Error." + status).Translate();
                }
            }

            if (lutTexture != null)
            {
                material.SetTexture(LutTexId, lutTexture);
                material.SetFloat(LutIntensityId, Mathf.Clamp01(image.LutIntensity));
            }
            else
            {
                material.SetFloat(LutIntensityId, 0f);
            }
        }

        private bool ShouldGrade()
        {
            if (material == null)
            {
                return false;
            }

            if (!PhotoModeManager.IsRenderingThisCurrentMap)
            {
                return false;
            }

            if (!WorldRendererUtility.DrawingMap)
            {
                return false;
            }

            return !PhotoModeManager.Current.State.PreviewBypassed;
        }

        private void OnDestroy()
        {
            if (active == this)
            {
                active = null;
            }

            PhotoModeManager.Current?.CaptureService.CancelIfActive("PostProcessDestroyed");

            if (material != null)
            {
                Destroy(material);
                material = null;
            }

            if (bloomMaterial != null)
            {
                Destroy(bloomMaterial);
                bloomMaterial = null;
            }

            if (selectiveBlurMaterial != null)
            {
                Destroy(selectiveBlurMaterial);
                selectiveBlurMaterial = null;
            }

            if (lutTexture != null)
            {
                Destroy(lutTexture);
                lutTexture = null;
            }

            loadedLutId = null;
        }
    }
}
