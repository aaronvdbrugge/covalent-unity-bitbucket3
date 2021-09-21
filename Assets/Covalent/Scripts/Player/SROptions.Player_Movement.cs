using System.ComponentModel;
using SRDebugger;

/// <summary>
/// Allows player speed to be configured from SRDebugger.
/// </summary>
public partial class SROptions {

	// Allows setting of player max speed...
	[Category("Movement")] 
	public float PlayerMaxSpeed {
		get => Player_Controller_Mobile.mine.playerMovement.maxSpeed;
		set => Player_Controller_Mobile.mine.playerMovement.maxSpeed = value;
	}
}