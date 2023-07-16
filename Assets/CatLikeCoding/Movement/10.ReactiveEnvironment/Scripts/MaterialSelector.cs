using UnityEngine;

namespace ReactiveEnvironment
{
    public class MaterialSelector : MonoBehaviour
    {
        [SerializeField]
        Material[] materials;

        [SerializeField]
        MeshRenderer meshRenderer;

        public void Select(int index)
        {
            if (meshRenderer && materials != null && index >= 0 && index < materials.Length)
            {
                meshRenderer.material = materials[index];
            }
        }
    }
}