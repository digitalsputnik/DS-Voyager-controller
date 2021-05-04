using UnityEngine;
using VoyagerController.Effects;

namespace VoyagerController.Mapping
{
    public abstract class EffectDisplay : MonoBehaviour
    {
        public abstract void Setup(Effect effect);
        public abstract void Clean();
    }
}