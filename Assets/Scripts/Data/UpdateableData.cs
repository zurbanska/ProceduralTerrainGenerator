using UnityEngine;
using System.Collections;

public class UpdatableData : ScriptableObject {

	public event System.Action OnValuesUpdated;
	public bool autoUpdate;
    private bool isUpdating;

	protected virtual void OnValidate() {
		if (autoUpdate && !isUpdating) {
			NotifyOfUpdatedValues ();
		}
	}


	public void NotifyOfUpdatedValues() {
        if (isUpdating) return; // Avoid recursion
        isUpdating = true;
        OnValuesUpdated?.Invoke();
        isUpdating = false;
    }

}
