using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TrombLoader.Data
{
    public class Tromboner
    {
        public GameObject gameObject;
        public Transform transform;
        public HumanPuppetController controller;
        public TrombonerPlaceholder placeholder;

        public Tromboner(GameObject _gameObject, TrombonerPlaceholder _placeholder)
        {
            gameObject = _gameObject;
            transform = _gameObject.transform;
            controller = _gameObject.GetComponent<HumanPuppetController>();
            placeholder = _placeholder;
        }
    }
}
