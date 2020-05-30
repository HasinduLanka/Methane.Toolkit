using System;
using System.Collections.Generic;
using System.Text;

namespace UniUI
{
    public interface IUniGUI : IUniUI
    {
        void AddPrompt(int ID, string Q, ref string Ans);
        void AddPrompt(int ID, string Q, IEnumerator<string> choices, ref string Ans);
        void ShowForm();
        Dictionary<int, string> GetAnswers();
        void ShowFormMessege(int PromptID, string msg);
        void FormDone();

    }
}
