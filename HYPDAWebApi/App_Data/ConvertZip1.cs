using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.GZip;
using System.Data;
using System.Xml;
using System.Xml.Serialization;

public class ConvertZip1
{
    /// <summary> 
    /// ѹ����ʽ�� 
    /// </summary> 
    public enum ConvertType
    {
        /// <summary> 
        /// GZip ѹ����ʽ 
        /// </summary> 
        GZip,

        /// <summary> 
        /// BZip2 ѹ����ʽ 
        /// </summary> 
        BZip2,

        /// <summary> 
        /// Zip ѹ����ʽ 
        /// </summary> 
        Zip
    }

    /// <summary> 
    /// ʹ�� SharpZipLib ����ѹ���ĸ����࣬�򻯶��ֽ�������ַ�������ѹ���Ĳ����� 
    /// </summary> 
    public class ConvertZip
    {
        /// <summary> 
        /// ѹ����Ӧ�ߣ�Ĭ��Ϊ GZip�� 
        /// </summary> 
        public static ConvertType CompressionProvider = ConvertType.GZip;

        /// <summary>
        /// ��base64String ת������DataTable���� 
        /// </summary>
        /// <param name="binaryData">�ֽ�����</param>
        /// <returns>DataTable����</returns>
        public static DataTable RetrieveDataTable(string base64String)
        {
            byte[] binaryData = Convert.FromBase64String(base64String);
            MemoryStream memStream = new MemoryStream(binaryData);
            XmlSerializer mySerializer = new XmlSerializer(typeof(System.String));
            Object obj = mySerializer.Deserialize(memStream);
            return (DataTable)obj;
        }

        public static byte[] GetBinaryFormatDataCompress(object obj)
        {
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            using (System.IO.MemoryStream mem = new MemoryStream())
            {
                System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(mem, Encoding.UTF8);
                ser.Serialize(writer, obj);
                writer.Close();
                return ConvertZip.Compress(mem.ToArray());
            }
        }

        /// <summary> 
        /// ��ԭʼ�ַ���������ѹ�����ַ����� 
        /// </summary> 
        /// <param name="stringToCompress">ԭʼ�ַ�����</param> 
        /// <returns>������ѹ�����ַ�����</returns> 
        public static string Compress(string stringToCompress)
        {
            byte[] compressedData = CompressToByte(stringToCompress);
            string strOut = Convert.ToBase64String(compressedData);
            return strOut;
        }

        #region Public methods
        /// <summary> 
        /// ��ԭʼ�ֽ�����������ѹ�����ֽ����顣 
        /// </summary> 
        /// <param name="bytesToCompress">ԭʼ�ֽ����顣</param> 
        /// <returns>������ѹ�����ֽ�����</returns> 
        public static byte[] Compress(byte[] bytesToCompress)
        {
            MemoryStream ms = new MemoryStream();
            Stream s = OutputStream(ms);
            s.Write(bytesToCompress, 0, bytesToCompress.Length);
            s.Flush();
            s.Close();
            return ms.ToArray();
        }


        public static byte[] GetBinaryFormatDataCompress(DataSet obj)
        {
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            using (System.IO.MemoryStream mem = new MemoryStream())
            {
                System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(mem, Encoding.UTF8);
                ser.Serialize(writer, obj);
                writer.Close();

                return ConvertZip.Compress(mem.ToArray());

                //byte[] d0 = mem.ToArray();
                //byte[] d1 = ConvertZip.Compress(d0);
                //byte[] d2 = ConvertZip.DeCompress(d1);
                //return d2;
            }
        }

        ///// <summary> 
        ///// ��ԭʼ�ַ���������ѹ�����ַ����� 
        ///// </summary> 
        ///// <param name="stringToCompress">ԭʼ�ַ�����</param> 
        ///// <returns>������ѹ�����ַ�����</returns> 
        //public static string Compress(string stringToCompress)
        //{
        //    byte[] compressedData = CompressToByte(stringToCompress);
        //    string strOut = Convert.ToBase64String(compressedData);
        //    return strOut;
        //}

        /// <summary> 
        /// ��ԭʼ�ַ���������ѹ�����ֽ����顣 
        /// </summary> 
        /// <param name="stringToCompress">ԭʼ�ַ�����</param> 
        /// <returns>������ѹ�����ֽ����顣</returns> 
        public static byte[] CompressToByte(string stringToCompress)
        {
            byte[] bytData = Encoding.Unicode.GetBytes(stringToCompress);
            return Compress(bytData);
        }

        /// <summary> 
        /// ����ѹ�����ַ�������ԭʼ�ַ����� 
        /// </summary> 
        /// <param name="stringToDecompress">��ѹ�����ַ�����</param> 
        /// <returns>����ԭʼ�ַ�����</returns> 
        public static string DeCompress(string stringToDecompress)
        {
            string outString = string.Empty;
            if (stringToDecompress == null)
            {
                throw new ArgumentNullException("stringToDecompress", "You tried to use an empty string");
            }

            try
            {
                byte[] inArr = Convert.FromBase64String(stringToDecompress.Trim());
                outString = Encoding.Unicode.GetString(DeCompress(inArr), 0, (DeCompress(inArr)).Length);
            }
            catch (NullReferenceException nEx)
            {
                return nEx.Message;
            }

            return outString;
        }

        /// <summary> 
        /// ����ѹ�����ֽ���������ԭʼ�ֽ����顣 
        /// </summary> 
        /// <param name="bytesToDecompress">��ѹ�����ֽ����顣</param> 
        /// <returns>����ԭʼ�ֽ����顣</returns> 
        public static byte[] DeCompress(byte[] bytesToDecompress)
        {
            byte[] writeData = new byte[4096];
            Stream s2 = InputStream(new MemoryStream(bytesToDecompress));
            MemoryStream outStream = new MemoryStream();

            while (true)
            {
                int size = s2.Read(writeData, 0, writeData.Length);
                if (size > 0)
                {
                    outStream.Write(writeData, 0, size);
                }
                else
                {
                    break;
                }
            }
            s2.Close();
            byte[] outArr = outStream.ToArray();
            outStream.Close();
            return outArr;
        }

        /// <summary>
        /// ��XMLת��ΪDataSet
        /// </summary>
        /// <param name="xmlData"></param>
        /// <returns></returns>
        public static DataSet ConvertXMLToDataSet(string xmlData)
        {
            StringReader stream = null;
            XmlTextReader reader = null;
            try
            {
                DataSet xmlDS = new DataSet();
                stream = new StringReader(xmlData);
                //��streamװ�ص�XmlTextReader
                reader = new XmlTextReader(stream);
                xmlDS.ReadXml(reader);
                return xmlDS;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (reader != null) reader.Close();
            }
        }

        /// <summary>
        /// ��ѹ��zip�ļ�
        /// </summary>
        /// <param name="zippedFile">zip�ļ�·��</param>
        /// <param name="unZippedPath">��ѹ���ļ���</param>
        public static bool UnZip(string zippedFile, string unZippedPath)
        {
            try
            {
                ZipInputStream s = new ZipInputStream(File.OpenRead(zippedFile));

                if (!Directory.Exists(unZippedPath))
                    Directory.CreateDirectory(unZippedPath);

                ZipEntry theentry = null;
                while ((theentry = s.GetNextEntry()) != null)
                {
                    string file = Path.GetFileName(theentry.Name);
                    if (file != string.Empty)
                    {
                        //��ѹ�ļ���ָ����Ŀ¼ 
                        using (FileStream streamwriter = File.Create(Path.Combine(unZippedPath, "Patch.CAB")))
                        {
                            int size = 2048;
                            byte[] data = new byte[2048];

                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                    streamwriter.Write(data, 0, size);
                                else
                                    break;
                            }
                        }
                    }
                }
                s.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Private methods

        /// <summary> 
        /// �Ӹ�����������ѹ��������� 
        /// </summary> 
        /// <param name="inputStream">ԭʼ����</param> 
        /// <returns>����ѹ���������</returns> 
        private static Stream OutputStream(Stream inputStream)
        {
            switch (CompressionProvider)
            {
                case ConvertType.BZip2:
                    return new BZip2OutputStream(inputStream);

                case ConvertType.GZip:
                    return new GZipOutputStream(inputStream);

                case ConvertType.Zip:
                    return new ZipOutputStream(inputStream);

                default:
                    return new GZipOutputStream(inputStream);
            }
        }

        /// <summary> 
        /// �Ӹ�����������ѹ���������� 
        /// </summary> 
        /// <param name="inputStream">ԭʼ����</param> 
        /// <returns>����ѹ����������</returns> 
        private static Stream InputStream(Stream inputStream)
        {
            switch (CompressionProvider)
            {
                case ConvertType.BZip2:
                    return new BZip2InputStream(inputStream);

                case ConvertType.GZip:
                    return new GZipInputStream(inputStream);

                case ConvertType.Zip:
                    return new ZipInputStream(inputStream);

                default:
                    return new GZipInputStream(inputStream);
            }
        }

        #endregion

    }
 }
