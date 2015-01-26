/*
 * Copyright (c) 2013 LowLevelTech
 * Alex Dunn
 * llt@dunnalex.com
*/

using UnityEngine;

namespace Fluidity.Auxilliary
{
    /// <summary>
    /// Singleton pattern object.
    /// 
    /// This abstract class can be inherited by MonoBehaviour (<see cref="UnityEngine.MonoBehaviour"/>) classes to give them a singleton behaviour.
    /// Useful for controllers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        /// <summary>
        /// A static pointer to the instance of this class.
        /// </summary>
        private static T instance;

        /// <summary>
        /// Singleton accessor, provides some error checking.
        /// </summary>
        protected static T self
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType(typeof(T)) as T;

                    if (instance == null)
                    {
                        Debug.LogError("Cannot find an instance of type: " + typeof(T) +" in the scene, and one is required.");
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Unity callback.
        /// 
        /// Initialize this object to be the singleton.
        /// Cannot override this method, for awake time initialization override the OnAwake() method. <see cref="OnAwake()"/>
        /// </summary>
        private void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
            }
            else
            {
                Debug.LogWarning("It would seem there are two instances of this class in the scene... There should only be one.  You might want to look into this.");
            }

            OnAwake();
        }

        /// <summary>
        /// Override this for awake time initialization code.
        /// </summary>
        protected virtual void OnAwake() { }
    }
}