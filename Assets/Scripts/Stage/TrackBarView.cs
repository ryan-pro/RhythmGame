using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RhythmGame
{
    public class TrackBarView : MonoBehaviour
    {
        [SerializeField]
        private UnityObjectPoolBase barPrefabPool;

        [SerializeField]
        private Transform barParent;
        [SerializeField]
        private RectTransform startPoint, endPoint;
    }
}
