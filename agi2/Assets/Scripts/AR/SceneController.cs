using System;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;

namespace AR {
    public class SceneController : MonoBehaviour {
        public GameObject GameWorldPrefab;
        public GameObject World;
        public GameObject resourceExtractor;

        [SerializeField] private float worldScale;

        private List<AugmentedImage> _augmentedImages = new List<AugmentedImage>();
        

        private Anchor worldAnchor;
        private Anchor extractorAnchor;

        private GameObject _gameWorld;
        private GameObject spawnedExtractor;

        void Start() {
            QuitOnConnectionErrors();
        }

        void Update() {
            // The session status must be Tracking in order to access the Frame.
            if (Session.Status != SessionStatus.Tracking) {
                int lostTrackingSleepTimeout = 15;
                Screen.sleepTimeout = lostTrackingSleepTimeout;
                return;
            }
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Session.GetTrackables<AugmentedImage>(_augmentedImages, TrackableQueryFilter.Updated);

            foreach (var image in _augmentedImages) {
                if (_gameWorld != null && image.TrackingState == TrackingState.Tracking && spawnedExtractor == null && 
                    (image.Name == "redExtractor" && Divider.Instance.AmIRedPlayer() ||
                    image.Name == "blueExtractor" && !Divider.Instance.AmIRedPlayer())) {
                    extractorAnchor = image.CreateAnchor(image.CenterPose);
                    spawnedExtractor = Instantiate(resourceExtractor, World.transform);
                    spawnedExtractor.transform.localScale = Vector3.one * worldScale;
                    continue;
                }

                if (image.TrackingState == TrackingState.Tracking && _gameWorld == null && image.Name == "world") {
                    // Create an anchor to ensure that ARCore keeps tracking this augmented image.
                    worldAnchor = image.CreateAnchor(image.CenterPose);
                    _gameWorld = Instantiate(GameWorldPrefab, World.transform);
                    _gameWorld.transform.localScale = Vector3.one * worldScale; //Scale down a bit so it isn't huge.
                } else if (image.TrackingState == TrackingState.Stopped) {
                    // TODO: Display something that tracking was lost
                }
            }

            if(spawnedExtractor != null && extractorAnchor != null) {
                spawnedExtractor.transform.position = extractorAnchor.transform.position;
                spawnedExtractor.transform.eulerAngles = new Vector3(0, extractorAnchor.transform.eulerAngles.y, 0);
            }

            if (_gameWorld != null && worldAnchor != null) {
                _gameWorld.transform.position = worldAnchor.transform.position; //Update position
                _gameWorld.transform.eulerAngles = new Vector3 (0, worldAnchor.transform.eulerAngles.y, 0); //Update rotation but keep the world flat so it doesn't mess up gravity.
            }

        }

        void QuitOnConnectionErrors() {
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted) {
                Debug.LogError("Camera permission is needed to run this application.");
            } else if (Session.Status.IsError()) {
                Debug.LogError("ARCore encountered a problem connecting. Please restart the app.");
            }
        }
    }
}
