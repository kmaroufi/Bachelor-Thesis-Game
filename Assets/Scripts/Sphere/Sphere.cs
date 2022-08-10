using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
struct ExampleVertex
{
    public Vector3 pos;
	public Color32 color;
    public Vector2 textcoord0;
}

public class Sphere : MonoBehaviour
{
	public LiquidColor? color;
	public LiquidColor? previousColor;
	public Valve valve;
	public Tank tank;
	public float targetSize;
	public bool toDestroy;
	public bool toSmoke;
	public bool touchedBottomTrigger;
	
	private GameManager gm;
	
    // Start is called before the first frame update
    void Start()
    {
		gm = GameManager.Instance;
		toDestroy = false;
		toSmoke = false;
		touchedBottomTrigger = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (targetSize > 0) {
			float sphereSize = transform.localScale.x;
			sphereSize += 0.05f;
			if (sphereSize > targetSize) {
				sphereSize = targetSize;
				targetSize = -1;
			}
			transform.localScale = new Vector3(sphereSize, sphereSize, sphereSize);
		}
    }
	
	public bool IsInSourceTank() {
		return valve == null;
	}
	
	public void SetColor(LiquidColor color) {
		if (gm == null) gm = GameManager.Instance;
		
		if (this.color != null) {
			previousColor = this.color;
		}
		
		this.color = color;
		gameObject.layer = gm.GetLiquidLayer(color);
		GetComponent<MeshRenderer> ().sharedMaterial = gm.liquidColors[(int)color];
		//GetComponent<SpriteRenderer> ().sharedMaterial = gm.spriteMaterial;
		
		return;
		/*
		Color c = gm.liquidColors[(int)color].GetColor("_Color");
		Color[] array = new Color[GetComponent<MeshFilter>().mesh.vertices.Length];
		for (int i = 0; i < array.Length; i++) {
			array[i] = c;
		}
		Debug.Log(c);
		GetComponent<MeshFilter>().mesh.colors = array;
		*/
		
		var mesh = GetComponent<MeshFilter>().mesh;
        // specify vertex count and layout
        var layout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
			new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
        };
        var vertexCount = mesh.vertexCount;
        mesh.SetVertexBufferParams(vertexCount, layout);

        // set vertex data
        var verts = new NativeArray<ExampleVertex>(vertexCount, Allocator.Temp);
		//var verts = new List<ExampleVertex>(vertexCount);
		Color32 c = gm.liquidColors[(int)color].GetColor("_Color");
		
		/*
		struct ExampleVertex* ptr = verts;
		for (int i = 0; i < vertexCount; i++, ptr++) {
			ptr->color = c;
		}*/
		Vector3[] v = mesh.vertices;
		Vector2[] uv = mesh.uv;
		for (int i = 0; i < vertexCount; i++) {
			var element = verts[i];
			element.pos = v[i];
			element.color = c;
			element.textcoord0 = uv[i];
			verts[i] = element;
			Debug.Log("e: " + element.color);
		}
		
		/*
		Color32[] array = new Color32[GetComponent<MeshFilter>().mesh.vertices.Length];
		for (int i = 0; i < array.Length; i++) {
			array[i] = c;
		}
		GetComponent<MeshFilter>().mesh.colors32 = array;*/
		
        mesh.SetVertexBufferData(verts, 0, 0, vertexCount);
		
		/*
		int numVerticesTotal = verts.Length// how many verts in your mesh
 
		int bytesPerFloat = 4;

		int floatsInCustomAttribute = 3; ... // how many floats in your struct. I use 3 in this example
		int structSize = bytesPerFloat * floatsInCustomAttribute;
		ComputeBuffer cb = new ComputeBuffer( numVerticesTotal, structSize );
		float[,] values = new float[numVerticesTotal,floatesInCustomAttribute];
		for( int i = 0; i < values.GetLength( 0 ); i++ ) {
			for( int structFloat=0; structFloat<floatsInCustomAttribute; structFloat++) {
				// Fill in your data here. I'm putting [..,0] = 1, [..,1] = 0, [..,2] = 0
				// i.e. a float3( 1,0,0 )
				// ... which the shader will read as "the color red"

				if( structFloat == 0 )
					values[ i, structFloat ] = 1f;
				else
					values[ i, structFloat ] = 0f;
			}
		}
		cb.SetData( values );
		Debug.Log( "Generated data for "+values.Length+" vertices" );

		MeshRenderer mr = .. // your mesh renderer
		foreach( Material subMaterial in mr.materials ) // will leak, but works for doing a debug / test
		{
		subMaterial.SetBuffer( "customAttributes", cb );
		Debug.Log( "Added vertex-attributes to material = "+subMaterial );
		}
		*/
	}
}
