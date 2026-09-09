using System.Collections;
using System.Collections.Generic;
using Driver.Controllers.Interfaces;
using UnityEngine;

namespace Driver.Controllers
{
    public class CoroutineController : MonoBehaviour, ICoroutineController
    {
        private static uint _countID;
        private const uint DefaultID = 0;
        private const uint InitID = 1;
        private readonly Dictionary<uint, IEnumerator> _enumerators = new();
        private readonly HashSet<uint> _usedIds = new();

        public MonoBehaviour Behaviour =>
            this;

        private void Awake()
        {
            name = nameof(CoroutineController);
        }

        public uint StartManagedCoroutine(IEnumerator enumerator)
        {
            uint id = GeneratorCoroutineID();
            if (IsCoroutineRunning(id))
            {
                Debug.LogWarning($"Coroutine with id{id} already running.");
                return id;
            }

            _enumerators[id] = enumerator;
            _usedIds.Add(id);
            StartCoroutine(RunEnumerator(id));
            return id;
        }

        public bool IsCoroutineRunning(uint id) =>
            _enumerators.ContainsKey(id);

        public void StopManagedCoroutine(uint id)
        {
            if (id == DefaultID)
            {
                return;
            }

            if (_enumerators.ContainsKey(id))
            {
                _enumerators.Remove(id);
                _usedIds.Remove(id);
            }
            else
            {
                Debug.LogWarning($"Enumerator with id{id} not running.");
            }
        }

        private IEnumerator RunEnumerator(uint id)
        {
            while (_usedIds.Contains(id))
            {
                IEnumerator enumerator = _enumerators[id];
                if (enumerator.MoveNext() == false)
                {
                    _enumerators.Remove(id);
                    _usedIds.Remove(id);
                    yield break;
                }

                yield return enumerator.Current;
            }
        }

        private uint GeneratorCoroutineID()
        {
            do
                if (_countID == uint.MaxValue)
                {
                    _countID = InitID;
                }
                else
                {
                    _countID++;
                }
            while (_usedIds.Contains(_countID));

            return _countID;
        }
    }
}