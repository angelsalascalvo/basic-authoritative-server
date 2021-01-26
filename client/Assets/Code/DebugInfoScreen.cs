using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Mostrar informacion de control o debug en la interfaz grafica de la ejecucion
/// </summary>
public class DebugInfoScreen : MonoBehaviour {
	//// REF PUB
	[Header("Player Position")]
	public GameObject player;
	[Header("Screen Texts")]
	public Text textFPS, textPosition, textDiff;

	//// VAR PRI
	private float deltaTime = 0.0f;
	private string text;
	private Vector2 diff;

	//------------------------------------------------------------->

	/// <summary>
	/// Ejecucion cada fotograma
	/// </summary>
	void Update() {
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

		//FPS
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		textFPS.text = text;

		//Posicion jugador
		text = "X: " + player.transform.position.x + " | Y: " + player.transform.position.y;
		textPosition.text = text;

		//Diferencia entre posicion jugador en local con servidor
		textDiff.text = "Local vs Server (X: " + diff.x + " | Y: " + diff.y+")";
	}


	//-------------------------------------------------------------//
	//                        SETs + GETs                          //
	//-------------------------------------------------------------//

	public void SetDiff(Vector2 diff) {
		this.diff = diff;
	}
}