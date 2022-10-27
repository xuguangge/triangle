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
    /// 压缩方式。 
    /// </summary> 
    public enum ConvertType
    {
        /// <summary> 
        /// GZip 压缩格式 
        /// </summary> 
        GZip,

        /// <summary> 
        /// BZip2 压缩格式 
        /// </summary> 
        BZip2,

        /// <summary> 
        /// Zip 压缩格式 
        /// </summary> 
        Zip
    }

    /// <summary> 
    /// 使用 SharpZipLib 进行压缩的辅助类，简化对字节数组和字符串进行压缩的操作。 
    /// </summary> 
    public class ConvertZip
    {
        /// <summary> 
        /// 压缩供应者，默认为 GZip。 
        /// </summary> 
        public static ConvertType CompressionProvider = ConvertType.GZip;

        /// <summary>
        /// 将base64String 转化化成DataTable对象 
        /// </summary>
        /// <param name="binaryData">字节数组</param>
        /// <returns>DataTable对象</returns>
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
        /// 从原始字符串生成已压缩的字符串。 
        /// </summary> 
        /// <param name="stringToCompress">原始字符串。</param> 
        /// <returns>返回已压缩的字符串。</returns> 
        public static string Compress(string stringToCompress)
        {
            byte[] compressedData = CompressToByte(stringToCompress);
            string strOut = Convert.ToBase64String(compressedData);
            return strOut;
        }

        #region Public methods
        /// <summary> 
        /// 从原始字节数组生成已压缩的字节数组。 
        /// </summary> 
        /// <param name="bytesToCompress">原始字节数组。</param> 
        /// <returns>返回已压缩的字节数组</returns> 
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
        ///// 从原始字符串生成已压缩的字符串。 
        ///// </summary> 
        ///// <param name="stringToCompress">原始字符串。</param> 
        ///// <returns>返回已压缩的字符串。</returns> 
        //public static string Compress(string stringToCompress)
        //{
        //    byte[] compressedData = CompressToByte(stringToCompress);
        //    string strOut = Convert.ToBase64String(compressedData);
        //    return strOut;
        //}

        /// <summary> 
        /// 从原始字符串生成已压缩的字节数组。 
        /// </summary> 
        /// <param name="stringToCompress">原始字符串。</param> 
        /// <returns>返回已压缩的字节数组。</returns> 
        public static byte[] CompressToByte(string stringToCompress)
        {
            byte[] bytData = Encoding.Unicode.GetBytes(stringToCompress);
            return Compress(bytData);
        }

        /// <summary> 
        /// 从已压缩的字符串生成原始字符串。 
        /// </summary> 
        /// <param name="stringToDecompress">已压缩的字符串。</param> 
        /// <returns>返回原始字符串。</returns> 
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
        /// 从已压缩的字节数组生成原始字节数组。 
        /// </summary> 
        /// <param name="bytesToDecompress">已压缩的字节数组。</param> 
        /// <returns>返回原始字节数组。</returns> 
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
        /// 将XML转换为DataSet
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
                //从stream装载到XmlTextReader
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
        /// 解压缩zip文件
        /// </summary>
        /// <param name="zippedFile">zip文件路径</param>
        /// <param name="unZippedPath">解压缩文件夹</param>
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
                        //解压文件到指定的目录 
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
        /// 从给定的流生成压缩输出流。 
        /// </summary> 
        /// <param name="inputStream">原始流。</param> 
        /// <returns>返回压缩输出流。</returns> 
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
        /// 从给定的流生成压缩输入流。 
        /// </summary> 
        /// <param name="inputStream">原始流。</param> 
        /// <returns>返回压缩输入流。</returns> 
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
