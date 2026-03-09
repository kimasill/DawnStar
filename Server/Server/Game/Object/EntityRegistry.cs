using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class EntityRegistry
    {
        public static EntityRegistry Instance { get; } = new EntityRegistry();

        object _lock = new object();
        Dictionary<int, Player> _players = new Dictionary<int, Player>();

        //[UNUSED(1)][TYPE(7)][OBJECTID(24)]
        int _counter = 0;

        public T Add<T>() where T : GameObject, new()
        {
            T obj = new T();
            lock (_lock)
            {
                obj.Id = GenerateId(obj.ObjectType);
                if (obj.ObjectType == GameObjectType.Player)
                {
                    _players.Add(obj.Id, obj as Player);
                }                    
            }            
            return obj;
        }

        public int GenerateId(GameObjectType type)
        {
            lock (_lock)
            {
                return ((int)type << 24) | (_counter++);
            }
        }

        public static GameObjectType GetObjectType(int id)
        {
            int type = (id >> 24) & 0x7F;
            return(GameObjectType)type;
        }

        public bool Remove(int objectId)
        {
            GameObjectType type = GetObjectType(objectId);
            lock (_lock)
            {
                if (type == GameObjectType.Player)
                    return _players.Remove(objectId);
            }

            return false;
        }

        public GameObject Find(int objectId)
        {
            GameObjectType type = GetObjectType(objectId);
            lock (_lock)
            {
                if (type == GameObjectType.Player)
                {
                    Player player = null;
                    if(_players.TryGetValue(objectId, out player))
                        return player;
                }
                return null;
            }
        }
    }
}
