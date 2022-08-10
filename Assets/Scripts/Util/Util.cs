using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public enum _Color {Green, Blue, Red, Yellow, Cyan, Purple, White, Black, Orange, Brown, Gray}

public enum Direction {Right, Up, Left, Down}


/*
public class MultipleObjectPool : MonoBehaviour {
	private List<GameObject> prefabs = null;
	private int initialSize;
	
	public Dictionary<GameObject, ObjectPool> map;
	
	public MultipleObjectPool(List<GameObject> prefabs, int initialSize) {
		this.prefabs = prefabs;
		this.initialSize = initialSize;
		
		map = new Dictionary<GameObject, ObjectPool>();
		
		foreach (GameObject prefab in prefabs) {
			map.Add(prefab, new ObjectPool(prefab, initialSize));
		}
	}
	
	public GameObject GetObject(GameObject prefab) {
		return map[prefab].GetObject();
	}
	
	public GameObject GetObject() {
		return map[GetRandomPrefab()].GetObject();
	}
	
	
	private GameObject GetRandomPrefab() {
		return prefabs[UnityEngine.Random.Range(0, prefabs.Count)];
	}
	
	public void ReturnAllObjectsToPool() {
		foreach (GameObject prefab in map.Keys) {
			map[prefab].ReturnAllObjectsToPool();
		}
	}
}
*/

public static class BezierCurve
{
    //Update the positions of the rope section
    public static void GetBezierCurve(Vector3 A, Vector3 B, Vector3 C, Vector3 D, List<Vector3> allRopeSections)
    {
        //The resolution of the line
        //Make sure the resolution is adding up to 1, so 0.3 will give a gap at the end, but 0.2 will work
        float resolution = 0.1f;

        //Clear the list
        allRopeSections.Clear();


        float t = 0;

        while(t <= 1f)
        {
            //Find the coordinates between the control points with a Bezier curve
            Vector3 newPos = DeCasteljausAlgorithm(A, B, C, D, t);

            allRopeSections.Add(newPos);

            //Which t position are we at?
            t += resolution;
        }

        allRopeSections.Add(D);
    }

    //The De Casteljau's Algorithm
    static Vector3 DeCasteljausAlgorithm(Vector3 A, Vector3 B, Vector3 C, Vector3 D, float t)
    {
        //Linear interpolation = lerp = (1 - t) * A + t * B
        //Could use Vector3.Lerp(A, B, t)

        //To make it faster
        float oneMinusT = 1f - t;

        //Layer 1
        Vector3 Q = oneMinusT * A + t * B;
        Vector3 R = oneMinusT * B + t * C;
        Vector3 S = oneMinusT * C + t * D;

        //Layer 2
        Vector3 P = oneMinusT * Q + t * R;
        Vector3 T = oneMinusT * R + t * S;

        //Final interpolated position
        Vector3 U = oneMinusT * P + t * T;

        return U;
    }
}
	 
public static class Util 
{
	
	public static bool FloatEqual(float a, float b) {
		return Mathf.Abs(a - b) < Mathf.Epsilon;
	}
	
	public static void decreaseAlpha(ref Color color, float amount) {
		color.a -= amount;
	}
	
	/*
	public static void TimeSinceSeconds(DateTime time) {
		
	}
	*/
		/*
        public static readonly Color Green = new Color(0, "green");
        BLUE(Color.BLUE, 1),
        RED(Color.RED, 2),
        YELLOW(Color.YELLOW, 3),
        CYAN(Color.CYAN, 4),
        LIGHT_YELLOW(new Color(0xffff0066), 5),
        WHITE(Color.WHITE, 6),
        BLACK(Color.BLACK, 7),
        ORANGE(Color.ORANGE, 8),
        BROWN(new Color(0xa64200aa), 9),
        GRAY(Color.GRAY, 10),
        LIGHT_GRAY(Color.LIGHT_GRAY, 11),
        LIGHTEST_YELLOW(new Color(0xffffff99), 12),
        LIGHT_GREEN(new Color(0xceffb0ff), 13),
        LIGHT_RED(new Color(0xffb0b0ff), 14),
        LIGHT_ORANGE(new Color(0xffd181ff), 15),
        LIGHT_CYAN(new Color(0xadffefff), 16),
        LIGHT_BLUE(new Color(0xadd6ffff), 17),
        LIGHT_PURPLE(new Color(0xe5adffff), 18),
        LIGHT_PINK(new Color(0xf8c4eaff), 19),
        DARK_GRAY(Color.DARK_GRAY, 20),
        DARK_YELLOW(new Color(0x625f00ff), 21),
        DARK_GREEN(new Color(0x1a5400ff), 22),
        DARK_RED(new Color(0x710000ff), 23),
        DARK_ORANGE(new Color(0x7c6000ff), 24),
        DARK_CYAN(new Color(0x005e5fff), 25),
        DARK_BLUE(new Color(0x00237cff), 26),
        DARK_PURPLE(new Color(0x3a007cff), 27),
        DARK_PINK(new Color(0x72007cff), 28),
        DARK_YELLOW2(new Color(0x625f00aa), 29),
        DARK_GREEN2(new Color(0x1a5400aa), 30),
        DARK_RED2(new Color(0x710000aa), 31),
        DARK_ORANGE2(new Color(0x7c6000aa), 32),
        DARK_CYAN2(new Color(0x005e5faa), 33),
        DARK_BLUE2(new Color(0x00237caa), 34),
        DARK_PURPLE2(new Color(0x3a007caa), 35),
        DARK_PINK2(new Color(0x72007caa), 36);
		*/

	private static readonly Dictionary<int, Color> map = new Dictionary<int, Color>();
	
	private static readonly Dictionary<_Color, int> map2 = new Dictionary<_Color, int>();
	
	public static readonly List<_Color> colorList = new List<_Color>();

	static Util() {
		/*
		map.Add(0, _Color.Green);
		map.Add(1, _Color.Blue);
		map.Add(2, _Color.Red);
		map.Add(3, _Color.Yellow);
		map.Add(4, _Color.Cyan);
		map.Add(5, _Color.Purple); //purple
		map.Add(6, _Color.White);
		map.Add(7, _Color.Black);
		map.Add(8, _Color.Orange); //orange
		map.Add(9, _Color.Brown); //brown
		map.Add(10, _Color.Gray);
		*/
		
		
		map.Add(0, Color.green);
		map.Add(1, Color.blue);
		map.Add(2, Color.red);
		map.Add(3, Color.yellow);
		map.Add(4, Color.cyan);
		map.Add(5, new Color32( 255 , 51 , 255, 1 )); //purple
		map.Add(6, Color.white);
		map.Add(7, Color.black);
		map.Add(8, new Color32(255, 165, 0, 1)); //orange
		map.Add(9, new Color32(255, 85, 0, 1)); //brown
		map.Add(10, Color.gray);
		
		
		map2.Add(_Color.Green, 0);
		map2.Add(_Color.Blue, 1);
		map2.Add(_Color.Red, 2);
		map2.Add(_Color.Yellow, 3);
		map2.Add(_Color.Cyan, 4);
		map2.Add(_Color.Purple, 5); //purple
		map2.Add(_Color.White, 6);
		map2.Add(_Color.Black, 7);
		map2.Add(_Color.Orange, 8); //orange
		map2.Add(_Color.Brown, 9); //brown
		map2.Add(_Color.Gray, 10);
		
		foreach(var item in map2.Keys)
		{
			colorList.Add(item);
		}
		
		/*

		map.put(11, LIGHT_GRAY);
		map.put(12, LIGHTEST_YELLOW);
		map.put(13, LIGHT_GREEN);
		map.put(14, LIGHT_RED);
		map.put(15, LIGHT_ORANGE);
		map.put(16, LIGHT_CYAN);
		map.put(17, LIGHT_BLUE);
		map.put(18, LIGHT_PURPLE);
		map.put(19, LIGHT_PINK);
		map.put(20, DARK_GRAY);
		map.put(21, DARK_YELLOW);
		map.put(22, DARK_GREEN);
		map.put(23, DARK_RED);
		map.put(24, DARK_ORANGE);
		map.put(25, DARK_CYAN);
		map.put(26, DARK_BLUE);
		map.put(27, DARK_PURPLE);
		map.put(28, DARK_PINK);
		map.put(29, DARK_YELLOW2);
		map.put(30, DARK_GREEN2);
		map.put(31, DARK_RED2);
		map.put(32, DARK_ORANGE2);
		map.put(33, DARK_CYAN2);
		map.put(34, DARK_BLUE2);
		map.put(35, DARK_PURPLE2);
		map.put(36, DARK_PINK2);
		*/
	}

	public static Color GetColorByID(int id) {
		return map[id] != null ? map[id] : Color.green;
	}

	public static int GetColorLastID() {
		return map.Count - 1;
	}
	
	public static int GetColorIDByType(_Color color) {
		return map2[color] != null ? map2[color] : 0;
	}
	
	public static _Color GetRandomColor() {
		return colorList[UnityEngine.Random.Range(0, colorList.Count)];
	}
	
	public static T GetEnumValue<T>(int intValue) where T : struct, IConvertible
	{
		if (!typeof(T).IsEnum)
		{
			throw new Exception("T must be an Enumeration type.");
		}
		T val = ((T[])Enum.GetValues(typeof(T)))[0];

		foreach (T enumValue in (T[])Enum.GetValues(typeof(T)))
		{
			if (Convert.ToInt32(enumValue).Equals(intValue))
			{
				val = enumValue;
				break;
			}             
		}
		return val;
	}
	
	public static string ToDebugString<TKey, TValue> (IDictionary<TKey, TValue> dictionary) {
		//return "{" + string.Join(",", dictionary.Select(kv => kv.Key + "=" + kv.Value).ToArray()) + "}";
		return "";
	}
	
    public static bool IsList(Type type) {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
    }
}
