using System;
using System.Collections.Generic;
using GoogleARCore;
using Networking;
using UnityEngine;

namespace AR
{
    public class SceneController : MonoBehaviour {
        public GameObject GameWorldPrefab;
        public GameObject World;
        public GameObject resourceExtractor;

        [SerializeField] private float inverseWorldScale;

        private List<AugmentedImage> _augmentedImages = new List<AugmentedImage>();


        private Anchor worldAnchor;
        private Anchor extractorAnchor;

        private GameObject _gameWorld;
        private GameObject spawnedExtractor;

        void Start() {
            QuitOnConnectionErrors();
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.Q))    //debug AR recognize
            {
                Debug.Log("Recognizing AR Debug");
                _gameWorld = Instantiate(GameWorldPrefab, World.transform);
                UnityClient.Instance.SendPacket(Packet.ReadyAR);
            }
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
                    (image.Name == "redExtractor" && NetworkedGameObject.Player == Player.Red ||
                    image.Name == "blueExtractor" && NetworkedGameObject.Player == Player.Blue)) {
                    extractorAnchor = image.CreateAnchor(image.CenterPose);
                    spawnedExtractor = Instantiate(resourceExtractor, World.transform);
                    UnityClient.Instance.SendPacket(Packet.ReadyAR); //Say ready when both world and harvester scanned
                    continue;
                }

                if (image.TrackingState == TrackingState.Tracking && _gameWorld == null && image.Name == "world") {
                    // Create an anchor to ensure that ARCore keeps tracking this augmented image.
                    worldAnchor = image.CreateAnchor(image.CenterPose);
                    _gameWorld = Instantiate(GameWorldPrefab, World.transform);
                    Debug.Log("Recognizing AR");
                } else if (image.TrackingState == TrackingState.Stopped) {
                    // TODO: Display something that tracking was lost
                }
            }

            //Update position of game world based on camera and anchor
            if (_gameWorld != null && worldAnchor != null) {
                _gameWorld.transform.position = Camera.main.transform.position + (worldAnchor.transform.position - Camera.main.transform.position) * inverseWorldScale;
                _gameWorld.transform.eulerAngles = new Vector3(0, worldAnchor.transform.eulerAngles.y, 0); //Update rotation but keep the world flat so it doesn't mess up gravity.

            }

            //Update position of harvester based on camera and anchor
            if (spawnedExtractor != null && extractorAnchor != null) {
                spawnedExtractor.transform.position = Camera.main.transform.position + (extractorAnchor.transform.position - Camera.main.transform.position) * inverseWorldScale;
                spawnedExtractor.transform.position = new Vector3(spawnedExtractor.transform.position.x, _gameWorld.transform.position.y, spawnedExtractor.transform.position.z); //make sure harvester stays level with game world
                spawnedExtractor.transform.eulerAngles = new Vector3(0, extractorAnchor.transform.eulerAngles.y, 0);
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
