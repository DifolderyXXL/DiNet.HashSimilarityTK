using DiNet.HashSimilarityTK.Infrastructure;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

const int step = 5;
const int hashCounts = 100;
const uint fixedSeed = 123456789;


var matchTable = new MatchSet<int>(
    shingleSize: 4,
    numHashes: 128,
    chunkStep: 16,
    seed: 123456789
);


var entries = new List<(long start, int length)>();

using var stream = new FileStream("TextSample1.txt", FileMode.Open, FileAccess.Read);
using var reader = new StreamReader(stream, Encoding.UTF8);

int chunkIndex = 0;
long totalCharsRead = 0;
var chunkTexts = new List<string>();

string? line;
while ((line = reader.ReadLine()) != null)
{
    string trimmed = line.Trim();

    // Фильтруем синтаксический шум (скобки, using, короткие строки < 20 символов)
    if (trimmed.Length < 20 ||
        trimmed is "{" or "}" or "};" ||
        trimmed.StartsWith("using "))
        continue;

    chunkTexts.Add(line);
    matchTable.Add(line.AsSpan(), chunkIndex++);

    entries.Add((totalCharsRead, line.Length));
    totalCharsRead += line.Length;
}

// 1. Собираем и объединяем пересекающиеся группы из банд LSH
var rawGroups = matchTable.EnumerateAllMatchings().Where(g => g.Count() > 1);
var mergedGroups = new List<HashSet<int>>();

foreach (var rawGroup in rawGroups)
{
    var set = rawGroup.ToHashSet();
    var existing = mergedGroups.Where(g => g.Overlaps(set)).ToList();

    if (existing.Count > 0)
    {
        foreach (var g in existing)
        {
            set.UnionWith(g);
            mergedGroups.Remove(g);
        }
    }
    mergedGroups.Add(set);
}

// 2. Превращаем mergedGroups в словарь chunkToGroups (чтобы понять, к каким группы относится чанк)
var chunkToGroups = new Dictionary<int, List<int>>();
for (int gIdx = 0; gIdx < mergedGroups.Count; gIdx++)
{
    int groupNum = gIdx + 1;
    foreach (var chunkId in mergedGroups[gIdx])
    {
        if (!chunkToGroups.TryGetValue(chunkId, out var groups))
        {
            groups = new List<int>();
            chunkToGroups[chunkId] = groups;
        }
        groups.Add(groupNum);
    }
}

ConsoleColor[] groupColors = new[]
{
    ConsoleColor.Cyan,
    ConsoleColor.Green,
    ConsoleColor.Yellow,
    ConsoleColor.Magenta,
    ConsoleColor.Red,
    ConsoleColor.DarkCyan,
    ConsoleColor.DarkYellow
};

Console.WriteLine($"\n═══ ПОЛНЫЙ ТЕКСТ (Найдено уникальных групп совпадений: {mergedGroups.Count}) ═══\n");
// 3. Вывод результата — строчка полностью окрашивается в цвет группы
for (int i = 0; i < chunkTexts.Count; i++)
{
    string cleanText = chunkTexts[i].Replace("\r", "").Replace("\n", "↵ ");

    if (chunkToGroups.TryGetValue(i, out var groupNums))
    {
        // Префикс чанка
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"[Chunk #{i,-3}] ");

        // Выводим теги всех групп, к которым принадлежит чанк
        foreach (var gNum in groupNums)
        {
            var tagColor = groupColors[(gNum - 1) % groupColors.Length];
            Console.ForegroundColor = tagColor;
            Console.Write($"[G{gNum}] ");
        }

        // Красим ВЕСЬ ТЕКСТ строки в цвет первой группы
        var mainColor = groupColors[(groupNums[0] - 1) % groupColors.Length];
        Console.ForegroundColor = mainColor;
        Console.WriteLine($"\"{cleanText}\"");
    }
    else
    {
        // Уникальная строка полностью выводится серым цветом
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"[Chunk #{i,-3}] [Уникальный] \"{cleanText}\"");
    }
}

Console.ResetColor();