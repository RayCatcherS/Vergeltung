using UnityEngine;

namespace MagicLightmapSwitcher
{
    /*
     * Your own class that will manage the Custom Blendable must inherit from the MLSCustomBlendable base class, 
     * which contains some methods for synchronizing fields with the MLS system.
     */

    [ExecuteInEditMode]
    public class EmissiveCustomBlendable : MLSCustomBlendable
    {
        /*
         * We declare the parameter(s) that must be taken into account
         * by the MLS system when blending/switching lightmaps
         * 
         * Such parameters should be marked with the "mls_b" prefix - this is necessary 
         * for MLS to recognize the required fields in your code
         */

        [ColorUsage(false, true)]
        public Color mls_b_color;
        [Range(0.5f, 1.5f)]
        public float mls_b_scale;

        /* 
         * Since in this case we want to control the parameters of the object's material, 
         * we need to access the "Mesh Renderer" and "Property Block"
         */

        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock propertyBlock;

        void OnEnable()
        {
            /*
             * A call to the base OnEnable method is required to correctly identify the object by the MLS
             */

            base.OnEnable();

            meshRenderer = GetComponent<MeshRenderer>();
            propertyBlock = new MaterialPropertyBlock();
        }

        // Update is called once per frame
        void Update()
        {
            /*
             * Next, it remains only to call the necessary methods with our mls_b_variable 
             * as the corresponding parameters
             */

#if MT_HDRP_7_INCLUDED || MT_HDRP_8_INCLUDED || MT_HDRP_9_INCLUDED || MT_HDRP_10_INCLUDED || MT_HDRP_11_INCLUDED || MT_HDRP_12_INCLUDED
            propertyBlock.SetColor("_EmissiveColor", mls_b_color);
#else
            propertyBlock.SetColor("_EmissionColor", mls_b_color);
#endif
            meshRenderer.SetPropertyBlock(propertyBlock);
            transform.localScale = Vector3.one * mls_b_scale;
        }
    }
}
