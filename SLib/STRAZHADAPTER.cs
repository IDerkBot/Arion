//////////////////////////////////////////////////////
// NoName                                           //
// date: 2021                                       //
//////////////////////////////////////////////////////


#define __X64

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using STRAZH_LIB;

enum V01_SDK_DATA_TYPE
{
    SDK_TYPE_CHAR = 1,
    SDK_TYPE_INT = 2,
    SDK_TYPE_DOUBLE = 3,
};

public struct TypeOfData
{
    public Int32 Command;
    public Int32 DataType; //SDK_DATA_TYPE
    public Int32 DataLength;
};

public static class DataType
{
    public const ushort DATASHORT = 4;      //2 byte integer
    public const ushort DATALONG = 32;      //4 byte integer
    public const ushort DATAFLOAT = 2;      //8 byte double
    public const ushort DATASIGNED = 8;    // signed
}

public static class Commands {
    public const int ID_RUN = 201;
    public const int ID_CONNECT = 1003;
    public const int ID_DISCONNECT = 1004;
    public const int ID_FRAME_AVERAGING = 1005;
    public const int ID_SAVE_FILE = 1006;
    public const int ID_SNAP = 1007;
    public const int ID_EXPOSURE = 1008;
    public const int ID_SAVE_TIFF = 1009;
    public const int ID_GAIN = 1010;
    public const int ID_TRIGGER_MODE = 1011;
    public const int ID_SAVE_FILE_PROMPT = 1012;
    public const int ID_OFFSET = 1013;
    public const int ID_NEW_FILE = 1014;
    public const int ID_XRAY_MODE = 1015;
    public const int ID_FRAME_TRIGGER_MODE = 1016;
    public const int ID_FILTER = 1017;
    public const int ID_SAVE_CONFIGURATION = 1018;
    public const int ID_SAVE_CONFIGURATION2 = 1019;
    public const int ID_RESTORE_CONFIGURATION = 1019;
}
public class STRAZHADAPTER : SHRAZHWRAP.SHRAZHWRAP_CLASS
{
    // Customer Data
    const int MAX_PATH = 260;
    const int GET_DARK_CAL = 128;
    const int GET_BRIGHT_CAL = 129;
    const int DataLen = 4; // hard len of Int32;
    private const int port = 1419;
    private const string server = "127.0.0.1";
    private IntPtr ZeroPtr = IntPtr.Zero;

    //const SOCKET ServerSocket = INVALID_SOCKET;
    const int HandShakeOut = 0x12FAC15D;
    const int HandShakeIn = 0x3BE67890;

    bool m_bConnected;

    int m_command;
    string m_screenText;
    string m_lpszLibPath;
    string m_lpszAveFile;

    //SDKSetCallback m_setCallback;
    SHRAZHWRAP.MAINSDKPROC m_mSDKProc;
    TcpClient m_client;
    NetworkStream m_stream;

    //public delegate void SDKSetCallback(ref STRAZHWRAP.MAINSDKPROC SDKCallback);
    // void __stdcall SDKSetCallback(SDK_V01_NAMESPACE::MAINSDKPROC* pSDKCallback);

    public string ScreenText
    {
        get => m_screenText;
        set => m_screenText = value;
    }

    public int Command
    {
        get => m_command;
        set => m_command = value;
    }

    public STRAZHADAPTER()
    {
        m_bConnected = false;
        m_mSDKProc = new SHRAZHWRAP.MAINSDKPROC(DefaultSDKProc);
        m_client = new TcpClient();
    }

    // преобразует последовательность int в байты
    private static void GetBytesFromInts(ref Int32[] arrInts, ref byte[] arrBytes)
    {
        int cnt = 0;
        foreach (int ivalue in arrInts)
        {
            byte[] locBytes = BitConverter.GetBytes(ivalue);
            foreach (byte b in locBytes)
                arrBytes[cnt++] = b;
        }
    }

    // наоборот из байтов в int
    private static void GetIntFromBytes(ref byte[] intBytes, ref Int32[] arrInts)
    {
        for (int j = 0; j < arrInts.Length; j++)
        {
            Int32 result = 0;
            byte[] locBytes = new byte[DataLen];
            for (int i = 0; i < DataLen; i++)
                locBytes[i] = intBytes[j * DataLen + i];
            Array.Reverse(locBytes);
            foreach (byte b in locBytes)
            {
                result <<= 8;
                result += b;
            }
            arrInts[j] = result;
        }
    }

    ///public void CallCallbackProc(int CommandID, int CommandStatus, ref IntPtr pData, int DataType, int DataSize)
    public void CallCallbackProc(int CommandID, int CommandStatus, int DataType, int DataSize)
    {
        if (m_mSDKProc != null)
            m_mSDKProc(CommandID, CommandStatus, DataType, DataSize);
    }

    public bool IsConnected() { return m_bConnected; } 
    public bool IsRunning() { return WRAPPER_SDKIsRunning();  }

    //public void ReceiveStart()
    //{
    //    ///TCP_SetTimeouts(ServerSocket, 0, 1000); //Set infinite receive timeout and limited send timeout

    //    if (m_hReceiveThread == IntPtr.Zero)
    //    {
    //        int dwThreadId;
    //        //m_hReceiveThread = CreateThread(NULL, NULL, (LPTHREAD_START_ROUTINE)ReceiveThread, NULL, NULL, &dwThreadId);
    //        Thread m_hReceiveThread = new Thread(new ThreadStart(ReceiveThreadAsync));
    //        m_hReceiveThread.Start();
    //    }
    //}
    /// <summary>
    /// Запуск
    /// </summary>
    public void Run()
    {
        if (IsRunning() == false)
            WRAPPER_SDKRunApplication();
    }
    /// <summary>
    /// Подключение к серверу
    /// </summary>
    public void ServerConnect()
    {  
        if (IsRunning())
            if (!IsConnected())
                ServerConnectAsync();
    }
    /// <summary>
    /// Отключение от сервера
    /// </summary>
    public void ServerDisconnect()
    {
        if (IsRunning() == true)
            if (m_bConnected)
            {
                string bParam = "";
                char[] chars = bParam.ToCharArray();
                ServerSendCommandAsync(V01.SDK_DISCONNECT, chars, (int)V01_SDK_DATA_TYPE.SDK_TYPE_CHAR, bParam.Length);
                m_bConnected = false;
            }
    }
    /// <summary>
    /// Захват изображения
    /// </summary>
    /// <returns>Возвращает статус захвата</returns>
    public int Snap()
    {
        if ((IsRunning() == true) && (m_bConnected))
        {
            string bParam = "";
            char[] chars = bParam.ToCharArray();
            ServerSendCommandAsync(V01.SDK_SNAP, chars, (int)V01_SDK_DATA_TYPE.SDK_TYPE_CHAR, bParam.Length);
            return V01.SDK_OK;
        }
        else
            return V01.SDK_ERR;
    }
    /// <summary>
    /// Создание нового файла
    /// </summary>
    /// <returns></returns>
    public int NewFile()
    {
        if ((IsRunning() == true) && m_bConnected)
        {
            string bParam = "";
            char[] chars = bParam.ToCharArray();
            ServerSendCommandAsync(V01.SDK_NEW_FILE, chars, (int)V01_SDK_DATA_TYPE.SDK_TYPE_CHAR, bParam.Length);
            return V01.SDK_OK;
        }
        else
            return V01.SDK_ERR;
    }
    /// <summary>
    /// Сохранение в DCOM формате
    /// </summary>
    /// <param name="bParam"></param>
    /// <returns></returns>
    public int SaveDCM(string bParam)
    {
        if ((WRAPPER_SDKIsRunning() == true) && (m_bConnected))
        {            
            char[] chars = bParam.ToCharArray();
            ServerSendCommandAsync(V01.SDK_SAVE_FILE, chars, (int)V01_SDK_DATA_TYPE.SDK_TYPE_CHAR, bParam.Length);
            return V01.SDK_OK;
        }
        else
            return V01.SDK_ERR;
    }

    public void TCPError(int Status)
    {
        char[] szError; // 512        
        //if (TCP_GetLastError(ServerLanguageID, szError, sizeof(szError)))
        //if 
        //{
        //    IntPtr pData = ZeroPtr;
        //    int len = 0;
        //    //char* pData;
        //    //int len;
        //    //len = (int)strlen(szError);
        //    //pData = (char*)GlobalAlloc(GMEM_FIXED | GMEM_ZEROINIT, len + 1);
        //    //if (pData != NULL)
        //    //{
        //    //    strcpy_s(pData, len + 1, szError);
        //    //CallCallbackProc(EVENT_SERVER_STATUS, Status, pData, SDK_TYPE_CHAR, len + 1);
        //    CallCallbackProc(V01.EVENT_SERVER_STATUS, Status, ref pData, (int)V01_SDK_DATA_TYPE.SDK_TYPE_CHAR, len + 1);
        //    //}
        //}
    }

    private async void ReceiveThreadAsync()
    {
        while (m_bConnected)
        {
            TypeOfData Data;
            Int32[] arrInts = new Int32[3];                               ///                                                                           ///TCP_Receive(Data);

            try
            {
                byte[] arrBytes = new byte[3 * DataLen];
                m_stream = m_client.GetStream();
                Int32 bytes = await m_stream.ReadAsync(arrBytes, 0, arrBytes.Length);
                // Считываем с сервера ответ        
                GetIntFromBytes(ref arrBytes, ref arrInts);
                Data.Command = arrInts[0];
                Data.DataType = arrInts[1];
                Data.DataLength = arrInts[2];

                if (Data.DataLength > 0)
                {
                    try
                    {
                        byte[] data = new byte[Data.DataLength];
                        m_stream = m_client.GetStream();
                        bytes = await m_stream.ReadAsync(data, 0, data.Length);
                        String response = String.Empty;
                        response = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);

                        CallCallbackProc(V01.EVENT_SERVER_NOTIFICATION, Data.Command, Data.DataType, Data.DataLength);
                        Debug.WriteLine("Receive: " + Data.Command.ToString() + " " + response);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                        Debug.WriteLine("Quit 1");
                        m_bConnected = false;
                    }
                }
                else
                {
                    ///await Task.Delay(1000);
                    CallCallbackProc(V01.EVENT_SERVER_NOTIFICATION, Data.Command, 0, 0);
                    Debug.WriteLine("Receive: " + Data.Command.ToString());
                }
                // Отмечаем результат ответа?
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                Debug.WriteLine("Quit 2");
                m_bConnected = false;
            }

            //else //No data available
            //{
            //    CallCallbackProc(V01.EVENT_SERVER_NOTIFICATION, Data.Command, 0, 0);
            //    Debug.WriteLine("No Data");
            //}
        }
        //m_stream.Close();
        m_client.Close();
        CallCallbackProc(V01.EVENT_SERVER_STATUS, V01.SDK_STATUS_DISCONNECTED, 0, 0);
        Debug.WriteLine("SDK_STATUS_DISCONNECTED");
        m_bConnected = false;
        
    }

    //private async void ServerDisconnectAsync()
    //{
    //    if (m_bConnected)
    //    {
    //        char[] chars = new char[1];
    //        chars[0] = '\0';
    //        //await ServerSendCommandAsync(V01.SDK_DISCONNECT, chars, 0, 1);
    //        CallCallbackProc(V01.EVENT_SERVER_STATUS, V01.SDK_STATUS_DISCONNECTED, 0, 0);
    //        Debug.WriteLine("SDK_STATUS_DISCONNECTED");
    //        m_bConnected = false;
    //        return;
    //        ///return V01.SDK_OK; //Connected;               
    //    }
    //}

    private async void ServerConnectAsync()
    {
        if (m_bConnected)
        {
            CallCallbackProc(V01.EVENT_APPLICATION_STATUS, V01.SDK_STATUS_ALREADY_CONNECTED, 0, 0);
            Debug.WriteLine("SDK_STATUS_ALREADY_CONNECTED");
            return;
            ///return V01.SDK_OK; //Connected;               
        }
        await Task.Delay(1000);
        //handshake
        int[] HandShakeInts = new int[1];
        byte[] handShakeBytes = new byte[DataLen];
        HandShakeInts[0] = HandShakeOut;
        GetBytesFromInts(ref HandShakeInts, ref handShakeBytes);

        await m_client.ConnectAsync(server, port);
        //m_client.ReceiveTimeout = 2000;
        //m_client.SendTimeout = 2000;

        m_stream = m_client.GetStream();//m_server.Server.Receive(handShakeBytes);        

        // Отправляем сообщение на сервер
        byte[] data = new byte[handShakeBytes.Length];
        try
        {
            await m_stream.WriteAsync(handShakeBytes, 0, data.Length);
            ///await Task.Delay(1000);
            // Считываем с сервера ответ        
            String response = String.Empty;
            data = new byte[handShakeBytes.Length];
            Int32 bytes = await m_stream.ReadAsync(data, 0, data.Length);
            response = System.Text.Encoding.UTF8.GetString(data, 0, bytes);
            GetIntFromBytes(ref data, ref HandShakeInts);
            if (HandShakeInts[0] == HandShakeIn)
            {
                m_bConnected = true;
                ///await Task.Delay(2000);
                CallCallbackProc(V01.EVENT_SERVER_STATUS, V01.SDK_STATUS_CONNECTED, 0, 0);
                Debug.WriteLine("SDK_STATUS_CONNECTED");
                ReceiveThreadAsync();
                return; //V01.SDK_OK; //Connected;
            }
            else
            {
                CallCallbackProc(V01.EVENT_SERVER_STATUS, V01.SDK_STATUS_HANDSHAKE_ERROR, 0, 0);
                Debug.WriteLine("SDK_STATUS_HANDSHAKE_ERROR");
                m_stream.Close();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
        CallCallbackProc(V01.EVENT_SERVER_STATUS, V01.SDK_STATUS_DISCONNECTED, 0, 0);
        Debug.WriteLine("SDK_STATUS_DISCONNECTED");
        ///return V01.SDK_ERR;
    }

    private async void ServerSendCommandAsync (int CommandID, char[] CommandStatus, int DataType, int DataSize)
    {
        if (m_bConnected)
        {
            TypeOfData Data;
            Int32[] arrInts = new Int32[3];            

            arrInts[0] = Data.Command = CommandID;
            arrInts[1] = Data.DataType = (int)V01_SDK_DATA_TYPE.SDK_TYPE_CHAR;
            arrInts[2] = Data.DataLength = DataSize;            

            // передаем данные (непустая последовательность)
            if (DataSize > 0)
            {
                // для команды
                byte[] arrBytes = new byte[3 * DataLen];            //  Здесь комманда                                                                         ///TCP_Receive(Data);
                GetBytesFromInts(ref arrInts, ref arrBytes); // По байтам
                try
                {
                    m_stream = m_client.GetStream();//
                    m_stream.Write(arrBytes, 0, arrBytes.Length); /// Не нужно ожидания
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    return;
                }
                ///await Task.Delay(1000);
                // Отправляем результат
                CallCallbackProc(V01.EVENT_SERVER_NOTIFICATION, Data.Command, Data.DataType, Data.DataLength);

                // для последовательности данных
                byte[] data = new byte[DataSize];
                for (int i = 0; i < DataSize; i++)
                    data[i] = (byte)CommandStatus[i];
                try
                {
                    m_stream = m_client.GetStream();//
                    await m_stream.WriteAsync(data, 0, DataSize);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    return;
                }
            }
            // последовательность пустая
            else
            {
                byte[] arrBytes = new byte[3 * DataLen];            //  Здесь комманда                                                                         ///TCP_Receive(Data);
                GetBytesFromInts(ref arrInts, ref arrBytes); // По байтам
                try
                {
                    m_stream = m_client.GetStream();//
                    await m_stream.WriteAsync(arrBytes, 0, arrBytes.Length);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    return;
                }

                ///await Task.Delay(1000);
                // Отправляем результат
                CallCallbackProc(V01.EVENT_SERVER_NOTIFICATION, Data.Command, Data.DataType, Data.DataLength);
            }
            Debug.WriteLine("Send: " + Data.Command.ToString());
        }
        else
	    {
            Debug.WriteLine("No Connection");
	    }
    }
    
    // возврат данных
    private int DefaultSDKProc(int CommandID, int CommandStatus, int DataType, int DataSize)
    {
        int Result = V01.SDK_OK;
        m_command = CommandStatus;
        switch (CommandID)
        {
            case V01.EVENT_APPLICATION_STATUS:
                {
                    switch (CommandStatus)
                    {
                        case V01.SDK_STATUS_START:
                            SetText("RUN");
                            break;
                        case V01.SDK_STATUS_RUNNING:
                            SetText("RUNNING");
                            break;
                        case V01.SDK_STATUS_STOP:
                            SetText("STOP");
                            break;
                        case V01.SDK_STATUS_ERROR:
                            SetText("ERROR");
                            break;
                        default:
                            Result = V01.SDK_ERR;
                            SetText("UNKNOWN APPLICATION STATUS");
                            break;
                    }
                    break;
                }
            case V01.EVENT_SERVER_STATUS:
                {
                    switch (CommandStatus)
                    {
                        case V01.SDK_STATUS_CONNECTED:
                            SetText("CONNECTION ESTABLISHED");
                            //Panel connection status SDK_NOTIFY_PANEL_CONNECTED or SDK_NOTIFY_PANEL_DISCONNECTED will be the next notification
                            break;
                        case V01.SDK_STATUS_DISCONNECTED:
                            SetText("DISCONNECTED");
                            break;
                        case V01.SDK_STATUS_ALREADY_CONNECTED:
                            SetText("ALREADY CONNECTED");
                            break;
                        case V01.SDK_STATUS_ALREADY_DISCONNECTED:
                            SetText("ALREADY DISCONNECTED");
                            break;
                        case V01.SDK_STATUS_MEMORY_ERROR:
                            SetText("MEMORY ERROR. DO NOT FORGET TO DISPOSE DATA");
                            break;
                        case V01.SDK_STATUS_SEND_ERROR:
                            SetText("ERROR SENDING DATA");
                            break;
                        case V01.SDK_STATUS_RECEIVE_ERROR:
                            SetText("ERROR RECEIVING DATA");
                            break;
                        case V01.SDK_STATUS_CONNECT_ERROR:
                            SetText("CONNECTION ERROR");
                            break;
                        case V01.SDK_STATUS_HANDSHAKE_ERROR:
                            SetText("HANDSHAKE MISMATCH");
                            break;
                        case V01.SDK_STATUS_UBNORMAL_RECEIVE_THREAD_TERMINATION:
                            SetText("UBNORMAL RECEIVE THREAD TERMINATION");
                            break;
                        default:
                            Result = V01.SDK_ERR;
                            SetText("UNKNOWN SERVER STATUS");
                            break;
                    }
                    break;
                }
            case V01.EVENT_SERVER_NOTIFICATION:
                {
                    switch (CommandStatus)
                    {
                        case V01.SDK_NOTIFY_PANEL_CONNECTED:
                            SetText("PANEL CONNECTED");
                            //Received asynchronously whenerver panel is connected or immediately after SDK_STATUS_CONNECTED notification
                            break;
                        case V01.SDK_NOTIFY_PANEL_DISCONNECTED:
                            SetText("PANEL DISCONNECTED");
                            //Received asynchronously whenerver panel is disconnected or immediately after SDK_STATUS_CONNECTED notification
                            break;
                        case V01.SDK_NOTIFY_ACQUISITION_ON:
                            SetText("ACQUISITION IS ALREADY ON");
                            break;
                        case V01.SDK_NOTIFY_BUSY:
                            SetText("ACQUISITION PANEL IS BUSY");
                            break;
                        case V01.SDK_NOTIFY_PANEL_ERROR:
                            SetText("PANEL ERROR");
                            break;
                        case V01.SDK_NOTIFY_IMAGE_READY:
                            SetText("IMAGE IS READY");
                            break;
                        case V01.SDK_NOTIFY_OFFSET_CALIBRATION_DONE:
                            SetText("OFFSET CALIBRATION DONE");
                            break;
                        case V01.SDK_NOTIFY_EXPOSURE:
                            SetText("EXPOSURE");
                            break;
                        case V01.SDK_NOTIFY_FRAME_AVERAGING:
                            SetText("FRAME AVERAGING");
                            break;
                        case V01.SDK_NOTIFY_GAIN:
                            SetText("GAIN");
                            break;
                        case V01.SDK_NOTIFY_TRIGGER_MODE:
                            SetText("TRIGGER MODE");
                            break;
                        case V01.SDK_NOTIFY_FRAME_TRIGGER_MODE:
                            SetText("FRAME TRIGGER MODE");
                            break;
                        case V01.SDK_NOTIFY_XRAY_MODE:
                            SetText("XRAY MODE");
                            break;
                        case V01.SDK_NOTIFY_FILE_SAVED:
                            SetText("FILE SAVED");
                            break;
                        case V01.SDK_NOTIFY_TIFF_SAVED:
                            SetText("TIFF SAVED");
                            break;
                        case V01.SDK_NOTIFY_CONFIGURATION_RESTORED:
                            SetText("CONFIGURATION RESTORED");
                            break;
                        case V01.SDK_NOTIFY_CONFIGURATION_SAVED:
                            SetText("CONFIGURATION SAVED");
                            break;
                        case V01.SDK_NOTIFY_CONFIGURATION_ERROR:
                            SetText("CONFIGURATION ERROR");
                            break;
                        case V01.SDK_NOTIFY_CONFIGURATION_FILE_NOT_FOUND:
                            SetText("CONFIGURATION FILE NOT FOUND");
                            break;
                        default:
                            Result = V01.SDK_ERR;
                            SetText("UNKNOWN SERVER NOTIFICATION");
                            break;
                    }
                    break;
                }
            default: //Unknown Command
                Result = V01.SDK_ERR;
                break;
        }
        if (DataSize != 0)
        {
            string szMessage = "";
            switch (DataType)
            {
                case (int)V01_SDK_DATA_TYPE.SDK_TYPE_CHAR:
                    SetText(szMessage); //File Name
                    break;
                case (int)V01_SDK_DATA_TYPE.SDK_TYPE_INT:
                    Debug.WriteLine("Int type: " + szMessage.ToString());
                    //sprintf_s(szMessage, sizeof(szMessage), "DATA=%d", *((int*)pData));
                    SetText(szMessage);
                    break;
                case (int)V01_SDK_DATA_TYPE.SDK_TYPE_DOUBLE:
                    Debug.WriteLine(szMessage);
                    //sprintf_s(szMessage, sizeof(szMessage), "DATA=%g", *((double*)pData));
                    SetText(szMessage);
                    break;
                default:
                    Result = V01.SDK_ERR;
                    SetText("UNKNOWN DATA TYPE");
                    break;
            }
        }
        return Result;
    }
    private void SetText(string szText)
    {
        m_screenText = szText;
    }
}


