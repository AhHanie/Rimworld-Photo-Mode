using UnityEngine;

namespace Photo_Mode
{
    public class PhotoModePatchPump : MonoBehaviour
    {
        private void Update()
        {
            PhotoModePatchController.PumpMainThread();
        }
    }
}
