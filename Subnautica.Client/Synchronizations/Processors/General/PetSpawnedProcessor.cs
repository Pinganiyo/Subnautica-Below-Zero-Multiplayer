namespace Subnautica.Client.Synchronizations.Processors.General
{
    using Subnautica.API.Extensions;
    using Subnautica.API.Features;
    using Subnautica.Client.Abstracts;
    using Subnautica.Client.Core;
    using Subnautica.Events.EventArgs;
    using Subnautica.Events.Patches.Events.Game;
    using Subnautica.Network.Models.Core;

    using UnityEngine;

    using ServerModel = Subnautica.Network.Models.Server;

    public class PetSpawnedProcessor : NormalProcessor
    {
        /**
         *
         * Gelen veriyi işler
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public override bool OnDataReceived(NetworkPacket networkPacket)
        {
            var packet = networkPacket.GetPacket<ServerModel.PetSpawnedArgs>();
            if (packet == null)
            {
                return false;
            }

            if (Network.Identifier.GetGameObject(packet.PetId, true) != null)
            {
                return true;
            }

            var fabricatorGameObject = Network.Identifier.GetGameObject(packet.FabricatorId, true);
            if (fabricatorGameObject == null)
            {
                return true;
            }

            try
            {
                var fabricatorComponent = fabricatorGameObject.GetComponent("PetFabricator");
                if (fabricatorComponent == null)
                {
                    return true;
                }

                var spawnMethod = fabricatorComponent.GetType().GetMethod("SpawnPet", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (spawnMethod == null)
                {
                    return true;
                }

                var callbackHolder = new RemoteSpawnCallback(packet.PetId, packet.PetName);
                var callback = new System.Action<GameObject>(callbackHolder.OnSpawned);

                PetFabricatorSpawnPatch.SuppressBroadcast = true;
                try
                {
                    spawnMethod.Invoke(fabricatorComponent, new object[] { packet.TechType, callback });
                }
                finally
                {
                    PetFabricatorSpawnPatch.SuppressBroadcast = false;
                }
            }
            catch (System.Exception ex)
            {
                Log.Warn($"PetSpawnedProcessor.OnDataReceived -> Exception: {ex.Message}");
            }

            return true;
        }

        /**
         *
         * Evcil hayvan üretildiğinde tetiklenir.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public static void OnPetSpawned(PetSpawnedEventArgs ev)
        {
            ServerModel.PetSpawnedArgs request = new ServerModel.PetSpawnedArgs()
            {
                FabricatorId = ev.FabricatorId,
                PetId        = ev.PetId,
                TechType     = ev.TechType,
                PetName      = ev.PetName,
            };

            NetworkClient.SendPacket(request);
        }

        /**
         *
         * Uzak istemcide üretilen evcil hayvana gönderenin kimliğini işler.
         *
         */
        private class RemoteSpawnCallback
        {
            public RemoteSpawnCallback(string petId, string petName)
            {
                this.PetId   = petId;
                this.PetName = petName;
            }

            public string PetId { get; set; }

            public string PetName { get; set; }

            public void OnSpawned(GameObject petGameObject)
            {
                try
                {
                    if (petGameObject == null)
                    {
                        return;
                    }

                    petGameObject.SetIdentityId(this.PetId);

                    if (!string.IsNullOrEmpty(this.PetName))
                    {
                        PetRenamedProcessor.ApplyPetName(petGameObject, this.PetName);
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Warn($"PetSpawnedProcessor.RemoteSpawnCallback -> Exception: {ex.Message}");
                }
            }
        }
    }
}
