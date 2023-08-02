using System.Collections;
using System.Linq;
using _Game.Scripts.Saving;
using UnityEngine;
using UnityEngine.Events;

namespace _Game.Scripts.Utils.LineRendererGPS
{
    public class TutorialIndicator : MonoBehaviour, ISaveable
    {
        public UnityEvent onAllPointsTraveled;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private TutorialElement[] tutorialElements;
        [SerializeField] private float speed = 30f;
        [SerializeField] private float speedMultiplier;
        [SerializeField] private Vector3 offset;

        private int currentIndex = 0;

        private Vector3 tempPos;
        private Transform destination;
        private WaitForSeconds lineRendererChangeDelayWaitForSeconds;
        private Coroutine coroutine;
        private bool isTutorialComplete;

        public bool IsTutorialComplete => isTutorialComplete;
        // private void OnEnable()
        // {
        //     SetFirstDestination();
        //     StartCoroutine(LineRendererCoroutine());
        // }

        public void SetPlayer(Transform player)
        {
            playerTransform = player;
            StartRenderer();
        }

        public void StartCoroutine()
        {
            EnableDisableRenderer(true);
            coroutine = StartCoroutine(LineRendererCoroutine());
        }

        public void StopCoroutine()
        {
            EnableDisableRenderer(false);
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }

        private void StartRenderer()
        {
            SetFirstDestination();
            coroutine = StartCoroutine(LineRendererCoroutine());
        }

        private void SetFirstDestination()
        {
            if (!tutorialElements.Any()) return;

            destination = tutorialElements[currentIndex].GetDestinationTransform();
            tutorialElements[currentIndex].OnConditionComplete += GoNextIndex;
        }


        private bool CanGetNextIndex(int i)
        {
            return i + 1 < tutorialElements.Length;
        }

        private int GetNextPointIndex(int i)
        {
            return i + 1;
        }

        private IEnumerator LineRendererCoroutine()
        {
            tempPos = playerTransform.position;
            tempPos.y = 0.0f;
            while (true)
            {
                speed = CalculateSpeedByMeters(CalculateDistance(tempPos));
                while (CalculateDistance(tempPos) > 0.5f)
                {
                    lineRenderer.SetPosition(0, playerTransform.position + offset);
                    lineRenderer.SetPosition(1, destination.position);
                    //  tempPos = Vector3.MoveTowards(tempPos, destination.position, speed);
                    yield return null;
                }

                tempPos = playerTransform.position;
                yield return null;
            }
        }

        private void GoNextIndex()
        {
            if (!CanGetNextIndex(currentIndex))
            {
                onAllPointsTraveled?.Invoke();
                DisableRenderer();
                isTutorialComplete = true;
                return;
            }

            tutorialElements[currentIndex].OnConditionComplete -= GoNextIndex;
            SetNextIndex();
            tutorialElements[currentIndex].OnConditionComplete += GoNextIndex;
        }

        private void SetNextIndex()
        {
            currentIndex = GetNextPointIndex(currentIndex);
            destination = tutorialElements[currentIndex].GetDestinationTransform();
            tempPos = playerTransform.position;
            speed = CalculateSpeedByMeters(CalculateDistance(tempPos));
        }

        private void DisableRenderer()
        {
            tutorialElements[currentIndex].OnConditionComplete -= GoNextIndex;
            StopAllCoroutines();
            EnableDisableRenderer(false);
            enabled = false;
        }

        private float CalculateDistance(Vector3 tempPos)
        {
            return Vector3.Distance(tempPos, destination.position);
        }

        private float CalculateSpeedByMeters(float distance)
        {
            return distance * speedMultiplier * Time.deltaTime;
        }

        private void EnableDisableRenderer(bool isEnable)
        {
            lineRenderer.enabled = isEnable;
        }

        public object CaptureState()
        {
            return isTutorialComplete;
        }

        public void RestoreState(object state)
        {
            isTutorialComplete = (bool)state;
        }
    }
}