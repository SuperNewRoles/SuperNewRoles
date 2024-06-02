using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperNewRoles.CustomObject;
using UnityEngine;

namespace SuperNewRoles.WaveCannonObj;
public interface IWaveCannonAnimationHandler
{
    public WaveCannonObject CannonObject { get; }
    public CustomAnimationOptions Init();
    public void OnShot();
    public void RendererUpdate();
    public void OnUpdate() { }
}