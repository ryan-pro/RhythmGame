using Cysharp.Threading.Tasks;
using RhythmGame.GeneralAudio;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace RhythmGame.UI
{
    public class MenuManager : BaseSceneController
    {
        [System.Serializable]
        public class MenuItem
        {
            public string Key;
            public BaseMenu Value;
        }

        [Header("References")]
        [SerializeField]
        private MenuMusicSynchronizer synchronizer;
        [SerializeField]
        private MenuItem[] menuItems;

        [Header("Configuration")]
        public string StartingMenuKey = "Title";

        private readonly Dictionary<string, BaseMenu> menuDictionary = new();

        public override async UniTask InitializeScene()
        {
            await base.InitializeScene();
            var audioTask = UniTask.CompletedTask;

            if (!AudioSystem.IsInitialized)
                audioTask = AudioSystem.Initialize(this.GetCancellationTokenOnDestroy());

            if(!FindObjectOfType<UnityEngine.EventSystems.EventSystem>())
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));

            foreach (var item in menuItems)
                item.Value.Initialize(this);

            await audioTask;
        }

        public override async UniTask StartScene()
        {
            var lifetimeToken = this.GetCancellationTokenOnDestroy();
            var conductTask = synchronizer.BeginMusicConduction(lifetimeToken);

            foreach (var item in menuItems)
                item.Value.gameObject.SetActive(false);

            menuDictionary.EnsureCapacity(menuItems.Length);

            foreach (var item in menuItems)
                menuDictionary.Add(item.Key, item.Value);

            await conductTask;
            await ChangeScene(StartingMenuKey, lifetimeToken);
        }

        public async UniTask ChangeScene(string sceneKey, CancellationToken token)
        {
            if (!menuDictionary.TryGetValue(sceneKey, out var menuToOpen))
            {
                Debug.LogError($"No menu with key {sceneKey} found");
                return;
            }

            var currentMenu = System.Array.Find(menuItems, a => a.Value.gameObject.activeSelf);

            if (currentMenu != null)
                await currentMenu.Value.Hide(token);

            await menuToOpen.Display(token);
        }

        public async UniTask CloseMenus(CancellationToken token)
        {
            var currentMenu = System.Array.Find(menuItems, a => a.Value.gameObject.activeSelf);

            if (currentMenu != null)
                await currentMenu.Value.Hide(token);
        }
    }
}
