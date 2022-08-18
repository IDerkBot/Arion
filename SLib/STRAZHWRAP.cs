//////////////////////////////////////////////////////
// NoName                                                 //
// date: 2021                                       //
//////////////////////////////////////////////////////



///#define __X64

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace STRAZH_LIB
{
    public static class DataType
    {
        public const ushort DATASHORT = 4;      //2 byte integer
        public const ushort DATALONG = 32;      //4 byte integer
        public const ushort DATAFLOAT = 2;      //8 byte double
        public const ushort DATASIGNED = 8;    // signed
    }
    public static class V01
    {
        //*****************EVENT_APPLICATION_STATUS
        public const int EVENT_APPLICATION_STATUS = 1;
        public const int SDK_STATUS_ERROR = -1;
        public const int SDK_STATUS_START = 1; //Type char, application file name
        public const int SDK_STATUS_RUNNING = 2;
        public const int SDK_STATUS_STOP = 3;

        //*****************EVENT_SERVER_STATUS
        public const int EVENT_SERVER_STATUS = 2;
        public const int SDK_STATUS_CONNECTED = 101;
        //Panel connection status SDK_NOTIFY_PANEL_CONNECTED or SDK_NOTIFY_PANEL_DISCONNECTED will be the next notification
        public const int SDK_STATUS_DISCONNECTED = 102;
        public const int SDK_STATUS_ALREADY_CONNECTED = 103;
        public const int SDK_STATUS_ALREADY_DISCONNECTED = 104;

        public const int SDK_STATUS_MEMORY_ERROR = -151;
        public const int SDK_STATUS_SEND_ERROR = -152;
        public const int SDK_STATUS_RECEIVE_ERROR = -153;
        public const int SDK_STATUS_CONNECT_ERROR = -154;
        public const int SDK_STATUS_WSA_ERROR = -155;
        public const int SDK_STATUS_HANDSHAKE_ERROR = -156;
        public const int SDK_STATUS_UBNORMAL_RECEIVE_THREAD_TERMINATION = -157;

        //*****************EVENT_SERVER_NOTIFICATION
        public const int EVENT_SERVER_NOTIFICATION = 3;
        //Server notifications
        public const int SDK_NOTIFY_PANEL_CONNECTED = 201; //No parameters; 
                                                           //Received asynchronously whenerver panel is connected or immediately after SDK_STATUS_CONNECTED notification
        public const int SDK_NOTIFY_IMAGE_READY = 202; //No parameters
        public const int SDK_NOTIFY_OFFSET_CALIBRATION_DONE = 203; //No parameters
        public const int SDK_NOTIFY_FRAME_AVERAGING = 204; //Type int, nFrames
        public const int SDK_NOTIFY_EXPOSURE = 205; //Type int, Exposure (ms)
        public const int SDK_NOTIFY_GAIN = 206; //Type int, Gain
        public const int SDK_NOTIFY_TRIGGER_MODE = 207; //Type int, Trigger Mode
        public const int SDK_NOTIFY_FRAME_TRIGGER_MODE = 208;   //Type int, Frame Trigger Mode
        public const int SDK_NOTIFY_XRAY_MODE = 209; //Type int, X-ray Mode
        public const int SDK_NOTIFY_FILE_SAVED = 210; //Type char, file name
        public const int SDK_NOTIFY_TIFF_SAVED = 211;   //Type char, file name
        public const int SDK_NOTIFY_CONFIGURATION_RESTORED = 212; //No parameters
        public const int SDK_NOTIFY_CONFIGURATION_SAVED = 213; //No parameters

        public const int SDK_NOTIFY_PANEL_DISCONNECTED = -251; //No parameters
                                                               //Received asynchronously whenerver panel is disconnected or immediately after SDK_STATUS_CONNECTED notification
        public const int SDK_NOTIFY_ACQUISITION_ON = -252; //No parameters
        public const int SDK_NOTIFY_BUSY = -253; //No parameters
        public const int SDK_NOTIFY_PANEL_ERROR = -254; //Type char, error text
        public const int SDK_NOTIFY_CONFIGURATION_ERROR = -255; //No parameters
        public const int SDK_NOTIFY_CONFIGURATION_FILE_NOT_FOUND = -256; //No parameters

        //****************Commands used by SDKSendCommand
        public const int SDK_DISCONNECT = 0;    //No parameters
        public const int SDK_SNAP = 1;  //No parameters
        public const int SDK_OFFSET_CALIBRATION = 2;    //No parameters
        public const int SDK_SET_EXPOSURE = 3;  //Type int, exposure in ms >= 100
        public const int SDK_SET_FRAME_AVERAGING = 4;   //Type int, number of frames to average >=1
        public const int SDK_SET_GAIN = 5;  //Type int {0,1,2,3,4}
        public const int SDK_SET_TRIGGER_MODE = 6;  //Type int {0,1}
        public const int SDK_SET_FRAME_TRIGGER_MODE = 7;    //Type int {0,1}
        public const int SDK_SET_XRAY_MODE = 8; //Type int {0,1}
        public const int SDK_SAVE_FILE = 9; //Type char: file name or no parameters in case of file name prompt
        public const int SDK_NEW_FILE = 10; //No parameters
        public const int SDK_SAVE_TIFF = 11;    //Type char: file name or no parameters in case of file name prompt
        public const int SDK_SET_FILTER = 12;   //Type int {0-no filter, 1,2,3,4} 4 preset fully adjustable filters are available 
                                                //No confirmation is to be received for this command
        public const int SDK_SAVE_CONFIGURATION = 13;   //Type int {0,1,2....any reasonable number}
        public const int SDK_RESTORE_CONFIGURATION = 14;    //Type int {0,1,2....any reasonable number}

        public const int SDK_OK = 0;
        public const int SDK_ERR = -1;
    }

    public class SHRAZHWRAP
    {
        #region struct definitions

        #region org_structs

        [StructLayout(LayoutKind.Sequential)]
        public struct SDK_FUNCTIONS
        {
            [MarshalAs(UnmanagedType.FunctionPtr)]  //tSDKSetCallback* SDKSetCallback;
            public IntPtr tSDKSetCallback;
            [MarshalAs(UnmanagedType.FunctionPtr)] //          tSDKIsRunning* SDKIsRunning;
            public IntPtr tSDKIsRunning;
            [MarshalAs(UnmanagedType.FunctionPtr)] //          tSDKRunApplication* SDKRunApplication;
            public IntPtr tSDKRunApplication;
            [MarshalAs(UnmanagedType.FunctionPtr)] //          tSDKSetServerParameters* SDKSetServerParameters;
            public IntPtr tSDKSetServerParameters;
            [MarshalAs(UnmanagedType.FunctionPtr)] //          tSDKConnect* SDKConnect;
            public IntPtr tSDKConnect;
            [MarshalAs(UnmanagedType.FunctionPtr)] //          tSDKDisconnect* SDKDisconnect;
            public IntPtr tSDKDisconnect;
            [MarshalAs(UnmanagedType.FunctionPtr)] //          tSDKSendCommand* SDKSendCommand;
            public IntPtr tSDKSendCommand;
        }

        #endregion  //org_structs
        #endregion // wpe_demo

        ///public delegate int MAINSDKPROC(int CommandID, int CommandStatus, ref IntPtr pData, int DataType, int DataSize);
        public delegate int MAINSDKPROC(int CommandID, int CommandStatus, int DataType, int DataSize);

        public class SHRAZHWRAP_CLASS
        {

            public SHRAZHWRAP_CLASS()
            {
            }

            //~SHRAZHWRAP_CLASS()
            //{
            //    WRAPPER_Acquisition_CloseAll();
            //}

            #region Wrapped Functions

            #region wrapper_org
            //      bool __stdcall SDKIsRunning(void); //Valid only when client is located on the same computer
            [DllImport("MAINSDKDLL.dll", EntryPoint = "SDKIsRunning")]
            private static extern bool SDKIsRunning();
            public bool WRAPPER_SDKIsRunning()
            {
                try
                {
                    return SDKIsRunning();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return false;
                }
            }

            //      int __stdcall SDKRunApplication(void); //Valid only when client is located on the same computer
            [DllImport("MAINSDKDLL.dll", EntryPoint = "SDKRunApplication")]
            private static extern int SDKRunApplication();
            public int WRAPPER_SDKRunApplication()
            {
                try
                {
                    return SDKRunApplication();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return V01.SDK_ERR;
                }
            }

            //void __stdcall SDKSetServerParameters(char* szName, int Port, int LanguageID);
            [DllImport("MAINSDKDLL.dll", EntryPoint = "SDKRunApplication")]
            private static extern void SDKSetServerParameters(ref char szName, int aPort, int aLanguageID);
            public void WRAPPER_SDKSetServerParameters(ref char szName, int aPort)
            {
                try
                {
                    int lang = (0x01 << 10) | 0x01;                    
                    SDKSetServerParameters(ref szName, aPort, lang);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            //      int __stdcall SDKConnect(void);
            [DllImport("MAINSDKDLL.dll", EntryPoint = "SDKConnect")]
            private static extern int SDKConnect();
            public int WRAPPER_SDKConnect()
            {
                try
                {
                    return SDKConnect();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return V01.SDK_ERR;
                }
            }

            //      int __stdcall SDKDisconnect(void);
            [DllImport("MAINSDKDLL.dll", EntryPoint = "SDKDisconnect")]
            private static extern int SDKDisconnect();
            public int WRAPPER_SDKDisconnect()
            {
                try
                {
                    return SDKDisconnect();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return V01.SDK_ERR;
                }
            }

            //      int __stdcall SDKSendCommand(int Command, void* pData, int DataType, int DataSize);
            [DllImport("MAINSDKDLL.dll", EntryPoint = "SDKSendCommand")]
            // unsafe private static extern int SDKSendCommand(int aCommand, void* pData, int aDataType, int aDataSize);
            unsafe private static extern int SDKSendCommand(int aCommand, void* pData, int aDataType, int aDataSize);
            unsafe public int WRAPPER_SDKSendCommand(int aCommand, IntPtr pStatCom, ref int aStatCom, int aDataType, int aDataSize)
            {
                int result = 0;
                try
                {
                    if (pStatCom == IntPtr.Zero)
                        result = SDKSendCommand(aCommand, null, aDataType, aDataSize);
                    else
                    {
                        result = SDKSendCommand(aCommand, (void*)aStatCom, aDataType, aDataSize);
                    }
                    return result;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    return V01.SDK_ERR;
                }
            }

            #endregion // wrapper_org
            #endregion
        }
    }
}
