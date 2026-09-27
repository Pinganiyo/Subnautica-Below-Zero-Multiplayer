namespace Subnautica.Client.Synchronizations.Processors.General
{
    using Subnautica.API.Extensions;
    using Subnautica.API.Features;
    using Subnautica.Client.Abstracts;
    using Subnautica.Client.Core;
    using Subnautica.Events.EventArgs;
    using Subnautica.Network.Models.Core;

    using UnityEngine;

    using ServerModel = Subnautica.Network.Models.Server;

    public class PetRenamedProcessor : NormalProcessor
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
            var packet = networkPacket.GetPacket<ServerModel.PetRenamedArgs>();
            if (packet == null)
            {
                return false;
            }

            var petGameObject = Network.Identifier.GetGameObject(packet.PetId, true);
            if (petGameObject == null)
            {
                return true;
            }

            PetRenamedProcessor.ApplyPetName(petGameObject, packet.NewName);
            return true;
        }

        /**
         *
         * Evcil hayvan yeniden adlandırıldığında tetiklenir.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public static void OnPetRenamed(PetRenamedEventArgs ev)
        {
            ServerModel.PetRenamedArgs request = new ServerModel.PetRenamedArgs()
            {
                PetId   = ev.PetId,
                NewName = ev.NewName,
            };

            NetworkClient.SendPacket(request);
        }

        /**
         *
         * Evcil hayvanın ismini SubnauticaPets bileşenine yansıtır.
         *
         */
        public static void ApplyPetName(GameObject petGameObject, string newName)
        {
            try
            {
                foreach (var component in petGameObject.GetComponents<Component>())
                {
                    if (component == null)
                    {
                        continue;
                    }

                    var componentType = component.GetType();

                    var nameProperty = componentType.GetProperty("PetName", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    if (nameProperty != null && nameProperty.CanWrite && nameProperty.PropertyType == typeof(string))
                    {
                        nameProperty.SetValue(component, newName);
                        return;
                    }

                    var nameField = componentType.GetField("petName", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    if (nameField != null && nameField.FieldType == typeof(string))
                    {
                        nameField.SetValue(component, newName);
                        return;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Warn($"PetRenamedProcessor.ApplyPetName -> Exception: {ex.Message}");
            }
        }
    }
}
