using UnityEngine;

namespace MagicLightmapSwitcher
{
    [ExecuteInEditMode]
    public class MLSStaticRenderer : MLSObject 
    {
        private new void OnEnable()
        {
            base.OnEnable();
            
            if (parentScene != null && parentScene != gameObject.scene.name)
            {
                parentScene = gameObject.scene.name;
            }
        }

        private new void Update()
        {
            base.Update();
        }
    }
}
