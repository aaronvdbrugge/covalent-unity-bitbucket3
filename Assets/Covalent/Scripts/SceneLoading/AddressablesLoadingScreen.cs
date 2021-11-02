using Covalent.Scripts.Util.Native_Proxy;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

/*
 Note! The Unity documentation doesn't do a great job of simplifying the rather raw new
 feature of Addressables. 
 I had MUCH better luck with this tutorial video:
 https://www.youtube.com/watch?v=KJbNsaj1c1o&ab_channel=VincentChu
*/


/// <summary>
/// Loads in the game's assets from a remote server (configured through Unity Addressables) and
/// displays progress.
/// </summary>
public class AddressablesLoadingScreen : MonoBehaviour
{
    /// <summary>
    /// Have we already been in Dateland before?
    /// If so, we'd better not go back until we for sure get a createPlayer call... testing be damned
    /// </summary>
    public static bool comingFromDateland = false;

    [Tooltip("Will be used if maxWaitForCreatePlayer counts down in Debug mode")]
    public TextAsset testUserJson;
    
    [Tooltip("Needs to be a full Addressables address.")]
    public AssetReference sceneToLoad;

    [Tooltip("Hidden on error.")]
    public GameObject progressObject;  
    public TMP_Text progressText;

    [Tooltip("Shown on error.")]
    public GameObject errorObject;
    public TMP_Text errorText;

    [Tooltip("Don't bother giving computery looking exceptions to users... just ask them to try again")]
    public string errorMessage = "A problem occurred.\nPlease try again!";

    [Tooltip("Stretches to fill its parent transform; has x pivot=0.  We'll change it accordingly")]
    public RectTransform progressBar; 
    
    [Tooltip("It just makes it easier for testing if we give up waiting for NativeEntryPoint calls and use spoofed data (for example, on-device builds that aren't integrated into native)")]
    public float maxWaitForCreatePlayer = 2.0f;

    [Tooltip("Only use maxWaitForCreatePlayer in Debug.")]
    public DebugSettings debugSettings;

    [Tooltip("If an error occurs, we'll eventually call playerDidLeaveGame")]
    public float timeUntilPlayerDidLeaveGame = 5.0f;


    [Tooltip("If error occurs, we'll prime this to restart the frame on createPlayer (next time they try again)")]
    public NativeEntryPoint createPlayerReceiver;


    [Header("Testing")]

    [Tooltip("Just simulate what it would look like, don't actually do it")]
    public bool simulate = false;
    public bool simulateError = false;

    [Tooltip("Change this in the inspector")]
    public float simulatePercent = 0.5f;  

    

    AsyncOperationHandle _loadDependenciesHandle;   // load scene dependencies first, then load scene
    AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance> _loadSceneHandle;



    bool _displayingError = false;
    bool _startedLoad = false;
    float _playerDidLeaveGameTimer = 0;   //counts up to timeUntilPlayerdidLeaveGame, if _displayingError is true

	public void Start()
	{
		if( !simulate && !string.IsNullOrEmpty(sceneToLoad.AssetGUID) )
            ResourceManager.ExceptionHandler = CustomExceptionHandler;

        progressText.text = "Loading...";
        SetProgressBarState(0);
	}


    void SetProgressBarState(float normalized)
    {
        progressBar.sizeDelta = new Vector2( (normalized-1) * progressBar.parent.GetComponent<RectTransform>().rect.width, progressBar.sizeDelta.y);
    }


    //Gets called for every error scenario encountered during an operation.
    //A common use case for this is having InvalidKeyExceptions fail silently when a location is missing for a given key.
    void CustomExceptionHandler(AsyncOperationHandle handle, Exception exception)
    {
        // NOTE: Addressables will throw exceptions here if it can't connect to the server,
        // gets a 404 etc

        Addressables.LogException(handle, exception);
        progressObject.SetActive(false);
        errorObject.SetActive(true);
        errorText.text = errorMessage;
        Debug.LogWarning("Error in AddressablesLoadingScreen.CustomExceptionHandler: " +  exception);
        _displayingError = true;
    }


	public void LoadScene()
    {      
        _loadDependenciesHandle = Addressables.DownloadDependenciesAsync(sceneToLoad.AssetGUID);
        _loadDependenciesHandle.Completed += DependenciesLoadComplete;
    }
 

    private void DependenciesLoadComplete(AsyncOperationHandle handle)
    {
        if( _loadDependenciesHandle.Status == AsyncOperationStatus.Succeeded )
        {
            // Now that we have dependencies, it's safe to load the scene itself.
            _loadSceneHandle = Addressables.LoadSceneAsync(sceneToLoad, UnityEngine.SceneManagement.LoadSceneMode.Single);
            _loadSceneHandle.Completed += SceneLoadComplete;
            progressText.text = "Starting scene load.";
        }
        else if( !_displayingError)
        {
            progressObject.SetActive(false);
            errorObject.SetActive(true);
            errorText.text = errorMessage;
            Debug.LogWarning("Failed to download dependencies.");
            _displayingError = true;
        }
    }

    private void SceneLoadComplete( AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance> handle)
    {
        if( handle.Status == AsyncOperationStatus.Succeeded )
        {
            Debug.Log("Successfully loaded scene.");
            // The scene will start automatically
        }
        else if( !_displayingError)
        {
            progressObject.SetActive(false);
            errorObject.SetActive(true);
            errorText.text = errorMessage;
            Debug.LogWarning("Failed to load scene.");
            _displayingError = true;
        }
    }

	private void Update()
	{
        if( debugSettings.mode == DebugSettings.BuildMode.Debug )   // in non debug modes, MUST wait for a createPlayer call before start.
            maxWaitForCreatePlayer -= Time.deltaTime;

        if( simulateError )
        {
            progressObject.SetActive(false);
            errorObject.SetActive(true);
            errorText.text = errorMessage;
            _displayingError = true;
        }
        else if( simulate )   // Pretend we're downloading to test the UI
        {
            progressText.text = "Connecting..." + ((int)((simulatePercent)*100) ) + "%";
            SetProgressBarState( simulatePercent );
        }

        // Not simulating. Wait for initialDelay, then call LoadScene
        else if( !string.IsNullOrEmpty(sceneToLoad.AssetGUID) && !_startedLoad)
        {
            if( Dateland_Network.realUserJson != null || (maxWaitForCreatePlayer <= 0 && !comingFromDateland && debugSettings.mode != DebugSettings.BuildMode.Release))   // We must receive the createPlayer call before we start loading... or just wait for maxWaitForCreatePlayer
            {
                if( string.IsNullOrEmpty(Dateland_Network.realUserJson) )   // Set up the spoofed user JSON, this is important to get Agora started among other things
                {
                    Dateland_Network.realUserJson = testUserJson.text;
                    Dateland_Network.playerFromJson = JsonUtility.FromJson<Player_Class>(testUserJson.text);
                }

                _startedLoad = true;
                LoadScene();
            }
        }

        else if( !_displayingError )   // Don't override errors
        {
            if( _loadSceneHandle.IsValid() )
            {
	            //progressText.text = "Connecting..." + ((int)((_loadSceneHandle.PercentComplete)*1000) / 10.0f) + "%";   // Try using PercentComplete for loading scene, since it's most likely all downloaded now.
                progressText.text = "Connecting...100%";   // just hang on 100% for scene loading
                SetProgressBarState( _loadSceneHandle.GetDownloadStatus().Percent );
            }
            else if( _loadDependenciesHandle.IsValid() )
            {
                progressText.text = "Connecting..." + ((int)((_loadDependenciesHandle.GetDownloadStatus().Percent)*100) ) + "%";
                SetProgressBarState( _loadDependenciesHandle.GetDownloadStatus().Percent );
            }
        }


        if( _displayingError )
        {
            if( _playerDidLeaveGameTimer == 0 )   // Can let native know we failed to connect...
                Dateland_Network.failureToConnect("Could not download or start the game.");

            // We could disconnect Agora here... but if they at least managed to get voice chat to connect, why stop them? At least let them talk

            // Count down until we leave the game!
            // However, be aware it still might be running in the BG of the native app.
            // So make sure we clean up and are ready for the next time they try again.
            _playerDidLeaveGameTimer += Time.deltaTime;

            createPlayerReceiver.restartSceneOnCreatePlayer = true;   // prime it to restart the frame on next createPlayer (when they try again).

            if( _playerDidLeaveGameTimer >= timeUntilPlayerDidLeaveGame && _playerDidLeaveGameTimer -Time.deltaTime < timeUntilPlayerDidLeaveGame)   // Just crossed the timeUntilPlayerDidLeaveGame threshold...
            {
                Debug.Log("Calling playerDidLeaveGame");
                if( !Application.isEditor )
                    NativeProxy.PlayerDidLeaveGame();  // Call it directly, Dateland_Network has a bunch of extra logic shoved in
            }
        }
	}
}
