using UnityEngine;
using UnityEngine.Rendering;

namespace TrombLoader.Helpers;

public class SceneSortingHelper:MonoBehaviour
{
    public TransparencySortMode sortingMode = TransparencySortMode.Perspective;
    public Vector3 sortingAxis = new Vector3(0.0f, 1.0f, 1.0f);
    
    public void Start()
    {
        GraphicsSettings.transparencySortMode = sortingMode;
        GraphicsSettings.transparencySortAxis = sortingAxis;
    }
}