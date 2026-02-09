using UnityEngine;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Wrapper around PlayerPrefs.Save() that catches QuotaExceededError
    /// on WebGL (localStorage). Prevents hard crash if storage is full.
    /// </summary>
    public static class SafePrefs
    {
        public static void Save()
        {
            try
            {
                PlayerPrefs.Save();
            }
            catch (System.Exception)
            {
                // WebGL: localStorage quota exceeded — silent.
                // Desktop: should never happen, but guard anyway.
            }
        }
    }
}
