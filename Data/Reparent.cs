using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace TrombLoader.Data
{
    public class Reparent : MonoBehaviour
    {
        public int instanceID;

        public void LateUpdate()
        {
            foreach(Transform child in GameObject.Find("BGCameraObj").transform.GetChild(1).GetChild(0))
            {
                if(child.name == "_" + instanceID)
                {
                    transform.parent = child.GetChild(0);
                    enabled = false;
                }
            }
        }
    }
}
