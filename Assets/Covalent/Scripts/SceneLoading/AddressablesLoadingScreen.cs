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



    [Tooltip("Needs to be a full Addressables address.")]
    public AssetReference sceneToLoad;
    public TMP_Text progressText;

    [Tooltip("Stretches to fill its parent transform; has x pivot=0.  We'll change it accordingly")]
    public RectTransform progressBar; 
    


    [Header("Testing")]
    [Tooltip("Just simulate what it would look like, don't actually do it")]
    public bool simulate = false;
    [Tooltip("Change this in the inspector")]
    public float simulatePercent = 0.5f;  

    AsyncOperationHandle _loadDependenciesHandle;   // load scene dependencies first, then load scene
    AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance> _loadSceneHandle;



    bool _displayingError = false;
    bool _startedLoad = false;


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
        progressText.text = "ERROR: " + exception;
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
            progressText.text = "Failed to download dependencies.";
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
            progressText.text = "Failed to load scene.";
            _displayingError = true;
        }
    }

	private void Update()
	{
        if( simulate )   // Pretend we're downloading to test the UI
        {
            progressText.text = "Downloading assets... " + ((int)((simulatePercent)*1000) / 10.0f) + "%";
            SetProgressBarState( simulatePercent );
        }

        // Not simulating. Wait for initialDelay, then call LoadScene
        else if( !string.IsNullOrEmpty(sceneToLoad.AssetGUID) && !_startedLoad)
        {
            if( Dateland_Network.realUserJson != null || Application.isEditor )   // We must receive the createPlayer call before we start loading.
            {
                _startedLoad = true;
                LoadScene();
            }
        }

        else if( !_displayingError )   // Don't override errors
        {
            if( _loadSceneHandle.IsValid() )
            {
	            progressText.text = "Loading scene... " + ((int)((_loadSceneHandle.GetDownloadStatus().Percent)*1000) / 10.0f) + "%";
                SetProgressBarState( _loadSceneHandle.GetDownloadStatus().Percent );
            }
            else if( _loadDependenciesHandle.IsValid() )
            {
                progressText.text = "Downloading assets... " + ((int)((_loadDependenciesHandle.GetDownloadStatus().Percent)*1000) / 10.0f) + "%";
                SetProgressBarState( _loadDependenciesHandle.GetDownloadStatus().Percent );
            }
        }
	}
}
