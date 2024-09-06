using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace ActivosFijos.Clases
{
    public class clCipherUtility
    {
        private const string Key_IC = "F735F6DDDBB1F0784403180DBF74FF3697F030D564B23AC18DAF0D4CB77AFF1C715CB1F11274B8824FFDD69C5D71C3E23B1238757FE2F52C1814642856B933C7";
        private const string Salto_IC = "F735F6DDDBB1F0784403180DBF74FF3697F030D564B23AC18DAF0D4CB77";
        private string path = Path.GetFullPath("../Bin/CifradoLibreria.dll");//MapPath("../Bin/CifradoLibreria.dll");
        delegate string CifradoLibreria([MarshalAs(UnmanagedType.LPStr)] string a);

        public static string Cifradolibreria(string value)
        {
            string resultado = null;
            IntPtr userApi1 = clNativeMethods.LoadLibrary("user32.dll");
            if (userApi1 == IntPtr.Zero)
                return "";
            try
            {
                IntPtr CifradoLibreriaAddress = clNativeMethods.GetProcAddress(userApi1, "DesCifrar1");
                if ((int)CifradoLibreriaAddress == 0)
                {
                    clNativeMethods.FreeLibrary(userApi1);
                    return "";
                }
                CifradoLibreria cifrar = (CifradoLibreria)Marshal.GetDelegateForFunctionPointer(CifradoLibreriaAddress, typeof(CifradoLibreria));
                resultado = cifrar(value);
            }
            finally
            {
                bool result = clNativeMethods.FreeLibrary(userApi1);
            }
            return resultado;
        }
        public static string Encript(string cad)
        {
            string CadAux;
            if (!String.IsNullOrEmpty(cad))
            {
                char[] charCar;
                byte[] bytAscii;
                Encoding Ascii = Encoding.Default;
                byte K = Convert.ToByte(cad.Length);
                CadAux = "";
                for (byte i = 0; i < K; i++)
                {
                    bytAscii = Ascii.GetBytes(cad[i].ToString());
                    bytAscii[0] = (byte)(bytAscii[0] + Convert.ToByte(2) * (i + 1));
                    charCar = Ascii.GetChars(bytAscii);
                    CadAux += charCar[0];
                }
                return CadAux;
            }
            else
                return "";
        }
        public static string UnEncript(string cad)
        {
            string CadAux;
            if (!String.IsNullOrEmpty(cad))
            {
                char[] charCar;
                byte[] bytAscii;
                Encoding Ascii = Encoding.Default;
                byte K = Convert.ToByte(cad.Length);
                CadAux = "";
                for (byte i = 0; i < K; i++)
                {
                    bytAscii = Ascii.GetBytes(cad[i].ToString());
                    bytAscii[0] = (byte)(bytAscii[0] - Convert.ToByte(2) * (i + 1));
                    charCar = Ascii.GetChars(bytAscii);
                    CadAux += charCar[0];
                }
                return CadAux;
            }
            else
                return "";
        }
        public static string Cifrar2(string value)
        {
            return Encrypt<AesManaged>(value, GetKey(), GetSalt());
        }
        public static string DesCifrar2(string value)
        {
            return Decrypt<AesManaged>(value, GetKey(), GetSalt());

        }
        public static string Encrypt<T>(string value, string password, string salt)
             where T : SymmetricAlgorithm, new()
        {
            DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));
            SymmetricAlgorithm algorithm = new T();
            byte[] rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            byte[] rgbIV = rgb.GetBytes(algorithm.BlockSize >> 3);
            ICryptoTransform transform = algorithm.CreateEncryptor(rgbKey, rgbIV);
            using (MemoryStream buffer = new MemoryStream())
            {
                using (CryptoStream stream = new CryptoStream(buffer, transform, CryptoStreamMode.Write))
                {
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.Unicode))
                    {
                        writer.Write(value);
                    }
                }
                return Convert.ToBase64String(buffer.ToArray());
            }
        }
        public static string Decrypt<T>(string text, string password, string salt)
           where T : SymmetricAlgorithm, new()
        {
            DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));
            SymmetricAlgorithm algorithm = new T();
            byte[] rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            byte[] rgbIV = rgb.GetBytes(algorithm.BlockSize >> 3);
            ICryptoTransform transform = algorithm.CreateDecryptor(rgbKey, rgbIV);
            using (MemoryStream buffer = new MemoryStream(Convert.FromBase64String(text)))
            {
                using (CryptoStream stream = new CryptoStream(buffer, transform, CryptoStreamMode.Read))
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.Unicode))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
        public static string GetKey()
        {
            return Key_IC;
        }
        public static string GetSalt()
        {
            return Salto_IC;
        }
    }
}
