using System;
using System.Runtime.InteropServices;

namespace EtherNope;

public static partial class Interop
{
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int MessageBoxW(IntPtr hWnd, string lpText, string lpCaption, uint uType);

    [LibraryImport("setupapi.dll", EntryPoint = "SetupDiGetClassDevsW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial IntPtr SetupDiGetClassDevs(
        ref Guid ClassGuid,
        string? Enumerator,
        IntPtr hwndParent,
        uint Flags
    );

    [LibraryImport("setupapi.dll", EntryPoint = "SetupDiEnumDeviceInfo", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetupDiEnumDeviceInfo(
        IntPtr deviceInfoSet,
        uint memberIndex,
        ref SpDevInfoData deviceInfoData
    );

    [LibraryImport("setupapi.dll", EntryPoint = "SetupDiGetDeviceRegistryPropertyW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetupDiGetDeviceRegistryProperty(
        IntPtr deviceInfoSet,
        ref SpDevInfoData deviceInfoData,
        uint property,
        out uint propertyRegDataType,
        byte[] propertyBuffer,
        uint propertyBufferSize,
        out uint requiredSize
    );

    [LibraryImport("setupapi.dll", EntryPoint = "SetupDiCallClassInstaller", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetupDiCallClassInstaller(
        uint installFunction,
        IntPtr deviceInfoSet,
        ref SpDevInfoData deviceInfoData
    );

    [LibraryImport("setupapi.dll", EntryPoint = "SetupDiDestroyDeviceInfoList", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

    [LibraryImport("setupapi.dll", EntryPoint = "SetupDiSetClassInstallParamsW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetupDiSetClassInstallParams(
        IntPtr deviceInfoSet,
        ref SpDevInfoData deviceInfoData,
        IntPtr classInstallParams,
        int classInstallParamsSize
    );

    [StructLayout(LayoutKind.Sequential)]
    public struct SpDevInfoData
    {
        public uint CbSize;
        public Guid ClassGuid;
        public uint DevInst;
        public IntPtr Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SpPropChangeParams
    {
        public SpClassInstallHeader ClassInstallHeader;
        public uint StateChange;
        public uint Scope;
        public uint HwProfile;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SpClassInstallHeader
    {
        public uint CbSize;
        public uint InstallFunction;
    }

    public static Guid GUID_DEVCLASS_NET = new Guid("4d36e972-e325-11ce-bfc1-08002be10318");
    public const uint DIGCF_PRESENT = 0x00000002; // Only devices that are currently present
    public const uint SPDRP_FRIENDLYNAME = 0x0000000C;
    public const uint SPDRP_DEVICEDESC = 0x00000000; 
    public const uint DIF_PROPERTYCHANGE = 0x12;
    public const uint DICS_ENABLE = 0x01;
    public const uint DICS_DISABLE = 0x02;
    public const uint DICS_FLAG_GLOBAL = 0x00000001;
}