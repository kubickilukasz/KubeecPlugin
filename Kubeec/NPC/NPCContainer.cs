using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kubeec.NPC {

    public class NPCContainer : Singleton<NPCContainer> {

        HashSet<PlayerReference> players = new HashSet<PlayerReference>();
        HashSet<NonPlayerCharacter> npcs = new HashSet<NonPlayerCharacter>();

        public IEnumerable<PlayerReference> Players => players;
        public IEnumerable<NonPlayerCharacter> NPCs => npcs;

        public void RegisterPlayer(PlayerReference player) {
            if (!players.Contains(player)) {
                players.Add(player);
            }
        }

        public void UnregisterPlayer(PlayerReference player) {
            if (players.Contains(player)) {
                players.Remove(player);
            }
        }

        public void RegisterNPC(NonPlayerCharacter npc) {
            if (!npcs.Contains(npc)) {
                npcs.Add(npc);
            }
        }

        public void UnregisterNPC(NonPlayerCharacter npc) {
            if (npcs.Contains(npc)) {
                npcs.Remove(npc);
            }
        }

    }

}
