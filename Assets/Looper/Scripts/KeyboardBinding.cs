using System.Collections.Generic;

public static class KeyboardBinding
{
    private static readonly Dictionary<char, int> keyboardCodeMap = new Dictionary<char, int>()
    {
        {'z', 50},
        {'x', 51},
        {'c', 52},
        {'v', 53},
        {'b', 54},
        {'n', 55},
        {'m', 56},
        {',', 57},
        {'.', 58},
        {'/', 59},
        {'a', 60},
        {'s', 61},
        {'d', 62},
        {'f', 63},
        {'g', 64},
        {'h', 65},
        {'j', 66},
        {'k', 67},
        {'l', 68},
        {';', 69},
        {'\'', 70},
        {'#', 71},
        {'q', 72},
        {'w', 73},
        {'e', 74},
        {'r', 75},
        {'t', 76},
        {'y', 77},
        {'u', 78},
        {'i', 79},
        {'o', 80},
        {'p', 81},
        {'[', 82},
        {']', 83},

    };

    public static int GetKeyFromString(char key)
    {
        if (keyboardCodeMap.ContainsKey(key))
            return keyboardCodeMap[key];
        else
            return -1;
    }
}
