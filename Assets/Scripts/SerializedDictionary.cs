using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializedDictionary<K, V>
{
    [SerializeField]
    public List<Pair<K,V>> Contents;
    [SerializeField]
    public List<K> Contentsa;

    [SerializeField]
    public K asncx;

    public Dictionary<K,V> GenerateDictionary()
    {
        Dictionary<K, V> result = new Dictionary<K, V>();

        foreach (Pair<K,V> pair in Contents)
        {
            if (!result.ContainsKey(pair.First))
            {
                result.Add(pair.First, pair.Second);
            }
        }

        return result;
    }
}

[Serializable]
public struct Pair<F, S>
{
    public F First;
    public S Second;
}
