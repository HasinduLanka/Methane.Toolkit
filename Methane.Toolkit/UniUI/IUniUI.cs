using System;

namespace UniUI
{
    public interface IUniUI
    {
        void Log(string s);
        void LogAppend(string s, int numberOfLines = 0);
        void LogSpecial(string s);
        void LogError(Exception ex, string Msg = "");

        string Prompt(string s);

        void SetStatus(string s);

        void Clear();

        void Hold();
        void Unhold();
    }
}
