using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MagicLightmapSwitcher
{
    public class MLSObject : MonoBehaviour
    {
        [SerializeField] 
        public string scriptId;
        [SerializeField] 
        public string parentScene;
        [SerializeField] 
        public Mesh defaultMesh;
        [SerializeField] 
        public Transform defaultTransform;
        
        public MagicLightmapSwitcher.AffectedObject affectableObject;
        public MaterialPropertyBlock propertyBlock;
        public MagicLightmapSwitcher switcherInstance;
        public MagicLightmapSwitcher.AffectedObject objectData;
        public MeshRenderer meshRenderer;
        public MaterialPropertyBlock materialPropertyBlock;
        public Terrain terrain;
        public List<ReflectionProbeBlendInfo> closestReflectionProbes = new List<ReflectionProbeBlendInfo>();
        public int _MLS_OBJECT_BLENDING_DATA;
        public string[] probeNames = new string[2];
        public int[] probeIndexes = new int[2];
        public bool isStatic;
        private Vector3 _lastPosition;
        private bool updated;
        

        public void OnEnable()
        {
            _MLS_OBJECT_BLENDING_DATA = Shader.PropertyToID("_MLS_OBJECT_BLENDING_DATA");
            
            switcherInstance = RuntimeAPI.GetSwitcherInstanceStatic(gameObject.scene.name);
            meshRenderer = gameObject.GetComponent<MeshRenderer>();
            terrain = gameObject.GetComponent<Terrain>();
            propertyBlock = new MaterialPropertyBlock();
            updated = false;

            if (meshRenderer != null)
            {
                meshRenderer.GetPropertyBlock(propertyBlock);
            }
            else if (terrain != null)
            {
                terrain.GetSplatMaterialPropertyBlock(propertyBlock);
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        public void Update()
        {
            if (switcherInstance == null)
            {
                switcherInstance = RuntimeAPI.GetSwitcherInstanceStatic(gameObject.scene.name);
            }
            
            if (!switcherInstance.storedDataUpdated)
            {
                return;
            }
            
            if (!switcherInstance.useTextureCubeArrays)
            {
                return;
            }
            
            if (meshRenderer == null)
            {
                meshRenderer = gameObject.GetComponent<MeshRenderer>();
            }

            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
                meshRenderer.GetPropertyBlock(propertyBlock);
            }

            if (_lastPosition != transform.position)
            {
                if (isStatic && updated)
                {
                    return;
                }

                meshRenderer.GetClosestReflectionProbes(closestReflectionProbes);

                if (closestReflectionProbes.Count > 0)
                {
                    probeNames[0] = closestReflectionProbes[0].probe.name
                        .Split(new [] { "::" }, System.StringSplitOptions.None)[1];

                    probeIndexes[0] = int.Parse(probeNames[0]);
                    
                    if (closestReflectionProbes.Count == 2)
                    {
                        probeNames[1] = closestReflectionProbes[1].probe.name
                            .Split(new [] { "::" }, System.StringSplitOptions.None)[1];
                        probeIndexes[1] = int.Parse(probeNames[1]);
                    }
                }
                
                propertyBlock.SetVector(
                    _MLS_OBJECT_BLENDING_DATA,
                    new Vector4(
                        meshRenderer.lightmapIndex,
                        probeIndexes[0],
                        probeIndexes[1], 
                        switcherInstance.sceneReflectionProbes.Count));
            
                meshRenderer.SetPropertyBlock(propertyBlock);

                _lastPosition = transform.position;
                updated = true;
            }
        }
        
        public void UpdateGUID()
        {
            scriptId = Guid.NewGuid().ToString();
        }
    }
}
