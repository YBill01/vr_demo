
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3_Interpolator : IInterpolationPattern<Vector3> {

	public Vector3 Interpolate(Vector3 current, float lerpAmount, Interpolation<Vector3>.FrameUpdate to, Interpolation<Vector3>.FrameUpdate from, Interpolation<Vector3>.FrameUpdate previous) {

		if (to.Value != from.Value) {
			return Vector3.Lerp(from.Value, to.Value, lerpAmount);
		}

		return current;

	}

}
