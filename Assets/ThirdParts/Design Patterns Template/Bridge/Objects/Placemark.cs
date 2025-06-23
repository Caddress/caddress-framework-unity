using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Caddress;

namespace Caddress.Template.Bridge {
    public class Placemark : MonoBehaviour, IDecorate {
        private bool _isShow = false;

        public Transform root = null;
        public CanvasGroup group = null;
        public Image shelter = null;

        private Coroutine _flashCoroutine;

        void Awake() {
            group = gameObject.GetComponentOrAdd<CanvasGroup>();
            root = transform.GetChild(0);
            if(root == null) {
                var rootObj = new GameObject("Root");
                rootObj.transform.SetParent(transform);
                root = rootObj.transform;
                root.transform.Reset();
            }
            shelter = root.gameObject.GetComponentOrAdd<Image>();
            shelter.color = Color.clear;
        }

        public void Show() {
            if (group == null) return;

            _isShow = true;
            group.alpha = 1;
            group.blocksRaycasts = true;
        }

        public void Hide() {
            if (group == null) return;

            _isShow = false;
            group.alpha = 0;
            group.blocksRaycasts = false;
        }

        public void Flash(Color color) {
            if (_flashCoroutine != null && color == Color.clear) {
                StopCoroutine(_flashCoroutine);
            }
            _flashCoroutine = StartCoroutine(FlashRoutine(color, 1f));
        }
        private IEnumerator FlashRoutine(Color color, float duration) {
            while (true) {
                yield return StartCoroutine(LerpColor(color, Color.clear, duration / 2f));
                yield return StartCoroutine(LerpColor(Color.clear, color, duration / 2f));
            }
        }
        private IEnumerator LerpColor(Color start, Color end, float time) {
            float elapsed = 0f;
            while (elapsed < time) {
                float t = elapsed / time;
                shelter.color = Color.Lerp(start, end, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            shelter.color = end;
        }

        public void Transparent() {
            if (group == null) return;
            group.alpha = 0.5f;
        }

        public void HighLight(Color color) {
            shelter.color = new Color(color.r, color.g, color.b, 0.5f);
        }

        public bool IsShow() => _isShow;
    }
}