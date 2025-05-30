using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Michsky.DreamOS
{
    [CreateAssetMenu(fileName = "New Widget Library", menuName = "DreamOS/New Widget Library")]
    public class WidgetLibrary : ScriptableObject
    {
        // Library Content
        public List<WidgetItem> widgets = new List<WidgetItem>();

        [System.Serializable]
        public class WidgetItem
        {
            public string widgetTitle = "Widget Title";
            [TextArea] public string widgetDescription = "Description";
            public Sprite widgetIcon;
            public GameObject widgetPrefab;
        }
    }
}