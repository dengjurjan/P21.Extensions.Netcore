using System.Collections;
using System.Text;

namespace P21.Extensions.DataAccess;

public class P21Encryption
{
    public static string Decrypt(string encryptedPW, string key)
    {
        var str1 = "";
        if (!string.IsNullOrEmpty(key))
        {
            key = ScrambleKey(key);
            var num = 0;

            _ = key.Length;

            _ = encryptedPW.Length;
            var encoding = Encoding.GetEncoding(1252);
            Encoding unicode = Encoding.Unicode;
            var bytes1 = unicode.GetBytes(encryptedPW);
            var bytes2 = Encoding.Convert(unicode, encoding, bytes1);

            _ = new char[encoding.GetCharCount(bytes2)];

            _ = new string(encoding.GetChars(bytes2));
            var bytes3 = unicode.GetBytes(key);
            var bytes4 = Encoding.Convert(unicode, encoding, bytes3);

            _ = new char[encoding.GetCharCount(bytes4)];

            _ = new string(encoding.GetChars(bytes4));
            var length3 = bytes2.Length;
            var length4 = bytes4.Length;
            for (var index = 0; index < length3; ++index)
            {
                if (int.TryParse(bytes2.GetValue(length3 - (length3 - index)).ToString(), out var result1) && int.TryParse(bytes4.GetValue(length4 - (length4 - num)).ToString(), out var result2))
                {
                    result1 -= result2;
                    while (result1 < 0)
                    {
                        result1 += byte.MaxValue;
                    }

                    str1 += ((char)result1).ToString();
                    ++num;
                    if (num > key.Length - 1)
                    {
                        num = 0;
                    }
                }
            }
        }
        return str1;
    }

    public static string Encrypt(string valueString, string key)
    {
        var str = "";
        var arrayList = new ArrayList();
        if (!string.IsNullOrEmpty(key))
        {
            key = ScrambleKey(key);
            var num = 0;
            var encoding = Encoding.GetEncoding(1252);
            Encoding unicode = Encoding.Unicode;
            var bytes1 = unicode.GetBytes(valueString);
            var numArray1 = Encoding.Convert(unicode, encoding, bytes1);
            var bytes2 = unicode.GetBytes(key);
            var numArray2 = Encoding.Convert(unicode, encoding, bytes2);
            var length1 = numArray1.Length;
            var length2 = numArray2.Length;
            for (var index = 0; index < length1; ++index)
            {
                if (int.TryParse(numArray1.GetValue(length1 - (length1 - index)).ToString(), out var result1) && int.TryParse(numArray2.GetValue(length2 - (length2 - num)).ToString(), out var result2))
                {
                    result1 += result2;
                    while (result1 > byte.MaxValue)
                    {
                        if (result1 > byte.MaxValue)
                        {
                            result1 -= byte.MaxValue;
                        }
                    }
                    str += ((char)result1).ToString();
                    _ = arrayList.Add((byte)result1);
                    ++num;
                    if (num > key.Length - 1)
                    {
                        num = 0;
                    }
                }
            }
            var bytes3 = new byte[arrayList.Count];
            arrayList.CopyTo(bytes3);
            str = Encoding.GetEncoding(1252).GetString(bytes3);
        }
        return str;
    }

    private static string ScrambleKey(string key)
    {
        var str1 = "";
        var charArray = key.ToCharArray();
        Array.Reverse(charArray);
        var str2 = new string(charArray);
        var length = key.Length;
        for (var startIndex = 0; startIndex < length; ++startIndex)
        {
            switch (startIndex)
            {
                case 0:
                    str1 = string.Concat("A", str2.AsSpan(startIndex, 1));
                    break;
                case 1:
                    str1 = $"{str1}3{str2.Substring(startIndex, 1)}";
                    break;
                case 2:
                    str1 = $"{str1}k{str2.Substring(startIndex, 1)}";
                    break;
                case 3:
                    str1 = $"{str1}o{str2.Substring(startIndex, 1)}";
                    break;
                case 4:
                    str1 = $"{str1}&{str2.Substring(startIndex, 1)}";
                    break;
                case 5:
                    str1 = $"{str1}1{str2.Substring(startIndex, 1)}";
                    break;
                case 6:
                    str1 = $"{str1}%{str2.Substring(startIndex, 1)}";
                    break;
                case 7:
                    str1 = $"{str1}M{str2.Substring(startIndex, 1)}";
                    break;
                case 8:
                    str1 = $"{str1}v{str2.Substring(startIndex, 1)}";
                    break;
                case 9:
                    str1 = $"{str1}Z{str2.Substring(startIndex, 1)}";
                    break;
            }
        }
        return str1;
    }
}
