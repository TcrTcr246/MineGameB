namespace MineGameB.Misc;
public class TyconNumberConvertor {
    public static string Convert(long number, int decimals = 1) {
        if (number < 1000) {
            return number.ToString();
        }

        string[] suffixes = { "", "K", "M", "B", "T", "Q" };
        int suffixIndex = 0;
        double displayNumber = number;

        while (displayNumber >= 1000 && suffixIndex < suffixes.Length - 1) {
            displayNumber /= 1000;
            suffixIndex++;
        }

        string format = decimals > 0 ? $"F{decimals}" : "F0";
        return displayNumber.ToString(format) + suffixes[suffixIndex];
    }
}
