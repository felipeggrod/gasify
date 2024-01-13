using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GAS;

namespace GAS {
    [System.Serializable]
    public class DictionaryQueue<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> {
        GenericDictionary<TKey, TValue> dictionary = new();
        Queue<TKey> keyQueue = new Queue<TKey>();

        public int Count => dictionary.Count;

        // Add key-value pair to the dictionary and the queue
        public void Add(TKey key, TValue value) {
            if (dictionary.ContainsKey(key)) {
                throw new ArgumentException("An element with the same key already exists in the dictionary.");
            }

            dictionary.Add(key, value);
            keyQueue.Enqueue(key);
        }

        // Remove the key-value pair with the specified key from the dictionary and the queue
        public bool Remove(TKey key) {
            if (dictionary.ContainsKey(key)) {
                dictionary.Remove(key);

                // Remove key from the queue
                List<TKey> keyList = new List<TKey>(keyQueue);
                keyList.Remove(key);
                keyQueue = new Queue<TKey>(keyList);

                return true;
            }

            return false;
        }

        // Clear all key-value pairs from the dictionary and the queue
        public void Clear() {
            dictionary.Clear();
            keyQueue.Clear();
        }

        // Check if the dictionary contains the specified key
        public bool ContainsKey(TKey key) {
            return dictionary.ContainsKey(key);
        }

        // Check if the dictionary contains the specified value
        public bool ContainsValue(TValue value) {
            return dictionary.ContainsValue(value);
        }

        // Get the value associated with the specified key
        public bool TryGetValue(TKey key, out TValue value) {
            return dictionary.TryGetValue(key, out value);
        }

        // GetEnumerator for iterating over the elements
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            foreach (var key in keyQueue) {
                yield return new KeyValuePair<TKey, TValue>(key, dictionary[key]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        // Enqueue an element to the dictionary and the queue
        public void Enqueue(TKey key, TValue value) {
            Add(key, value);
        }

        // Dequeue the first element from the dictionary and the queue
        public KeyValuePair<TKey, TValue> Dequeue() {
            if (keyQueue.Count == 0) {
                throw new InvalidOperationException("Queue is empty");
            }

            TKey key = keyQueue.Dequeue();
            TValue value = dictionary[key];
            dictionary.Remove(key);

            return new KeyValuePair<TKey, TValue>(key, value);
        }

        // Peek at the first element in the queue without removing it
        public KeyValuePair<TKey, TValue> Peek() {
            if (keyQueue.Count == 0) {
                throw new InvalidOperationException("Queue is empty");
            }

            TKey key = keyQueue.Peek();
            TValue value = dictionary[key];

            return new KeyValuePair<TKey, TValue>(key, value);
        }
    }

}