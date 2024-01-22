using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class ObjectPoolingManager : MonoBehaviour, INetworkObjectPool
{
    private Dictionary<NetworkObject, List<NetworkObject>> prefabsThatHadBeenInstantiated = new();

    private void Start()
    {
        if (GlobalManagers.Instance != null)
        {
            GlobalManagers.Instance.ObjectPoolingManager = this;
        }
    }

    public NetworkObject AcquireInstance(NetworkRunner runner, NetworkPrefabInfo info)
    {
        NetworkObject networkObject = null;
        NetworkProjectConfig.Global.PrefabTable.TryGetPrefab(info.Prefab, out var prefab);
        prefabsThatHadBeenInstantiated.TryGetValue(prefab, out var networkObjects);

        bool foundMatch = false;
        if (networkObjects?.Count > 0)
        {
            foreach (var item in networkObjects)
            {
                if (item != null && item.gameObject.activeSelf == false)
                {
                    networkObject = item;
    
                    foundMatch = true;
                    break;
                }
            }
        }

        if (foundMatch == false)
        {
            networkObject = CreateObjectInstance(prefab);
        }

        return networkObject;
    }

    private NetworkObject CreateObjectInstance(NetworkObject prefab)
    {
        var obj = Instantiate(prefab);

        if (prefabsThatHadBeenInstantiated.TryGetValue(prefab, out var instanceData))
        {
            instanceData.Add(obj);
        }
        else
        {
            var list = new List<NetworkObject> { obj };
            prefabsThatHadBeenInstantiated.Add(prefab, list);
        }

        return obj;
    }

    public void ReleaseInstance(NetworkRunner runner, NetworkObject instance, bool isSceneObject)
    {
        instance.gameObject.SetActive(false);
    }

    public void RemoveNetworkObjectFromDic(NetworkObject obj)
    {
        if (prefabsThatHadBeenInstantiated.Count > 0)
        {
            foreach (var item in prefabsThatHadBeenInstantiated)
            {
                foreach (var networkObject in item.Value.Where(networkObject => networkObject == obj))
                {
                    item.Value.Remove(networkObject);
                    break;
                }
            }
        }
    }
}















