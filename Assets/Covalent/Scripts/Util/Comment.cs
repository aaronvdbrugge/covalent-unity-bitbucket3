using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Just let us leave comments in the inspector.
/// </summary>
public class Comment : MonoBehaviour
{
	[TextArea(5, 20)]
	public string comment;
}
