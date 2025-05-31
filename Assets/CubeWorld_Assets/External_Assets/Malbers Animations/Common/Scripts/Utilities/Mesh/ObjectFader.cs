using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is for the camera. It handles the fading of objects that block the player character's view.
public class ObjectFader : MonoBehaviour
{
    [Tooltip("The transform of the player character. Don't place the Transform too low, otherwise the ground will eventually be effected. Use the head instead.")]
    [SerializeField] TransformVar player;
    [Tooltip("The layer(s) that can obstruct the view of the player.")]
    [SerializeField] LayerMask obstructionLayer;
    [Tooltip("The duration over which the fade effect occurs.")]
    [SerializeField] float fadeDuration = 0.5f;
    [Range(0, 1), Tooltip("The target alpha value for faded objects. Should be between 0 (completely transparent) and 1 (completely opaque).")]
    [SerializeField] float fadeAmount = 0.5f;

    private Dictionary<Renderer, Coroutine> fadeCoroutines = new Dictionary<Renderer, Coroutine>();

    private void Update()
    {
        CheckForObstructions();
    }

    private void CheckForObstructions()
    {
        List<Renderer> currentObstructions = new List<Renderer>();

        // Perform raycast from the camera to the player
        Vector3 direction = player.Value.position - Camera.main.transform.position;
        float distance = Vector3.Distance(Camera.main.transform.position, player.Value.position);

        RaycastHit[] hits = Physics.RaycastAll(Camera.main.transform.position, direction, distance, obstructionLayer);

        foreach (RaycastHit hit in hits)
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null)
            {
                currentObstructions.Add(renderer);
                if (!fadeCoroutines.ContainsKey(renderer) || fadeCoroutines[renderer] == null)
                {
                    fadeCoroutines[renderer] = StartCoroutine(FadeTo(renderer, fadeAmount, fadeDuration)); // Fade out
                }
            }
        }

        List<Renderer> keys = new List<Renderer>(fadeCoroutines.Keys);
        foreach (Renderer renderer in keys)
        {
            if (!currentObstructions.Contains(renderer))
            {
                if (fadeCoroutines[renderer] != null)
                {
                    StopCoroutine(fadeCoroutines[renderer]);
                }
                fadeCoroutines[renderer] = StartCoroutine(FadeTo(renderer, 1f, fadeDuration)); // Fade in
            }
        }
    }

    private IEnumerator FadeTo(Renderer renderer, float targetAlpha, float duration)
    {
        List<Material> materials = new List<Material>(renderer.materials);
        List<float> startAlphas = new List<float>();

        // Get current alpha values
        foreach (Material mat in materials)
        {
            startAlphas.Add(mat.color.a);
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlphas[0], targetAlpha, elapsed / duration);

            // Apply new alpha values
            foreach (Material mat in materials)
            {
                Color color = mat.color;
                color.a = newAlpha;
                mat.color = color;

                // Ensure the material uses a shader that supports transparency
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }

            yield return null;
        }

        // Ensure the final alpha is set correctly
        foreach (Material mat in materials)
        {
            Color color = mat.color;
            color.a = targetAlpha;
            mat.color = color;
        }

        fadeCoroutines[renderer] = null; // Clear the coroutine reference
    }
}
