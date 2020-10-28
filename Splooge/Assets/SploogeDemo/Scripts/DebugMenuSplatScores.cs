using UnityEngine;
using System.Collections;

public class DebugMenuSplatScores : MonoBehaviour 
{
	public Texture2D m_sliderYellow;
	public Texture2D m_sliderRed;
	public Texture2D m_sliderGreen;
	public Texture2D m_sliderBlue;

	void OnGUI () 
	{
		Vector4 scores = SplatManager.Instance.Scores + new Vector4(0.001f,0.001f,0.001f,0.001f);
		float totalScores = scores.x + scores.y + scores.z + scores.w;
		int yelowScore = (int)( 512 * ( scores.x / totalScores ) );
		int redScore = (int)( 512 * ( scores.y / totalScores ) );
		int greenScore = (int)( 512 * ( scores.z / totalScores ) );
		int blueScore = (int)( 512 * ( scores.w / totalScores ) );

		GUI.DrawTexture(new Rect(20, 20, yelowScore, 30), m_sliderYellow);
		GUI.DrawTexture(new Rect(20, 60, redScore, 30), m_sliderRed);
		GUI.DrawTexture(new Rect(20, 100, greenScore, 30), m_sliderGreen);
		GUI.DrawTexture(new Rect(20, 140, blueScore, 30), m_sliderBlue);
	}
}
