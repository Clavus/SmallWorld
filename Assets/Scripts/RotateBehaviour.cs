using UnityEngine;

namespace Clavusaurus.Movement
{
    public class RotateBehaviour : MonoBehaviour
    {
        public Vector3 axis = Vector3.up;
        public float anglePerSecond = 10;
        public bool axisIsLocal = false;

        private void Update()
        {
            transform.Rotate(axis, anglePerSecond * Time.deltaTime, axisIsLocal ? Space.Self : Space.World);
        }
    }
}