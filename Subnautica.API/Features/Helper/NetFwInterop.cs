namespace NetFwTypeLib
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    [Guid("98325047-C671-4174-8D81-DEFCD3F03186")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface INetFwPolicy2
    {
        [DispId(7)]
        INetFwRules Rules { get; }
    }

    [Guid("9C4C6277-5027-4416-AFAE-E4F14720227E")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface INetFwRules : IEnumerable
    {
        [DispId(1)]
        int Count { get; }
    }

    [Guid("AF22427C-8EA4-4611-9CF5-6FBDE7BFEE1D")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface INetFwRule
    {
        [DispId(1)]
        string Name { get; set; }

        [DispId(2)]
        string Description { get; set; }

        [DispId(3)]
        string ApplicationName { get; set; }

        [DispId(8)]
        int Protocol { get; set; }

        [DispId(11)]
        int Profiles { get; set; }

        [DispId(12)]
        NET_FW_ACTION_ Action { get; set; }

        [DispId(13)]
        bool Enabled { get; set; }
    }

    [Flags]
    public enum NET_FW_PROFILE_TYPE2_
    {
        NET_FW_PROFILE2_DOMAIN = 1,
        NET_FW_PROFILE2_PRIVATE = 2,
        NET_FW_PROFILE2_PUBLIC = 4,
        NET_FW_PROFILE2_ALL = 0x7FFFFFFF
    }

    public enum NET_FW_ACTION_
    {
        NET_FW_ACTION_BLOCK = 0,
        NET_FW_ACTION_ALLOW = 1,
        NET_FW_ACTION_MAX = 2
    }

    public enum NET_FW_IP_PROTOCOL_
    {
        NET_FW_IP_PROTOCOL_TCP = 6,
        NET_FW_IP_PROTOCOL_UDP = 17,
        NET_FW_IP_PROTOCOL_ANY = 256
    }
}
