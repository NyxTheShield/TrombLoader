using System;
using BaboonAPI.Hooks.Tracks;
using UnityEngine;

namespace TrombLoader.CustomTracks.Backgrounds;

public abstract class AbstractBackground : IDisposable
{
    protected AssetBundle Bundle;

    protected AbstractBackground(AssetBundle bundle)
    {
        Bundle = bundle;
    }

    public abstract GameObject Load(BackgroundContext ctx);

    public abstract void SetUpBackground(BGController controller, GameObject bg);

    public void Dispose()
    {
        if (Bundle != null)
        {
            Bundle.Unload(false);
        }
    }
}