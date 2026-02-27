using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IDictionary<TKey, TValue> : 
    Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [Serializable]
    public class Pair
    {
        public TKey key = default;
        public TValue value = default;

        /// <summary>
        /// Pair
        /// </summary>
        public Pair(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [SerializeField]
    private List<Pair> _list = null;

    /// <summary>
    /// OnAfterDeserialize
    /// </summary>
    void ISerializationCallbackReceiver.OnAfterDeserialize() // データを受け取った後に行う
    {
        Clear();
        foreach (Pair pair in _list)
        {
            if (ContainsKey(pair.key))
            {
                continue;
            }
            Add(pair.key, pair.value);
        }
    }

    /// <summary>
	/// OnBeforeSerialize
	/// </summary>
	void ISerializationCallbackReceiver.OnBeforeSerialize()　// データを受け取る前に行う
    {
        // 処理なし
    }
}
