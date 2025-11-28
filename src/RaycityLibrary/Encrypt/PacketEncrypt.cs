namespace RayCityLibrary.Encrypt;

public static class PacketEncrypt
    {
        public static int GetKey(long data1, long data2, byte shift)
        {
            return (int)((((data1 << shift) >>> 0x20) ^ ((data2 << shift) >>> 0x20) ^ 0xA815B623) & 0xFFFFFFFF);
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

        public static int EncryptPacketLen(int encryptedSize, int key)
        {
            return unchecked((int)0xA05F33BA) ^ encryptedSize ^ key;
        }

        public static int DecryptPacketLen(int decryptedSize, int key)
        {
            return EncryptPacketLen(decryptedSize, key);
        }
    }