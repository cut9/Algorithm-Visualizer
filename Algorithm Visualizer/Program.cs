using System.Text;

Console.OutputEncoding = Encoding.UTF8;

MainClass algorithm = new MainClass();
algorithm.StartProgram();

class MainClass
{
    private int arraySize;
    private char arrayChar;
    private int iterationTimeMs;
    private int target;
    private Random rng = new Random();
    private ConsoleVisualizer consoleVisualizer;
    private List<ISearcher> Searchers = new List<ISearcher>
    {
        new LinearSearch(),
        new BinarySearch()
    };
    private List<ISorter> Sorters = new List<ISorter>
    {
        new BubbleSort(),
        new QuickSort(),
        new MergeSort()
    };

    public void StartProgram()
    {
        while (true)
        {
            string algorithmNames = "";
            string chosedAlgorithm = "";
            Console.WriteLine(@"Выберете команду
[1]. Поиск
[2]. Сортировка
[3]. Выход");
            switch (Choice(0, 4))
            {
                case 1:
                    for (int idx = 0; idx < Searchers.Count; idx++)
                    {
                        algorithmNames += $"[{idx + 1}]. {Searchers[idx].Name}\n";
                    }
                    Console.Write("Выберете алгоритм поиска\n" + algorithmNames);
                    chosedAlgorithm = Searchers[Choice(0, Searchers.Count + 1) - 1].Name;
                    CreateConsoleVisualizer();
                    if (chosedAlgorithm == Searchers[0].Name)
                    {
                        CreateArray(out string[] unsortedArray);
                        Console.WriteLine("Введите целевое значение для поиска (1 до " + (arraySize) + ").");
                        target = Choice(0, arraySize + 1);
                        Searchers[0].StateChanged += consoleVisualizer.OnSortStateChanged;
                        Searchers[0].Search(unsortedArray, target);
                        Searchers[0].StateChanged -= consoleVisualizer.OnSortStateChanged;
                    }
                    else if (chosedAlgorithm == Searchers[1].Name)
                    {
                        CreateArray(out string[] sortedArray, sorted: true);
                        Console.WriteLine("Введите целевое значение для поиска (1 до " + (arraySize) + ").");
                        target = Choice(0, arraySize + 1);
                        Searchers[1].StateChanged += consoleVisualizer.OnSortStateChanged;
                        Searchers[1].Search(sortedArray, target);
                        Searchers[1].StateChanged -= consoleVisualizer.OnSortStateChanged;
                    }
                    break;
                case 2:
                    for (int idx = 0; idx < Sorters.Count; idx++)
                    {
                        algorithmNames += $"[{idx + 1}]. {Sorters[idx].Name}\n";
                    }
                    Console.WriteLine("Выберете алгоритм сортироки\n" + algorithmNames);
                    chosedAlgorithm = Sorters[Choice(0, Sorters.Count + 1) - 1].Name;
                    CreateConsoleVisualizer();
                    foreach (var sorter in Sorters)
                    {
                        if (sorter.Name == chosedAlgorithm)
                        {
                            CreateArray(out string[] unsortedArray);
                            sorter.StateChanged += consoleVisualizer.OnSortStateChanged;
                            sorter.Sort(unsortedArray);
                            sorter.StateChanged -= consoleVisualizer.OnSortStateChanged;
                        }
                    }
                    break;
                default:
                    return;
            }
            Console.WriteLine("Нажмите любую клавишу для продолжения.");
            Console.ReadKey();
            Console.Clear();
        }
    }

    private int Choice(int min, int max)
    {
        while (true)
        {
            string userTextInput = Console.ReadLine();
            if (int.TryParse(userTextInput, out int userNumberInput) && (userNumberInput > min && userNumberInput < max))
            {
                Console.Clear();
                return userNumberInput;
            }
            else
            {
                Console.WriteLine("Некорректное значение!");
            }
        }
    }
    private void CreateConsoleVisualizer()
    {
        Console.WriteLine(@"Выберете режим визуализации
[1]. Пошаговый
[2]. Автоматический");
        if (Choice(0, 3) == 1)
        {
            consoleVisualizer = new ConsoleVisualizer(true);
        }
        else
        {
            iterationTimeMs = 540;
            Console.WriteLine($"Введите задержку между итерациями в миллисекундах (0 - мгновенно, -1 - использовать значение по умолчанию {iterationTimeMs})");
            int userChoice = Choice(-2, int.MaxValue);
            if (userChoice != -1)
            {
                iterationTimeMs = userChoice;
            }
            consoleVisualizer = new ConsoleVisualizer(iterationTimeMs);
        }
    }
    private void CreateArray(out string[] array, bool sorted = false)
    {
        int arrMaxSize = 40;
        Console.WriteLine($"Введите размер массива (1-{arrMaxSize}).");
        arraySize = Choice(0, arrMaxSize + 1);
        Console.WriteLine("Введите символ для заполнения массива (пропустите для символа по умолчанию '⊞').");
        string userChoice = Console.ReadLine();
        arrayChar = string.IsNullOrEmpty(userChoice) || userChoice[0] == ' ' ? '⊞' : userChoice[0];
        Console.Clear();

        array = new string[arraySize];
        for (int i = 0; i < arraySize; i++)
        {
            array[i] = new string(arrayChar, i + 1);
        }
        if (sorted) return;
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}

interface ISearcher
{
    public string Name { get; }
    public void Search(string[] array, int target);
    public event EventHandler<StateEventArgs> StateChanged;
    public void OnStateChanged(StateEventArgs e);
}

interface ISorter
{
    public string Name { get; }
    public void Sort(string[] array);
    public event EventHandler<StateEventArgs> StateChanged;
    public void OnStateChanged(StateEventArgs e);
}

class LinearSearch : ISearcher
{
    public string Name { get => "Linear Search"; }
    private string[] _arr { get; set; }
    private int targetIndex = -1;
    public event EventHandler<StateEventArgs> StateChanged;
    public void OnStateChanged(StateEventArgs e)
    {
        StateChanged?.Invoke(this, e);
    }
    public void Search(string[] array, int target)
    {
        _arr = array;
        if (_arr == null || _arr.Length < 1) return;
        targetIndex = SearchTargetIndex(target);
        LinearSearchInternal(target);
        OnStateChanged(StateEventArgs.Create(_arr, k: targetIndex));
    }
    private void LinearSearchInternal(int target)
    {
        for (int i = 0; i < _arr.Length; i++)
        {
            int len = _arr[i]?.Length ?? 0;
            OnStateChanged(StateEventArgs.Create(_arr, i: i, k: targetIndex));
            if (len == target)
            {
                return;
            }
        }
    }
    private int SearchTargetIndex(int target)
    {
        for (int i = 0; i < _arr.Length; i++)
        {
            int len = _arr[i]?.Length ?? 0;
            if (len == target)
            {
                return i;
            }
        }
        return -1;
    }
}

class BinarySearch : ISearcher
{
    public string Name { get => "Binary Search"; }
    private string[] _arr { get; set; }

    public event EventHandler<StateEventArgs> StateChanged;

    public void OnStateChanged(StateEventArgs e)
    {
        StateChanged?.Invoke(this, e);
    }

    public void Search(string[] array, int target)
    {
        _arr = array;
        if (_arr == null || _arr.Length < 1) return;
        BinarySearchInternal(target);
        OnStateChanged(StateEventArgs.Create(_arr, k: target));
    }

    private void BinarySearchInternal(int target)
    {
        int left = 0;
        int right = _arr.Length - 1;
        OnStateChanged(StateEventArgs.Create(_arr, low: left, high: right, k: target));
        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            int len = _arr[mid]?.Length ?? 0;
            OnStateChanged(StateEventArgs.Create(_arr, low: left, high: right, i: len, k: target));
            if (len == target)
            {
                OnStateChanged(StateEventArgs.Create(_arr, i: len, k: target));
                return;
            }
            if (len < target)
                left = mid + 1;
            else
                right = mid - 1;
            OnStateChanged(StateEventArgs.Create(_arr, low: left, high: right, i: len, k: target));
        }
    }
}

class BubbleSort : ISorter
{
    public string Name { get => "Bubble Sort"; }
    private string[] _arr { get; set; }

    public event EventHandler<StateEventArgs> StateChanged;

    public void OnStateChanged(StateEventArgs e)
    {
        StateChanged?.Invoke(this, e);
    }

    public void Sort(string[] array)
    {
        _arr = array;
        if (_arr == null || _arr.Length <= 1) return;
        BubbleSortInternal();
        OnStateChanged(StateEventArgs.Create(_arr));
    }

    private void BubbleSortInternal()
    {
        int n = _arr.Length;
        for (int i = 0; i < n - 1; i++)
        {
            bool swapped = false;
            OnStateChanged(StateEventArgs.Create(_arr, k: i));
            for (int j = 0; j < n - i - 1; j++)
            {
                OnStateChanged(StateEventArgs.Create(_arr, k: i, i: j, j: j + 1));
                if (_arr[j].Length > _arr[j + 1].Length)
                {
                    (_arr[j], _arr[j + 1]) = (_arr[j + 1], _arr[j]);
                    swapped = true;
                    OnStateChanged(StateEventArgs.Create(_arr, k: i, i: j, j: j + 1));
                }
            }
            if (!swapped)
            {
                break;
            }
        }
    }
}

class QuickSort : ISorter
{
    public string Name { get => "Quick Sort"; }
    private string[] _arr { get; set; }

    public event EventHandler<StateEventArgs> StateChanged;

    public void OnStateChanged(StateEventArgs e)
    {
        StateChanged?.Invoke(this, e);
    }

    public void Sort(string[] array)
    {
        _arr = array;
        if (_arr == null || _arr.Length <= 1) return;
        QuickSortInternal(_arr, 0, _arr.Length - 1);
        OnStateChanged(StateEventArgs.Create(_arr));
    }

    private void QuickSortInternal(string[] array, int low, int high)
    {
        if (low < high)
        {
            OnStateChanged(StateEventArgs.Create(array, low, high));
            int pi = Partition(array, low, high);
            OnStateChanged(StateEventArgs.Create(array, low, high));
            QuickSortInternal(array, low, pi - 1);
            QuickSortInternal(array, pi + 1, high);
        }
    }

    private int Partition(string[] array, int low, int high)
    {
        int pivot = array[high].Length;
        int i = low - 1;
        OnStateChanged(StateEventArgs.Create(array, low, high, i: i));
        for (int j = low; j < high; j++)
        {
            OnStateChanged(StateEventArgs.Create(array, low, high, i: i, j: j));
            if (array[j].Length < pivot)
            {
                i++;
                if (i != j)
                {
                    OnStateChanged(StateEventArgs.Create(array, low, high, i: i, j: j));
                    (array[j], array[i]) = (array[i], array[j]);
                    OnStateChanged(StateEventArgs.Create(array, low, high, i: i, j: j));
                }
            }
        }
        i++;
        OnStateChanged(StateEventArgs.Create(array, low, high, i: i, j: high));
        (array[high], array[i]) = (array[i], array[high]);
        OnStateChanged(StateEventArgs.Create(array, low, high, i: i, j: high));
        return i;
    }
}

class MergeSort : ISorter
{
    public string Name { get => "Merge Sort"; }
    private string[] _arr { get; set; }

    public event EventHandler<StateEventArgs> StateChanged;

    public void OnStateChanged(StateEventArgs e)
    {
        StateChanged?.Invoke(this, e);
    }

    public void Sort(string[] array)
    {
        _arr = array;
        if (_arr == null || _arr.Length <= 1) return;
        MergeSortInternal(_arr, 0, _arr.Length - 1);
        OnStateChanged(StateEventArgs.Create(_arr));
    }

    private void MergeSortInternal(string[] array, int left, int right)
    {
        if (left >= right) return;
        int mid = (left + right) / 2;
        OnStateChanged(StateEventArgs.Create(array, low: left, high: right));
        MergeSortInternal(array, left, mid);
        MergeSortInternal(array, mid + 1, right);
        Merge(array, left, mid, right);
    }

    private void Merge(string[] array, int left, int mid, int right)
    {
        int n1 = mid - left + 1;
        int n2 = right - mid;

        string[] leftArr = new string[n1];
        string[] rightArr = new string[n2];

        Array.Copy(array, left, leftArr, 0, n1);
        Array.Copy(array, mid + 1, rightArr, 0, n2);

        int i = 0, j = 0, k = left;

        while (i < n1 && j < n2)
        {
            OnStateChanged(StateEventArgs.Create(array, low: left, high: right, i: left + i, j: mid + 1 + j, k: k));

            if (leftArr[i].Length <= rightArr[j].Length)
            {
                array[k] = leftArr[i];
                i++;
            }
            else
            {
                array[k] = rightArr[j];
                j++;
            }

            OnStateChanged(StateEventArgs.Create(array, low: left, high: right, k: k));
            k++;
        }

        while (i < n1)
        {
            OnStateChanged(StateEventArgs.Create(array, low: left, high: right, i: left + i, k: k));
            array[k] = leftArr[i];
            OnStateChanged(StateEventArgs.Create(array, low: left, high: right, k: k));
            i++;
            k++;
        }

        while (j < n2)
        {
            OnStateChanged(StateEventArgs.Create(array, low: left, high: right, j: mid + 1 + j, k: k));
            array[k] = rightArr[j];
            OnStateChanged(StateEventArgs.Create(array, low: left, high: right, k: k));
            j++;
            k++;
        }

        OnStateChanged(StateEventArgs.Create(array, low: left, high: right));
    }
}

public class StateEventArgs : EventArgs
{
    public string[] ArraySnapshot { get; }
    public int? Low { get; }
    public int? High { get; }
    public int? I { get; }
    public int? K { get; }
    public int? J { get; }

    private StateEventArgs(string[] snapshot, int? low = null, int? high = null, int? i = null, int? k = null, int? j = null)
    {
        ArraySnapshot = snapshot;
        Low = low;
        High = high;
        I = i;
        K = k;
        J = j;
    }

    public static StateEventArgs Create(string[]? arr, int? low = null, int? high = null, int? i = null, int? k = null, int? j = null)
    {
        var source = arr ?? Array.Empty<string>();
        var copy = (string[])source.Clone();
        return new StateEventArgs(copy, low, high, i, k, j);
    }
}

class ConsoleVisualizer
{
    private readonly int iterationTimeMs;
    private readonly bool turnBased;
    private int consoleTop = -1;
    private int lastDrawnLines = 0;

    private static readonly ConsoleColor[] Palette = new[]
    {
        ConsoleColor.White,
        ConsoleColor.DarkMagenta,
        ConsoleColor.Red,
        ConsoleColor.Yellow,
        ConsoleColor.Green,
        ConsoleColor.DarkCyan,
        ConsoleColor.DarkGreen,
        ConsoleColor.DarkBlue,
        ConsoleColor.Blue,
        ConsoleColor.DarkYellow,
        ConsoleColor.DarkYellow,
        ConsoleColor.Cyan,
        ConsoleColor.DarkBlue,
        ConsoleColor.DarkRed,
        ConsoleColor.DarkRed,
        ConsoleColor.Red,
        ConsoleColor.Magenta
    };

    public ConsoleVisualizer(int iterationTimeMs)
    {
        this.iterationTimeMs = iterationTimeMs;
    }

    public ConsoleVisualizer(bool turnBased)
    {
        this.turnBased = turnBased;
    }

    public void OnSortStateChanged(object sender, StateEventArgs e)
    {
        Draw(e);
    }

    public void Draw(StateEventArgs state)
    {
        if (consoleTop == -1)
            consoleTop = Console.CursorTop;

        var array = state.ArraySnapshot ?? Array.Empty<string>();

        bool prevCursorVisible = Console.CursorVisible;
        Console.CursorVisible = false;

        int linesToClear = Math.Max(lastDrawnLines, array.Length);
        Console.SetCursorPosition(0, consoleTop);
        for (int i = 0; i < linesToClear; i++)
        {
            Console.WriteLine(new string(' ', Math.Max(0, Console.WindowWidth - 1)));
        }

        Console.SetCursorPosition(0, consoleTop);

        for (int idx = 0; idx < array.Length; idx++)
        {
            bool iPosition = state.I.HasValue && state.I.Value == idx;
            bool kPosition = state.K.HasValue && state.K.Value == idx;
            bool jPosition = state.J.HasValue && state.J.Value == idx;
            bool lowPosition = state.Low.HasValue && state.Low.Value == idx;
            bool highPosition = state.High.HasValue && state.High.Value == idx;

            int mask = (kPosition ? 1 : 0)
                     | (jPosition ? 2 : 0)
                     | (iPosition ? 4 : 0)
                     | (lowPosition ? 8 : 0)
                     | (highPosition ? 16 : 0);

            var parts = new List<string>();
            if (iPosition) parts.Add("i");
            if (jPosition) parts.Add("j");
            if (kPosition) parts.Add("k");
            if (lowPosition) parts.Add("min");
            if (highPosition) parts.Add("max");
            string comboLabel = parts.Count > 0 ? string.Join(", ", parts) : string.Empty;

            var color = Palette[mask % Palette.Length];

            string labelSuffix = string.IsNullOrEmpty(comboLabel) ? "" : $" - {comboLabel}";
            string line = $"{idx,2}: {array[idx]}{labelSuffix}".PadRight(Math.Max(0, Console.WindowWidth - 1));

            Console.ForegroundColor = color;
            Console.WriteLine(line);
        }

        Console.ResetColor();
        Console.CursorVisible = prevCursorVisible;
        lastDrawnLines = array.Length;
        if (turnBased)
        {
            Console.ReadKey();
        }
        else
            Thread.Sleep(iterationTimeMs);
    }
}