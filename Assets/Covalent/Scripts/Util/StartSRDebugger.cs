using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Can connect a button to this
/// </summary>
public class StartSRDebugger : MonoBehaviour
{
	public void DoStart()
	{
		SRDebug.Instance.ShowDebugPanel(false);
	}
}
