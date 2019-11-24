using System;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;

namespace AR
{
    public class SceneController : MonoBehaviour
    {
        public GameObject GameWorldPrefab;
        public GameObject World;

        private List<AugmentedImage> _augmentedImages = new List<AugmentedImage>();
        private GameObject _gameWorld;

        void Start()
        {
            QuitOnConnectionErrors();
        }

        void Update()
        {
            // The session status must be Tracking in order to access the Frame.
            if (Session.Status != SessionStatus.Tracking)
            {
                int lostTrackingSleepTimeout = 15;
                Screen.sleepTimeout = lostTrackingSleepTimeout;
                return;
            }
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Session.GetTrackables<AugmentedImage>(_augmentedImages, TrackableQueryFilter.Updated);

            foreach (var image in _augmentedImages)
            {
                if (image.TrackingState == TrackingState.Tracking && _gameWorld == null)
                {
                    // Create an anchor to ensure that ARCore keeps tracking this augmented image.
                    Anchor anchor = image.CreateAnchor(image.CenterPose);
                    World.transform.position = anchor.transform.position;
                    _gameWorld = Instantiate(GameWorldPrefab, World.transform);
                }
                else if (image.TrackingState == TrackingState.Stopped)
                {
                    // TODO: Display something that tracking was lost
                }
            }
        }

        void QuitOnConnectionErrors()
        {
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                Debug.LogError("Camera permission is needed to run this application.");
            }
            else if (Session.Status.IsError())
            {
                Debug.LogError("ARCore encountered a problem connecting. Please restart the app.");
            }
        }
    }
}
