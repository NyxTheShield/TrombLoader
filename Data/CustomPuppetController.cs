using UnityEngine;

namespace TrombLoader.Data;

public class CustomPuppetController : MonoBehaviour
{
    public Tromboner Tromboner { get; set; }

    public void Start()
    {
        var controller = Tromboner.controller;
        var movementType = GetMovementType();

        LeanTween.value(movementType == 0 ? 10f : -38f, -48f, 7f).setLoopPingPong().setEaseInOutQuart().setOnUpdate(delegate (float val)
        {
            controller.p_parent.transform.localEulerAngles = new Vector3(0f, val, 0f);
        });

        controller.estudious = movementType == 1;
    }

    private int GetMovementType()
    {
        if (Tromboner.placeholder.MovementType == TrombonerMovementType.DoNotOverride)
        {
            return GlobalVariables.chosen_vibe;
        }
        else
        {
            return (int) Tromboner.placeholder.MovementType;
        }
    }
}
