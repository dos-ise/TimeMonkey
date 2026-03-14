using System;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using MelonLoader;
using UnityEngine;

namespace TimeMaster;

/// <summary>
/// Keeps the Time Master model tilted at the correct angle when a custom
/// 3-D asset-bundle display is used.  Attach via
/// <c>gameObject.AddComponent&lt;TransformTimeMaster&gt;()</c> from the
/// display-factory hook.
/// </summary>
[RegisterTypeInIl2Cpp]
public class TransformTimeMaster : MonoBehaviour
{
    // Il2Cpp interop constructors – required boilerplate
    public TransformTimeMaster(IntPtr obj0)
        : base(obj0)
    {
        ClassInjector.DerivedConstructorBody((Il2CppObjectBase)(object)this);
    }

    public TransformTimeMaster()
        : base(ClassInjector.DerivedConstructorPointer<TransformTimeMaster>())
    {
    }

    public void Update()
    {
        // Lock the X-rotation so the model always faces the right direction
        Vector3 angles = transform.eulerAngles;
        angles.x = 345f;
        transform.eulerAngles = angles;
    }
}
