using System.Collections;

namespace Driver.Controllers.Interfaces
{
    public interface ICoroutineController
    {
        public uint StartManagedCoroutine(IEnumerator enumerator);
        public void StopManagedCoroutine(uint id);
        public bool IsCoroutineRunning(uint id);
    }
}