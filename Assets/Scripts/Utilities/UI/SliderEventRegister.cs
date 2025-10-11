using UnityEngine.Events;
using UnityEngine.UI;

namespace PlayingCard.Utilities.UI
{
    public static class SliderEventRegister
    {
        public static void AddOnChangeEvent(this Slider slider, UnityAction<float> action)
        {
#if UNITY_EDITOR
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(slider.onValueChanged, action);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(slider.onValueChanged, action);
#else
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(action);
#endif
        }
    }
}
