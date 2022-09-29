using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

#if UNITY
using UnityEditor;
#endif

namespace TrombLoader.Data
{
    [Serializable] public class Vector3Event : UnityEvent<Vector3> { }
    [Serializable] public class IntEvent : UnityEvent<int> { }

    [Serializable, DisallowMultipleComponent]
    public class TromboneEventManager : MonoBehaviour
    {
        [NonSerialized]
        public BackgroundEvent[] Events;

        public UnityEvent OnBeat;
        public UnityEvent OnBar;
        public UnityEvent NoteStart;
        public UnityEvent NoteEnd;

        public IntEvent ComboUpdated;
        public Vector3Event MousePositionUpdated;

        private Vector3 mousePosition;

        [SerializeField, HideInInspector]
        private string serializedMousePositionJson;

        [SerializeField, HideInInspector]
        private UnityEngine.Object[] serializedMousePositionTargets;

        [SerializeField, HideInInspector]
        private string serializedComboJson;

        [SerializeField, HideInInspector]
        private UnityEngine.Object[] serializedComboTargets;

        public void SerializeAllGenericEvents()
        {
            // I wanted to use JSON used elsewhere here
            // Unfortunately it doesn't quite serialize objects the way JsonUtility does
            serializedMousePositionJson = JsonUtility.ToJson(MousePositionUpdated);
            serializedComboJson = JsonUtility.ToJson(ComboUpdated);
#if UNITY
            serializedMousePositionTargets = GetTargets(this, "MousePositionUpdated");
            serializedComboTargets = GetTargets(this, "ComboUpdated");
#endif
        }

        public void DeserializeAllGenericEvents()
        {
            if (Events == null)
            {
                Events = GetComponents<BackgroundEvent>();
            }

            if (!string.IsNullOrEmpty(serializedMousePositionJson))
            {
                MousePositionUpdated = JsonUtility.FromJson<Vector3Event>(serializedMousePositionJson);
                AssignTargets(MousePositionUpdated, serializedMousePositionTargets);
            }

            if (!string.IsNullOrEmpty(serializedComboJson))
            {
                ComboUpdated = JsonUtility.FromJson<IntEvent>(serializedComboJson);
                AssignTargets(ComboUpdated, serializedComboTargets);
            }

        }


        // should be completely unnecessary but unity really does not like serializing generic UnityEvents in AssetBundles
        // thank you ckosmic
        public static void AssignTargets(UnityEventBase unityEvent, UnityEngine.Object[] objects)
        {
            BindingFlags bindings = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
            Type PersistentCall = typeof(UnityEventBase).Assembly.GetType("UnityEngine.Events.PersistentCall");
            var m_PersistentCalls = typeof(UnityEventBase).GetField("m_PersistentCalls", bindings);
            var m_Calls = m_PersistentCalls.FieldType.GetField("m_Calls", bindings);
            var _items = m_Calls.FieldType.GetField("_items", bindings);

            object persistentCalls = m_PersistentCalls.GetValue(unityEvent);
            object calls = m_Calls.GetValue(persistentCalls);
            object[] items = (object[])_items.GetValue(calls);

            var m_Target = PersistentCall.GetField("m_Target", bindings);
            for (int i = 0; i < items.Length; i++)
            {
                m_Target.SetValue(items[i], objects[i]);
            }
        }

#if UNITY
        public static UnityEngine.Object[] GetTargets(UnityEngine.Object obj, string eventName)
        {
            UnityEngine.Object[] targets;
            SerializedObject so = new SerializedObject(obj);
            SerializedProperty persistentCalls = so.FindProperty(eventName).FindPropertyRelative("m_PersistentCalls.m_Calls");
            targets = new UnityEngine.Object[persistentCalls.arraySize];
            for (int i = 0; i < persistentCalls.arraySize; i++)
            {
                targets[i] = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Target").objectReferenceValue;
            }
            return targets;
        }
#endif

        public void Update()
        {
            if (!string.IsNullOrEmpty(serializedMousePositionJson) && MousePositionUpdated == null)
            {
                DeserializeAllGenericEvents();
            }
            else if (!string.IsNullOrEmpty(serializedComboJson) && ComboUpdated == null)
            {
                DeserializeAllGenericEvents();
            }
            if (MousePositionUpdated != null)
            {
                if ((Vector3)Input.mousePosition != mousePosition)
                {
                    mousePosition = Input.mousePosition;
                    MousePositionUpdated.Invoke(new Vector3(mousePosition.x / Screen.width, mousePosition.y / Screen.height, 0));
                }
            }
        }
    }
}
