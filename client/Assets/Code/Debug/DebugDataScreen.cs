using UnityEngine;
using System.Collections;
 
public class DebugDataScreen : MonoBehaviour {
	//// REF PUB
	[Header("Player Position")]
	public GameObject player;

	//// VAR PRI
	private float deltaTime = 0.0f;
	private string text;
	private Rect rect;

	void Update() {
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
	}

	void OnGUI() {
		int w = Screen.width, h = Screen.height;

		GUIStyle style = new GUIStyle();

	
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 100;
		style.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

		//FPS
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		rect = new Rect(130, 0, w, h * 2 / 100);
		text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		GUI.Label(rect, text, style);

		//Posicion jugador
		rect = new Rect(130, 25, w, h * 2 / 100);
		text = "X: " + player.transform.position.x + " | Y: " + player.transform.position.y;
		GUI.Label(rect, text, style);
	}
}