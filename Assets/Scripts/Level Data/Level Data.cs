using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData {
	public List<TankInfo> tankInfos;
}

[System.Serializable]
public class TankInfo {

}

/*
[System.Serializable]
public class SerializedVector {
	public float x;
	public float y;
	public float z;

	public Vector3 Convert() {
		return new Vector3(x, y, z);
	}
	
	public static SerializedVector ConvertToSerialized(this Vector3 vector) {
        return new SerializedVector() { x=vector.x, y=vector.y, z=vector.z};
    }

}
*/