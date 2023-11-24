using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using JUTPS.InputEvents;
using JUTPSEditor.JUHeader;
namespace JUTPS
{
    [AddComponentMenu("JU TPS/Utilities/JU Pause Game")]
    public class JUPauseGame : MonoBehaviour
    {
        public static JUPauseGame instance;
        public static bool Paused;
        
        [JUHeader("Pause Input")]
        public MultipleActionEvent PauseInputs;

        [JUHeader("On Pause Events")]
        public UnityEvent OnPause;
        public UnityEvent OnUnpause;
        private FX.JUSlowmotion SlowmotionInstance;
        void Start()
        {
            instance = this;
            SlowmotionInstance = FindObjectOfType<FX.JUSlowmotion>();
            PauseInputs.OnButtonsDown.AddListener(Pause);
        }
        private void OnEnable() { PauseInputs.Enable(); }
        private void OnDisable() { PauseInputs.Disable(); }
        
        public static void Pause()
        {
            //Update state
            Paused = !Paused;
            //Update time scale
            Time.timeScale = Paused ? 0 : 1;
            //Trigger events
            if (Paused) { instance.OnPause.Invoke(); } else { instance.OnUnpause.Invoke(); }

            //Disable / Enable Slowmotion
            instance.SlowmotionInstance.EnableSlowmotion = !Paused;
        }
    }
}