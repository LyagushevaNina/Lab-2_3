#pragma warning disable CA1861
#pragma warning disable CS8604
using System.Collections;
using System.Text;
internal static class Program
{
    [STAThread]
    internal static void Main()
    {
        BitArray primary = new(new bool[] { true, false, true, false, false, false, true, true });

        BitArray polynomial = new(new bool[] { true, false, true, false, false, false, true, true });

        LFSR lfsr = new();
        lfsr.Init(primary, polynomial);

        while (true)
        {
            Console.WriteLine($"0. Exit");
            Console.WriteLine($"1. Encrypt/Decrypt");
            Console.WriteLine($"{nameof(primary)} = {BitArrayToString(primary)}");
            Console.WriteLine($"{nameof(polynomial)} = {BitArrayToString(polynomial)}");

            int input = GetInt($"{nameof(input)} In Range (0-1): ");

            if (input == 0)
            {
                break;
            }

            else if (input == 1)
            {
                using OpenFileDialog openFileDialog = new()
                {
                    Filter = "Text files (*.txt)|*.txt",
                    Title = "Choose a file to decrypt"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string inputFile = openFileDialog.FileName;
                    string outputFile = Path.Combine(Path.GetDirectoryName(inputFile), Path.GetFileNameWithoutExtension(inputFile) + "_mod" + Path.GetExtension(inputFile));
                    byte[] data = File.ReadAllBytes(inputFile);
                    byte[] handleData = HandleData(data, lfsr);
                    File.WriteAllBytes(outputFile, handleData);
                    Console.WriteLine("File encrypted/decrypted");
                }
            }

            else
            {
                Console.WriteLine("Unknown Input");
            }
        }
    }
    // Генерация гаммы
    private static byte[] HandleData(byte[] data, LFSR lfsr)
    {
        byte[] keyStream = Key(lfsr, data.Length);
        byte[] handleData = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            handleData[i] = (byte)(data[i] ^ keyStream[i]);
        }

        return handleData;
    }
    private static byte[] Key(LFSR lfsr, int length)
    {
        byte[] streamKey = new byte[length];
        BitArray currentState = new(lfsr.Primary);

        for (int i = 0; i < length; i++)
        {
            currentState = lfsr.NextState(currentState);
            byte byteKey = BitArrayToByte(currentState);
            streamKey[i] = byteKey;
        }

        return streamKey;
    }
    //Вспомогательные функции
    private static byte BitArrayToByte(BitArray bits)
    {
        if (bits.Length > 8)
        {
            throw new ArgumentException("BitArray length must be at most 8 bits.");
        }

        byte[] bytes = new byte[1];
        bits.CopyTo(bytes, 0);
        return bytes[0];
    }
    internal static string BitArrayToString(BitArray array)
    {
        StringBuilder sb = new();
        for (int i = 0; i < array.Length; i++)
        {
            _ = sb.Append(array[i] ? "1" : "0");
        }

        return sb.ToString();
    }
    internal static int GetInt(string message, int defaultValue = 0)
    {
        if (!string.IsNullOrEmpty(message))
        {
            Console.WriteLine(message);
        }

        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out int iValue))
            {
                return iValue;
            }

            if (defaultValue != 0)
            {
                return defaultValue;
            }

            Console.WriteLine("Parsing Error. Enter Other Value. Format Int32: ");
        }
    }
}
// Класс реализующий регистр сдвига с линейной обратной связью
internal sealed class LFSR
{
    internal BitArray Polynomial { get; set; }
    internal BitArray Primary { get; set; }
    internal void Init(BitArray _primary, BitArray _polynomial)
    {
        Polynomial = _polynomial;
        Primary = _primary;
    }

    // Метод для вычисления следующего состояния регистра
    internal BitArray NextState(BitArray currentState)
    {
        BitArray nextState = new(currentState.Length);

        BitArray xoredBits = XOR(currentState, Polynomial);
        nextState[0] = xoredBits[0];

        for (int i = 1; i < currentState.Length; i++)
        {
            nextState[i] = currentState[i - 1];
        }

        return nextState;
    }

    // Метод XOR 
    private static BitArray XOR(BitArray baseBits, BitArray polynomial)
    {
        BitArray result = new(polynomial.Length);
        bool? bit = null;

        for (int j = 0; j < polynomial.Length; j++)
        {
            if (polynomial[j])
            {
                if (bit == null)
                {
                    bit = baseBits[j];
                }
                else
                {
                    bool currentBitValue = baseBits[j];
                    bit ^= currentBitValue;
                }
            }
        }

        result[0] = (bit & true) == true;
        return result;
    }
}
