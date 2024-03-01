using General;
using UnityEngine;

namespace DefaultNamespace
{
    public class OfflinePlayer : MonoBehaviour, IPlayer
    {
        public bool IsMe => true;
    }
}