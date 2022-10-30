using System;
using UnityEngine;

namespace TrombLoader.Data
{
    public class TrombonerPlaceholder : MonoBehaviour
    {
        public TrombonerType TrombonerType = TrombonerType.DoNotOverride;
        public TromboneSkin TromboneSkin = TromboneSkin.DoNotOverride;
        public TrombonerMovementType MovementType = TrombonerMovementType.DoNotOverride;

        [HideInInspector, SerializeField]
        public int InstanceID = 0;
    }

    [Serializable]
    public enum TrombonerType
    {
        [InspectorName("Do Not Override (Default)")]
        DoNotOverride = -1,
        [InspectorName("Appaloosa")]
        Female1 = 0,
        [InspectorName("Beezerly")]
        Female2 = 1,
        [InspectorName("Kaizyle II")]
        Female3 = 2,
        [InspectorName("Trixiebell")]
        Female4 = 3,
        [InspectorName("Meldor")]
        Male1 = 4,
        [InspectorName("Jermajesty")]
        Male2 = 5,
        [InspectorName("Horn Lord")]
        Male3 = 6,
        [InspectorName("Soda")]
        Male4 = 7,
        [InspectorName("Polygon")]
        Female5 = 8,
        [InspectorName("Servant Of Babi")]
        Male5 = 9,
    }

    [Serializable]
    public enum TromboneSkin
    {
        [InspectorName("Do Not Override (Default)")]
        DoNotOverride = -1,
        [InspectorName("Brass")]
        Brass = 0,
        [InspectorName("Silver")]
        Silver = 1,
        [InspectorName("Red")]
        Red = 2,
        [InspectorName("Blue")]
        Blue = 3,
        [InspectorName("Green")]
        Green = 4,
        [InspectorName("Pink")]
        Pink = 5,
        [InspectorName("Polygon")]
        Polygon = 6,
        [InspectorName("Champ")]
        Champ = 7,
    }

    [Serializable]
    public enum TrombonerMovementType
    {
        [InspectorName("Do Not Override (Default)")]
        DoNotOverride = -1,
        [InspectorName("Jubilant")]
        Jubilant = 0,
        [InspectorName("Estudious")]
        Estudious = 1,
    }
}
