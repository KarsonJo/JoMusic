using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MusicCrawler.Netease
{
    public static class NeteaseEncrypt
    {
        static string AesKey { get; }
        static string AesIV { get; }
        static string RsaExponent { get; }
        static string RsaModulus { get; }

        static NeteaseEncrypt()
        {
            //这些规则都是从网易云音乐的网页分析出来的
            
            Dictionary<string, string> allCodes = new()
            {
                {"色", "00e0b"  },
                {"流感", "509f6"},
                {"这边", "259df"},
                {"弱", "8642d"  },
                {"嘴唇", "bc356"},
                {"亲", "62901"  },
                {"开心", "477df"},
                {"呲牙", "22677"},
                {"憨笑", "ec152"},
                {"猫", "b5ff6"  },
                {"皱眉", "8ace6"},
                {"幽灵", "15bb7"},
                {"蛋糕", "b7251"},
                {"发怒", "52b3a"},
                {"大哭", "b17a8"},
                {"兔子", "76aea"},
                {"星星", "8a5aa"},
                {"钟情", "76d2e"},
                {"牵手", "41762"},
                {"公鸡", "9ec4e"},
                {"爱意", "e341f"},
                {"禁止", "56135"},
                {"狗", "fccf6"  },
                {"亲亲", "95280"},
                {"叉", "104e0"  },
                {"礼物", "312ec"},
                {"晕", "bda92"  },
                {"呆", "557c9"  },
                {"生病", "38701"},
                {"钻石", "14af6"},
                {"拜", "c9d05"  },
                {"怒", "c4f7f"  },
                {"示爱", "0c368"},
                {"汗", "5b7a4"  },
                {"小鸡", "6bee2"},
                {"痛苦", "55932"},
                {"撇嘴", "575cc"},
                {"惶恐", "e10b4"},
                {"口罩", "24d81"},
                {"吐舌", "3cfe4"},
                {"心碎", "875d3"},
                {"生气", "e8204"},
                {"可爱", "7b97d"},
                {"鬼脸", "def52"},
                {"跳舞", "741d5"},
                {"男孩", "46b8e"},
                {"奸笑", "289dc"},
                {"猪", "6935b"  },
                {"圈", "3ece0"  },
                {"便便", "462db"},
                {"外星", "0a22b"},
                {"圣诞", "8e7"  },
                {"流泪", "01000"},
                {"强", "1"      },
                {"爱心", "0CoJU"},
                {"女孩", "m6Qyw"},
                {"惊恐", "8W8ju"},
                {"大笑", "d"}
            };
            string[] usingCharacters1 = new string[] { "流泪", "强" };
            string[] usingCharacters2 = new string[] { "色", "流感", "这边", "弱", "嘴唇", "亲", "开心", "呲牙", "憨笑", "猫", "皱眉", "幽灵", "蛋糕", "发怒", "大哭", "兔子", "星星", "钟情", "牵手", "公鸡", "爱意", "禁止", "狗", "亲亲", "叉", "礼物", "晕", "呆", "生病", "钻石", "拜", "怒", "示爱", "汗", "小鸡", "痛苦", "撇嘴", "惶恐", "口罩", "吐舌", "心碎", "生气", "可爱", "鬼脸", "跳舞", "男孩", "奸笑", "猪", "圈", "便便", "外星", "圣诞" };
            string[] usingCharacters3 = new string[] { "爱心", "女孩", "惊恐", "大笑" };

            string KeyStringFromEmoji(string[] select)
            {
                string key = "";
                foreach (var item in select)
                {
                    key += allCodes[item];
                }
                return key;
            }

            //以下所有操作，是一堆无意义的加密和格式转换，都是网易云恶心我们的操作（
            AesKey = KeyStringFromEmoji(usingCharacters3);
            AesIV = "0102030405060708";
            RsaExponent = KeyStringFromEmoji(usingCharacters1);
            RsaModulus = KeyStringFromEmoji(usingCharacters2);
        }

        public static string Netease163KeyEncrypt(string plainData)
        {
            return NeteaseAesEncrypt(plainData, "#14ljk_!\\]&0U<'(", null, CipherMode.ECB);
        }

        public static KeyValuePair<string, string>[] ParamsEncrypt(Dictionary<string, object> ParamPairs)
        {
            string jsonParams = JsonConvert.SerializeObject(ParamPairs, Formatting.None);
            //jsonParams = "{\"ids\": \"['4877040']\", \"level\": \"standard\", \"encodeType\": \"aac\", \"csrf_token\": \"\"}";
            Console.WriteLine(jsonParams);
            string encryptedParams = NeteaseAesEncrypt(jsonParams, AesKey, AesIV);
            string aesRandomKey = GetAesRandomKey();
            encryptedParams = NeteaseAesEncrypt(encryptedParams, aesRandomKey, AesIV);
            string encSecKey = NeteaseRsaEncrypt(aesRandomKey, RsaExponent, RsaModulus);

            //var result = JsonConvert.SerializeObject(new Dictionary<string, string>()
            //{
            //    { "params", encryptedParams },
            //    { "encSecKey", encSecKey }
            //});


            var result = new[]
            {
                new KeyValuePair<string, string>("params", encryptedParams),
                new KeyValuePair<string, string>("encSecKey", encSecKey)
            };

            //Console.WriteLine(result);
            return result;
        }

        /// <summary>
        /// 针对网易云音乐的野鸡加密
        /// 传入视为"十六进制字符串"的数据
        /// </summary>
        /// <param name="plainData"></param>
        /// <param name="exponent"></param>
        /// <param name="modulus"></param>
        /// <returns></returns>
        public static string NeteaseRsaEncrypt(string plainData, string exponent, string modulus)
        {
            //Aes中，司马网易云把字符串的编码直接看十六进制字符串

            static string Reverse(string s)
            {
                char[] charArray = s.ToCharArray();
                Array.Reverse(charArray);
                return new string(charArray);
            }

            static BigInteger ParseBigIntFromHexString(string scource)
            {
                return BigInteger.Parse("0" + scource, System.Globalization.NumberStyles.HexNumber);
            }


            //针对原文还特别加工恶心了一下
            var d = ParseBigIntFromHexString(Convert.ToHexString(Encoding.UTF8.GetBytes(Reverse(plainData))));
            var e = ParseBigIntFromHexString(exponent);
            var n = ParseBigIntFromHexString(modulus);

            //直接用最土的方法，算踏马的
            var bigIntResult = BigInteger.ModPow(d, e, n);

            //最后还不忘把数字转换回十六进制字符串
            return bigIntResult.ToString("x").TrimStart('0');
        }

        public static string NeteaseAesEncrypt(string plainData, string stringKey, string stringIv, CipherMode mode = CipherMode.CBC)
        {
            //Aes中，司马网易云把字符串的编码直接看做key
            byte[] key = Encoding.UTF8.GetBytes(stringKey);
            byte[] iv = stringIv != null? Encoding.UTF8.GetBytes(stringIv) : null;

            using AesCryptoServiceProvider aesAlg = new();
            //aesAlg.Key = key;
            //aesAlg.IV = iv;
            aesAlg.Mode = mode;
            //aesAlg.Padding = PaddingMode.PKCS7;
            //aesAlg.BlockSize = 16*8;
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(key, iv);
            using MemoryStream msEncrypt = new();
            using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (StreamWriter swEncrypt = new(csEncrypt))
            {
                swEncrypt.Write(plainData);
            }
            byte[] bytes = msEncrypt.ToArray();
            return Convert.ToBase64String(bytes);
        }
        static string GetAesRandomKey()
        {
            var character = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string strKey = "";
            Random rng = new();
            for (int i = 0; i < 16; i++)
            {
                strKey += character[rng.Next(character.Length)];
            }
            return strKey;
        }
    }
}
