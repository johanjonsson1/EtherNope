using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace EtherNope;

public class NetworkAdapterManager
{
    private readonly ILogger _logger;

    public NetworkAdapterManager(ILogger logger)
    {
        _logger = logger;
    }

    public void FindAndChangeAdapterState(string targetName, bool shouldDisable)
    {
        IntPtr deviceInfoSet = Interop.SetupDiGetClassDevs(
            ref Interop.GUID_DEVCLASS_NET,
            null,
            IntPtr.Zero,
            Interop.DIGCF_PRESENT);

        if (deviceInfoSet == IntPtr.Zero)
        {
            _logger.LogError("Failed to get device info set.");
            return;
        }

        try
        {
            uint index = 0;
            var devInfoData = new Interop.SpDevInfoData();
            devInfoData.CbSize = (uint)Marshal.SizeOf<Interop.SpDevInfoData>();
            bool found = false;

            while (Interop.SetupDiEnumDeviceInfo(deviceInfoSet, index, ref devInfoData))
            {
                index++;
                string? name = GetDeviceName(deviceInfoSet, ref devInfoData);

                if (name == null || !name.Contains(targetName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                found = true;
                var header = new Interop.SpClassInstallHeader
                {
                    CbSize = (uint)Marshal.SizeOf<Interop.SpClassInstallHeader>(),
                    InstallFunction = Interop.DIF_PROPERTYCHANGE
                };

                var propChangeParams = new Interop.SpPropChangeParams
                {
                    ClassInstallHeader = header,
                    StateChange = shouldDisable ? Interop.DICS_DISABLE : Interop.DICS_ENABLE,
                    Scope = Interop.DICS_FLAG_GLOBAL,
                    HwProfile = 0
                };

                IntPtr paramsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<Interop.SpPropChangeParams>());
                try
                {
                    Marshal.StructureToPtr(propChangeParams, paramsPtr, false);

                    bool setParams = Interop.SetupDiSetClassInstallParams(deviceInfoSet, ref devInfoData, paramsPtr, Marshal.SizeOf<Interop.SpPropChangeParams>());
                    if (!setParams)
                    {
                        _logger.LogError($"Failed to set class install params for device {name}");
                        continue;
                    }

                    bool result = Interop.SetupDiCallClassInstaller(
                        Interop.DIF_PROPERTYCHANGE,
                        deviceInfoSet,
                        ref devInfoData);

                    var action = shouldDisable ? "disable" : "enable";
                    if (result)
                    {
                        _logger.LogInformation($"Adapter {name} {action}d successfully.");
                    }
                    else
                    {
                        _logger.LogError($"Failed to {action} adapter {name}.");
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(paramsPtr); // always free allocated memory
                }
            }

            if (!found)
            {
                _logger.LogError("No adapter not found!");
            }
        }
        finally
        {
            Interop.SetupDiDestroyDeviceInfoList(deviceInfoSet); // release resources
        }
    }

    private static string? GetDeviceName(IntPtr deviceInfoSet, ref Interop.SpDevInfoData devInfoData)
    {
        byte[] buffer = new byte[256];

        bool callSucceeded = Interop.SetupDiGetDeviceRegistryProperty(
            deviceInfoSet,
            ref devInfoData,
            Interop.SPDRP_FRIENDLYNAME,
            out uint regType,
            buffer,
            (uint)buffer.Length,
            out uint requiredSize);

        if (callSucceeded)
        {
            return Encoding.Unicode.GetString(buffer).TrimEnd('\0');
        }

        callSucceeded = Interop.SetupDiGetDeviceRegistryProperty(
            deviceInfoSet,
            ref devInfoData,
            Interop.SPDRP_DEVICEDESC,
            out regType,
            buffer,
            (uint)buffer.Length,
            out requiredSize);

        if (callSucceeded)
        {
            return Encoding.Unicode.GetString(buffer).TrimEnd('\0');
        }
        
        return null;
    }
}
