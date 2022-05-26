namespace TetrisCS;

/// <summary> 블록 모양의 종류 </summary>
public enum BlockType { None, I, J, L, O, S, T, Z }

public static class BlockTypeExtentions
{
    public static ConsoleColor ToBlockColor(this BlockType type) => type switch
    {
        BlockType.None => ConsoleColor.Black,
        BlockType.I => ConsoleColor.Cyan,
        BlockType.J => ConsoleColor.Blue,
        BlockType.L => ConsoleColor.DarkYellow,
        BlockType.O => ConsoleColor.Yellow,
        BlockType.S => ConsoleColor.Green,
        BlockType.T => ConsoleColor.Magenta,
        BlockType.Z => ConsoleColor.Red,
        _ => ConsoleColor.White
    };
}
