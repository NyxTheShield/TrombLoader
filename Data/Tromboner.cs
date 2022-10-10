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

        public Tromboner(GameObject _gameObject)
        {
            gameObject = _gameObject;
            transform = _gameObject.transform;
            controller = _gameObject.GetComponent<HumanPuppetController>();
        }
    }
}
