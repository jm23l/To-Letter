using UnityEngine;
namespace TimeCourier
{
    [ExecuteAlways, RequireComponent(typeof(Renderer))]
    public class VoxelTint : MonoBehaviour
    {
        public Color color = Color.white;
        private void OnEnable() { Apply(); }
        private void OnValidate() { Apply(); }
        public void Apply()
        {
            var block = new MaterialPropertyBlock();
            block.SetColor("_Color", color);
            GetComponent<Renderer>().SetPropertyBlock(block);
        }
    }
}
