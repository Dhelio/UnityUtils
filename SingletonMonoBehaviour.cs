using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CastrimarisStudios.Utilities
{
    /// <summary>
    /// Simple inheritable class to make anything a singleton quickly.
    /// </summary>
    public class SingletonMonoBehaviour<T> : MonoBehaviour
    {
        public static SingletonMonoBehaviour<T> Instance;

        public virtual void Awake() 
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(this);
        }
    }
}
