
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public static class JsonHelper {

	public static T[] FromJson<T>(string json) {
		Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
		return wrapper.Items;
	}

	public static string ToJson<T>(T[] array) {
		Wrapper<T> wrapper = new Wrapper<T>();
		wrapper.Items = array;
		return JsonUtility.ToJson(wrapper);
	}

	public static string ToJson<T>(T[] array, bool prettyPrint) {
		Wrapper<T> wrapper = new Wrapper<T>();
		wrapper.Items = array;
		return JsonUtility.ToJson(wrapper, prettyPrint);
	}

	public static string ToJson<K, V>(Dictionary<K,V> dictionary, bool prettyPrint = false) {
		DictionaryWrapper<K, V> wrapper = new DictionaryWrapper<K, V>();
		wrapper.Keys = (from k in dictionary select k.Key).ToArray();
		wrapper.Values = (from v in dictionary select v.Value).ToArray();
		return JsonUtility.ToJson(wrapper, prettyPrint);
	}

	public static Dictionary<K,V> FromJson<K,V>(string json) {
		DictionaryWrapper<K, V> wrapper = JsonUtility.FromJson<DictionaryWrapper<K,V>>(json);
		Dictionary<K, V> result = new Dictionary<K, V>(wrapper.Keys.Length);
		for (int i = 0; i < wrapper.Keys.Length; i++) {
			result.Add(wrapper.Keys[i], wrapper.Values[i]);
		}
		return result;
	}

	[System.Serializable]
	private class Wrapper<T> {
		public T[] Items;
	}
	[System.Serializable]
	private class DictionaryWrapper<K, V> {
		public K[] Keys;
		public V[] Values;
	}


}
