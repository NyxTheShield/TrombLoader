using System;
using UnityEngine;
using UnityEngine.Events;

namespace TrombLoader.Data
{
    [Serializable, RequireComponent(typeof(TromboneEventManager))]
    public class BackgroundEvent : MonoBehaviour
    {
        [SerializeField]
        public int BackgroundEventID;

        public UnityEvent UnityEvent;
    }
}
