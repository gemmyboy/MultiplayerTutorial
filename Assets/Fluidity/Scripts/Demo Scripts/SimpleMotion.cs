/*
 * Copyright (c) 2013 LowLevelTech
 * Alex Dunn
 * llt@dunnalex.com
*/

using UnityEngine;

namespace Fluidity.DemoUtilities
{
    /// <summary>
    /// A simple script that moves a transfrom around in an XY circle at speed and radius with the option to ocilate up and down in Y.
    /// </summary>
    public class SimpleMotion : MonoBehaviour
    {
        public Transform centre;

        public float radius = 1.0f;
        public float rotationSpeed = 30.0f;
        public float bobbingSpeed = 1.0f;
        public float bobbingScale = 1.0f;
        public float offset;

        private float currentTheta;
        private float currentY;

        private void Update()
        {
            currentTheta += rotationSpeed * Time.deltaTime;
            currentY = Mathf.Sin(Time.time * bobbingSpeed + offset) * bobbingScale;

            transform.position = centre.position + new Vector3(Mathf.Sin((currentTheta + offset) * Mathf.Deg2Rad) * radius, currentY, Mathf.Cos((currentTheta + offset) * Mathf.Deg2Rad) * radius);
        }
    }
}