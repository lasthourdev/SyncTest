using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace milan_UI
{
    public abstract class UIPanel : MonoBehaviour
    {
        [SerializeField] private bool destroyOnHide = false;

        public UnityEvent OnShow = new UnityEvent();
        public UnityEvent OnHide = new UnityEvent();

        public bool DestroyOnHide => destroyOnHide;

        protected virtual void Awake()
        {
            if (UIManager.HasInstance)
            {
                UIManager.Instance.RegisterPanel(this);
            }
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            OnShow.Invoke();
        }

        public virtual void Hide()
        {
            OnHide.Invoke();
            gameObject.SetActive(false);

            if (destroyOnHide && UIManager.HasInstance)
            {
                UIManager.Instance.DestroyPanel(this);
            }
        }

        public virtual void OnDataReceived(object data) { }
    }

    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;

        public static bool HasInstance => _instance != null;

        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<UIManager>();

                    if (_instance == null)
                    {
                        GameObject managerObject = new GameObject("UIManager");
                        _instance = managerObject.AddComponent<UIManager>();
                        DontDestroyOnLoad(managerObject);
                    }
                }
                return _instance;
            }
        }

        [SerializeField] private Transform panelContainer;
        [SerializeField] private List<UIPanel> panelPrefabs = new List<UIPanel>();

        private readonly Dictionary<Type, UIPanel> _activePanels = new Dictionary<Type, UIPanel>();
        private readonly Dictionary<Type, UIPanel> _prefabCache = new Dictionary<Type, UIPanel>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            if (panelContainer == null)
            {
                panelContainer = transform;
            }

            CachePrefabs();
            FindAndRegisterScenePanels();
        }

        private void CachePrefabs()
        {
            foreach (var prefab in panelPrefabs)
            {
                if (prefab == null) continue;

                var type = prefab.GetType();
                if (!_prefabCache.ContainsKey(type))
                {
                    _prefabCache[type] = prefab;
                }
            }
        }

        private void FindAndRegisterScenePanels()
        {
            var scenePanels = FindObjectsOfType<UIPanel>(true);
            foreach (var panel in scenePanels)
            {
                RegisterPanel(panel);
                panel.gameObject.SetActive(false);
            }
        }

        public void RegisterPanel(UIPanel panel)
        {
            if (panel == null) return;

            var type = panel.GetType();
            _activePanels[type] = panel;
        }

        public T Get<T>() where T : UIPanel
        {
            var type = typeof(T);
            return _activePanels.TryGetValue(type, out var panel) ? panel as T : null;
        }

        public T Show<T>(object data = null) where T : UIPanel
        {
            var panel = GetOrCreatePanel<T>();

            if (panel != null)
            {
                if (data != null)
                {
                    panel.OnDataReceived(data);
                }

                panel.Show();
            }

            return panel;
        }

        public void Hide<T>() where T : UIPanel
        {
            var panel = Get<T>();
            panel?.Hide();
        }

        public void HideAll()
        {
            foreach (var panel in _activePanels.Values)
            {
                if (panel != null && panel.gameObject.activeSelf)
                {
                    panel.Hide();
                }
            }
        }

        public bool IsActive<T>() where T : UIPanel
        {
            var panel = Get<T>();
            return panel != null && panel.gameObject.activeSelf;
        }

        public void DestroyPanel(UIPanel panel)
        {
            if (panel == null) return;

            var type = panel.GetType();
            if (_activePanels.ContainsKey(type))
            {
                _activePanels.Remove(type);
            }

            Destroy(panel.gameObject);
        }

        public void Destroy<T>() where T : UIPanel
        {
            var panel = Get<T>();
            if (panel != null)
            {
                DestroyPanel(panel);
            }
        }

        private T GetOrCreatePanel<T>() where T : UIPanel
        {
            var type = typeof(T);

            if (_activePanels.TryGetValue(type, out var existingPanel) && existingPanel != null)
            {
                return existingPanel as T;
            }

            if (!_prefabCache.TryGetValue(type, out var prefab))
            {
                Debug.LogError($"No prefab found for panel type {type.Name}");
                return null;
            }

            var panelObject = Instantiate(prefab.gameObject, panelContainer);
            var panel = panelObject.GetComponent<T>();

            if (panel == null)
            {
                Debug.LogError($"Prefab does not contain component {type.Name}");
                Destroy(panelObject);
                return null;
            }

            RegisterPanel(panel);
            return panel;
        }

        public T GetActivePanel<T>() where T : UIPanel
        {
            if (_activePanels.TryGetValue(typeof(T), out var panel) && panel != null && panel.gameObject.activeSelf)
                return panel as T;
            return null;
        }

        public List<UIPanel> GetAllActivePanels()
        {
            var activeList = new List<UIPanel>();
            foreach (var panel in _activePanels.Values)
            {
                if (panel != null && panel.gameObject.activeSelf)
                    activeList.Add(panel);
            }
            return activeList;
        }

        public List<T> GetAllActivePanelsOfType<T>() where T : UIPanel
        {
            var result = new List<T>();
            foreach (var panel in _activePanels.Values)
            {
                if (panel != null && panel.gameObject.activeSelf && panel is T typedPanel)
                {
                    result.Add(typedPanel);
                }
            }
            return result;
        }
    }
}
