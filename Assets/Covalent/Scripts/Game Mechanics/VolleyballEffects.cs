using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles particle effects, sprite color change etc, of the volleyball
/// </summary>
public class VolleyballEffects : MonoBehaviour
{
    [Header("References")]
    public VolleyBall volleyball;

    [Tooltip("Smoke effect")]
    public ParticleSystem firstParticles; 
    [Tooltip("Fire effect")]
    public ParticleSystem secondParticles;
    
    [Header("Settings")]
    [Tooltip("The ball will start smoking...")]
    public int hitThresholdFirst = 8;

    [Tooltip("The ball will catch fire!!")]
    public int hitThresholdSecond = 15;


    [Tooltip("Sound cue under CameraSound")]
    public string firstSound = "volleyball_smoke";
    public string secondSound = "volleyball_fire";

    public Color firstColor = Color.white;
    public Color secondColor = Color.white;

    [Tooltip("We'll do a shake effect in this radius")]
    public float firstShakeAmt = 0.1f;
    public float secondShakeAmt = 0.1f;

    [Tooltip("We'll shake for this long")]
    public float firstShakeTime = 0.5f;
    public float secondShakeTime = 0.5f;






    float _firstShakeCooldown = 0;
    float _secondShakeCooldown = 0;
    int _lastHitStreak = int.MaxValue;

    Camera_Sound _cameraSound;

	private void Start()
	{
		_cameraSound = Camera.main.GetComponent<Camera_Sound>();

        //Disable particle effects
        firstParticles.Stop();
        secondParticles.Stop();
	}


	public void Update()
	{
		if( volleyball.hitStreak != _lastHitStreak )  // hit streak changes.
        {
            _lastHitStreak = volleyball.hitStreak;

            if( _lastHitStreak == hitThresholdFirst )   // just passed the threshold
            {
                _cameraSound.PlaySoundAtPosition(firstSound, volleyball.ballSprite.transform.position );
                _firstShakeCooldown = firstShakeTime;  // start shaking!
            }
            else if( _lastHitStreak == hitThresholdSecond )   // just passed the threshold
            {
                _cameraSound.PlaySoundAtPosition(secondSound, volleyball.ballSprite.transform.position );
                _secondShakeCooldown = secondShakeTime;  // start shaking!
            }

            // Enable / disable particle effects
            if (_lastHitStreak >= hitThresholdFirst)
                firstParticles.Play();
            else
                firstParticles.Stop();

            if (_lastHitStreak >= hitThresholdSecond)
                secondParticles.Play();
            else
                secondParticles.Stop();


            //Change color of the sprite
            volleyball.ballSprite.color = Color.white;
            if( _lastHitStreak >= hitThresholdFirst )
                volleyball.ballSprite.color = firstColor;
            else if ( _lastHitStreak >= hitThresholdSecond )
                volleyball.ballSprite.color = secondColor;

        }


        //Shake sprite around
        volleyball.spriteOffset = Vector2.zero;
        if( _firstShakeCooldown > 0 )
        {
            _firstShakeCooldown -= Time.deltaTime;
            volleyball.spriteOffset = Random.insideUnitCircle * firstShakeAmt * (_firstShakeCooldown / firstShakeTime);
        } 
        else if( _secondShakeCooldown > 0 )
        {
            _secondShakeCooldown -= Time.deltaTime;
            volleyball.spriteOffset = Random.insideUnitCircle * secondShakeAmt * (_secondShakeCooldown / secondShakeTime);
        }
	}



}
