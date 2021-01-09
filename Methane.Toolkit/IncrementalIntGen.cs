using System.Collections.Generic;
using UniUI;

public class IncrementalIntGen : UniUI.IPipe
{
    public IncrementalIntGen()
    {
        UI = new UniUI.NoUI();
    }

    public IncrementalIntGen(UniUI.IUniUI ui)
    {
        UI = ui;

    }
    public IncrementalIntGen(long start, long end, int length, UniUI.IUniUI ui)
    {
        Start = start;
        End = end;
        Length = length;
        UI = ui;

    }


    public IEnumerator<string> RunIterator()
    {
        return GenerateEnumerable().GetEnumerator();
    }

    public IEnumerable<string> GenerateEnumerable()
    {
        long end = End + 1;


        for (long i = Start; i < end; i++)
        {
            yield return i.ToString().PadLeft(Length, '0');
        }
    }

    public void PromptParameters()
    {
    PromptStart:


        if (!long.TryParse(UI.Prompt("Enter start integer"), out long start))
        {
            UI.Log("Can't read");
            goto PromptStart;
        }

    PromptEnd:
        if (!long.TryParse(UI.Prompt("Enter end integer (Inclusive)"), out long end))
        {
            UI.Log("Can't read");
            goto PromptEnd;
        }

    PromptLength:
        if (!int.TryParse(UI.Prompt("Enter the fixed length which output string should be. Enter 0 for the string to variable length. (Ex: Enter 4 to get outputs like 0012 or enter 0 to get 12) "), out int length))
        {
            UI.Log("Can't read");
            goto PromptLength;
        }

        Start = start;
        End = end;
        Length = length;
        
    }

    public void BuildFromParameters()
    {

    }

    public void RunService()
    {

    }

    public long Start { get; set; }
    public long End { get; set; }
    public int Length { get; set; }

    public IWorkerType WorkerType => IWorkerType.PipeReusable;

    [System.Text.Json.Serialization.JsonIgnore] public UniUI.IUniUI UI { get; set; }

}