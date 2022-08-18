//////////////////////////////////////////////////////
// Varex Imaging                                    //
// date: 2021                                       //
//////////////////////////////////////////////////////



using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace HIS
{
//    [StructLayout(LayoutKind.Sequential)]
    public class WinHeaderType
    {
	    public short FileType;			            // File ID (0x7000)
        public short HeaderSize;        		    // Size of this file header in Bytes
        public short HeaderVersion;		            // yy.y
        public int FileSize;			            // Size of the whole file in Bytes
        public short ImageHeaderSize;	            // Size of the image header in Bytes
        public short    ULX,    
                        ULY,    
                        BRX,    
                        BRY;                        // bounding rectangle of the image
        public short NrOfFrames;    	        	// self explanatory
        public short Correction;        	    	// 0 = none, 1 = offset, 2 = gain, 4 = bad pixel, (ored)
        public double IntegrationTime;	            // frame time in microseconds
        public short TypeOfNumbers;         		//	if (Header.TypeOfNumbers & 1) nType |= ERRORMAPLOADED;
                                                    //if (Header.TypeOfNumbers & 8) nType |= DATASIGNED;
                                                    //if (Header.TypeOfNumbers & 2) nType |= DATAFLOAT;
                                                    //if (Header.TypeOfNumbers & 4) nType |= DATASHORT;
                                                    //if (Header.TypeOfNumbers & 32) nType |= DATALONG;
        public byte[] x;

        public WinHeaderType(int cntRedundantBytes)
        {
            FileType            = 0x7000;			// File ID (0x7000)
            HeaderSize		    = 68;               // Size of this file header in Bytes
            HeaderVersion		= 100;              // yy.y
            FileSize            = 2097252;          // Size of the whole file in Bytes
            ImageHeaderSize     = 32;	            // Size of the image header in Bytes
            ULX                 = 1;                // NOT 0!!! that was the problem for the schraeg bilder
            ULY                 = 1;                // NOT 0!!! that was the problem for the schraeg bilder
            BRX                 = 512;
            BRY                 = 512;              // bounding rectangle of the image
            NrOfFrames          = 1;    		    // self explanatory
            Correction          = 0;	    	    // 0 = none, 1 = offset, 2 = gain, 4 = bad pixel, (ored)
            IntegrationTime     = 66000.0;	        // frame time in microseconds
            TypeOfNumbers       = 4; 		        // short, long integer, float, signed/unsigned, inverted, 
            x = new byte[cntRedundantBytes];
        }

        public void SetParams(short sRows, short sCols, short sFrames, short sCorrection, double dblTiming, short sDataType)
        {
            BRX = sRows;
            BRY = sCols;                    // bounding rectangle of the image
            NrOfFrames = sFrames;    		// self explanatory
            Correction = sCorrection;	    // 0 = none, 1 = offset, 2 = gain, 4 = bad pixel, (ored)
            IntegrationTime = dblTiming;	// frame time in microseconds
            TypeOfNumbers = sDataType; 	    // short, long integer, float, signed/unsigned, inverted, 

            if ((TypeOfNumbers & 4) !=0)                                                    // ushort or signed short
                FileSize = (sRows * sCols * sFrames * sizeof(short)) + ImageHeaderSize + HeaderSize;
            else if ( ( (TypeOfNumbers & 32) != 0) || ((TypeOfNumbers == 1) ) )             // uint or signed int or fault data
                FileSize = (sRows * sCols * sFrames * sizeof(int)) + ImageHeaderSize + HeaderSize;
            else if (TypeOfNumbers == 2)                                                    // double
                FileSize = (sRows * sCols * sFrames * sizeof(double)) + ImageHeaderSize + HeaderSize;
        }
    };

    public class HisImage
    {
        public WinHeaderType    m_FileHeader;
        private ushort[]        m_usImageData;
        private uint[]          m_uiImageData;
        private double[]        m_dblImageData;
        private short[]        m_sImageData;
        private int[]          m_iImageData;
        private uint[]          m_uiFaultData;
        public string           m_strFileName;
        protected FileStream    m_HisFileStream;
        protected BinaryWriter  m_binWriter;

        public HisImage(short sTypeOfNumbers)
        {
            m_strFileName = "default.his";
            m_FileHeader = new WinHeaderType(34);
            m_FileHeader.TypeOfNumbers = sTypeOfNumbers;
        }

        public HisImage(string strFileName, short sRows, short sColumns, short sFrames, short sCorrections, double dblIntTime, short sTypeOfNumbers)
        {
            m_strFileName = strFileName;
            m_FileHeader = new WinHeaderType(34);
            m_FileHeader.SetParams(sColumns, sRows, sFrames, sCorrections, dblIntTime, sTypeOfNumbers);
        }

        public void LoadHisImage(string strFileName)
        {
            if (Path.GetExtension(strFileName).ToLower() == ".his")
            {
                /***************************************** Init ************************************************/

                m_strFileName = strFileName;
                m_FileHeader = new WinHeaderType(34);        // 34->68byte size; 66->100byte size

                // Fill header struct
                int headersize = m_FileHeader.ImageHeaderSize + m_FileHeader.HeaderSize;

                byte[] buffer = new byte[headersize];
                buffer = File.ReadAllBytes(strFileName);

                //if (sizeof(buffer) == 0)
                //    return;

                m_FileHeader.FileType = (short)((short)buffer[0] + ((short)buffer[1] << 8));
                m_FileHeader.HeaderSize = (short)((short)buffer[2] + ((short)buffer[3] << 8));
                m_FileHeader.HeaderVersion = (short)((short)buffer[4] + ((short)buffer[5] << 8));
                m_FileHeader.FileSize = (short)((short)buffer[6] + ((short)buffer[7] << 8) + ((short)buffer[8] << 16) + ((short)buffer[9] << 24));
                m_FileHeader.ImageHeaderSize = (short)((short)buffer[10] + ((short)buffer[11] << 8));
                m_FileHeader.ULX = (short)((short)buffer[12] + ((short)buffer[13] << 8));
                m_FileHeader.ULY = (short)((short)buffer[14] + ((short)buffer[15] << 8));
                m_FileHeader.BRX = (short)((short)buffer[16] + ((short)buffer[17] << 8));
                m_FileHeader.BRY = (short)((short)buffer[18] + ((short)buffer[19] << 8));
                m_FileHeader.NrOfFrames = (short)((short)buffer[20] + ((short)buffer[21] << 8));
                m_FileHeader.Correction = (short)((short)buffer[22] + ((short)buffer[23] << 8)); ;
                m_FileHeader.IntegrationTime = (short)((short)buffer[24] + ((short)buffer[25] << 8) + ((short)buffer[26] << 16) + ((short)buffer[27] << 24)
                                                    + (short)((short)buffer[28] << 32) + ((short)buffer[29] << 40) + ((short)buffer[30] << 48) + ((short)buffer[31] << 56));
                m_FileHeader.TypeOfNumbers = (short)((short)buffer[32] + ((short)buffer[33] << 8));

                int width = m_FileHeader.BRX;
                int height = m_FileHeader.BRY;
                int frames = m_FileHeader.NrOfFrames;

                byte[] bBuffer = new byte[width * height * frames * 2 + headersize];
                
                bBuffer = File.ReadAllBytes(m_strFileName);

                int nPixelCnt = 0;
                ushort nShift = 8;
                ushort nDataWidth = 2;

                if (m_FileHeader.TypeOfNumbers == 4)            // DATASHORT;
                {
                    m_usImageData = new ushort[width * height * frames];
                    nDataWidth = 2;

                    for (int nByteCnt = headersize; nByteCnt < headersize + width * height * frames * nDataWidth; nByteCnt = nByteCnt + nDataWidth)           // headersize immer gleich
                    {
                        for (int i = 0; i < nDataWidth; i++ )
                            m_usImageData[nPixelCnt] += (ushort)(bBuffer[nByteCnt + i] << (nShift * i));
                        
                        nPixelCnt++;
                    }
                }
                else if (m_FileHeader.TypeOfNumbers == 32)      // DATALONG
                {
                    m_uiImageData = new uint[width * height * frames];
                    nDataWidth = 4;

                    for (int nByteCnt = headersize; nByteCnt < headersize + width * height * frames * nDataWidth; nByteCnt = nByteCnt + nDataWidth)           // headersize immer gleich
                    {
                        for (int i = 0; i < nDataWidth; i++)
                            m_uiImageData[nPixelCnt] += (uint)(bBuffer[nByteCnt + i] << (nShift * i));

                        nPixelCnt++;
                    }
                }
                else if (m_FileHeader.TypeOfNumbers == 2)       // DATAFLOAT
                {
                    m_dblImageData = new double[width * height * frames];
                    nDataWidth = 8;

                    for (int nByteCnt = headersize; nByteCnt < headersize + width * height * frames * nDataWidth; nByteCnt = nByteCnt + nDataWidth)           // headersize immer gleich
                    {
                        for (int i = 0; i < nDataWidth; i++)
                            m_dblImageData[nPixelCnt] += (double)(bBuffer[nByteCnt + i] << (nShift * i));

                        nPixelCnt++;
                    }
                }
                else if ((m_FileHeader.TypeOfNumbers & 8) != 0)     // DATASIGNED
                {
                    if ((m_FileHeader.TypeOfNumbers & 4) != 0)      // DATASHORT;
                    {
                        m_sImageData = new short[width * height * frames];
                        nDataWidth = 2;

                        for (int nByteCnt = headersize; nByteCnt < headersize + width * height * frames * nDataWidth; nByteCnt = nByteCnt + nDataWidth)           // headersize immer gleich
                        {
                            for (int i = 0; i < nDataWidth; i++)
                                m_sImageData[nPixelCnt] += (short)(bBuffer[nByteCnt + i] << (nShift * i));

                            nPixelCnt++;
                        }
                    }
                    else if ((m_FileHeader.TypeOfNumbers & 32) != 0)     // DATALONG
                    {
                        m_iImageData = new int[width * height * frames];
                        nDataWidth = 4;

                        for (int nByteCnt = headersize; nByteCnt < headersize + width * height * frames * nDataWidth; nByteCnt = nByteCnt + nDataWidth)           // headersize immer gleich
                        {
                            for (int i = 0; i < nDataWidth; i++)
                                m_iImageData[nPixelCnt] += (int)(bBuffer[nByteCnt + i] << (nShift * i));

                            nPixelCnt++;
                        }
                    }
                }
                else if (m_FileHeader.TypeOfNumbers == 1)     // ERRORMAPLOADED
                {
                    m_uiFaultData = new uint[width * height * frames];
                    nDataWidth = 4;

                    for (int nByteCnt = headersize; nByteCnt < headersize + width * height * frames * nDataWidth; nByteCnt = nByteCnt + nDataWidth)           // headersize immer gleich
                    {
                        for (int i = 0; i < nDataWidth; i++)
                            m_uiFaultData[nPixelCnt] += (uint)(bBuffer[nByteCnt + i] << (nShift * i));

                        nPixelCnt++;
                    }
                }
            }
            else
            {
                Debug.WriteLine("No HIS file, cannot be opened!");
            }
        }

        public int SaveImage(string path, bool bOverwrite)
        {
            try
            {
                m_HisFileStream = new FileStream(path, FileMode.CreateNew);
            }
            catch (IOException ioE)
            {
                if(bOverwrite == true)
                    m_HisFileStream = new FileStream(path, FileMode.Create);
                else
                    return XISL_LIBRARY.XISL.XislConstants.ErrorCodes.HIS_ERROR_WRITE_DATA;
            }

            //	if (pFile)
            if(true)
            {
                m_binWriter = new BinaryWriter(m_HisFileStream);

                // FileHeader
                m_binWriter.Write(BitConverter.GetBytes(m_FileHeader.FileType));
                m_binWriter.Write(BitConverter.GetBytes(m_FileHeader.HeaderSize));
                m_binWriter.Write(BitConverter.GetBytes(m_FileHeader.HeaderVersion));
                m_binWriter.Write(BitConverter.GetBytes(m_FileHeader.FileSize));
                m_binWriter.Write(BitConverter.GetBytes(m_FileHeader.ImageHeaderSize));
                m_binWriter.Write(BitConverter.GetBytes(m_FileHeader.ULX));
                m_binWriter.Write(BitConverter.GetBytes(m_FileHeader.ULY));
                m_binWriter.Write(BitConverter.GetBytes(m_FileHeader.BRX));
                m_binWriter.Write(BitConverter.GetBytes(m_FileHeader.BRY));
                m_binWriter.Write(BitConverter.GetBytes(m_FileHeader.NrOfFrames));
                m_binWriter.Write(BitConverter.GetBytes(m_FileHeader.Correction));
                m_binWriter.Write(BitConverter.GetBytes(m_FileHeader.IntegrationTime));
                m_binWriter.Write(BitConverter.GetBytes(m_FileHeader.TypeOfNumbers));
                m_binWriter.Write(m_FileHeader.x);

                // Dummy data
                for (int lf = 0; lf < 32; lf++)
                    m_binWriter.Write((byte)1);

                if (m_FileHeader.TypeOfNumbers == 4)      // DATASHORT;
                {
                    m_FileHeader.FileSize = m_FileHeader.HeaderSize
                                          + m_FileHeader.ImageHeaderSize
                                          + (
                                            m_FileHeader.BRX
                                          * m_FileHeader.BRY
                                          * m_FileHeader.NrOfFrames
                                          * sizeof(short)
                                            );

                    for (int iPxlCnt = 0; iPxlCnt < m_FileHeader.BRX * m_FileHeader.BRY * m_FileHeader.NrOfFrames; iPxlCnt++)
                    {
                        m_binWriter.Write(BitConverter.GetBytes(m_usImageData[iPxlCnt]));                        
                    }
                }
                else if (m_FileHeader.TypeOfNumbers == 32)     // DATALONG
                {
                    m_FileHeader.FileSize = m_FileHeader.HeaderSize
                                          + m_FileHeader.ImageHeaderSize
                                          + (
                                            m_FileHeader.BRX
                                          * m_FileHeader.BRY
                                          * m_FileHeader.NrOfFrames
                                          * sizeof(int)
                                            );

                    for (int iPxlCnt = 0; iPxlCnt < m_FileHeader.BRX * m_FileHeader.BRY * m_FileHeader.NrOfFrames; iPxlCnt++)
                    {
                        m_binWriter.Write(BitConverter.GetBytes(m_uiImageData[iPxlCnt]));
                    }
                }
                else if (m_FileHeader.TypeOfNumbers == 2)     // DATAFLOAT
                {
                    m_FileHeader.FileSize = m_FileHeader.HeaderSize
                                          + m_FileHeader.ImageHeaderSize
                                          + (
                                            m_FileHeader.BRX
                                          * m_FileHeader.BRY
                                          * m_FileHeader.NrOfFrames
                                          * sizeof(double)
                                            );

                    for (int iPxlCnt = 0; iPxlCnt < m_FileHeader.BRX * m_FileHeader.BRY * m_FileHeader.NrOfFrames; iPxlCnt++)
                    {
                        m_binWriter.Write(BitConverter.GetBytes(m_dblImageData[iPxlCnt]));
                    }
                }
                else if ((m_FileHeader.TypeOfNumbers & 8) != 0)         // DATASIGNED
                {
                    if ((m_FileHeader.TypeOfNumbers & 4) != 0)          // DATASHORT;
                    {
                        m_FileHeader.FileSize = m_FileHeader.HeaderSize
                                          + m_FileHeader.ImageHeaderSize
                                          + (
                                            m_FileHeader.BRX
                                          * m_FileHeader.BRY
                                          * m_FileHeader.NrOfFrames
                                          * sizeof(short)
                                            );

                        for (int iPxlCnt = 0; iPxlCnt < m_FileHeader.BRX * m_FileHeader.BRY * m_FileHeader.NrOfFrames; iPxlCnt++)
                        {
                            m_binWriter.Write(BitConverter.GetBytes(m_sImageData[iPxlCnt]));
                        }
                    }
                    else if ((m_FileHeader.TypeOfNumbers & 32) != 0)        // DATALONG
                    {
                        m_FileHeader.FileSize = m_FileHeader.HeaderSize
                                          + m_FileHeader.ImageHeaderSize
                                          + (
                                            m_FileHeader.BRX
                                          * m_FileHeader.BRY
                                          * m_FileHeader.NrOfFrames
                                          * sizeof(int)
                                            );

                        for (int iPxlCnt = 0; iPxlCnt < m_FileHeader.BRX * m_FileHeader.BRY * m_FileHeader.NrOfFrames; iPxlCnt++)
                        {
                            m_binWriter.Write(BitConverter.GetBytes(m_iImageData[iPxlCnt]));
                        }
                    }
                }
                else if (m_FileHeader.TypeOfNumbers == 1)                   // ERRORMAPLOADED
                {
                    m_FileHeader.FileSize = m_FileHeader.HeaderSize
                                          + m_FileHeader.ImageHeaderSize
                                          + (
                                            m_FileHeader.BRX
                                          * m_FileHeader.BRY
                                          * m_FileHeader.NrOfFrames
                                          * sizeof(int)
                                            );

                    for (int iPxlCnt = 0; iPxlCnt < m_FileHeader.BRX * m_FileHeader.BRY * m_FileHeader.NrOfFrames; iPxlCnt++)
                    {
                        m_binWriter.Write(BitConverter.GetBytes(m_uiFaultData[iPxlCnt]));
                    }
                }

                m_HisFileStream.Close();
            }
            else
            {
                Debug.WriteLine("Error creating file!");
            }

            return 1;
        }      

        public ushort[] ImageAsUShort
        {
            get { return m_usImageData; }
            set { m_usImageData = value; }
        }

        public uint[] ImageAsUInt
        {
            get { return m_uiImageData; }
            set { m_uiImageData = value; }
        }

        public double[] ImageAsDouble
        {
            get { return m_dblImageData; }
            set { m_dblImageData = value; }
        }

        public string FileNameString
        {
            get { return m_strFileName; }
            set { m_strFileName = value; }
        }

        public int SaveAsSingleRawImage(string path, bool bOverwrite, int iImageIndex)
        {
            try
            {
                m_HisFileStream = new FileStream(path, FileMode.CreateNew);
            }
            catch (IOException ioE)
            {
                if (bOverwrite == true)
                    m_HisFileStream = new FileStream(path, FileMode.Create);
                else
                    return XISL_LIBRARY.XISL.XislConstants.ErrorCodes.HIS_ERROR_WRITE_DATA;
            }

            //	if (pFile)
            if (true)
            {
                m_binWriter = new BinaryWriter(m_HisFileStream);

                int iStartCnt = iImageIndex * m_FileHeader.BRX * m_FileHeader.BRY;

                if (m_FileHeader.TypeOfNumbers == 4)            // DATASHORT;
                {
                    for (int iPxlCnt = 0; iPxlCnt < m_FileHeader.BRX * m_FileHeader.BRY; iPxlCnt++)
                    {
                        m_binWriter.Write(BitConverter.GetBytes(m_usImageData[iStartCnt+iPxlCnt]));
                    }
                }
                else if (m_FileHeader.TypeOfNumbers == 32)              // DATALONG
                {
                    for (int iPxlCnt = 0; iPxlCnt < m_FileHeader.BRX * m_FileHeader.BRY; iPxlCnt++)
                    {
                        m_binWriter.Write(BitConverter.GetBytes(m_uiImageData[iStartCnt+iPxlCnt]));
                    }
                }   
                else if (m_FileHeader.TypeOfNumbers == 2)               // DATAFLOAT
                {
                    for (int iPxlCnt = 0; iPxlCnt < m_FileHeader.BRX * m_FileHeader.BRY; iPxlCnt++)
                    {
                        m_binWriter.Write(BitConverter.GetBytes(m_dblImageData[iStartCnt+iPxlCnt]));
                    }
                }
                else if ((m_FileHeader.TypeOfNumbers & 8) != 0)         // DATASIGNED
                {
                    if ((m_FileHeader.TypeOfNumbers & 4) != 0)          // DATASHORT;
                    {
                        for (int iPxlCnt = 0; iPxlCnt < m_FileHeader.BRX * m_FileHeader.BRY; iPxlCnt++)
                        {
                            m_binWriter.Write(BitConverter.GetBytes(m_sImageData[iStartCnt+iPxlCnt]));
                        }
                    }
                    else if ((m_FileHeader.TypeOfNumbers & 32) != 0)     // DATALONG
                    {
                        for (int iPxlCnt = 0; iPxlCnt < m_FileHeader.BRX * m_FileHeader.BRY; iPxlCnt++)
                        {
                            m_binWriter.Write(BitConverter.GetBytes(m_iImageData[iStartCnt+iPxlCnt]));
                        }
                    }
                }
                else if (m_FileHeader.TypeOfNumbers == 1)     // ERRORMAPLOADED
                {
                    for (int iPxlCnt = 0; iPxlCnt < m_FileHeader.BRX * m_FileHeader.BRY; iPxlCnt++)
                    {
                        m_binWriter.Write(BitConverter.GetBytes(m_uiFaultData[iStartCnt+iPxlCnt]));
                    }
                }

                m_HisFileStream.Close();
            }
            else
            {
                Console.WriteLine("Error creating file!");
            }

            return 1;
        }   
    }
}