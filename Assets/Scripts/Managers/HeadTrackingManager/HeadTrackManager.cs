using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections;

public class HeadRigController : MonoBehaviour
{
    [System.Serializable]
    public class CharacterHeadRig
    {
        [Header("Character Info")]
        public CharacterType characterType;
        public MultiAimConstraint headConstraint;
    }

    public CharacterHeadRig[] characterHeadRig;
    public CharacterType activeCharacterType;
    public float transitionDuration = 0.5f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(SmoothSetHeadTarget(activeCharacterType, CharacterTarget.Mother));
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            StartCoroutine(SmoothSetHeadTarget(activeCharacterType, CharacterTarget.Father));
        }
    }

    IEnumerator SmoothSetHeadTarget(CharacterType type, CharacterTarget target)
    {
        foreach (var rig in characterHeadRig)
        {
            if (rig.characterType != type) continue;

            var sources = rig.headConstraint.data.sourceObjects;
            int targetIndex = (int)target;
            float time = 0f;

            // Capture initial weights
            float[] initialWeights = new float[sources.Count];
            for (int i = 0; i < sources.Count; i++)
                initialWeights[i] = sources.GetWeight(i);

            while (time < transitionDuration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / transitionDuration);

                for (int i = 0; i < sources.Count; i++)
                {
                    float targetWeight = (i == targetIndex) ? 1f : 0f;
                    float newWeight = Mathf.Lerp(initialWeights[i], targetWeight, t);
                    sources.SetWeight(i, newWeight);
                }

                rig.headConstraint.data.sourceObjects = sources;
                yield return null;
            }

            // Finalize weights
            for (int i = 0; i < sources.Count; i++)
                sources.SetWeight(i, i == targetIndex ? 1f : 0f);

            rig.headConstraint.data.sourceObjects = sources;
            break;
        }
    }
}