using System;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class Skin
{
    public string name;
    public Transform parent;
    public TRS spawnMatrix;

    private GameObject _currentModel;

    public Skin() { }
    
    public Skin(GameObject model, Transform parent) {

        name = model.name;
        this.parent = parent;
        spawnMatrix = new TRS(model.transform);
        
    }

    public void SetActive(bool active) {

        if (!active) {
            if (_currentModel != null) {
                if (Application.isEditor && Application.isPlaying == false)
                    Object.DestroyImmediate(_currentModel);
                else
                    Object.Destroy(_currentModel);
            }
        }
        else {

            if (_currentModel != null) {
                return;
            }

            _currentModel = Object.Instantiate(Resources.Load(name), parent, false) as GameObject;
            spawnMatrix.Apply(_currentModel.transform);
            _currentModel.SetActive(true);

        }

    }
}

[System.Serializable]
public struct TRS {

    public Vector3 LocalPosition;
    public Vector3 LocalRotation;
    public Vector3 LocalScale;

    public TRS(Transform target) {
        LocalPosition = target.localPosition;
        LocalRotation = target.localEulerAngles;
        LocalScale = target.localScale;
    }

    public void Apply(Transform target) {
        target.localPosition = LocalPosition;
        target.localEulerAngles = LocalRotation;
        target.localScale = LocalScale;
    }

}