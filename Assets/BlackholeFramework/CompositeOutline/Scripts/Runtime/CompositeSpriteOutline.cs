using UnityEngine;
using UnityEngine.Rendering;

namespace Blackhole.CompositeOutline
{
    [AddComponentMenu("Blackhole/Rendering/Composite Sprite Outline")]
    [DisallowMultipleComponent]
    public sealed class CompositeSpriteOutline : MonoBehaviour
    {
        private const string DisplayName = "Composite Outline";
        private const string MaskMaterialResourcePath = "CompositeOutline/CompositeSpriteMask";
        private const string OutlineMaterialResourcePath = "CompositeOutline/CompositeSpriteOutline";
        private static readonly int MaskTextureId = Shader.PropertyToID("_MaskTex");
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
        private static readonly Vector2[] DisplayUvs = { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
        private static readonly int[] DisplayTriangles = { 0, 1, 2, 0, 2, 3 };

        [SerializeField] private SpriteRenderer[] _sourceRenderers;
        [SerializeField] private Material _maskMaterial;
        [SerializeField] private Material _outlineMaterial;
        [SerializeField, Min(32)] private int _textureSize = 256;
        [SerializeField] private Color _outlineColor = new(0.05f, 0.05f, 0.05f, 1f);
        [SerializeField, Range(1f, 6f)] private float _outlineWidth = 3f;
        [SerializeField, Range(0.01f, 0.25f)] private float _padding = 0.05f;

        private CommandBuffer _commandBuffer;
        private RenderTexture _maskTexture;
        private Material _maskMaterialInstance;
        private Material _outlineMaterialInstance;
        private Mesh _displayMesh;
        private MeshRenderer _displayRenderer;
        private readonly Vector3[] _displayVertices = new Vector3[4];

        public void SetSourceRenderers(params SpriteRenderer[] sourceRenderers)
        {
            _sourceRenderers = sourceRenderers;
        }

        public void SetOutline(Color color, float width)
        {
            _outlineColor = color;
            _outlineWidth = Mathf.Clamp(width, 1f, 6f);
            ApplyOutlineProperties();
        }

        public void SetTextureSize(int textureSize)
        {
            _textureSize = Mathf.Max(32, textureSize);
        }

        private void Reset()
        {
            _sourceRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        private void OnValidate()
        {
            _textureSize = Mathf.Max(32, _textureSize);
            _outlineWidth = Mathf.Clamp(_outlineWidth, 1f, 6f);
            _padding = Mathf.Clamp(_padding, 0.01f, 0.25f);
            ApplyOutlineProperties();
        }

        private void OnEnable()
        {
            CreateResources();
        }

        private void LateUpdate()
        {
            if (!EnsureResources() || !TryGetLocalBounds(out Bounds localBounds))
            {
                SetDisplayEnabled(false);
                return;
            }

            SetDisplayEnabled(true);
            Bounds paddedBounds = AddPadding(localBounds);
            UpdateDisplayMesh(paddedBounds);
            UpdateDisplaySorting();
            RenderMask(paddedBounds);
        }

        private void OnDisable()
        {
            ReleaseResources();
        }

        private void OnDestroy()
        {
            ReleaseResources();
        }

        private bool EnsureResources()
        {
            if (_maskTexture != null && _maskTexture.width == _textureSize && _maskMaterialInstance != null && _outlineMaterialInstance != null && _displayRenderer != null)
            {
                return true;
            }

            ReleaseResources();
            CreateResources();
            return _maskTexture != null && _maskMaterialInstance != null && _outlineMaterialInstance != null && _displayRenderer != null;
        }

        private void CreateResources()
        {
            Material maskMaterial = ResolveMaterial(_maskMaterial, MaskMaterialResourcePath);
            Material outlineMaterial = ResolveMaterial(_outlineMaterial, OutlineMaterialResourcePath);

            if (!isActiveAndEnabled || maskMaterial == null || outlineMaterial == null)
            {
                return;
            }

            _commandBuffer = new CommandBuffer { name = "Composite Sprite Outline" };
            _maskMaterialInstance = new Material(maskMaterial) { name = "Composite Sprite Mask Material (Runtime)" };
            _outlineMaterialInstance = new Material(outlineMaterial) { name = "Composite Sprite Outline Material (Runtime)" };

            int size = Mathf.Max(32, _textureSize);
            _maskTexture = new RenderTexture(size, size, 0, RenderTextureFormat.ARGB32)
            {
                name = "Composite Sprite Outline Mask",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                useMipMap = false,
                autoGenerateMips = false
            };
            _maskTexture.Create();

            _outlineMaterialInstance.SetTexture(MaskTextureId, _maskTexture);
            ApplyOutlineProperties();
            CreateDisplay();
        }

        private static Material ResolveMaterial(Material configuredMaterial, string resourcePath)
        {
            return configuredMaterial != null ? configuredMaterial : Resources.Load<Material>(resourcePath);
        }

        private void ApplyOutlineProperties()
        {
            if (_outlineMaterialInstance == null)
            {
                return;
            }

            _outlineMaterialInstance.SetColor(OutlineColorId, _outlineColor);
            _outlineMaterialInstance.SetFloat(OutlineWidthId, _outlineWidth);
        }

        private void CreateDisplay()
        {
            var display = new GameObject(DisplayName);
            display.transform.SetParent(transform, false);

            _displayMesh = new Mesh { name = "Composite Sprite Outline Mesh" };
            _displayMesh.MarkDynamic();
            _displayMesh.vertices = _displayVertices;
            _displayMesh.uv = DisplayUvs;
            _displayMesh.triangles = DisplayTriangles;

            var meshFilter = display.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = _displayMesh;

            _displayRenderer = display.AddComponent<MeshRenderer>();
            _displayRenderer.sharedMaterial = _outlineMaterialInstance;
            _displayRenderer.shadowCastingMode = ShadowCastingMode.Off;
            _displayRenderer.receiveShadows = false;
            _displayRenderer.lightProbeUsage = LightProbeUsage.Off;
            _displayRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private bool TryGetLocalBounds(out Bounds localBounds)
        {
            localBounds = default;
            bool hasBounds = false;

            if (_sourceRenderers == null)
            {
                return false;
            }

            foreach (SpriteRenderer sourceRenderer in _sourceRenderers)
            {
                if (sourceRenderer == null || !sourceRenderer.enabled || sourceRenderer.sprite == null)
                {
                    continue;
                }

                Bounds spriteBounds = sourceRenderer.sprite.bounds;
                EncapsulatePoint(sourceRenderer, new Vector3(spriteBounds.min.x, spriteBounds.min.y), ref localBounds, ref hasBounds);
                EncapsulatePoint(sourceRenderer, new Vector3(spriteBounds.min.x, spriteBounds.max.y), ref localBounds, ref hasBounds);
                EncapsulatePoint(sourceRenderer, new Vector3(spriteBounds.max.x, spriteBounds.min.y), ref localBounds, ref hasBounds);
                EncapsulatePoint(sourceRenderer, new Vector3(spriteBounds.max.x, spriteBounds.max.y), ref localBounds, ref hasBounds);
            }

            return hasBounds && localBounds.size.x > Mathf.Epsilon && localBounds.size.y > Mathf.Epsilon;
        }

        private void EncapsulatePoint(SpriteRenderer sourceRenderer, Vector3 sourcePoint, ref Bounds localBounds, ref bool hasBounds)
        {
            Vector3 localPoint = transform.InverseTransformPoint(sourceRenderer.transform.TransformPoint(sourcePoint));
            if (!hasBounds)
            {
                localBounds = new Bounds(localPoint, Vector3.zero);
                hasBounds = true;
                return;
            }

            localBounds.Encapsulate(localPoint);
        }

        private Bounds AddPadding(Bounds bounds)
        {
            Vector3 size = bounds.size;
            float padding = Mathf.Max(size.x, size.y) * _padding;
            size.x += padding * 2f;
            size.y += padding * 2f;
            bounds.size = size;
            return bounds;
        }

        private void UpdateDisplayMesh(Bounds bounds)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            _displayVertices[0] = new Vector3(min.x, min.y, 0f);
            _displayVertices[1] = new Vector3(min.x, max.y, 0f);
            _displayVertices[2] = new Vector3(max.x, max.y, 0f);
            _displayVertices[3] = new Vector3(max.x, min.y, 0f);
            _displayMesh.vertices = _displayVertices;
            _displayMesh.RecalculateBounds();
        }

        private void UpdateDisplaySorting()
        {
            int sortingLayerId = 0;
            int sortingOrder = 0;
            bool foundRenderer = false;

            foreach (SpriteRenderer sourceRenderer in _sourceRenderers)
            {
                if (sourceRenderer == null)
                {
                    continue;
                }

                if (!foundRenderer || sourceRenderer.sortingOrder < sortingOrder)
                {
                    sortingLayerId = sourceRenderer.sortingLayerID;
                    sortingOrder = sourceRenderer.sortingOrder;
                    foundRenderer = true;
                }
            }

            _displayRenderer.sortingLayerID = sortingLayerId;
            _displayRenderer.sortingOrder = sortingOrder - 1;
        }

        private void RenderMask(Bounds bounds)
        {
            Matrix4x4 viewMatrix = transform.worldToLocalMatrix;
            Matrix4x4 projectionMatrix = Matrix4x4.Ortho(bounds.min.x, bounds.max.x, bounds.min.y, bounds.max.y, -100f, 100f);
            Matrix4x4 gpuProjectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, true);

            _commandBuffer.Clear();
            _commandBuffer.SetRenderTarget(_maskTexture);
            _commandBuffer.ClearRenderTarget(false, true, Color.clear);
            _commandBuffer.SetViewProjectionMatrices(viewMatrix, gpuProjectionMatrix);

            foreach (SpriteRenderer sourceRenderer in _sourceRenderers)
            {
                if (sourceRenderer != null && sourceRenderer.enabled && sourceRenderer.sprite != null)
                {
                    _commandBuffer.DrawRenderer(sourceRenderer, _maskMaterialInstance);
                }
            }

            Graphics.ExecuteCommandBuffer(_commandBuffer);
        }

        private void SetDisplayEnabled(bool enabled)
        {
            if (_displayRenderer != null)
            {
                _displayRenderer.enabled = enabled;
            }
        }

        private void ReleaseResources()
        {
            _commandBuffer?.Release();
            _commandBuffer = null;

            if (_maskTexture != null)
            {
                _maskTexture.Release();
            }

            DestroyRuntimeObject(_maskTexture);
            DestroyRuntimeObject(_maskMaterialInstance);
            DestroyRuntimeObject(_outlineMaterialInstance);
            DestroyRuntimeObject(_displayMesh);

            if (_displayRenderer != null)
            {
                DestroyRuntimeObject(_displayRenderer.gameObject);
            }

            _maskTexture = null;
            _maskMaterialInstance = null;
            _outlineMaterialInstance = null;
            _displayMesh = null;
            _displayRenderer = null;
        }

        private static void DestroyRuntimeObject(Object runtimeObject)
        {
            if (runtimeObject == null)
            {
                return;
            }

            if (UnityEngine.Application.isPlaying)
            {
                Destroy(runtimeObject);
                return;
            }

            DestroyImmediate(runtimeObject);
        }
    }
}
