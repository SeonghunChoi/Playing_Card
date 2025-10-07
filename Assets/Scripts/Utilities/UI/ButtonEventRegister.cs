using UnityEngine.Events;
using UnityEngine.UI;

namespace PlayingCard.Utilities.UI
{
    public static class ButtonEventRegister
    {
        public static void AddOnClickEvent(this Button button, UnityAction action)
        {
#if UNITY_EDITOR
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(button.onClick, action);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, action);
#else
		    button.onClick.RemoveAllListeners();
		    button.onClick.AddListener(action);
#endif
        }
    }
}
