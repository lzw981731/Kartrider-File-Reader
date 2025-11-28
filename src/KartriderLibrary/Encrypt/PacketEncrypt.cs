namespace KartLibrary.Encrypt;

public static class PacketEncrypt
    {
        public static int GetKey(int vec1, int vec2)
        {
            return (int)(vec1 ^ vec2);
        }

        public static unsafe int Encrypt(byte[] dest,byte[] source, int key)
        {
            int[] keys = new int[]
            {
                (int)(key ^ 0x14B307C8),
                (int)(key ^ 0x8CBF12AC),
                (int)(key ^ 0x240397C1),
                (int)(key ^ 0xF3BD29C0)
            };
            int checkCode = 0;
            int len = source.Length >> 4;
            fixed (byte* inPtr = source, outPtr = dest)
            {
                int* intInPtr = (int*)inPtr;
                int* intOutPtr = (int*)outPtr;
                for (int i = 0; i < len; i++)
                {
                    checkCode ^= intInPtr[(i << 2)];
                    checkCode ^= intInPtr[(i << 2) + 1];
                    checkCode ^= intInPtr[(i << 2) + 2];
                    checkCode ^= intInPtr[(i << 2) + 3];
                    intOutPtr[(i << 2)] = intInPtr[(i << 2)] ^ keys[0];
                    intOutPtr[(i << 2) + 1] = intInPtr[(i << 2) + 1] ^ keys[1];
                    intOutPtr[(i << 2) + 2] = intInPtr[(i << 2) + 2] ^ keys[2];
                    intOutPtr[(i << 2) + 3] = intInPtr[(i << 2) + 3] ^ keys[3];
                }
            }
            int startIndex = (len << 4);
            for (int i = startIndex; i < source.Length; i++)
            {
                int curKey = keys[(i >> 2) & 3];
                checkCode ^= source[i] << (i & 15);
                dest[i] = (byte)(source[i] ^ ((curKey >> ((i & 3) << 3)) & 0xFF));
            }
            return checkCode;
        }

        public static unsafe int Decrypt(byte[] dest,byte[] source, int key)
        {
            int[] keys = new int[]
            {
                (int)(key ^ 0x14B307C8),
                (int)(key ^ 0x8CBF12AC),
                (int)(key ^ 0x240397C1),
                (int)(key ^ 0xF3BD29C0)
            };
            int checkCode = 0;
            int len = source.Length >> 4;
            fixed (byte* inPtr = source, outPtr = dest)
            {
                int* int_inPtr = (int*)inPtr;
                int* int_outPtr = (int*)outPtr;
                for (int i = 0; i < len; i++)
                {
                    int_outPtr[(i << 2)] = int_inPtr[(i << 2)] ^ keys[0];
                    int_outPtr[(i << 2) + 1] = int_inPtr[(i << 2) + 1] ^ keys[1];
                    int_outPtr[(i << 2) + 2] = int_inPtr[(i << 2) + 2] ^ keys[2];
                    int_outPtr[(i << 2) + 3] = int_inPtr[(i << 2) + 3] ^ keys[3];
                    checkCode ^= int_outPtr[(i << 2)];
                    checkCode ^= int_outPtr[(i << 2) + 1];
                    checkCode ^= int_outPtr[(i << 2) + 2];
                    checkCode ^= int_outPtr[(i << 2) + 3];
                }
            }
            int startIndex = (len << 4);
            for (int i = startIndex; i < source.Length; i++)
            {
                int curKey = keys[(i >> 2) & 3];
                dest[i] = (byte)(source[i] ^ ((curKey >> ((i & 3) << 3)) & 0xFF));
                checkCode ^= dest[i] << (i & 15);
            }
            return checkCode;
        }

        public static unsafe int Encrypt(int key, byte[] dest, int destIndex, byte[] source, int sourceIndex, int length)
        {
            if (destIndex < 0 || sourceIndex < 0 || destIndex + length > dest.Length ||
                sourceIndex + length > source.Length)
                throw new IndexOutOfRangeException("given index is out of range.");
            int[] keys = new int[]
            {
                (int)(key ^ 0x14B307C8),
                (int)(key ^ 0x8CBF12AC),
                (int)(key ^ 0x240397C1),
                (int)(key ^ 0xF3BD29C0)
            };
            int checkCode = 0;
            int len = length >> 4;
            fixed (byte* inPtr = &source[sourceIndex], outPtr = &dest[destIndex])
            {
                int* intInPtr = (int*)inPtr;
                int* intOutPtr = (int*)outPtr;
                for (int i = 0; i < len; i++)
                {
                    checkCode ^= intInPtr[(i << 2)];
                    checkCode ^= intInPtr[(i << 2) + 1];
                    checkCode ^= intInPtr[(i << 2) + 2];
                    checkCode ^= intInPtr[(i << 2) + 3];
                    intOutPtr[(i << 2)] = intInPtr[(i << 2)] ^ keys[0];
                    intOutPtr[(i << 2) + 1] = intInPtr[(i << 2) + 1] ^ keys[1];
                    intOutPtr[(i << 2) + 2] = intInPtr[(i << 2) + 2] ^ keys[2];
                    intOutPtr[(i << 2) + 3] = intInPtr[(i << 2) + 3] ^ keys[3];
                }
            }
            int startIndex = (len << 4);
            for (int i = startIndex; i < length; i++)
            {
                int curKey = keys[(i >> 2) & 3];
                checkCode ^= source[sourceIndex + i] << (i & 15);
                dest[destIndex + i] = (byte)(source[destIndex + i] ^ ((curKey >> ((i & 3) << 3)) & 0xFF));
            }
            return checkCode;
        }

        public static unsafe int Decrypt(int key, byte[] dest, int destIndex, byte[] source, int sourceIndex, int length)
        {
            if (destIndex < 0 || sourceIndex < 0 || destIndex + length > dest.Length ||
                sourceIndex + length > source.Length)
                throw new IndexOutOfRangeException("given index is out of range.");
            int[] keys = new int[]
            {
                (int)(key ^ 0x14B307C8),
                (int)(key ^ 0x8CBF12AC),
                (int)(key ^ 0x240397C1),
                (int)(key ^ 0xF3BD29C0)
            };
            int checkCode = 0;
            int len = length >> 4;
            fixed (byte* inPtr = &source[sourceIndex], outPtr = &dest[destIndex])
            {
                int* intInPtr = (int*)inPtr;
                int* intOutPtr = (int*)outPtr;
                for (int i = 0; i < len; i++)
                {
                    intOutPtr[(i << 2)] = intInPtr[(i << 2)] ^ keys[0];
                    intOutPtr[(i << 2) + 1] = intInPtr[(i << 2) + 1] ^ keys[1];
                    intOutPtr[(i << 2) + 2] = intInPtr[(i << 2) + 2] ^ keys[2];
                    intOutPtr[(i << 2) + 3] = intInPtr[(i << 2) + 3] ^ keys[3];
                    checkCode ^= intOutPtr[(i << 2)];
                    checkCode ^= intOutPtr[(i << 2) + 1];
                    checkCode ^= intOutPtr[(i << 2) + 2];
                    checkCode ^= intOutPtr[(i << 2) + 3];
                }
            }
            int startIndex = (len << 4);
            for (int i = startIndex; i < length; i++)
            {
                int curKey = keys[(i >> 2) & 3];
                dest[i + destIndex] = (byte)(source[i + sourceIndex] ^ ((curKey >> ((i & 3) << 3)) & 0xFF));
                checkCode ^= dest[i + destIndex] << (i & 15);
            }
            return checkCode;
        }

        
        public static int EncryptPacketLen(int EncryptedSize, int Key)
        {
            return unchecked((int)0xF834A608) ^ EncryptedSize ^ Key;
        }

        public static int DecryptPacketLen(int DecryptedSize, int Key)
        {
            return EncryptPacketLen(DecryptedSize, Key);
        }
    }