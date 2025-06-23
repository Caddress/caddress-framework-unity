using Caddress.Tools;
using System.Collections;
using System.Collections.Generic;
using TriLib;
using UnityEngine;

namespace Caddress.Template.Bridge {
    public class Placement : MonoBehaviour, IDecorate {

        private bool _isShow = false;
        protected SelectEffect selEffect = null;

        private Coroutine _flashCoroutine;

        List<Renderer> renderList = new List<Renderer>();
        List<Collider> colliderList = new List<Collider>();
        List<Material> materialList = new List<Material>();
        List<Color> sourceColorList = new List<Color>();

        void Awake() {
            selEffect = SelectEffectManager.Instance.Create();
            renderList = new List<Renderer>(GetComponentsInChildren<Renderer>());
            colliderList = colliderList = new List<Collider>(GetComponentsInChildren<Collider>());
            var mrs = GetComponentsInChildren<MeshRenderer>();
            foreach (var mr in mrs) {
                materialList.AddRange(mr.materials);
            }
            foreach(var mat in materialList) {
                sourceColorList.Add(mat.color);
            }
        }

        public void Show() {
            _isShow = true;
            foreach(var render in renderList) {
                render.enabled = true;
            }
            foreach (var collider in colliderList) {
                collider.enabled = true;
            }
        }

        public void Hide() {
            _isShow = false;
            foreach (var render in renderList) {
                render.enabled = false;
            }
            foreach (var collider in colliderList) {
                collider.enabled = false;
            }
        }

        public void Flash(Color color) {
            if (_flashCoroutine != null && color == Color.clear) {
                StopCoroutine(_flashCoroutine);
                SetColor(Color.clear);
            }
            _flashCoroutine = StartCoroutine(FlashRoutine(color, 1f));
        }
        private IEnumerator FlashRoutine(Color color, float duration) {
            while (true) {
                yield return StartCoroutine(LerpColor(color, Color.gray, duration / 2f));
                yield return StartCoroutine(LerpColor(Color.gray, color, duration / 2f));
            }
        }
        private IEnumerator LerpColor(Color start, Color end, float time) {
            float elapsed = 0f;
            while (elapsed < time) {
                float t = elapsed / time;
                SetColor(Color.Lerp(start, end, t));
                elapsed += Time.deltaTime;
                yield return null;
            }
            SetColor(end);
        }
        void SetColor(Color color) {
            var resultColor = color;
            for (var i = 0; i < materialList.Count; i++) {
                var material = materialList[i];
                if(color == Color.clear) {
                    resultColor = sourceColorList[i];
                }
                if (material.shader.name == "Custom/NoLighting") {
                    material.shader = Shader.Find("Custom/NoLightingFade");
                }
                else if (material.shader.name == "Custom/NoLightingFade") {
                    material.shader = Shader.Find("Custom/NoLighting");
                }
                else if (material.shader.name == "Legacy Shaders/Diffuse") {
                    material.shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
                }
                else if (material.shader.name == "Legacy Shaders/Transparent/Diffuse") {
                    material.shader = Shader.Find("Legacy Shaders/Diffuse");
                }
                material.color = resultColor;
            }
        }

        public void Transparent() {
            gameObject.Fade(0.5f);
        }

        public void HighLight(Color color) {
            selEffect.Select(gameObject, color);
        }

        public bool IsShow() => _isShow;
    }
}