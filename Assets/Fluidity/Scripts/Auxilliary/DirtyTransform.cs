using UnityEngine;

namespace Fluidity.Auxilliary
{
    /// <summary>
    /// Use this to store the state of a transform.  Then refer to it 
    /// to see if any changes have been made. (ie. if it's moved...)
    /// </summary>
    internal class DirtyTransform
    {
        private Vector3 position;
        private Quaternion rotation;
        private Vector3 scale;

        public bool Set(Transform t)
        {
            bool isDirty = (position != t.position || rotation != t.rotation || scale != t.lossyScale);
            position = t.position;
            rotation = t.rotation;
            scale = t.lossyScale;
            return isDirty;
        }
    }
}