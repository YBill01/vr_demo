
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TODO - change to scene 3side management architecture
public class GameInPoint : MonoBehaviour {

	[SerializeField]
	private List<CustomStartInstuction> _instructions = new List<CustomStartInstuction>();

	private void Awake() {
		CoroutineBehavior.StartCoroutine(InitializeRoutine());
	}

	private IEnumerator InitializeRoutine() {

		_instructions?.ForEach(x => x?.Initialize());

		yield return null;
		Game.OnInitialize();
		yield return null;
		Game.OnStart();
		yield return null;
		Game.OnFinalSetup();
		yield return null;

		_instructions?.ForEach(x => x?.OnGameInitialized());

	}

}

public abstract class CustomStartInstuction : MonoBehaviour {
	public abstract void Initialize();
	public virtual void OnGameInitialized() { }
}


