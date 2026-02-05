using System;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using MelonLoader;
using UnityEngine;

namespace TimeMaster;

[RegisterTypeInIl2Cpp]
public class TransformTimeMaster : MonoBehaviour
{
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
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 eulerAngles = ((Component)this).transform.eulerAngles;
		eulerAngles.x = 345f;
		((Component)this).transform.eulerAngles = eulerAngles;
	}
}
