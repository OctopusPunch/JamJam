using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EyeAnimationSet", menuName = "Eye/Eye Animation Set")]
public sealed class EyeAnimationSet : ScriptableObject
{
    public List<EyeAnimClip> clips = new List<EyeAnimClip>();
}
