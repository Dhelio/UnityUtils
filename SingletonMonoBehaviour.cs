using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CastrimarisStudios
{

    /// <summary>
    /// Simple inheritable class to make anything a singleton quickly.
    /// </summary>
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance = null;

        public virtual void Awake() 
        {
            if (Instance == null)
                Instance = this as T;
            else if (Instance != this)
                Destroy(this);
        }
    }
}
