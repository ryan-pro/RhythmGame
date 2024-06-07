using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace RhythmGame.UI
{
    //Abstract instead of interface so we can still use Unity's inspector
    public abstract class BaseMenu : MonoBehaviour
    {
        protected MenuManager manager;

        public virtual void Initialize(MenuManager manager) => this.manager = manager;

        public abstract UniTask Display(CancellationToken token);
        public abstract UniTask Hide(CancellationToken token);
    }
}
