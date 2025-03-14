using System;
using UnityEngine;
using UnityEngine.VFX;

namespace GaussianSplatting.Runtime
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(VisualEffect))]
    public class GaussianSplatVfx : MonoBehaviour
    {
        [Serializable]
        [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
        public struct SplatVfxData
        {
            public Vector4 pos;
            public Vector3 axisX, axisY;
            public uint colorPair1;
            public uint colorPair2;
        }

        public VisualEffect Vfx {get; private set; }
        public GaussianSplatRenderer gaussianSplatRenderer;
        
        public bool Initialized {get; private set;}

        private void OnValidate()
        {
            Vfx = GetComponent<VisualEffect>();
        }

        [ContextMenu("Initialize")]
        public void Initialize()
        {
            if (Vfx != null && gaussianSplatRenderer != null && gaussianSplatRenderer.asset != null)
            {
                if (gaussianSplatRenderer.m_RenderMode != GaussianSplatRenderer.RenderMode.VFXDataOnly) return;
                
                Vfx.Reinit();
                Vfx.SetUInt("SplatCount", (uint)gaussianSplatRenderer.splatCount);
                Vfx.SetGraphicsBuffer ("SplatsData", gaussianSplatRenderer.SplatsVfxData);
                Vfx.SetGraphicsBuffer ("SplatsSortingKey", gaussianSplatRenderer.GpuSortKeys);
                Vfx.SendEvent("CreateSplats");

                if (Initialized)
                {
                    gaussianSplatRenderer.vfxDataCalculated -= SyncDataToVfx;
                }
                gaussianSplatRenderer.vfxDataCalculated += SyncDataToVfx;
                Initialized = true;
                Debug. Log($"Splat VFX Vfx initialized for {gaussianSplatRenderer.asset.name} ({gaussianSplatRenderer.splatCount})");   
            }
        }

        private void SyncDataToVfx(Camera cam)
        {
            Vfx.SetFloat("ViewAspectRatio", cam.aspect);
        }

        public void Stop()
        {
            Vfx.Reinit();
            Initialized = false;
        }

        private void Awake()
        {
            Vfx = GetComponent<VisualEffect>();
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            #if UNITY_EDITOR
            EditorPreview();
            #endif
        }

        private void EditorPreview()
        {
            if (Vfx == null) return;
            if (gaussianSplatRenderer?.m_RenderMode != GaussianSplatRenderer.RenderMode.VFXDataOnly)
            {
                if (Vfx.HasAnySystemAwake() && Initialized) Stop();
                return;
            }

            if (!Application.isPlaying && !Initialized)
            {
                Initialize();
                return;
            }

            if (Initialized && !Vfx.HasAnySystemAwake()) Initialize();
        }
    }
}
