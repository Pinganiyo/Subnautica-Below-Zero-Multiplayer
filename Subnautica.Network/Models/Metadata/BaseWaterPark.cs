namespace Subnautica.Network.Models.Metadata
{
    using System;
    using System.Collections.Generic;
    using MessagePack;
    using Subnautica.Network.Core.Components;
    using Subnautica.Network.Models.Storage.World.Childrens;

    [MessagePackObject]
    public class BaseWaterPark : MetadataComponent
    {
        [Key(0)]
        public BaseWaterParkProcessType ProcessType { get; set; }

        [Key(1)]
        public WorldDynamicEntity Entity { get; set; }

        [Key(2)]
        public HashSet<string> Eggs { get; set; } = new HashSet<string>();

        public bool AddCreatureEgg(string uniqueId)
        {
            if (this.Eggs == null)
            {
                this.Eggs = new HashSet<string>();
            }

            return this.Eggs.Add(uniqueId);
        }
    }

    public enum BaseWaterParkProcessType : byte
    {
        None = 0,
        ItemDrop = 1
    }
}
