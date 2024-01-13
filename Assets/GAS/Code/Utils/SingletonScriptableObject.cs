

namespace GAS {


#pragma warning disable 0618
    using UnityEngine;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;

    public class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject {
        private static T _instance;

        public static T Instance {
            get {
                if (_instance == null) {
                    // _instance = Resources.Load<T>(typeof(T).Name); //This was suppoed to work. Why doesnt it?

                    var objs = Resources.LoadAll("").ToList();
                    var obj = objs.FirstOrDefault(x => x.GetType() == typeof(T));

                    _instance = obj as T;

                    // Debug.Log($"Name: {obj.name}");
                    // Debug.Log($"Type: {obj.GetType().Name}");

                    if (_instance == null) {
                        Debug.LogError($"SingletonScriptableObject<{typeof(T).Name}> not found in Resources folder.");
                    }
                }

                return _instance;
            }
        }

        // Your other variables and methods go here

        public virtual void OnEnable() {
            if (_instance == null) {
                _instance = this as T;
                DontDestroyOnLoad(this);
            } else {
                // If an instance already exists, destroy this duplicate instance
                Destroy(this);
            }
        }
    }

#pragma warning restore 0618

    public class SingletonScriptableObjectLibrary<T, S> : SingletonScriptableObject<T> where T : ScriptableObject where S : Object {
        public bool AutoRefresh = false;
        public Dictionary<string, S> itemDictionary = new Dictionary<string, S>();
        public List<S> itemList = new List<S>();
        public string folder = ""; //Restricts asset search to a specific folder. Useful for libraries of prefabs/gameObjects for example.

        public override void OnEnable() {
            base.OnEnable();
            TryRefresh();
        }

        [EasyButtons.Button]
        public void TryRefresh() {
            if (AutoRefresh == false) {
                return;
            }
            Refresh();
        }

        protected virtual void Refresh() {
            UnityEngine.Object[] assets = Resources.LoadAll(folder, typeof(S));
            itemList = assets.OfType<S>().ToList();
            itemList = itemList.OrderBy(x => x.name).ToList();
            itemDictionary = itemList.ToDictionary(x => x.name, x => x);
        }

        public S GetByName(string name) {
            if (itemList.Count != itemDictionary.Count)
                itemDictionary = itemList.ToDictionary(x => x.name, x => x);

            if (itemDictionary.TryGetValue(name, out var foundItem)) {
                return foundItem;
            }

            Debug.LogError($"{typeof(T).Name} with name '{name}' doesn't exist.");
            return null;
        }
    }


}