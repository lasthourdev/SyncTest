using iPAHeartBeat.Core.Dependency;
using milan_UI;
using UnityEngine;

namespace milan.Core
{
    public class MainMenuPanel : UIPanel
    {
        public void OnStartBtn()
        {
            GameEvents.TriggerGameStart();
            UIManager.Instance.Show<GamePanel>();
            UIManager.Instance.Hide<MainMenuPanel>();
        }
    }
}
