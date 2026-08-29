using PhumFarm.Core;
using UnityEngine;

namespace PhumFarm
{
    public sealed class PhumFarmApplication : MonoBehaviour
    {
        private GameFlowController flow;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureApplication()
        {
            if (FindAnyObjectByType<PhumFarmApplication>() != null) return;
            new GameObject("PhumFarmApplication").AddComponent<PhumFarmApplication>();
        }

        private void Awake()
        {
            if (FindObjectsByType<PhumFarmApplication>(FindObjectsInactive.Exclude).Length > 1) { Destroy(gameObject); return; }
            DontDestroyOnLoad(gameObject);
            GameServices.Ensure();
            flow = gameObject.AddComponent<GameFlowController>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            gameObject.AddComponent<DevelopmentDebugMenu>();
#endif
        }
    }
}
